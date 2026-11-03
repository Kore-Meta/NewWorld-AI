using System;
using System.Linq;
using UnityEngine;

namespace StudyEverywhere
{
    // The holodeck's catalogue: four spacecraft whose designs are publicly documented, two from NASA and two from SpaceX,
    // each as a short fact sheet and a list of parts. The figures come from the makers' published material (NASA/JPL
    // mission fact sheets and the Hubble site; SpaceX's Falcon 9 User's Guide and Dragon pages). The models on the deck
    // are built by OrbitBuilder from these dimensions, not from the makers' CAD, so they are faithful in proportion and
    // layout rather than in every rivet; Orbit (the assistant) says so if asked.
    //
    // One source of truth: the deck reads this for its labels and cards, and OrbitBuilder writes it out as
    // Tools/CatServer/orbit-craft.json for the assistant's system prompt. Coordinates are in the craft's own frame, in
    // real metres, +Y along the long axis (up when the model stands on the deck); `spread` is where a part floats to in
    // the exploded view, so the layout reads like an anatomical plate.
    public static class OrbitCraft
    {
        [Serializable] public class Part { public string id, name, summary; public Vector3 spread; }
        [Serializable] public class Craft
        {
            public string id, name, maker, tagline, summary;
            public string[] facts;
            [Tooltip("Model metres per real metre: how far the craft is shrunk to float over the deck.")] public float scale;
            public Part[] parts;
        }
        [Serializable] class Catalogue { public Craft[] craft; }

        static Part P(string id, string name, float sx, float sy, float sz, string summary) => new Part { id = id, name = name, spread = new Vector3(sx, sy, sz), summary = summary };

