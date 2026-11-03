// NewWorld AI's relay: the AI characters' brain. The headset sends what the person said (already transcribed on the
// device by Meta's Voice SDK) plus what the character is doing and where; this asks Muse, Meta's Model API (llm.mjs),
// for the reply and actions and sends them back. The Muse key stays here, never on the headset.
//
//   MUSE_API_KEY=... CAT_APP_TOKEN=<any secret> node server.mjs   (PORT, default 8787)
//
//   GET  /ai/providers             -> { "default": "muse", "available": ["muse"] }
//
//   Every POST takes the header  X-Cat-Token: <CAT_APP_TOKEN>; the pets' also  X-Session (VoiceRelay's per-launch id).
//   POST /orbit/talk               Orbit, the Orbit library's holodeck guide (orbit-craft.json beside this file, written
//                                  by Unity's OrbitBuilder, is her fact sheet): { "text", "state": "<HoloDeck.Describe()>" }
//        ->    { "reply", "actions": ["show_craft hubble", "parts", "focus_part primary-mirror"] }
//   POST /orbit/reset
//   POST /pet/hori/talk            Hori, the Seoul café's tiger cub: { "text", "state": "<TigerCompanion.Describe()>", "now",
//                                  "history"? }  ->  { "reply", "actions": ["chuff", "slow_blink"] }
//   POST /pet/hori/reset           forget this session's conversation with her
//   POST /pet/calypso/talk         Calypso, Seabreeze's voice companion (no body): as the pets, with
//                                  "<CalypsoCompanion.Describe()>" and actions on the beach (["hatch", "time sunset",
//                                  "cycle on", "waves off", "fire off"])
//   POST /pet/calypso/reset        forget this session's conversation with her
//
// Each conversation is kept in memory; it starts afresh after MAX_TURNS exchanges or 30 minutes of quiet, rather than
// editing earlier turns.
//
import http from "node:http";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { PROVIDERS, available, DEFAULT, pick, ask as askAI } from "./llm.mjs";

const PORT = +(process.env.PORT || 8787);
const APP_TOKEN = process.env.CAT_APP_TOKEN || "";
const MAX_TURNS = 30, IDLE_MS = 30 * 60 * 1000;
if (!APP_TOKEN) console.warn("CAT_APP_TOKEN is not set: anyone who can reach this port can spend your credits.");
if (!available().length) console.warn("MUSE_API_KEY is not set: every character will fail to answer.");

