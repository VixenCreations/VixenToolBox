#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine.Animations;

namespace VixenTools.Editor
{
    public class VixenAccessoryEngine : EditorWindow
    {
        private const string UssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenAvatarValidatorStyles.uss";
        private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";
        private const string GENERATED_ASSET_PATH = "Assets/VixenTools/Meshes/BakedAccessories/";

        private Font _cyberFont;

        public enum PipelineMode { FullGeneration, AppendToExisting }
        public enum MountStrategy { DestructiveAutoRig, KinematicConstraint }

        [System.Serializable]
        public class AccessoryMapping
        {
            public GameObject sourceAccessory;
            public Transform targetBone;
        }

        [SerializeField] private PipelineMode activePipeline = PipelineMode.FullGeneration;
        [SerializeField] private Transform sourceArmatureRoot;
        [SerializeField] private Transform targetAccessoryRoot;
        [SerializeField] private MountStrategy strategy = MountStrategy.DestructiveAutoRig;
        [SerializeField] private List<AccessoryMapping> accessoryMappings = new List<AccessoryMapping>();

        private SerializedObject _serializedObject;

        [MenuItem("VixenTools/Avatars/Accessory Engine", priority = 42)]
        public static void ShowWindow()
        {
            var window = GetWindow<VixenAccessoryEngine>("Accessory Engine");
            window.minSize = new Vector2(480, 650);
            window.Show();
        }

