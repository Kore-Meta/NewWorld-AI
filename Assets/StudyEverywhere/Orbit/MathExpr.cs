using System;
using System.Collections.Generic;
using System.Globalization;

namespace StudyEverywhere
{
    // A small expression language for the holodeck's math panel: numbers, x, y, t, pi, e, + - * / ^, brackets, unary
    // minus, and sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow. Parsed once into
    // a flat program (reverse Polish) and evaluated on a small stack with no allocation, so a 48x48 grid every frame is
    // cheap. Parse errors say where and what, so the panel can show them.
    public sealed class MathExpr
    {
        enum Op { Num, X, Y, T, Add, Sub, Mul, Div, Pow, Neg, Fn }
        enum Fn { Sin, Cos, Tan, Asin, Acos, Atan, Sinh, Cosh, Tanh, Exp, Ln, Log, Sqrt, Abs, Floor, Sign, Min, Max, Pow, Atan2 }
        struct Code { public Op op; public double num; public Fn fn; }

        readonly Code[] program;
        readonly double[] stack;
        public readonly string Source;
        public bool UsesY { get; private set; }
        public bool UsesT { get; private set; }

        static readonly Dictionary<string, (Fn fn, int args)> functions = new Dictionary<string, (Fn, int)>
        {
            ["sin"] = (Fn.Sin, 1), ["cos"] = (Fn.Cos, 1), ["tan"] = (Fn.Tan, 1), ["asin"] = (Fn.Asin, 1), ["acos"] = (Fn.Acos, 1), ["atan"] = (Fn.Atan, 1),
            ["sinh"] = (Fn.Sinh, 1), ["cosh"] = (Fn.Cosh, 1), ["tanh"] = (Fn.Tanh, 1), ["exp"] = (Fn.Exp, 1), ["ln"] = (Fn.Ln, 1), ["log"] = (Fn.Log, 1),
            ["sqrt"] = (Fn.Sqrt, 1), ["abs"] = (Fn.Abs, 1), ["floor"] = (Fn.Floor, 1), ["sign"] = (Fn.Sign, 1),
            ["min"] = (Fn.Min, 2), ["max"] = (Fn.Max, 2), ["pow"] = (Fn.Pow, 2), ["atan2"] = (Fn.Atan2, 2),
        };

        MathExpr(string source, List<Code> code, bool usesY, bool usesT)
        {
            Source = source; program = code.ToArray(); stack = new double[program.Length + 2]; UsesY = usesY; UsesT = usesT;
        }

        // Parses, or returns null with the error in `error`.
        public static MathExpr Parse(string text, out string error)
        {
            error = null;
            try { var p = new Parser(text); var code = p.Run(); return new MathExpr(text, code, p.usesY, p.usesT); }
            catch (ParseError e) { error = e.Message; return null; }
        }

        public double Eval(double x, double y, double t)
        {
            int sp = 0;
            for (int i = 0; i < program.Length; i++)
            {
                var c = program[i];
                switch (c.op)
                {
                    case Op.Num: stack[sp++] = c.num; break;
                    case Op.X: stack[sp++] = x; break;
                    case Op.Y: stack[sp++] = y; break;
                    case Op.T: stack[sp++] = t; break;
                    case Op.Add: sp--; stack[sp - 1] += stack[sp]; break;
                    case Op.Sub: sp--; stack[sp - 1] -= stack[sp]; break;
                    case Op.Mul: sp--; stack[sp - 1] *= stack[sp]; break;
                    case Op.Div: sp--; stack[sp - 1] /= stack[sp]; break;
                    case Op.Pow: sp--; stack[sp - 1] = Math.Pow(stack[sp - 1], stack[sp]); break;
                    case Op.Neg: stack[sp - 1] = -stack[sp - 1]; break;
                    case Op.Fn:
                        switch (c.fn)
                        {
                            case Fn.Sin: stack[sp - 1] = Math.Sin(stack[sp - 1]); break;
                            case Fn.Cos: stack[sp - 1] = Math.Cos(stack[sp - 1]); break;
                            case Fn.Tan: stack[sp - 1] = Math.Tan(stack[sp - 1]); break;
                            case Fn.Asin: stack[sp - 1] = Math.Asin(stack[sp - 1]); break;
                            case Fn.Acos: stack[sp - 1] = Math.Acos(stack[sp - 1]); break;
                            case Fn.Atan: stack[sp - 1] = Math.Atan(stack[sp - 1]); break;
                            case Fn.Sinh: stack[sp - 1] = Math.Sinh(stack[sp - 1]); break;
                            case Fn.Cosh: stack[sp - 1] = Math.Cosh(stack[sp - 1]); break;
                            case Fn.Tanh: stack[sp - 1] = Math.Tanh(stack[sp - 1]); break;
                            case Fn.Exp: stack[sp - 1] = Math.Exp(stack[sp - 1]); break;
                            case Fn.Ln: stack[sp - 1] = Math.Log(stack[sp - 1]); break;
                            case Fn.Log: stack[sp - 1] = Math.Log10(stack[sp - 1]); break;
                            case Fn.Sqrt: stack[sp - 1] = Math.Sqrt(stack[sp - 1]); break;
                            case Fn.Abs: stack[sp - 1] = Math.Abs(stack[sp - 1]); break;
                            case Fn.Floor: stack[sp - 1] = Math.Floor(stack[sp - 1]); break;
                            case Fn.Sign: stack[sp - 1] = Math.Sign(stack[sp - 1]); break;
                            case Fn.Min: sp--; stack[sp - 1] = Math.Min(stack[sp - 1], stack[sp]); break;
                            case Fn.Max: sp--; stack[sp - 1] = Math.Max(stack[sp - 1], stack[sp]); break;
                            case Fn.Pow: sp--; stack[sp - 1] = Math.Pow(stack[sp - 1], stack[sp]); break;
                            case Fn.Atan2: sp--; stack[sp - 1] = Math.Atan2(stack[sp - 1], stack[sp]); break;
                        }
                        break;
                }
            }
            return sp > 0 ? stack[0] : 0;
        }