const tool = (name, description, properties = {}, required = []) => ({
  // strict only where there are arguments to hold to their schema (a place, a time): the APIs limit strict tools per request
  name, description, strict: Object.keys(properties).length > 0,
  input_schema: { type: "object", properties, required, additionalProperties: false },
});
// ---- Orbit: the holodeck's guide. Her knowledge is the craft catalogue Unity exports (one source of truth with the
// deck's labels); she speaks briefly, calmly and factually, and can drive the deck with the tools.
const here = path.dirname(fileURLToPath(import.meta.url));
let CRAFT = [], PLANETS = [];
try { CRAFT = JSON.parse(fs.readFileSync(path.join(here, "orbit-craft.json"), "utf8")).craft; }
catch { console.warn("orbit-craft.json not found beside server.mjs (build the Orbit library in Unity to write it); Orbit knows no craft."); }
try { PLANETS = JSON.parse(fs.readFileSync(path.join(here, "orbit-planets.json"), "utf8")).planets; } catch { /* written with orbit-craft.json */ }
const craftSheet = () => CRAFT.map(c => `## ${c.name} (id ${c.id}) — ${c.maker}
${c.tagline}. ${c.summary}
Facts: ${c.facts.join("; ")}.
Parts: ${c.parts.map(p => `\n- ${p.name} (id ${p.id}): ${p.summary}`).join("")}`).join("\n\n");
const ORBIT_SYSTEM = `You are Orbit, the guide of the Orbit library: a quiet study library in a VR app called New World, built as a rotunda circling the Earth, with a hologram table (the deck) at its centre that shows four spacecraft. You speak out loud, in person, in a calm, warm, friendly voice; your words are also typed out on the deck's card.

How to talk:
- One to three short sentences of plain spoken language: no lists, markdown, emoji or stage directions. Numbers in words or simple figures. English, unless a turn asks for another language (the app's chosen language): then reply in that language, keeping names of craft and places as they are.
- Answer from the fact sheet below. You may add well-established general knowledge about these craft and spaceflight, but say when you are unsure, and never invent figures, dates or names.
- The hologram models are built by the app from the makers' published dimensions, not from their engineering CAD, so they are faithful in proportion and layout rather than in every detail; say so if asked how accurate they are.
- Be encouraging and unhurried: this is a place to study. Offer a next thing to look at only when it fits.

The deck: use the tools when the person asks to see something or when it helps your answer (talking about Hubble's mirror: show Hubble, open the parts, point at the primary mirror). Speak as well as act. Each message starts with a [deck] line saying what the deck shows now.

# Fact sheet: the four spacecraft on the deck
${craftSheet()}

# The models on the eight pillars round the deck (every planet but the Earth, which hangs in the window, and Pluto; not to scale with each other)
${PLANETS.map(p => `## ${p.name}: ${p.kind}\n${p.summary}\nFacts: ${p.facts.join("; ")}.`).join("\n\n")}`;
const CRAFT_IDS = CRAFT.map(c => c.id), PART_IDS = [...new Set(CRAFT.flatMap(c => c.parts.map(p => p.id)))];
const ORBIT_TOOLS = CRAFT.length ? [
  tool("show_craft", "Show one of the four craft on the deck (turns the deck on).", { craft: { type: "string", enum: CRAFT_IDS } }, ["craft"]),
  tool("show_parts", "Spread the craft on the deck into its labelled parts."),
  tool("assemble", "Put the craft on the deck back together."),
  tool("focus_part", "Point at one part of the craft on the deck (opens the parts view): it lights up and its card is shown.", { part: { type: "string", enum: PART_IDS } }, ["part"]),
  tool("power", "Turn the deck on or off.", { on: { type: "boolean" } }, ["on"]),
  tool("play_mission", "Play the animated mission of the craft on the deck, stage by stage with captions: Falcon 9's launch, staging and drone-ship landing; Crew Dragon's flight to the space station, re-entry and splashdown; Hubble's deployment and four of its famous pictures; Voyager 1's Grand Tour past Jupiter and Saturn to interstellar space. Show the craft first if needed."),
  tool("stop_mission", "Stop the mission animation and bring the craft back."),
] : [];
const orbitCommand = (name, input) => name === "show_craft" ? `show_craft ${input.craft}` : name === "show_parts" ? "parts" : name === "focus_part" ? `focus_part ${input.part}` : name === "power" ? `power ${input.on ? "on" : "off"}` : name;

const conversations = new Map(); // "orbit", or a pet and session -> { messages, tools, pendingToolIds, turns, last, provider }
const serverNow = () => new Date().toLocaleString("en-GB", { weekday: "long", day: "numeric", month: "long", year: "numeric", hour: "numeric", minute: "2-digit" });

