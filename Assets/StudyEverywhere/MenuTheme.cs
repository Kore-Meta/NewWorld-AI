using System;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // A place's menu colours. The UI kit is drawn in the café's own colours (UIStyle: warm ivory, sage, ink); a place
    // with its own palette maps those onto its own: surface for the ivory cards and pills, accent for sage (selected
    // pills, primary buttons, labels, slider fills), ink for text, with matching colours for night. Brass trim stays.
    // Builders recolour a finished menu with Recolour; WristPanel then marks selections and switches to night with it.
    [Serializable]
    public struct MenuTheme
    {
        public Color surface, border, tint, accent, ink, secondary;
        public Color nightSurface, nightPill, nightEdge, nightHover, nightText, nightLabel;

        static Color Hex(int rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
        static MenuTheme Make(int surface, int border, int accent, int ink, int secondary, int nSurface, int nPill, int nEdge, int nHover, int nText, int nLabel)
        {
            var t = new MenuTheme { surface = Hex(surface), border = Hex(border), accent = Hex(accent), ink = Hex(ink), secondary = Hex(secondary), nightSurface = Hex(nSurface), nightPill = Hex(nPill), nightEdge = Hex(nEdge), nightHover = Hex(nHover), nightText = Hex(nText), nightLabel = Hex(nLabel) };
            t.tint = Color.Lerp(t.surface, t.accent, .1f);
            return t;
        }

        // The café's colours: the kit as drawn.
        public static MenuTheme Cafe => new MenuTheme
        {
            surface = UIStyle.WarmIvory, border = UIStyle.LightBorder, tint = UIStyle.TintedSage, accent = UIStyle.Sage, ink = UIStyle.Ink, secondary = UIStyle.Secondary,
            nightSurface = UIStyle.NightCard, nightPill = UIStyle.NightPill, nightEdge = UIStyle.NightEdge, nightHover = UIStyle.NightHover, nightText = UIStyle.NightText, nightLabel = UIStyle.NightLabel,
        };
        // Haven: cream plaster, honey oak and walnut ink by day; dark walnut with candle-gold labels at night.
        public static MenuTheme Haven => Make(0xF5EEE3, 0xE3D7C5, 0x8C5B34, 0x2E2419, 0x7A6A58, 0x211913, 0x32271E, 0x4A3A2C, 0x5C4430, 0xF1E7D8, 0xD9AE7E);
        // Solace: white terrazzo and voile with the dusty blue of its chairs and navy ink by day; slate at night.
        public static MenuTheme Solace => Make(0xF6F4EE, 0xDCDFD8, 0x4E6A86, 0x1F2A3A, 0x6A7380, 0x161B24, 0x222A36, 0x344052, 0x3A4A62, 0xEDF0F2, 0xA9C0D6);
        // Orbit: pale warm grey with the amber of its ring lights and charcoal ink by day; deep space blue with amber labels at night.
        public static MenuTheme Orbit => Make(0xECE9E3, 0xD3CFC7, 0xC07A2C, 0x1C222C, 0x6D7079, 0x11161E, 0x1C2430, 0x2E3A4A, 0x3A4A60, 0xECE9E2, 0xE2A95B);
        // The holodeck's own panels: deep blue glass with cyan light and pale text, the same by day and night.
        public static MenuTheme Holo => Make(0x0D1A24, 0x1E3A4A, 0x3FC6F0, 0xDCF4FF, 0x7FB7CC, 0x0D1A24, 0x163040, 0x1E3A4A, 0x27485C, 0xDCF4FF, 0x3FC6F0);
        // Evercrest: snow-white limestone with the tarn's glacial teal and slate ink by day; spruce-dark slate with pale teal labels at night.
        public static MenuTheme Evercrest => Make(0xF3F2EE, 0xD9DCDA, 0x2F6E73, 0x1D2629, 0x67727A, 0x161D20, 0x222C30, 0x334247, 0x3A5458, 0xECF0EF, 0x9FCCCB);
        // The lobby: pale limestone with the deep green of its living walls.
        public static MenuTheme Lobby => Make(0xF2ECE2, 0xDCD3C4, 0x2F5141, 0x22201C, 0x6B655C, 0x1A201D, 0x29322E, 0x3B4641, 0x35503F, 0xEDE8DF, 0x9CC2AE);
        // Seabreeze: sun-bleached sand with the lagoon's teal and driftwood-dark ink by day; the night sea with aqua labels at night.
        public static MenuTheme Seabreeze => Make(0xF7F2E8, 0xE4D9C6, 0x1E8580, 0x1D2A30, 0x6C7A7E, 0x0E1B24, 0x172B37, 0x24404F, 0x2A5363, 0xEAF3F2, 0x82D4CE);

        // New York: warm cream plaster with the café's bottle-green counter and near-black steel ink by day; bottle green
        // gone to night with brass labels after dusk.
        public static MenuTheme NewYork => Make(0xF4EEE2, 0xDDD3C2, 0x1F4D3A, 0x1C201E, 0x6B6A62, 0x121815, 0x1D2722, 0x2D3A33, 0x2F4A3C, 0xEEE9DF, 0xD4B47C);

        static bool Near(Color a, Color b) => UIStyle.Near(a, b);
        static Color Keep(Color from, Color to) => new Color(to.r, to.g, to.b, from.a);

        // A kit colour (as UIPill, UIRound and UIText draw it) in this theme; anything else unchanged.
        public Color FromKit(Color c)
        {
            if (Near(c, UIStyle.WarmIvory)) return Keep(c, surface);
            if (Near(c, UIStyle.TintedSage)) return Keep(c, tint);
            if (Near(c, UIStyle.LightBorder)) return Keep(c, border);
            if (Near(c, UIStyle.Sage)) return Keep(c, accent);
            if (Near(c, UIStyle.Ink)) return Keep(c, ink);
            if (Near(c, UIStyle.Secondary)) return Keep(c, secondary);
            if (Near(c, Color.Lerp(UIStyle.Sage, UIStyle.Ink, .55f))) return Keep(c, Color.Lerp(accent, ink, .55f));            // a primary pill's key edge
            if (Near(c, Color.Lerp(UIStyle.LightBorder, UIStyle.Ink, .3f))) return Keep(c, Color.Lerp(border, ink, .3f));      // a plain pill's key edge
            if (Near(c, Color.Lerp(UIStyle.WarmIvory, UIStyle.LightBorder, .45f))) return Keep(c, Color.Lerp(surface, border, .45f));   // disabled
            if (Near(c, Color.Lerp(UIStyle.Ink, Color.black, .3f))) return Keep(c, Color.Lerp(ink, Color.black, .3f));        // pressed primary
            return c;
        }

        // A day colour of this theme at night: cards and pills dark, borders a lighter dark, text light, labels warm.
        public Color ToNight(Color c, bool text)
        {
            if (text) return Near(c, ink) ? Keep(c, nightText) : Near(c, accent) ? Keep(c, nightLabel) : Near(c, secondary) ? Keep(c, new Color(.68f, .66f, .62f)) : c;
            return Near(c, surface) ? Keep(c, nightSurface) : Near(c, border) ? Keep(c, nightEdge) : Near(c, tint) ? Keep(c, nightPill) : c;
        }

        // Selected pills take the accent with surface text; the others the surface (or, at night, a dark pill) with ink.
        public void Mark(Button b, bool on, bool dark)
        {
            var c = b.colors;
            c.normalColor = on ? accent : dark ? nightPill : surface;
            b.colors = c;
            var label = b.GetComponentInChildren<TMPro.TMP_Text>();
            if (label) label.color = on ? surface : dark ? nightText : ink;
        }

        // Recolours everything under root from the kit's colours to this theme's (images, text and button tints).
        public void Recolour(Transform root)
        {
            foreach (var g in root.GetComponentsInChildren<Graphic>(true)) g.color = FromKit(g.color);
            foreach (var b in root.GetComponentsInChildren<Selectable>(true))
            {
                var cb = b.colors;
                cb.normalColor = FromKit(cb.normalColor); cb.highlightedColor = FromKit(cb.highlightedColor); cb.selectedColor = FromKit(cb.selectedColor);
                cb.pressedColor = FromKit(cb.pressedColor); cb.disabledColor = FromKit(cb.disabledColor);
                b.colors = cb;
            }
        }
    }
}
