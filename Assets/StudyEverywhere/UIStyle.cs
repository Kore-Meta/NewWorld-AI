using UnityEngine;

namespace StudyEverywhere
{
    // New World UI tokens (product UI sample): warm ivory surfaces, ink text, sage actions, brass accents,
    // pill buttons at least 48 units tall, 24-unit card corners, a dark translucent focus status strip.
    // Headings use Lora, body text Plus Jakarta Sans when those fonts are in the project.
    public static class UIStyle
    {
        public static readonly Color WarmIvory = Hex(0xF6F3ED), Ink = Hex(0x202827), Sage = Hex(0x54736A), Brass = Hex(0xA68149),
            LightBorder = Hex(0xD9DDD8), Secondary = Hex(0x666666), Live = Hex(0x4ADE80);
        public static readonly Color SurfaceDark = new Color32(32, 40, 39, 217);                      // rgba(32,40,39,.85)
        public static readonly Color TintedSage = Color.Lerp(Hex(0xF6F3ED), Hex(0x54736A), 0.1f);    // sage at 10% over ivory
        // Night (dark mode, for places at night such as the Starry Meadow): deep slate cards, ivory text, light sage labels.
        public static readonly Color NightCard = Hex(0x1B2325), NightPill = Hex(0x2B3634), NightEdge = Hex(0x3E4A48), NightHover = Hex(0x36504A),
            NightText = Hex(0xEDE8DF), NightLabel = Hex(0x94B6AB), NightKey = Hex(0x111718);

        // The night colour for a day colour: ivory surfaces turn slate, borders and tracks a lighter slate, ink text ivory,
        // sage text a lighter sage, grey text a warm grey. Anything else (sage fills, brass, shadows) stays.
        public static bool Near(Color a, Color b) => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) < .04f;
        public static Color ToNight(Color c, bool text)
        {
            Color To(Color n) => new Color(n.r, n.g, n.b, c.a);
            if (text) return Near(c, Ink) ? To(NightText) : Near(c, Sage) ? To(NightLabel) : Near(c, Secondary) ? To(new Color(.68f, .66f, .62f)) : c;
            return Near(c, WarmIvory) ? To(NightCard) : Near(c, LightBorder) ? To(NightEdge) : Near(c, TintedSage) ? To(NightPill) : c;
        }

        static Color Hex(int rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);

        // The shared rounded sprite is a 128 px circle sliced at 64 px: this multiplier gives a corner radius in canvas units.
        public static float CornerMultiplier(float radius) => 64f / Mathf.Max(1, radius);
    }
}