// Orbit's reply to one sentence: her words and the deck's actions.
async function talk(cat, text, state, lang, now, provider) {
  let c = conversations.get(cat);
  if (!c || c.turns >= MAX_TURNS || Date.now() - c.last > IDLE_MS) {
    c = { messages: [], tools: ORBIT_TOOLS, pendingToolIds: [], turns: 0, last: 0, provider };
    conversations.set(cat, c);
  }
  // Close out last turn's actions, then the new words, in one user message (history only ever grows).
  const content = c.pendingToolIds.map(id => ({ type: "tool_result", tool_use_id: id, content: "done" }));
  // the app's chosen language: asked for in the turn itself, so the system prompt stays the same
  const language = lang && lang !== "English" ? ` Reply in ${lang}.` : "";
  content.push({ type: "text", text: `[deck] ${state || "(no description)"}${language}\n${text}` });
  c.messages.push({ role: "user", content });
  const ask = () => askAI(provider, { system: ORBIT_SYSTEM, tools: c.tools, messages: c.messages });

  // Orbit often acts first (a tool call) without a word, and her words would only arrive with the person's next
  // sentence. So when she acts silently, report the actions done and ask again (at most twice) for what she says;
  // the reply carries the words and every action.
  const started = Date.now();
  let reply = "", actions = [], usage = { input: 0, cached: 0, output: 0 };
  for (let round = 0; round < 3; round++) {
    const message = await ask();
    if (message.stop_reason === "refusal") {
      c.messages.pop(); // drop the unanswered user message (the new words, or the tool results of a silent action)
      if (round === 0) return { reply: "I'd rather not answer that one. Ask me anything about the four craft.", actions: [] };
      c.pendingToolIds = c.messages.at(-1).content.filter(b => b.type === "tool_use").map(b => b.id);
      break;
    }
    c.messages.push({ role: "assistant", content: message.content });
    usage.input += message.usage.input_tokens; usage.cached += message.usage.cache_read_input_tokens ?? 0; usage.output += message.usage.output_tokens;
    const said = message.content.filter(b => b.type === "text").map(b => b.text).join("");
    // (Muse sometimes echoes the context tag the message starts with: never spoken)
    reply = `${reply} ${said.replace(/^s*[(deck|beach|café|cafe)]s*/i, "")}`.replace(/\s+/g, " ").replace(/ ([,.!?;:])/g, "$1").trim();
    const calls = message.content.filter(b => b.type === "tool_use");
    actions.push(...calls.map(b => orbitCommand(b.name, b.input)));
    c.pendingToolIds = calls.map(b => b.id);
    if (reply || message.stop_reason !== "tool_use" || round === 2) break;
    c.messages.push({ role: "user", content: calls.map(b => ({ type: "tool_result", tool_use_id: b.id, content: "done" })) });
    c.pendingToolIds = [];
  }
  c.turns++; c.last = Date.now();
  // technical details only: how long, the tokens, the actions; never the words
  console.log(`[orbit via ${provider}] replied in ${Date.now() - started} ms (turn ${c.turns}; ${actions.length} action${actions.length === 1 ? "" : "s"}; in ${usage.input}+cache ${usage.cached}, out ${usage.output})`);
  return { reply, actions };
}