        private void OnEnable()
        {
            _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            _serializedObject = new SerializedObject(this);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.name = "hub-root";

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var header = new VisualElement { name = "hub-header", style = { minHeight = 80, justifyContent = Justify.Center, alignItems = Align.Center } };
            var titleLabel = new Label("<color=#00e5ff>ACCESSORY</color> <color=#ff00aa>MOUNTING</color> ENGINE") { enableRichText = true };
            titleLabel.AddToClassList("hub-header-title");
            if (_cyberFont != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            header.Add(titleLabel);
            root.Add(header);

            var scroll = new ScrollView() { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };

            var modePanel = CreateCyberPanel("1. Mode", "#ffaa00");
            var pipelineField = new PropertyField(_serializedObject.FindProperty("activePipeline"), "Execution Mode");
            pipelineField.Bind(_serializedObject);
            modePanel.Add(pipelineField);

            var modeDesc = new Label() { enableRichText = true, style = { marginTop = 5, marginBottom = 5, whiteSpace = WhiteSpace.Normal } };
            modeDesc.AddToClassList("md-p");
            pipelineField.RegisterValueChangeCallback(evt => UpdateModeDescription(modeDesc, (PipelineMode)evt.changedProperty.enumValueIndex));
            UpdateModeDescription(modeDesc, activePipeline);
            modePanel.Add(modeDesc);
            scroll.Add(modePanel);

            var configPanel = CreateCyberPanel("2. Armature Core Targets", "#00e5ff");
            var sourceField = new PropertyField(_serializedObject.FindProperty("sourceArmatureRoot"), "Source Armature");
            var targetField = new PropertyField(_serializedObject.FindProperty("targetAccessoryRoot"), "Target Accessory Root");
            sourceField.Bind(_serializedObject);
            targetField.Bind(_serializedObject);
            configPanel.Add(sourceField);
            configPanel.Add(targetField);
            scroll.Add(configPanel);

            var strategyPanel = CreateCyberPanel("3. How To Mount", "#ff00aa");
            var strategyField = new PropertyField(_serializedObject.FindProperty("strategy"), "Mounting Strategy");
            strategyField.Bind(_serializedObject);
            strategyPanel.Add(strategyField);

            var strategyDesc = new Label() { enableRichText = true, style = { marginTop = 5, marginBottom = 10, whiteSpace = WhiteSpace.Normal } };
            strategyDesc.AddToClassList("md-p");
            strategyField.RegisterValueChangeCallback(evt => UpdateStrategyDescription(strategyDesc, (MountStrategy)evt.changedProperty.enumValueIndex));
            UpdateStrategyDescription(strategyDesc, strategy);
            strategyPanel.Add(strategyDesc);

            var listField = new PropertyField(_serializedObject.FindProperty("accessoryMappings"), "Accessory Mappings");
            listField.Bind(_serializedObject);
            strategyPanel.Add(listField);
            scroll.Add(strategyPanel);

            var execPanel = new VisualElement { style = { marginTop = 20, marginBottom = 20 } };
            var scanBtn = new Button(ExecuteEngine) { text = "MOUNT ACCESSORIES" };
            scanBtn.AddToClassList("cyber-action-btn");
            scanBtn.AddToClassList("cyan-btn");
            execPanel.Add(scanBtn);
            scroll.Add(execPanel);

            root.Add(scroll);
        }

        private void UpdateModeDescription(Label label, PipelineMode mode)
        {
            if (mode == PipelineMode.FullGeneration) label.text = "<b><color=#00e5ff>Full Generation:</color></b> Clones a fresh, empty armature from the source and mounts the accessories onto it.";
            else label.text = "<b><color=#ffaa00>Append To Existing:</color></b> Skips the clone. Adds new accessories to the armature already on your target.";
        }

        private void UpdateStrategyDescription(Label label, MountStrategy strat)
        {
            if (strat == MountStrategy.DestructiveAutoRig) label.text = "<b><color=#ff00aa>Destructive Auto-Rig:</color></b> Re-rigs the accessory onto the armature for real. Best for soft, deforming meshes.";
            else label.text = "<b><color=#00ff66>Kinematic Constraint:</color></b> Attaches the accessory with a parent constraint instead of re-rigging it. Best for rigid props, particles or audio sources.";
        }

        private VisualElement CreateCyberPanel(string title, string hex)
        {
            var panel = new VisualElement();
            panel.AddToClassList("cyber-panel");
            var header = new Label($"<color={hex}>{title}</color>") { enableRichText = true };
            header.AddToClassList("panel-header");
            if (_cyberFont != null) header.style.unityFontDefinition = new StyleFontDefinition(_cyberFont);
            panel.Add(header);

            var sep = new VisualElement();
            sep.AddToClassList("md-separator");
            ColorUtility.TryParseHtmlString(hex, out Color c); c.a = 0.3f;
            sep.style.backgroundColor = c;
            panel.Add(sep);

            return panel;
        }

        private void ExecuteEngine()
        {
            if (sourceArmatureRoot == null || targetAccessoryRoot == null)
            {
                Debug.LogWarning("[VixForge] Execution halted: Missing Armature or Target Root.");
                return;
            }

            Undo.SetCurrentGroupName("Mount Accessories");
            int undoGroup = Undo.GetCurrentGroup();

            Dictionary<Transform, Transform> boneMap = new Dictionary<Transform, Transform>();

            if (activePipeline == PipelineMode.FullGeneration)
            {
                CloneHierarchy(sourceArmatureRoot, targetAccessoryRoot, boneMap);
                Debug.Log("<b>[VixForge]</b> Pipeline: Generated new sterile hierarchy.");
            }
            else
            {
                MapExistingHierarchy(sourceArmatureRoot, targetAccessoryRoot, boneMap);
                Debug.Log("<b>[VixForge]</b> Pipeline: Mapped to existing sterile hierarchy.");
            }

            if (strategy == MountStrategy.DestructiveAutoRig)
            {
                if (!AssetDatabase.IsValidFolder("Assets/VixenTools"))
                    AssetDatabase.CreateFolder("Assets", "VixenTools");
                if (!AssetDatabase.IsValidFolder("Assets/VixenTools/Meshes"))
                    AssetDatabase.CreateFolder("Assets/VixenTools", "Meshes");
                if (!AssetDatabase.IsValidFolder("Assets/VixenTools/Meshes/BakedAccessories"))
                    AssetDatabase.CreateFolder("Assets/VixenTools/Meshes", "BakedAccessories");
            }

            foreach (var mapping in accessoryMappings)
            {
                if (mapping.sourceAccessory == null || mapping.targetBone == null) continue;

                if (boneMap.TryGetValue(mapping.targetBone, out Transform sterileBone))
                {
                    if (strategy == MountStrategy.DestructiveAutoRig)
                        BakeAndRigHierarchy(mapping.sourceAccessory, sterileBone, targetAccessoryRoot);
                    else
                        MountWithKinematicConstraint(mapping.sourceAccessory, sterileBone, targetAccessoryRoot);
                }
                else
                {
                    Debug.LogWarning($"[VixForge] Could not resolve a mapping for [{mapping.targetBone.name}]. Ensure it exists in the sterile root.");
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            AssetDatabase.SaveAssets();
            Debug.Log($"<b>[VixForge]</b> Pipeline complete. Processed {accessoryMappings.Count} accessories into {targetAccessoryRoot.name}.");
            EditorGUIUtility.PingObject(targetAccessoryRoot);
        }

        private void CloneHierarchy(Transform source, Transform currentParent, Dictionary<Transform, Transform> boneMap)
        {
            GameObject cloneObj = new GameObject(source.name);
            Undo.RegisterCreatedObjectUndo(cloneObj, "Create Bone");

            Transform cloneTransform = cloneObj.transform;
            cloneTransform.SetParent(currentParent);
            cloneTransform.localPosition = source.localPosition;
            cloneTransform.localRotation = source.localRotation;
            cloneTransform.localScale = source.localScale;

            boneMap[source] = cloneTransform;
            foreach (Transform child in source) CloneHierarchy(child, cloneTransform, boneMap);
        }

        private void MapExistingHierarchy(Transform sourceRoot, Transform sterileRoot, Dictionary<Transform, Transform> boneMap)
        {
            Transform actualSterileRoot = sterileRoot.Find(sourceRoot.name);
            if (actualSterileRoot == null)
            {
                Debug.LogError($"[VixForge] Critical Failure: Could not find '{sourceRoot.name}' inside '{sterileRoot.name}'.");
                return;
            }
            TraverseAndMap(sourceRoot, sourceRoot, actualSterileRoot, boneMap);
        }

        private void TraverseAndMap(Transform globalSourceRoot, Transform currentSourceNode, Transform actualSterileRoot, Dictionary<Transform, Transform> boneMap)
        {
            string relativePath = AnimationUtility.CalculateTransformPath(currentSourceNode, globalSourceRoot);

            if (string.IsNullOrEmpty(relativePath)) boneMap[currentSourceNode] = actualSterileRoot;
            else
            {
                Transform sterileNode = actualSterileRoot.Find(relativePath);
                if (sterileNode != null) boneMap[currentSourceNode] = sterileNode;
            }

            foreach (Transform child in currentSourceNode) TraverseAndMap(globalSourceRoot, child, actualSterileRoot, boneMap);
        }

        private void MountWithKinematicConstraint(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)
        {
            Undo.RecordObject(accessoryRoot, "Kinematic Mount");
            Undo.SetTransformParent(accessoryRoot.transform, parentRoot, "Set Parent Root");

            ParentConstraint constraint = accessoryRoot.GetComponent<ParentConstraint>();
            if (constraint == null) constraint = Undo.AddComponent<ParentConstraint>(accessoryRoot);

            Vector3 positionOffset = sterileBone.InverseTransformPoint(accessoryRoot.transform.position);
            Quaternion rotationOffset = Quaternion.Inverse(sterileBone.rotation) * accessoryRoot.transform.rotation;

            ConstraintSource source = new ConstraintSource { sourceTransform = sterileBone, weight = 1f };

            Undo.RecordObject(constraint, "Configure Constraint");
            while (constraint.sourceCount > 0) constraint.RemoveSource(0);
            constraint.AddSource(source);

            constraint.SetTranslationOffset(0, positionOffset);
            constraint.SetRotationOffset(0, rotationOffset.eulerAngles);
            constraint.constraintActive = true;
            constraint.locked = true;

            Debug.Log($"[VixForge] Kinematically mounted [{accessoryRoot.name}] (and children) to [{sterileBone.name}].");
        }

        private void BakeAndRigHierarchy(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)
        {
            Undo.RecordObject(accessoryRoot, "Auto-Rig Accessory Hierarchy");

            List<System.Tuple<GameObject, Mesh, Material[]>> targetMeshes = new List<System.Tuple<GameObject, Mesh, Material[]>>();

            var smrs = accessoryRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                if (smr != null && smr.sharedMesh != null)
                    targetMeshes.Add(new System.Tuple<GameObject, Mesh, Material[]>(smr.gameObject, smr.sharedMesh, smr.sharedMaterials));
            }

            var meshFilters = accessoryRoot.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in meshFilters)
            {
                if (mf != null && mf.sharedMesh != null)
                {
                    bool alreadyTracked = false;
                    foreach (var tracked in targetMeshes)
                    {
                        if (tracked.Item1 == mf.gameObject) { alreadyTracked = true; break; }
                    }

                    if (!alreadyTracked)
                    {
                        Material[] mats = mf.GetComponent<MeshRenderer>()?.sharedMaterials;
                        targetMeshes.Add(new System.Tuple<GameObject, Mesh, Material[]>(mf.gameObject, mf.sharedMesh, mats));
                    }
                }
            }

            foreach (var target in targetMeshes)
            {
                ProcessSingleMesh(target.Item1, target.Item2, target.Item3, sterileBone);
            }

            Undo.SetTransformParent(accessoryRoot.transform, parentRoot, "Set Parent Root");

            ParentConstraint constraint = accessoryRoot.GetComponent<ParentConstraint>();
            if (constraint == null) constraint = Undo.AddComponent<ParentConstraint>(accessoryRoot);

            Vector3 positionOffset = sterileBone.InverseTransformPoint(accessoryRoot.transform.position);
            Quaternion rotationOffset = Quaternion.Inverse(sterileBone.rotation) * accessoryRoot.transform.rotation;

            ConstraintSource source = new ConstraintSource { sourceTransform = sterileBone, weight = 1f };

            Undo.RecordObject(constraint, "Configure Constraint");
            while (constraint.sourceCount > 0) constraint.RemoveSource(0);
            constraint.AddSource(source);

            constraint.SetTranslationOffset(0, positionOffset);
            constraint.SetRotationOffset(0, rotationOffset.eulerAngles);
            constraint.constraintActive = true;
            constraint.locked = true;

            Debug.Log($"[VixForge] Auto-Rigged [{accessoryRoot.name}]. Synced GameObject to [{sterileBone.name}] to preserve physics.");
        }

        private void ProcessSingleMesh(GameObject targetObj, Mesh sourceMesh, Material[] mats, Transform sterileBone)
        {
            Mesh bakedMesh = Instantiate(sourceMesh);
            bakedMesh.name = $"{sourceMesh.name}_Rigged_{System.Guid.NewGuid().ToString().Substring(0, 5)}";

            Matrix4x4 localToBoneOffset = sterileBone.worldToLocalMatrix * targetObj.transform.localToWorldMatrix;

            Vector3[] verts = bakedMesh.vertices;
            Vector3[] normals = bakedMesh.normals;
            Vector4[] tangents = bakedMesh.tangents;
            BoneWeight[] boneWeights = new BoneWeight[verts.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = localToBoneOffset.MultiplyPoint3x4(verts[i]);
                if (normals.Length > i) normals[i] = localToBoneOffset.MultiplyVector(normals[i]).normalized;
                if (tangents.Length > i)
                {
                    Vector3 tDir = localToBoneOffset.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z)).normalized;
                    tangents[i] = new Vector4(tDir.x, tDir.y, tDir.z, tangents[i].w);
                }
                boneWeights[i] = new BoneWeight { boneIndex0 = 0, weight0 = 1f };
            }

