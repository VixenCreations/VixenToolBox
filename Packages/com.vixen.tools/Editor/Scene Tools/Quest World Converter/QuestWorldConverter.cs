#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    public class QuestWorldConverter : EditorWindow
    {
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/QuestConversionEngineStyles.uss";
        private const string OutputRoot = "Assets/VixenTools/Quest World";

        private class MaterialNode
        {
            public Material Material;
            public bool Convert = true;
            public bool AlreadyMobile;
            public int UserCount;
        }

        private class RendererBinding
        {
            public Renderer Renderer;
            public Terrain Terrain;
        }

        private Font _cyberFont;
        private readonly List<MaterialNode> _materials = new List<MaterialNode>();
        private readonly List<RendererBinding> _bindings = new List<RendererBinding>();
        private readonly Dictionary<Texture, Texture> _textureCache = new Dictionary<Texture, Texture>();

        private readonly int[] _textureSizeOptions = { 256, 512, 1024, 2048 };
        private readonly List<string> _textureSizeLabels = new List<string> { "256", "512", "1024", "2048" };
        private int _selectedTextureSizeIndex = 1;

        private MobileShaderTarget _targetShader = MobileShaderTarget.ToonStandard;
        private bool _applyToScene = false;
        private bool _hasScanned = false;

        private ScrollView _materialScroll;
        private Label _summaryLabel;
        private Label _statusLabel;

        [MenuItem("VixenTools/Scene/Quest World Converter")]
        public static void ShowWindow() => GetWindow<QuestWorldConverter>("Quest World Converter");

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            minSize = new Vector2(620, 700);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.name = "quest-world-root";

            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styles != null) root.styleSheets.Add(styles);

            var title = new Label("QUEST WORLD CONVERTER");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18;
            title.style.marginTop = 8;
            title.style.marginBottom = 2;
            title.style.marginLeft = 8;
            if (_cyberFont != null) title.style.unityFont = _cyberFont;
            root.Add(title);

            var blurb = new Label("Makes Quest-ready copies of the materials in your open scenes. Your originals are never changed.");
            blurb.style.whiteSpace = WhiteSpace.Normal;
            blurb.style.marginLeft = 8;
            blurb.style.marginRight = 8;
            blurb.style.marginBottom = 8;
            root.Add(blurb);

            var shaderField = new DropdownField("Target Shader", VixenQuestKit.AllShaderLabels(), VixenQuestKit.GetShaderLabel(_targetShader));
            shaderField.RegisterValueChangedCallback(e => _targetShader = VixenQuestKit.TargetFromLabel(e.newValue));
            shaderField.style.marginLeft = 8;
            shaderField.style.marginRight = 8;
            root.Add(shaderField);

            var sizeField = new DropdownField("Max Texture Size", _textureSizeLabels, _selectedTextureSizeIndex);
            sizeField.RegisterValueChangedCallback(e => _selectedTextureSizeIndex = _textureSizeLabels.IndexOf(e.newValue));
            sizeField.style.marginLeft = 8;
            sizeField.style.marginRight = 8;
            root.Add(sizeField);

            var applyToggle = new Toggle("Point the scene at the new materials");
            applyToggle.value = _applyToScene;
            applyToggle.tooltip = "Off by default. When on, renderers in your open scenes are switched to the converted materials and the scene is marked dirty.";
            applyToggle.RegisterValueChangedCallback(e => _applyToScene = e.newValue);
            applyToggle.style.marginLeft = 8;
            applyToggle.style.marginTop = 4;
            root.Add(applyToggle);

            var scanButton = new Button(ScanScenes) { text = "Scan Open Scenes" };
            scanButton.style.marginLeft = 8;
            scanButton.style.marginRight = 8;
            scanButton.style.marginTop = 10;
            scanButton.style.height = 30;
            root.Add(scanButton);

            _summaryLabel = new Label("Nothing scanned yet.");
            _summaryLabel.style.marginLeft = 8;
            _summaryLabel.style.marginTop = 8;
            root.Add(_summaryLabel);

            var selectRow = new VisualElement();
            selectRow.style.flexDirection = FlexDirection.Row;
            selectRow.style.marginLeft = 8;
            selectRow.style.marginRight = 8;
            selectRow.style.marginTop = 4;

            var selectAll = new Button(() => { _materials.ForEach(m => m.Convert = !m.AlreadyMobile); RefreshMaterialList(); }) { text = "Select All" };
            selectAll.style.flexGrow = 1;
            var selectNone = new Button(() => { _materials.ForEach(m => m.Convert = false); RefreshMaterialList(); }) { text = "Select None" };
            selectNone.style.flexGrow = 1;
            selectRow.Add(selectAll);
            selectRow.Add(selectNone);
            root.Add(selectRow);

            _materialScroll = new ScrollView();
            _materialScroll.style.flexGrow = 1;
            _materialScroll.style.marginLeft = 8;
            _materialScroll.style.marginRight = 8;
            _materialScroll.style.marginTop = 6;
            root.Add(_materialScroll);

            var convertButton = new Button(Convert) { text = "Convert Selected Materials" };
            convertButton.style.marginLeft = 8;
            convertButton.style.marginRight = 8;
            convertButton.style.marginTop = 6;
            convertButton.style.height = 34;
            root.Add(convertButton);

            _statusLabel = new Label(string.Empty);
            _statusLabel.style.whiteSpace = WhiteSpace.Normal;
            _statusLabel.style.marginLeft = 8;
            _statusLabel.style.marginRight = 8;
            _statusLabel.style.marginBottom = 8;
            root.Add(_statusLabel);
        }

        private void ScanScenes()
        {
            _materials.Clear();
            _bindings.Clear();
            _textureCache.Clear();

            var byMaterial = new Dictionary<Material, MaterialNode>();

            foreach (var go in EnumerateRootObjects())
            {
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null) continue;
                    _bindings.Add(new RendererBinding { Renderer = renderer });

                    foreach (var mat in renderer.sharedMaterials)
                    {
                        AddMaterial(byMaterial, mat);
                    }
                }

                foreach (var terrain in go.GetComponentsInChildren<Terrain>(true))
                {
                    if (terrain == null) continue;
                    _bindings.Add(new RendererBinding { Terrain = terrain });
                    AddMaterial(byMaterial, terrain.materialTemplate);
                }
            }

            AddMaterial(byMaterial, RenderSettings.skybox);

            _materials.AddRange(byMaterial.Values.OrderBy(m => m.AlreadyMobile).ThenBy(m => m.Material.name));
            _hasScanned = true;

            RefreshMaterialList();
        }

        private void AddMaterial(Dictionary<Material, MaterialNode> byMaterial, Material mat)
        {
            if (mat == null) return;

            if (byMaterial.TryGetValue(mat, out MaterialNode existing))
            {
                existing.UserCount++;
                return;
            }

            bool alreadyMobile = mat.shader != null && VixenQuestKit.IsWhitelisted(mat.shader);

            byMaterial[mat] = new MaterialNode
            {
                Material = mat,
                AlreadyMobile = alreadyMobile,
                Convert = !alreadyMobile,
                UserCount = 1
            };
        }

        private static IEnumerable<GameObject> EnumerateRootObjects()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var go in scene.GetRootGameObjects())
                {
                    yield return go;
                }
            }
        }

        private void RefreshMaterialList()
        {
            if (_materialScroll == null) return;

            _materialScroll.Clear();

            if (!_hasScanned)
            {
                _summaryLabel.text = "Nothing scanned yet.";
                return;
            }

            int convertible = _materials.Count(m => !m.AlreadyMobile);
            int ready = _materials.Count - convertible;

            _summaryLabel.text = $"{_materials.Count} materials found. {convertible} need converting, {ready} already use a Quest shader.";

            foreach (var node in _materials)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 2;

                var toggle = new Toggle();
                toggle.value = node.Convert;
                toggle.SetEnabled(!node.AlreadyMobile);
                var captured = node;
                toggle.RegisterValueChangedCallback(e => captured.Convert = e.newValue);
                row.Add(toggle);

                string shaderName = node.Material.shader != null ? node.Material.shader.name : "missing shader";
                string suffix = node.AlreadyMobile ? "  (already Quest ready)" : string.Empty;

                var label = new Label($"{node.Material.name}  -  {shaderName}{suffix}");
                label.style.flexGrow = 1;
                label.style.overflow = Overflow.Hidden;
                if (node.AlreadyMobile) label.style.opacity = 0.55f;
                row.Add(label);

                var ping = new Button(() => EditorGUIUtility.PingObject(captured.Material)) { text = "Show" };
                row.Add(ping);

                _materialScroll.Add(row);
            }
        }

        private void Convert()
        {
            if (!_hasScanned)
            {
                _statusLabel.text = "Scan your open scenes first.";
                return;
            }

            var queue = _materials.Where(m => m.Convert && !m.AlreadyMobile && m.Material != null).ToList();
            if (queue.Count == 0)
            {
                _statusLabel.text = "Nothing selected to convert.";
                return;
            }

            Shader targetShader = VixenQuestKit.ResolveShader(_targetShader);
            if (targetShader == null)
            {
                _statusLabel.text = "Could not find a VRChat mobile shader in this project. Make sure the VRChat SDK sample assets are imported.";
                return;
            }

            if (_applyToScene)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Use the converted materials in your scene?",
                    $"This converts {queue.Count} materials, then points your open scenes at the copies. You can undo the swap, but the scenes will be marked as changed.",
                    "Convert and swap",
                    "Cancel");

                if (!confirmed) return;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "Untitled";

            string baseDir = $"{OutputRoot}/{sceneName}";
            string materialsDir = $"{baseDir}/Materials";
            string texturesDir = $"{baseDir}/Textures";

            VixenQuestKit.EnsureDirectoryExists(OutputRoot);
            VixenQuestKit.EnsureDirectoryExists(baseDir);
            VixenQuestKit.EnsureDirectoryExists(materialsDir);
            VixenQuestKit.EnsureDirectoryExists(texturesDir);

            int targetSize = _textureSizeOptions[_selectedTextureSizeIndex];
            var remap = new Dictionary<Material, Material>();
            int converted = 0;

            try
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    MaterialNode node = queue[i];

                    EditorUtility.DisplayProgressBar(
                        "Quest World Converter",
                        $"Converting {node.Material.name} ({i + 1} of {queue.Count})",
                        (float)i / queue.Count);

                    Material questMat = new Material(targetShader) { name = node.Material.name + "_Quest" };

                    VixenQuestKit.TransferProperties(node.Material, questMat, (tex, isNormal, isLinear) =>
                        VixenQuestKit.ProcessAndCloneTexture(tex, isNormal, isLinear, texturesDir, targetSize, _textureCache, null));

                    string matPath = AssetDatabase.GenerateUniqueAssetPath($"{materialsDir}/{questMat.name}.mat");
                    AssetDatabase.CreateAsset(questMat, matPath);

                    remap[node.Material] = questMat;
                    converted++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();

            int swapped = 0;
            if (_applyToScene) swapped = ApplyRemap(remap);

            AssetDatabase.Refresh();

            _statusLabel.text = _applyToScene
                ? $"Converted {converted} materials into {materialsDir} and pointed {swapped} renderers at them."
                : $"Converted {converted} materials into {materialsDir}. Nothing in your scene was changed.";

            ScanScenes();
        }

        private int ApplyRemap(Dictionary<Material, Material> remap)
        {
            int swapped = 0;

            foreach (var binding in _bindings)
            {
                if (binding.Renderer != null)
                {
                    Material[] mats = binding.Renderer.sharedMaterials;
                    bool changed = false;

                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null && remap.TryGetValue(mats[i], out Material replacement))
                        {
                            mats[i] = replacement;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        Undo.RecordObject(binding.Renderer, "Use Quest Materials");
                        binding.Renderer.sharedMaterials = mats;
                        EditorUtility.SetDirty(binding.Renderer);
                        swapped++;
                    }
                }

                if (binding.Terrain != null && binding.Terrain.materialTemplate != null)
                {
                    if (remap.TryGetValue(binding.Terrain.materialTemplate, out Material replacement))
                    {
                        Undo.RecordObject(binding.Terrain, "Use Quest Materials");
                        binding.Terrain.materialTemplate = replacement;
                        EditorUtility.SetDirty(binding.Terrain);
                        swapped++;
                    }
                }
            }

            if (swapped > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }

            return swapped;
        }
    }
}
#endif
