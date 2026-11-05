// What a character can look up outside the app, run here on the relay (never by the headset): the weather from
// Open-Meteo (free, no key) and today's headlines from news agencies' public RSS feeds. Only a city name goes out (for
// the weather); nothing about the person. Results are cached for a while, so a busy café doesn't hammer either.

const WEATHER_TTL = 10 * 60 * 1000, NEWS_TTL = 15 * 60 * 1000;
const cache = new Map();   // key -> { at, value }
async function cached(key, ttl, make) {
  const hit = cache.get(key);
  if (hit && Date.now() - hit.at < ttl) return hit.value;
  const value = await make();
  cache.set(key, { at: Date.now(), value });
  return value;
}

async function getJson(url) {
  const res = await fetch(url, { signal: AbortSignal.timeout(10000), headers: { "User-Agent": "NewWorldRelay/1.0" } });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

// ---- Weather (Open-Meteo) -------------------------------------------------------------------------------------------

export const SEOUL = { name: "Seoul", country: "South Korea", latitude: 37.5665, longitude: 126.978 };

// WMO weather codes, in words
const SKY = {
  0: "clear sky", 1: "mainly clear", 2: "partly cloudy", 3: "overcast", 45: "fog", 48: "freezing fog",
  51: "light drizzle", 53: "drizzle", 55: "heavy drizzle", 56: "freezing drizzle", 57: "heavy freezing drizzle",
  61: "light rain", 63: "rain", 65: "heavy rain", 66: "freezing rain", 67: "heavy freezing rain",
  71: "light snow", 73: "snow", 75: "heavy snow", 77: "snow grains", 80: "light showers", 81: "showers", 82: "violent showers",
  85: "light snow showers", 86: "snow showers", 95: "thunderstorm", 96: "thunderstorm with hail", 99: "thunderstorm with heavy hail",
};
const sky = code => SKY[code] ?? "mixed weather";

async function place(city) {
  if (!city || /^\s*seoul\s*$/i.test(city)) return SEOUL;
  const q = String(city).slice(0, 80);
  return cached(`geo:${q.toLowerCase()}`, 24 * 60 * 60 * 1000, async () => {
    const data = await getJson(`https://geocoding-api.open-meteo.com/v1/search?count=1&language=en&format=json&name=${encodeURIComponent(q)}`);
    const r = data.results?.[0];
    return r ? { name: r.name, country: r.country ?? "", latitude: r.latitude, longitude: r.longitude } : null;
  });
}

// A short plain-text report for the character to put in its own words.
export async function weather(city) {
  try {
    const p = await place(city);
    if (!p) return `No place called "${city}" was found.`;
    return await cached(`wx:${p.latitude},${p.longitude}`, WEATHER_TTL, async () => {
      const d = await getJson(`https://api.open-meteo.com/v1/forecast?latitude=${p.latitude}&longitude=${p.longitude}`
        + "&current=temperature_2m,apparent_temperature,relative_humidity_2m,precipitation,weather_code,wind_speed_10m"
        + "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto&forecast_days=2");
      const c = d.current ?? {}, day = d.daily ?? {};
      const at = String(c.time ?? "").replace("T", " ");
      const lines = [
        `Weather in ${p.name}${p.country ? ", " + p.country : ""} (local time ${at}): ${sky(c.weather_code)}, ${Math.round(c.temperature_2m)}°C (feels like ${Math.round(c.apparent_temperature)}°C), humidity ${c.relative_humidity_2m}%, wind ${Math.round(c.wind_speed_10m)} km/h${c.precipitation > 0 ? `, ${c.precipitation} mm of rain or snow in the last hour` : ""}.`,
      ];
      const names = ["Today", "Tomorrow"];
      for (let i = 0; i < Math.min(2, day.time?.length ?? 0); i++)
        lines.push(`${names[i]}: ${sky(day.weather_code[i])}, ${Math.round(day.temperature_2m_min[i])} to ${Math.round(day.temperature_2m_max[i])}°C, ${day.precipitation_probability_max[i] ?? 0}% chance of rain.`);
      return lines.join("\n") + "\n(Source: Open-Meteo.)";
    });
  } catch (e) {
    return `The weather couldn't be fetched just now (${e.message}).`;
  }
}

// ---- News (public RSS feeds) ----------------------------------------------------------------------------------------

// Each a news agency's own public feed, in English. (AP publishes no feed and blocks automated reads: its news is only
// licensed through its paid API, so it isn't here.)
export const FEEDS = {
  yonhap: { name: "Yonhap News Agency", url: "https://en.yna.co.kr/RSS/news.xml" },
};

const decode = s => s.replace(/<!\[CDATA\[([\s\S]*?)\]\]>/g, "$1").replace(/<[^>]+>/g, "")
  .replace(/&amp;/g, "&").replace(/&lt;/g, "<").replace(/&gt;/g, ">").replace(/&quot;/g, '"').replace(/&#39;|&apos;/g, "'")
  .replace(/\s+/g, " ").trim();
const field = (item, tag) => decode((item.match(new RegExp(`<${tag}[^>]*>([\\s\\S]*?)</${tag}>`)) ?? [])[1] ?? "");

async function headlines(id, count) {
  const feed = FEEDS[id];
  return cached(`news:${id}`, NEWS_TTL, async () => {
    const res = await fetch(feed.url, { signal: AbortSignal.timeout(10000), headers: { "User-Agent": "NewWorldRelay/1.0" } });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const xml = await res.text();
    const seen = new Set();
    return [...xml.matchAll(/<item[\s>][\s\S]*?<\/item>/g)].slice(0, 30).map(m => {
      // updates of one story ("(LEAD)", "(URGENT)", "(2nd LD)") are told once
      const title = field(m[0], "title").replace(/^(\((LEAD|URGENT|\d+\w* LD|News Focus|Yonhap Interview)[^)]*\)\s*)+/i, ""), when = field(m[0], "pubDate");
      const key = title.toLowerCase().replace(/[^a-z0-9]+/g, " ").split(" ").slice(0, 6).join(" ");
      if (seen.has(key)) return { title: "" };
      seen.add(key);
      // the agency's dateline and the trailing "..." are trimmed from the teaser
      const teaser = field(m[0], "description").replace(/^[A-Z][A-Z .,'-]+, \w+\.? \d+ \([^)]*\) -- /, "").replace(/\.\.\.$/, "").slice(0, 220);
      return { title, when, teaser };
    }).filter(h => h.title);
  }).then(list => list.slice(0, count));
}

export async function news(source = "yonhap", count = 6) {
  const ids = source && FEEDS[source] ? [source] : Object.keys(FEEDS);
  const parts = [];
  for (const id of ids) {
    try {
      const list = await headlines(id, Math.max(1, Math.min(10, count)));
      parts.push(`Latest headlines from ${FEEDS[id].name}:\n` + list.map(h => `- ${h.title}${h.teaser ? ": " + h.teaser : ""} (${h.when})`).join("\n"));
    } catch (e) {
      parts.push(`${FEEDS[id].name}'s headlines couldn't be fetched just now (${e.message}).`);
    }
  }
  return parts.join("\n\n");
}
