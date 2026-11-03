using System;
using System.Linq;

namespace StudyEverywhere
{
    // The holodeck's Solar System and Universe modes: what they show and what the card says. Figures from NASA's
    // planetary fact sheets (NSSDCA), NASA Solar System Exploration and NASA/JPL Solar System Dynamics (mean radii,
    // semi-major axes, periods); the planets' own write-ups are OrbitPlanets'. The explorers lines name the missions and
    // people who went (NASA, ESA, JAXA, SpaceX public records). Star sizes and the sizes of the largest structures are
    // estimates, and the card says so.
    public static class OrbitSolar
    {
        [Serializable] public class Moon
        {
            public string name, about;
            public float radiusKm, orbitKm, periodDays;   // periodDays < 0: it goes round backwards (retrograde)
        }
        [Serializable] public class Body
        {
            public string id, name, kind, summary, explorers;
            public string[] facts;
            public float radiusKm, orbitAu, periodDays, dayHours, tiltDegrees;
            public Moon[] moons = new Moon[0];
        }

        static Moon M(string name, float r, float a, float p, string about) => new Moon { name = name, radiusKm = r, orbitKm = a, periodDays = p, about = about };
        static Body Planet(string id, float r, float au, float p, float day, float tilt, string explorers, params Moon[] moons)
        {
            var info = OrbitPlanets.Find(id);
            return new Body { id = id, name = info.name, kind = info.kind, summary = info.summary, facts = info.facts, radiusKm = r, orbitAu = au, periodDays = p, dayHours = day, tiltDegrees = tilt, explorers = explorers, moons = moons };
        }

