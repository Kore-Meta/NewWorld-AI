using System;
using System.Linq;

namespace StudyEverywhere
{
    // The models on the eight pillars round Orbit's hologram deck: every planet but the Earth (which hangs in the big
    // window), and Pluto. Figures from NASA's planetary fact sheets and Solar System Exploration pages (moon counts as
    // of 2025). The models are not to scale with one another; Orbit says so if asked. Exported with the craft
    // (Tools/CatServer/orbit-craft.json) so Orbit can answer about them too.
    public static class OrbitPlanets
    {
        [Serializable] public class Planet
        {
            public string id, name, kind, summary;
            public string[] facts;
        }

        public static readonly Planet[] All =
        {
            new Planet { id = "mercury", name = "Mercury", kind = "Rocky planet, closest to the Sun",
                summary = "The smallest planet and the fastest, racing round the Sun every 88 days. With almost no air to hold heat, its days reach 430 °C and its nights fall to −180 °C, and ice may hide in craters at its poles that sunlight never reaches.",
                facts = new[] { "Diameter: 4,879 km", "From the Sun: 57.9 million km", "Year: 88 Earth days", "One day (sunrise to sunrise): 176 Earth days", "Moons: none", "Visited by Mariner 10 and MESSENGER; BepiColombo is on its way" } },
            new Planet { id = "venus", name = "Venus", kind = "Rocky planet, wrapped in cloud",
                summary = "Nearly Earth's twin in size, but its thick carbon dioxide air traps so much heat that the surface is about 465 °C, hotter than Mercury. It spins backwards, so the Sun rises in the west, and a day there is longer than its year.",
                facts = new[] { "Diameter: 12,104 km", "From the Sun: 108.2 million km", "Year: 225 Earth days", "Turns once in 243 Earth days, backwards", "Surface pressure: about 92 times Earth's", "Moons: none" } },
            new Planet { id = "mars", name = "Mars", kind = "Rocky planet, the red one",
                summary = "Rusty iron dust makes Mars red. It has the tallest volcano known, Olympus Mons, nearly three times the height of Everest, and a canyon, Valles Marineris, as long as the United States is wide. Rovers such as Perseverance are searching it for signs of ancient life.",
                facts = new[] { "Diameter: 6,779 km", "From the Sun: 227.9 million km", "Year: 687 Earth days", "Day: 24 hours 37 minutes", "Moons: 2, Phobos and Deimos", "Olympus Mons: about 22 km high" } },
            new Planet { id = "jupiter", name = "Jupiter", kind = "Gas giant, the largest planet",
                summary = "More than twice as massive as all the other planets put together. Its stripes are bands of cloud racing in opposite directions, and the Great Red Spot is a storm wider than the Earth that has raged for centuries. Its moon Europa may hide an ocean under its ice.",
                facts = new[] { "Diameter: 139,820 km, 11 Earths across", "From the Sun: 778.5 million km", "Year: 11.9 Earth years", "Day: 9 hours 56 minutes, the shortest of any planet", "Moons: 95 officially recognised", "Visited by Juno, Galileo, the Voyagers and Pioneers" } },
            new Planet { id = "saturn", name = "Saturn", kind = "Gas giant, the ringed planet",
                summary = "Its rings are billions of pieces of ice and rock, some as small as sand grains and some as big as houses, spread 280,000 km wide but mostly only about ten metres thick. Saturn is so light for its size that it would float in a big enough bath.",
                facts = new[] { "Diameter: 116,460 km", "From the Sun: 1.43 billion km", "Year: 29.4 Earth years", "Day: 10 hours 33 minutes", "Moons: 274 known (2025), Titan the largest", "Cassini orbited it from 2004 to 2017" } },
            new Planet { id = "uranus", name = "Uranus", kind = "Ice giant, tipped on its side",
                summary = "Something huge once knocked Uranus over: it spins tilted 98 degrees, rolling round the Sun like a ball, so each pole has 42 years of sunlight and then 42 years of night. Methane in its air gives it its pale blue-green.",
                facts = new[] { "Diameter: 50,724 km", "From the Sun: 2.87 billion km", "Year: 84 Earth years", "Day: 17 hours 14 minutes", "Moons: 29 known", "Visited once, by Voyager 2 in 1986" } },
            new Planet { id = "neptune", name = "Neptune", kind = "Ice giant, the farthest planet",
                summary = "Dark, cold and windy: Neptune has the fastest winds in the solar system, over 2,000 km/h. It was the first planet found by mathematics, predicted from the way it tugged on Uranus before anyone saw it in 1846.",
                facts = new[] { "Diameter: 49,244 km", "From the Sun: 4.5 billion km", "Year: 165 Earth years", "Day: 16 hours 6 minutes", "Moons: 16 known, Triton the largest", "Visited once, by Voyager 2 in 1989" } },
            new Planet { id = "pluto", name = "Pluto", kind = "Dwarf planet in the Kuiper Belt",
                summary = "Counted as the ninth planet until 2006, Pluto is now a dwarf planet: a world of nitrogen ice, mountains of water ice and a hazy blue sky. NASA's New Horizons flew past in 2015 and found a bright heart-shaped plain, Tombaugh Regio.",
                facts = new[] { "Diameter: 2,377 km, smaller than our Moon", "Average distance from the Sun: 5.9 billion km", "Year: 248 Earth years", "Day: 6.4 Earth days", "Moons: 5, Charon the largest", "New Horizons flew by on 14 July 2015" } },
        };

        public static Planet Find(string id) => All.FirstOrDefault(p => p.id == id);
    }
}