        public static readonly Craft[] All =
        {
            new Craft
            {
                id = "voyager-1", name = "Voyager 1", maker = "NASA / JPL", tagline = "The farthest human-made object",
                summary = "Launched on 5 September 1977 on a Titan IIIE-Centaur, Voyager 1 flew past Jupiter in March 1979 and Saturn in November 1980, then climbed out of the plane of the planets. In August 2012 it crossed the heliopause and became the first spacecraft in interstellar space. It is now more than 24 billion kilometres away: its radio signal takes over 22 hours to reach Earth, arriving at about 160 bits per second.",
                facts = new[] { "Launch: 5 September 1977, Cape Canaveral", "Mass at launch: 825 kg", "Power: three radioisotope generators, 470 W at launch, slowly fading", "Antenna: 3.7 m high-gain dish", "Interstellar space since 25 August 2012", "Distance: over 24 billion km, more than 160 times the Earth–Sun distance" },
                scale = .11f,
                parts = new[]
                {
                    P("high-gain-antenna", "High-gain antenna", 0, 1.9f, 0, "A 3.7-metre dish that always points at Earth. It talks in S-band and X-band; at launch it sent 115 kilobits per second from Jupiter, and from interstellar space it manages about 160 bits per second, caught by NASA's Deep Space Network dishes."),
                    P("bus", "Spacecraft bus", 0, 0, 0, "A ten-sided aluminium ring 1.78 m across and 47 cm high. Its bays hold the computers, radios and power electronics, and the spherical hydrazine tank sits in the middle. The whole spacecraft is built around it."),
                    P("rtg", "Radioisotope generators", 1.5f, -.5f, 0, "Three radioisotope thermoelectric generators on a boom, turning the heat of decaying plutonium-238 into electricity. They made 470 watts at launch and lose about four watts a year, which is why instruments are being switched off one by one."),
                    P("science-boom", "Science boom and scan platform", 0, -.4f, 1.7f, "A 2.3-metre boom carrying the scan platform: the narrow- and wide-angle cameras that took the famous planet portraits and the Pale Blue Dot, the infrared and ultraviolet spectrometers, and the photopolarimeter."),
                    P("magnetometer-boom", "Magnetometer boom", -1.6f, .4f, 0, "A 13-metre fibreglass boom that unfolded after launch, holding four magnetometers well away from the spacecraft's own magnetic field. These are the instruments that still measure the interstellar magnetic field today."),
                    P("pra-antennas", "Plasma wave antennas", 0, -1.4f, -1.0f, "Two 10-metre whip antennas in a V, shared by the planetary radio astronomy and plasma wave instruments. The plasma wave instrument heard the 'hum' of interstellar plasma and still reports the density of the space around the craft."),
                    P("golden-record", "Golden Record", .9f, .7f, 1.1f, "A 12-inch gold-plated copper phonograph record bolted to the bus, with 115 images, greetings in 55 languages, sounds of Earth and 90 minutes of music. Its cover shows, in binary, how to play it and where Earth is."),
                    P("thrusters", "Attitude thrusters", 0, -1.0f, 0, "Sixteen small hydrazine thrusters keep the antenna pointed at Earth and steady the craft. Some had not fired since 1980 when engineers woke them in 2017 to take over from worn-out primaries, and they worked."),
                },
            },
            new Craft
            {
                id = "hubble", name = "Hubble Space Telescope", maker = "NASA / ESA", tagline = "An observatory in low Earth orbit since 1990",
                summary = "Hubble was carried to orbit by the shuttle Discovery on 24 April 1990 and deployed the next day. It is 13.2 m long and 4.2 m across, about the size of a school bus, and circles Earth every 95 minutes at roughly 540 km. Its 2.4-metre mirror was ground slightly wrong; a corrective package fitted in December 1993 fixed the blur, and five servicing missions kept it at the front of astronomy.",
                facts = new[] { "Launch: 24 April 1990, shuttle Discovery, STS-31", "Length 13.2 m, diameter 4.2 m, about 11 tonnes at launch", "Orbit: about 540 km, one circuit every 95 minutes", "Primary mirror 2.4 m across", "Five servicing missions, 1993 to 2009", "Points steadily to 7 thousandths of an arcsecond" },
                scale = .085f,
                parts = new[]
                {
                    P("aperture-door", "Aperture door", 0, 2.6f, .9f, "A hinged lid at the front that closes if the telescope ever swings toward the Sun, so sunlight can never reach the mirrors. It stuck open for a while after launch and has since worked faithfully."),
                    P("light-shield", "Light shield", 0, 1.7f, 0, "The forward tube, lined with baffles that soak up stray light so only starlight from the aimed direction reaches the mirror. Hubble looks like a telescope tube, and this is the tube."),
                    P("secondary-mirror", "Secondary mirror", 2.6f, .8f, 0, "A 30-centimetre convex mirror held on a truss 4.9 m in front of the primary. It bounces the light back through a hole in the primary mirror to the instruments behind, the Ritchey–Chrétien arrangement."),
                    P("primary-mirror", "Primary mirror", -2.8f, 0, 0, "The 2.4-metre, 828-kilogram main mirror, polished so smoothly that if it were scaled to the width of the Earth the biggest bump would be 15 cm high. Its edge was ground 2.2 micrometres too flat, the flaw that corrective optics fixed in 1993."),
                    P("solar-arrays", "Solar arrays", 0, .4f, 2.7f, "Two wings of solar cells, the third pair fitted in 2002, each about 7.6 by 2.6 metres. Together they make around 5.7 kilowatts and charge six nickel-hydrogen batteries for the dark half of each orbit."),
                    P("high-gain-antennas", "High-gain antennas", 0, -.3f, -2.9f, "Two 1.3-metre dishes on booms that relay data through NASA's TDRS satellites at up to a megabit per second: Hubble's pictures come down this way, several times a day."),
                    P("equipment-section", "Equipment section", 2.9f, -.7f, 0, "The ring around the mirror holding what keeps Hubble alive and pointed: gyroscopes, four reaction wheels that turn it without any thruster, the batteries, computers and radios. Astronauts swapped parts here on every servicing mission."),
                    P("aft-shroud", "Aft shroud and instruments", 0, -2.5f, 0, "The rear section where the science instruments sit in bays behind the mirror: Wide Field Camera 3, the Cosmic Origins Spectrograph, the imaging spectrograph and the Advanced Camera for Surveys, with three fine guidance sensors that lock onto guide stars."),
                },
            },
            new Craft
            {
                id = "falcon-9", name = "Falcon 9 Block 5", maker = "SpaceX", tagline = "The first orbital rocket to land and fly again",
                summary = "Falcon 9 is a two-stage rocket 70 m tall and 3.7 m wide, burning kerosene and liquid oxygen. Its first stage flies back and lands on legs, on a drone ship at sea or at the launch site; the first landing was on 21 December 2015, and Block 5, flying since May 2018, was designed for rapid reuse. Single boosters have now flown well over twenty missions, carrying Starlink, cargo and crews to the Space Station.",
                facts = new[] { "Height 70 m, diameter 3.7 m, mass 549 tonnes fuelled", "Nine Merlin 1D engines: 7,607 kN at sea level", "To low Earth orbit: 22.8 tonnes", "Propellants: liquid oxygen and rocket-grade kerosene (RP-1)", "First booster landing: 21 December 2015", "Block 5 first flew on 11 May 2018" },
                scale = .0165f,
                parts = new[]
                {
                    P("fairing", "Payload fairing", 0, 10, 0, "Two carbon-composite halves, 13.1 m long and 5.2 m across, that shelter the satellite through the atmosphere and split apart about three minutes into flight. They fall back under parachutes and are fished out of the sea to fly again."),
                    P("second-stage", "Second stage", 0, 6, 0, "A single Merlin Vacuum engine with a large niobium nozzle, 981 kN of thrust, that takes the payload from the edge of space to orbit in a burn of about six and a half minutes. It can relight to reach higher orbits."),
                    P("interstage", "Interstage", 0, 3, 0, "A composite sleeve joining the stages and housing the Merlin Vacuum nozzle. Pneumatic pushers separate the stages here about two and a half minutes after lift-off; the stage below then turns around and flies home."),
                    P("grid-fins", "Grid fins", 6.5f, 3, 0, "Four titanium lattice fins at the top of the booster that fold out during descent and steer it through the air, like the feathers on an arrow that can also turn. Titanium replaced aluminium in 2017 so they survive re-entry heat unrefurbished."),
                    P("first-stage", "First stage", 0, 0, 0, "The booster: 41 m of aluminium-lithium tanks holding subcooled liquid oxygen above kerosene, with cold-gas thrusters at the top to flip it after separation. Three engines relight for the boost-back and entry burns, and one for the landing."),
                    P("landing-legs", "Landing legs", -6.5f, -4, 0, "Four carbon-fibre legs with aluminium honeycomb that stay folded against the booster and deploy seconds before touchdown, spanning about 18 metres. Crushable cores in the legs soak up a hard landing."),
                    P("octaweb", "Octaweb and Merlin engines", 0, -5.5f, 0, "Nine Merlin 1D engines in an octagonal frame: eight around one centre engine, which is the one that lands the rocket. Together they produce 7,607 kilonewtons at lift-off, and the rocket can lose one and still reach orbit."),
                },
            },
            new Craft
            {
                id = "crew-dragon", name = "Crew Dragon", maker = "SpaceX", tagline = "The capsule that returned crew launches to the United States",
                summary = "Dragon 2 is a reusable capsule 8.1 m tall with its trunk and 4 m across, flying people and cargo to the International Space Station. Demo-2, on 30 May 2020, carried Bob Behnken and Doug Hurley: the first crew launched from American soil since the shuttle retired in 2011. It flies itself, docks autonomously, and comes home under four parachutes to a splashdown off Florida.",
                facts = new[] { "Height 8.1 m with trunk, diameter 4 m", "Pressurised volume 9.3 m³; carries up to 7, NASA flies 4", "Cargo to the Station: 6,000 kg; back to Earth: 3,000 kg", "First crewed flight: Demo-2, 30 May 2020", "Launch escape: eight SuperDraco engines, 71 kN each", "Lands at sea under four main parachutes" },
                scale = .13f,
                parts = new[]
                {
                    P("nosecone", "Nosecone", 0, 2.3f, 0, "A hinged cap that protects the docking port during launch and re-entry and swings open in orbit. On Inspiration4 in 2021 the port behind it was replaced by a glass cupola, the largest window ever flown."),
                    P("docking-system", "Docking system", 1.9f, 1.5f, 0, "A soft-capture ring and hooks built to the international docking standard, so Dragon can berth itself to the Station's ports without a robotic arm. The whole approach is automatic; the crew only watch."),
                    P("capsule", "Capsule", 0, 0, 0, "The pressurised cabin, 9.3 cubic metres, with four seats, three touch-screens in place of switch panels, and an environmental system good for a week on its own. The hull is reused: the same capsules have flown several crews."),
                    P("superdraco", "SuperDraco engines", 2.3f, .2f, 0, "Eight 3D-printed engines in four pods around the capsule, 71 kilonewtons each, burning stored hypergolic propellants. They fire only to pull the capsule away from a failing rocket, which they demonstrated in flight in January 2020."),
                    P("draco", "Draco thrusters", -2.3f, 1.0f, 0, "Sixteen small thrusters of about 400 newtons each, used for every manoeuvre in orbit: rendezvous, attitude control and the deorbit burn that brings the capsule home."),
                    P("heat-shield", "Heat shield", 0, -1.7f, 0, "A shield of PICA-X, SpaceX's version of NASA's phenolic-impregnated carbon ablator, covering the base. It chars and slowly erodes while re-entry heats it to around 1,600 °C, keeping the cabin cool."),
                    P("trunk", "Trunk", 0, -3.4f, 0, "An unpressurised cylinder below the capsule carrying external cargo, with solar cells over half its surface and fins that steady the stack during an abort. It is jettisoned before the deorbit burn and burns up."),
                    P("parachutes", "Parachutes", -1.9f, 2.5f, 0, "Two drogue chutes to steady the capsule, then four Mark 3 main parachutes with reinforced Zylon lines, slowing it to about 25 km/h for splashdown. Any three mains are enough for a safe landing."),
                },
            },
        };

        public static Craft Find(string id) => All.FirstOrDefault(c => c.id == id);
        public static string ToJson() => JsonUtility.ToJson(new Catalogue { craft = All }, true);
    }
}