        // The Sun, the eight planets and Pluto, in order out from the Sun.
        public static readonly Body[] Bodies =
        {
            new Body { id = "sun", name = "The Sun", kind = "Our star, a yellow dwarf", radiusKm = 695700, dayHours = 609.1f, tiltDegrees = 7.25f,
                summary = "A ball of hot plasma holding 99.8 percent of the Solar System's mass. In its core, 15 million °C, it fuses about 600 million tonnes of hydrogen into helium every second, and the light that leaves its surface now reaches us in about eight minutes.",
                facts = new[] { "Diameter: 1.39 million km, 109 Earths across", "Surface: about 5,500 °C", "Age: about 4.6 billion years", "Turns once in about 25 days at its equator", "Light to the Earth: 8 minutes 20 seconds" },
                explorers = "Parker Solar Probe flew closer than anything made by people (6.1 million km, 2024); SOHO and the Solar Dynamics Observatory watch it every day." },
            Planet("mercury", 2439.7f, .387f, 87.97f, 1407.6f, .03f, "Mariner 10 (1974–75) and MESSENGER, which orbited it from 2011 to 2015; ESA and JAXA's BepiColombo arrives in 2026."),
            Planet("venus", 6051.8f, .723f, 224.7f, -5832.5f, 177.4f, "The Soviet Venera landers sent the only pictures from its surface; NASA's Magellan mapped it by radar (1990–94)."),
            new Body { id = "earth", name = "Earth", kind = "Rocky planet, our home", radiusKm = 6371, orbitAu = 1, periodDays = 365.26f, dayHours = 24, tiltDegrees = 23.44f,
                summary = "The only world known to have life, and liquid water open on its surface. Its magnetic field and air shield that life from the Sun's wind and from space, and its one large Moon steadies its tilt, which keeps the seasons mild.",
                facts = new[] { "Diameter: 12,742 km", "From the Sun: 149.6 million km (1 AU)", "Year: 365.26 days", "Day: 24 hours", "Moons: 1" },
                explorers = "About 280 people from 23 countries have lived on the International Space Station since 2000; SpaceX's Crew Dragon has flown crews there since 2020.",
                moons = new[] { M("The Moon", 1737.4f, 384400, 27.32f, "Probably made from the debris of a Mars-sized world hitting the young Earth. Twelve people walked on it on Apollo missions, 1969 to 1972, first Neil Armstrong and Buzz Aldrin.") } },
            Planet("mars", 3389.5f, 1.524f, 686.98f, 24.62f, 25.19f, "NASA's rovers Sojourner, Spirit, Opportunity, Curiosity and Perseverance; the Ingenuity helicopter made the first powered flight on another world (2021).",
                M("Phobos", 11.3f, 9376, .319f, "A lumpy moon about 22 km across, so close it circles three times a day, rising in the west. It is slowly spiralling in."),
                M("Deimos", 6.2f, 23463, 1.263f, "The smaller and farther moon, about 12 km across, smoothed by dust. Both may be captured asteroids.")),
            Planet("jupiter", 69911f, 5.203f, 4332.6f, 9.93f, 3.13f, "Pioneers 10 and 11, Voyagers 1 and 2, Galileo (orbited 1995–2003), Juno (in orbit since 2016); Europa Clipper arrives in 2030.",
                M("Io", 1821.6f, 421700, 1.769f, "The most volcanic world known, with hundreds of volcanoes, squeezed and heated by Jupiter's pull."),
                M("Europa", 1560.8f, 671034, 3.551f, "Under its cracked ice shell is an ocean with more water than all of Earth's: one of the best places to look for life."),
                M("Ganymede", 2634.1f, 1070412, 7.155f, "The largest moon in the Solar System, bigger than Mercury, and the only moon with its own magnetic field."),
                M("Callisto", 2410.3f, 1882709, 16.69f, "The most heavily cratered surface known: an ancient face, barely changed in four billion years.")),
            Planet("saturn", 58232f, 9.537f, 10759f, 10.66f, 26.73f, "Pioneer 11, Voyagers 1 and 2, and Cassini, which orbited it for 13 years and dropped the Huygens probe onto Titan in 2005.",
                M("Mimas", 198.2f, 185539, .942f, "Its giant crater Herschel makes it look like the Death Star."),
                M("Enceladus", 252.1f, 237948, 1.37f, "Geysers at its south pole spray water from an ocean under the ice into space; Cassini flew through them."),
                M("Tethys", 531.1f, 294619, 1.888f, "Almost pure water ice, with a canyon running three-quarters of the way round it."),
                M("Dione", 561.4f, 377396, 2.737f, "Bright ice cliffs streak its trailing side."),
                M("Rhea", 763.8f, 527108, 4.518f, "Saturn's second-largest moon, an icy ball heavily cratered."),
                M("Titan", 2574.7f, 1221870, 15.95f, "Bigger than Mercury, with thick orange air and lakes of liquid methane: the only moon with a real atmosphere. NASA's Dragonfly will fly there."),
                M("Iapetus", 734.5f, 3560820, 79.33f, "Two-toned: one side dark as coal, the other bright as snow.")),
            Planet("uranus", 25362f, 19.19f, 30687f, -17.24f, 97.77f, "Only Voyager 2, which flew past in January 1986.",
                M("Miranda", 235.8f, 129390, 1.413f, "A patchwork world with cliffs about 20 km high, the tallest known."),
                M("Ariel", 578.9f, 191020, 2.52f, "The brightest of Uranus's large moons, with long valleys."),
                M("Umbriel", 584.7f, 266000, 4.144f, "The darkest of them, with a mysterious bright ring on one crater."),
                M("Titania", 788.4f, 435910, 8.706f, "Uranus's largest moon, cut by huge faults."),
                M("Oberon", 761.4f, 583520, 13.46f, "The outermost large moon, old and cratered.")),
            Planet("neptune", 24622f, 30.07f, 60190f, 16.11f, 28.32f, "Only Voyager 2, which flew past in August 1989.",
                M("Proteus", 210f, 117647, 1.122f, "One of the darkest objects known, almost as big as a body can be without being pulled round."),
                M("Triton", 1353.4f, 354759, -5.877f, "Goes round backwards, so it was probably captured from the Kuiper Belt. Voyager 2 saw geysers of nitrogen on it."),
                M("Nereid", 170f, 5513818, 360.1f, "On a long, stretched orbit that takes almost a year.")),
            Planet("pluto", 1188.3f, 39.48f, 90560f, -153.3f, 122.5f, "NASA's New Horizons flew past on 14 July 2015 and showed its heart-shaped plain.",
                M("Charon", 606f, 19591, 6.387f, "Half Pluto's size: the two always face each other and circle a point between them.")),
        };
        public static Body Find(string id) => Bodies.FirstOrDefault(b => b.id == id);

        // ---- the Universe mode: one step after another, each thing shown beside the last at true relative size
        [Serializable] public class Step
        {
            public string id, name, kind, about;
            public double radiusKm;
            public bool estimate, nested;   // nested: drawn round the one before (a system, not a ball)
            public string look;             // how it is drawn: "rock", "earth", "moon", "gas", "star:<hex colour>", "orbits", "cloud", "galaxy", "group", "web"
        }
        const double Sun = 695700, Ly = 9.4607e12, Au = 1.496e8;
        static Step S(string id, string name, string kind, double r, string look, string about, bool estimate = false, bool nested = false)
            => new Step { id = id, name = name, kind = kind, radiusKm = r, look = look, about = about, estimate = estimate, nested = nested };

