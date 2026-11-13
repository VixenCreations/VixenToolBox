using System.Collections.Generic;

namespace VixenTools.MMD.Editor
{
    internal enum RecipeKind { Tuned, Alias, Proposal }

    internal enum RecipeTermKind { Keys, Viseme, Slot }

    internal sealed class RecipeTerm
    {
        public readonly RecipeTermKind Kind;
        public readonly string[] FaceKeys;
        public readonly string Name;
        public readonly float Scale;

        private RecipeTerm(RecipeTermKind kind, string[] keys, string name, float scale)
        {
            Kind = kind;
            FaceKeys = keys;
            Name = name;
            Scale = scale;
        }

        public static RecipeTerm Keys(float scale, params string[] keys) => new RecipeTerm(RecipeTermKind.Keys, keys, null, scale);
        public static RecipeTerm Viseme(string viseme, float scale) => new RecipeTerm(RecipeTermKind.Viseme, null, viseme, scale);
        public static RecipeTerm Slot(string slot, float scale) => new RecipeTerm(RecipeTermKind.Slot, null, slot, scale);
    }

    internal sealed class Recipe
    {
        public readonly string Name;
        public readonly RecipeKind Kind;
        public readonly string AliasOf;
        public readonly RecipeTerm[] Terms;

        public Recipe(string name, RecipeKind kind, string aliasOf, params RecipeTerm[] terms)
        {
            Name = name;
            Kind = kind;
            AliasOf = aliasOf;
            Terms = terms ?? new RecipeTerm[0];
        }
    }

    internal static partial class VixenMMDRecipes
    {
        private static Dictionary<string, Recipe> _byName;

        internal static Recipe Find(string name)
        {
            if (_byName == null)
            {
                _byName = new Dictionary<string, Recipe>();
                foreach (var r in All) _byName[r.Name] = r;
            }
            return name != null && _byName.TryGetValue(name, out var hit) ? hit : null;
        }

        internal static Recipe Resolve(Recipe r)
        {
            int guard = 0;
            while (r != null && r.Kind == RecipeKind.Alias && guard++ < 8) r = Find(r.AliasOf);
            return r;
        }

        internal static IEnumerable<string> SlotNames()
        {
            var seen = new HashSet<string>();
            foreach (var r in All)
            {
                foreach (var t in r.Terms)
                {
                    if (t.Kind == RecipeTermKind.Slot && seen.Add(t.Name)) yield return t.Name;
                }
            }
        }

        internal static string SlotLabel(string slot)
        {
            switch (slot)
            {
                case "smile": return VixenMMDText.L("Smile expression");
                case "blush": return VixenMMDText.L("Blush");
                case "beg": return VixenMMDText.L("Pleading or worried face");
                case "angry": return VixenMMDText.L("Angry expression");
                case "happy": return VixenMMDText.L("Happy expression");
                case "sad": return VixenMMDText.L("Sad expression");
                case "surprised": return VixenMMDText.L("Surprised expression");
                case "furrow": return VixenMMDText.L("Furrowed brows");
                case "squint": return VixenMMDText.L("Squint expression");
                case "hideEyeShine": return VixenMMDText.L("Hide eye shine");
                case "hideLens": return VixenMMDText.L("Hide eye lens");
                default: return slot;
            }
        }

        internal static bool IsFaceWide(string slot)
        {
            switch (slot)
            {
                case "smile":
                case "happy":
                case "sad":
                case "angry":
                case "surprised":
                case "beg":
                    return true;
                default:
                    return false;
            }
        }

        internal static IEnumerable<string> VisemeNames()
        {
            var seen = new HashSet<string>();
            foreach (var r in All)
            {
                foreach (var t in r.Terms)
                {
                    if (t.Kind == RecipeTermKind.Viseme && seen.Add(t.Name)) yield return t.Name;
                }
            }
        }

        internal static string[] SlotHints(string slot)
        {
            switch (slot)
            {
                case "smile": return new[] { "smile", "joy", "happy" };
                case "blush": return new[] { "blush", "cheek", "照れ", "赤面", "頬" };
                case "beg": return new[] { "beg", "worried", "plead", "puppy" };
                case "angry": return new[] { "angry", "anger", "mad" };
                case "happy": return new[] { "happy", "joy", "smile" };
                case "sad": return new[] { "sad", "cry" };
                case "surprised": return new[] { "surprised", "surprise", "shock" };
                case "furrow": return new[] { "furrow", "frown" };
                case "squint": return new[] { "squint" };
                case "hideEyeShine": return new[] { "hideeyeshine", "eyeshine", "highlight", "ハイライト" };
                case "hideLens": return new[] { "hidelens", "lens" };
                default: return new[] { slot };
            }
        }
    }
}