        sealed class ParseError : Exception { public ParseError(string m) : base(m) { } }

        // Recursive descent with precedence: sum > product > unary minus > power (right-assoc) > atom; implicit multiplication
        // between an atom and what follows it ("2x", "3sin(x)", "(x+1)(x-1)").
        sealed class Parser
        {
            readonly string s; int i; readonly List<Code> code = new List<Code>();
            public bool usesY, usesT;
            public Parser(string text) { s = (text ?? "").Replace("−", "-").Replace("×", "*").Replace("÷", "/").Replace("π", "pi").Replace("²", "^2").Replace("³", "^3"); }
            public List<Code> Run()
            {
                Sum(); Skip();
                if (i < s.Length) throw new ParseError(Localization.Format("Unexpected '{0}' at {1}", s[i], i + 1));
                if (code.Count == 0) throw new ParseError(Localization.Tr("Type an expression"));
                return code;
            }
            void Skip() { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }
            bool Take(char c) { Skip(); if (i < s.Length && s[i] == c) { i++; return true; } return false; }
            void Sum()
            {
                Product();
                while (true)
                {
                    if (Take('+')) { Product(); code.Add(new Code { op = Op.Add }); }
                    else if (Take('-')) { Product(); code.Add(new Code { op = Op.Sub }); }
                    else return;
                }
            }
            void Product()
            {
                Unary();
                while (true)
                {
                    if (Take('*')) { Unary(); code.Add(new Code { op = Op.Mul }); }
                    else if (Take('/')) { Unary(); code.Add(new Code { op = Op.Div }); }
                    else { Skip(); if (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '(')) { Unary(); code.Add(new Code { op = Op.Mul }); } else return; }   // implicit product
                }
            }
            void Unary()
            {
                if (Take('-')) { Unary(); code.Add(new Code { op = Op.Neg }); return; }
                Take('+');
                Power();
            }
            void Power()
            {
                Atom();
                if (Take('^')) { Unary(); code.Add(new Code { op = Op.Pow }); }
            }
            void Atom()
            {
                Skip();
                if (i >= s.Length) throw new ParseError(Localization.Tr("Something is missing at the end"));
                char c = s[i];
                if (c == '(')
                {
                    i++; Sum();
                    if (!Take(')')) throw new ParseError(Localization.Tr("Missing ')'"));
                    return;
                }
                if (char.IsDigit(c) || c == '.')
                {
                    int start = i; while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
                    if (!double.TryParse(s.Substring(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) throw new ParseError(Localization.Format("Bad number at {0}", start + 1));
                    code.Add(new Code { op = Op.Num, num = v }); return;
                }
                if (char.IsLetter(c))
                {
                    int start = i; while (i < s.Length && char.IsLetter(s[i])) i++;
                    string name = s.Substring(start, i - start).ToLowerInvariant();
                    if (functions.TryGetValue(name, out var f))
                    {
                        if (!Take('(')) throw new ParseError(Localization.Format("{0} needs brackets: {0}(…)", name));
                        Sum();
                        for (int k = 1; k < f.args; k++) { if (!Take(',')) throw new ParseError(Localization.Format("{0} takes {1} values", name, f.args)); Sum(); }
                        if (!Take(')')) throw new ParseError(Localization.Tr("Missing ')'"));
                        code.Add(new Code { op = Op.Fn, fn = f.fn }); return;
                    }
                    switch (name)
                    {
                        case "x": code.Add(new Code { op = Op.X }); return;
                        case "y": usesY = true; code.Add(new Code { op = Op.Y }); return;
                        case "t": usesT = true; code.Add(new Code { op = Op.T }); return;
                        case "pi": code.Add(new Code { op = Op.Num, num = Math.PI }); return;
                        case "e": code.Add(new Code { op = Op.Num, num = Math.E }); return;
                    }
                    // letters run together ("xy", "tx"): split off one variable at a time
                    if (name.Length > 1 && "xyt".IndexOf(name[0]) >= 0) { i = start + 1; Atom(); return; }
                    throw new ParseError(Localization.Format("I don't know '{0}'", name));
                }
                throw new ParseError(Localization.Format("Unexpected '{0}' at {1}", c, i + 1));
            }
        }
    }
}
