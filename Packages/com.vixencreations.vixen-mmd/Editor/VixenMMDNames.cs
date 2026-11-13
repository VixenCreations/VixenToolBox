using System;
using System.Collections.Generic;
using System.Text;

namespace VixenTools.MMD.Editor
{
    internal enum MmdCategory { Mouth, Eyes, Brows, Other }

    [Flags]
    internal enum MmdFamily { None = 0, Standard = 1, Extended = 2, InochiPM = 4 }

    internal readonly struct MmdName
    {
        public readonly string Name;
        public readonly string English;
        public readonly MmdCategory Category;
        public readonly MmdFamily Families;
        public readonly bool DanceCore;

        public MmdName(string name, string english, MmdCategory category, MmdFamily families, bool danceCore)
        {
            Name = name;
            English = english;
            Category = category;
            Families = families;
            DanceCore = danceCore;
        }
    }

    internal static partial class VixenMMDNames
    {
        private static Dictionary<string, int> _index;
        private static Dictionary<string, string> _folded;
        private static Dictionary<string, string> _loose;

        internal static bool TryGet(string name, out MmdName entry)
        {
            Build();
            if (name != null && _index.TryGetValue(name, out int i))
            {
                entry = All[i];
                return true;
            }
            entry = default;
            return false;
        }

        internal static bool IsKnown(string name)
        {
            Build();
            return name != null && _index.ContainsKey(name);
        }

        internal static string FindNearName(string name)
        {
            Build();
            if (string.IsNullOrEmpty(name) || _index.ContainsKey(name)) return null;
            if (_folded.TryGetValue(Fold(name), out string hit)) return hit;
            return _loose.TryGetValue(Loose(name), out hit) && hit != null ? hit : null;
        }

        private static string Loose(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in Fold(s))
            {
                if (c != '!' && c != '?' && !char.IsWhiteSpace(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        internal static string Fold(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (char c in s.Normalize(NormalizationForm.FormKC))
            {
                sb.Append(c == '^' ? '∧' : c);
            }
            return sb.ToString();
        }

        private static void Build()
        {
            if (_index != null) return;
            _index = new Dictionary<string, int>(StringComparer.Ordinal);
            _folded = new Dictionary<string, string>(StringComparer.Ordinal);
            _loose = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < All.Length; i++)
            {
                _index[All[i].Name] = i;
                string f = Fold(All[i].Name);
                if (!_folded.ContainsKey(f)) _folded[f] = All[i].Name;
                string l = Loose(All[i].Name);
                if (l.Length == 0) continue;
                if (_loose.TryGetValue(l, out string prior)) { if (prior != All[i].Name) _loose[l] = null; }
                else _loose[l] = All[i].Name;
            }
        }

        internal static string CategoryLabel(MmdCategory c)
        {
            switch (c)
            {
                case MmdCategory.Mouth: return VixenMMDText.L("Mouth");
                case MmdCategory.Eyes: return VixenMMDText.L("Eyes");
                case MmdCategory.Brows: return VixenMMDText.L("Brows");
                default: return VixenMMDText.L("Other");
            }
        }
    }
}