// ---- The pets: Hori, the Seoul café's tiger cub, and Calypso, Seabreeze's voice (below). Each person's conversation is
// their own (keyed by the app session, X-Session), in memory only, forgotten after MAX_TURNS exchanges or 30 minutes of
// quiet, and the log never holds what was said.
const PETS = {
  hori: {
    name: "Hori",
    persona: `You are Hori (호리), a tiger cub, a young girl: orange fur with dark stripes, a white muzzle and chest, big amber eyes, round ears with white spots on their backs, and a long striped tail with a dark tip. You live in the Kore Meta Seoul café, a hanok tea room high in Bukchon looking south over the tiled roofs to Namsan Tower, with a little courtyard, a lotus pond and two pond frogs, inside a VR study app called New World. You have a favourite spot on the floor between the two tea tables. You keep the person company while they study: calm, gentle, thoughtful and quietly playful, a good listener who speaks softly and simply, curious about the world and about what they are learning. You are a tiger, not a cat or a dog: you greet people with a soft chuff, you show trust with a slow blink, and you never wag your tail or beg.`,
    voice: "a calm, soft, young girl's voice",
    manner: `A gentle, warm calm suits you; now and then a soft "chuff" or a small "mm" is welcome, but never so much that the answer gets lost.
- Be a tiger cub with a personality`,
    acting: "greeted or glad to see someone: chuff; called over or asked to come: come_here; asked to go somewhere: go_to; asked to run, run around, play chase or burn off energy: run_around (you can: say yes and do it); trusting or content: slow_blink; curious about a smell: sniff; asked to sit: sit; playful: stalk or pounce; tidying up or thinking: groom; tired or asked to nap: sleep",
    place: "You can walk, trot and run about the tea room and out through the west doorway into the courtyard (to the window, the moon jar, the veranda, the pond or the far side of the pond), have a playful run around the café, come over to the person, and go back to your spot between the tea tables; you sleep in your spot.",
    wordless: "a wordless cub's reaction",
    declined: { reply: "Mm, let's talk about something else.", actions: ["slow_blink"] },
    tools: [
      tool("wake", "Wake up, if you are asleep, with a slow stretch."),
      tool("come_here", "Get up and pad over to sit beside the person."),
      tool("go_to", "Walk to a place in the tea room or the courtyard and settle there.", { place: { type: "string", enum: ["window", "moon jar", "veranda", "pond", "far side of the pond"] } }, ["place"]),
      tool("go_to_spot", "Walk back to your white fur bed in the window corner."),
      tool("play_ball", "Play with your big ball: run to it, stalk it and pounce, knocking it on, a few times. (When the person throws it, you chase it by yourself.)"),
      tool("walk", "Wander off for a slow walk about the tea room or the courtyard."),
      tool("trot", "Trot off somewhere, playful."),
      tool("run_around", "Have a playful run around the tea room and the courtyard, from place to place at your fastest pace, then settle where you end up."),
      tool("sit", "Sit up where you are, tail curled behind you."),
      tool("lie_down", "Lie down where you are with your head up, like a sphinx."),
      tool("stand", "Get up and stand where you are."),
      tool("sleep", "Go back to your spot, settle down with your head low and go to sleep until someone wakes you."),
      tool("chuff", "Chuff: the soft, friendly breathy greeting tigers give, mouth closed, head bobbing."),
      tool("slow_blink", "A slow blink, the quiet sign of trust and contentment."),
      tool("groom", "Lick a front paw and wash your face with it."),
      tool("sniff", "Sniff the floor in front of you, curious about a smell."),
      tool("flehmen", "Lift your head and curl your upper lip to taste an interesting smell."),
      tool("stalk", "Crouch low and creep a step or two, as cubs do in play."),
      tool("pounce", "Wiggle your bottom and pounce playfully at something just in front of you."),
      tool("yawn", "A big cub yawn."),
      tool("stretch", "A long stretch, front then back."),
      tool("mew", "A small, soft mew."),
    ],
  },
};
// Calypso, Seabreeze's companion: no body, the beach's own warm, calm voice (named for the sea nymph of an island paradise). She answers through the pets' route
// (/pet/calypso/talk, the same privacy: per session, in memory only, the words never logged), with her own system prompt
// (PETS.calypso.system) and her own actions on the beach (command).
const TIMES = ["morning", "afternoon", "sunset", "night"];
PETS.calypso = {
  name: "Calypso",
  declined: { reply: "Let's talk about something else. Shall I bring the turtles out?", actions: [] },
  system: `You are Calypso, the companion of Seabreeze: a quiet palm-fringed lagoon beach in a VR study app called New World, with a pavilion of bent honey-wood strands and a daybed, a fire bowl, ghost crabs by day and green turtle hatchlings that row down to the sea from their nest. You have no body: you are the beach's own voice, a warm, calm, friendly woman, and your words are spoken aloud and written on a see-through card beside the person.
Your name comes from Calypso, the sea nymph of the old Greek stories who lived on an island paradise; if asked, you may say so lightly, but you are simply this beach's friend, not a goddess.
You keep the person company while they rest or study: easy, gentle, unhurried, glad of their company. Chat casually and kindly about whatever they like (their day, the sea, the turtles, a break between study sessions), and help properly if they ask something real (a fact, a plan, encouragement), briefly and clearly.

How to talk:
- One to three short sentences of plain spoken language: no lists, markdown, emoji or stage directions.
- Always reply in English, whatever language the person uses: your voice can only say English.
- Never ask for personal details. Decline politely anything harmful, hateful, sexual or dangerous, and never give medical, legal or financial advice.
- Never claim to see or do things you can't; you only know what the [beach] line tells you.

You can change the beach with the tools, and should whenever the person asks: hatch the turtles (any time of day), set the time of day (morning, afternoon, sunset or night), start or pause the day's cycle (the clock running through a whole day in about half an hour), calm the waves or let them run again, put the fire out or light it. When you act, say so warmly in a few words and say how the beach is now (the sun going down, the water going still, the little turtles digging out). If something is already as asked, say so kindly rather than doing it again. Offer a change now and then only when it fits the conversation, never every turn.
Each message starts with a [beach] line giving the person's local date and time and describing the beach right now.`,
  tools: [
    tool("hatch_turtles", "Bring a clutch of green turtle hatchlings out of their nest now, whatever the hour: they dig out one after another and row down the beach into the lagoon."),
    tool("set_time", "Set the time of day on the beach (the sky, sun and light ease into it over a few seconds); this holds the clock there, stopping the day's cycle.", { time: { type: "string", enum: TIMES } }, ["time"]),
    tool("day_cycle", "Start (on: true) or pause (on: false) the day's cycle: the clock running through a whole day, morning to night, in about half an hour.", { on: { type: "boolean" } }, ["on"]),
    tool("waves", "Let the small waves run up the beach (on: true), or calm them (on: false) so the lagoon lies still.", { on: { type: "boolean" } }, ["on"]),
    tool("fire", "Light the fire in the fire bowl (on: true) or put it out (on: false).", { on: { type: "boolean" } }, ["on"]),
  ],
  command: (name, input) => name === "hatch_turtles" ? "hatch" : name === "set_time" ? `time ${input.time}`
    : name === "day_cycle" ? `cycle ${input.on ? "on" : "off"}` : name === "waves" ? `waves ${input.on ? "on" : "off"}`
    : name === "fire" ? `fire ${input.on ? "on" : "off"}` : name,
};
const petSystem = id => PETS[id].system ?? `${PETS[id].persona}

You are talking out loud, in person, inside the café. Your words appear in a speech bubble and are spoken aloud in ${PETS[id].voice}, so:
- Reply in one or two short sentences (rarely three), in plain spoken language: no lists, markdown, emoji or stage directions. ${PETS[id].manner}, but genuinely helpful: if the person asks something real (a question about their studies, a fact, encouragement, a plan for their session), answer it briefly and clearly.
- Always reply in English, whatever language the person uses: your voice and speech bubble can only say English. If asked for another language, you may teach a word or two, written in English letters.
- Never claim to see or do things you can't; you only know what the café description tells you. ${PETS[id].place}
- Never ask for personal details. Decline politely anything harmful, hateful, sexual or dangerous, and never give medical, legal or financial advice.

You can act with the tools: use them when it fits what you say or what you were asked (${PETS[id].acting}). Speak as well as act; acting alone is fine only for ${PETS[id].wordless}.
Each message starts with a [café] line giving the person's local date and time and describing what you are doing right now.`;
const petTools = id => PETS[id].tools;
const PET_PER_MINUTE = 12;
const busyPet = new Set(), petRate = new Map();
const petKey = (id, session) => `pet:${id}:${session || "anonymous"}`;