        public static readonly Step[] Steps =
        {
            S("moon", "The Moon", "Moon", 1737.4, "moon", "Our Moon is about a quarter of the Earth's width. Start here and keep going."),
            S("earth", "Earth", "Planet", 6371, "earth", "Our home. Fifty Moons would fit inside it."),
            S("neptune", "Neptune", "Ice giant", 24622, "gas:3A5BD0", "About four Earths wide; nearly 58 Earths would fit inside."),
            S("jupiter", "Jupiter", "Gas giant", 69911, "gas:C9A27E", "The largest planet: 11 Earths across, and more than 1,300 Earths would fit inside it."),
            S("sun", "The Sun", "Star (yellow dwarf)", Sun, "star:FFE7A0", "Ten Jupiters across, 109 Earths: about 1.3 million Earths would fit inside. And it is an ordinary star."),
            S("sirius", "Sirius A", "Star, 8.6 light-years away", 1.71 * Sun, "star:CFE2FF", "The brightest star in our night sky, about 1.7 times the Sun's width and 25 times as bright."),
            S("pollux", "Pollux", "Orange giant, 34 light-years away", 9.1 * Sun, "star:FFC68A", "A giant star in Gemini about 9 times the Sun's width.", true),
            S("arcturus", "Arcturus", "Red giant, 37 light-years away", 25.4 * Sun, "star:FFB070", "About 25 Suns across. Our Sun will swell into a giant like this in about five billion years.", true),
            S("rigel", "Rigel", "Blue supergiant, about 860 light-years away", 79 * Sun, "star:BFD6FF", "Orion's bright foot: around 79 Suns wide and some 100,000 times as bright.", true),
            S("betelgeuse", "Betelgeuse", "Red supergiant, about 550 light-years away", 760 * Sun, "star:FF8A55", "Orion's red shoulder. If it sat where the Sun is, it would reach past the orbit of Mars. Estimates range from about 640 to 1,000 Suns across.", true),
            S("vy-cma", "VY Canis Majoris", "Red hypergiant, about 3,900 light-years away", 1420 * Sun, "star:FF7048", "One of the largest stars known, around 1,400 Suns wide: light would take nearly two hours to cross it.", true),
            S("stephenson", "Stephenson 2-18", "Red supergiant, about 19,000 light-years away", 2150 * Sun, "star:FF6040", "Possibly the largest star known, perhaps 2,150 Suns across, wider than Saturn's orbit. Its size is very uncertain.", true),
            S("solar-system", "Our Solar System", "The Sun and its planets, out to Neptune", 30.07 * Au, "orbits", "Neptune's orbit is 9 billion km across: light takes about 8 hours to cross it. Stephenson 2-18 would fill a third of it.", false, true),
            S("oort", "The Oort Cloud", "A shell of icy bodies round the Sun", 100000 * Au, "cloud", "Trillions of comets may circle far beyond the planets, out to perhaps 100,000 times the Earth's distance from the Sun, more than a light-year. No one has seen it directly yet.", true, true),
            S("milky-way", "The Milky Way", "Our galaxy, a barred spiral", 50000 * Ly, "galaxy", "Some 100 to 400 billion stars in a disc about 100,000 light-years across. The Sun is about 26,000 light-years from the centre and takes 230 million years to go round once.", true, true),
            S("local-group", "The Local Group", "Our neighbourhood of galaxies", 5e6 * Ly, "group", "More than 80 galaxies within about 10 million light-years, led by the Milky Way and Andromeda, which will merge in a few billion years.", true, true),
            S("observable", "The Observable Universe", "Everything whose light has had time to reach us", 46.5e9 * Ly, "web", "About 93 billion light-years across, with hundreds of billions of galaxies (some estimates say two trillion) strung along a cosmic web. Beyond it there is surely more, but its light has not reached us yet.", true, true),
        };

        // A size in words: kilometres, then Suns, then light-years. In English unless translate (the card shows it in
        // the chosen language; Orbit's fact-sheet answers build English sentences round it).
        public static string SizeLine(Step s, bool translate = false)
        {
            string F(string en, params object[] a) => translate ? Localization.Format(en, a) : string.Format(en, a);
            double d = s.radiusKm * 2, suns = s.radiusKm / Sun;
            string size =
                s.radiusKm <= Sun ? (s.radiusKm > 20000 ? F("{0:N0} km across, {1:N0} Earths", d, d / 12742) : F("{0:N0} km across", d))
                : d < 1e9 ? F("{0} times the Sun's width", suns < 10 ? suns.ToString("N1") : suns.ToString("N0"))
                : d < 1e13 ? F("{0:N0} AU across ({1:N1} light-hours)", d / Au, d / 1.0793e9)
                : d < Ly * 10 ? F("{0:N1} light-years across", d / Ly)
                : F("{0:N0} light-years across", d / Ly);
            return s.estimate ? F("About {0} (an estimate)", size) : size;
        }
    }
}
