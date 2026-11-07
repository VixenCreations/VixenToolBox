using System;
using System.Collections.Generic;
using nadena.dev.ndmf.localization;
using UnityEditor;

namespace VixenTools.MMD.Editor
{
    internal static class VixenMMDText
    {
        private const string Folder = "Packages/com.vixencreations.vixens-toolbox/VixenMMD/Editor/Localization/";
        private static readonly string[] Guids = { "dc741e5b284c459c929984ebda741c55", "e42c2b23d3854b1789fdb37058eb4d61", "78d2e97eef724ce7a26274f16ca2dfcd", "f53f105886dc4256b4b91e2fc1cd658c" };
        private static readonly string[] Files = { "en-US.po", "ja-JP.po", "zh-Hans.po", "ko-KR.po" };
        private static Localizer _localizer;

        internal static Localizer Localizer => _localizer ??= new Localizer("en-US", Load);

        private static List<UnityEngine.LocalizationAsset> Load()
        {
            var list = new List<UnityEngine.LocalizationAsset>();
            for (int i = 0; i < Guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(Guids[i]);
                if (string.IsNullOrEmpty(path)) path = Folder + Files[i];
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.LocalizationAsset>(path);
                if (asset != null) list.Add(asset);
            }
            return list;
        }

        internal static string L(string english)
        {
            if (string.IsNullOrEmpty(english)) return english;
            return Localizer.TryGetLocalizedString(english, out var value) ? value : english;
        }

        internal static string L(string english, params object[] args)
        {
            string text = L(english);
            try { return string.Format(text, args); }
            catch (FormatException) { return string.Format(english, args); }
        }

        internal static string Meaning(string mmdName, string english)
        {
            if (!string.IsNullOrEmpty(mmdName) && Localizer.TryGetLocalizedString("mmd:" + mmdName, out var value)) return value;
            return english ?? "";
        }

        internal static void DrawLanguagePicker() => nadena.dev.ndmf.ui.LanguageSwitcher.DrawImmediate();
    }
}