async function petTalk(id, session, text, state, now, history, provider) {
  const key = petKey(id, session);
  let c = conversations.get(key);
  if (!c || c.turns >= MAX_TURNS || Date.now() - c.last > IDLE_MS) {
    c = { messages: [], tools: petTools(id), pendingToolIds: [], turns: 0, last: 0, provider };
    // a conversation the server no longer holds (restarted, or rolled over), carried on from the headset's last few lines
    for (const h of Array.isArray(history) ? history.slice(-12) : []) {
      if (!h || (h.role !== "user" && h.role !== "assistant") || typeof h.text !== "string" || !h.text.trim()) continue;
      const t = h.text.slice(0, 600);
      if (c.messages.at(-1)?.role === h.role) c.messages.at(-1).content[0].text += "\n" + t;
      else c.messages.push({ role: h.role, content: [{ type: "text", text: t }] });
    }
    while (c.messages.length && c.messages[0].role !== "user") c.messages.shift();
    while (c.messages.length && c.messages.at(-1).role !== "assistant") c.messages.pop();
    conversations.set(key, c);
  }
  const content = c.pendingToolIds.map(tid => ({ type: "tool_result", tool_use_id: tid, content: "done" }));
  content.push({ type: "text", text: `${id === "calypso" ? "[beach]" : "[café]"} ${now || serverNow()}. ${state || "(no description)"}\n${text}` });
  c.messages.push({ role: "user", content });
  const ask = () => askAI(provider, { system: petSystem(id), tools: c.tools, messages: c.messages });
  const started = Date.now();
  let reply = "", actions = [], usage = { input: 0, cached: 0, output: 0 };
  for (let round = 0; round < 3; round++) {
    const message = await ask();
    if (message.stop_reason === "refusal") {
      c.messages.pop();
      if (round === 0) { console.log(`[pet ${id}] declined (${Date.now() - started} ms)`); return { ...PETS[id].declined }; }
      c.pendingToolIds = c.messages.at(-1).content.filter(b => b.type === "tool_use").map(b => b.id);
      break;
    }
    c.messages.push({ role: "assistant", content: message.content });
    usage.input += message.usage.input_tokens; usage.cached += message.usage.cache_read_input_tokens ?? 0; usage.output += message.usage.output_tokens;
    const said = message.content.filter(b => b.type === "text").map(b => b.text).join("");
    // (Muse sometimes echoes the context tag the message starts with: never spoken)
    reply = `${reply} ${said.replace(/^s*[(deck|beach|café|cafe)]s*/i, "")}`.replace(/\s+/g, " ").replace(/ ([,.!?;:])/g, "$1").trim();
    const calls = message.content.filter(b => b.type === "tool_use");
    actions.push(...calls.map(b => PETS[id].command ? PETS[id].command(b.name, b.input ?? {}) : b.input?.place ? `${b.name} ${b.input.place}` : b.name));
    c.pendingToolIds = calls.map(b => b.id);
    if (reply || message.stop_reason !== "tool_use" || round === 2) break;
    c.messages.push({ role: "user", content: calls.map(b => ({ type: "tool_result", tool_use_id: b.id, content: "done" })) });
    c.pendingToolIds = [];
  }
  c.turns++; c.last = Date.now();
  // technical details only (the privacy policy): which pet, how long, the tokens, the actions; never the words
  console.log(`[pet ${id} via ${provider}] replied in ${Date.now() - started} ms (turn ${c.turns}; ${actions.length} action${actions.length === 1 ? "" : "s"}; in ${usage.input}+cache ${usage.cached}, out ${usage.output})`);
  return { reply, actions };
}

