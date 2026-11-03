using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StudyEverywhere
{
    // The holodeck's Math mode. A panel beside the deck takes an equation on its keypad (MathExpr: x, y, t and the
    // usual functions) and the deck draws it as glowing lines: y = f(x) as a curve, z = f(x, y) as a surface, both
    // animated when t appears. Presets add three pieces of real black-hole mathematics:
    //  · the funnel: Flamm's paraboloid, the curved space round a Schwarzschild black hole, with a planet on a
    //    precessing orbit and a ray of light bent near the photon sphere, both integrated from the geodesic equations;
    //  · Penrose diagrams: Schwarzschild computed from Kruskal coordinates (an infaller's worldline crossing the horizon,
    //    light cones along it), and the extremal Reissner–Nordström diagram drawn schematically;
    //  · the Aretakis instability: a scalar wave on an extremal Reissner–Nordström black hole, solved numerically in
    //    ingoing coordinates: on the horizon the field decays but its radial derivative does not (the Aretakis constant)
    //    and the second derivative grows without bound; the same wave on a sub-extremal hole decays entirely.
    // The builder (MathBuilder) makes the panel, the renderers and the labels; this fills their meshes each frame.
    public sealed class HoloMath : MonoBehaviour
    {
        public enum Kind { Custom, Funnel, PenroseSchwarzschild, PenroseExtremal, Aretakis }
        // The pieces, as the deck's item pill and list show them.
        public static readonly string[] PieceIds = { "equation", "funnel", "penrose", "penrose-extremal", "aretakis" };
        public static readonly string[] PieceNames = { "Your equation", "Space round a black hole", "Penrose diagram", "Penrose: extremal hole", "Aretakis instability" };

        public HoloDeck deck;
        [Tooltip("Everything on the stage; the deck grows and shrinks it.")] public Transform root;
        [Tooltip("The surface fill (its child \"Wireframe\" carries the surface lines).")] public MeshFilter fill;
        public MeshFilter fillWire;
        [Tooltip("More lines: axes, orbits, diagrams (faint) and the bright set (trails, cones).")] public MeshFilter lines, bright;
        [Tooltip("Small glowing spheres: the planet, the ray of light, the infaller.")] public Transform[] dots;
        [Tooltip("Labels placed by each drawing.")] public TMP_Text[] labels;
        [Tooltip("The graph's own wide line (the deck's look would otherwise give the surface the models' thin wireframe).")] public Material lineMaterial;
        [Tooltip("For the deck's look: the fill and the dots, their real finish and whether they glow.")]
        public Renderer[] renderers; public Material[] real; public bool[] glowing;
        [Header("The pop-up (the deck's Calculator / Variables pill opens it)")]
        public GameObject panel;
        [Tooltip("Its two pages: the keyboard for equations, the variables for the Aretakis piece.")] public GameObject keyboardPage, variablesPage;
        public TMP_Text expression, status;
        public Button[] keys; public string[] keyValues;
        public Button[] presets; public string[] presetIds;
        [Tooltip("Aretakis variables: each button sets one (\"charge:0.9\", \"mode:1\", \"pulse:near\", \"speed:2\").")] public Button[] vars; public string[] varValues;
        public TMP_Text readout;
        [Tooltip("Back to the console, and close: both put the pop-up away.")] public Button popupBack, popupClose;
        [Header("Size on the deck")]
        public float width = .62f, height = .26f;

        Kind kind;
        MathExpr expr;
        string text = "sin(x) * cos(y + t)";
        float since, vertical;
        bool failed;   // the last Plot could not read the equation (the status line shows why)
        readonly HoloLines surfaceLines = new HoloLines(), faint = new HoloLines(), hot = new HoloLines();
        Mesh fillMesh, fillWireMesh, linesMesh, brightMesh;
        readonly List<Vector3> fillVerts = new List<Vector3>(); readonly List<int> fillTris = new List<int>();
        public Kind Current => kind;

        void Awake()
        {
            Init();
            foreach (var d in dots) if (d) d.gameObject.SetActive(false);
            foreach (var l in labels) if (l) l.transform.parent.gameObject.SetActive(false);
        }
        // meshes of its own (the builder leaves the filters empty); also made on the first draw in the editor
        void Init() { if (fillMesh == null) { fillMesh = NewMesh(fill); fillWireMesh = NewMesh(fillWire); linesMesh = NewMesh(lines); brightMesh = NewMesh(bright); } }
        static Mesh NewMesh(MeshFilter f) { if (!f) return null; var m = new Mesh { name = f.name + " (runtime)" }; m.MarkDynamic(); f.sharedMesh = m; return m; }
        void OnEnable()
        {
            for (int i = 0; keys != null && i < keys.Length; i++) { int k = i; if (keys[i]) keys[i].onClick.AddListener(() => Key(keyValues[k])); }
            for (int i = 0; presets != null && i < presets.Length; i++) { int k = i; if (presets[i]) presets[i].onClick.AddListener(() => Preset(presetIds[k])); }
            for (int i = 0; vars != null && i < vars.Length; i++) { int k = i; if (vars[i]) vars[i].onClick.AddListener(() => Variable(varValues[k])); }
            if (popupBack) popupBack.onClick.AddListener(() => { ShowPopup(false); if (deck) deck.Refresh(); });
            if (popupClose) popupClose.onClick.AddListener(() => { ShowPopup(false); if (deck) deck.Refresh(); });
            Localization.Changed += Relabel;
            if (expr == null) Plot();
            else Tell();
        }
        // the card and the status line again in the new language (the labels follow on the next frame)
        void Relabel()
        {
            if (deck && deck.CurrentMode == HoloDeck.Mode.Math) Tell();
            if (failed && status) { MathExpr.Parse(text, out var error); status.text = error; }
        }
        static string T(string english) => Localization.Tr(english);
        void OnDisable()
        {
            Localization.Changed -= Relabel;
            if (keys != null) foreach (var k in keys) if (k) k.onClick.RemoveAllListeners();
            if (presets != null) foreach (var p in presets) if (p) p.onClick.RemoveAllListeners();
            if (vars != null) foreach (var v in vars) if (v) v.onClick.RemoveAllListeners();
            if (popupBack) popupBack.onClick.RemoveAllListeners();
            if (popupClose) popupClose.onClick.RemoveAllListeners();
        }

        // ---- the pop-up: the keyboard for an equation, the variables for the Aretakis piece
        public string PopupName => kind == Kind.Aretakis ? "Variables" : "Calculator";
        public bool PopupOpen => panel && panel.activeSelf;
        public void TogglePopup() => ShowPopup(!PopupOpen);
        public void ShowPopup(bool open)
        {
            if (!panel) return;
            if (panel.activeSelf != open) panel.SetActive(open);
            bool variables = kind == Kind.Aretakis;
            if (keyboardPage && keyboardPage.activeSelf != !variables) keyboardPage.SetActive(!variables);
            if (variablesPage && variablesPage.activeSelf != variables) variablesPage.SetActive(variables);
            if (variables) MarkVariables();
        }
        public int PieceIndex => kind == Kind.Custom ? 0 : kind == Kind.Funnel ? 1 : kind == Kind.PenroseSchwarzschild ? 2 : kind == Kind.PenroseExtremal ? 3 : 4;
        public void Piece(int i) { if (i >= 0 && i < PieceIds.Length) Preset(PieceIds[i]); }

        // ---- the keypad
        public void Key(string k)
        {
            switch (k)
            {
                case "⌫": if (text.Length > 0) text = text.Substring(0, text.Length - 1); break;
                case "Clear": text = ""; break;
                case "Plot": Plot(); return;
                default:
                    // a function key types its bracket too; a letter after a letter gets a space, so "x y" is x·y and "sin" stays a word
                    if (k.Length > 1 && char.IsLetter(k[0]) && k != "pi") k += "(";
                    if (text.Length > 0 && char.IsLetterOrDigit(text[text.Length - 1]) && char.IsLetter(k[0]) && k.Length == 1) text += " ";
                    text += k; break;
            }
            if (expression) expression.text = text + "_";
        }
        public void Plot()
        {
            var e = MathExpr.Parse(text, out var error);
            failed = e == null;
            if (e == null) { if (status) status.text = error; if (expression) expression.text = text + "_"; return; }
            expr = e; kind = Kind.Custom; since = 0; vertical = 0;
            if (expression) expression.text = text + "_";
            Tell();
        }
        public void Preset(string id)
        {
            switch (id)
            {
                case "equation": if (expr == null) Plot(); else { kind = Kind.Custom; Tell(); } ShowPopup(true); return;
                case "funnel": kind = Kind.Funnel; SetupFunnel(); ShowPopup(false); break;
                case "penrose": kind = Kind.PenroseSchwarzschild; ShowPopup(false); break;
                case "penrose-extremal": kind = Kind.PenroseExtremal; ShowPopup(false); break;
                case "aretakis": kind = Kind.Aretakis; SetupAretakis(); ShowPopup(true); break;
                case "aretakis-sub": kind = Kind.Aretakis; charge = .9f; SetupAretakis(); ShowPopup(true); break;
                default: text = id; Plot(); return;   // an equation preset
            }
            since = 0; Tell();
        }
        // The deck's arrows: on to the next piece; its Parts pill: start this one again.
        public void Step(int dir) => Piece((PieceIndex + dir + PieceIds.Length) % PieceIds.Length);
        public void Replay() { since = 0; if (kind == Kind.Aretakis) SetupAretakis(); else if (kind == Kind.Funnel) SetupFunnel(); }
        public void Reskin()
        {
            if (!deck) return;
            for (int i = 0; i < renderers.Length; i++) deck.Dress(renderers[i], real != null && i < real.Length ? real[i] : null, glowing != null && i < glowing.Length && glowing[i], deck.Realistic);
            if (fillWire && lineMaterial) fillWire.GetComponent<Renderer>().sharedMaterial = lineMaterial;
        }

        // What the card says.
        void Tell()
        {
            if (!deck) return;
            switch (kind)
            {
                case Kind.Custom:
                    bool surface = expr != null && expr.UsesY;
                    deck.Info(T("MATH") + " · " + T(surface ? "SURFACE" : "CURVE") + (expr != null && expr.UsesT ? " · " + T("ANIMATED") : ""), (surface ? "z = " : "y = ") + (expr?.Source ?? ""),
                        T(surface ? "A surface over the x–y plane, x and y from −4 to 4" : "A curve, x from −4 to 4"),
                        T(expr != null && expr.UsesT ? "t is the time in seconds since Plot, so the graph moves. Height rescales itself to fit the deck." : "Add t to the equation to animate it: for example sin(x − t). Height rescales itself to fit the deck."),
                        new[] { T("Functions: sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow"), T("Constants: pi, e. Powers with ^. Products can be implicit: 2x, 3sin(x), (x+1)(x−1)."), T("Try: sin(sqrt(x^2+y^2) − 2t), exp(−(x^2+y^2)/4)cos(3t), x^2 − y^2, sin(x)sin(y)sin(t)") });
                    if (status) status.text = T("Plotted");
                    break;
                case Kind.Funnel:
                    deck.Info(T("MATH · GENERAL RELATIVITY"), T("Space round a black hole"), T("Flamm's paraboloid, with a planet and a ray of light"), T(
                        "The funnel is the curved space of a Schwarzschild black hole of mass M: a slice through its equator, with its stretching drawn as depth (z = 2√(2M(r − 2M))). The throat is the event horizon at r = 2M. The planet follows a true orbit, integrated from the geodesic equation d²r/dτ² = −M/r² + L²/r³ − 3ML²/r⁴: its ellipse turns a little every lap, the precession that Mercury shows. The ray of light (d²u/dφ² + u = 3Mu², u = 1/r) sweeps in near the photon sphere at r = 3M and is bent away again."),
                        new[] { T("Rings: the horizon (2M), the photon sphere (3M, brighter), then 4M, 6M, 8M, 10M, 12M"), T("Depth is the stretching of space, not a real dip: a model, not a picture"), T("Dots: the planet (warm) and the ray of light (bright)") });
                    break;
                case Kind.PenroseSchwarzschild:
                    deck.Info(T("MATH · PENROSE DIAGRAM"), T("Schwarzschild black hole"), T("All of spacetime on one card, light always at 45°"), T(
                        "Each point is a sphere; time runs up, light moves along the diagonals. The diagram is computed from Kruskal–Szekeres coordinates, squashed with the arctangent so that infinity fits on the page. The right diamond is our universe outside the hole; the triangle above it is the inside, ending at the singularity r = 0 (a moment in time, not a place). The moving dot is someone falling in: their light cone stays at 45°, so once past the horizon every future path ends at r = 0. The faint lines are constant r and constant t."),
                        new[] { T("i0: spatial infinity. i+ and i−: the far future and past. The upper and lower right edges: where light goes and comes from (null infinity)"), T("The horizon is the diagonal from the centre to i+: nothing special happens there to the one falling"), T("The left diamond and the lower triangle are the other exterior and the white hole of the full solution, never reached from here") });
                    break;
                case Kind.PenroseExtremal:
                    deck.Info(T("MATH · PENROSE DIAGRAM"), T("Extremal Reissner–Nordström"), T("A charged hole with charge equal to its mass (Q = M): schematic"), T(
                        "With Q = M the two horizons of a charged black hole merge into one at r = M, and the diagram changes shape: the singularity r = 0 is time-like, a vertical line you can steer away from, and beyond the horizon lies an inner (Cauchy) horizon and then, in the mathematics, another exterior region, and another, without end. The curve is an infaller passing through to the next region. Extremal holes have zero surface gravity and zero Hawking temperature; they are the setting of the Aretakis instability."),
                        new[] { T("Schematic: the shapes are right, the spacing is not computed"), T("Real charged holes are never exactly extremal, but near-extremal black holes and the extremal Kerr limit (a maximally spinning hole) behave much like this"), T("Next: the Aretakis instability on this horizon") });
                    break;
                case Kind.Aretakis:
                {
                    bool ext = charge >= .999f;
                    string hole = ext ? T("an extremal Reissner–Nordström black hole (Q = M)") : Localization.Format("a Reissner–Nordström black hole with Q = {0:0.00} M", charge);
                    string pulseName = pulse == "horizon" ? "on the horizon" : pulse == "near" ? "near it" : "away";
                    deck.Info(T("MATH · HORIZON INSTABILITY"), T(ext ? "The Aretakis instability" : "The same wave on a sub-extremal hole"), Localization.Format("A scalar wave on {0}, computed live. Change its variables with the pill.", hole),
                        T("A scalar field ψ obeys the wave equation 2∂v∂rφ + ∂r(f∂rφ) − V φ = 0 (φ = rψ, ingoing coordinates v and r, f = 1 − 2M/r + Q²/r²) and is solved here step by step from a pulse. The halo round the hole is the field's profile ψ(r) now: it collapses as the wave leaves and falls in.") +
                        (Localization.Code == "ja" ? "" : " ") + T(ext ? "On an extremal horizon f and f′ both vanish, so there the equation says ∂v(∂rφ) = 0: the slope of the field at the horizon never decays. It is a conserved charge, the Aretakis constant H₀, and it forces the next derivative to grow in proportion to v: watch the kink at the horizon stay sharp while the rest of the halo flattens. Stefanos Aretakis proved this in 2011."
                             : "Away from extremality f′ is not zero on the horizon, and there the equation reads ∂v(∂rφ) = −½f′∂rφ: the slope decays at the surface-gravity rate κ, and so does everything else. Set Q = M to see the difference."),
                        new[] { Localization.Format("Variables: Q/M = {0:0.00}, r₊ = {1:0.00}M, surface gravity κ = {2:0.000}/M, mode ℓ = {3}, pulse {4}", charge, rH, kappa, ell, T(pulseName)), T("Traces at the right, on the horizon: the field, its slope and its curvature against v"), T("Numerics: 240 points in r from the horizon to 12M, third-order Runge–Kutta in v, a little smoothing; a model of the equations, not an exact solution") });
                    break;
                }
            }
        }

        void Update()
        {
            if (!root || !root.gameObject.activeInHierarchy) return;
            Init();
            float dt = Time.deltaTime; since += dt;
            faint.Clear(); hot.Clear(); surfaceLines.Clear(); fillVerts.Clear(); fillTris.Clear();
            foreach (var d in dots) if (d) d.gameObject.SetActive(false);
            foreach (var l in labels) if (l) l.transform.parent.gameObject.SetActive(false);
            switch (kind)
            {
                case Kind.Custom: DrawCustom(); break;
                case Kind.Funnel: DrawFunnel(dt); break;
                case Kind.PenroseSchwarzschild: DrawPenrose(); break;
                case Kind.PenroseExtremal: DrawPenroseExtremal(); break;
                case Kind.Aretakis: DrawAretakis(); break;
            }
            if (fillMesh) { fillMesh.Clear(); if (fillVerts.Count > 0) { fillMesh.SetVertices(fillVerts); fillMesh.SetTriangles(fillTris, 0); fillMesh.RecalculateNormals(); fillMesh.RecalculateBounds(); } }
            if (fillWireMesh) surfaceLines.Apply(fillWireMesh);
            if (linesMesh) faint.Apply(linesMesh);
            if (brightMesh) hot.Apply(brightMesh);
            var head = Camera.main;
            if (head) foreach (var l in labels) if (l && l.transform.parent.gameObject.activeSelf) { var c = l.transform.parent; var away = c.position - head.transform.position; if (away.sqrMagnitude > 1e-4f) c.rotation = Quaternion.LookRotation(away); }
        }
        void Dot(int i, Vector3 at, float size) { if (i < dots.Length && dots[i]) { dots[i].gameObject.SetActive(true); dots[i].localPosition = at; dots[i].localScale = Vector3.one * size; } }
        // each label is a small canvas (the builder's): bold white words on a dark plate fitted to them (HoloLabel), placed,
        // shown and turned to the viewer by its canvas (the words in the chosen language: set every frame, so the scene's
        // Localizer would never catch them). Labels sit beside what they name, not on the lines: `align` puts the plate's
        // left edge (1), middle (0) or right edge (−1) at `at`, and `from`, when given, is the feature named, joined to
        // the nearest point of the plate by a short faint leader line.
        void Label(int i, string s, Vector3 at, int align = 0, Vector3? from = null)
        {
            if (i >= labels.Length || !labels[i]) return;
            var c = labels[i].transform.parent; c.gameObject.SetActive(true);
            var words = Localization.Tr(s); if (labels[i].text != words) labels[i].text = words;
            var size = HoloLabel.Fit(labels[i]) * c.localScale.x;   // the plate, in this mode's own units
            var centre = at + new Vector3(align * size.x / 2, 0, 0);
            c.localPosition = centre;
            if (from is Vector3 f)
            {
                var edge = new Vector3(Mathf.Clamp(f.x, centre.x - size.x / 2, centre.x + size.x / 2), Mathf.Clamp(f.y, centre.y - size.y / 2, centre.y + size.y / 2), centre.z);
                if ((edge - f).sqrMagnitude > 1e-6f) faint.Add(f, edge);
            }
        }
        void Axes(float w, float h, float d)
        {
            // the frame of the domain at z = 0, and the vertical through the origin
            var c = new[] { new Vector3(-w, 0, -d), new Vector3(w, 0, -d), new Vector3(w, 0, d), new Vector3(-w, 0, d) };
            faint.Add(c, true);
            faint.Add(new Vector3(-w, 0, 0), new Vector3(w, 0, 0)); faint.Add(new Vector3(0, 0, -d), new Vector3(0, 0, d));
            faint.Add(new Vector3(0, -h, 0), new Vector3(0, h, 0));
        }

        // ================================================================== the equation
        const int Curve = 160, Grid = 40; const float Domain = 4;
        void DrawCustom()
        {
            if (expr == null) return;
            float w = width / 2, t = since;
            if (!expr.UsesY)
            {
                var pts = new List<Vector3>(Curve + 1); float peak = 1e-3f;
                var ys = new float[Curve + 1];
                for (int i = 0; i <= Curve; i++) { double x = -Domain + 2 * Domain * i / Curve; ys[i] = Clamp(expr.Eval(x, 0, t)); peak = Mathf.Max(peak, Mathf.Abs(ys[i])); }
                Fit(peak);
                for (int i = 0; i <= Curve; i++) pts.Add(new Vector3(-w + width * i / Curve, ys[i] * vertical, 0));
                Axes(w, height, .02f);
                hot.Add(pts);
                return;
            }
            // the surface: values first (to fit the height), then the fill and the lines
            int n = Grid; var z = new float[(n + 1) * (n + 1)]; float max = 1e-3f;
            for (int j = 0; j <= n; j++) for (int i = 0; i <= n; i++)
            {
                double x = -Domain + 2 * Domain * i / n, y = -Domain + 2 * Domain * j / n;
                float v = Clamp(expr.Eval(x, y, t)); z[j * (n + 1) + i] = v; max = Mathf.Max(max, Mathf.Abs(v));
            }
            Fit(max);
            for (int j = 0; j <= n; j++) for (int i = 0; i <= n; i++) fillVerts.Add(new Vector3(-w + width * i / n, z[j * (n + 1) + i] * vertical, -w + width * j / n));
            for (int j = 0; j < n; j++) for (int i = 0; i < n; i++)
            {
                int a = j * (n + 1) + i, b = a + 1, c = a + n + 1, d = c + 1;
                fillTris.Add(a); fillTris.Add(c); fillTris.Add(b); fillTris.Add(b); fillTris.Add(c); fillTris.Add(d);
            }
            for (int j = 0; j <= n; j += 2) for (int i = 0; i < n; i++) surfaceLines.Add(fillVerts[j * (n + 1) + i], fillVerts[j * (n + 1) + i + 1]);
            for (int i = 0; i <= n; i += 2) for (int j = 0; j < n; j++) surfaceLines.Add(fillVerts[j * (n + 1) + i], fillVerts[(j + 1) * (n + 1) + i]);
            Axes(w, height, w);
        }
        static float Clamp(double v) => double.IsNaN(v) || double.IsInfinity(v) ? 0 : (float)Math.Max(-1e6, Math.Min(1e6, v));
        // Height so the tallest value fills the deck's height, eased, so a moving graph does not jump.
        void Fit(float peak) { float want = height / Mathf.Max(peak, 1e-3f); vertical = vertical <= 0 ? want : Mathf.Lerp(vertical, want, 1 - Mathf.Exp(-Time.deltaTime * 4)); }

        // ================================================================== the funnel
        const float M = 1, FunnelR = 14, FunnelScale = .032f;
        double pr, pv, pphi;   // the planet: r, dr/dτ, φ
        double planetE, planetL;
        readonly List<Vector3> trail = new List<Vector3>();
        List<Vector3> ray; float rayAt;
        static float FunnelZ(double r) => -(float)(2 * Math.Sqrt(2 * M * Math.Max(r - 2 * M, 0)) * FunnelScale);
        static Vector3 Funnel(double r, double phi) => new Vector3((float)(r * Math.Cos(phi) * FunnelScale), FunnelZ(r), (float)(r * Math.Sin(phi) * FunnelScale));
        void SetupFunnel()
        {
            // a planet between r = 7M and r = 20M: E and L from the two turning points of the effective potential
            double r1 = 7, r2 = 19, f1 = 1 - 2 * M / r1, f2 = 1 - 2 * M / r2;
            double L2 = (f2 - f1) / (f1 / (r1 * r1) - f2 / (r2 * r2)); planetL = Math.Sqrt(L2); planetE = Math.Sqrt(f1 * (1 + L2 / (r1 * r1)));
            pr = r1; pv = 0; pphi = 0; trail.Clear();
            // a ray of light from far away with impact parameter 5.3M (just outside the critical 3√3 M ≈ 5.2M): in, round, out
            ray = new List<Vector3>(); double b = 5.3, u = 1.0 / 40, du = Math.Sqrt(Math.Max(1 / (b * b) - u * u * (1 - 2 * M * u), 0)), dphi = .01;   // u grows as the ray comes in
            for (int i = 0; i < 4000 && u > 1.0 / 40 - 1e-9 || i < 10; i++)
            {
                // d²u/dφ² + u = 3Mu², RK2
                double k1 = 3 * M * u * u - u, um = u + du * dphi / 2, dum = du + k1 * dphi / 2, k2 = 3 * M * um * um - um;
                u += dum * dphi; du += k2 * dphi;
                if (u <= 0 || u > .5 || i > 3000) break;
                if (1 / u <= FunnelR) ray.Add(Funnel(1 / u, i * dphi + Math.PI * .9));
            }
            rayAt = 0;
        }
        void DrawFunnel(float dt)
        {
            if (ray == null) SetupFunnel();
            // the funnel: rings and spokes
            foreach (double r in new[] { 2.02, 2.3, 2.7, 3.0, 3.6, 4.4, 5.4, 6.6, 8, 10, 12, 14 })
            {
                var target = Math.Abs(r - 3) < 1e-6 ? hot : faint; Vector3 last = Funnel(r, 0);
                for (int i = 1; i <= 72; i++) { var p = Funnel(r, i * Math.PI * 2 / 72); target.Add(last, p); last = p; }
            }
            for (int k = 0; k < 24; k++)
            {
                double phi = k * Math.PI * 2 / 24; Vector3 last = Funnel(2.02, phi);
                for (int i = 1; i <= 40; i++) { double r = 2.02 + (FunnelR - 2.02) * Math.Pow(i / 40.0, 1.6); var p = Funnel(r, phi); faint.Add(last, p); last = p; }
            }
            // the planet: four geodesic steps a frame, RK4 in proper time
            for (int s = 0; s < 4; s++) PlanetStep(dt * 1.8);
            var at = Funnel(pr, pphi); Dot(0, at, .012f);
            trail.Add(at); if (trail.Count > 400) trail.RemoveAt(0);
            hot.Add(trail);
            // the ray of light along its bent path
            if (ray.Count > 1)
            {
                faint.Add(ray);
                rayAt = Mathf.Repeat(rayAt + dt * 220, ray.Count - 1);
                int i = (int)rayAt; Dot(1, Vector3.Lerp(ray[i], ray[Mathf.Min(i + 1, ray.Count - 1)], rayAt - i), .009f);
                int tail = Mathf.Max(0, i - 40); for (int k = tail; k < i; k++) hot.Add(ray[k], ray[k + 1]);
            }
            // the two rings named from above the throat, where nothing is drawn (the funnel falls away outward and down),
            // one to each side, each with a leader to its ring; the planet's name rides just above it
            Label(0, "event horizon  r = 2M", new Vector3(-.03f, .1f, 0), -1, Funnel(2.02, Math.PI));
            Label(1, "photon sphere  r = 3M", new Vector3(.03f, .1f, 0), 1, Funnel(3, 0));
            Label(2, "planet", at + Vector3.up * .045f);
        }
        void PlanetStep(double h)
        {
            // state (r, ṙ, φ); ṙ' = −M/r² + L²/r³ − 3ML²/r⁴, φ' = L/r²
            double L = planetL;
            void F(double r, double v, out double dr, out double dv, out double dphi) { dr = v; dv = -M / (r * r) + L * L / (r * r * r) - 3 * M * L * L / (r * r * r * r); dphi = L / (r * r); }
            F(pr, pv, out var a1, out var b1, out var c1);
            F(pr + h / 2 * a1, pv + h / 2 * b1, out var a2, out var b2, out var c2);
            F(pr + h / 2 * a2, pv + h / 2 * b2, out var a3, out var b3, out var c3);
            F(pr + h * a3, pv + h * b3, out var a4, out var b4, out var c4);
            pr += h / 6 * (a1 + 2 * a2 + 2 * a3 + a4); pv += h / 6 * (b1 + 2 * b2 + 2 * b3 + b4); pphi += h / 6 * (c1 + 2 * c2 + 2 * c3 + c4);
            if (pr < 2.05 || pr > FunnelR || double.IsNaN(pr)) SetupFunnel();
        }

        // ================================================================== Penrose diagrams
        const float Diagram = .5f;   // half a metre wide
        // Kruskal U, V from Schwarzschild t and r (outside the horizon), compactified with atan into the (X, T) of the page.
        static Vector3 Page(double U, double V) { double u = Math.Atan(U), v = Math.Atan(V); return new Vector3((float)((v - u) / Math.PI) * Diagram, (float)((v + u) / Math.PI) * Diagram, 0); }
        static Vector3 PageTR(double t, double r)
        {
            double rs = r + 2 * M * Math.Log(Math.Abs(r / (2 * M) - 1));
            return Page(-Math.Exp(-(t - rs) / (4 * M)), Math.Exp((t + rs) / (4 * M)));
        }
        // Someone falling from rest far away, in Painlevé–Gullstrand time T (regular at the horizon): r(T), then U and V in
        // a form with no infinities at r = 2M.
        static Vector3 Infaller(double T, out double r)
        {
            double r0 = 8;
            double k = Math.Pow(r0, 1.5) - 1.5 * Math.Sqrt(2 * M) * T; r = k > 0 ? Math.Pow(k, 2.0 / 3) : 0;
            double sr = Math.Sqrt(r), s2 = Math.Sqrt(2 * M), core = T - 2 * Math.Sqrt(2 * M * r);
            double U = -(sr - s2) * Math.Exp(-(core - r) / (4 * M)) / s2, V = (sr + s2) * Math.Exp((core + r) / (4 * M)) / s2;
            return Page(U, V);
        }
        void DrawPenrose()
        {
            float D = Diagram;
            Vector3 i0 = new Vector3(D, 0, 0), ip = new Vector3(D / 2, D / 2, 0), im = new Vector3(D / 2, -D / 2, 0), o = Vector3.zero;
            // the right exterior, the left one faint, the horizons, the singularities
            hot.Add(i0, ip); hot.Add(i0, im); hot.Add(o, ip); hot.Add(o, im);
            faint.Add(-i0, ip - new Vector3(D, 0, 0)); faint.Add(-i0, im - new Vector3(D, 0, 0)); faint.Add(o, new Vector3(-D / 2, D / 2, 0)); faint.Add(o, new Vector3(-D / 2, -D / 2, 0));
            for (int i = 0; i < 16; i++) { float x0 = -D / 2 + D * i / 16, x1 = x0 + D / 16; hot.Add(new Vector3(x0, D / 2 + .006f * (i % 2 == 0 ? 1 : -1), 0), new Vector3(x1, D / 2 + .006f * (i % 2 == 0 ? -1 : 1), 0)); faint.Add(new Vector3(x0, -D / 2, 0), new Vector3(x1, -D / 2, 0)); }
            // constant r and constant t in our exterior
            foreach (double r in new[] { 2.3, 3, 4, 6, 10 }) { var pts = new List<Vector3>(); for (int i = 0; i <= 60; i++) pts.Add(PageTR(-40 + 80.0 * i / 60, r)); faint.Add(pts); }
            foreach (double t in new[] { -12, -6, 0, 6, 12 }) { var pts = new List<Vector3>(); for (int i = 0; i <= 60; i++) pts.Add(PageTR(t, 2.0001 + 60 * Math.Pow(i / 60.0, 3))); faint.Add(pts); }
            // the infaller's worldline, and where they are now with their light cone
            var line = new List<Vector3>(); double rNow = 8;
            for (int i = 0; i <= 160; i++) { var p = Infaller(-24 + 34.6 * i / 160, out var r); if (r <= 1e-3) break; line.Add(p); }
            hot.Add(line);
            double Tn = -24 + Mathf.Repeat(since * 2.2f, 34.5f);
            var now = Infaller(Tn, out rNow); Dot(2, now, .012f);
            float c = .05f; hot.Add(now, now + new Vector3(c, c, 0)); hot.Add(now, now + new Vector3(-c, c, 0)); faint.Add(now, now + new Vector3(c, -c, 0)); faint.Add(now, now + new Vector3(-c, -c, 0));
            // every label just outside the line or corner it names (the corners' outward, the edges' along their outward
            // normal), so none sits on a line; the horizon's inside the hole's triangle, with a short leader to the diagonal
            float n = .03f;
            Label(0, "i0", i0 + new Vector3(.025f, 0, 0), 1); Label(1, "i+", ip + new Vector3(.02f, .02f, 0), 1); Label(2, "i−", im + new Vector3(.02f, -.02f, 0), 1);
            Label(3, "r = 0  singularity", new Vector3(-.04f, D / 2 + .055f, 0));
            var onHorizon = new Vector3(D * .3f, D * .3f, 0);
            Label(4, "event horizon", onHorizon + new Vector3(-n, n, 0) * 1.4f + new Vector3(-.01f, 0, 0), -1, onHorizon);
            Label(5, "light out →", (i0 + ip) / 2 + new Vector3(n, n, 0), 1); Label(6, "← light in", (i0 + im) / 2 + new Vector3(n, -n, 0), 1);
            Label(7, rNow > 2 ? Localization.Format("falling in, r = {0:0.0}M", rNow) : rNow > .05 ? Localization.Format("inside, r = {0:0.0}M", rNow) : "the end", now + new Vector3(-.045f, -.035f, 0), -1, now);
        }
        void DrawPenroseExtremal()
        {
            float D = Diagram * .8f; Vector3 P(float x, float y) => new Vector3(x * D - D / 2, y * D - D / 2, 0);
            // exterior diamonds and the interior triangles between them, the singularity their left edge
            for (int k = 0; k < 2; k++)
            {
                float y = k; var L = P(0, y); var T = P(.5f, y + .5f); var R = P(1, y); var B = P(.5f, y - .5f);
                var ext = k == 0 ? hot : faint; ext.Add(R, T); ext.Add(R, B); ext.Add(L, T); ext.Add(L, B);
                hot.Add(L, P(0, y + 1));                       // r = 0, time-like
                faint.Add(T, P(0, y + 1));                     // the inner (Cauchy) horizon
            }
            for (int i = 0; i < 6; i++) faint.Add(P(0, 2 + i * .08f), P(0, 2.04f + i * .08f));
            // an infaller through the horizon, past the singularity and out through the inner horizon
            var curve = new List<Vector3>();
            Vector2[] k4 = { new Vector2(.85f, -.3f), new Vector2(.6f, .02f), new Vector2(.4f, .36f), new Vector2(.22f, .64f), new Vector2(.16f, .9f), new Vector2(.3f, 1.12f), new Vector2(.5f, 1.4f) };
            for (int s = 0; s + 1 < k4.Length; s++) for (int i = 0; i < 12; i++)
            {
                float u = i / 12f; var a = k4[Mathf.Max(s - 1, 0)]; var b = k4[s]; var c = k4[s + 1]; var d = k4[Mathf.Min(s + 2, k4.Length - 1)];
                var q = .5f * (2 * b + (-a + c) * u + (2 * a - 5 * b + 4 * c - d) * u * u + (-a + 3 * b - 3 * c + d) * u * u * u); curve.Add(P(q.x, q.y));
            }
            curve.Add(P(k4[k4.Length - 1].x, k4[k4.Length - 1].y));
            hot.Add(curve);
            float at = Mathf.Repeat(since * 9, curve.Count - 1); int ci = (int)at; var now = Vector3.Lerp(curve[ci], curve[Mathf.Min(ci + 1, curve.Count - 1)], at - ci);
            Dot(2, now, .012f); float c2 = .045f; hot.Add(now, now + new Vector3(c2, c2, 0)); hot.Add(now, now + new Vector3(-c2, c2, 0));
            // two columns clear of the diagram: on the left, past the singularity's line, what lies along it (each with a
            // short leader to the line it names); on the right, past the diamonds' corners, the regions
            float left = P(0, 0).x - .035f, rightX = P(1, 0).x + .025f;
            Label(1, "event horizon  r = M", new Vector3(left, P(0, -.05f).y, 0), -1, P(.25f, .25f));
            Label(2, "r = 0  time-like", new Vector3(left, P(0, .5f).y, 0), -1, P(0, .5f));
            Label(3, "inner horizon", new Vector3(left, P(0, .95f).y, 0), -1, P(.3f, .7f));
            Label(5, "and so on…", new Vector3(left, P(0, 2.2f).y, 0), -1, P(0, 2.2f));
            Label(0, "i0", new Vector3(rightX, P(1, 0).y, 0), 1); Label(4, "another exterior", new Vector3(rightX, P(1, 1).y, 0), 1);
            Label(6, "schematic", new Vector3(rightX, P(1, -.5f).y, 0), 1);
        }

        // ================================================================== the Aretakis instability
        // The variables (the pop-up's Variables page): the hole's charge Q/M (1 = extremal), the spherical mode ℓ, where
        // the pulse starts, and how fast v runs. The readout shows the quantities that matter as they change.
        const int N = 240, Rings = 22, Spokes = 36; const double RMax = 12, HaloMax = 5;
        float charge = 1, speed = 1; int ell; string pulse = "horizon";
        double[] Pi, Phi, fr, Vr, k1, k2, k3, tmp, tmpPhi; double dr, rH, kappa, v, conserved; int conservedOrder;
        readonly List<Vector3> trA = new List<Vector3>(), trB = new List<Vector3>(), trC = new List<Vector3>();
        float[] traceMax = { 1e-3f, 1e-3f, 1e-3f };
        const double VMax = 260;

        public void Variable(string v)
        {
            var kv = v.Split(':'); if (kv.Length != 2) return;
            switch (kv[0])
            {
                case "charge": charge = float.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture); break;
                case "mode": ell = int.Parse(kv[1]); break;
                case "pulse": pulse = kv[1]; break;
                case "speed": speed = float.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture); break;
            }
            if (kind != Kind.Aretakis) kind = Kind.Aretakis;
            SetupAretakis(); since = 0; Tell(); MarkVariables();
        }
        // the pop-up's pills: the chosen value of each variable is lit
        void MarkVariables()
        {
            if (vars == null) return;
            for (int i = 0; i < vars.Length; i++)
            {
                if (!vars[i] || i >= varValues.Length) continue;
                var kv = varValues[i].Split(':'); if (kv.Length != 2) continue;
                bool on = kv[0] == "charge" ? Mathf.Abs(float.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture) - charge) < 1e-3f
                        : kv[0] == "mode" ? int.Parse(kv[1]) == ell
                        : kv[0] == "pulse" ? kv[1] == pulse
                        : kv[0] == "speed" && Mathf.Abs(float.Parse(kv[1], System.Globalization.CultureInfo.InvariantCulture) - speed) < 1e-3f;
                MenuTheme.Holo.Mark(vars[i], on, false);
            }
        }
        void SetupAretakis()
        {
            double Q2 = charge * charge * M * M; rH = M + Math.Sqrt(Math.Max(M * M - Q2, 0)); double rMinus = M - Math.Sqrt(Math.Max(M * M - Q2, 0));
            kappa = (rH - rMinus) / (2 * rH * rH);
            Pi = new double[N]; Phi = new double[N]; fr = new double[N]; Vr = new double[N]; k1 = new double[N]; k2 = new double[N]; k3 = new double[N]; tmp = new double[N]; tmpPhi = new double[N];
            dr = (RMax - rH) / (N - 1);
            double r0 = rH + (pulse == "horizon" ? .3 : pulse == "near" ? 1.2 : 3.0), w = pulse == "horizon" ? .35 : .5;
            for (int i = 0; i < N; i++)
            {
                double r = rH + i * dr, f = 1 - 2 * M / r + Q2 / (r * r), fp = 2 * M / (r * r) - 2 * Q2 / (r * r * r);
                fr[i] = f; Vr[i] = f * (fp / r + ell * (ell + 1) / (r * r));
                double g = Math.Exp(-((r - r0) / w) * ((r - r0) / w));
                Phi[i] = g; Pi[i] = -2 * (r - r0) / (w * w) * g;
            }
            Phi[N - 1] = 0; v = 0; trA.Clear(); trB.Clear(); trC.Clear(); traceMax = new[] { 1e-3f, 1e-3f, 1e-3f };
            // the conserved quantity on an extremal horizon for mode ℓ is the (ℓ+1)-th radial derivative
            conservedOrder = ell + 1; conserved = Derivative(conservedOrder);
        }
        double Derivative(int order)
        {
            if (Pi == null) return 0;
            switch (order)
            {
                case 0: return Phi[0];
                case 1: return Pi[0];
                case 2: return (Pi[1] - Pi[0]) / dr;
                default: return (Pi[2] - 2 * Pi[1] + Pi[0]) / (dr * dr);
            }
        }
        // ∂vΠ = −½[∂r(fΠ) − Vφ], with φ rebuilt from Π by integrating in from φ(rMax) = 0.
        void Rhs(double[] P, double[] F, double[] outK)
        {
            for (int i = 0; i < N; i++)
            {
                double d = i == 0 ? (fr[1] * P[1] - fr[0] * P[0]) / dr : i == N - 1 ? (fr[i] * P[i] - fr[i - 1] * P[i - 1]) / dr : (fr[i + 1] * P[i + 1] - fr[i - 1] * P[i - 1]) / (2 * dr);
                outK[i] = -.5 * (d - Vr[i] * F[i]);
            }
        }
        void Rebuild(double[] P, double[] F) { F[N - 1] = 0; for (int i = N - 2; i >= 0; i--) F[i] = F[i + 1] - dr * .5 * (P[i] + P[i + 1]); }
        void AretakisStep()
        {
            double dv = dr;
            Rhs(Pi, Phi, k1);
            for (int i = 0; i < N; i++) tmp[i] = Pi[i] + dv * k1[i]; Rebuild(tmp, tmpPhi); Rhs(tmp, tmpPhi, k2);
            for (int i = 0; i < N; i++) tmp[i] = .75 * Pi[i] + .25 * (tmp[i] + dv * k2[i]); Rebuild(tmp, tmpPhi); Rhs(tmp, tmpPhi, k3);
            for (int i = 0; i < N; i++) Pi[i] = Pi[i] / 3 + 2.0 / 3 * (tmp[i] + dv * k3[i]);
            for (int i = 1; i < N - 1; i++) tmp[i] = Pi[i] + .04 * (Pi[i - 1] - 2 * Pi[i] + Pi[i + 1]);   // a little smoothing keeps it stable
            for (int i = 1; i < N - 1; i++) Pi[i] = tmp[i];
            Rebuild(Pi, Phi); v += dv;
            if (double.IsNaN(Pi[0]) || Math.Abs(Pi[0]) > 1e6) SetupAretakis();
        }
        void DrawAretakis()
        {
            if (Pi == null) SetupAretakis();
            if (v < VMax) for (int n = 0; n < Mathf.RoundToInt(14 * speed); n++) AretakisStep();
            // the three horizon quantities this mode cares about: the field, its slope and its curvature, or one order up for ℓ = 1
            int o = ell > 0 ? 1 : 0;
            float a = (float)Derivative(o), b = (float)Derivative(o + 1), c = (float)Derivative(o + 2);
            if (v < VMax) { trA.Add(new Vector3((float)v, a, 0)); trB.Add(new Vector3((float)v, b, 0)); trC.Add(new Vector3((float)v, c, 0)); }
            traceMax[0] = Mathf.Max(traceMax[0], Mathf.Abs(a)); traceMax[1] = Mathf.Max(traceMax[1], Mathf.Abs(b)); traceMax[2] = Mathf.Max(traceMax[2], Mathf.Abs(c));
            // ---- the hole and its halo: ψ(r) = φ/r now, revolved round the hole, from the horizon out to 5M
            float s = .07f, h = height * 1.1f; var centre = new Vector3(-.18f, 0, 0);
            Vector3 Halo(double r, double th, double psi) => centre + new Vector3((float)(r * s * Math.Cos(th)), (float)(psi * h), (float)(r * s * Math.Sin(th)));
            int baseV = fillVerts.Count;
            for (int j = 0; j <= Rings; j++)
            {
                double r = rH + (HaloMax - rH) * Math.Pow(j / (double)Rings, 1.5); int k = (int)Math.Round((r - rH) / dr); k = Math.Min(Math.Max(k, 0), N - 1);
                double psi = Phi[k] / r;
                for (int i = 0; i <= Spokes; i++) fillVerts.Add(Halo(r, i * Math.PI * 2 / Spokes, psi));
            }
            for (int j = 0; j < Rings; j++) for (int i = 0; i < Spokes; i++)
            {
                int p = baseV + j * (Spokes + 1) + i, q = p + 1, u = p + Spokes + 1, w = u + 1;
                fillTris.Add(p); fillTris.Add(u); fillTris.Add(q); fillTris.Add(q); fillTris.Add(u); fillTris.Add(w);
            }
            for (int j = 0; j <= Rings; j += 2) for (int i = 0; i < Spokes; i++) surfaceLines.Add(fillVerts[baseV + j * (Spokes + 1) + i], fillVerts[baseV + j * (Spokes + 1) + i + 1]);
            for (int i = 0; i < Spokes; i += 3) for (int j = 0; j < Rings; j++) surfaceLines.Add(fillVerts[baseV + j * (Spokes + 1) + i], fillVerts[baseV + (j + 1) * (Spokes + 1) + i]);
            // the horizon ring, bright, and the hole as a wire sphere just inside it
            for (int i = 0; i < Spokes; i++) hot.Add(fillVerts[baseV + i], fillVerts[baseV + i + 1]);
            float hr = (float)(rH * s) * .97f;
            foreach (var q in new[] { Quaternion.identity, Quaternion.Euler(90, 0, 0), Quaternion.Euler(90, 90, 0) })
                for (int i = 0; i < 48; i++) { float t0 = i * Mathf.PI * 2 / 48, t1 = (i + 1) * Mathf.PI * 2 / 48; faint.Add(centre + q * new Vector3(Mathf.Cos(t0) * hr, 0, Mathf.Sin(t0) * hr), centre + q * new Vector3(Mathf.Cos(t1) * hr, 0, Mathf.Sin(t1) * hr)); }
            // the slope at the horizon, drawn as a needle on the near side: this is what an extremal horizon keeps
            var foot = Halo(rH, Math.PI / 2, Phi[0] / rH); double slope = (Pi[0] - Phi[0] / rH) / rH;   // dψ/dr at the horizon
            var tip = foot + new Vector3(0, (float)(slope * .4 * h * s), 0) + new Vector3(0, 0, .4f * s);
            hot.Add(foot, tip); hot.Add(tip, tip + new Vector3(0, .012f, -.008f)); hot.Add(tip, tip + new Vector3(0, -.012f, -.008f));
            // ---- the traces, each in its own box at the right
            float bx = .18f, bw = .3f, bh = .06f;
            string[] names = ell > 0
                ? new[] { "∂rφ on the horizon", charge >= .999f ? "∂r²φ: stays (H₁, the Aretakis constant for ℓ = 1)" : "∂r²φ: fades", charge >= .999f ? "∂r³φ: grows" : "∂r³φ: fades" }
                : new[] { "φ on the horizon: fades", charge >= .999f ? "∂rφ: stays at H₀, the Aretakis constant" : "∂rφ: fades at rate κ", charge >= .999f ? "∂r²φ: grows ∝ v" : "∂r²φ: fades" };
            var traces = new[] { trA, trB, trC };
            for (int k = 0; k < 3; k++)
            {
                float by = .17f - k * (bh * 2 + .07f);   // room between the boxes for the next one's name
                faint.Add(new Vector3(bx, by, 0), new Vector3(bx + bw, by, 0)); faint.Add(new Vector3(bx, by - bh, 0), new Vector3(bx, by + bh, 0));
                var tr = traces[k]; Vector3 last = Vector3.zero;
                for (int i = 0; i < tr.Count; i += 2)
                {
                    var p = new Vector3(bx + bw * tr[i].x / (float)VMax, by + Mathf.Clamp(tr[i].y / Mathf.Max(traceMax[k], 1e-3f), -1, 1) * bh, 0);
                    if (i > 0) hot.Add(last, p); last = p;
                }
                // each trace's name above its box, starting at the box's left edge (so a long name runs right, away
                // from the halo), clear of the trace's top
                Label(k, names[k], new Vector3(bx, by + bh + .036f, 0), 1);
            }
            // the halo's labels off the disc: the hole's below it, with a leader up to the horizon on the near side; the
            // clock under that; the needle's above the halo's highest swell, with a leader down to the needle's tip
            float under = centre.y - .11f;
            Label(3, charge >= .999f ? "extremal hole  r = M" : Localization.Format("hole  r₊ = {0:0.00}M", rH), new Vector3(centre.x, under, 0), 0, centre + new Vector3(0, 0, -(float)(rH * s)));
            Label(5, v >= VMax ? Localization.Format("v = {0:0}M  (done: Replay)", v) : $"v = {v:0}M", new Vector3(centre.x, under - .065f, 0));
            Label(4, "the slope at the horizon", new Vector3(centre.x, centre.y + h + .04f, 0), 0, tip);
            // ---- the readout on the pop-up: the variables and the quantities that matter, live
            if (readout)
            {
                string ext = charge >= .999f ? Localization.Tr("extremal: κ = 0, so the horizon keeps the slope") : Localization.Format("sub-extremal: κ = {0:0.000}/M, so the slope fades like e^(−κv)", kappa);
                readout.text = $"Q/M = {charge:0.00}    r₊ = {rH:0.000} M    r₋ = {M - Math.Sqrt(Math.Max(M * M - charge * charge * M * M, 0)):0.000} M    ℓ = {ell}    v = {v:0.0} M\n" +
                    $"{ext}\n" +
                    Localization.Format("on the horizon now:   φ = {0:0.000}    ∂rφ = {1:0.000}    ∂r²φ = {2:0.00}    ∂r³φ = {3:0.0}", Derivative(0), Derivative(1), Derivative(2), Derivative(3)) + "\n" +
                    Localization.Format("conserved charge H{0} = ∂r^{1}φ|H at the start: {2:0.000}   now: {3:0.000}", ell, conservedOrder, conserved, Derivative(conservedOrder)) + Localization.Tr(charge >= .999f ? "   (the same: that is the instability)" : "   (decaying)");
            }
        }
    }
}
