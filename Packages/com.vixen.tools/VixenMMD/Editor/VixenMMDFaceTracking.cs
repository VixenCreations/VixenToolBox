using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace VixenTools.MMD.Editor
{
    internal sealed class FaceShapeResolver
    {
        internal static readonly string[] VisemeOrder = { "sil", "PP", "FF", "TH", "DD", "kk", "CH", "SS", "nn", "RR", "aa", "E", "ih", "oh", "ou" };

        private static readonly Dictionary<string, string> ArkitBase = new Dictionary<string, string>
        {
            { "EyeClosed", "eyeBlink" }, { "EyeWide", "eyeWide" }, { "EyeSquint", "eyeSquint" },
            { "EyeLookUp", "eyeLookUp" }, { "EyeLookDown", "eyeLookDown" }, { "EyeLookIn", "eyeLookIn" }, { "EyeLookOut", "eyeLookOut" },
            { "BrowLowerer", "browDown" }, { "BrowPinch", "browDown" }, { "BrowInnerUp", "browInnerUp" }, { "BrowOuterUp", "browOuterUp" },
            { "CheekSquint", "cheekSquint" }, { "CheekPuff", "cheekPuff" }, { "NoseSneer", "noseSneer" },
            { "JawOpen", "jawOpen" }, { "JawForward", "jawForward" }, { "JawLeft", "jawLeft" }, { "JawRight", "jawRight" },
            { "MouthClosed", "mouthClose" }, { "MouthCornerPull", "mouthSmile" }, { "MouthCornerSlant", "mouthSmile" },
            { "MouthFrown", "mouthFrown" }, { "MouthStretch", "mouthStretch" }, { "MouthDimple", "mouthDimple" },
            { "MouthPress", "mouthPress" }, { "MouthLowerDown", "mouthLowerDown" }, { "MouthUpperUp", "mouthUpperUp" },
            { "LipFunnel", "mouthFunnel" }, { "LipPucker", "mouthPucker" }, { "LipSuckUpper", "mouthRollUpper" }, { "LipSuckLower", "mouthRollLower" },
            { "MouthRaiserUpper", "mouthShrugUpper" }, { "MouthRaiserLower", "mouthShrugLower" }, { "TongueOut", "tongueOut" },
        };

        private static readonly Dictionary<string, string[]> Synonyms = new Dictionary<string, string[]>
        {
            { "BrowLowerer", new[] { "BrowDown" } }, { "BrowPinch", new[] { "BrowDown" } },
            { "BrowInnerUp", new[] { "BrowUp" } }, { "BrowOuterUp", new[] { "BrowUp" } },
            { "MouthCornerPull", new[] { "MouthSmile" } }, { "MouthCornerSlant", new[] { "MouthSmile" } },
            { "MouthFrown", new[] { "MouthSad" } }, { "MouthUpper", new[] { "Mouth" } }, { "MouthLower", new[] { "Mouth" } },
        };

        private static readonly string[] CornerUpStyle = { "MouthCornerAngleUp", "MouthHappy" };

        private static readonly Dictionary<string, string[]> JapaneseStyle = new Dictionary<string, string[]>
        {
            { "BrowInnerUp", new[] { "BrowSad", "BrowTrouble", "BrowHeadUp" } }, { "BrowOuterUp", new[] { "BrowTailUp" } },
            { "EyeClosed", new[] { "EyeClose", "Blink" } }, { "EyeWide", new[] { "EyelidUp" } }, { "EyeSquint", new[] { "EyeUnderUp" } },
            { "EyeLookIn", new[] { "EyeLookNear" } }, { "EyeConstrict", new[] { "PupilSmall" } }, { "EyeDilation", new[] { "PupilBig" } },
            { "JawOpen", new[] { "MouthA" } }, { "LipFunnel", new[] { "MouthO" } }, { "LipPucker", new[] { "MouthU" } }, { "MouthPress", new[] { "MouthM" } },
            { "MouthCornerPull", CornerUpStyle }, { "MouthCornerSlant", CornerUpStyle }, { "MouthFrown", new[] { "MouthCornerAngleDown" } },
            { "MouthUpperUp", new[] { "MouthCenterUp", "MouthTopUp" } }, { "MouthLowerDown", new[] { "MouthCenterDown", "MouthUnderDown" } },
            { "MouthStretch", new[] { "MouthWide" } }, { "MouthTightener", new[] { "MouthNarrow" } }, { "MouthDimple", new[] { "MouthCornerBack" } },
            { "TongueOut", new[] { "MouthPero", "TonguePero" } }, { "TongueUp", new[] { "TonguePositionUp" } }, { "TongueDown", new[] { "TonguePositionDown" } },
        };

        private static readonly Dictionary<string, string> JapaneseVowels = new Dictionary<string, string>
        {
            { "aa", "a" }, { "ih", "i" }, { "ou", "u" }, { "E", "e" }, { "oh", "o" },
        };

        private readonly SkinnedMeshRenderer _face;
        private readonly string[] _names;
        private readonly HashSet<string> _exact;
        private readonly Dictionary<string, string> _lower = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _squashed = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _unnumbered = new Dictionary<string, string>();
        private readonly Dictionary<string, (string shape, float mult)> _native = new Dictionary<string, (string, float)>();
        private readonly string[] _visemes = new string[VisemeOrder.Length];

        private static bool _sdkSearched;
        private static MethodInfo _sdkFind;
        private static Type _keyType;

        public bool UsesNativeFaceTracking { get; private set; }
        public int NativeKeyCount => _native.Count;
        public int VisemeCount => _visemes.Count(v => !string.IsNullOrEmpty(v));
        public bool HasSdkMatcher => _sdkFind != null;
        public IReadOnlyList<string> ShapeNames => _names;

        public FaceShapeResolver(VRCAvatarDescriptor descriptor, SkinnedMeshRenderer face)
        {
            _face = face;
            var mesh = face != null ? face.sharedMesh : null;
            int count = mesh != null ? mesh.blendShapeCount : 0;
            _names = new string[count];
            for (int i = 0; i < count; i++) _names[i] = mesh.GetBlendShapeName(i);
            _exact = new HashSet<string>(_names, StringComparer.Ordinal);
            foreach (var n in _names)
            {
                string k = n.ToLowerInvariant();
                if (!_lower.ContainsKey(k)) _lower[k] = n;
                if (IsDivider(n)) continue;
                string squashed = Squash(n, false);
                if (!_squashed.ContainsKey(squashed)) _squashed[squashed] = n;
                string unnumbered = Squash(n, true);
                if (!_unnumbered.ContainsKey(unnumbered)) _unnumbered[unnumbered] = n;
            }
            FindSdkMatcher();
            ReadDescriptor(descriptor);
        }

        public bool Has(string shape) => shape != null && _exact.Contains(shape);

        private Dictionary<string, string> _nearByMmd;

        public string NearShape(string mmdName)
        {
            if (_nearByMmd == null)
            {
                _nearByMmd = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var n in _names)
                {
                    string v = VixenMMDNames.FindNearName(n);
                    if (v != null && !_exact.Contains(v) && !_nearByMmd.ContainsKey(v)) _nearByMmd[v] = n;
                }
            }
            if (mmdName == null) return null;
            if (_nearByMmd.TryGetValue(mmdName, out string hit)) return hit;
            var recipe = VixenMMDRecipes.Find(mmdName);
            var source = recipe != null && recipe.Kind == RecipeKind.Alias ? VixenMMDRecipes.Resolve(recipe) : null;
            return source != null && _exact.Contains(source.Name) ? source.Name : null;
        }

        public IEnumerable<string> UsableShapeNames => _names.Where(n => !IsDivider(n));

        public static bool IsDivider(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            int run = 0;
            foreach (char c in name)
            {
                bool mark = IsDividerMark(c) || (c == '_' && run > 0);
                run = mark ? run + 1 : 0;
                if (run >= 3) return true;
            }
            string t = name.Trim();
            return t.Length >= 4 && IsDividerMark(t[0]) && IsDividerMark(t[1]) && IsDividerMark(t[t.Length - 1]) && IsDividerMark(t[t.Length - 2]);
        }

        private static bool IsDividerMark(char c) => c == '-' || c == '=' || c == '#' || c == '*' || c == '~';

        public bool TryViseme(string viseme, out string shape)
        {
            shape = null;
            int i = Array.IndexOf(VisemeOrder, viseme);
            if (i < 0) return false;
            if (!string.IsNullOrEmpty(_visemes[i]) && _exact.Contains(_visemes[i]))
            {
                shape = _visemes[i];
                return true;
            }
            string v = viseme.ToLowerInvariant();
            foreach (var cand in new[] { "vrc.v_" + v, "v_" + v, "vrc.v_" + v + v, v, v + v })
            {
                if (_lower.TryGetValue(cand, out shape)) return true;
            }
            foreach (var cand in new[] { "vrc.v_" + v, "v_" + v, "viseme_" + v, "vrc.v_" + v + v })
            {
                if (FindFramed(cand, out shape)) return true;
            }
            if (JapaneseVowels.TryGetValue(viseme, out string vowel) && FindFramed("mouth_" + vowel, out shape)) return true;
            shape = null;
            return false;
        }

        public bool TryKey(string key, out string shape, out float multiplier, out bool combined)
        {
            if (TryKeyExact(key, out shape, out multiplier, out combined)) return true;
            if (string.IsNullOrEmpty(key)) return false;
            var framed = Candidates(key, true).ToList();
            foreach (bool sidedOnly in new[] { true, false })
            {
                foreach (var (cand, isCombined) in framed)
                {
                    if (!FindFramed(cand, out shape)) continue;
                    bool both = isCombined || IsCombinedFor(key, shape);
                    if (sidedOnly && both) continue;
                    combined = both;
                    return true;
                }
            }
            shape = null;
            return false;
        }

        private bool TryKeyExact(string key, out string shape, out float multiplier, out bool combined)
        {
            multiplier = 1f;
            combined = false;
            shape = null;
            if (string.IsNullOrEmpty(key)) return false;
            if (_native.TryGetValue(key, out var hit))
            {
                shape = hit.shape;
                multiplier = hit.mult;
                combined = IsCombinedFor(key, shape);
                return true;
            }
            if (_sdkFind != null && _keyType != null && _names.Length > 0)
            {
                try
                {
                    object keyValue = Enum.Parse(_keyType, key);
                    int idx = (int)_sdkFind.Invoke(null, new object[] { _names, keyValue, "" });
                    if (idx >= 0 && idx < _names.Length)
                    {
                        shape = _names[idx];
                        combined = IsCombinedFor(key, shape);
                        return true;
                    }
                }
                catch (Exception)
                {
                }
            }
            foreach (var (cand, isCombined) in Candidates(key, false))
            {
                if (_lower.TryGetValue(cand.ToLowerInvariant(), out shape))
                {
                    combined = isCombined || IsCombinedFor(key, shape);
                    return true;
                }
            }
            return false;
        }

        public bool TryCombined(IEnumerable<string> keys, out string shape)
        {
            shape = null;
            var bases = new HashSet<string>();
            bool left = false, right = false;
            foreach (var k in keys)
            {
                if (!HasSide(k, out string b, out string side)) return false;
                if (side == "Left") left = true;
                else right = true;
                bases.Add(b);
            }
            if (!left || !right) return false;
            var stripped = new HashSet<string>(bases.Select(b => b.EndsWith("Upper") || b.EndsWith("Lower") ? b.Substring(0, b.Length - 5) : b));
            var names = new List<string>();
            if (bases.Count == 1) names.Add(bases.First());
            if (stripped.Count == 1) names.Add(stripped.First());
            int ownNames = names.Count;
            var syn = stripped.Select(b => Synonyms.TryGetValue(b, out var s) ? s[0] : null).Distinct().ToList();
            if (syn.Count == 1 && syn[0] != null) names.Add(syn[0]);
            var ark = stripped.Select(b => ArkitBase.TryGetValue(b, out var a) ? a : null).Distinct().ToList();
            if (ark.Count == 1 && ark[0] != null) names.Add(ark[0]);
            foreach (var n in names)
            {
                if (n == "Mouth") continue;
                if (_lower.TryGetValue(n.ToLowerInvariant(), out shape)) return true;
            }
            shape = null;
            if (keys.Any(k => TryKeyExact(k, out _, out _, out _))) return false;
            var style = stripped.Select(b => JapaneseStyle.TryGetValue(b, out var j) ? j : null).Distinct().ToList();
            if (style.Count == 1 && style[0] != null) names.InsertRange(ownNames, style[0]);
            foreach (var n in names)
            {
                if (n != "Mouth" && FindFramed(n, out shape)) return true;
            }
            shape = null;
            return false;
        }

        internal bool FindFramed(string name, out string shape) => _squashed.TryGetValue(Squash(name, false), out shape) || _unnumbered.TryGetValue(Squash(name, true), out shape);

        internal static string Squash(string name, bool dropNumbers)
        {
            string text = name;
            try
            {
                text = name.Normalize(NormalizationForm.FormKC);
            }
            catch (ArgumentException)
            {
            }
            var whole = new StringBuilder(text.Length);
            var word = new StringBuilder();
            foreach (char c in text)
            {
                if (IsNameSeparator(c)) AppendWord(whole, word, dropNumbers);
                else word.Append(char.ToLowerInvariant(c));
            }
            AppendWord(whole, word, dropNumbers);
            return whole.ToString();
        }

        private static bool IsNameSeparator(char c) => c == ' ' || c == '_' || c == '-' || c == '.' || c == (char)0x30FB;

        private static void AppendWord(StringBuilder whole, StringBuilder word, bool dropNumbers)
        {
            bool number = word.Length > 0;
            for (int i = 0; number && i < word.Length; i++) number = char.IsDigit(word[i]);
            if (!dropNumbers || !number) whole.Append(word);
            word.Clear();
        }

        private static bool IsCombinedFor(string key, string shape)
        {
            if (!HasSide(key, out _, out _)) return false;
            string s = shape.ToLowerInvariant();
            if (s.Contains("left") || s.Contains("right") || s.Contains("_l_") || s.Contains("_r_")) return false;
            foreach (var suffix in new[] { "_l", "_r", ".l", ".r", "-l", "-r" })
            {
                if (s.EndsWith(suffix)) return false;
            }
            return true;
        }

        internal static bool HasSide(string key, out string baseName, out string side)
        {
            if (key.EndsWith("Left")) { baseName = key.Substring(0, key.Length - 4); side = "Left"; return true; }
            if (key.EndsWith("Right")) { baseName = key.Substring(0, key.Length - 5); side = "Right"; return true; }
            baseName = key;
            side = "";
            return false;
        }

        private static IEnumerable<(string, bool)> Candidates(string key, bool framed)
        {
            yield return (key, false);
            bool sided = HasSide(key, out string b, out string side);
            string s1 = side == "Left" ? "L" : side == "Right" ? "R" : "";
            var bases = new List<string> { b };
            if (b.EndsWith("Upper") || b.EndsWith("Lower")) bases.Add(b.Substring(0, b.Length - 5));
            if (framed) bases.AddRange(bases.ToArray().SelectMany(own => JapaneseStyle.TryGetValue(own, out var style) ? style : Array.Empty<string>()));
            if (Synonyms.TryGetValue(b, out var syn)) bases.AddRange(syn);
            foreach (var bb in bases)
            {
                if (sided)
                {
                    yield return (bb + side, false);
                    yield return (bb + "_" + s1, false);
                    yield return (bb + s1, false);
                    yield return (bb + "." + s1, false);
                }
                if (ArkitBase.TryGetValue(bb, out string ak))
                {
                    if (sided) yield return (ak + side, false);
                    yield return (ak, sided);
                }
            }
            foreach (var bb in bases)
            {
                if (bb != key && bb != "Mouth") yield return (bb, sided);
            }
        }

        private static void FindSdkMatcher()
        {
            if (_sdkSearched) return;
            _sdkSearched = true;
            _keyType = typeof(VRCAvatarDescriptor).GetNestedType("FaceTrackingKey", BindingFlags.Public | BindingFlags.NonPublic);
            if (_keyType == null) return;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t;
                try { t = asm.GetType("VRC.SDK3A.Editor.FaceTrackingAutoFill", false); }
                catch (Exception) { continue; }
                if (t == null) continue;
                var m = t.GetMethod("FindFaceTrackingBlendshape", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null && m.GetParameters().Length == 3 && m.ReturnType == typeof(int)) _sdkFind = m;
                break;
            }
        }

        private void ReadDescriptor(VRCAvatarDescriptor descriptor)
        {
            if (descriptor == null) return;
            if (descriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape && descriptor.VisemeSkinnedMesh == _face && descriptor.VisemeBlendShapes != null)
            {
                for (int i = 0; i < _visemes.Length && i < descriptor.VisemeBlendShapes.Length; i++)
                {
                    _visemes[i] = descriptor.VisemeBlendShapes[i];
                }
            }
            var so = new SerializedObject(descriptor);
            var enabled = so.FindProperty("enableFaceTracking");
            var sets = so.FindProperty("faceTrackingBlendshapesSetList");
            if (enabled == null || sets == null || !enabled.boolValue || sets.arraySize == 0 || _keyType == null) return;
            var set = sets.GetArrayElementAtIndex(0);
            var names = set.FindPropertyRelative("faceTrackingBlendshapeNames");
            var mults = set.FindPropertyRelative("faceTrackingBlendshapeMultipliers");
            if (names != null && names.isArray)
            {
                for (int i = 0; i < names.arraySize; i++)
                {
                    string shape = names.GetArrayElementAtIndex(i).stringValue;
                    if (string.IsNullOrEmpty(shape) || !_exact.Contains(shape)) continue;
                    string key = Enum.GetName(_keyType, i);
                    if (key == null) continue;
                    float mult = mults != null && mults.isArray && i < mults.arraySize ? mults.GetArrayElementAtIndex(i).floatValue : 1f;
                    _native[key] = (shape, mult);
                }
            }
            var vnames = set.FindPropertyRelative("visemeBlendshapeNames");
            if (vnames != null && vnames.isArray)
            {
                for (int i = 0; i < vnames.arraySize && i < _visemes.Length; i++)
                {
                    string shape = vnames.GetArrayElementAtIndex(i).stringValue;
                    if (string.IsNullOrEmpty(_visemes[i]) && !string.IsNullOrEmpty(shape)) _visemes[i] = shape;
                }
            }
            UsesNativeFaceTracking = _native.Count > 0;
        }
    }

    internal sealed class AutoFillInputs
    {
        public readonly Dictionary<string, List<VixenMMDSource>> Expressions = new Dictionary<string, List<VixenMMDSource>>();
        public readonly Dictionary<string, List<VixenMMDSource>> Visemes = new Dictionary<string, List<VixenMMDSource>>();

        public static AutoFillInputs From(VixenMMD mmd)
        {
            var inputs = new AutoFillInputs();
            if (mmd == null) return inputs;
            Copy(mmd.expressionShapes, inputs.Expressions);
            Copy(mmd.visemeShapes, inputs.Visemes);
            return inputs;
        }

        private static void Copy(List<VixenMMDShapeSet> from, Dictionary<string, List<VixenMMDSource>> to)
        {
            if (from == null) return;
            foreach (var set in from)
            {
                if (set == null || string.IsNullOrEmpty(set.key) || set.sources == null) continue;
                var list = set.sources.Where(s => s != null && !string.IsNullOrEmpty(s.shape)).ToList();
                if (list.Count > 0) to[set.key] = list;
            }
        }
    }

    internal sealed class AutoFillOptions
    {
        public bool SkipMeshShapes = true;
        public bool ReplaceSetUp = false;
        public bool IncludeAliases = false;
        public bool IncludeProposals = false;
    }

    internal sealed class PlannedMorph
    {
        public string Name;
        public RecipeKind Kind;
        public readonly List<VixenMMDSource> Sources = new List<VixenMMDSource>();
        public readonly List<string> Notes = new List<string>();
        public string Skipped;
        public bool WillApply => Skipped == null && Sources.Count > 0;
    }

    internal static class VixenMMDAutoFill
    {
        private static readonly Dictionary<string, string> RigShapes = new Dictionary<string, string>
        {
            { "上歯上げ", "tooth_top_position_up" }, { "下歯下げ", "tooth_under_position_down" }, { "上歯を隠す", "tooth_top_x" },
            { "犬歯右", "mouth_yaeba(tooth)_top_R" }, { "犬歯左", "mouth_yaeba(tooth)_top_L" },
            { "歯右２", "mouth_yaeba(tooth)_under_R" }, { "歯左２", "mouth_yaeba(tooth)_under_L" },
        };

        internal static PlannedMorph PlanOne(Recipe recipe, string morphName, FaceShapeResolver resolver, AutoFillInputs inputs)
        {
            var plan = new PlannedMorph { Name = morphName, Kind = recipe.Kind };
            var source = VixenMMDRecipes.Resolve(recipe);
            if (source == null)
            {
                plan.Skipped = VixenMMDText.L("Its recipe is missing.");
                return plan;
            }
            var order = new List<string>();
            var acc = new Dictionary<string, float>();
            foreach (var term in source.Terms)
            {
                switch (term.Kind)
                {
                    case RecipeTermKind.Viseme:
                        if (inputs != null && inputs.Visemes.TryGetValue(term.Name, out var vset) && AddSet(acc, order, vset, term.Scale, resolver)) break;
                        if (resolver.TryViseme(term.Name, out string vs)) Add(acc, order, vs, term.Scale);
                        else plan.Notes.Add(VixenMMDText.L("No shape for the {0} viseme.", term.Name));
                        break;
                    case RecipeTermKind.Slot:
                        if (inputs != null && inputs.Expressions.TryGetValue(term.Name, out var eset) && AddSet(acc, order, eset, term.Scale, resolver)) break;
                        plan.Notes.Add(VixenMMDText.L("Needs shapes for \"{0}\".", VixenMMDRecipes.SlotLabel(term.Name)));
                        break;
                    default:
                        AddKeys(plan, term, resolver, acc, order);
                        break;
                }
            }
            foreach (var shape in order)
            {
                float s = Mathf.Round(acc[shape] * 1000f) / 1000f;
                if (Mathf.Abs(s) > 0.0005f) plan.Sources.Add(new VixenMMDSource(shape, s));
            }
            if (plan.Sources.Count == 0 && RigShapes.TryGetValue(source.Name, out string rigName) && resolver.FindFramed(rigName, out string rigShape))
            {
                plan.Notes.Clear();
                plan.Sources.Add(new VixenMMDSource(rigShape, 1f));
            }
            if (plan.Sources.Count == 0) plan.Skipped = VixenMMDText.L("No matching shapes on this face.");
            return plan;
        }

        private static void AddKeys(PlannedMorph plan, RecipeTerm term, FaceShapeResolver resolver, Dictionary<string, float> acc, List<string> order)
        {
            if (!resolver.UsesNativeFaceTracking && resolver.TryCombined(term.FaceKeys, out string both))
            {
                Add(acc, order, both, term.Scale);
                return;
            }
            bool allLeft = term.FaceKeys.All(k => k.EndsWith("Left"));
            bool allRight = term.FaceKeys.All(k => k.EndsWith("Right"));
            bool oneSided = allLeft || allRight;
            var shapes = new List<(string shape, float mult)>();
            bool usedCombined = false;
            bool leftFound = false, rightFound = false;
            foreach (var key in term.FaceKeys)
            {
                if (resolver.TryKey(key, out string shape, out float mult, out bool combined))
                {
                    if (combined) usedCombined = true;
                    if (key.EndsWith("Left")) leftFound = true;
                    if (key.EndsWith("Right")) rightFound = true;
                    if (!shapes.Any(x => x.shape == shape)) shapes.Add((shape, mult));
                }
            }
            bool hasLeft = term.FaceKeys.Any(k => k.EndsWith("Left"));
            bool hasRight = term.FaceKeys.Any(k => k.EndsWith("Right"));
            if (oneSided && usedCombined)
            {
                plan.Notes.Add(VixenMMDText.L("Left out {0}: this face only has a shape for both sides.", string.Join(", ", term.FaceKeys)));
                return;
            }
            if (shapes.Count == 0)
            {
                plan.Notes.Add(VixenMMDText.L("No shape for {0}.", string.Join(", ", term.FaceKeys)));
                return;
            }
            if (hasLeft && hasRight && leftFound != rightFound)
            {
                plan.Notes.Add(leftFound ? VixenMMDText.L("Only the left side has a shape for {0}.", string.Join(", ", term.FaceKeys)) : VixenMMDText.L("Only the right side has a shape for {0}.", string.Join(", ", term.FaceKeys)));
            }
            foreach (var (shape, mult) in shapes) Add(acc, order, shape, term.Scale * mult);
        }

        private static bool AddSet(Dictionary<string, float> acc, List<string> order, List<VixenMMDSource> set, float scale, FaceShapeResolver resolver)
        {
            bool any = false;
            foreach (var s in set)
            {
                if (s == null || !resolver.Has(s.shape)) continue;
                Add(acc, order, s.shape, scale * s.scale);
                any = true;
            }
            return any;
        }

        private static void Add(Dictionary<string, float> acc, List<string> order, string shape, float scale)
        {
            if (!acc.ContainsKey(shape))
            {
                acc[shape] = 0f;
                order.Add(shape);
            }
            acc[shape] += scale;
        }

        internal static List<PlannedMorph> Plan(VixenMMD target, FaceShapeResolver resolver, AutoFillInputs inputs, AutoFillOptions options)
        {
            var result = new List<PlannedMorph>();
            foreach (var recipe in VixenMMDRecipes.All)
            {
                if (recipe.Kind == RecipeKind.Alias && !options.IncludeAliases) continue;
                if (recipe.Kind == RecipeKind.Proposal && !options.IncludeProposals) continue;
                var plan = PlanOne(recipe, recipe.Name, resolver, inputs);
                if (options.SkipMeshShapes && resolver.Has(recipe.Name))
                {
                    plan.Skipped = VixenMMDText.L("The face already has this shape.");
                }
                else if (options.SkipMeshShapes && resolver.NearShape(recipe.Name) is string near)
                {
                    plan = CopyOf(recipe.Name, recipe.Kind, near);
                }
                var existing = target != null ? target.FindMorph(recipe.Name) : null;
                if (plan.Skipped == null && existing != null && existing.sources != null && existing.sources.Count > 0 && !options.ReplaceSetUp)
                {
                    plan.Skipped = VixenMMDText.L("Already set up on this component.");
                }
                result.Add(plan);
            }
            return result;
        }

        internal static PlannedMorph PlanBest(string name, FaceShapeResolver resolver, AutoFillInputs inputs)
        {
            var recipe = VixenMMDRecipes.Find(name);
            var kind = recipe != null ? recipe.Kind : RecipeKind.Tuned;
            string near = resolver.NearShape(name);
            if (near != null) return CopyOf(name, kind, near);
            if (recipe == null) return new PlannedMorph { Name = name, Kind = kind, Skipped = VixenMMDText.L("VixenMMD has no recipe for this morph. Add its shapes below.") };
            var guessed = GuessMissingSlots(VixenMMDRecipes.Resolve(recipe), resolver, inputs);
            var plan = PlanOne(recipe, name, resolver, inputs);
            foreach (var slot in guessed) plan.Notes.Add(VixenMMDText.L("Guessed the shapes for \"{0}\". Check it with Preview.", VixenMMDRecipes.SlotLabel(slot)));
            return plan;
        }

        private static PlannedMorph CopyOf(string name, RecipeKind kind, string shape)
        {
            var plan = new PlannedMorph { Name = name, Kind = kind == RecipeKind.Proposal ? RecipeKind.Tuned : kind };
            plan.Sources.Add(new VixenMMDSource(shape, 1f));
            plan.Notes.Add(VixenMMDText.L("Copies \"{0}\", your face's own version of this shape.", shape));
            return plan;
        }

        private static List<string> GuessMissingSlots(Recipe recipe, FaceShapeResolver resolver, AutoFillInputs inputs)
        {
            var guessed = new List<string>();
            if (recipe == null || inputs == null) return guessed;
            HashSet<string> mmdNames = null;
            foreach (var term in recipe.Terms)
            {
                if (term.Kind != RecipeTermKind.Slot) continue;
                if (inputs.Expressions.TryGetValue(term.Name, out var set) && set.Any(s => resolver.Has(s.shape))) continue;
                if (mmdNames == null) mmdNames = new HashSet<string>(VixenMMDNames.All.Select(n => n.Name));
                var guess = GuessExpression(term.Name, resolver, mmdNames);
                if (guess.Count == 0) continue;
                inputs.Expressions[term.Name] = guess;
                guessed.Add(term.Name);
            }
            return guessed;
        }

        internal static int Apply(VixenMMD target, IEnumerable<PlannedMorph> plan)
        {
            int n = 0;
            Undo.RecordObject(target, "VixenMMD auto-fill");
            foreach (var p in plan)
            {
                if (!p.WillApply) continue;
                var m = target.FindMorph(p.Name);
                if (m == null)
                {
                    m = new VixenMMDMorph(p.Name);
                    target.morphs.Add(m);
                }
                m.enabled = true;
                m.sources = p.Sources.Select(s => new VixenMMDSource(s.shape, s.scale)).ToList();
                n++;
            }
            EditorUtility.SetDirty(target);
            return n;
        }

        internal static List<VixenMMDSource> GuessExpression(string slot, FaceShapeResolver resolver, ISet<string> mmdNames)
        {
            var hints = VixenMMDRecipes.SlotHints(slot).Select(h => h.ToLowerInvariant()).ToArray();
            bool hide = slot.StartsWith("hide");
            var scored = new List<(string name, int score, string region)>();
            foreach (var n in resolver.UsableShapeNames)
            {
                string l = n.ToLowerInvariant();
                string squashed = FaceShapeResolver.Squash(n, false);
                int score = 0;
                foreach (var h in hints)
                {
                    if (l == h || squashed == h) score = Math.Max(score, 100);
                    else if (l.Contains(h) || (h.Length >= 6 && squashed.Contains(h))) score = Math.Max(score, 10);
                }
                if (score == 0) continue;
                if (score < 100 && ((mmdNames != null && mmdNames.Contains(n)) || l.StartsWith("vrc."))) continue;
                if (hide && (l.Contains("hide") || l.Contains("off") || l.Contains("消") || l.StartsWith("no") || l.EndsWith("_x") || l.Contains("none"))) score += 20;
                if (l.EndsWith("_l") || l.EndsWith("_r") || l.EndsWith("left") || l.EndsWith("right")) score -= 3;
                scored.Add((n, score, Region(l)));
            }
            var best = scored.OrderByDescending(x => x.score).ThenBy(x => x.name.Length).ToList();
            if (best.Count == 0) return new List<VixenMMDSource>();
            if (!VixenMMDRecipes.IsFaceWide(slot) || best[0].score >= 100) return new List<VixenMMDSource> { new VixenMMDSource(best[0].name, 1f) };
            var picks = new List<VixenMMDSource>();
            foreach (var region in new[] { "brow", "eye", "mouth" })
            {
                var hit = best.FirstOrDefault(x => x.region == region);
                if (hit.name != null) picks.Add(new VixenMMDSource(hit.name, 1f));
            }
            if (picks.Count == 0) picks.Add(new VixenMMDSource(best[0].name, 1f));
            return picks;
        }

        private static string Region(string lower)
        {
            if (lower.Contains("brow")) return "brow";
            if (lower.Contains("mouth") || lower.Contains("lip") || lower.Contains("jaw") || lower.Contains("tongue") || lower.Contains("teeth")) return "mouth";
            if (lower.Contains("eye") || lower.Contains("lid") || lower.Contains("pupil")) return "eye";
            return "other";
        }
    }
}