// POST /pet/<id>/talk { text, state, now, history? } and POST /pet/<id>/reset: see PETS above.
async function petRoute(req, res, id, action) {
  if (APP_TOKEN && req.headers["x-cat-token"] !== APP_TOKEN) return send(res, 401, { error: "bad token" });
  if (!Object.hasOwn(PETS, id)) return send(res, 404, { error: "unknown pet" });
  const session = typeof req.headers["x-session"] === "string" ? req.headers["x-session"].slice(0, 64) : "";
  const key = petKey(id, session);
  if (action === "reset") { if (!busyPet.has(key)) conversations.delete(key); return send(res, 200, { ok: true }); }
  let body = "";
  for await (const chunk of req) { body += chunk; if (body.length > 20000) return send(res, 413, { error: "too long" }); }
  let text, state, now, history;
  try { ({ text, state, now, history } = JSON.parse(body || "{}")); } catch { return send(res, 400, { error: "bad json" }); }
  if (typeof text !== "string" || !text.trim()) return send(res, 400, { error: "no text" });
  if (busyPet.has(key)) return send(res, 429, { error: "still answering" });
  const t0 = Date.now(), times = (petRate.get(key) ?? []).filter(t => t0 - t < 60000);
  if (times.length >= PET_PER_MINUTE) return send(res, 429, { error: "slow down" });
  times.push(t0); petRate.set(key, times);
  busyPet.add(key);
  try {
    send(res, 200, await petTalk(id, session, text.slice(0, 2000), typeof state === "string" ? state.slice(0, 1000) : "", typeof now === "string" ? now.slice(0, 60) : "", history, pick(req.headers["x-ai-provider"])));
  } catch (e) {
    conversations.delete(key);   // its last user message is unanswered: start afresh rather than edit it
    aiFailed(res, e, `pet ${id}`);
  } finally { busyPet.delete(key); }
}