            bakedMesh.vertices = verts;
            bakedMesh.normals = normals;
            bakedMesh.tangents = tangents;
            bakedMesh.boneWeights = boneWeights;
            bakedMesh.bindposes = new Matrix4x4[] { Matrix4x4.identity };
            bakedMesh.RecalculateBounds();

            bakedMesh.ClearBlendShapes();
            for (int b = 0; b < sourceMesh.blendShapeCount; b++)
            {
                string shapeName = sourceMesh.GetBlendShapeName(b);
                for (int f = 0; f < sourceMesh.GetBlendShapeFrameCount(b); f++)
                {
                    float weight = sourceMesh.GetBlendShapeFrameWeight(b, f);
                    Vector3[] deltaV = new Vector3[verts.Length];
                    Vector3[] deltaN = new Vector3[verts.Length];
                    Vector3[] deltaT = new Vector3[verts.Length];

                    sourceMesh.GetBlendShapeFrameVertices(b, f, deltaV, deltaN, deltaT);

                    for (int i = 0; i < verts.Length; i++)
                    {
                        deltaV[i] = localToBoneOffset.MultiplyVector(deltaV[i]);
                        deltaN[i] = localToBoneOffset.MultiplyVector(deltaN[i]);
                        deltaT[i] = localToBoneOffset.MultiplyVector(deltaT[i]);
                    }
                    bakedMesh.AddBlendShapeFrame(shapeName, weight, deltaV, deltaN, deltaT);
                }
            }

            string assetPath = $"{GENERATED_ASSET_PATH}{bakedMesh.name}.asset";
            AssetDatabase.CreateAsset(bakedMesh, assetPath);

            MeshFilter filter = targetObj.GetComponent<MeshFilter>();
            MeshRenderer mr = targetObj.GetComponent<MeshRenderer>();

            if (filter != null) Undo.DestroyObjectImmediate(filter);
            if (mr != null) Undo.DestroyObjectImmediate(mr);

            SkinnedMeshRenderer smr = targetObj.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) smr = Undo.AddComponent<SkinnedMeshRenderer>(targetObj);

            Undo.RecordObject(smr, "Apply Rigged Mesh");
            smr.sharedMesh = bakedMesh;
            if (mats != null) smr.sharedMaterials = mats;

            smr.bones = new Transform[] { sterileBone };
            smr.rootBone = sterileBone;
            smr.localBounds = new Bounds(Vector3.zero, new Vector3(2.5f, 2.5f, 2.5f));
        }
    }
}
#endif