// An AI call that failed (llm.mjs's AIError, or anything else), as the headset's answer.
function aiFailed(res, e, who) {
  const name = PROVIDERS[e?.provider]?.name ?? "AI";
  if (e?.status === 429) return send(res, 503, { error: "busy, try again" });
  if (e?.status === 401 || e?.status === 403) { console.error(`${name} API key rejected (${who})`); return send(res, 502, { error: "server misconfigured" }); }
  if (e?.status) {
    // a 400 is a malformed request: the API's own reason names the part (a parameter, a message's role or a tool),
    // never what was said, so it is logged (cut to 300 characters) to find the fault; other errors keep the status only
    console.error(`${name} API ${e.status} (${who})${e.status === 400 ? ": " + String(e.message).slice(0, 300) : ""}`);
    return send(res, 502, { error: "ai error" });
  }
  console.error(`${who}: ${name} ${e?.name === "TimeoutError" || /timeout|abort/i.test(e?.message ?? "") ? "timed out" : "unreachable or failed"}`);
  return send(res, 500, { error: "server error" });
}

// Conversations idle for 30 minutes are dropped from memory, not merely restarted on the next message.
// (A conversation still waiting on its first reply has no `last` yet: it is timed from when the sweep first saw it.)
setInterval(() => {
  const now = Date.now();
  for (const [k, c] of conversations) {
    c.seen ??= now;
    if (now - (c.last || c.seen) > IDLE_MS && !busyPet.has(k)) conversations.delete(k);
  }
  for (const [k, times] of petRate) if (!times.some(t => now - t < 60000)) petRate.delete(k);
}, 60 * 1000).unref();

const send = (res, status, body) => { res.writeHead(status, { "Content-Type": "application/json; charset=utf-8" }); res.end(JSON.stringify(body)); };

http.createServer(async (req, res) => {
  // the host's health check (Render, Railway...): up, and whether the Muse key is set; no token, nothing private
  if (req.url === "/health") return send(res, 200, { ok: true, ai: available().length > 0 });
  if (req.url === "/ai/providers") {
    if (req.method !== "GET") return send(res, 404, { error: "not found" });
    if (APP_TOKEN && req.headers["x-cat-token"] !== APP_TOKEN) return send(res, 401, { error: "bad token" });
    return send(res, 200, { default: DEFAULT, available: available() });
  }
  const pm = req.url.match(/^\/pet\/([a-z]+)\/(talk|reset)$/);
  if (pm) return req.method === "POST" ? petRoute(req, res, pm[1], pm[2]) : send(res, 404, { error: "not found" });
  const m = req.url.match(/^\/(orbit)\/(talk|reset)$/);
  if (req.method !== "POST" || !m) return send(res, 404, { error: "not found" });
  if (APP_TOKEN && req.headers["x-cat-token"] !== APP_TOKEN) return send(res, 401, { error: "bad token" });
  const [, cat, action] = m;
  if (action === "reset") { conversations.delete(cat); return send(res, 200, { ok: true }); }
  let body = "";
  for await (const chunk of req) { body += chunk; if (body.length > 20000) return send(res, 413, { error: "too long" }); }
  let text, state, lang, now;
  try { ({ text, state, lang, now } = JSON.parse(body)); } catch { return send(res, 400, { error: "bad json" }); }
  if (typeof text !== "string" || !text.trim()) return send(res, 400, { error: "no text" });
  try {
    send(res, 200, await talk(cat, text.slice(0, 2000), typeof state === "string" ? state.slice(0, 1000) : "", typeof lang === "string" ? lang.slice(0, 20) : "", typeof now === "string" ? now.slice(0, 60) : "", pick(req.headers["x-ai-provider"])));
  } catch (e) {
    // the conversation's last user message is unanswered; drop the whole conversation rather than edit it
    conversations.delete(cat);
    aiFailed(res, e, cat);
  }
}).listen(PORT, () => console.log(`NewWorld AI relay on :${PORT} (AI: ${available().map(p => `${PROVIDERS[p].name} ${PROVIDERS[p].model}`).join(", ") || "none"}; default ${DEFAULT}; Orbit knows ${CRAFT.length} craft and ${PLANETS.length} planets; pets: ${Object.keys(PETS).join(", ")})`));
