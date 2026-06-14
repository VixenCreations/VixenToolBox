# Vixens Toolbox - Developer Info (in-code comment reference)

> **What this is.** On **2026-06-11** every in-code comment was moved out of the first-party VixForge source files in this package into this document, and stripped from the code, to shrink the source files so Unity imports and compiles them faster. This file is the canonical record of that knowledge.
>
> **How it's organised.** Comments are grouped by source file, then by the nearest enclosing code structure (the function / property / section they belonged to - the "structure method" anchor). Each entry keeps its text and the original pre-strip line number. The **structure signature is the durable anchor**; line numbers refer to the source as it stood just before the strip and will drift as the files change.
>
> **Convention going forward:** new code comments for this project live here, filed under their structure, not inline in the source.
>
> **Update (2026-06-12).** The VixenWear Latex Ultra shader was split into its own standalone release, so its source files and their comment entries were removed from this package and from this document. The world-side AreaLit GI broadcaster that feeds those shaders stays in the toolbox, and its comments remain below.


*Total entries: 822*


---

## `Runtime/AreaLitBroadcaster/AreaLitGlobalBroadcaster.cs`

*19 comment(s).*


### `(file scope)`
<sub>L1–L19</sub>

- **L1** - AreaLit -> VixenWear avatar GI bridge.  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L2** -   <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L3** - AreaLit (unlike LTCGI) ships no global broadcast - its LightCam just renders the  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L4** - area-light meshes into a LightMesh RenderTexture, and each AreaLit/Standard material  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L5** - is pointed at that RT per-material. This helper closes that gap the same way the  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L6** - LTCGI controller does: drop it on a GameObject next to your AreaLit LightCam, assign  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L7** - the same LightMesh RenderTexture + light/video RenderTexture the AreaLit/Standard  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L8** - materials use, and it broadcasts them scene-wide as:  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L9** - _Udon_AreaLit_LightMesh   (Texture2D - quad positions / uv / tint)  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L10** - _Udon_AreaLit_Tex0        (Texture2D - the area-light / video colour)  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L11** - _Udon_AreaLit_Enable      (float     - 1 when live)  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L12** - VixenWear avatars then intercept the world's AreaLit at the GI level automatically,  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L13** - exactly like they read LTCGI's _Udon_LTCGI_* globals - no per-material assignment on  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L14** - the avatar.  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L15** -   <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L16** - This file lives in its own assembly (VixenWear.AreaLitBroadcaster.asmdef) gated on the  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L17** - UdonSharp package via the VW_UDONSHARP_READY define, so it is excluded entirely in  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L18** - avatar projects that do not have the VRChat Worlds SDK / UdonSharp - it never breaks a  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>
- **L19** - build that can't use it.  <br/><sub>↳ before `#if VW_UDONSHARP_READY`</sub>

---

## `Editor/Avatar Tools/Armature Cloner/AccessoryArmatureCloner.cs`

*13 comment(s).*


### `private void CreateGUI()`
<sub>L68–L91</sub>

- **L68** - 1. Pipeline  <br/><sub>↳ before `var modePanel = CreateCyberPanel("1. Pipeline Control", "#ffaa00");`</sub>
- **L81** - 2. Targets  <br/><sub>↳ before `var configPanel = CreateCyberPanel("2. Armature Core Targets", "#00e5ff");`</sub>
- **L91** - 3. Strategy  <br/><sub>↳ before `var strategyPanel = CreateCyberPanel("3. Accessory Execution Strategy", "#ff00aa");`</sub>

### `private void MountWithKinematicConstraint(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)`
<sub>L270–L272</sub>

- **L270** - ====================================================================  <br/><sub>↳ before `private void BakeAndRigHierarchy(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)`</sub>
- **L271** - UPGRADED ALGORITHM: RECURSIVE HIERARCHY AUTO-RIGGING  <br/><sub>↳ before `private void BakeAndRigHierarchy(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)`</sub>
- **L272** - ====================================================================  <br/><sub>↳ before `private void BakeAndRigHierarchy(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)`</sub>

### `private void BakeAndRigHierarchy(GameObject accessoryRoot, Transform sterileBone, Transform parentRoot)`
<sub>L277–L319</sub>

- **L277** - 1. GATHER PHASE  <br/><sub>↳ before `List<System.Tuple<GameObject, Mesh, Material[]>> targetMeshes = new List<System.Tuple<GameObject, Mesh, Material[]>>();`</sub>
- **L306** - 2. EXECUTION PHASE  <br/><sub>↳ before `foreach (var target in targetMeshes)`</sub>
- **L312** - 3. KINEMATIC SYNC (The PhysBone Fix)  <br/><sub>↳ before `Undo.SetTransformParent(accessoryRoot.transform, parentRoot, "Set Parent Root");`</sub>
- **L313** - Instead of zeroing the root and breaking physics, we lock the GameObject to the sterile bone.  <br/><sub>↳ before `Undo.SetTransformParent(accessoryRoot.transform, parentRoot, "Set Parent Root");`</sub>
- **L319** - Calculate exact offsets to maintain visual and physical integrity  <br/><sub>↳ before `Vector3 positionOffset = sterileBone.InverseTransformPoint(accessoryRoot.transform.position);`</sub>

### `private void ProcessSingleMesh(GameObject targetObj, Mesh sourceMesh, Material[] mats, Transform sterileBone)`
<sub>L342–L395</sub>

- **L342** - Calculate offset dynamically based on this specific child's world transform  <br/><sub>↳ before `Matrix4x4 localToBoneOffset = sterileBone.worldToLocalMatrix * targetObj.transform.localToWorldMatrix;`</sub>
- **L395** - In-Place Component Swapping  <br/><sub>↳ before `MeshFilter filter = targetObj.GetComponent<MeshFilter>();`</sub>

---

## `Editor/Avatar Tools/Avatar Validator/VixenAvatarValidator.cs`

*85 comment(s).*


### `(file scope)`
<sub>L22–L23</sub>

- **L22** - VixForge Core: Advanced dual-pipeline validation and automated optimization engine.  <br/><sub>↳ before `public static class AvatarSDKValidator`</sub>
- **L23** - Incorporates proprietary VixForge Topology Erasure and tight-fit bounding constraints.  <br/><sub>↳ before `public static class AvatarSDKValidator`</sub>

### `public HashSet<Texture> UniqueTextures = new HashSet<Texture>();`
<sub>L64</sub>

- **L64** - Hard Cap Raw Data  <br/><sub>↳ before `public int PolyCount = 0;`</sub>

### `public List<OptimizationTask> OptimizationSuite = new List<OptimizationTask>();`
<sub>L81–L82</sub>

- **L81** - Authoritative results from VRChat's own performance calculator (additive to the  <br/><sub>↳ before `public string OfficialOverallRating = null;`</sub>
- **L82** - hand-rolled hardware-cap panel). Null rating = SDK calc was unavailable.  <br/><sub>↳ before `public string OfficialOverallRating = null;`</sub>

### `public static ValidationReport RunFullSweep(GameObject avatarRoot, int targetTexSize = 1024, PCPerformanceRank targetRank = PCPerformanceRank.Poor, ResizeMode resizeMode = ResizeMode.Downscale)`
<sub>L92–L616</sub>

- **L92** - --- 1. ARMATURE & DEPENDENCY GRAPH MAPPING ---  <br/><sub>↳ before `var animator = avatarRoot.GetComponent<Animator>();`</sub>
- **L125** - --- 2. DESTRUCTIVE TOPOLOGY ERASURE SYSTEM ---  <br/><sub>↳ before `var allTransforms = avatarRoot.GetComponentsInChildren<Transform>(true);`</sub>
- **L164** - Task: Purge Orphaned Transforms  <br/><sub>↳ before `List<Transform> orphanedTransforms = new List<Transform>();`</sub>
- **L187** - Task: Strip Disabled Components  <br/><sub>↳ before `List<Behaviour> disabledComponents = new List<Behaviour>();`</sub>
- **L209** - Task: Per-Mesh Auto-Fit Bounds (PhysBone aware)  <br/><sub>↳ before `report.OptimizationSuite.Add(new OptimizationTask`</sub>
- **L218** - 1.5x = +25% per side: covers normal animation drift on static meshes.  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L219** - 3.0x = +100% per side: covers PhysBone swing on hair/tail/cape/breast bones,  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L220** - which Unity's import-time bounds CANNOT account for (PhysBones move bones  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L221** - at runtime; the SMR docs explicitly list this as a case where the imported  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L222** - bounds may be exceeded).  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L223** - minBoundsSize floors near-zero bounds on degenerate meshes so they don't  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L224** - instantly cull.  <br/><sub>↳ before `const float staticMargin = 1.5f;`</sub>
- **L229** - Walk every PhysBone's root subtree once and record affected bones.  <br/><sub>↳ before `var physBoneAffected = new HashSet<Transform>();`</sub>
- **L230** - Any SMR whose bone list touches this set gets the larger margin.  <br/><sub>↳ before `var physBoneAffected = new HashSet<Transform>();`</sub>
- **L246** - We strictly keep this false. updateWhenOffscreen=true recalculates bounds  <br/><sub>↳ before `smr.updateWhenOffscreen = false;`</sub>
- **L247** - every frame which Unity's docs admit is fine but a perf killer in VRChat.  <br/><sub>↳ before `smr.updateWhenOffscreen = false;`</sub>
- **L250** - Pick margin per-mesh based on whether this SMR is skinned to any PhysBone subtree.  <br/><sub>↳ before `bool hasPhysBone = false;`</sub>
- **L264** - sharedMesh.bounds is the bind-pose AABB in MESH local space (the SMR's  <br/><sub>↳ before `Bounds bind = smr.sharedMesh.bounds;`</sub>
- **L265** - transform space). smr.localBounds is in ROOT BONE local space - Unity  <br/><sub>↳ before `Bounds bind = smr.sharedMesh.bounds;`</sub>
- **L266** - docs: "the bounds move along with [the root bone] transform". So we  <br/><sub>↳ before `Bounds bind = smr.sharedMesh.bounds;`</sub>
- **L267** - convert by transforming the 8 corners through both spaces.  <br/><sub>↳ before `Bounds bind = smr.sharedMesh.bounds;`</sub>
- **L290** - Task: Topology Erasure (Leaf Bone Weight Transfer)  <br/><sub>↳ before `List<Transform> deepLeafBones = new List<Transform>();`</sub>
- **L322** - Task: Destructive Vertex Welding  <br/><sub>↳ before `List<SkinnedMeshRenderer> heavyMeshes = skinnedRenderers.Where(s => s.sharedMesh != null && CountTriangles(s.sharedMesh) > 15000).ToList();`</sub>
- **L340** - 1. Map protected material slots  <br/><sub>↳ before `HashSet<int> protectedSlots = new HashSet<int>();`</sub>
- **L348** - 2. Build 4D-Chess Kinematic Protection System  <br/><sub>↳ before `HashSet<int> protectedBoneIndices = new HashSet<int>();`</sub>
- **L362** - 3. Engage the Multipass Microwelder (EXTREME PRECISION)  <br/><sub>↳ before `VixenMeshPatcher.MultipassTargetedWeld(`</sub>
- **L367** - HARD CAP at 5mm. Absolute visual safety.  <br/><sub>↳ on `maxThreshold: 0.005f,`</sub>
- **L380** - --- 3. VRAM HEURISTICS & DEEP MATERIAL SYSTEM SCAN ---  <br/><sub>↳ before `HashSet<Material> allMaterials = new HashSet<Material>();`</sub>
- **L418** - Official VRChat performance pass: authoritative ratings + the ~19 categories the  <br/><sub>↳ before `RunOfficialPerformanceScan(avatarRoot, report);`</sub>
- **L419** - hand-rolled hardware-cap panel doesn't measure. Additive and fully guarded.  <br/><sub>↳ before `RunOfficialPerformanceScan(avatarRoot, report);`</sub>
- **L422** - Count only textures we will actually touch: skips RenderTextures, out-of-project  <br/><sub>↳ before `int processableTextures = 0;`</sub>
- **L423** - assets, and protected shader/data textures (Poiyomi internals, .exr LUTs, etc.).  <br/><sub>↳ before `int processableTextures = 0;`</sub>
- **L428** - Gating: both modes require at least one processable texture. Downscale additionally  <br/><sub>↳ before `bool showResizeTask = processableTextures > 0 &&`</sub>
- **L429** - requires the VRAM threshold (it's a perf fix); Upscale fires on intent alone.  <br/><sub>↳ before `bool showResizeTask = processableTextures > 0 &&`</sub>
- **L447** - --- NEW: VIXFORGE CORE HEURISTICS (TEXTURE OPTIMIZATION & DATA INTEGRITY) ---  <br/><sub>↳ before `foreach (var tex in report.UniqueTextures)`</sub>
- **L449** - 1. Mipmap Streaming Validation  <br/><sub>↳ before `foreach (var tex in report.UniqueTextures)`</sub>
- **L475** - 2. Deep Shader Inspection: Packed Map sRGB Validation  <br/><sub>↳ before `foreach (var mat in allMaterials)`</sub>
- **L490** - Packed PBR maps MUST be linear. sRGB corrupts the structural data.  <br/><sub>↳ before `if (importer != null && importer.sRGBTexture)`</sub>
- **L511** - --- 4. PC PIPELINE SYSTEM (RAW DATA & WARNINGS) ---  <br/><sub>↳ before `var illegalPC = AvatarValidation.FindIllegalComponents(avatarRoot).ToList();`</sub>
- **L540** - Deep Physics Extraction  <br/><sub>↳ before `HashSet<Transform> uniquePbTransforms = new HashSet<Transform>();`</sub>
- **L589** - Sort physics nodes by depth (leaf nodes first for safe culling)  <br/><sub>↳ before `report.PhysicsNodes.Sort((a, b) => GetDepth(b.Component.transform).CompareTo(GetDepth(a.Component.transform)));`</sub>
- **L592** - Dynamic Limit Warning Logic  <br/><sub>↳ before `int maxPb = 32;`</sub>
- **L616** - --- 5. QUEST PIPELINE SYSTEM ---  <br/><sub>↳ before `var mobileShaderWhitelist = new HashSet<string>(VRC.SDKBase.Validation.AvatarValidation.ShaderWhiteList);`</sub>

### `private static bool HasPhysBoneProtection(Transform target, GameObject root)`
<sub>L678–L680</sub>

- **L678** - Re-expresses an axis-aligned bounds (originally in `sourceSpace` local coords) as an  <br/><sub>↳ before `private static Bounds TransformBoundsToSpace(Bounds source, Transform sourceSpace, Transform targetSpace)`</sub>
- **L679** - axis-aligned bounds in `targetSpace` local coords. Walks all 8 corners through both  <br/><sub>↳ before `private static Bounds TransformBoundsToSpace(Bounds source, Transform sourceSpace, Transform targetSpace)`</sub>
- **L680** - transforms; the result encompasses the rotated source AABB. Identity when spaces match.  <br/><sub>↳ before `private static Bounds TransformBoundsToSpace(Bounds source, Transform sourceSpace, Transform targetSpace)`</sub>

### `private static Bounds TransformBoundsToSpace(Bounds source, Transform sourceSpace, Transform targetSpace)`
<sub>L705–L706</sub>

- **L705** - Non-allocating triangle count. Mesh.triangles allocates a full int[] copy on every  <br/><sub>↳ before `private static int CountTriangles(Mesh mesh)`</sub>
- **L706** - access; GetIndexCount returns each submesh's index count with zero allocation.  <br/><sub>↳ before `private static int CountTriangles(Mesh mesh)`</sub>

### `private static int CountTriangles(Mesh mesh)`
<sub>L716–L718</sub>

- **L716** - Runs VRChat's own performance calculator so our rating matches the upload screen  <br/><sub>↳ before `private static void RunOfficialPerformanceScan(GameObject avatarRoot, ValidationReport report)`</sub>
- **L717** - exactly, and surfaces categories (particles, lights, cloth, audio, constraints,  <br/><sub>↳ before `private static void RunOfficialPerformanceScan(GameObject avatarRoot, ValidationReport report)`</sub>
- **L718** - contacts, PhysBone collision checks, etc.) the hardware-cap panel doesn't measure.  <br/><sub>↳ before `private static void RunOfficialPerformanceScan(GameObject avatarRoot, ValidationReport report)`</sub>

### `private static void RunOfficialPerformanceScan(GameObject avatarRoot, ValidationReport report)`
<sub>L741–L758</sub>

- **L741** - Anything above "Info" is a real concern worth surfacing to the user.  <br/><sub>↳ before `if (level != PerformanceInfoDisplayLevel.None && level != PerformanceInfoDisplayLevel.Info)`</sub>
- **L756** - Single policy gate for which textures the resize/optimize pass may touch.  <br/><sub>↳ before `private static bool IsProcessableTexture(Texture tex, out string assetPath)`</sub>
- **L757** - Excludes RenderTextures, assets outside the project's Assets folder, and anything  <br/><sub>↳ before `private static bool IsProcessableTexture(Texture tex, out string assetPath)`</sub>
- **L758** - VixenMagickKit flags as protected (shader-internal textures, .exr/.hdr data, etc.).  <br/><sub>↳ before `private static bool IsProcessableTexture(Texture tex, out string assetPath)`</sub>

### `private static void ProcessTexturesWithMagick(HashSet<Texture> textures, int targetSize, ResizeMode mode)`
<sub>L782–L824</sub>

- **L782** - Skip RenderTextures, out-of-project assets, and protected shader/data  <br/><sub>↳ before `if (!IsProcessableTexture(tex, out string path)) continue;`</sub>
- **L783** - textures (Poiyomi internals, .exr LUTs, etc.) in one policy check.  <br/><sub>↳ before `if (!IsProcessableTexture(tex, out string path)) continue;`</sub>
- **L786** - Cancelable progress bar - essential because Magick.NET resize + sharpen + lossless  <br/><sub>↳ before `if (EditorUtility.DisplayCancelableProgressBar(`</sub>
- **L787** - re-encode can run for minutes on big upscale targets, during which Unity is  <br/><sub>↳ before `if (EditorUtility.DisplayCancelableProgressBar(`</sub>
- **L788** - otherwise frozen with no feedback.  <br/><sub>↳ before `if (EditorUtility.DisplayCancelableProgressBar(`</sub>
- **L803** - Downscale: any dim over target → shrink. Upscale: both dims under target → grow.  <br/><sub>↳ before `bool needsWork = mode == ResizeMode.Downscale`</sub>
- **L810** - Lanczos preserves detail best on downscale. Mitchell avoids ringing on upscale.  <br/><sub>↳ before `img.FilterType = mode == ResizeMode.Downscale ? FilterType.Lanczos : FilterType.Mitchell;`</sub>
- **L815** - Mild sharpening to recover crispness that Mitchell smooths out.  <br/><sub>↳ before `img.AdaptiveSharpen(0, 0.6);`</sub>
- **L823** - TryLosslessOptimize internally drops to single-pass mode for files >10MB  <br/><sub>↳ before `VixenMagickKit.TryLosslessOptimize(path);`</sub>
- **L824** - so this is safe to call regardless of target size.  <br/><sub>↳ before `VixenMagickKit.TryLosslessOptimize(path);`</sub>

### `private PopupField<int> _sizePopup;`
<sub>L851</sub>

- **L851** - Same preset ladder Unity uses for the TextureImporter Max Size dropdown.  <br/><sub>↳ before `private static readonly List<int> SizePresets = new List<int> { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };`</sub>

### `private void OnEnable() => _cyberFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);`
<sub>L869–L871</sub>

- **L869** - ====================================================================  <br/><sub>↳ before `private double _nextScanTime = 0;`</sub>
- **L870** - REACTIVE UI (DEBOUNCED)  <br/><sub>↳ before `private double _nextScanTime = 0;`</sub>
- **L871** - ====================================================================  <br/><sub>↳ before `private double _nextScanTime = 0;`</sub>

### `private bool _scanQueued = false;`
<sub>L876</sub>

- **L876** - Taps into the Editor's frame update loop  <br/><sub>↳ before `private void Update()`</sub>

### `private void Update()`
<sub>L889</sub>

- **L889** - Queues a scan with a 500ms delay to prevent Editor lockup during rapid changes  <br/><sub>↳ before `private void QueueDeepScan()`</sub>

### `private void QueueDeepScan()`
<sub>L892–L900</sub>

- **L892** - Only queue if we already have an active session  <br/><sub>↳ before `if (_lastReport != null && _targetField != null && _targetField.value != null)`</sub>
- **L900** - Triggered when anything in the scene hierarchy is added, deleted, or reparented  <br/><sub>↳ before `private void OnHierarchyChange() => QueueDeepScan();`</sub>

### `private void OnHierarchyChange() => QueueDeepScan();`
<sub>L903</sub>

- **L903** - Triggered when assets are modified (e.g., textures reimported, materials changed)  <br/><sub>↳ before `private void OnProjectChange() => QueueDeepScan();`</sub>

### `private void OnProjectChange() => QueueDeepScan();`
<sub>L906</sub>

- **L906** - Optional Quality-of-Life: Auto-target if you click a new avatar root in the hierarchy  <br/><sub>↳ before `private void OnSelectionChange()`</sub>

### `private void ExecuteDeepScan()`
<sub>L968–L1112</sub>

- **L968** - --- 1. HIERARCHY TOPOLOGY ---  <br/><sub>↳ before `var archPanel = CreateCyberPanel("Hierarchy Topology", "#00e5ff");`</sub>
- **L980** - --- 1.5. OFFICIAL VRCHAT PERFORMANCE (authoritative SDK calculator) ---  <br/><sub>↳ before `if (_lastReport.OfficialOverallRating != null)`</sub>
- **L997** - --- 2. HARDWARE CAP ANALYSIS (Persistent Stats Panel) ---  <br/><sub>↳ before `int maxPb = 32; int maxContacts = 32; int maxAnimators = 2;`</sub>
- **L1032** - --- 3. INTERACTIVE PHYSICS SYSTEM ---  <br/><sub>↳ before `if (_lastReport.PhysicsNodes.Count > 0)`</sub>
- **L1112** - --- 4. OPTIMIZATION SELECTION SYSTEM ---  <br/><sub>↳ before `if (_lastReport.OptimizationSuite.Count > 0)`</sub>

---

## `Editor/Avatar Tools/Avatar Validator/VixenMeshPatcher.cs`

*56 comment(s).*


### `(file scope)`
<sub>L11–L13</sub>

- **L11** - VixForge Core: Advanced Mesh Topology Engine.  <br/><sub>↳ before `public static class VixenMeshPatcher`</sub>
- **L12** - Clones FBX mesh data into memory, applies vertex/bone transformations,  <br/><sub>↳ before `public static class VixenMeshPatcher`</sub>
- **L13** - and serializes the optimized mesh to disk for SDK compilation.  <br/><sub>↳ before `public static class VixenMeshPatcher`</sub>

### `private const string GENERATED_ASSET_PATH = "Assets/VixenTools/Meshes/Patched/";`
<sub>L20</sub>

- **L20** - Executes a destructive patch safely by cloning the mesh, applying an action, and saving it.  <br/><sub>↳ before `public static void PatchSkinnedMesh(SkinnedMeshRenderer smr, string patchLabel, System.Action<Mesh, SkinnedMeshRenderer> patchingLogic)`</sub>

### `public static void PatchSkinnedMesh(SkinnedMeshRenderer smr, string patchLabel, System.Action<Mesh, SkinnedMeshRenderer> patchingLogic)`
<sub>L26–L66</sub>

- **L26** - 1. Memory Clone: Instantiate bypasses the FBX read-only lock.  <br/><sub>↳ before `Mesh clonedMesh = UnityEngine.Object.Instantiate(smr.sharedMesh);`</sub>
- **L30** - 2. Execute Custom Topology System (Vertices, UVs, Bones)  <br/><sub>↳ before `patchingLogic?.Invoke(clonedMesh, smr);`</sub>
- **L33** - 3. Recalculate structural integrity  <br/><sub>↳ before `clonedMesh.RecalculateBounds();`</sub>
- **L38** - 4. Persistence: Step-through folder validation to ensure the path exists  <br/><sub>↳ before `if (!AssetDatabase.IsValidFolder("Assets/VixenTools"))`</sub>
- **L48** - 5. Serialize to Disk  <br/><sub>↳ before `string assetPath = $"{GENERATED_ASSET_PATH}{clonedMesh.name}_{System.Guid.NewGuid().ToString().Substring(0, 5)}.asset";`</sub>
- **L53** - 6. Apply the swap with Undo support  <br/><sub>↳ before `Undo.RecordObject(smr, "Apply Patched Mesh");`</sub>
- **L60** - ====================================================================  <br/><sub>↳ before `public static void WeldVertices(`</sub>
- **L61** - DESTRUCTIVE TOPOLOGY PIPELINES (VERTEX WELDING + BLENDSHAPE RECOVERY)  <br/><sub>↳ before `public static void WeldVertices(`</sub>
- **L62** - ====================================================================  <br/><sub>↳ before `public static void WeldVertices(`</sub>
- **L65** - Precision Microwelder: Seals sub-millimeter splits while strictly preserving UV texture seams.  <br/><sub>↳ before `public static void WeldVertices(`</sub>
- **L66** - Utilizes a 5D Hash System (X, Y, Z, U, V) to ensure rendering integrity is never compromised.  <br/><sub>↳ before `public static void WeldVertices(`</sub>

### `HashSet<int> protectedBones = null)`
<sub>L84–L268</sub>

- **L84** - 1. SURGICAL EXCLUSION SCAN (MATERIALS)  <br/><sub>↳ before `HashSet<int> protectedVertIndices = new HashSet<int>();`</sub>
- **L95** - 2. BLENDSHAPE MEMORY EXTRACTION  <br/><sub>↳ before `int blendShapeCount = mesh.blendShapeCount;`</sub>
- **L118** - 3. THE 5D PRECISION HASH SYSTEM (Zero-GC, UV-Safe)  <br/><sub>↳ before `List<Vector3> newVerts = new List<Vector3>();`</sub>
- **L126** - UPGRADED: 5D Tuple (X, Y, Z, U, V) protects UV texture seams from collapsing.  <br/><sub>↳ before `var spatialHash = new Dictionary<(long, long, long, long, long), int>();`</sub>
- **L130** - Fixed high-precision scale for UV maps (0.0 to 1.0 space)  <br/><sub>↳ on `float uvMultiplier = 10000f;`</sub>
- **L152** - Absolute unique key. i guarantees it never merges.  <br/><sub>↳ before `key = (long.MaxValue, long.MaxValue, long.MaxValue, 0, i);`</sub>
- **L189** - 4. SUBMESH TRIANGLE REBUILD  <br/><sub>↳ before `int subMeshCount = mesh.subMeshCount;`</sub>
- **L214** - 5. TOPOLOGY APPLICATION  <br/><sub>↳ before `mesh.Clear();`</sub>
- **L230** - 6. BLENDSHAPE RE-MAPPING SYSTEM  <br/><sub>↳ before `foreach (var shape in extractedBlendShapes)`</sub>
- **L266** - Vixen Core: Multi-Pass Precision Microwelder.  <br/><sub>↳ before `public static void MultipassTargetedWeld(`</sub>
- **L267** - Iteratively seals spatial seams while STRICTLY locking UV coordinates.  <br/><sub>↳ before `public static void MultipassTargetedWeld(`</sub>
- **L268** - Prioritizes absolute visual integrity over reaching polygon targets.  <br/><sub>↳ before `public static void MultipassTargetedWeld(`</sub>

### `float startThreshold = 0.0001f,`
<sub>L274</sub>

- **L274** - Hard cap at 5mm. Extreme precision only.  <br/><sub>↳ on `float maxThreshold = 0.005f,`</sub>

### `HashSet<int> protectedBones = null)`
<sub>L284–L495</sub>

- **L284** - 1. INITIAL STATE EXTRACTION  <br/><sub>↳ before `Vector3[] originalVerts = mesh.vertices;`</sub>
- **L294** - 2. THE MASTER TRANSLATION MAP  <br/><sub>↳ before `int[] masterMap = new int[originalVerts.Length];`</sub>
- **L298** - 3. BLENDSHAPE MEMORY ISOLATION  <br/><sub>↳ before `int blendShapeCount = mesh.blendShapeCount;`</sub>
- **L322** - 4. THE IN-MEMORY ITERATION SYSTEM (UV-Locked 5D Hash)  <br/><sub>↳ before `int currentTriCount = mesh.triangles.Length / 3;`</sub>
- **L345** - STRICT UV LOCK: Re-introduced U and V to the system. Textures physically cannot tear.  <br/><sub>↳ before `var spatialHash = new Dictionary<(long, long, long, long, long), int>();`</sub>
- **L349** - High-precision UV quantization  <br/><sub>↳ on `float uvMultiplier = 10000f;`</sub>
- **L441** - 5. TOPOLOGY APPLICATION  <br/><sub>↳ before `mesh.Clear();`</sub>
- **L452** - 6. MASTER BLENDSHAPE RE-MAPPING  <br/><sub>↳ before `foreach (var shape in extractedBlendShapes)`</sub>
- **L488** - ====================================================================  <br/><sub>↳ before `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`</sub>
- **L489** - KINEMATIC ISOLATION & PROTECTION SYSTEM  <br/><sub>↳ before `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`</sub>
- **L490** - ====================================================================  <br/><sub>↳ before `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`</sub>
- **L493** - Intelligently maps Humanoid Avatar bones (and their entire descendent hierarchies)  <br/><sub>↳ before `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`</sub>
- **L494** - to specific SMR bone indices. Essential for protecting delicate features like  <br/><sub>↳ before `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`</sub>
- **L495** - hands, face, and jaw from the welder.  <br/><sub>↳ before `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`</sub>

### `public static HashSet<int> GenerateProtectedBoneIndices(Animator animator, SkinnedMeshRenderer smr, params HumanBodyBones[] protectedHumanBones)`
<sub>L507–L540</sub>

- **L507** - 1. Gather all Transforms that need protection (Base bone + all recursive children)  <br/><sub>↳ before `HashSet<Transform> protectedTransforms = new HashSet<Transform>();`</sub>
- **L515** - Traverse and collect the entire hierarchy under this bone  <br/><sub>↳ before `CollectTransformsRecursive(boneTransform, protectedTransforms);`</sub>
- **L524** - 2. Map Physical Transforms to the SMR's internal bone array indices  <br/><sub>↳ before `Transform[] smrBones = smr.bones;`</sub>
- **L539** - Recursively spiders down a transform hierarchy to ensure all child joints  <br/><sub>↳ before `private static void CollectTransformsRecursive(Transform current, HashSet<Transform> collection)`</sub>
- **L540** - (e.g., finger joints, ear pivots, hair roots) are caught in the protection net.  <br/><sub>↳ before `private static void CollectTransformsRecursive(Transform current, HashSet<Transform> collection)`</sub>

### `private static void CollectTransformsRecursive(Transform current, HashSet<Transform> collection)`
<sub>L552–L553</sub>

- **L552** - Evaluates a BoneWeight struct to determine which bone has the highest  <br/><sub>↳ before `private static int GetDominantBone(BoneWeight bw)`</sub>
- **L553** - structural influence over the vertex. Used for kinematic isolation.  <br/><sub>↳ before `private static int GetDominantBone(BoneWeight bw)`</sub>

### `private static int GetDominantBone(BoneWeight bw)`
<sub>L567</sub>

- **L567** - --- Data structures for the BlendShape memory cache ---  <br/><sub>↳ before `private struct BlendShapeExtract`</sub>

### `private struct BlendShapeFrame`
<sub>L582–L587</sub>

- **L582** - ====================================================================  <br/><sub>↳ before `public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)`</sub>
- **L583** - KINEMATIC OPTIMIZATION PIPELINES  <br/><sub>↳ before `public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)`</sub>
- **L584** - ====================================================================  <br/><sub>↳ before `public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)`</sub>
- **L587** - Heuristic to collapse specific bones and transfer their vertex weights to a parent bone.  <br/><sub>↳ before `public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)`</sub>

### `public static void CollapseBonesToParent(SkinnedMeshRenderer smr, List<Transform> bonesToCull)`
<sub>L599</sub>

- **L599** - [Vixen Core Fix] Guard against null joints in dirty SMR bone arrays.  <br/><sub>↳ before `if (currentBones[i] != null)`</sub>

### `private static BoneWeight ProcessWeightChannel(BoneWeight w, int channel, Transform[] allBones, List<Transform> cullList, Dictionary<Transform, int> indexMap)`
<sub>L628</sub>

- **L628** - [Vixen Core Fix] Ensure currentBone is not null before checking hierarchy to prevent NREs  <br/><sub>↳ before `if (currentBone != null && cullList.Contains(currentBone) && currentBone.parent != null)`</sub>

---

## `Editor/Avatar Tools/Badge Maker/VixenBadgeMaker.cs`

*28 comment(s).*


### `(file scope)`
<sub>L15</sub>

- **L15** - JSON Wrapper for dynamic template layouts  <br/><sub>↳ before `[Serializable]`</sub>

### `public float titleRotation = 0f;`
<sub>L24</sub>

- **L24** - Legacy variable preserved for backwards compatibility with old layout.json files  <br/><sub>↳ before `public Color neonColor = Color.white;`</sub>

### `public bool emitTitle = true;`
<sub>L32</sub>

- **L32** - Safety flag for older JSON files  <br/><sub>↳ on `public bool hasUpgradedBools = true;`</sub>

### `private enum TargetShader`
<sub>L45</sub>

- **L45** - <-- Added new shader  <br/><sub>↳ on `FuralityModular`</sub>

### `private ToolMode _currentMode = ToolMode.BadgeGenerator;`
<sub>L50</sub>

- **L50** - --- Shared State ---  <br/><sub>↳ before `private const string VixenRootPath = "Assets/VixenTools/Badges/Template Files";`</sub>

### `private Font _cyberFont;`
<sub>L58</sub>

- **L58** - --- Generator State ---  <br/><sub>↳ before `private string _badgeName = "";`</sub>

### `private bool _showAdvancedLayout = false;`
<sub>L89</sub>

- **L89** - --- Template Builder State ---  <br/><sub>↳ before `private AuthoringType _authoringType = AuthoringType.IngestFromSource;`</sub>

### `private Texture2D _sourceEmission;`
<sub>L97</sub>

- **L97** - --- UV Mapper State ---  <br/><sub>↳ before `public bool IsMappingActive { get; private set; } = false;`</sub>

### `private int _lastPixelY = 0;`
<sub>L110</sub>

- **L110** - --- UI Elements ---  <br/><sub>↳ before `private VisualElement _generatorContainer;`</sub>

### `private void BuildGeneratorUI(VisualElement container)`
<sub>L299–L315</sub>

- **L299** - COLOR CONTROLS  <br/><sub>↳ before `var colorLabel = new Label("Material & Map Colors") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } };`</sub>
- **L315** - PIPELINE PROCESS CONTROLS  <br/><sub>↳ before `var processLabel = new Label("Pipeline Processing") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } };`</sub>

### `private void LoadLayoutConfig(string folderPath)`
<sub>L694</sub>

- **L694** - Fallback safety to prevent old templates from disabling glows  <br/><sub>↳ before `if (!layout.hasUpgradedBools)`</sub>

### `private string GetShaderDisplayName(TargetShader shader)`
<sub>L809</sub>

- **L809** - <-- Added  <br/><sub>↳ on `case TargetShader.FuralityModular: return "Furality Modular (Ultra)";`</sub>

### `private string GetShaderString(TargetShader shader)`
<sub>L826</sub>

- **L826** - <-- Added  <br/><sub>↳ on `case TargetShader.FuralityModular: return "Furality/Modular/Standard";`</sub>

### `private void GenerateBadgeEndToEnd()`
<sub>L900–L911</sub>

- **L900** - Hardcode Alpha channel to 65535 (fully opaque) to prevent invisible text renders  <br/><sub>↳ before `MagickColor mMainText = new MagickColor((ushort)(_mainTextColor.r * 65535), (ushort)(_mainTextColor.g * 65535), (ushort)(_mainTextColor.b * 65535), 65535);`</sub>
- **L907** - Generate plates for the DIFFUSE map  <br/><sub>↳ before `using MagickImage nameImg = GenerateTextPlate(fontAbsolutePath, _badgeName, _nameW, _nameH, mMainText, _nameRotation);`</sub>
- **L911** - Generate targeted plates for the EMISSION map based on UI Toggles  <br/><sub>↳ before `using MagickImage nameImgEmi = _emitName ? GenerateTextPlate(fontAbsolutePath, _badgeName, _nameW, _nameH, mEmiText, _nameRotation) : null;`</sub>

### `private MagickImage GenerateTextPlate(string fontPath, string text, int w, int h, MagickColor color, float rotation)`
<sub>L934</sub>

- **L934** - @filename indirection avoids the label: parser choking on ' " ` @ : in user text.  <br/><sub>↳ before `string tempFile = Path.Combine(Path.GetTempPath(), $"vixen_label_{Guid.NewGuid():N}.txt").Replace("\\", "/");`</sub>

### `private void CompositeTexture(string baseTexPath, MagickImage namePlate, MagickImage titlePlate, string outPath, bool applyGrayscale, bool isEmission)`
<sub>L957–L968</sub>

- **L957** - Force alpha channel support for the incoming plate blending  <br/><sub>↳ on `img.HasAlpha = true;`</sub>
- **L966** - CRITICAL ARCHITECTURAL FIX: Flatten the mask to pure black.  <br/><sub>↳ before `using MagickImage blackBg = new MagickImage(MagickColors.Black, img.Width, img.Height);`</sub>
- **L967** - Unity completely ignores Alpha on emission maps and reads raw RGB.  <br/><sub>↳ before `using MagickImage blackBg = new MagickImage(MagickColors.Black, img.Width, img.Height);`</sub>
- **L968** - If Magick saves a cleared background as Transparent White (255,255,255,0), the whole badge blows out.  <br/><sub>↳ before `using MagickImage blackBg = new MagickImage(MagickColors.Black, img.Width, img.Height);`</sub>

### `private void ApplyToMaterial(string conventionName, string tierName, string difPath, string emiPath)`
<sub>L1049</sub>

- **L1049** - CRITICAL FIX: Push explicit user colors to the material properties  <br/><sub>↳ before `if (material.HasProperty("_Color")) material.SetColor("_Color", _matBaseColor);`</sub>

### `private void GenerateFuralityLayouts()`
<sub>L1288–L1318</sub>

- **L1288** - --- Furality Luma ---  <br/><sub>↳ before `GenerateLayout(basePath, "Furality Luma", new BadgeLayout {`</sub>
- **L1295** - --- Furality Umbra ---  <br/><sub>↳ before `GenerateLayout(basePath, "Furality Umbra", new BadgeLayout {`</sub>
- **L1302** - --- Furality Somna ---  <br/><sub>↳ before `ColorUtility.TryParseHtmlString("#ffeead", out Color somnaColor);`</sub>
- **L1310** - --- Furality Sylva ---  <br/><sub>↳ before `ColorUtility.TryParseHtmlString("#66ff00", out Color sylvaColor);`</sub>
- **L1318** - --- Furality Ultra ---  <br/><sub>↳ before `ColorUtility.TryParseHtmlString("#ff00aa", out Color ultraColor);`</sub>

---

## `Editor/Avatar Tools/Physbone Mapper/PhysBoneBlueprint.cs`

*1 comment(s).*


### `(file scope)`
<sub>L9</sub>

- **L9** - VixenTools Core: A blueprint asset to store the exact skeletal paths and presets of an avatar's physics.  <br/><sub>↳ before `public class PhysBoneBlueprint : ScriptableObject`</sub>

---

## `Editor/Avatar Tools/Physbone Mapper/PhysBoneTopologyMapper.cs`

*7 comment(s).*


### `(file scope)`
<sub>L17–L18</sub>

- **L17** - VixForge Editor: Extracts and Injects complete PhysBone architectures across avatars.  <br/><sub>↳ before `public class PhysBoneTopologyMapper : EditorWindow`</sub>
- **L18** - Safely degrades in non-VRChat Unity environments.  <br/><sub>↳ before `public class PhysBoneTopologyMapper : EditorWindow`</sub>

### `public class PhysBoneTopologyMapper : EditorWindow`
<sub>L22</sub>

- **L22** - Centralized styling paths  <br/><sub>↳ before `private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";`</sub>

### `private void CreateGUI()`
<sub>L53–L65</sub>

- **L53** - Load USS  <br/><sub>↳ before `var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);`</sub>
- **L58** - --- HEADER ---  <br/><sub>↳ before `var headerRect = new VisualElement { name = "tool-header" };`</sub>
- **L65** - --- SCROLL CONTENT ---  <br/><sub>↳ before `var scrollContainer = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>

### `private void ExtractTopology()`
<sub>L140</sub>

- **L140** - Create the Blueprint Asset  <br/><sub>↳ before `PhysBoneBlueprint blueprint = ScriptableObject.CreateInstance<PhysBoneBlueprint>();`</sub>

---

## `Editor/Avatar Tools/Quest Converter/QuestConversionEngine.cs`

*37 comment(s).*


### `(file scope)`
<sub>L22–L23</sub>

- **L22** - VixForge Core: Non-destructive Quest material and hierarchy conversion engine.  <br/><sub>↳ before `public class QuestConversionEngine : EditorWindow`</sub>
- **L23** - Maps 100% of VRChat Mobile Performance Limits natively.  <br/><sub>↳ before `public class QuestConversionEngine : EditorWindow`</sub>

### `private GameObject _sourceAvatar;`
<sub>L33</sub>

- **L33** - --- Deep System Scan Data ---  <br/><sub>↳ before `private int _totalTriangles = 0;`</sub>

### `private Dictionary<Texture, Texture> _textureCache = new Dictionary<Texture, Texture>();`
<sub>L54</sub>

- **L54** - Deep Mat Cache  <br/><sub>↳ on `private HashSet<Material> _scannedMaterials = new HashSet<Material>();`</sub>

### `private string _activeTexturesDir;`
<sub>L57</sub>

- **L57** - --- Interactive Topology System State ---  <br/><sub>↳ before `private class TopologyNode`</sub>

### `private class TopologyNode`
<sub>L67</sub>

- **L67** - --- Interactive Texture Processing System State ---  <br/><sub>↳ before `private class TextureNode`</sub>

### `private List<TextureNode> _scannedTextures = new List<TextureNode>();`
<sub>L94</sub>

- **L94** - UI Elements  <br/><sub>↳ before `private VisualElement _dynamicContainer;`</sub>

### `private void AnalyzeHierarchy()`
<sub>L460–L593</sub>

- **L460** - Reset deep cache  <br/><sub>↳ on `_scannedMaterials.Clear();`</sub>
- **L462** - --- 1. GATHER MATERIALS FROM RENDERERS ---  <br/><sub>↳ before `foreach (var smr in _sourceAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))`</sub>
- **L483** - --- 2. GATHER NESTED MATERIALS FROM ANIMATORS ---  <br/><sub>↳ before `foreach (var animator in _sourceAvatar.GetComponentsInChildren<Animator>(true))`</sub>
- **L496** - --- 3. GATHER NESTED MATERIALS FROM VRCFURY ---  <br/><sub>↳ before `foreach (var mono in _sourceAvatar.GetComponentsInChildren<MonoBehaviour>(true))`</sub>
- **L511** - --- 4. EXTRACT TEXTURES FROM ALL COLLECTED MATERIALS ---  <br/><sub>↳ before `HashSet<Texture> uniqueTexs = new HashSet<Texture>();`</sub>
- **L543** - --- COMPONENT SCANNING ---  <br/><sub>↳ before `_scannedAnimators.Clear(); _scannedParticles.Clear(); _scannedTrails.Clear(); _scannedLines.Clear();`</sub>
- **L547** - --- VRCFT & VRCFURY HUNTER-KILLER ---  <br/><sub>↳ before `foreach (var t in _sourceAvatar.GetComponentsInChildren<Transform>(true))`</sub>
- **L552** - 1. Path-based Detection (Standard VRCFT Templates)  <br/><sub>↳ before `#if UNITY_EDITOR`</sub>
- **L554** - FIXED: Standard Unity 2022.3 API for retrieving prefab asset paths  <br/><sub>↳ before `string prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject));`</sub>
- **L562** - 2. Name-based Detection (VRCFury Specific Branches)  <br/><sub>↳ before `if (!isFaceTracking)`</sub>
- **L572** - 3. Component-based Detection (Class & Namespace check)  <br/><sub>↳ before `if (!isFaceTracking)`</sub>
- **L581** - Targeted check for VRCFT, FaceTracking, or adjerry91 namespaces/classes  <br/><sub>↳ before `if (typeName.Contains("vrcft") \|\|`</sub>
- **L584** - Target internal VF nodes  <br/><sub>↳ on `typeName.Contains("vf_ue_vrcft") \|\|`</sub>
- **L593** - If flagged, add to the structural purge system  <br/><sub>↳ before `if (isFaceTracking)`</sub>

### `private void ExecuteConversion()`
<sub>L786–L848</sub>

- **L786** - 1. Swap Standard Renderers  <br/><sub>↳ before `Renderer[] cloneRenderers = questClone.GetComponentsInChildren<Renderer>(true);`</sub>
- **L804** - 2. Deep Reference Swapper for VRCFury scripts via SerializedObject streams  <br/><sub>↳ before `Component[] allComponents = questClone.GetComponentsInChildren<Component>(true);`</sub>
- **L848** - Execute Structural Purge for Face Tracking  <br/><sub>↳ before `ProcessGameObjectPurge(_scannedFaceTracking, questClone);`</sub>

### `private void ProcessGameObjectPurge(List<TopologyNode> nodes, GameObject clone)`
<sub>L885–L893</sub>

- **L885** - Crucial Architecture: Sort descending by depth.  <br/><sub>↳ before `var sortedNodes = nodes.Where(n => !n.keep).OrderByDescending(n => n.depth).ToList();`</sub>
- **L886** - Obliterate leaf nodes before their parents to prevent NullReferenceExceptions mid-loop.  <br/><sub>↳ before `var sortedNodes = nodes.Where(n => !n.keep).OrderByDescending(n => n.depth).ToList();`</sub>
- **L893** - Absolute safety protocol: Never nuke the root avatar object  <br/><sub>↳ before `if (targetTransform != null && targetTransform.gameObject != clone)`</sub>

### `private Texture ProcessAndCloneTexture(Texture sourceTex, bool isNormalMap = false, bool isLinear = false)`
<sub>L976–L1033</sub>

- **L976** - Bypass empty paths and Unity's built-in virtual assets.  <br/><sub>↳ before `if (string.IsNullOrEmpty(sourcePath) \|\| sourcePath.StartsWith("Resources/") \|\| sourcePath.StartsWith("Library/"))`</sub>
- **L977** - Built-in assets ("Resources/unity_builtin_extra", etc.) are already cross-platform optimized.  <br/><sub>↳ before `if (string.IsNullOrEmpty(sourcePath) \|\| sourcePath.StartsWith("Resources/") \|\| sourcePath.StartsWith("Library/"))`</sub>
- **L983** - Never rewrite shader-internal or HDR data textures (Poiyomi fallback LUTs, .exr  <br/><sub>↳ before `if (VixenMagickKit.IsProtectedAsset(sourcePath))`</sub>
- **L984** - reflection probes, etc.). Resizing them corrupts the source shader and triggers a  <br/><sub>↳ before `if (VixenMagickKit.IsProtectedAsset(sourcePath))`</sub>
- **L985** - Unity reimport storm. Pass the original reference through untouched.  <br/><sub>↳ before `if (VixenMagickKit.IsProtectedAsset(sourcePath))`</sub>
- **L991** - Absolute sanity check: Ensure the file actually exists on disk before we feed it to ImageMagick  <br/><sub>↳ before `if (!File.Exists(sourcePath))`</sub>
- **L1019** - AdaptiveSharpen targets edges and ignores flat areas, so it sharpens detail  <br/><sub>↳ before `img.AdaptiveSharpen(0, 1.0);`</sub>
- **L1020** - without amplifying noise in skin/hair/background. Visibly crisper than the  <br/><sub>↳ before `img.AdaptiveSharpen(0, 1.0);`</sub>
- **L1021** - previous mild UnsharpMask after a Lanczos downscale.  <br/><sub>↳ before `img.AdaptiveSharpen(0, 1.0);`</sub>
- **L1033** - Hardened fallback: Prevent the Unity GUI popup crash if the file is locked or phantom  <br/><sub>↳ before `if (File.Exists(sourcePath))`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/AnimationWorkbenchWindow.cs`

*56 comment(s).*


### `(file scope)`
<sub>L14–L15</sub>

- **L14** - VixForge Editor: Advanced Animation Curve editor with programmatic  <br/><sub>↳ before `public class AnimationWorkbenchWindow : EditorWindow`</sub>
- **L15** - easing generation, property discovery, and bulk management.  <br/><sub>↳ before `public class AnimationWorkbenchWindow : EditorWindow`</sub>

### `public class AnimationWorkbenchWindow : EditorWindow`
<sub>L19</sub>

- **L19** - UI root  <br/><sub>↳ before `private VisualElement root;`</sub>

### `private const string PackageFontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";`
<sub>L24</sub>

- **L24** - Models  <br/><sub>↳ before `private AnimationClip currentClip;`</sub>

### `private readonly Dictionary<EditorCurveBinding, AnimationCurve> stagedCurves =`
<sub>L32</sub>

- **L32** - UI  <br/><sub>↳ before `private ObjectField clipField;`</sub>

### `private Label statusLabel;`
<sub>L48</sub>

- **L48** - Zoom  <br/><sub>↳ before `private SliderInt zoomSlider;`</sub>

### `private int zoomPercent = 100;`
<sub>L52</sub>

- **L52** - Material property binding helpers  <br/><sub>↳ before `private Button materialPickerButton;`</sub>

### `private MaterialPropertySearchPopup.Entry currentMaterialEntry;`
<sub>L60</sub>

- **L60** - time / sampling  <br/><sub>↳ before `private float startTime = 0f;`</sub>

### `public static void ShowWindow()`
<sub>L72</sub>

- **L72** - Lowered minimum size to allow deep resizing  <br/><sub>↳ on `w.minSize = new Vector2(500, 600);`</sub>

### `private void ConstructUI()`
<sub>L113–L508</sub>

- **L113** - --------------------------------------------------------------------  <br/><sub>↳ before `var headerRect = new VisualElement();`</sub>
- **L114** - SIGNATURE BRANDING HEADER  <br/><sub>↳ before `var headerRect = new VisualElement();`</sub>
- **L115** - --------------------------------------------------------------------  <br/><sub>↳ before `var headerRect = new VisualElement();`</sub>
- **L135** - --------------------------------------------------------------------  <br/><sub>↳ before `var topToolbar = new VisualElement { name = "top-toolbar" };`</sub>
- **L136** - TOP TOOLBAR  <br/><sub>↳ before `var topToolbar = new VisualElement { name = "top-toolbar" };`</sub>
- **L137** - --------------------------------------------------------------------  <br/><sub>↳ before `var topToolbar = new VisualElement { name = "top-toolbar" };`</sub>
- **L140** - FIX: Allow toolbar to wrap  <br/><sub>↳ on `topToolbar.style.flexWrap = Wrap.Wrap;`</sub>
- **L152** - Flex constraints  <br/><sub>↳ on `clipField.style.minWidth = 200;`</sub>
- **L171** - Flex constraints  <br/><sub>↳ on `previewTargetField.style.minWidth = 200;`</sub>
- **L187** - --------------------------------------------------------------------  <br/><sub>↳ before `var mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>
- **L188** - MAIN SCROLL AREA  <br/><sub>↳ before `var mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>
- **L189** - --------------------------------------------------------------------  <br/><sub>↳ before `var mainScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>
- **L198** - --------------------------------------------------------------------  <br/><sub>↳ before `var controlRow = new VisualElement();`</sub>
- **L199** - CONTROLS ROW (Responsive 3-column layout)  <br/><sub>↳ before `var controlRow = new VisualElement();`</sub>
- **L200** - --------------------------------------------------------------------  <br/><sub>↳ before `var controlRow = new VisualElement();`</sub>
- **L203** - FIX: This allows panels to stack if the window is crushed  <br/><sub>↳ on `controlRow.style.flexWrap = Wrap.Wrap;`</sub>
- **L208** - ---------------------------  <br/><sub>↳ before `var selectionBox = new VisualElement { name = "selection-panel" };`</sub>
- **L209** - Selection Panel  <br/><sub>↳ before `var selectionBox = new VisualElement { name = "selection-panel" };`</sub>
- **L210** - ---------------------------  <br/><sub>↳ before `var selectionBox = new VisualElement { name = "selection-panel" };`</sub>
- **L213** - FIX: Baseline width  <br/><sub>↳ on `selectionBox.style.minWidth = 280;`</sub>
- **L214** - FIX: Expand to fill dead space  <br/><sub>↳ on `selectionBox.style.flexGrow = 1;`</sub>
- **L282** - ---------------------------  <br/><sub>↳ before `var bindingBox = new VisualElement { name = "bindings-panel" };`</sub>
- **L283** - BINDINGS PANEL  <br/><sub>↳ before `var bindingBox = new VisualElement { name = "bindings-panel" };`</sub>
- **L284** - ---------------------------  <br/><sub>↳ before `var bindingBox = new VisualElement { name = "bindings-panel" };`</sub>
- **L287** - FIX: Baseline width  <br/><sub>↳ on `bindingBox.style.minWidth = 350;`</sub>
- **L288** - FIX: Give this panel 2x priority when stretching horizontally  <br/><sub>↳ on `bindingBox.style.flexGrow = 2;`</sub>
- **L319** - DESTRUCTIVE ACTION  <br/><sub>↳ before `var deleteSelectedBtn = new Button(DeleteSelectedBindings) { text = "Delete Selected" };`</sub>
- **L328** - Material property row  <br/><sub>↳ before `var materialRow = new VisualElement();`</sub>
- **L373** - Defaults row  <br/><sub>↳ before `var defaultsRow = new VisualElement();`</sub>
- **L376** - Allow wrapping inside the box  <br/><sub>↳ on `defaultsRow.style.flexWrap = Wrap.Wrap;`</sub>
- **L401** - ---------------------------  <br/><sub>↳ before `var actionBox = new VisualElement();`</sub>
- **L402** - ACTION PANEL (VixForge Styled)  <br/><sub>↳ before `var actionBox = new VisualElement();`</sub>
- **L403** - ---------------------------  <br/><sub>↳ before `var actionBox = new VisualElement();`</sub>
- **L406** - FIX: Baseline width  <br/><sub>↳ on `actionBox.style.minWidth = 220;`</sub>
- **L407** - FIX: Expand to fill dead space  <br/><sub>↳ on `actionBox.style.flexGrow = 1;`</sub>
- **L456** - --------------------------------------------------------------------  <br/><sub>↳ before `var zoomRow = new VisualElement();`</sub>
- **L457** - ZOOM ROW  <br/><sub>↳ before `var zoomRow = new VisualElement();`</sub>
- **L458** - --------------------------------------------------------------------  <br/><sub>↳ before `var zoomRow = new VisualElement();`</sub>
- **L476** - --------------------------------------------------------------------  <br/><sub>↳ before `var graphContainer = new VisualElement { name = "curve-graph-container" };`</sub>
- **L477** - GRAPH  <br/><sub>↳ before `var graphContainer = new VisualElement { name = "curve-graph-container" };`</sub>
- **L478** - --------------------------------------------------------------------  <br/><sub>↳ before `var graphContainer = new VisualElement { name = "curve-graph-container" };`</sub>
- **L491** - --------------------------------------------------------------------  <br/><sub>↳ before `timelineRibbon = new TimelineRibbon();`</sub>
- **L492** - TIMELINE  <br/><sub>↳ before `timelineRibbon = new TimelineRibbon();`</sub>
- **L493** - --------------------------------------------------------------------  <br/><sub>↳ before `timelineRibbon = new TimelineRibbon();`</sub>
- **L506** - --------------------------------------------------------------------  <br/><sub>↳ before `var bottomRow = new VisualElement { name = "status-bar" };`</sub>
- **L507** - STATUS BAR  <br/><sub>↳ before `var bottomRow = new VisualElement { name = "status-bar" };`</sub>
- **L508** - --------------------------------------------------------------------  <br/><sub>↳ before `var bottomRow = new VisualElement { name = "status-bar" };`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/CurveGraphView.cs`

*5 comment(s).*


### `private Vector2 _panAnchor;`
<sub>L30</sub>

- **L30** - Cached rendering bounds for mouse hit detection  <br/><sub>↳ before `private float _minV = 0f;`</sub>

### `private void OnMouseDown(MouseDownEvent e)`
<sub>L99–L110</sub>

- **L99** - Middle Click: Pan  <br/><sub>↳ on `if (e.button == 2)`</sub>
- **L105** - Double Left Click: Add Key  <br/><sub>↳ on `else if (e.button == 0 && e.clickCount == 2)`</sub>
- **L110** - Right Click: Delete Key  <br/><sub>↳ on `else if (e.button == 1)`</sub>

### `private void OnGUI()`
<sub>L257</sub>

- **L257** - Cache for hit detection  <br/><sub>↳ before `_minV = minV;`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/CurveOperations.cs`

*12 comment(s).*


### `public static class CurveOperations`
<sub>L9–L13</sub>

- **L9** - Rebuilds a curve so that between [sTime, eTime] it transitions from sVal to eVal  <br/><sub>↳ before `public static AnimationCurve BuildStretchedCurve(`</sub>
- **L10** - with the given easing and intermediate key count.  <br/><sub>↳ before `public static AnimationCurve BuildStretchedCurve(`</sub>
- **L12** - Keys BEFORE sTime are preserved.  <br/><sub>↳ before `public static AnimationCurve BuildStretchedCurve(`</sub>
- **L13** - Keys AFTER eTime are intentionally dropped, so the generated region defines the tail.  <br/><sub>↳ before `public static AnimationCurve BuildStretchedCurve(`</sub>

### `public static AnimationCurve BuildStretchedCurve(`
<sub>L27–L76</sub>

- **L27** - 1. Preserve keys strictly before the edit region  <br/><sub>↳ before `if (original != null && original.keys != null)`</sub>
- **L37** - 2. Insert explicit start key  <br/><sub>↳ before `buffer.Add(new Keyframe(sTime, sVal));`</sub>
- **L40** - 3. Insert intermediate easing keys  <br/><sub>↳ before `if (intermediates > 0)`</sub>
- **L52** - 4. Insert explicit end key  <br/><sub>↳ before `buffer.Add(new Keyframe(eTime, eVal));`</sub>
- **L55** - 5. Sort keys by time  <br/><sub>↳ before `buffer.Sort((a, b) => a.time.CompareTo(b.time));`</sub>
- **L58** - 6. Deduplicate same-time keys (Unity hates identical times)  <br/><sub>↳ before `var dedup = new List<Keyframe>();`</sub>
- **L71** - Prefer the later key (usually the generated easing one)  <br/><sub>↳ before `dedup[dedup.Count - 1] = k;`</sub>
- **L76** - 7. Build final curve and smooth tangents  <br/><sub>↳ before `result.keys = dedup.ToArray();`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/EasingDropdown.cs`

*7 comment(s).*


### `(file scope)`
<sub>L11–L12</sub>

- **L11** - Lightweight easing selector for UI Toolkit, backed by GenericMenu.  <br/><sub>↳ before `public class EasingDropdown : VisualElement`</sub>
- **L12** - No EditorWindow, no HostView issues.  <br/><sub>↳ before `public class EasingDropdown : VisualElement`</sub>

### `public EasingDropdown(EasingFunctions.EaseType defaultValue)`
<sub>L49–L55</sub>

- **L49** - Label shows the currently selected easing  <br/><sub>↳ before `_label = new Label(defaultValue.ToString());`</sub>
- **L55** - Button opens a GenericMenu near the mouse  <br/><sub>↳ before `_button = new Button(OpenPopup)`</sub>

### `private void OpenPopup()`
<sub>L75–L87</sub>

- **L75** - Capture local variable  <br/><sub>↳ before `var captured = opt;`</sub>
- **L83** - Show near mouse; safe in UI Toolkit / editor context  <br/><sub>↳ before `menu.ShowAsContext();`</sub>
- **L87** - Text preview for each easing type  <br/><sub>↳ before `private string RenderPreview(EasingFunctions.EaseType t)`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/MaterialPropertySearchPopup.cs`

*27 comment(s).*


### `public class MaterialPropertySearchPopup : EditorWindow`
<sub>L13</sub>

- **L13** - DATA MODEL =====================================================================  <br/><sub>↳ before `public class Entry`</sub>

### `private ScrollView _scroll;`
<sub>L32–L34</sub>

- **L32** - ================================================================================  <br/><sub>↳ before `public static void Show(`</sub>
- **L33** - SHOW WINDOW  <br/><sub>↳ before `public static void Show(`</sub>
- **L34** - ================================================================================  <br/><sub>↳ before `public static void Show(`</sub>

### `public static void Show(`
<sub>L56–L69</sub>

- **L56** - user cannot shrink too small  <br/><sub>↳ on `wnd.minSize = new Vector2(300, 240);`</sub>
- **L57** - start centered  <br/><sub>↳ on `wnd.position = new Rect(`</sub>
- **L67** - ================================================================================  <br/><sub>↳ before `private void OnEnable()`</sub>
- **L68** - UI BUILD  <br/><sub>↳ before `private void OnEnable()`</sub>
- **L69** - ================================================================================  <br/><sub>↳ before `private void OnEnable()`</sub>

### `private void OnEnable()`
<sub>L76–L110</sub>

- **L76** - ─ Search Field ───────────────────────────────────────  <br/><sub>↳ before `_search = new TextField("Search");`</sub>
- **L82** - Lazy rebuild for performance  <br/><sub>↳ before `EditorApplication.delayCall += RebuildList;`</sub>
- **L87** - ─ ScrollView ─────────────────────────────────────────  <br/><sub>↳ before `_scroll = new ScrollView();`</sub>
- **L93** - ─ Close Row ──────────────────────────────────────────  <br/><sub>↳ before `var closeRow = new VisualElement();`</sub>
- **L104** - Initial build  <br/><sub>↳ before `EditorApplication.delayCall += RebuildList;`</sub>
- **L108** - ================================================================================  <br/><sub>↳ before `private void Filter(string txt)`</sub>
- **L109** - SEARCH FILTER  <br/><sub>↳ before `private void Filter(string txt)`</sub>
- **L110** - ================================================================================  <br/><sub>↳ before `private void Filter(string txt)`</sub>

### `private void Filter(string txt)`
<sub>L129–L131</sub>

- **L129** - ================================================================================  <br/><sub>↳ before `private static string DetectCategory(string prop)`</sub>
- **L130** - CATEGORY DETECTION (Poiyomi Friendly)  <br/><sub>↳ before `private static string DetectCategory(string prop)`</sub>
- **L131** - ================================================================================  <br/><sub>↳ before `private static string DetectCategory(string prop)`</sub>

### `private static string DetectCategory(string prop)`
<sub>L151–L153</sub>

- **L151** - ================================================================================  <br/><sub>↳ before `private void RebuildList()`</sub>
- **L152** - MAIN LIST BUILD (MATERIAL → CATEGORY → PROPERTY)  <br/><sub>↳ before `private void RebuildList()`</sub>
- **L153** - ================================================================================  <br/><sub>↳ before `private void RebuildList()`</sub>

### `private void RebuildList()`
<sub>L165–L196</sub>

- **L165** - Group by material  <br/><sub>↳ before `var mats = _filtered`</sub>
- **L179** - CONTENT MUST BE ADDED TO matFold.contentContainer  <br/><sub>↳ before `var matContainer = matFold.contentContainer;`</sub>
- **L182** - Group categories under each material  <br/><sub>↳ before `var categories = matGroup`</sub>
- **L196** - CONTENT MUST BE ADDED TO catFold.contentContainer  <br/><sub>↳ before `var catContainer = catFold.contentContainer;`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/PreviewEngine.cs`

*1 comment(s).*


### `public void StartPreview(AnimationClip c, float from = 0f)`
<sub>L23</sub>

- **L23** - reset if needed  <br/><sub>↳ on `StopPreview();`</sub>

---

## `Editor/Engine Tools/AnimationWorkbench/TimelineRibbon.cs`

*1 comment(s).*


### `private void OnGUI()`
<sub>L45</sub>

- **L45** - Fix: Allows expanding bounds past the current clip length and prevents 0-length slider collapse.  <br/><sub>↳ before `float maxLen = Mathf.Max(1f, clip.length, start, end);`</sub>

---

## `Editor/Engine Tools/Preset Generator/BulkPresetGenerator.cs`

*25 comment(s).*


### `(file scope)`
<sub>L14–L16</sub>

- **L14** - VixForge Core: A unified pipeline tool that handles both bulk extraction of presets  <br/><sub>↳ before `public class BulkPresetGenerator : EditorWindow`</sub>
- **L15** - from existing assets, and the programmatic authoring of standardized Importer presets  <br/><sub>↳ before `public class BulkPresetGenerator : EditorWindow`</sub>
- **L16** - from scratch using a Phantom Asset architecture.  <br/><sub>↳ before `public class BulkPresetGenerator : EditorWindow`</sub>

### `private ToolMode _currentMode = ToolMode.Extraction;`
<sub>L23</sub>

- **L23** - Centralized styling paths  <br/><sub>↳ before `private const string FontPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/Cyberpunk-Regular.ttf";`</sub>

### `private Font _cyberFont;`
<sub>L29</sub>

- **L29** - --- Shared Configuration ---  <br/><sub>↳ before `private string _outputDirectory = "Assets/VixenTools/GeneratedPresets";`</sub>

### `private string _outputDirectory = "Assets/VixenTools/GeneratedPresets";`
<sub>L32</sub>

- **L32** - --- Extraction Variables ---  <br/><sub>↳ before `private bool _ignoreTransforms = true;`</sub>

### `private string _extractionFilter = "";`
<sub>L38</sub>

- **L38** - --- Authoring Variables (Texture Standards) ---  <br/><sub>↳ before `private string _authoringPresetName = "Global_4K_Texture_Standard";`</sub>

### `private string _authoringFilter = "";`
<sub>L47</sub>

- **L47** - --- UI Elements ---  <br/><sub>↳ before `private Button _btnExtractionTab;`</sub>

### `private void CreateGUI()`
<sub>L74–L102</sub>

- **L74** - Load USS  <br/><sub>↳ before `var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);`</sub>
- **L79** - --- HEADER ---  <br/><sub>↳ before `var headerRect = new VisualElement { name = "tool-header" };`</sub>
- **L86** - --- TABS ---  <br/><sub>↳ before `var tabContainer = new VisualElement { name = "tab-toolbar" };`</sub>
- **L96** - --- SCROLL CONTENT ---  <br/><sub>↳ before `var scrollContainer = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>
- **L102** - --- BUILD UI CONTAINERS ---  <br/><sub>↳ before `_extractionContainer = new VisualElement();`</sub>

### `private void BuildAuthoringUI(VisualElement container)`
<sub>L207</sub>

- **L207** - Default to 4096  <br/><sub>↳ on `if (initialSizeIndex == -1) initialSizeIndex = 2;`</sub>

### `private void ExecuteExtraction()`
<sub>L266–L271</sub>

- **L266** - Batch every CreateAsset into a single import pass. Safe here because nothing in the  <br/><sub>↳ before `AssetDatabase.StartAssetEditing();`</sub>
- **L267** - loop reads a created asset back from disk - the in-memory Preset objects are used  <br/><sub>↳ before `AssetDatabase.StartAssetEditing();`</sub>
- **L268** - directly - so deferring the import does not break anything. CreateAsset still  <br/><sub>↳ before `AssetDatabase.StartAssetEditing();`</sub>
- **L269** - registers each path synchronously, so GenerateUniqueAssetPath stays collision-free.  <br/><sub>↳ before `AssetDatabase.StartAssetEditing();`</sub>
- **L270** - try/finally guarantees StopAssetEditing runs even if a CreateAsset throws, so the  <br/><sub>↳ before `AssetDatabase.StartAssetEditing();`</sub>
- **L271** - editor can never be left in a locked asset-editing state.  <br/><sub>↳ before `AssetDatabase.StartAssetEditing();`</sub>

### `private void ExecuteTextureAuthoring()`
<sub>L311–L339</sub>

- **L311** - 1. Create a "Phantom Asset" (Temporary file to base the importer on)  <br/><sub>↳ before `string phantomPath = "Assets/VixenTools_PhantomTexture.png";`</sub>
- **L313** - Minimal valid PNG header  <br/><sub>↳ on `File.WriteAllBytes(phantomPath, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });`</sub>
- **L316** - 2. Grab the importer and inject our standardized rules  <br/><sub>↳ before `TextureImporter importer = AssetImporter.GetAtPath(phantomPath) as TextureImporter;`</sub>
- **L326** - 3. Rip the configuration into a permanent Preset  <br/><sub>↳ before `Preset newPreset = new Preset(importer);`</sub>
- **L339** - 4. Clean up the Phantom Asset  <br/><sub>↳ before `AssetDatabase.DeleteAsset(phantomPath);`</sub>

---

## `Editor/Scene Tools/Placement Tools/FixSceneData.cs`

*7 comment(s).*


### `(file scope)`
<sub>L10–L11</sub>

- **L10** - VixForge Utility: Forces serialization of lighting data to resolve  <br/><sub>↳ before `public class FixSceneData`</sub>
- **L11** - missing or unlinked lightmap references in the active scene.  <br/><sub>↳ before `public class FixSceneData`</sub>

### `public class FixSceneData`
<sub>L15</sub>

- **L15** - Placed at the root of the VixenTools menu for immediate access  <br/><sub>↳ before `[MenuItem("VixenTools/Scene/Fix Scene Data")]`</sub>

### `public static void FixLightingDataAssignment()`
<sub>L27–L42</sub>

- **L27** - Reference the existing lighting data asset for the active scene  <br/><sub>↳ before `var lightingData = Lightmapping.lightingDataAsset;`</sub>
- **L36** - Re-assigning the asset forces Unity to refresh the serialized reference in the scene file  <br/><sub>↳ before `Lightmapping.lightingDataAsset = lightingData;`</sub>
- **L39** - Mark the scene as 'dirty' so the Editor knows it has unsaved changes  <br/><sub>↳ before `EditorSceneManager.MarkSceneDirty(currentScene);`</sub>
- **L42** - Save the scene and flush all asset changes to disk (Serialization)  <br/><sub>↳ before `bool saveSuccess = EditorSceneManager.SaveScene(currentScene);`</sub>

---

## `Editor/Scene Tools/Placement Tools/SnapToSurface.cs`

*28 comment(s).*


### `(file scope)`
<sub>L9–L11</sub>

- **L9** - VixForge Utility: Enterprise-grade surface snapping, locked to the VRChat Worlds SDK.  <br/><sub>↳ before `[InitializeOnLoad]`</sub>
- **L10** - Features Dual-System Gravity Detection and the new 'Precision Click-to-Place' Camera Raycaster  <br/><sub>↳ before `[InitializeOnLoad]`</sub>
- **L11** - for flawless architectural decorating and shelf placement.  <br/><sub>↳ before `[InitializeOnLoad]`</sub>

### `private const string PRECISION_SNAP_MENU = "VixenTools/Scene/Precision Click-to-Place";`
<sub>L18</sub>

- **L18** - Ctrl+Alt+S  <br/><sub>↳ on `private const string DROP_MENU_PATH = "VixenTools/Scene/Drop to Surface %&s";`</sub>

### `private static bool _precisionPlacementEnabled;`
<sub>L23–L24</sub>

- **L23** - VRChat specific layer mask:  <br/><sub>↳ before `private const int VRC_SNAP_LAYER_MASK = ~((1 << 2) \| (1 << 4) \| (1 << 5) \| (1 << 9) \| (1 << 10) \| (1 << 12) \| (1 << 13));`</sub>
- **L24** - 2: Ignore Raycast | 4: Water | 5: UI | 9: Player | 10: PlayerLocal | 12: UiMenu | 13: Pickup  <br/><sub>↳ before `private const int VRC_SNAP_LAYER_MASK = ~((1 << 2) \| (1 << 4) \| (1 << 5) \| (1 << 9) \| (1 << 10) \| (1 << 12) \| (1 << 13));`</sub>

### `static SnapToSurface()`
<sub>L36–L38</sub>

- **L36** - =================================================================================  <br/><sub>↳ before `[MenuItem(LIVE_SNAP_MENU, priority = 100)]`</sub>
- **L37** - MENU TOGGLES  <br/><sub>↳ before `[MenuItem(LIVE_SNAP_MENU, priority = 100)]`</sub>
- **L38** - =================================================================================  <br/><sub>↳ before `[MenuItem(LIVE_SNAP_MENU, priority = 100)]`</sub>

### `public static void ForceSnapSelection()`
<sub>L71–L73</sub>

- **L71** - =================================================================================  <br/><sub>↳ before `private static void OnSceneGUI(SceneView sceneView)`</sub>
- **L72** - PRECISION CLICK-TO-PLACE (THE SNIPER DOT)  <br/><sub>↳ before `private static void OnSceneGUI(SceneView sceneView)`</sub>
- **L73** - =================================================================================  <br/><sub>↳ before `private static void OnSceneGUI(SceneView sceneView)`</sub>

### `private static void OnSceneGUI(SceneView sceneView)`
<sub>L79–L176</sub>

- **L79** - Take control of the mouse to prevent Unity from box-selecting while we paint  <br/><sub>↳ before `int controlID = GUIUtility.GetControlID(FocusType.Passive);`</sub>
- **L86** - 1. SURGICAL SHIELDING: Disable selected colliders so the raycast doesn't hit the prop you're holding  <br/><sub>↳ before `List<Collider> disabledColliders = new List<Collider>();`</sub>
- **L99** - 2. CAMERA-TO-WORLD MATRIX  <br/><sub>↳ before `RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, VRC_SNAP_LAYER_MASK);`</sub>
- **L114** - 3. RESTORE SHIELDING  <br/><sub>↳ before `foreach (var c in disabledColliders) if (c != null) c.enabled = true;`</sub>
- **L117** - 4. EXECUTE PLACEMENT & UI  <br/><sub>↳ before `if (foundSurface)`</sub>
- **L120** - Draw the cyber-aesthetic UV Mapper dot in the Scene View  <br/><sub>↳ before `Handles.color = new Color(0f, 0.898f, 1f, 0.6f);`</sub>
- **L127** - If user clicks or click-drags (paint mode). Allow Shift to bypass rotation alignment  <br/><sub>↳ before `if ((e.type == EventType.MouseDown \|\| e.type == EventType.MouseDrag) && e.button == 0 &&`</sub>
- **L140** - Temporarily reset rotation/position to calculate the pure local Y bottom offset  <br/><sub>↳ before `Vector3 originalPos = t.position;`</sub>
- **L151** - Tilt the object to match the surface normal while preserving its local spin (yaw)  <br/><sub>↳ before `t.rotation = Quaternion.FromToRotation(t.up, bestHit.normal) * t.rotation;`</sub>
- **L154** - Snap to point and push out along the normal by the bottom offset  <br/><sub>↳ before `t.position = bestHit.point + (bestHit.normal * bottomOffset);`</sub>
- **L159** - Legacy vertical-only drop  <br/><sub>↳ before `bottomOffset = CalculateFeetOffset(t);`</sub>
- **L164** - Prevent Live Snapping from fighting the Precision Snapping  <br/><sub>↳ on `t.hasChanged = false;`</sub>
- **L174** - =================================================================================  <br/><sub>↳ before `private static void OnEditorUpdate()`</sub>
- **L175** - LEGACY LIVE GRAVITY SNAPPING  <br/><sub>↳ before `private static void OnEditorUpdate()`</sub>
- **L176** - =================================================================================  <br/><sub>↳ before `private static void OnEditorUpdate()`</sub>

### `private static void ExecuteGravitySnap(Transform t)`
<sub>L198</sub>

- **L198** - Ignore Raycast  <br/><sub>↳ on `t.gameObject.layer = 2;`</sub>

---

## `Editor/Scene Tools/World Engine/ShaderDictionaryAsset.cs`

*9 comment(s).*


### `public static bool IsGloballyProtected(Shader s)`
<sub>L55–L132</sub>

- **L55** - Allows the Null Material Recovery Protocol to fix missing shaders  <br/><sub>↳ on `if (s == null) return false;`</sub>
- **L59** - === 1. EXPLICIT NAME OVERRIDES ===  <br/><sub>↳ before `if (name == "Particles/Standard Unlit" \|\| name == "Unlit/Color") return true;`</sub>
- **L62** - === 2. NATIVE UNITY & ENVIRONMENT PROTECTION ===  <br/><sub>↳ before `if (name.StartsWith("Skybox/", System.StringComparison.OrdinalIgnoreCase) \|\|`</sub>
- **L63** - Tightly guard structural rendering pipelines but leave "Standard" and "Legacy" exposed  <br/><sub>↳ before `if (name.StartsWith("Skybox/", System.StringComparison.OrdinalIgnoreCase) \|\|`</sub>
- **L83** - Deeply embedded Unity default resources protection  <br/><sub>↳ before `if (path == "Resources/unity_builtin_extra" \|\| path == "Library/unity default resources")`</sub>
- **L94** - === 3. 3RD-PARTY ECOSYSTEM PROTECTION ===  <br/><sub>↳ before `string[] protectedPaths = new string[]`</sub>
- **L97** - PROTECT AUDIOLINK  <br/><sub>↳ on `"Packages/com.llealloo.audiolink/",`</sub>
- **L98** - PROTECT LEGACY AUDIOLINK  <br/><sub>↳ on `"Assets/AudioLink/",`</sub>
- **L132** - === 4. DYNAMIC REGEX PROTECTION ===  <br/><sub>↳ before `if (System.Text.RegularExpressions.Regex.IsMatch(path, @"Packages/com\.acchosen\.vr-stage-lighting/Runtime/Shaders/.*"))`</sub>

---

## `Editor/Scene Tools/World Engine/ShaderDictionaryAssetEditor.cs`

*3 comment(s).*


### `public override void OnInspectorGUI()`
<sub>L42–L56</sub>

- **L42** - --- THE SOFT NUKE & REBUILD BUTTON ---  <br/><sub>↳ before `GUIStyle resetBtnStyle = new GUIStyle(GUI.skin.button);`</sub>
- **L53** - Nuke the current data  <br/><sub>↳ before `dict.shaders.Clear();`</sub>
- **L56** - Smart repopulate: Check the filename to know which default schema to run  <br/><sub>↳ before `string path = AssetDatabase.GetAssetPath(dict);`</sub>

---

## `Editor/Scene Tools/World Engine/VixenEngineStressTest.cs`

*68 comment(s).*


### `public static void GenerateChaos()`
<sub>L32–L69</sub>

- **L32** - --- SCENE MANAGEMENT PROTOCOL ---  <br/><sub>↳ before `Scene activeScene = EditorSceneManager.GetActiveScene();`</sub>
- **L46** - User aborted  <br/><sub>↳ on `return;`</sub>
- **L50** - --- VRC BASE WORLD GENERATION ---  <br/><sub>↳ before `GenerateVRChatBaseArchitecture();`</sub>
- **L55** - Deploy Base Pods (Always Available via Unity/VRCSDK)  <br/><sub>↳ before `CreateStandardPerformanceIssues(root.transform, "1. Performance & Physics Pit", 0, 0);`</sub>
- **L62** - Deploy Third-Party Pods (Verified via Reflection first)  <br/><sub>↳ before `CreateProTVIssues(root.transform, "3. ProTV Logic Sink", 2, 0);`</sub>
- **L69** - New Omni-Chaos Pods  <br/><sub>↳ before `CreateGeometryAndMaterialNightmare(root.transform, "11. Geometry & Material Hell", 0, 2);`</sub>

### `private static void GenerateVRChatBaseArchitecture()`
<sub>L82–L104</sub>

- **L82** - 1. Lighting  <br/><sub>↳ before `if (RenderSettings.sun == null && !GameObject.Find("Directional Light"))`</sub>
- **L93** - 2. Floor  <br/><sub>↳ before `if (!GameObject.Find("Floor"))`</sub>
- **L104** - 3. VRC Scene Descriptor  <br/><sub>↳ before `if (Object.FindObjectOfType<VRCSceneDescriptor>() == null)`</sub>

### `private static Transform DeployPod(Transform root, string name, int x, int z)`
<sub>L123</sub>

- **L123** - 20-meter spread  <br/><sub>↳ on `pod.transform.position = new Vector3(x * 20f, 0, z * 20f);`</sub>

### `private static void CreateStandardPerformanceIssues(Transform root, string name, int x, int z)`
<sub>L131–L213</sub>

- **L131** - --- LIGHTING & PROBES ---  <br/><sub>↳ before `GameObject lightObj = new GameObject("Expensive Realtime Light");`</sub>
- **L142** - Fillrate executioner  <br/><sub>↳ on `pLight.range = 500f;`</sub>
- **L149** - --- PHYSICS DRAG ---  <br/><sub>↳ before `GameObject physObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);`</sub>
- **L156** - Trigger non-convex warning  <br/><sub>↳ on `mc.convex = false;`</sub>
- **L161** - CPU Executioner  <br/><sub>↳ on `rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;`</sub>
- **L168** - --- SAFE MESH CREATION (replace existing block inside CreateStandardPerformanceIssues) ---  <br/><sub>↳ before `if (!File.Exists(TestMeshPath))`</sub>
- **L173** - Create a small valid tetrahedron for collision indices  <br/><sub>↳ before `Vector3[] baseVerts = new Vector3[4];`</sub>
- **L180** - Create a large vertex buffer to simulate heavy vertex count (visual/CPU weight)  <br/><sub>↳ before `Vector3[] verts = new Vector3[66000];`</sub>
- **L188** - Triangles reference the first 4 vertices (valid tetrahedron)  <br/><sub>↳ before `int[] tris = new int[] { 0, 1, 2,  0, 2, 3,  0, 3, 1,  1, 3, 2 };`</sub>
- **L191** - allow large vertex counts  <br/><sub>↳ on `heavyMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;`</sub>
- **L197** - Ensure the asset directory exists before creating the asset  <br/><sub>↳ before `var dir = Path.GetDirectoryName(TestMeshPath);`</sub>
- **L213** - Importer logic: Force Read/Write and disable compression  <br/><sub>↳ before `ModelImporter imp = AssetImporter.GetAtPath(TestMeshPath) as ModelImporter;`</sub>

### `private static void CreateGeometryAndMaterialNightmare(Transform root, string name, int x, int z)`
<sub>L243–L250</sub>

- **L243** - 1 submesh  <br/><sub>↳ on `matBloatObj.AddComponent<MeshFilter>().sharedMesh = loadedMesh;`</sub>
- **L250** - explicitly not static  <br/><sub>↳ on `dynamicObj.isStatic = false;`</sub>

### `private static void CreateLightingAndEnvironmentApocalypse(Transform root, string name, int x, int z)`
<sub>L269–L277</sub>

- **L269** - Massive LOD rendering  <br/><sub>↳ on `terrain.heightmapPixelError = 1f;`</sub>
- **L275** - Everything (double rendering geometry)  <br/><sub>↳ on `cam.cullingMask = -1;`</sub>
- **L277** - Global Lighting Sabotage  <br/><sub>↳ before `Lightmapping.realtimeGI = true;`</sub>

### `private static void CreateUIAndCanvasIssues(Transform root, string name, int x, int z)`
<sub>L297</sub>

- **L297** - Auditor Trigger: Nested Raycaster Bloat  <br/><sub>↳ before `GameObject nestedCanvasObj = new GameObject("Nested GraphicRaycaster");`</sub>

### `private static void CreateProTVIssues(Transform root, string name, int x, int z)`
<sub>L319–L349</sub>

- **L319** - --- TV FIGHTING INSTANCES ---  <br/><sub>↳ before `for (int i = 1; i <= 2; i++)`</sub>
- **L332** - --- SUB-COMPONENT ERRORS ---  <br/><sub>↳ before `System.Type rtgiType = GetTypeSafe("ArchiTech.ProTV.RTGIUpdater");`</sub>
- **L349** - FIX: Explicit byte cast for reflection strictly typed fields  <br/><sub>↳ before `SetField(search, "searchAggressionLevel", (byte)20);`</sub>

### `private static void CreateUmbrellaIssues(Transform root, string name, int x, int z)`
<sub>L405–L423</sub>

- **L405** - Trigger > 15 massive array hitch warning  <br/><sub>↳ on `SetField(toggle, "actions", new int[20]);`</sub>
- **L413** - 2 = COLLIDER (missing physical collider attachment)  <br/><sub>↳ on `SetField(zt, "triggerType", 2);`</sub>
- **L421** - Required to pass unity compile  <br/><sub>↳ on `proxyObj.AddComponent<BoxCollider>();`</sub>
- **L423** - Silent collision failure  <br/><sub>↳ on `SetField(proxy, "eventTarget", null);`</sub>

### `private static void CreateExtrasIssues(Transform root, string name, int x, int z)`
<sub>L443</sub>

- **L443** - Mismatch/empty string out of bounds  <br/><sub>↳ on `SetField(proxy, "parameters", new string[] { "ValidParam", "" });`</sub>

### `private static void CreateVizVidIssues(Transform root, string name, int x, int z)`
<sub>L600–L636</sub>

- **L600** - Multi-Singleton violation  <br/><sub>↳ on `gsObj.AddComponent(gsType);`</sub>
- **L611** - Disconnected handler array  <br/><sub>↳ on `SetField(core, "playerHandlers", new Component[0]);`</sub>
- **L621** - Quest Failure Point  <br/><sub>↳ on `SetField(handler, "fallbackHandler", null);`</sub>
- **L625** - 2D Audio Bleed Risk  <br/><sub>↳ on `audio.spatialBlend = 0f;`</sub>
- **L627** - Topology disconnect  <br/><sub>↳ on `SetField(core2, "audioLink", null);`</sub>
- **L636** - Dead UI layer  <br/><sub>↳ on `SetField(front, "core", null);`</sub>

### `private static void CreateAudioLinkAndLightVolumeIssues(Transform root, string name, int x, int z)`
<sub>L652–L692</sub>

- **L652** - Quest GPU Stall Readback  <br/><sub>↳ on `SetField(al, "audioDataToggle", true);`</sub>
- **L672** - Multi-Manager Tearing  <br/><sub>↳ on `lvMgr.AddComponent(lvMgrType);`</sub>
- **L679** - Impossible sphere sizes  <br/><sub>↳ on `SetField(setup, "LightsBrightnessCutoff", 0.05f);`</sub>
- **L685** - Area Light math sink  <br/><sub>↳ on `SetField(plv, "Type", 2);`</sub>
- **L692** - Strobe warning  <br/><sub>↳ on `SetField(tvgi, "AntiFlickering", false);`</sub>

### `private static void CreateRinvoIssues(Transform root, string name, int x, int z)`
<sub>L712</sub>

- **L712** - Serialization death limit  <br/><sub>↳ on `SetField(rinvo, "poolSize", 500000);`</sub>

### `private static void CreateVideoPipelineIssues(Transform root, string name, int x, int z)`
<sub>L767–L796</sub>

- **L767** - Enable Global Texture  <br/><sub>↳ on `SetField(tv, "enableGSV", true);`</sub>
- **L773** - Set parent so it finds the TV Manager  <br/><sub>↳ on `vpmObj.transform.SetParent(tvObj.transform);`</sub>
- **L779** - Exact 1:1 physical bounds  <br/><sub>↳ on `screenObj.transform.localScale = new Vector3(1, 1, 1);`</sub>
- **L786** - Force GSV missing keyword desync  <br/><sub>↳ on `mat.DisableKeyword("_USEGLOBALTEXTURE");`</sub>
- **L795** - 3D  <br/><sub>↳ on `audioSrc.spatialBlend = 1.0f;`</sub>
- **L796** - Massive Bleed Range  <br/><sub>↳ on `audioSrc.maxDistance = 500f;`</sub>

### `private static void CreateVramNightmare(Transform root, string name, int x, int z)`
<sub>L807–L854</sub>

- **L807** - --- SAFE TEXTURE CREATION & IMPORT ---  <br/><sub>↳ before `if (!File.Exists(TestTexturePath))`</sub>
- **L810** - Ensure directory exists  <br/><sub>↳ before `var texDir = Path.GetDirectoryName(TestTexturePath);`</sub>
- **L820** - Try to write PNG and import, but guard against postprocessor exceptions  <br/><sub>↳ before `try`</sub>
- **L826** - Prefer ImportAsset only if path exists and AssetDatabase is available  <br/><sub>↳ before `AssetDatabase.ImportAsset(TestTexturePath, ImportAssetOptions.ForceUpdate);`</sub>
- **L832** - As a fallback, create an in-memory asset (less persistent) to avoid breaking the generator  <br/><sub>↳ before `AssetDatabase.CreateAsset(nukeTex, TestTexturePath);`</sub>
- **L837** - Configure importer safely  <br/><sub>↳ before `TextureImporter importer = AssetImporter.GetAtPath(TestTexturePath) as TextureImporter;`</sub>
- **L854** - --- VISUAL REPRESENTATION ---  <br/><sub>↳ before `GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);`</sub>

### `private static void SetField(object target, string fieldName, object value)`
<sub>L896–L958</sub>

- **L896** - cannot assign null to non-nullable value type; skip  <br/><sub>↳ before `return;`</sub>
- **L905** - Directly assignable  <br/><sub>↳ before `if (fieldType.IsAssignableFrom(valueType))`</sub>
- **L912** - Enums  <br/><sub>↳ before `if (fieldType.IsEnum)`</sub>
- **L922** - Numeric conversions (int -> byte, etc.)  <br/><sub>↳ before `if (IsNumericType(fieldType) && IsNumericType(valueType))`</sub>
- **L930** - Arrays: try to convert element-wise  <br/><sub>↳ before `if (fieldType.IsArray && valueType.IsArray)`</sub>
- **L951** - Last resort: try Convert.ChangeType for simple conversions  <br/><sub>↳ before `try`</sub>
- **L958** - incompatible - fall through  <br/><sub>↳ on `catch { }`</sub>

---

## `Editor/Scene Tools/World Engine/VixenHeuristicsDashboard.cs`

*22 comment(s).*


### `private HashSet<Texture> _detectedUITextures = new HashSet<Texture>();`
<sub>L18</sub>

- **L18** - 4D-Chess: Static reflection cache. Eliminates the massive AppDomain assembly sweep bottleneck.  <br/><sub>↳ before `private static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();`</sub>

### `private static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();`
<sub>L21</sub>

- **L21** - 4D-Chess: Tuple-keyed Scene Object Cache. Prevents cache poisoning between Active/Inactive queries.  <br/><sub>↳ before `private Dictionary<(Type, bool), UnityEngine.Object[]> _sceneObjectCache = new Dictionary<(Type, bool), UnityEngine.Object[]>();`</sub>

### `private void RenderDashboard()`
<sub>L77–L368</sub>

- **L77** - === BASE VRAM SWEEPS (Memory loads regardless of active state) ===  <br/><sub>↳ before `long texBytes = _detectedTextures.Sum(t => t != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t) : 0);`</sub>
- **L83** - === AUDIOLINK VRAM & COMPUTE ===  <br/><sub>↳ before `long audioLinkBytes = 0;`</sub>
- **L91** - Bulletproof check: Is component enabled AND is the GameObject active?  <br/><sub>↳ before `audioLinkActive = alInstances.Cast<Behaviour>().Count(b => b != null && b.enabled && b.gameObject.activeInHierarchy);`</sub>
- **L106** - === LTCGI VRAM & COMPUTE ===  <br/><sub>↳ before `long ltcgiBytes = 0;`</sub>
- **L142** - Bulletproof filter for active screen compute  <br/><sub>↳ before `if (((Behaviour)adapter).enabled && ((Component)adapter).gameObject.activeInHierarchy)`</sub>
- **L151** - === LIGHTMAP VRAM ===  <br/><sub>↳ before `long lightmapBytes = 0;`</sub>
- **L163** - === LIGHT VOLUMES VRAM ===  <br/><sub>↳ before `long lvBytes = 0;`</sub>
- **L195** - === SCENE METRICS (COMPUTE COSTS) ===  <br/><sub>↳ before `var renderers = GetCachedObjects<Renderer>(false);`</sub>
- **L196** - Unity explicit .enabled AND .activeInHierarchy checks to prevent compute score bloat  <br/><sub>↳ before `var renderers = GetCachedObjects<Renderer>(false);`</sub>
- **L201** - Rigidbodies don't have .enabled, so we strictly check activeInHierarchy  <br/><sub>↳ before `int rigidbodies = GetCachedObjects<Rigidbody>(false).Count(rb => rb != null && rb.gameObject.activeInHierarchy);`</sub>
- **L204** - 4D-Chess: Pre-filter all lights with the absolute truth guard  <br/><sub>↳ before `var activeLightsList = GetCachedObjects<Light>(false).Where(l => l != null && l.enabled && l.gameObject.activeInHierarchy).ToList();`</sub>
- **L214** - === VIDEO PLAYER DETECTION ===  <br/><sub>↳ before `HashSet<Component> logicalPlayers = new HashSet<Component>();`</sub>
- **L259** - === SCREEN DETECTION ===  <br/><sub>↳ before `HashSet<GameObject> uniqueScreens = new HashSet<GameObject>();`</sub>
- **L262** - PROTV SCREENS  <br/><sub>↳ before `Type vpmType = GetTypeSafe("ArchiTech.ProTV.VPManager");`</sub>
- **L284** - AVPRO & IWA SCREENS  <br/><sub>↳ before `Type avproScreenType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoScreen");`</sub>
- **L297** - VIZVID (VVMW) SCREENS  <br/><sub>↳ before `if (vvmwCoreType != null)`</sub>
- **L316** - DEFAULT UNITY SCREENS  <br/><sub>↳ before `if (unityPlayerType != null)`</sub>
- **L331** - === SCENE OBJECT METRICS ===  <br/><sub>↳ before `int activeCameras = GetCachedObjects<Camera>(false).Count(c =>`</sub>
- **L348** - === COMPUTE SCORE ===  <br/><sub>↳ before `float computeScore =`</sub>
- **L368** - === UI PANEL ===  <br/><sub>↳ before `var dash = new VisualElement();`</sub>

---

## `Editor/Scene Tools/World Engine/VixenWorldSpider.cs`

*237 comment(s).*


### `private readonly List<string> _resolutionOptions = new List<string> { "512", "1024", "2048", "4096" };`
<sub>L36</sub>

- **L36** - Font Swap Targets  <br/><sub>↳ before `private TMP_FontAsset _targetTMPFont;`</sub>

### `private HashSet<Texture> _detectedUITextures = new HashSet<Texture>();`
<sub>L51</sub>

- **L51** - <-- NEW: Tracks open UI folders  <br/><sub>↳ on `private HashSet<string> _expandedCategories = new HashSet<string>();`</sub>

### `public UnityEngine.Object Context;`
<sub>L60</sub>

- **L60** - FIX 1: Default to unchecked  <br/><sub>↳ on `public bool IsSelected = false;`</sub>

### `private void CreateGUI()`
<sub>L93–L234</sub>

- **L93** - HEADER  <br/><sub>↳ before `var header = new VisualElement { name = "tool-header", style = { justifyContent = Justify.Center, alignItems = Align.Center, paddingLeft = 0 } };`</sub>
- **L111** - CONTROL PANEL  <br/><sub>↳ before `var controlPanel = new VisualElement();`</sub>
- **L115** - Texture Resolution (replace existing block)  <br/><sub>↳ before `var resRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };`</sub>
- **L120** - Determine initial index from current _targetTextureResolution  <br/><sub>↳ before `int initialIndex = _resolutionOptions.IndexOf(_targetTextureResolution.ToString());`</sub>
- **L122** - fallback to last option  <br/><sub>↳ on `if (initialIndex < 0) initialIndex = Mathf.Clamp(_resolutionOptions.Count - 1, 0, _resolutionOptions.Count - 1);`</sub>
- **L127** - Ensure the dropdown text matches the runtime value  <br/><sub>↳ before `resDropdown.value = _targetTextureResolution.ToString();`</sub>
- **L143** - TMP Font Swap  <br/><sub>↳ before `var fontRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };`</sub>
- **L154** - Legacy Font Swap  <br/><sub>↳ before `var legacyFontRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 15 } };`</sub>
- **L165** - Global Shader Replacer Target  <br/><sub>↳ before `var shaderTargetRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };`</sub>
- **L205** - Target Dictionary (.asset)  <br/><sub>↳ before `var targetDictRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };`</sub>
- **L219** - Whitelist Dictionary (.asset)  <br/><sub>↳ before `var whitelistRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 10 } };`</sub>
- **L234** - Action Buttons  <br/><sub>↳ before `var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };`</sub>

### `private void EnsureDictionariesExist(bool forceRebuild = false)`
<sub>L263–L289</sub>

- **L263** - THE NUKE: Delete existing files if a rebuild is triggered  <br/><sub>↳ before `if (forceRebuild)`</sub>
- **L289** - Force Unity to acknowledge the new files immediately so the UI doesn't hitch  <br/><sub>↳ before `if (forceRebuild) AssetDatabase.Refresh();`</sub>

### `private void RefreshCustomDropdown()`
<sub>L330</sub>

- **L330** - === 4D-CHESS CACHING: Prevents brutal O(N) Scene Sweeps ===  <br/><sub>↳ before `private Dictionary<Type, UnityEngine.Object[]> _sceneObjectCache = new Dictionary<Type, UnityEngine.Object[]>();`</sub>

### `private Dictionary<string, Texture2D> _textureRecoveryCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);`
<sub>L334</sub>

- **L334** - Accepts the 'includeInactive' boolean  <br/><sub>↳ before `private T[] GetCachedObjects<T>(bool includeInactive = true) where T : UnityEngine.Object`</sub>

### `private T[] GetCachedObjects<T>(bool includeInactive = true) where T : UnityEngine.Object`
<sub>L344</sub>

- **L344** - Accepts the 'includeInactive' boolean for the Type-based lookups  <br/><sub>↳ before `private UnityEngine.Object[] GetCachedObjects(Type t, bool includeInactive = true)`</sub>

### `private UnityEngine.Object[] GetCachedObjects(Type t, bool includeInactive = true)`
<sub>L354</sub>

- **L354** - === 4D-CHESS CACHING: Persistent JSON Scene Checksum (Failure-Aware) ===  <br/><sub>↳ before `[Serializable]`</sub>

### `private WorldEngineCache _worldCache = new WorldEngineCache();`
<sub>L376</sub>

- **L376** - Fast lookup maps (RAM only)  <br/><sub>↳ before `private Dictionary<string, AssetRecord> _textureRecordMap = new Dictionary<string, AssetRecord>(StringComparer.OrdinalIgnoreCase);`</sub>

### `private void RecordMeshResult(string guid, string assetPath, bool success)`
<sub>L512–L517</sub>

- **L512** - Keeps schema consistent  <br/><sub>↳ on `rec.lastResolution = _targetTextureResolution;`</sub>
- **L517** - === 4D-CHESS CACHING: Reflection & Background Queue ===  <br/><sub>↳ before `private static Dictionary<(Type, string), System.Reflection.FieldInfo> _fieldCache = new Dictionary<(Type, string), System.Reflection.FieldInfo>();`</sub>

### `private void StartProcessingQueue()`
<sub>L540</sub>

- **L540** - Suspends Unity's file watcher (CRITICAL for I/O speed)  <br/><sub>↳ on `AssetDatabase.StartAssetEditing();`</sub>

### `private void ProcessQueueTick()`
<sub>L546–L571</sub>

- **L546** - Process 2 heavy ImageMagick files per frame to keep the Editor responsive  <br/><sub>↳ on `int perTick = 2;`</sub>
- **L563** - One single refresh for all modified textures  <br/><sub>↳ on `AssetDatabase.Refresh();`</sub>
- **L565** - Save the persistent JSON database  <br/><sub>↳ before `SaveLookupCache();`</sub>
- **L571** - Re-render the UI system to clear the ghosts  <br/><sub>↳ before `InitiateFullSystemScan();`</sub>

### `private void InitiateFullSystemScan()`
<sub>L583–L618</sub>

- **L583** - === INTERNAL ENGINE CALLS ===  <br/><sub>↳ before `LoadLookupCache();`</sub>
- **L584** - <-- Added 05/10/26  <br/><sub>↳ on `LoadLookupCache();`</sub>
- **L594** - === ENGINE ARCHITECTURE AUDITS ===  <br/><sub>↳ before `AuditUdonAndNetwork();`</sub>
- **L598** - <-- Added 05/06/26  <br/><sub>↳ on `AuditTerrainAndEnvironment();`</sub>
- **L605** - === THIRD-PARTY ECOSYSTEM AUDITS ===  <br/><sub>↳ before `AuditNativeVideoPipelines();`</sub>
- **L607** - <-- Added 05/07/26  <br/><sub>↳ on `AuditLightVolumesEcosystem();`</sub>
- **L611** - <-- Added 05/07/26  <br/><sub>↳ on `AuditVizVidEcosystem();`</sub>
- **L612** - <-- Added 05/07/26  <br/><sub>↳ on `AuditRinvoSearchEcosystem();`</sub>
- **L613** - <-- Added 05/07/26  <br/><sub>↳ on `AuditAudioLinkEcosystem();`</sub>
- **L614** - <-- Added 05/09/26  <br/><sub>↳ on `AuditLTCGIPipeline();`</sub>
- **L618** - POP OUT THE NEW HEURISTICS WINDOW  <br/><sub>↳ before `VixenHeuristicsDashboard.Open(_detectedTextures, _detectedMeshes, _detectedAudio, _detectedUITextures);`</sub>

### `private void RenderDiagnosticSystem()`
<sub>L629–L661</sub>

- **L629** - Check if the user had this specific category open before the live refresh  <br/><sub>↳ before `bool isExpanded = _expandedCategories.Contains(category);`</sub>
- **L641** - If it was already expanded from a previous scan, populate it immediately  <br/><sub>↳ before `if (isExpanded)`</sub>
- **L648** - LAZY LOAD DOM: Only build the UI nodes when the user clicks the category open  <br/><sub>↳ before `foldout.RegisterValueChangedCallback(evt => {`</sub>
- **L652** - Memorize state  <br/><sub>↳ on `_expandedCategories.Add(category);`</sub>
- **L661** - Forget state  <br/><sub>↳ on `_expandedCategories.Remove(category);`</sub>

### `private static bool _udonReflectionInitialized = false;`
<sub>L777</sub>

- **L777** - Instance-level cache to deduplicate lookups during a scan  <br/><sub>↳ before `private Dictionary<VRC.Udon.AbstractUdonProgramSource, string> _udonTypeNameCache = new Dictionary<VRC.Udon.AbstractUdonProgramSource, string>();`</sub>

### `private string GetUdonTypeNameSafe(UdonBehaviour udon)`
<sub>L782–L836</sub>

- **L782** - Fast exit if there is no program source to identify  <br/><sub>↳ before `if (udon == null \|\| udon.programSource == null) return string.Empty;`</sub>
- **L785** - 1. O(1) Memoization: Have we already resolved this exact script asset during this scan?  <br/><sub>↳ before `if (_udonTypeNameCache.TryGetValue(udon.programSource, out string cachedName))`</sub>
- **L786** - If you have 500 toggle buttons sharing the same script, 499 of them will instantly return here.  <br/><sub>↳ before `if (_udonTypeNameCache.TryGetValue(udon.programSource, out string cachedName))`</sub>
- **L792** - 2. Initialize AppDomain reflection exactly ONCE per Unity session.  <br/><sub>↳ before `if (!_udonReflectionInitialized)`</sub>
- **L804** - Fail silently  <br/><sub>↳ on `catch (Exception) { }`</sub>
- **L813** - 3. Attempt to invoke the statically cached reflection method  <br/><sub>↳ before `if (_getUdonTypeMethod != null)`</sub>
- **L821** - Let the fallback take over if invocation fails  <br/><sub>↳ on `catch (Exception) { }`</sub>
- **L824** - 4. Heuristic Fallback to the physical program asset name  <br/><sub>↳ before `if (string.IsNullOrEmpty(resolvedName))`</sub>
- **L830** - 5. Cache the final result to protect the CPU on all future iterations  <br/><sub>↳ before `_udonTypeNameCache[udon.programSource] = resolvedName;`</sub>
- **L836** - Struct to hold the validation results  <br/><sub>↳ before `public struct LTCGIValidationReport`</sub>

### `public struct LTCGIValidationReport`
<sub>L845</sub>

- **L845** - The Validator: Safely extracts and checks data without hard-linking to the LTCGI assembly  <br/><sub>↳ before `public LTCGIValidationReport CheckForStaleLTCGIData(Component adapter)`</sub>

### `public LTCGIValidationReport CheckForStaleLTCGIData(Component adapter)`
<sub>L860–L934</sub>

- **L860** - Extract Arrays  <br/><sub>↳ before `var screensField = adapterType.GetField("_Screens", flags);`</sub>
- **L871** - 1. Validate Screens (The Emitters)  <br/><sub>↳ before `if (screens != null && extraData != null)`</sub>
- **L880** - Check for nulls (destroyed objects) or objects that have been moved out of active scenes  <br/><sub>↳ before `if (screenObj == null \|\| !screenObj.activeInHierarchy)`</sub>
- **L883** - Read the ExtraData. If w-component (flags) or color isn't zeroed out, we have a ghost light.  <br/><sub>↳ before `Vector4 data = extraData[i];`</sub>
- **L885** - It's disabled in hierarchy but active in shader memory  <br/><sub>↳ on `if (data.sqrMagnitude > 0.01f)`</sub>
- **L890** - Immediate Mitigation: Zero out the data to kill the light in the shader immediately in editor  <br/><sub>↳ before `extraData[i] = Vector4.zero;`</sub>
- **L896** - 4D Chess: Verify Transform bounds on STATIC screens.  <br/><sub>↳ before `Transform t = transforms[i];`</sub>
- **L897** - Dynamic screens update at runtime, but static screens bake their position.  <br/><sub>↳ before `Transform t = transforms[i];`</sub>
- **L898** - If a static screen moved in the editor, its emission bounds are permanently desynced until a rebuild.  <br/><sub>↳ before `Transform t = transforms[i];`</sub>
- **L913** - 2. Validate Renderers (The Receivers)  <br/><sub>↳ before `if (renderers != null)`</sub>
- **L920** - We only strictly flag NULL renderers (deleted objects).  <br/><sub>↳ before `if (r == null)`</sub>
- **L921** - Disabled renderers (!r.enabled) are fine as Udon logic might toggle them on during gameplay.  <br/><sub>↳ before `if (r == null)`</sub>
- **L934** - The Execution Block: Hooks the validation report into the UI/Auto-Fixer  <br/><sub>↳ before `private void AuditLTCGIPipeline()`</sub>

### `private void AuditLTCGIPipeline()`
<sub>L950–L1089</sub>

- **L950** - === 1. NRE DEADLOCK FIX (BAKE CACHE PURGE) ===  <br/><sub>↳ before `var bakeKeyField = controllerType.GetField("bakeMaterialReset_key", flags);`</sub>
- **L959** - If Unity serialization lost the list reference while bakeInProgress is stuck true  <br/><sub>↳ before `if (isBaking && (keys == null \|\| keys.Equals(null)))`</sub>
- **L967** - Atomically reconstruct the lists in memory to satisfy the SerializedObject  <br/><sub>↳ before `Type matListType = typeof(System.Collections.Generic.List<Material>);`</sub>
- **L985** - === 2. VIDEO PLAYER AUTO-LINKING ===  <br/><sub>↳ before `var videoTexField = controllerType.GetField("VideoTexture", flags);`</sub>
- **L991** - Check if dynamic screens actually exist  <br/><sub>↳ before `var screens = GetCachedObjects(screenType);`</sub>
- **L1005** - Hunt for a valid Video Player CRT  <br/><sub>↳ before `Texture detectedVideoTex = null;`</sub>
- **L1009** - Target A: ProTV  <br/><sub>↳ before `Type protvType = GetTypeSafe("ArchiTech.ProTV.TVManager");`</sub>
- **L1020** - Target B: TXL  <br/><sub>↳ before `if (detectedVideoTex == null) {`</sub>
- **L1053** - === 3. ARRAY FRAGMENTATION / GHOST SCREEN VALIDATION ===  <br/><sub>↳ before `if (adapterType != null)`</sub>
- **L1076** - 4D Chess: Try to invoke the parameterless UpdateMaterials(), if pi changed the signature, fallback to the bool override.  <br/><sub>↳ before `var updateMethodParamless = controllerType.GetMethod("UpdateMaterials", new Type[0]);`</sub>
- **L1089** - --- VIDEO TEXTURE BINDING GUARD ---  <br/><sub>↳ before `var blurCrtField = adapterType.GetField("BlurCRTInput", flags);`</sub>

### `private void AuditNativeVideoPipelines()`
<sub>L1114–L1209</sub>

- **L1114** - PRE-AUDIT: Locate AudioLink Core for connectivity handshake  <br/><sub>↳ before `Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");`</sub>
- **L1119** - === 1. AVPRO NATIVE PIPELINE ===  <br/><sub>↳ before `Type avProType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer");`</sub>
- **L1127** - --- Resolution & Latency Guard ---  <br/><sub>↳ before `var maxResField = avProType.GetField("maximumResolution", flags);`</sub>
- **L1157** - --- AUDIOLINK TOPOLOGY HANDSHAKE ---  <br/><sub>↳ before `if (alCore != null)`</sub>
- **L1183** - === 2. UNITY NATIVE PIPELINE ===  <br/><sub>↳ before `Type unityVideoType = GetTypeSafe("VRC.SDK3.Video.Components.VRCUnityVideoPlayer");`</sub>
- **L1191** - --- Resolution Guard ---  <br/><sub>↳ before `var maxResField = unityVideoType.GetField("maximumResolution", flags);`</sub>
- **L1209** - --- AUDIOLINK TOPOLOGY HANDSHAKE ---  <br/><sub>↳ before `if (alCore != null)`</sub>

### `private void AuditTxlEcosystem()`
<sub>L1240–L1373</sub>

- **L1240** - PRE-AUDIT: Locate AudioLink Core for connectivity handshake  <br/><sub>↳ before `Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");`</sub>
- **L1245** - === 1. BASIC UDON HYGIENE ===  <br/><sub>↳ before `var udonBehaviours = GetCachedObjects<UdonBehaviour>(true);`</sub>
- **L1257** - === 2. TEXEL UTILITY AUDITS ===  <br/><sub>↳ before `Type debugUserListType = GetTypeSafe("Texel.DebugUserList");`</sub>
- **L1285** - === 3. TXL PLAYER -> AUDIOLINK HANDSHAKE ===  <br/><sub>↳ before `if (alCore != null)`</sub>
- **L1318** - === 4. TXL SCREEN MANAGER & CRT ECOSYSTEM ===  <br/><sub>↳ before `Type screenManagerType = GetTypeSafe("Texel.ScreenManager");`</sub>
- **L1373** - === 5. TXL QUEUE + ACCESS CONTROL ECOSYSTEM ===  <br/><sub>↳ before `Type rinvoType = GetTypeSafe("Rinvo.YoutubeSearchManager");`</sub>

### `private void AuditProTVEcosystem()`
<sub>L1421–L1870</sub>

- **L1421** - Locate AudioLink Core for connectivity handshake  <br/><sub>↳ before `var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;`</sub>
- **L1427** - === 1. PROTV TOPOLOGY & AUDIOLINK HANDSHAKE ===  <br/><sub>↳ before `var tvs = GetCachedObjects(proTvType, true);`</sub>
- **L1437** - Fallback check: If no adapter, is AudioLink directly listening to ANY of the TV's speakers?  <br/><sub>↳ before `var alSourceField = audioLinkType.GetField("audioSource", flags);`</sub>
- **L1470** - Verify ProTV AudioLinkAdapter bindings  <br/><sub>↳ before `foreach (var adapter in adapters)`</sub>
- **L1505** - === 2. TV MANAGER CONFIGURATION ===  <br/><sub>↳ before `int globalTextureCount = 0;`</sub>
- **L1542** - Ensure aspect ratio is strictly managed via SerializedObject to survive recompiles  <br/><sub>↳ before `SerializedProperty aspectProp = tvSO.FindProperty("defaultAspectRatio");`</sub>
- **L1544** - Fallback for older versions  <br/><sub>↳ on `if (aspectProp == null) aspectProp = tvSO.FindProperty("aspectRatio");`</sub>
- **L1549** - 1.777777f is ProTV's exact internal default for 16:9  <br/><sub>↳ before `if (tvAspect <= 0f \|\| Math.Abs(tvAspect - 1.777777f) > 0.05f && Math.Abs(tvAspect - 1.333333f) > 0.05f && Math.Abs(tvAspect - 2.333333f) > 0.05f)`</sub>
- **L1573** - Safely query the custom texture and correct sizing to 1920x1080  <br/><sub>↳ before `SerializedProperty customTexProp = tvSO.FindProperty("customTexture");`</sub>
- **L1587** - Flush GPU memory  <br/><sub>↳ on `customTex.Release();`</sub>
- **L1590** - Reallocate  <br/><sub>↳ on `customTex.Create();`</sub>
- **L1605** - === 3. SUB-COMPONENTS & UI ===  <br/><sub>↳ before `Type mediaControlsType = GetTypeSafe("ArchiTech.ProTV.MediaControls");`</sub>
- **L1870** - === 4. UMBRELLA & EXTRAS ===  <br/><sub>↳ before `Type atToggleType = GetTypeSafe("ArchiTech.Umbrella.ATToggle");`</sub>

### `private void AuditIwaSyncEcosystem()`
<sub>L1976–L2124</sub>

- **L1976** - PRE-AUDIT: Locate AudioLink Core for connectivity handshake  <br/><sub>↳ before `Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");`</sub>
- **L1981** - === 1. CORE & RESOLUTION ===  <br/><sub>↳ before `Type iwaType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");`</sub>
- **L2006** - === 2. NETWORK QUEUE (PLAYLISTS) ===  <br/><sub>↳ before `Type playlistType = GetTypeSafe("HoshinoLabs.IwaSync3.Playlist");`</sub>
- **L2031** - === 3. AUDIO TOPOLOGY & AUDIOLINK HANDSHAKE ===  <br/><sub>↳ before `Type speakerType = GetTypeSafe("HoshinoLabs.IwaSync3.Speaker");`</sub>
- **L2043** - Spatialization Check  <br/><sub>↳ before `var spatializeField = speakerType.GetField("spatialize", flags);`</sub>
- **L2060** - Extract AudioSource for Handshake  <br/><sub>↳ before `AudioSource speakerSource = component.GetComponent<AudioSource>();`</sub>
- **L2075** - Execute AudioLink Handshake  <br/><sub>↳ before `if (alCore != null && speakers.Length > 0 && !isAudioLinkConnected && firstValidSpeakerSource != null)`</sub>
- **L2089** - === 4. SCREEN SHADERS & RENDER TARGETS ===  <br/><sub>↳ before `Type screenType = GetTypeSafe("HoshinoLabs.IwaSync3.Screen");`</sub>
- **L2124** - === 5. CORE UDON LOGIC & INSTANTIATION TIMING ===  <br/><sub>↳ before `Type videoCoreType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoCore");`</sub>

### `private void AuditVizVidEcosystem()`
<sub>L2190–L2335</sub>

- **L2190** - Soft-dependency resolution  <br/><sub>↳ before `Type vvmwCoreType = GetTypeSafe("JLChnToZ.VRC.VVMW.Core");`</sub>
- **L2204** - 1. Singleton Enforcement: Global Settings  <br/><sub>↳ before `if (vvmwGlobalSettingsType != null)`</sub>
- **L2221** - 2. Audit Player Handlers & Cross-Platform Fallbacks  <br/><sub>↳ before `var handlersField = vvmwCoreType.GetField("playerHandlers", flags);`</sub>
- **L2234** - Scan for Android/Quest compatibility gaps  <br/><sub>↳ before `foreach(var handler in handlers)`</sub>
- **L2256** - 3. Audio Spatialization  <br/><sub>↳ before `var audioSourcesField = vvmwCoreType.GetField("audioSources", flags);`</sub>
- **L2279** - 4. AUDIOLINK TOPOLOGY HANDSHAKE  <br/><sub>↳ before `var alRefField = vvmwCoreType.GetField("audioLink", flags);`</sub>
- **L2296** - 5. Material Color Space & Shader Compatibility  <br/><sub>↳ before `var screenTargetsField = vvmwCoreType.GetField("screenTargets", flags);`</sub>
- **L2323** - 6. Rate Limiter Validation  <br/><sub>↳ before `if (vvmwRateLimitType != null)`</sub>
- **L2335** - 7. Interface Decoupling Checks (Orphaned UI/Frontends)  <br/><sub>↳ before `if (vvmwFrontendType != null)`</sub>

### `private void AuditAudioLinkEcosystem()`
<sub>L2379–L2536</sub>

- **L2379** - Core Type Definitions  <br/><sub>↳ before `Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");`</sub>
- **L2385** - 1. DATA VORTEX: FIND THE CORE  <br/><sub>↳ before `var alInstances = audioLinkType != null ? GetCachedObjects(audioLinkType, true) : null;`</sub>
- **L2399** - --- PIPELINE SYNC: VIDEO PLAYER -> AUDIOLINK ---  <br/><sub>↳ before `var sourceField = audioLinkType.GetField("audioSource", flags);`</sub>
- **L2405** - Scan for VizVid (VVMW) Master Source  <br/><sub>↳ before `if (vvmwCoreType != null)`</sub>
- **L2416** - Scan for ProTV Master Source (Fallback)  <br/><sub>↳ before `if (detectedMasterSource == null && proTvType != null)`</sub>
- **L2419** - FIX: Explicitly cast the returned UnityEngine.Object to a Component  <br/><sub>↳ before `var tv = FindObjectOfType(proTvType, true) as Component;`</sub>
- **L2423** - ProTV stores speakers in VPManagers, but often has a main audio source  <br/><sub>↳ before `detectedMasterSource = tv.GetComponentInChildren<AudioSource>();`</sub>
- **L2440** - --- PERFORMANCE: QUEST READBACK CHECK ---  <br/><sub>↳ before `var readbackField = audioLinkType.GetField("audioDataToggle", flags);`</sub>
- **L2456** - 2. SHADER PROBE: POIYOMI / LILTOON  <br/><sub>↳ before `var sceneMaterials = GetCachedObjects<Renderer>(true)`</sub>
- **L2457** - We use the scene-scraped materials from AuditGeometryAndMaterials for efficiency  <br/><sub>↳ before `var sceneMaterials = GetCachedObjects<Renderer>(true)`</sub>
- **L2468** - Poiyomi Detection  <br/><sub>↳ before `if (sName.Contains("Poiyomi") && mat.HasProperty("_AudioLinkEnable"))`</sub>
- **L2479** - lilToon Detection  <br/><sub>↳ before `if (sName.Contains("lilToon") && mat.HasProperty("_AudioLink"))`</sub>
- **L2491** - 3. SCRIPT PROBE: VRSL / LTCGI / VVMW  <br/><sub>↳ before `Type vrslAdapterType = GetTypeSafe("VRSL.AudioLinkAdapter.VRSL_AudioLinkAdapter");`</sub>
- **L2493** - VRSL Check  <br/><sub>↳ before `Type vrslAdapterType = GetTypeSafe("VRSL.AudioLinkAdapter.VRSL_AudioLinkAdapter");`</sub>
- **L2503** - LTCGI Check  <br/><sub>↳ before `Type ltcgiControllerType = GetTypeSafe("LTCGI.LTCGI_Controller");`</sub>
- **L2510** - 1 = AL Mode  <br/><sub>↳ on `if (alInput != null && (int)alInput.GetValue(ltcgi) == 1 && !coreExists)`</sub>
- **L2517** - VizVid (VVMW) Internal Reference Check  <br/><sub>↳ before `if (vvmwCoreType != null && coreExists)`</sub>
- **L2536** - 4. NATIVE REACTIVE ORPHANS  <br/><sub>↳ before `if (reactiveType != null)`</sub>

### `private void AuditRinvoSearchEcosystem()`
<sub>L2567–L2870</sub>

- **L2567** - Fetch Core Fields  <br/><sub>↳ before `var uiControllerField = rinvoType.GetField("VideoPlayerUIController", flags);`</sub>
- **L2576** - === 1. MISSING REFERENCES & AUTO-LINKING ===  <br/><sub>↳ before `if (currentUiController == null \|\| currentUrlInput == null)`</sub>
- **L2584** - Attempt A: ProTV 3  <br/><sub>↳ before `Type protvUrlInputType = GetTypeSafe("ArchiTech.ProTV.UrlInput");`</sub>
- **L2593** - VideoPlayerType.ProTV3  <br/><sub>↳ on `detectedEnum = 2;`</sub>
- **L2598** - Attempt B: IwaSync3  <br/><sub>↳ before `if (detectedUi == null)`</sub>
- **L2609** - VideoPlayerType.IwaSync3  <br/><sub>↳ on `detectedEnum = 3;`</sub>
- **L2615** - Attempt C: TXL (Texel) Input Proxy  <br/><sub>↳ before `if (detectedUi == null)`</sub>
- **L2631** - VideoPlayerType.Other (TXL hooks natively)  <br/><sub>↳ on `detectedEnum = 6;`</sub>
- **L2637** - Attempt D: USharpVideo  <br/><sub>↳ before `if (detectedUi == null)`</sub>
- **L2652** - VideoPlayerType.USharpVideo  <br/><sub>↳ on `detectedEnum = 0;`</sub>
- **L2688** - === 2. ARCHITECTURAL DECOUPLING (CORE VS UI LAYER) ===  <br/><sub>↳ before `Type protvTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");`</sub>
- **L2761** - === 3. ENUM / TARGET MISMATCH LOGIC ===  <br/><sub>↳ before `if (uiName.Contains("UrlInput") && currentPlayerType != 2) { expectedEnum = 2; expectedName = "ProTV 3"; }`</sub>
- **L2778** - === 4. TEXEL (TXL) CONFLICT RESOLUTION ===  <br/><sub>↳ before `if (uiName.IndexOf("InputProxy", StringComparison.OrdinalIgnoreCase) >= 0 \|\|`</sub>
- **L2806** - "ALWAYS USE QUEUE" UX MISMATCH (Align Rinvo to TXL)  <br/><sub>↳ before `if (alwaysQ && usingQ && !onlyQ)`</sub>
- **L2820** - === 5. UNITY BASE / USHARPVIDEO CONFLICT RESOLUTION ===  <br/><sub>↳ before `if (expectedEnum == 0 \|\| uiName.Contains("USharpVideo"))`</sub>
- **L2837** - === 6. FALLBACK QUEUE AUTO-LINKING (For ProTV / Non-TXL) ===  <br/><sub>↳ before `var usingQueueFieldCheck = rinvoType.GetField("UsingQueue", flags);`</sub>
- **L2846** - Note: TXL Queue linking is already handled comprehensively in Step 4A. This is a fallback for ProTV  <br/><sub>↳ before `Type protvQueueType = GetTypeSafe("ArchiTech.ProTV.Queue");`</sub>
- **L2870** - === 7. POOL SIZE BOUNDS CHECKS ===  <br/><sub>↳ before `var poolSizeFieldCheck = rinvoType.GetField("poolSize", flags);`</sub>

### `private void AuditLightVolumesEcosystem()`
<sub>L2903–L2991</sub>

- **L2903** - 1. Manager Integrity  <br/><sub>↳ before `Type managerType = GetTypeSafe("VRCLightVolumes.LightVolumeManager");`</sub>
- **L2916** - 2. Setup Thresholds & Bounding Spheres  <br/><sub>↳ before `Type setupType = GetTypeSafe("VRCLightVolumes.LightVolumeSetup");`</sub>
- **L2941** - 3. Point Light Compute Loads  <br/><sub>↳ before `Type plvType = GetTypeSafe("VRCLightVolumes.PointLightVolume");`</sub>
- **L2952** - 2 corresponds to AreaLight in the Enum  <br/><sub>↳ on `if (typeVal == 2)`</sub>
- **L2962** - 4. TVGI Integration & Strobe Safety  <br/><sub>↳ before `Type tvgiType = GetTypeSafe("VRCLightVolumes.LightVolumeTVGI");`</sub>
- **L2991** - 5. AudioLink Strobe Safety  <br/><sub>↳ before `Type alType = GetTypeSafe("VRCLightVolumes.LightVolumeAudioLink");`</sub>

### `private void AuditUdonAndNetwork()`
<sub>L3024–L3113</sub>

- **L3024** - === C# SOURCE-OF-TRUTH CROSS-REFERENCE ===  <br/><sub>↳ before `bool isDeclaredNoSync = false;`</sub>
- **L3036** - 4D Chess: UdonSharp API volatility protection.  <br/><sub>↳ before `var propInfo = udbSyncModeAttrType.GetProperty("behaviourSyncMode", BindingFlags.Public \| BindingFlags.Instance);`</sub>
- **L3037** - We check for both a Property and a Field, as internal implementations shift between SDK versions.  <br/><sub>↳ before `var propInfo = udbSyncModeAttrType.GetProperty("behaviourSyncMode", BindingFlags.Public \| BindingFlags.Instance);`</sub>
- **L3059** - === CONTINUOUS SYNC HEURISTICS ===  <br/><sub>↳ before `if (udon.SyncMethod == VRC.SDKBase.Networking.SyncType.Continuous)`</sub>
- **L3068** - 4 is the integer value for SyncType.None in modern VRChat SDKs.  <br/><sub>↳ before `udon.SyncMethod = (VRC.SDKBase.Networking.SyncType)4;`</sub>
- **L3069** - We cast to prevent compiler errors on older SDKs where 'None' didn't exist in the enum.  <br/><sub>↳ before `udon.SyncMethod = (VRC.SDKBase.Networking.SyncType)4;`</sub>
- **L3076** - Only flag actual Continuous syncs if they lack physical movement components  <br/><sub>↳ before `bool hasPhysics = udon.GetComponent<Rigidbody>() != null;`</sub>
- **L3089** - === COMPUTE INSTRUCTION HEURISTICS ===  <br/><sub>↳ before `if (udon.programSource is UdonSharpProgramAsset uAsset && getUasm != null && cache != null)`</sub>
- **L3095** - Micro-optimization: Avoid .Split allocating a massive string array for heavy scripts  <br/><sub>↳ before `int count = 0;`</sub>
- **L3113** - Ensure we don't flag static objects just because they have VRCObjectSync attached  <br/><sub>↳ before `var rb = objSync.GetComponent<Rigidbody>();`</sub>

### `private void AuditLightingAndCameras()`
<sub>L3124–L3221</sub>

- **L3124** - === LIGHTING ===  <br/><sub>↳ before `foreach (var light in GetCachedObjects<Light>(true))`</sub>
- **L3129** - 4D-CHESS FIX: Ignore lights that are explicitly disabled in the hierarchy or component  <br/><sub>↳ before `if (!light.enabled \|\| !light.gameObject.activeInHierarchy) continue;`</sub>
- **L3164** - === REFLECTION PROBES ===  <br/><sub>↳ before `foreach (var probe in GetCachedObjects<ReflectionProbe>(true))`</sub>
- **L3169** - 4D-CHESS FIX: Ignore disabled reflection probes to prevent false positives  <br/><sub>↳ before `if (!probe.enabled \|\| !probe.gameObject.activeInHierarchy) continue;`</sub>
- **L3197** - === CAMERAS ===  <br/><sub>↳ before `foreach (var cam in GetCachedObjects<Camera>(true))`</sub>
- **L3204** - 1. Skip VRChat's safe reference cameras  <br/><sub>↳ before `if (cam.name == "VRCCam" \|\| cam.gameObject.tag == "MainCamera") continue;`</sub>
- **L3207** - 2. Skip cameras that are physically disabled  <br/><sub>↳ before `if (!cam.gameObject.activeInHierarchy \|\| !cam.enabled) continue;`</sub>
- **L3210** - 3. UI Event Camera Protection  <br/><sub>↳ before `bool isEventCamera = cam.cullingMask == 0;`</sub>
- **L3211** - If the Culling Mask is 0 ("Nothing"), it renders no geometry.  <br/><sub>↳ before `bool isEventCamera = cam.cullingMask == 0;`</sub>
- **L3212** - It is functionally safe and operates purely for UI Raycasts.  <br/><sub>↳ before `bool isEventCamera = cam.cullingMask == 0;`</sub>
- **L3215** - If it renders to the screen (no target texture) and actually draws geometry...  <br/><sub>↳ before `if (cam.targetTexture == null && !isEventCamera)`</sub>
- **L3221** - Safely disable the component rather than the GameObject  <br/><sub>↳ before `Undo.RecordObject(cam, "Disable Rogue Camera");`</sub>

### `private void AuditPhysics()`
<sub>L3239–L3262</sub>

- **L3239** - === COLLIDERS ===  <br/><sub>↳ before `foreach (var collider in GetCachedObjects<MeshCollider>(true))`</sub>
- **L3262** - === RIGIDBODIES ===  <br/><sub>↳ before `foreach (var rb in GetCachedObjects<Rigidbody>(true))`</sub>

### `private void AuditTerrainAndEnvironment()`
<sub>L3282–L3388</sub>

- **L3282** - === TERRAINS ===  <br/><sub>↳ before `foreach (var terrain in GetCachedObjects<Terrain>(true))`</sub>
- **L3285** - Null-safety check for objects in transition  <br/><sub>↳ before `if (terrain == null) continue;`</sub>
- **L3323** - === GLOBAL ILLUMINATION & LIGHTMAPS ===  <br/><sub>↳ before `if (Lightmapping.realtimeGI)`</sub>
- **L3340** - 4D-CHESS FIX: Unity 2022+ throws an exception here if no asset is assigned.  <br/><sub>↳ before `LightingSettings lightingSettings = null;`</sub>
- **L3348** - Explicitly swallow API exception to prevent scan interruption  <br/><sub>↳ before `}`</sub>
- **L3388** - === 4D-CHESS CACHING: Prevents AssetDatabase spam during material loops ===  <br/><sub>↳ before `private HashSet<string> _failedTextureSearches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);`</sub>

### `private void AttemptTextureRecovery(Material mat)`
<sub>L3395–L3431</sub>

- **L3395** - Strip common Unity suffixes to find the true root name (e.g., "Floor01 (Instance)" -> "Floor01")  <br/><sub>↳ before `string baseName = mat.name.Replace(" (Instance)", "").Replace("_Mat", "").Replace("_Material", "").Trim();`</sub>
- **L3398** - The Omni-Schema: Maps common Shader Property Names -> Suffixes to search for on disk  <br/><sub>↳ before `var recoverySchema = new Dictionary<string[], string[]>`</sub>
- **L3401** - Core PBR / Diffuse  <br/><sub>↳ before `{ new[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_ColorMap" }, new[] { "_BaseMap", "_Albedo", "_Color", "_Diffuse", "_Main", "_Base" } },`</sub>
- **L3403** - Normal / Bump  <br/><sub>↳ before `{ new[] { "_BumpMap", "_NormalMap", "_Normal" }, new[] { "_Normal", "_NormalMap", "_Bump", "_Nrm", "_NRM" } },`</sub>
- **L3405** - Metallic / Smoothness / Roughness / Masks  <br/><sub>↳ before `{ new[] { "_MetallicGlossMap", "_MaskMap", "_SpecGlossMap", "_MetallicMap", "_RoughnessMap" }, new[] { "_MaskMap", "_Metallic", "_Smoothness", "_Specular", "_Roughness", "_Mask", "_Rgh", "_Met" } },`</sub>
- **L3407** - Emission / Glow  <br/><sub>↳ before `{ new[] { "_EmissionMap", "_Emissive", "_Emission" }, new[] { "_Emission", "_Emissive", "_Glow", "_Illum" } },`</sub>
- **L3409** - Ambient Occlusion  <br/><sub>↳ before `{ new[] { "_OcclusionMap", "_AmbientOcclusionMap", "_AO" }, new[] { "_AO", "_Occlusion", "_AmbientOcclusion" } },`</sub>
- **L3411** - Height / Parallax  <br/><sub>↳ before `{ new[] { "_ParallaxMap", "_HeightMap" }, new[] { "_Height", "_HeightMap", "_Parallax", "_Displacement" } },`</sub>
- **L3413** - --- POIYOMI / TOON SPECIFIC ---  <br/><sub>↳ before `{ new[] { "_MatcapTex", "_Matcap", "_MatcapTexture", "_Matcap1", "_Matcap2" }, new[] { "_Matcap", "_MC", "_MatcapTex" } },`</sub>
- **L3414** - Matcaps  <br/><sub>↳ before `{ new[] { "_MatcapTex", "_Matcap", "_MatcapTexture", "_Matcap1", "_Matcap2" }, new[] { "_Matcap", "_MC", "_MatcapTex" } },`</sub>
- **L3417** - Shadows / Ramps  <br/><sub>↳ before `{ new[] { "_ShadowTex", "_ShadowMap", "_ShadowRamp" }, new[] { "_Shadow", "_ShadowMap", "_Ramp" } },`</sub>
- **L3420** - Outlines  <br/><sub>↳ before `{ new[] { "_OutlineTexture", "_OutlineTex" }, new[] { "_Outline", "_OutlineTex" } },`</sub>
- **L3423** - Fur (Poiyomi Fur)  <br/><sub>↳ before `{ new[] { "_FurNormalMap", "_FurNormal" }, new[] { "_FurNormal", "_FurNrm" } },`</sub>
- **L3427** - Details (Filamented & Poiyomi)  <br/><sub>↳ before `{ new[] { "_DetailTex", "_DetailAlbedoMap" }, new[] { "_Detail", "_DetailAlbedo" } },`</sub>
- **L3431** - Decals  <br/><sub>↳ before `{ new[] { "_DecalTexture", "_DecalTex", "_DecalColorMap", "_Decal0", "_Decal1" }, new[] { "_Decal", "_DecalTex", "_Logo" } },`</sub>

### `private void AuditGeometryAndMaterials()`
<sub>L3553–L4017</sub>

- **L3553** - === 0. NULL MATERIAL RECOVERY PROTOCOL ===  <br/><sub>↳ before `if (hasMissingMats)`</sub>
- **L3619** - === 1. STATIC GEOMETRY PROTECTION ===  <br/><sub>↳ before `bool isProtectedVideoComponent = false;`</sub>
- **L3699** - === 1.5 OMNI-HARVESTER ===  <br/><sub>↳ before `if (RenderSettings.skybox != null) { sceneMaterials.Add(RenderSettings.skybox); ScrapeTexturesFromMaterial(RenderSettings.skybox); }`</sub>
- **L3712** - 4D-Chess: Only attempt to read materials if the font asset physically exists.  <br/><sub>↳ before `if (tmp.font != null)`</sub>
- **L3713** - This prevents internal TMPro NullReferenceExceptions on corrupted UI elements.  <br/><sub>↳ before `if (tmp.font != null)`</sub>
- **L3737** - Explicitly swallow internal TMP crashes on ghost objects  <br/><sub>↳ before `}`</sub>
- **L3738** - to prevent the system scan from halting.  <br/><sub>↳ before `}`</sub>
- **L3833** - === 2. SCRIPT ASSET SCRAPER ===  <br/><sub>↳ before `Type txlScreenMgrType = GetTypeSafe("Texel.ScreenManager");`</sub>
- **L3895** - === 3. IMPORTER LEAKS & COMPRESSION (CACHE GUARDED) ===  <br/><sub>↳ before `bool cacheUpdatedDuringScan = false;`</sub>
- **L3958** - === 4. MATERIAL PROTECTION & SHADER COMPLIANCE ===  <br/><sub>↳ before `foreach (var mat in sceneMaterials)`</sub>
- **L3978** - === A. SHADER ENFORCER (WHITELIST & REPLACER) ===  <br/><sub>↳ before `bool isInternalPluginShader = shaderName.IndexOf("AudioLink", StringComparison.OrdinalIgnoreCase) >= 0 \|\|`</sub>
- **L4017** - === B. GLOBAL MATERIAL OPTIMIZATION ===  <br/><sub>↳ before `if (shaderName.IndexOf("Poiyomi", StringComparison.OrdinalIgnoreCase) >= 0 \|\|`</sub>

### `private void AnalyzeTextures()`
<sub>L4192–L4194</sub>

- **L4192** - ==========================================  <br/><sub>↳ before `private bool ResizeTextureWithMagick(string fullPath, string assetPath, int maxWidth, int maxHeight)`</sub>
- **L4193** - IMAGEMAGICK DEPLOYMENT PROTOCOLS  <br/><sub>↳ before `private bool ResizeTextureWithMagick(string fullPath, string assetPath, int maxWidth, int maxHeight)`</sub>
- **L4194** - ==========================================  <br/><sub>↳ before `private bool ResizeTextureWithMagick(string fullPath, string assetPath, int maxWidth, int maxHeight)`</sub>

### `private bool ResizeTextureWithMagick(string fullPath, string assetPath, int maxWidth, int maxHeight)`
<sub>L4198</sub>

- **L4198** - Never resize shader-internal or HDR data textures (Poiyomi internals, .exr, etc.).  <br/><sub>↳ before `if (VixenMagickKit.IsProtectedAsset(assetPath)) return false;`</sub>

### `private bool OptimizeTextureWithMagick(string fullPath, string assetPath)`
<sub>L4228</sub>

- **L4228** - Never re-encode shader-internal or HDR data textures (Poiyomi internals, .exr, etc.).  <br/><sub>↳ before `if (VixenMagickKit.IsProtectedAsset(assetPath)) return false;`</sub>

### `private void ExecuteSelectedProtocols()`
<sub>L4516–L4538</sub>

- **L4516** - Prevent double execution  <br/><sub>↳ on `diag.FixPayload = null;`</sub>
- **L4518** - Keeps visual continuity during the split-second before the rescan hits  <br/><sub>↳ on `diag.OnFixedUIUpdate?.Invoke();`</sub>
- **L4523** - Persist all standard Unity structural changes (Materials, Prefabs, Shaders, etc.)  <br/><sub>↳ before `AssetDatabase.SaveAssets();`</sub>
- **L4526** - 4D-Chess: Check if the fixes generated heavy I/O operations (ImageMagick)  <br/><sub>↳ before `if (_workQueue.Count > 0)`</sub>
- **L4529** - Offload to the background thread to prevent Editor freezing.  <br/><sub>↳ before `StartProcessingQueue();`</sub>
- **L4530** - ProcessQueueTick() will handle the Refresh(), SaveLookupCache(), and InitiateFullSystemScan() when finished.  <br/><sub>↳ before `StartProcessingQueue();`</sub>
- **L4535** - If there was no heavy IO, execute the Live Refresh instantly  <br/><sub>↳ before `AssetDatabase.Refresh();`</sub>
- **L4538** - Force a save to the JSON database just in case any instant-fixes modified the cache  <br/><sub>↳ before `SaveLookupCache();`</sub>

---

## `Editor/VixenHub.cs`

*68 comment(s).*


### `(file scope)`
<sub>L13</sub>

- **L13** - --- AUTONOMOUS SCENE HUD NOTIFIER ---  <br/><sub>↳ before `[InitializeOnLoad]`</sub>

### `static VixenUpdateNotifier()`
<sub>L24–L27</sub>

- **L24** - 1. Autonomous Version Detection on Domain Reload  <br/><sub>↳ before `CheckForPackageChanges();`</sub>
- **L27** - 2. Bind the UI injector  <br/><sub>↳ before `SceneView.duringSceneGui += OnSceneGUI;`</sub>

### `private static void CheckForPackageChanges()`
<sub>L45–L54</sub>

- **L45** - If the version changed (or it's a fresh install), trigger the HUD  <br/><sub>↳ before `if (string.IsNullOrEmpty(storedVersion) \|\| storedVersion != currentVersion)`</sub>
- **L54** - Fail silently to prevent disrupting editor loads if package is migrating  <br/><sub>↳ on `catch { }`</sub>

### `private static Button BuildCyberBadge()`
<sub>L91–L126</sub>

- **L91** - --- LAYOUT & POSITIONING ---  <br/><sub>↳ before `badge.style.position = Position.Absolute;`</sub>
- **L98** - --- CYBERPUNK STYLING ---  <br/><sub>↳ before `badge.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.95f);`</sub>
- **L106** - Hot Pink  <br/><sub>↳ on `badge.style.borderLeftColor = new Color(1f, 0f, 0.66f);`</sub>
- **L107** - Cyan Glow  <br/><sub>↳ on `badge.style.borderBottomColor = new Color(0f, 0.9f, 1f, 0.3f);`</sub>
- **L121** - --- INTERACTIVITY ---  <br/><sub>↳ before `badge.style.transitionDuration = new List<TimeValue> { new TimeValue(0.15f) };`</sub>
- **L126** - --- TYPOGRAPHY ---  <br/><sub>↳ before `var label = new Label(">> <color=#00e5ff>VIX</color><color=#ff00aa>FORGE</color> UPDATE") { enableRichText = true };`</sub>

### `public static class VixenUpdateNotifier`
<sub>L139–L142</sub>

- **L139** - Process-wide Magick.NET tuning + shared lossless re-encode helper.  <br/><sub>↳ before `[InitializeOnLoad]`</sub>
- **L140** - Used by every VixForge tool that emits images so Unity restarts are not required  <br/><sub>↳ before `[InitializeOnLoad]`</sub>
- **L141** - to release file handles and so all PNG/JPEG/GIF/ICO outputs get the best compression  <br/><sub>↳ before `[InitializeOnLoad]`</sub>
- **L142** - Magick.NET can produce.  <br/><sub>↳ before `[InitializeOnLoad]`</sub>

### `static VixenMagickKit()`
<sub>L148–L162</sub>

- **L148** - OpenMP defaults to 1 thread on some Windows configurations; force all cores.  <br/><sub>↳ before `try`</sub>
- **L149** - Resource limits are process-wide globals so re-running on each domain reload  <br/><sub>↳ before `try`</sub>
- **L150** - is safe and idempotent.  <br/><sub>↳ before `try`</sub>
- **L158** - Asset-path fragments that mark shader / tool package internals. Textures inside these  <br/><sub>↳ before `private static readonly string[] ProtectedPathFragments =`</sub>
- **L159** - are hardcoded by the shader (fallback LUTs, matcaps, ramps, noise, reflection probes);  <br/><sub>↳ before `private static readonly string[] ProtectedPathFragments =`</sub>
- **L160** - resizing or re-encoding them corrupts the shader and triggers a Unity reimport storm.  <br/><sub>↳ before `private static readonly string[] ProtectedPathFragments =`</sub>
- **L161** - Matched case-insensitively as substrings of the forward-slash-normalized path.  <br/><sub>↳ before `private static readonly string[] ProtectedPathFragments =`</sub>
- **L162** - Extend this list as new shader packages are encountered.  <br/><sub>↳ before `private static readonly string[] ProtectedPathFragments =`</sub>

### `private static readonly string[] ProtectedPathFragments =`
<sub>L173–L175</sub>

- **L173** - Texture file formats that hold data, not visual content: HDR maps, color-grading LUTs,  <br/><sub>↳ before `private static readonly string[] ProtectedExtensions =`</sub>
- **L174** - reflection probes, lightmaps. They must never be resampled or re-encoded - doing so  <br/><sub>↳ before `private static readonly string[] ProtectedExtensions =`</sub>
- **L175** - destroys the data the shader reads. This alone catches the common ".exr fallback" case.  <br/><sub>↳ before `private static readonly string[] ProtectedExtensions =`</sub>

### `private static readonly string[] ProtectedExtensions =`
<sub>L181–L184</sub>

- **L181** - Central policy: returns true when the asset at this path must not be touched by any  <br/><sub>↳ before `public static bool IsProtectedAsset(string path)`</sub>
- **L182** - image pass (resize, sharpen, lossless re-encode). Accepts both project-relative asset  <br/><sub>↳ before `public static bool IsProtectedAsset(string path)`</sub>
- **L183** - paths ("Assets/...") and absolute filesystem paths. A null/empty path is treated as  <br/><sub>↳ before `public static bool IsProtectedAsset(string path)`</sub>
- **L184** - protected (can't verify it, so don't touch it).  <br/><sub>↳ before `public static bool IsProtectedAsset(string path)`</sub>

### `public static bool IsProtectedAsset(string path)`
<sub>L202–L212</sub>

- **L202** - Lossless re-encode that never holds the source file open.  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L203** - - Reads bytes via managed I/O (so the OS handle is released before Magick sees it).  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L204** - - Runs ImageOptimizer with OptimalCompression which tries multiple filter/strategy  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L205** - combos and keeps the smallest output.  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L206** - - Only overwrites when the result is genuinely smaller.  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L207** - - Silently skips unsupported formats (TGA, DDS, EXR, etc.) so callers can fire it  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L208** - blindly after any Write().  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L209** - Above this file size, OptimalCompression's 4x re-encode pass is disproportionately  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L210** - expensive (it tries qualities 91/94/95/97 sequentially). On a 30 MB PNG that's  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L211** - ~2 minutes per file, which is exactly what locked up the editor on the upscale path.  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>
- **L212** - Below the threshold we still do the full 4-pass search.  <br/><sub>↳ before `private const long OptimalCompressionMaxBytes = 10L * 1024 * 1024;`</sub>

### `public static bool TryLosslessOptimize(string path)`
<sub>L218–L224</sub>

- **L218** - Backstop: never re-encode protected shader/data assets even if a caller asks.  <br/><sub>↳ before `if (IsProtectedAsset(path)) return false;`</sub>
- **L223** - PngHelper.GetQualityList: returns 4 qualities when OptimalCompression=true,  <br/><sub>↳ before `bool useOptimal = fileBytes <= OptimalCompressionMaxBytes;`</sub>
- **L224** - returns 1 quality when false. For big files we use the single-pass mode.  <br/><sub>↳ before `bool useOptimal = fileBytes <= OptimalCompressionMaxBytes;`</sub>

### `private Font _cyberFont;`
<sub>L267</sub>

- **L267** - --- Dynamic Version States ---  <br/><sub>↳ before `private string _packageVersion = "Unknown";`</sub>

### `private string _sdkVersion = "Unknown";`
<sub>L271</sub>

- **L271** - --- Changelog Pagination State ---  <br/><sub>↳ before `private class ChangelogEntry`</sub>

### `private void CreateGUI()`
<sub>L376–L441</sub>

- **L376** - --- HEADER BANNER ---  <br/><sub>↳ before `var headerRect = new VisualElement { name = "hub-header" };`</sub>
- **L394** - --- TABS NAVIGATION ---  <br/><sub>↳ before `var tabContainer = new VisualElement { name = "tab-container" };`</sub>
- **L404** - Metrics Engine docs (HOWITWORKS.md) describe the World Profiler, so only surface the tab in World SDK (Udon) projects.  <br/><sub>↳ before `_btnMetricsDocs = new Button(() => SwitchMode(TabMode.MetricsDocs)) { text = "Metrics Engine" };`</sub>
- **L434** - --- TAB DESCRIPTION BOX ---  <br/><sub>↳ before `var descContainer = new VisualElement { name = "desc-container" };`</sub>
- **L441** - --- CONTENT AREA ---  <br/><sub>↳ before `_contentScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>

### `private string LoadMarkdownFile(string fileName)`
<sub>L583–L588</sub>

- **L583** - =========================================================================  <br/><sub>↳ before `#if UDON`</sub>
- **L584** - DOCUMENTATION LOADERS  <br/><sub>↳ before `#if UDON`</sub>
- **L585** - =========================================================================  <br/><sub>↳ before `#if UDON`</sub>
- **L588** - World-only: HOWITWORKS.md documents the World Profiler heuristics, so it is only loaded when the Udon (World SDK) define is present.  <br/><sub>↳ before `private void RenderMetricsDocs()`</sub>

### `private void RenderShaderDocs()`
<sub>L602–L604</sub>

- **L602** - =========================================================================  <br/><sub>↳ before `private void RenderCoreModules()`</sub>
- **L603** - BUTTON GRIDS (Core Modules, Network, Support, Supported Modules)  <br/><sub>↳ before `private void RenderCoreModules()`</sub>
- **L604** - =========================================================================  <br/><sub>↳ before `private void RenderCoreModules()`</sub>

### `private void RenderCoreModules()`
<sub>L627–L661</sub>

- **L627** - Read the live state directly from EditorPrefs for both tools  <br/><sub>↳ before `bool isSnapActive = EditorPrefs.GetBool("VixenTools/Scene/Live Surface Snapping", false);`</sub>
- **L629** - Injecting Cyan rich text for the active state to pop against the UI  <br/><sub>↳ before `string snapTitle = isSnapActive`</sub>
- **L643** - Execute the core logic  <br/><sub>↳ before `EditorApplication.ExecuteMenuItem("VixenTools/Scene/Live Surface Snapping");`</sub>
- **L645** - Force the UIElements layout to flush and rebuild to catch the new EditorPrefs state  <br/><sub>↳ before `SwitchMode(TabMode.CoreModules);`</sub>
- **L659** - -------------------------  <br/><sub>↳ before `(() =>`</sub>
- **L660** - NEW: World Engine entry  <br/><sub>↳ before `(() =>`</sub>
- **L661** - -------------------------  <br/><sub>↳ before `(() =>`</sub>

### `private void RenderActionGrid(string headerText, string accentHex, List<(System.Action action, string title, string desc)> items)`
<sub>L794–L796</sub>

- **L794** - ================  <br/><sub>↳ before `private void ParseMarkdownAndInject(string text, VisualElement container)`</sub>
- **L795** - MARKDOWN PARSER  <br/><sub>↳ before `private void ParseMarkdownAndInject(string text, VisualElement container)`</sub>
- **L796** - ================  <br/><sub>↳ before `private void ParseMarkdownAndInject(string text, VisualElement container)`</sub>

---

## `Shaders/VixenWear Latex.shader`

*362 comment(s).*


### `(file scope)`
<sub>L1–L6</sub>

- **L1** — VixenWear / Latex Ultra - Built-in Render Pipeline only (VRChat targets Built-in).  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra"`</sub>
- **L2** — This is a #pragma surface shader, which the HDRP/URP scriptable pipelines cannot compile;  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra"`</sub>
- **L3** — HDRP support would be a separate ShaderGraph/HDRP-Lit shader, not this file. World-lighting  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra"`</sub>
- **L4** — integrations (AudioLink, LTCGI, AreaLit, VRSL + VRSL GI, VRC Light Volumes)  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra"`</sub>
- **L5** — are all fail-safe: each is keyword-stripped or runtime-gated and probes its data source for  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra"`</sub>
- **L6** — liveness, so entering a world without a given system costs nothing and shows no artifact.  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra"`</sub>

### `Properties`
<sub>L11</sub>

- **L11** — Rendering mode drives the alpha workflow - Opaque (no clip/blend), Cutout (clip on _CutOff), Fade (straight alpha - everything fades), Transparent (premultiplied - specular survives); defaults to Cutout for historical clip(c.a - _CutOff) behavior.  <br/><sub>↳ before `[Enum(Opaque,0,Cutout,1,Fade,2,Transparent,3)] _Mode ("Rendering Mode", Float) = 1`</sub>

### `[NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}`
<sub>L30</sub>

- **L30** — Poiyomi PBR Mask compatibility - per-channel selectors so Poiyomi/Substance/Marmoset-packed masks drop in without re-authoring; defaults match VixenWear's native packing (R:Met G:AO B:Disp A:Smooth).  <br/><sub>↳ before `[Enum(R,0,G,1,B,2,A,3)] _PBR_Met_Ch ("Metallic Channel", Float) = 0`</sub>

### `[Enum(R,0,G,1,B,2,A,3)] _PBR_Height_Ch ("Height Channel", Float) = 2`
<sub>L38</sub>

- **L38** — Poiyomi/Mochie packed-map masks - reflection mask dims environment/probe reflections, specular mask dims direct highlights. Channel defaults (B/A) match Mochie "Metallic Maps" packing (R:Met G:Smooth B:ReflMask A:SpecMask). Default off so existing materials are unchanged.  <br/><sub>↳ before `[Toggle] _UsePackedMasks ("Enable Reflection / Specular Masks", Float) = 0`</sub>

### `[Toggle] _UseMultiScatter ("Multi-Scatter Energy Compensation", Float) = 1`
<sub>L82</sub>

- **L82** — Polish layer master gate + B&W mask - scales the entire polish lighting layer (clearcoat, thin film, SSS, transmission, anisotropy, rim, multi-scatter) per-pixel. Toggle on + white mask preserves the historical look; runtime-gated (no keyword) so VRCFury can animate it.  <br/><sub>↳ before `[Toggle] _UsePolish ("Enable Polish Layer", Float) = 1`</sub>

### `[Enum(R,0,G,1,B,2,A,3)] _PolishMaskCh ("Polish Mask Channel", Float) = 0`
<sub>L87</sub>

- **L87** — Drip - procedural vertical rivulets that mimic water running off the latex (per-pixel wet streaks). Own toggle so off = no cost.  <br/><sub>↳ before `[Toggle] _UseDrip ("Enable Drip (Water Run-Off)", Float) = 0`</sub>

### `_Drip_Normal ("Drip Normal Bump", Range(0, 1)) = 0.5`
<sub>L98</sub>

- **L98** — Clear 3D drips - water beads that swell and pinch off, then run down the surface and dry out (fade away); shaded as clear water tinted to the clearcoat color. Vertex bulge plus surface glass, gated under the Wet toggle.  <br/><sub>↳ before `_Drip3D_Strength ("Clear Drip Amount", Range(0, 1)) = 0`</sub>

### `_Drip3D_Fall ("Clear Drip Fall Length", Range(0, 1)) = 0.6`
<sub>L104</sub>

- **L104** — Clear drip physics + collision - ambient sway/wobble, surface-slide down the body while attached, and a floor splat that pools on the shared world floor (_Goo_GroundY). All default off so existing droplet materials are unchanged.  <br/><sub>↳ before `_Drip_Sway ("Droplet Sway / Wobble", Range(0, 1)) = 0`</sub>

### `[Toggle] _Drip_FloorCollide ("Droplet Floor Splat", Float) = 0`
<sub>L109</sub>

- **L109** — Wet soak - global "just out of the shower/pool" wetness layered under the run-off rivulets above.  <br/><sub>↳ before `_Wet_Amount ("Wetness (Soaked)", Range(0, 1)) = 0.7`</sub>

### `_Wet_Flatten ("Wet Normal Flatten", Range(0, 1)) = 0.5`
<sub>L116</sub>

- **L116** — Goo - gravity-aligned vertex sag that mimics melting/runny latex or wax. Runs in disp(); own toggle.  <br/><sub>↳ before `[Toggle] _UseGoo ("Enable Goo (Melting Sag)", Float) = 0`</sub>

### `_Goo_GroundY ("Goo Ground Height (World Y)", Float) = 0`
<sub>L129</sub>

- **L129** — Goo physics + collision - ambient pendulum sway, surface-follow body collision, and a floor clamp with pooling. All default off so existing materials are unchanged; _Goo_GroundY is the shared world floor for both goo and droplet collision.  <br/><sub>↳ before `_Goo_Sway ("Goo Sway Amount", Range(0, 1)) = 0`</sub>

### `[NoScaleOffset] _EmissionMap ("Emission Map (RGB tint, A mask)", 2D) = "black" {}`
<sub>L145</sub>

- **L145** — Poiyomi-style secondary emission layer - independent texture, color, mask, and AL band reactor.  <br/><sub>↳ before `[Toggle] _UseEmission2 ("Enable Secondary Emission Layer", Float) = 0`</sub>

### `_AL_Emis2_Mod ("Emission 2 AL Amplitude", Range(0,1)) = 0.0`
<sub>L153</sub>

- **L153** — Poiyomi-style multi-region color mask - RGB zones each drive an albedo tint and emission boost.  <br/><sub>↳ before `[Toggle] _UseRegionMask ("Enable Multi-Region Color Mask", Float) = 0`</sub>

### `[NoScaleOffset] _MatCapMask ("MatCap 1 Mask", 2D) = "white" {}`
<sub>L165</sub>

- **L165** — Mask channel pick - defaults to R for single-channel mask compat; set to G/B/A to drive layer 1 from a different channel of an RGB region mask.  <br/><sub>↳ before `[Enum(R,0,G,1,B,2,A,3)] _MatCap_MaskCh ("MatCap 1 Mask Channel", Float) = 0`</sub>

### `_MatCap_Lit ("MatCap 1 Lighting Mix", Range(0,1)) = 1.0`
<sub>L172</sub>

- **L172** — Second matcap layer - own texture/mask/channel/tint/intensity/rotation/blend mode; common workflow drops the same red/blue/black region mask into both layers and picks R for layer 1, B for layer 2 so each zone shows a different matcap.  <br/><sub>↳ before `[Toggle] _UseMatCap2 ("Enable MatCap 2 Layer", Float) = 0`</sub>

### `_LTCGI_Diff_Mix ("LTCGI Diffuse Mix", Range(0,2)) = 1.0`
<sub>L193</sub>

- **L193** — AreaLit (PiMaker area lights) - point the two slots at the world's AreaLit LightMesh + video RenderTexture (AreaLit data is per-material, not a scene global). Keyword-gated by _AreaLit_Int > 0 via the editor.  <br/><sub>↳ before `[NoScaleOffset] _AreaLit_LightMesh ("AreaLit LightMesh RT", 2D) = "black" {}`</sub>

### `[VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Auto_Transform ("Autocorrelator Transform", Vector) = (0,0,1,0)`
<sub>L247</sub>

- **L247** — Per-effect reactors for the Autocorrelator HUD ring. Each effect is toggled on/off and driven by its own AudioLink band.  <br/><sub>↳ before `[Toggle] _Cyber_Auto_Shimmer ("AC Shimmer Effect", Float) = 1`</sub>

### `_AL_Glitch_Mod ("Digital Glitch Tear", Range(0,1)) = 0.0`
<sub>L318</sub>

- **L318** — Outline pass - Sylva-style Cull Front backface extrusion; toggle gates the entire variant so off = zero runtime cost.  <br/><sub>↳ before `[Toggle(_OUTLINE_ON)] _UseOutline ("Enable Outline", Float) = 0`</sub>

### `SubShader`
<sub>L333</sub>

- **L333** — Tags listed here are SubShader defaults - VixenWearEditor overrides RenderType/Queue/VRCFallback per material via SetOverrideTag to match the selected _Mode (Opaque/Cutout/Fade/Transparent).  <br/><sub>↳ before `Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }`</sub>

### `Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }`
<sub>L337</sub>

- **L337** — PASS 0: OUTLINE (Cull Front backface extrusion - Sylva-style). Keyword-gated by _OUTLINE_ON so the unused variant is the no-keyword default and costs nothing at runtime. Always-opaque blend so the outline is solid regardless of the material's selected alpha mode.  <br/><sub>↳ before `Cull Front`</sub>

### `CGPROGRAM`
<sub>L344</sub>

- **L344** — Minimal surface shader: no GI, no extra lights, no shadow/lightmap variants. Outline color goes to Emission; lighting fn returns black so the only contribution is the emission tint.  <br/><sub>↳ before `#pragma surface outlineSurf Outline keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap noshadowmask nometa …`</sub>

### `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRE…`
<sub>L349</sub>

- **L349** — Outline master toggle - when off, vertex skips extrusion and surface clips the pixel so the pass is effectively dead. Alpha keywords mirror the main pass so cutout textures don't cause outlines to float in transparent regions.  <br/><sub>↳ before `#pragma shader_feature_local _OUTLINE_ON`</sub>

### `#include "UnityCG.cginc"`
<sub>L356</sub>

- **L356** — AudioLink for optional emission boost - runtime-gated by _UseAudioLink so it costs nothing when AL isn't in scene.  <br/><sub>↳ before `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`</sub>

### `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`
<sub>L359</sub>

- **L359** — _MainTex_ST is auto-declared by the surface compiler because Input.uv_MainTex is present; redeclaring it (or any *_ST for a used uv) collides at the FORWARD pass.  <br/><sub>↳ before `sampler2D _MainTex;`</sub>

### `struct Input`
<sub>L376</sub>

- **L376** — None=0 (full strength), R/G/B/A=1..4 (matches inspector enum). Mirrored from main pass ChannelPick with the extra None slot for "no mask, just use everywhere".  <br/><sub>↳ before `inline float OL_ChannelPick(fixed4 packed, float ch)`</sub>

### `#if defined(_OUTLINE_ON)`
<sub>L389–L406</sub>

- **L389** — Eye-depth scaling keeps the outline a visually constant thickness at distance instead of vanishing.  <br/><sub>↳ before `float eyeDepth = -UnityObjectToViewPos(v.vertex.xyz).z;`</sub>
- **L393** — 0.0001 scale converts the 0-1000 slider into reasonable world-units; min() clamps so the outline doesn't blow up at far distance.  <br/><sub>↳ before `float wBase = lerp(0.0, _OutlineWidth    * 0.0001, saturate(_OutlineWidth));`</sub>
- **L401** — View fudge nudges the extruded shell toward the camera to mitigate z-fighting against the main pass when ZWrite is on for both.  <br/><sub>↳ before `float3 worldPos  = mul(unity_ObjectToWorld, v.vertex).xyz;`</sub>
- **L406** — Convert world-space offset back to object space without translation.  <br/><sub>↳ before `v.vertex.xyz += mul((float3x3)unity_WorldToObject, worldOffset);`</sub>

### `#endif`
<sub>L411</sub>

- **L411** — Black direct lighting - emission carries the visible color so the outline doesn't pick up scene lighting.  <br/><sub>↳ before `inline half4 LightingOutline(SurfaceOutput s, half3 lightDir, half atten)`</sub>

### `#if !defined(_OUTLINE_ON)`
<sub>L420</sub>

- **L420** — Toggle off: kill every fragment. Cheaper than letting the BRDF math run; the un-extruded backfaces would z-fight with the main pass anyway.  <br/><sub>↳ before `clip(-1);`</sub>

### `#endif`
<sub>L426–L431</sub>

- **L426** — Match the main pass cutout behavior so the outline respects the same alpha test.  <br/><sub>↳ before `#if defined(_ALPHATEST_ON)`</sub>
- **L431** — Optional AL emission boost - runtime-gated, no keyword variant. Uses raw band amplitude (no Chronotensity) to keep this pass cheap.  <br/><sub>↳ before `half3 alBoost = 0;`</sub>

### `ENDCG`
<sub>L447–L452</sub>

- **L447** — Blend/ZWrite are property-driven so the editor flips them per-material without a recompile - Opaque/Cutout use One/Zero/ZWrite On; Fade uses SrcAlpha/OneMinusSrcAlpha/ZWrite Off; Transparent uses One/OneMinusSrcAlpha/ZWrite Off.  <br/><sub>↳ before `Cull Off`</sub>
- **L452** — PASS 1: CORE PBR SURFACE (BASE SUIT, FRACTURE CLIP)  <br/><sub>↳ before `CGPROGRAM`</sub>

### `CGPROGRAM`
<sub>L454</sub>

- **L454** — Surface pragma drops Deferred/Meta + LIGHTMAP/DIRLIGHTMAP/SHADOWMASK/LPPV variants (VRChat forward-only, avatar clothing never lightmapped); keepalpha preserves LightingStandardLatex alpha so Fade/Transparent get real alpha. noforwardadd skips the ForwardAdd pass entirely (avatar gets directional + probes + LV + LTCGI; loses realtime per-light additive contributions) - critical for ps_5_0 sampler budget because ForwardAdd's POINT/POINT_COOKIE + SHADOWS_CUBE built-in samplers stacked on our 13 texture samplers blew past the 16-register cap.  <br/><sub>↳ before `#pragma surface surf StandardLatex keepalpha fullforwardshadows addshadow noforwardadd vertex:disp tessellate:tessEdge exclude_path:deferre…`</sub>

### `#pragma target 5.0`
<sub>L458</sub>

- **L458** — Defensive against Unity 2022.3.x emitting lightmap/LOD variants despite the no* directives above. Cookie + cube-shadow variants are also skipped for sampler budget - any directional cookie / point cube shadow would add 1-2 samplers, and avatars don't typically use them.  <br/><sub>↳ before `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRE…`</sub>
- **Import-time trims (`only_renderers` + `SHADOWS_SOFT`)** — `only_renderers d3d11` follows every `#pragma target 5.0` (all programs: outline, surface, and the geometry effect passes) so Unity compiles one graphics API instead of the whole desktop set (gles3/metal/vulkan/glcore). VixenWear is PC / Built-in-RP only and PC VRChat runs DX11, so this cuts source reimport and the VRCFury SPS patch+import several-fold. Tradeoff: a player forcing `-vulkan` or `-dx12` gets a broken shader (rare, experimental launch options). `SHADOWS_SOFT` was added to the skip_variants list to roughly halve the ForwardBase shadow-receiving set (slightly harder shadow edges). Do NOT add `VERTEXLIGHT_ON` to skip_variants: VRCFury SPS (`sps_light.cginc`) reads the per-vertex light arrays `unity_4LightAtten0` / `unity_LightColor` / `unity_4LightPosX0` for socket detection, which only populate in ForwardBase under VERTEXLIGHT_ON. Per `SpsPatcher.cs` the patched shader compiles every pass twice (a `ShaderUtil.CompilePass` precheck plus a `ForceSynchronousImport`) and is hash-cached, so this cost is paid once per shader edit, not per build, scaled by pass and variant count.  <br/><sub>↳ before `#pragma only_renderers d3d11`</sub>

### `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRE…`
<sub>L461</sub>

- **L461** — VRChat single-pass stereo / GPU instancing - required for avatar batching in VR.  <br/><sub>↳ before `#pragma multi_compile_instancing`</sub>

### `#pragma multi_compile_instancing`
<sub>L463</sub>

- **L463** — AudioLink always compiled and runtime-gated via _UseAudioLink so VRCFury material-toggle animations can flip it without a build-time variant (VRC materials can't change keywords at runtime); VRSL_ENABLE is referenced in disp() so it needs full per-stage variants - the rest are fragment-only.  <br/><sub>↳ before `#pragma shader_feature_local VRSL_ENABLE`</sub>

### `#pragma shader_feature_local_fragment _DETAIL_NORMAL`
<sub>L469</sub>

- **L469** — Alpha workflow keywords - set by VixenWearEditor based on _Mode. Mutually exclusive; Opaque mode = none on.  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>

### `#endif`
<sub>L481–L487</sub>

- **L481** — AudioLink.cginc is always included (runtime-gated by _UseAudioLink) so VRCFury toggles work without keyword variants.  <br/><sub>↳ before `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`</sub>
- **L487** — VRChat mirror cameras leave _WorldSpaceCameraPos at the player's head - view-dependent math (specular, parallax, cubemap) renders wrong in the mirror; UNITY_MATRIX_I_V._m03_m13_m23 is the actual rendering camera world pos (per-eye correct under single-pass instanced).  <br/><sub>↳ before `float3 vw_CameraPos()    { return UNITY_MATRIX_I_V._m03_m13_m23; }`</sub>

### `struct Input`
<sub>L540–L578</sub>

- **L540** — _MainTex uses an explicit texture + sampler so the fragment-stage B&W masks (_PolishMask, _DripMask, _CyberMask) can borrow its sampler instead of each consuming one of the 16 ps_5_0 sampler registers. A borrowed sampler only resolves in a stage where its donor texture is actually sampled, so _GooMask keeps its own combined sampler: it is read in the vertex/displacement stage (and the auto-generated shadow caster), where _MainTex is not sampled. Net sampler count is unchanged versus before these effects: _CyberMask gives up its register, _GooMask takes one.  <br/><sub>↳ before `UNITY_DECLARE_TEX2D(_MainTex);`</sub>
- **L553** — Poiyomi compat: PBR mask channel selectors + invert toggles.  <br/><sub>↳ before `float _PBR_Met_Ch, _PBR_Met_Inv, _PBR_Smooth_Ch, _PBR_Smooth_Inv, _PBR_AO_Ch, _PBR_Height_Ch;`</sub>
- **L556** — Poiyomi compat: secondary emission layer + multi-region color mask.  <br/><sub>↳ before `float _UseEmission2, _Emis2_MaskCh, _AL_Band_Emis2, _AL_Emis2_Mod;`</sub>
- **L565** — Polish master gate + B&W mask, plus the drip (surface) and goo (vertex) latex effects.  <br/><sub>↳ before `float _UsePolish, _PolishMaskCh;`</sub>
- **L578** — AreaLit area lights (analytic LTC). Mix floats always declared (cheap); the data textures + math live in the keyword-gated include so they strip when unused. Included here - AFTER UNITY_DECLARE_TEX2D(_MainTex) above - because the vendored sampler borrows sampler_MainTex.  <br/><sub>↳ before `float _AreaLit_Int, _AreaLit_Spec_Mix, _AreaLit_Diff_Mix;`</sub>

### `#endif`
<sub>L610–L614</sub>

- **L610** — _Udon_DMXGridStrobeOutput dropped - declared but never sampled in this shader, just consumed a sampler register.  <br/><sub>↳ before `uniform sampler2D _Udon_DMXGridRenderTextureMovement;`</sub>
- **L614** — HELPERS  <br/><sub>↳ before `float FetchVRSLChannel(uint absoluteChannel, sampler2D tex, float4 texelSize)`</sub>

### `float2 RotateUVDeg(float2 uv, float deg)`
<sub>L670</sub>

- **L670** — Hue (0..1) to RGB - cheap triangle-wave approximation, no HSV stack required.  <br/><sub>↳ before `inline float3 HUEtoRGB(float h)`</sub>

### `float4 tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)`
<sub>L685</sub>

- **Detail + cap (fixes inverted/uncapped tess lag)** — `_Tess_Detail` (0..1) replaced the old `_Tess_Edge` (px, Range 1..50). The old control was both inverted and uncapped: `UnityEdgeLengthBasedTess`'s parameter is a *target edge length* and sits in the denominator of the tess factor (`factor ≈ edgeLen_world × screenHeight / (param × dist)`), so a **low** number meant tiny target edges = runaway subdivision = severe lag - every triangle hitting the GPU's hard 64× cap on a dense displaced mesh. Now detail maps the intuitive way via `edgeLen = lerp(40, 2, saturate(_Tess_Detail))` (0 = coarse/cheap, 1 = dense), the distance/screen LOD of `UnityEdgeLengthBasedTess` is preserved (far/small-on-screen surfaces stay cheap), and the returned float4 is clamped with `min(tess, VW_TESS_MAX)` where `VW_TESS_MAX = 32` so the close-up worst case can't melt the GPU. Property was **renamed** (not just inverted) so old materials reset to the 0.5 default rather than silently inheriting an inverted value. SPS twin has no `tessellate:` pragma, so this is base-shader-only.  <br/><sub>↳ before `float edgeLen = lerp(40.0, 2.0, saturate(_Tess_Detail));`</sub>
- **L685** — Poiyomi-style packed PBR channel picker. Channel index: 0=R, 1=G, 2=B, 3=A.  <br/><sub>↳ before `inline float ChannelPick(fixed4 packed, float ch)`</sub>

### `inline float ChannelPick(fixed4 packed, float ch)`
<sub>L694</sub>

- **L694** — Hash + smooth 3D value noise (0..1) driving the Goo melt's procedural per-strand variation.  <br/><sub>↳ before `float gooHash3(float3 p) { return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453); }`</sub>

### `float gooNoise3(float3 p)`
<sub>L716</sub>

- **L716** — Returns true if AudioLink should be considered active for this frame.  <br/><sub>↳ before `bool AL_Active()`</sub>

### `void FetchAudioLinkBands(out float4 amps, out float4 chronos, out float4 al_color, out float raw_waveform, out float autoCorr, float2 uv)`
<sub>L740–L782</sub>

- **L740** — stronger mapping for visible reaction  <br/><sub>↳ before `amps.x = saturate(pow(al_amps.x * 4.0, 0.35));`</sub>
- **L746** — Chronotensity is opt-in via _UseChronoFX to avoid 4 extra texture samples for amplitude-only users.  <br/><sub>↳ before `if (_UseChronoFX > 0.5)`</sub>
- **L757** — CCCOLORS index 0 is always black, so band → note is offset by +1.  <br/><sub>↳ before `if (colorMode == 1)`</sub>
- **L760** — Theme 0..3 live at uint2(0..3, 23), not CCCOLORS row+1.  <br/><sub>↳ before `else if (colorMode >= 2 && colorMode <= 5)`</sub>
- **L771** — Respect media state: when enabled, mute effects if media is NOT playing  <br/><sub>↳ before `if (_UseMediaState > 0.5 && _MediaPlaying < 0.5)`</sub>
- **L782** — Vertex displacement + AudioLink-driven pump/fracture/autocorrelator.  <br/><sub>↳ before `void disp(inout appdata_full v)`</sub>

### `void disp(inout appdata_full v)`
<sub>L787–L791</sub>

- **L787** — Base displacement from packed PBR map (channel chosen by _PBR_Height_Ch for Poiyomi-pack compat).  <br/><sub>↳ before `float dispHeight = ChannelPick(tex2Dlod(_MetallicGlossMap, float4(uv, 0, 0)), _PBR_Height_Ch);`</sub>
- **L791** — VRSL geometric warp  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>

### `#endif`
<sub>L803–L902</sub>

- **L803** — AudioLink-driven pump + fracture (runtime-gated so VRCFury toggle controls activation) - all vertex effects masked by _UseVtxKinetic so sliders alone do nothing without the master toggle.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && _UseVtxKinetic > 0.5)`</sub>
- **L806** — Fetch AudioLink bands for this vertex UV  <br/><sub>↳ before `float4 amps; float4 chronos; float4 al_color; float raw_wave; float autoCorr;`</sub>
- **L810** — Vertex pump (inflate along normal)  <br/><sub>↳ before `if (_Vtx_Pump_Str > 0.0001)`</sub>
- **L818** — Spherical autocorrelator ripple (object-space coords) - only fires with live AL data, never falls back to a static slider value.  <br/><sub>↳ before `if (_Vtx_AutoCorr_Str > 0.0001 && AudioLinkIsAvailable())`</sub>
- **L825** — Vertex fracture is now a real geometry-shader effect (see "PASS 4: FRACTURE SHARDS"), driven by _Vtx_Fracture_Amount; the old in-place vertex scatter is removed.  <br/><sub>↳ before `}`</sub>
- **L828** — GOO - melting/runny latex. Gravity-aligned, masked, and procedurally varied so it forms uneven runny tendrils. Range is dramatically extendable via _Goo_Reach, and it can optionally melt all the way down to the world ground plane (_Goo_ToGround). Runs in disp(); own toggle, independent of the AL kinetic gate.  <br/><sub>↳ before `if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)`</sub>
- **L834** — World position (for melt-to-ground) and world normal (downward-facing surfaces melt more).  <br/><sub>↳ before `float3 gooWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;`</sub>
- **L840** — PROCEDURAL GENERATION - coarse per-strand identity (coherent tendrils) plus two octaves of value noise for organic, uneven melting. _Goo_Variation blends from a uniform melt (0) to wildly varying strand lengths (1).  <br/><sub>↳ before `float3 gooNP = v.vertex.xyz * _Goo_Noise;`</sub>
- **L848** — Slow time wobble so the melt stays alive and runny; staggered per strand.  <br/><sub>↳ before `float wobble = 0.75 + 0.25 * sin(_Time.y * _Goo_Speed * 6.2831 + strandHash * 6.2831);`</sub>
- **L851** — Common melt weight (0..~1.5); some strands reach further than others.  <br/><sub>↳ before `float meltWeight = gooMask * faceWeight * strandReach * wobble * saturate(_Goo_Strength);`</sub>
- **L854** — DRAMATICALLY EXTENDED RANGE. Distance mode stretches down a large, settable distance (_Goo_Reach world units). Ground mode pulls each vertex down toward the world ground plane (Y = _Goo_GroundY) so strands reach the floor regardless of avatar height. Computed in world space, then converted to object space so non-uniform scale is handled.  <br/><sub>↳ before `float distDown   = _Goo_Reach * meltWeight;`</sub>
- **L859** — PHYSICS - lateral pendulum sway, growing with how far the strand has melted so the tip swings most, like a weighted strand. Staggered per strand so tendrils never move in lock-step.  <br/><sub>↳ before `float3 lateral = 0;`</sub>
- **L868** — BODY COLLISION (best-effort) - project the melt onto the surface tangent plane so goo flows ALONG the body instead of tunnelling straight through it (1 = pure surface flow, 0 = straight gravity).  <br/><sub>↳ before `if (_Goo_BodyFollow > 0.0001)`</sub>
- **L878** — FLOOR COLLISION - clamp the melted world position to the floor plane (_Goo_GroundY) and splay sideways into a shallow pool where it lands.  <br/><sub>↳ before `float3 meltedWP = gooWorldPos + meltWorld;`</sub>
- **L893** — Back to object space (handles non-uniform scale).  <br/><sub>↳ before `v.vertex.xyz += mul((float3x3)unity_WorldToObject, meltedWP - gooWorldPos);`</sub>
- **L898** — Static displacement  <br/><sub>↳ before `v.vertex.xyz += v.normal * d;`</sub>
- **L902** — PBR HELPERS  <br/><sub>↳ before `float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)`</sub>

### `float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)`
<sub>L905–L910</sub>

- **L905** — Derivatives are taken up front in uniform control flow so the tex2Dgrad calls inside the dynamic loop stay valid, and the function uses a single return path so FXC can prove every local is initialized (silences the "potentially uninitialized variable" warning in the shadow caster).  <br/><sub>↳ before `float2 dx = ddx(uv);`</sub>
- **L910** — Early-out when depth ~= 0 - otherwise the loop below re-samples the same texel up to 50 times (stepUVOffset collapses to zero) and exits only when the heightmap value rises above the descending layer height, burning ~35 tex2Dgrad samples per pixel on any non-white surface map.  <br/><sub>↳ before `[branch] if (parallaxDepth >= 1e-4)`</sub>

### `inline half HDRPSpecularOcclusion(half NdotV, half AO, half roughness)`
<sub>L948</sub>

- **L948** — Geometric specular AA - Toksvig-style filtering on screen-space normal derivative variance.  <br/><sub>↳ before `inline half GeometricSpecAA(float3 worldNormal, half roughness, half strength)`</sub>

### `inline half GeometricSpecAA(float3 worldNormal, half roughness, half strength)`
<sub>L960</sub>

- **L960** — GGX BRDF HELPERS: D=Trowbridge-Reitz, V=Smith Joint, F=Schlick, Diffuse=Burley, Indirect=Karis split-sum, MS=Filament.  <br/><sub>↳ before `inline float D_GGX(float NdotH, float a2)`</sub>

### `inline float V_SmithJointGGX(float NdotL, float NdotV, float a2)`
<sub>L974</sub>

- **L974** — Anisotropic GGX (Burley 2012)  <br/><sub>↳ before `inline float D_GGX_Aniso(float NdotH, float TdotH, float BdotH, float ax, float ay)`</sub>

### `inline float3 F_Schlick(float u, float3 F0)`
<sub>L1001</sub>

- **L1001** — Burley/Disney diffuse. Returns scalar (caller multiplies by NdotL and color).  <br/><sub>↳ before `inline float Burley_Diffuse(float NdotV, float NdotL, float LdotH, float roughness)`</sub>

### `inline float Burley_Diffuse(float NdotV, float NdotL, float LdotH, float roughness)`
<sub>L1010</sub>

- **L1010** — Karis split-sum env BRDF: AB.x = F0 scale, AB.y = bias; env_brdf = F0*AB.x + AB.y.  <br/><sub>↳ before `inline float2 EnvBRDFApprox_AB(float roughness, float NdotV)`</sub>

### `inline float3 EnvBRDFApprox(float3 F0, float roughness, float NdotV)`
<sub>L1026</sub>

- **L1026** — Filament/Frostbite multi-scatter compensation. Returns 1 + F0*((1-E)/E), E≈dfg_AB.x+dfg_AB.y.  <br/><sub>↳ before `inline float3 EnergyCompensation(float3 F0, float2 dfg_AB)`</sub>

### `inline float3 EnergyCompensation(float3 F0, float2 dfg_AB)`
<sub>L1033</sub>

- **L1033** — BRDF: GGX base + clearcoat, optional anisotropy/MS-compensation, Burley diffuse/transmission/SSS, parallax shadow, thin film, rim, LTCGI, matcap.  <br/><sub>↳ before `half4 BRDF_Latex_GGX(`</sub>

### `half4 BRDF_Latex_GGX(`
<sub>L1061–L1222</sub>

- **L1061** — Polish layer master gate + per-pixel B&W mask. polish=0 collapses the whole polish layer to a flat GGX base: clearcoat off (so baseEnergy returns to 1), thin film neutral, no transmission, isotropic spec. Clearcoat/film/transmission/aniso scale here; SSS, rim, and multi-scatter pick it up below.  <br/><sub>↳ before `half polish = saturate(s.PolishMask);`</sub>
- **L1068** — Geometric specular AA: roughens normals based on screen-space variance.  <br/><sub>↳ before `half aBase   = GeometricSpecAA(N,  s.BaseRoughness, s.SpecAA);`</sub>
- **L1073** — Roughness squared (alpha2) - used in GGX D/V.  <br/><sub>↳ before `half a2_base = max(aBase   * aBase,   1e-5);`</sub>
- **L1080** — Thin film (Schlick base reflectance, wavelength-dependent phase).  <br/><sub>↳ before `half3 thinFilmColor = 1.0;`</sub>
- **L1092** — Parallax shadowing (POM-coupled self-shadowing) - gated on ParallaxDepth so a bound surface map with parallax disabled skips the tex2Dlod entirely.  <br/><sub>↳ before `float shadowTrace = 1.0;`</sub>
- **L1102** — Tinted dielectric clearcoat - white tint at F0=0.04 reproduces standard dielectric exactly.  <br/><sub>↳ before `half3 ccF0      = _CC_F0 * _CC_Tint.rgb;`</sub>
- **L1107** — Per-channel base attenuation; with a tinted coat this gives the under-layer a complementary cast.  <br/><sub>↳ before `half3 baseEnergy = 1.0 - ccFresEnv;`</sub>
- **L1110** — BASE LAYER - direct specular (GGX, optionally anisotropic)  <br/><sub>↳ before `float D_base;`</sub>
- **L1117** — Rotate world tangent by AnisoRotation around N to align with stretch direction.  <br/><sub>↳ before `float3 worldTangent   = s.WorldToTangent[0];`</sub>
- **L1125** — Anisotropic alpha split (Burley) - pass aBase, not a2_base; D_GGX_Aniso squares internally.  <br/><sub>↳ before `float ax = max(aBase * (1.0 + aniso), 1e-4);`</sub>
- **L1148** — BASE LAYER - direct diffuse (Burley)  <br/><sub>↳ before `float burley     = Burley_Diffuse(NdotV, NdotL, LdotH, aBase);`</sub>
- **L1152** — CLEARCOAT - direct specular (GGX isotropic)  <br/><sub>↳ before `float D_cc = D_GGX(NcH, a2_cc);`</sub>
- **L1158** — SSS - wrap + back-scatter  <br/><sub>↳ before `float wrap = saturate((NdotL + _SSS_Dist) / max(1e-5, 1.0 + _SSS_Dist));`</sub>
- **L1166** — Transmission - back-light through thin parts (Burley/Filament)  <br/><sub>↳ before `half3 transmission = 0;`</sub>
- **L1170** *(inline)* — back-side illumination via flipped normal
- **L1171** *(inline)* — Beer-Lambert absorption
- **L1172** *(inline)* — view-aligned back-light falloff
- **L1178** — Rim - fake atmospheric edge  <br/><sub>↳ before `half rimExponent = lerp(30.0, 0.1, saturate(_Rim_Power / 10.0));`</sub>
- **L1184** — Indirect - Karis split-sum env BRDF. gi.specular is raw IBL (no Fresnel); we multiply F here.  <br/><sub>↳ before `float2 dfg_base = EnvBRDFApprox_AB(aBase,   NdotV);`</sub>
- **L1190** — Multi-scatter compensation (Filament). Skipped when toggle off.  <br/><sub>↳ before `half3 baseMS = 1.0;`</sub>
- **L1198** — Indirect base specular (energy-attenuated by clearcoat).  <br/><sub>↳ before `half3 indirectBaseSpec = gi.specular * envBRDF_base * baseEnergy * baseSpecOcc * baseMS;`</sub>
- **L1201** — Indirect clearcoat specular (uses its own roughness-mip env color).  <br/><sub>↳ before `half3 indirectCCSpec = clearcoatEnv * envBRDF_cc * thinFilmColor * ccSpecOcc;`</sub>
- **L1204** — Poiyomi/Mochie packed-map masks - specular mask dims direct light highlights, reflection mask dims environment/probe reflections (incl. clearcoat env, Light Volume, and LTCGI specular). Both are 1.0 (no effect) unless _UsePackedMasks is on.  <br/><sub>↳ before `half specMask = s.SpecularMask;`</sub>
- **L1208** — Combine  <br/><sub>↳ before `half3 finalColor =`</sub>
- **L1210** *(inline)* — indirect diffuse (Poiyomi-realistic: raw scalar AO, no multi-bounce)
- **L1211** *(inline)* — direct diffuse (Burley)
- **L1222** — LTCGI (area lights)  <br/><sub>↳ before `#if defined(LTCGI_ENABLE)`</sub>

### `#endif`
<sub>L1241–L1243</sub>

- **L1241** — === WORLD-LIGHTING INTEGRATIONS ===  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>
- **L1243** — VRSL GI WASH - the DMX fixtures' colour spilling onto the suit as real additive light (a stage wash), distinct from the emission "stage hijack" in surf(). Reuses the same DMX grid + channel offsets (base+3/4/5 RGB) the hijack reads, so wash and hijack agree. Keyword-gated (heavy, stripped when VRSL unused) + runtime float gate (VRCFury) + a liveness probe on the grid's TexelSize so a world with no DMX node contributes nothing.  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>

### `#if defined(VRSL_ENABLE)`
<sub>L1252</sub>

- **L1252** — Desaturate toward luma so the wash tints the suit to the stage colour without nuking its own design (_VRSL_GI_Sat=1 keeps full DMX colour).  <br/><sub>↳ before `half vrslLum = dot(vrslCol, half3(0.299, 0.587, 0.114));`</sub>

### `#endif`
<sub>L1263–L1279</sub>

- **L1263** — AREALIT (PiMaker area lights) - analytic LTC, same role as LTCGI but the data is per-material: point _AreaLit_LightMesh + _AreaLit_LightTex0 at the world's AreaLit RTs. Keyword-gated (heavy 16-quad loop, stripped when _AreaLit_Int==0 via the editor). With no LightMesh assigned, ShadeAreaLitLatex's first .Load is 0 and it contributes nothing.  <br/><sub>↳ before `#if defined(AREALIT_ENABLE)`</sub>
- **L1275** — Matcap  <br/><sub>↳ before `half3 matcapEval = matcap * saturate(gi.diffuse + light.color * smoothstep(0.0, 0.15, NcL)) * baseSpecOcc;`</sub>
- **L1279** — Emission + AL neon overlay  <br/><sub>↳ before `finalColor += s.Emission * _Emis_Exp;`</sub>

### `void LightingStandardLatex_GI(SurfaceOutputStandardLatex s, UnityGIInput data, inout UnityGI gi)`
<sub>L1287–L1301</sub>

- **L1287** — Same mirror-camera fix as LightingStandardLatex - UnityGIInput.worldViewDir was filled from _WorldSpaceCameraPos and drives the indirect specular reflection direction below.  <br/><sub>↳ before `data.worldViewDir = vw_WorldViewDir(s.WorldPos);`</sub>
- **L1292** — Light Volume diffuse (pre-baked into s.LVDiffuse in surf) - Additive mode ADDs to Unity's probe diffuse (volumes layer on top); Full/deringed mode REPLACES it (LV is the authoritative SH source).  <br/><sub>↳ before `if (s.LVActive > 0.5)`</sub>
- **L1301** — Roughness-blurred IBL (no Fresnel - applied per-layer in BRDF). Occlusion=1 here; specOcc is per-layer.  <br/><sub>↳ before `Unity_GlossyEnvironmentData g =`</sub>

### `inline half4 LightingStandardLatex(SurfaceOutputStandardLatex s, half3 viewDir, UnityGI gi)`
<sub>L1310</sub>

- **L1310** — Unity's surface-shader plumbing computes incoming viewDir from _WorldSpaceCameraPos in the generated vertex stage (wrong in VRChat mirrors); reproject from the actual rendering camera so clearcoat reflections and BRDF NdotV are correct.  <br/><sub>↳ before `viewDir = vw_WorldViewDir(s.WorldPos);`</sub>

### `#endif`
<sub>L1325–L1338</sub>

- **L1325** — Alpha workflow branches by mode keyword - Opaque+Cutout force outputAlpha=1 (SubShader Blend is One/Zero so value would be discarded, but explicit avoids surprises); Fade uses straight alpha (SrcAlpha/OneMinusSrcAlpha); Transparent uses Unity's PreMultiplyAlpha so specular survives at low opacity.  <br/><sub>↳ before `half outputAlpha = 1.0;`</sub>
- **L1338** — Safe vector indexing macro to bypass HLSL arrayification bugs  <br/><sub>↳ before `#define GET_AL_BAND(vec, bandIdx) ( \`</sub>

### `#define GET_AL_BAND(vec, bandIdx) ( \`
<sub>L1345</sub>

- **L1345** — SURFACE FUNCTION  <br/><sub>↳ before `void surf (Input IN, inout SurfaceOutputStandardLatex o)`</sub>

### `void surf (Input IN, inout SurfaceOutputStandardLatex o)`
<sub>L1355–L1412</sub>

- **L1355** — Animation time stays on real time; chronotensity is opt-in per FX via _UseChronoFX.  <br/><sub>↳ before `float animTime = _Time.y;`</sub>
- **L1360** — AudioLink bands (zeroed by default; FetchAudioLinkBands only runs when the master toggle is on).  <br/><sub>↳ before `float4 amps = float4(0,0,0,0);`</sub>
- **L1372** — DFT note pull-out (across all octaves), used to bias emission  <br/><sub>↳ before `float dftAmp = 0.0;`</sub>
- **L1393** — Standard time-driven UV scroll (chronotensity drive removed - was unpredictable).  <br/><sub>↳ before `baseUV += float2(_SpeedX, _SpeedY) * _Time.y;`</sub>
- **L1396** — Bio pulse  <br/><sub>↳ before `half heartbeat  = amps.x * 0.65 + amp_emis * 0.35;`</sub>
- **L1404** — Audio Color Blend cycles AL tint through rainbow (time + bio + worldPos.y). Applied before VRSL hijack.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && _AL_Col_Blend > 0.001)`</sub>
- **L1412** — VRSL color hijack (DMX colour wash override for AL color)  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>

### `#endif`
<sub>L1425–L1808</sub>

- **L1425** — (Geometry-level primID fracture clip removed - broke under tessellation. Per-pixel noise clip below handles shards.)  <br/><sub>↳ before `float2 cUV = baseUV;`</sub>
- **L1427** — UV AUDIO DISTORTION CHAIN: vortex → pump → fracture → rotation → glitch tear → parallax (compounding).  <br/><sub>↳ before `float2 cUV = baseUV;`</sub>
- **L1430** — Per-fragment fracture pop mask - read by parallax stage; declared outside AL guard.  <br/><sub>↳ before `float fracturePop = 0;`</sub>
- **L1433** — UV distortion effects all funnel through band amplitudes which are zero when _UseAudioLink is off.  <br/><sub>↳ before `if (_UseALVortex > 0.5)`</sub>
- **L1441** — Radial falloff - centre twists hardest. Chrono FX adds an oscillating breath.  <br/><sub>↳ before `float chronoMod = (_UseChronoFX > 0.5) ? sin(GET_AL_BAND(chronos, _AL_Vortex_Band) * UNITY_PI) : 1.0;`</sub>
- **L1450** — Radial scale around pump centre: pump<1 zooms in, pump>1 zooms out.  <br/><sub>↳ before `float bandAmp = GET_AL_BAND(amps, _AL_Pump_Band);`</sub>
- **L1462** — Two-axis slice hash advancing with time so shards re-roll instead of locking.  <br/><sub>↳ before `float2 fUV = TransformUV(cUV, _AL_Fracture_UV);`</sub>
- **L1474** — Shard mask drives a tiny parallax pop (read at o.ParallaxDepth below).  <br/><sub>↳ before `fracturePop = fractureMask;`</sub>
- **L1479** — UV rotation applied after audio distortions so it composes with vortex/pump. Vortex+ChronoFX adds an audio-driven spin (~8.6 deg/unit).  <br/><sub>↳ before `float uvRotDeg = _UV_Rot;`</sub>
- **L1486** — Glitch UV tear - X skews with live waveform, Y micro-wobble reads as VHS tracking.  <br/><sub>↳ before `float2 glitchOffset = 0;`</sub>
- **L1506** — Parallax over audio-distorted UV (fracturePop pushes shards a hair off the surface) - IN.viewDir would derive from _WorldSpaceCameraPos and break parallax in VRChat mirrors; vw_WorldViewDir reads the actual rendering camera via UNITY_MATRIX_I_V instead.  <br/><sub>↳ before `float3 viewDirWorld   = vw_WorldViewDir(IN.worldPos);`</sub>
- **L1512** — Base textures  <br/><sub>↳ before `fixed4 c      = UNITY_SAMPLE_TEX2D(_MainTex, finalUV) * _Color;`</sub>
- **L1516** — Fracture dissolve clip - the body opens up as the fracture progresses (manual _Vtx_Fracture_Amount plus AudioLink jitter). On non-SPS the removed region flies off as real shards in PASS 4; on SPS it simply dissolves.  <br/><sub>↳ before `float fracProg = saturate(_Vtx_Fracture_Amount + (_UseAudioLink > 0.5 ? GET_AL_BAND(amps, _Vtx_Fracture_Band) * _Vtx_Fracture_Str * 0.2 : 0…`</sub>
- **L1524** — Alpha workflow - Cutout: hard clip on _CutOff (also clips addshadow so shadows match silhouette); Fade/Transparent: discard fully invisible pixels so the shadow caster doesn't punch opaque shadow holes; Opaque: no clip, alpha ignored.  <br/><sub>↳ before `#if defined(_ALPHATEST_ON)`</sub>
- **L1532** — ShadowCaster/depth passes only need alpha for the cutout clips handled above. Everything  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1533** — below is per-pixel surface + world-light prep that is dead code in those passes - but  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1534** — `addshadow` compiles this entire surf into the generated ShadowCaster, which (stacked with  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1535** — tessellation + the world-light includes) bloats that snippet enormously and pushes the  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1536** — shader compiler toward the OOM that crashes it on import. Bail out so depth stays cheap.  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1537** — Mirrors the same guard in "VixenWear Latex SPS.shader" - keep the two in sync.  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1542** — Poiyomi-style multi-region color mask - RGB zones each multiply a tint into albedo and contribute emission boost later; channels are independent so overlapping zones stack.  <br/><sub>↳ before `float regionEmis = 0;`</sub>
- **L1547** — Channels are independent masks (not blended) so authors can paint hard-edged feature zones.  <br/><sub>↳ before `float3 regionTint = lerp(float3(1,1,1), _Region_R_Tint.rgb, regionSample.r)`</sub>
- **L1559** — Metallic / smoothness with channel-selectable Poiyomi-pack support + AL modulation.  <br/><sub>↳ before `float pbrMet    = ChannelPick(packed, _PBR_Met_Ch);`</sub>
- **L1568** — AO (channel selectable); "None" (channel 4) yields a constant 1.0 so Poiyomi/Mochie packs without an AO channel don't read a wrong channel.  <br/><sub>↳ before `float pbrAO = (_PBR_AO_Ch > 3.5) ? 1.0 : ChannelPick(packed, _PBR_AO_Ch);`</sub>
- **L1574** — Height (channel selectable; parallax raymarch and BRDF shadow trace use the same channel).  <br/><sub>↳ before `float pbrHeight = ChannelPick(packed, _PBR_Height_Ch);`</sub>
- **L1578** — Poiyomi/Mochie packed-map masks - reads reflection + specular masks from the packed PBR map so a Mochie "Metallic Maps" texture (R:Met G:Smooth B:ReflMask A:SpecMask) drives our masking. Default off keeps both masks neutral (1.0); applied in the BRDF combine - reflection mask dims environment/probe specular, specular mask dims direct highlights.  <br/><sub>↳ before `o.ReflectionMask = 1.0;`</sub>
- **L1592** — Normals  <br/><sub>↳ before `float3 normalTS = UnpackNormal(tex2D(_BumpMap, finalUV));`</sub>
- **L1604** — Clearcoat + thin film with AL modulation  <br/><sub>↳ before `o.ClearcoatStrength   = saturate(_CC_Strength + amp_shat * _AL_CC_Shatter);`</sub>
- **L1611** — Thickness (SSS) from bio pulse  <br/><sub>↳ before `o.Thickness = bio;`</sub>
- **L1614** — Anisotropic specular controls (latex stretch direction).  <br/><sub>↳ before `o.Anisotropy    = _Aniso;`</sub>
- **L1618** — Transmission (thin-part back-light), modulated by bio so SSS bleeds through audio-reactive regions.  <br/><sub>↳ before `o.Transmission = saturate(_Trans_Str + bio * 0.1);`</sub>
- **L1621** — Polish layer master gate + B&W mask - sampled once here, applied to the whole polish layer in the BRDF. Default white mask + toggle on = 1 (full polish, historical look).  <br/><sub>↳ before `o.PolishMask = _UsePolish * ChannelPick(UNITY_SAMPLE_TEX2D_SAMPLER(_PolishMask, _MainTex, finalUV), _PolishMaskCh);`</sub>
- **L1624** — WET - full "soaked / just out of the shower" look plus run-off rivulets. The soak (darken + near-mirror gloss + water-film sheen + flattened micro-normal) covers the whole masked area; animated UV-vertical rivulets add concentrated run-off streaks on top. UV-space keeps it stable on skinned avatars. Own toggle so it costs nothing when off.  <br/><sub>↳ before `if (_UseDrip > 0.5)`</sub>
- **L1630** — Run-off rivulets: animated vertical streaks where extra water is pouring down. Computed first; the normal tilt is applied last so streaks still pop over the flattened film.  <br/><sub>↳ before `float rivulet = 0;`</sub>
- **L1638** — Coverage gate - only a fraction of columns carry a rivulet.  <br/><sub>↳ before `float hasCol  = step(1.0 - saturate(_Drip_Coverage), colHash);`</sub>
- **L1640** — Gaussian rivulet across the column (centre is wettest); higher _Drip_Width = thinner streak.  <br/><sub>↳ before `float xInCol  = frac(colF) - 0.5;`</sub>
- **L1643** — Downward flow - per-column speed/phase variance so streaks don't march in lockstep.  <br/><sub>↳ before `float flow    = finalUV.y - _Time.y * _Drip_Speed * (0.6 + colHash) - colHash * 7.0;`</sub>
- **L1645** — Travelling beads so it reads as running water; 0.35 floor keeps a continuous trickle between beads.  <br/><sub>↳ before `float bead    = sin(flow * 18.0) * 0.5 + 0.5;`</sub>
- **L1649** — Gaussian derivative across the streak - rounds it so it catches a glint.  <br/><sub>↳ before `rivuletSlope  = clamp(-2.0 * xInCol * _Drip_Width * ridge * hasCol, -4.0, 4.0);`</sub>
- **L1653** — Total wetness: global soak + rivulet streaks, masked and clamped.  <br/><sub>↳ before `float wetness = saturate(_Wet_Amount + rivulet) * wetMaskTex;`</sub>
- **L1657** — 1. Water absorption darkens the surface (deeper in the most-soaked areas).  <br/><sub>↳ before `o.Albedo *= lerp(1.0, 1.0 - _Wet_Darken * 0.65, wetness);`</sub>
- **L1659** — 2. A water film is near-mirror smooth - drive smoothness toward the wet target.  <br/><sub>↳ before `o.Smoothness    = lerp(o.Smoothness, _Wet_Smoothness, wetness);`</sub>
- **L1662** — 3. The film fills micro-detail, flattening the shading normal toward the surface.  <br/><sub>↳ before `o.Normal = normalize(lerp(o.Normal, float3(0,0,1), wetness * _Wet_Flatten));`</sub>
- **L1664** — 4. The thin water sheet reads as an extra dielectric clearcoat (F0~0.04 = water), giving the bright wet Fresnel sheen. Gated by the Polish layer in the BRDF.  <br/><sub>↳ before `o.ClearcoatStrength = saturate(o.ClearcoatStrength + wetness * _Wet_Sheen);`</sub>
- **L1666** — Run-off streak tilt applied last so it survives the film flattening.  <br/><sub>↳ before `o.Normal = normalize(o.Normal + float3(rivuletSlope * _Drip_Normal * 0.15, 0, 0));`</sub>
- **L1672** — Matcap - world-anchored sphere mapping. The basis vectors come from view-direction + world-up instead of UNITY_MATRIX_V, because UNITY_MATRIX_V carries the camera's full rotation including roll - head tilt in VR (or any camera roll) would spin the matcap pattern around the view axis, making highlights swim instead of staying world-locked the way a real metal/latex surface would behave. vw_WorldViewDir reads from the actual rendering camera (UNITY_MATRIX_I_V), so this stays mirror-correct.  <br/><sub>↳ before `float3 nWorld   = normalize(WorldNormalVector(IN, float3(0,0,1)));`</sub>
- **L1675** — Swap reference up when looking near-vertical so cross(refUp, viewDirW) doesn't collapse - using world Z as the fallback keeps the basis well-defined.  <br/><sub>↳ before `float3 refUp    = (abs(dot(viewDirW, float3(0,1,0))) > 0.999) ? float3(0,0,1) : float3(0,1,0);`</sub>
- **L1681** — Layer 1 - channel-selectable mask + per-layer tint.  <br/><sub>↳ before `float rad = _MatCap_Rot * (UNITY_PI / 180.0);`</sub>
- **Tiling + 3-axis scroll** — `_MatCap_Tiling.xy` repeats the matcap; `_MatCap_Scroll` drives smooth motion: `.x`/`.y` pan the UV (`+ _MatCap_Scroll.xy * _Time.y`) and `.z` is a continuous spin in degrees/sec folded into the rotation as `matcapSpin = _MatCap_Rot + fmod(_MatCap_Scroll.z * _Time.y, 360)`. A matcap is a 2D sphere projection with no real depth axis, so rotation is the only "third axis" that behaves like a scroll (continuous and one-directional); a zoom can't, because it would either run away or have to bounce. The rotation `mul` is split from the `+0.5` re-centre so tiling scales the rotated UV around the matcap centre (`* tiling + 0.5`) rather than the texture origin, otherwise tile != 1 pushes the highlight into the corner. The `fmod(..., 360)` keeps the spin angle bounded so sin/cos stay precise (no jitter) over long sessions. Defaults (Tiling `(1,1)`, Scroll `(0,0,0)`) reduce to the original static `mul(...) + 0.5`. Visible repeat at tile > 1 needs the matcap texture's Wrap Mode = Repeat.  <br/><sub>↳ before `matcapUV = matcapUV * _MatCap_Tiling.xy + 0.5 + _MatCap_Scroll.xy * _Time.y;`</sub>
- **L1689** — Matcap audio boost gated by the user emission amount - without it the surface still pulses when AL is on with all sliders at zero.  <br/><sub>↳ before `half3 matcap1 = matcapTex.rgb * _MatCap_Tint.rgb * matcap1Mask * _MatCap_Int * (1.0 + amp_emis * _AL_Emis_Mod * 0.5);`</sub>
- **L1693** — Layer 2 - independent matcap/mask channel/rotation/tint/blend mode; "Replace" blend uses the mask as a lerp so layer 2 takes over inside its mask zone.  <br/><sub>↳ before `if (_UseMatCap2 > 0.5)`</sub>
- **L1707** *(inline)* — Replace inside mask
- **L1709** *(inline)* — Multiply inside mask
- **L1711** *(inline)* — Add (default)
- **L1714** — EMISSION - autocorrelator vertically warps the emission UV so circuitry breathes without recolouring.  <br/><sub>↳ before `float2 emisUV = finalUV;`</sub>
- **L1718** — autoCorr is zero-centered via the 0.007 scale (matches the SPS variant); no -0.5 offset.  <br/><sub>↳ before `emisUV.y += autoCorr * _AL_AutoCorr_Mod * 0.2;`</sub>
- **L1724** — Manual surface emission: circuitry lines ONLY  <br/><sub>↳ before `float3 manualEmis = emisTex.rgb * _EmissionColor.rgb;`</sub>
- **L1731** — 1. BASE GLOW: Locked to circuitry lines  <br/><sub>↳ before `float3 emisBase = (manualEmis + alLayer) * emisMask;`</sub>
- **L1734** — Emission boost via bio pulse (heartbeat + tension + neuroSpike + chrono breath).  <br/><sub>↳ before `if (_UseAudioLink > 0.5)`</sub>
- **L1741** — Poiyomi-style secondary emission layer - independent texture/color/mask, optional AL band reactor.  <br/><sub>↳ before `if (_UseEmission2 > 0.5)`</sub>
- **L1748** — Pull a band amp specifically for this layer so the artist can route bass/treble independently.  <br/><sub>↳ before `float amp_emis2 = GET_AL_BAND(amps, _AL_Band_Emis2);`</sub>
- **L1756** — Region mask emission boost - each painted zone multiplies local emission so the user can brighten specific feature areas (panels, claws, paw-print decals) without a second map.  <br/><sub>↳ before `if (_UseRegionMask > 0.5 && regionEmis > 0.001)`</sub>
- **L1762** — Dynamic effects bleed onto the emisMask.  <br/><sub>↳ before `float effectMask = emisMask;`</sub>
- **L1767** — CRT-bar scanline: smoothstep wave multiplied through emission. chr_scan is 0 unless ChronoFX is enabled.  <br/><sub>↳ before `float scanTime = fmod((_Time.y * _AL_Scan_Speed * 1.8) + (chr_scan * _AL_Scan_React * 0.8), 628.318);`</sub>
- **L1776** — Faint highlight on waveform peaks so the UV warp reads on dim backgrounds (decoration, not the main effect).  <br/><sub>↳ before `float waveformRipple = raw_waveform * _AL_Waveform_Mod;`</sub>
- **L1783** — Autocorrelator ripple → EMISSION block; glitch tear → UV AUDIO DISTORTION CHAIN above.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)`</sub>
- **L1785** — CYBER HUD now renders as real lifted geometry in its own pass (see "PASS 3: CYBER HUD HOVER" below) instead of being parallax-faked onto the surface here.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)`</sub>
- **L1787** — Amplitude-driven flicker sparkle on top of the steady AL emission (decoration only) - gated by _AL_Emis_Mod so users can fully disable AL emission response with the slider.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)`</sub>
- **L1797** — Clearcoat normal - flatten lerps the normal-mapped "skin" toward the smooth geometric normal.  <br/><sub>↳ before `float3 nClearcoat = normalize(nWorld);`</sub>
- **L1798** — _CC_Flat = 1 -> fully flat glassy coat (geometric normal); _CC_Flat = 0 -> coat rides the normal map.  <br/><sub>↳ before `float3 nClearcoat = normalize(nWorld);`</sub>
- **L1799** — Early-out on the default (1.0) end skips the unneeded normal-map mul; the lerp runs all the way to 0.  <br/><sub>↳ before `float3 nClearcoat = normalize(nWorld);`</sub>
- **L1803** *(inline)* — tangent → world: row vec * matrix
- **L1808** — LIGHT VOLUMES (stashes diffuse + base/clearcoat specular) - _LV_AdditiveOnly samples only additive volumes (preserves Unity probe baseline); _LV_Bias pushes along world normal as worldPosOffset to fix light bleed at sharp edges (matches official LV PBR); _LV_PosOffset is a manual world-space offset for thin/sleeve geometry; _LV_ProbeDering is an opt-in Bakery L1 fallback that swaps Unity SH9 for dering'd L0+L1 (without it, non-LV worlds keep Unity's full probe path preserving L2 detail and avoiding black-out from negative L1 reconstruction).  <br/><sub>↳ before `o.LVDiffuse = 0;`</sub>

### `#if defined(LIGHTVOLUMES_ENABLE)`
<sub>L1821–L1840</sub>

- **L1821** — World-space shaded normal (with normalmap) for diffuse fidelity.  <br/><sub>↳ before `float3 nWorldShaded = normalize(mul(o.Normal, o.WorldToTangent));`</sub>
- **L1824** — Normal-bias offset + user-provided manual offset.  <br/><sub>↳ before `float3 lvOffset = nWorldShaded * _LV_Bias + _LV_PosOffset.xyz;`</sub>
- **L1833** — Clamp evaluated diffuse to 0 - probe SH (especially Bakery's dering path) can produce negative values when L1 magnitude > L0, blacking out the avatar on default worlds.  <br/><sub>↳ before `o.LVDiffuse = max(LightVolumeEvaluate(nWorldShaded, lv_L0, lv_L1r, lv_L1g, lv_L1b), 0);`</sub>
- **L1837** — _WorldSpaceCameraPos is the player's head, not the mirror camera - route through the helper.  <br/><sub>↳ before `float3 worldViewDir = vw_WorldViewDir(IN.worldPos);`</sub>
- **L1840** — LV specular layers only fire when an actual LV system is in the scene - they need real L1 directionality, not dering'd probes which would duplicate Unity's reflection probes.  <br/><sub>↳ before `if (lvAvailable && _LV_Spec_Mix > 0.001)`</sub>

### `#endif`
<sub>L1861</sub>

- **L1861** — Store UV  <br/><sub>↳ before `o.UV = finalUV;`</sub>

### `ENDCG`
<sub>L1866</sub>

- **L1866** — PASS 2: CLEAR DRIP (geometry-amplified water droplets) - PC only. A real geometry stage emits camera-facing droplet billboards from downward-facing, wet-masked triangles; each droplet swells, forms a neck, pinches off, then falls away as free geometry and dries out (fades). Surface shaders cannot host a geometry stage, so this is its own custom vert/geom/frag pass. Runtime-gated by _UseDrip and _Drip3D_Strength so it stays VRCFury-animatable and emits zero vertices when off. Droplets are tinted to the clearcoat color.  <br/><sub>↳ before `Pass`</sub>

### `struct dripG2F`
<sub>L1910–L1912</sub>

- **L1910** *(inline)* — billboard local coords: x in [-1,1], y in [0,1] (top to bottom)
- **L1912** *(inline)* — x = beadCenterY, y = neck width factor, z = envelope alpha

### `void dripGeom(triangle dripV2G p[3], inout TriangleStream<dripG2F> stream)`
<sub>L1935–L2017</sub>

- **L1935** — Runtime gate - emit nothing when the effect is off.  <br/><sub>↳ before `if (_UseDrip < 0.5 \|\| _Drip3D_Strength < 0.0001) return;`</sub>
- **L1944** — Drips form on downward-facing surfaces - skip up-facing triangles.  <br/><sub>↳ before `float facingDown = saturate(-N.y);`</sub>
- **L1948** — Wet mask gate (same mask as the Wet layer).  <br/><sub>↳ before `float mask = dripChan(tex2Dlod(_DripMask, float4(uv, 0, 0)), _DripMaskCh);`</sub>
- **L1952** — Per-triangle identity + sparse coverage so droplets scatter instead of covering every triangle.  <br/><sub>↳ before `float h = dripHash(floor(C * 80.0));`</sub>
- **L1956** — Lifecycle phase (staggered per emitter).  <br/><sub>↳ before `float phase = frac(_Time.y * _Drip_Speed * (0.5 + h) + h);`</sub>
- **L1959** *(inline)* — 0 attached, 1 detached
- **L1965** — Sizes in world units (a droplet is a few millimetres).  <br/><sub>↳ before `float beadR = (0.5 + 0.5 * swell) * _Drip3D_Scale * 0.004;`</sub>
- **L1967** *(inline)* — neck length, retracts at pinch
- **L1968** *(inline)* — accelerating free-fall distance
- **L1972** — BODY SLIDE - while still attached, the bead clings and runs DOWN ALONG the surface (downhill tangent) rather than hanging straight from the centroid; a detached drop falls under gravity.  <br/><sub>↳ before `float3 hangDir = worldDown;`</sub>
- **L1984** — PHYSICS - sway (surface-tension wobble + breeze) grows with fall distance so a fresh bead barely moves while a long thread trails and swings.  <br/><sub>↳ before `float swayPh = _Time.y * 3.0 + h * 6.2831;`</sub>
- **L1991** — FLOOR COLLISION - when the bead reaches the shared world floor (_Goo_GroundY) it pins to the floor and splats into a spreading puddle that fades as it dries.  <br/><sub>↳ before `float splat = 0.0;`</sub>
- **L2000** — Camera-facing billboard basis with world-up kept vertical so the drop hangs naturally.  <br/><sub>↳ before `float3 viewDir = normalize(_WorldSpaceCameraPos - beadCenter);`</sub>
- **L2017** — SPLAT MORPH - collapse the vertical drop into a flat, ground-aligned puddle disc that grows as it spreads and fades out.  <br/><sub>↳ before `if (splat > 0.001)`</sub>

### `fixed4 dripFrag(dripG2F i) : SV_Target`
<sub>L2055–L2067</sub>

- **L2055** — Bead - a soft disc centred at (0, beadCenterY).  <br/><sub>↳ before `float2 bp = float2(x, (y - beadCenterY) / max(1.0 - beadCenterY, 1e-4));`</sub>
- **L2060** — Neck - a tapering column above the bead that vanishes as the drop pinches off.  <br/><sub>↳ before `float neckHalf = lerp(0.12, 0.5, saturate(y / max(beadCenterY, 1e-4))) * neckW;`</sub>
- **L2067** — Spherical normal across the bead for a glassy fresnel + reflection.  <br/><sub>↳ before `float2 sp = clamp(bp, -1.0, 1.0);`</sub>

### `ENDCG`
<sub>L2088</sub>

- **L2088** — PASS 3: CYBER HUD HOVER (geometry-amplified holographic shell) - PC only. Each body triangle whose centroid falls inside the Cyber mask is duplicated and pushed out along its world normal by _Cyber_Hover (plus a subtle bob), so the masked HUD window literally floats off the suit instead of being parallax-faked onto it; the five HUD layers (VU, Spectrum, Waveform, DMX, Autocorrelator) are drawn on that lifted shell. Surface shaders cannot host a geometry stage, so this is its own vert/geom/frag pass, runtime-gated by _UseCyber so it emits zero vertices when off. Kept off the SPS variant because VRCFury's SPS patcher rewrites the vertex stage.  <br/><sub>↳ before `Pass`</sub>

### `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`
<sub>L2121</sub>

- **L2121** — Safe vector indexing, mirror of the surf-pass GET_AL_BAND macro.  <br/><sub>↳ before `#define HUD_AL_BAND(vec, bandIdx) ( \`</sub>

### `#define HUD_AL_BAND(vec, bandIdx) ( \`
<sub>L2128</sub>

- **L2128** — HUD layer placement (offset/scale/rotation), identical to the surf-pass TransformHUD.  <br/><sub>↳ before `float2 HudTransform(float2 uv, float4 transform)`</sub>

### `float2 HudTransform(float2 uv, float4 transform)`
<sub>L2139–L2141</sub>

- **L2139** — Footprint placement only (offset + scale, rotation ignored). Effect bounds use this so spinning  <br/><sub>↳ before `float2 HudPlace(float2 uv, float4 transform)`</sub>
- **L2140** — an effect via Rotation never reshapes its lit/emission area - it only orients the meter graphic,  <br/><sub>↳ before `float2 HudPlace(float2 uv, float4 transform)`</sub>
- **L2141** — which is still sampled from the full HudTransform above.  <br/><sub>↳ before `float2 HudPlace(float2 uv, float4 transform)`</sub>

### `float2 HudPlace(float2 uv, float4 transform)`
<sub>L2147–L2149</sub>

- **L2147** — Per-effect ColorChord/Theme colour. Each HUD layer passes its own band so it can light up  <br/><sub>↳ before `float3 HudBandColor(int band)`</sub>
- **L2148** — with a different note colour; Theme and Strip modes ignore the band. Emission is the idle  <br/><sub>↳ before `float3 HudBandColor(int band)`</sub>
- **L2149** — fallback when AudioLink is off or paused.  <br/><sub>↳ before `float3 HudBandColor(int band)`</sub>

### `float3 HudBandColor(int band)`
<sub>L2167–L2168</sub>

- **L2167** — The VU meter listens to every band at once: an amplitude-weighted blend of the four band  <br/><sub>↳ before `float3 HudAllBandColor(float4 amps)`</sub>
- **L2168** — colours (a small floor keeps a silent mix as an even blend instead of going black).  <br/><sub>↳ before `float3 HudAllBandColor(float4 amps)`</sub>

### `float3 HudAllBandColor(float4 amps)`
<sub>L2177–L2178</sub>

- **L2177** — Band-independent feeds shared by every HUD layer: the four band amplitudes and the scrolling  <br/><sub>↳ before `void HudFetchAL(float2 uv, out float4 amps, out float raw_waveform)`</sub>
- **L2178** — raw waveform. Per-effect colour now comes from HudBandColor so each layer can pick its own band.  <br/><sub>↳ before `void HudFetchAL(float2 uv, out float4 amps, out float raw_waveform)`</sub>

### `void hudGeom(triangle hudV2G p[3], inout TriangleStream<hudG2F> stream)`
<sub>L2236–L2288</sub>

- **L2236** — Runtime gate - emit nothing when the HUD is off.  <br/><sub>↳ before `if (_UseCyber < 0.5) return;`</sub>
- **L2241** — Mask gate: lift any triangle with at least one corner on the white side of the mask, so  <br/><sub>↳ before `float m0 = tex2Dlod(_CyberMask, float4(p[0].uv, 0, 0)).r;`</sub>
- **L2242** — boundary triangles survive for the fragment stage to razor-clip and the shell never covers  <br/><sub>↳ before `float m0 = tex2Dlod(_CyberMask, float4(p[0].uv, 0, 0)).r;`</sub>
- **L2243** — the black (transparent) region of the body.  <br/><sub>↳ before `float m0 = tex2Dlod(_CyberMask, float4(p[0].uv, 0, 0)).r;`</sub>
- **L2249** — World-space lift distance along the surface normal, with the subtle bob from the old hover sliders.  <br/><sub>↳ before `float lift = _Cyber_Hover + sin(_Time.y * 1.6) * _Cyber_Hover * _Cyber_Hover_Bob * 0.25;`</sub>
- **L2268** — ===== LIVING VU CONSOLE =====  <br/><sub>↳ before `static const float3 VU_BG       = 0.033;`</sub>
- **L2269** — A self-playing AudioLink control panel ported from AudioLinkUI-Functions.cginc. The slider/handle INPUTS  <br/><sub>↳ before `static const float3 VU_BG       = 0.033;`</sub>
- **L2270** — (band thresholds, gain, hit-fade, exp-falloff) are fed live audio instead of user values, so the console  <br/><sub>↳ before `static const float3 VU_BG       = 0.033;`</sub>
- **L2271** — animates itself. MSDF icon buttons (power/reset/autogain) and the HSV theme pickers are omitted - they need  <br/><sub>↳ before `static const float3 VU_BG       = 0.033;`</sub>
- **L2272** — textures this shader doesn't ship. SDF primitives transcribed from the upstream panel.  <br/><sub>↳ before `static const float3 VU_BG       = 0.033;`</sub>
- **L2286** — Shared HDR glow multiplier so every HUD toggle reaches comparable brightness at a given  <br/><sub>↳ before `#define HUD_GLOW 10.0`</sub>
- **L2287** — intensity slider value. The VU console scales this up (its SDR panel palette tops out well  <br/><sub>↳ before `#define HUD_GLOW 10.0`</sub>
- **L2288** — below 1.0 once the dark background floor is subtracted, see hudFrag).  <br/><sub>↳ before `#define HUD_GLOW 10.0`</sub>

### `float vuTriRight(float2 p, float hw, float hh)`
<sub>L2369</sub>

- **L2369** — Top spectrum area: 4 threshold/crossover boxes + handles over the live DFT waveform. threshold[]/crossover[]/gain are audio-driven.  <br/><sub>↳ before `float3 vuDrawTopArea(float2 uv, float threshold[4], float crossover[4], float gain)`</sub>

### `float3 vuDrawTopArea(float2 uv, float threshold[4], float crossover[4], float gain)`
<sub>L2384</sub>

- **L2384** — if/else (not a ternary) so FXC dead-code-eliminates the xo[bi+1] read at bi==3 - a ternary evaluates both operands and reads xo[4] out of bounds (X3504).  <br/><sub>↳ before `float boxWidth;`</sub>

### `float3 vuDrawFourBandArea(float2 uv, float2 size)`
<sub>L2504</sub>

- **L2504** — Cheap hash used for the autocorrelator's electric fizzle sparks.  <br/><sub>↳ before `float hudHash21(float2 p)`</sub>

### `float hudHash21(float2 p)`
<sub>L2512</sub>

- **L2512** *(inline)* — normalized 0..1
- **L2512** *(inline)* — unused - keep for signature compatibility

### `float3 vuDrawAutoCorr(float2 uv /* normalized 0..1 */, float2 size /* unused - keep for signature compatibility */)`
<sub>L2514–L2548</sub>

- **L2514** — Expect uv to already be normalized. If not, call frac(uv) or use WorldUV before calling.  <br/><sub>↳ before `float2 normUV = uv;`</sub>
- **L2517** — Optional: tile the worldUV periodically  <br/><sub>↳ before `float2 mirroredUV = abs(2.0 * (normUV - 0.5));`</sub>
- **L2518** — normUV = frac(normUV);  <br/><sub>↳ before `float2 mirroredUV = abs(2.0 * (normUV - 0.5));`</sub>
- **L2520** — Mirror around center like the ring logic  <br/><sub>↳ before `float2 mirroredUV = abs(2.0 * (normUV - 0.5));`</sub>
- **L2523** — Sample autocorrelator consistently with the ring  <br/><sub>↳ before `float3 ac = AudioLinkLerp(ALPASS_AUTOCORRELATOR + float2(mirroredUV.x * AUDIOLINK_WIDTH, 0)).rrr;`</sub>
- **L2527** — Centerline is normalized  <br/><sub>↳ before `const float middle = 0.5;`</sub>
- **L2530** — Distance from centerline in normalized UV space  <br/><sub>↳ before `float edge0 = 0.003;`</sub>
- **L2531** — smoothstep expects edge0 < edge1  <br/><sub>↳ before `float edge0 = 0.003;`</sub>
- **L2535** *(inline)* — 0..1
- **L2536** — Optionally soften or sharpen the band  <br/><sub>↳ before `float acDistSoft = pow(acDist, 0.9); // tweak exponent for softness`</sub>
- **L2537** *(inline)* — tweak exponent for softness
- **L2548** — Lay out the console in a normalized panel and feed every slider live audio.  <br/><sub>↳ before `float3 vuDrawConsole(float2 uv, float4 amps, float vuLevel, float3 tint)`</sub>

### `float3 vuDrawConsole(float2 uv, float4 amps, float vuLevel, float3 tint)`
<sub>L2553–L2592</sub>

- **L2553** — ===== the "manipulate its sliders to match the audio" part =====  <br/><sub>↳ before `float threshold[4] = { amps.x, amps.y, amps.z, amps.w };       // box heights pulse per band`</sub>
- **L2554** *(inline)* — box heights pulse per band
- **L2555** *(inline)* — stable layout
- **L2556** *(inline)* — gain handle tracks the VU level
- **L2557** *(inline)* — bass drives hit-fade
- **L2558** *(inline)* — treble drives exp-falloff
- **L2592** — Gentle ColorChord/Theme tint so the console takes on the music's color.  <br/><sub>↳ before `color = lerp(color, color * (tint * 1.5 + 0.001), 0.25);`</sub>

### `fixed4 hudFrag(hudG2F i) : SV_Target`
<sub>L2601–L2779</sub>

- **L2601** — Razor-edged mask: a hard 0.5 cutoff with a 1px antialiased rim, so the HUD lands exactly  <br/><sub>↳ before `float maskRaw = tex2D(_CyberMask, hudUV).r;`</sub>
- **L2602** — on the white of the emission mask. Black is fully transparent (discarded) with no soft  <br/><sub>↳ before `float maskRaw = tex2D(_CyberMask, hudUV).r;`</sub>
- **L2603** — bleed past the edge; white shows at full strength. fwidth keeps the edge ~1px regardless  <br/><sub>↳ before `float maskRaw = tex2D(_CyberMask, hudUV).r;`</sub>
- **L2604** — of how blurry the mask texture's ramp is, collapsing it to the 0.5 contour.  <br/><sub>↳ before `float maskRaw = tex2D(_CyberMask, hudUV).r;`</sub>
- **L2615** — VU Meter  <br/><sub>↳ before `if (_UseCyberVU > 0.5)`</sub>
- **L2625** — Living AudioLink console, lifted from SDR into HDR (see consoleCol below). Listens to  <br/><sub>↳ before `float3 al_color = HudAllBandColor(amps);`</sub>
- **L2626** — all bands: overall level drives the gain handle, the all-band blend tints it.  <br/><sub>↳ before `float3 al_color = HudAllBandColor(amps);`</sub>
- **L2630** — The console palette is SDR and dominated by dark chrome (VU_BG); on an additive HUD that  <br/><sub>↳ before `float3 consoleCol = max(0.0, vuDrawConsole(cUV, amps, vu, al_color) - VU_BG);`</sub>
- **L2631** — floor reads as a dim grey wash, which is why the meter looked extremely dim even at  <br/><sub>↳ before `float3 consoleCol = max(0.0, vuDrawConsole(cUV, amps, vu, al_color) - VU_BG);`</sub>
- **L2632** — max intensity. Subtract it so only the lit content glows, then push it into HDR.  <br/><sub>↳ before `float3 consoleCol = max(0.0, vuDrawConsole(cUV, amps, vu, al_color) - VU_BG);`</sub>
- **L2638** — Multi-band bar - one horizontal lane per band, filled to its own level and lit in  <br/><sub>↳ before `float lane = saturate(vuUV.y) * 4.0;`</sub>
- **L2639** — its own ColorChord colour, so the bar displays every band across the HUD emission.  <br/><sub>↳ before `float lane = saturate(vuUV.y) * 4.0;`</sub>
- **L2649** — Spectrum (CC) bars  <br/><sub>↳ before `if (_UseCyberCC > 0.5)`</sub>
- **L2672** — Waveform  <br/><sub>↳ before `if (_UseCyberWave > 0.5)`</sub>
- **L2681** — The waveform feed is full-spectrum PCM, so the selected band breathes its amplitude  <br/><sub>↳ before `float wave = abs((waveUV.y - 0.5) - raw_waveform * lerp(0.1, 0.3, waveBand));`</sub>
- **L2682** — (and tints it) to give this layer a distinct band source.  <br/><sub>↳ before `float wave = abs((waveUV.y - 0.5) - raw_waveform * lerp(0.1, 0.3, waveBand));`</sub>
- **L2688** — DMX grid mini-readout  <br/><sub>↳ before `if (_UseCyberDMX > 0.5)`</sub>
- **L2697** — The DMX feed is VRSL data, not audio, so the selected band pulses the readout  <br/><sub>↳ before `hud += dmxSample * lerp(0.4, 1.0, dmxBand) * _Cyber_DMX_Str * HUD_GLOW;`</sub>
- **L2698** — brightness (floored so the grid stays legible) to give it a band source.  <br/><sub>↳ before `hud += dmxSample * lerp(0.4, 1.0, dmxBand) * _Cyber_DMX_Str * HUD_GLOW;`</sub>
- **L2703** — Autocorrelator scope ring - a polar-wrapped mirror of the in-world panel oscilloscope  <br/><sub>↳ before `if (_UseCyberAuto > 0.5)`</sub>
- **L2704** — trace (drawAutoCorrelatorArea / vuDrawAutoCorr): the autocorrelation swells a soft scope  <br/><sub>↳ before `if (_UseCyberAuto > 0.5)`</sub>
- **L2705** — line out from a baseline circle and the brightness tracks FilteredVU intensity.  <br/><sub>↳ before `if (_UseCyberAuto > 0.5)`</sub>
- **L2717** *(inline)* — Maps radial angle to linear 0-1
- **L2724** — Identical fetch + 0.007 deflection scale to the panel trace; abs() so the  <br/><sub>↳ before `acVal = abs(AudioLinkLerp(ALPASS_AUTOCORRELATOR + float2(acPos * AUDIOLINK_WIDTH, 0)).r * 0.007);`</sub>
- **L2725** — band swells symmetrically. FilteredVU drives brightness like the panel.  <br/><sub>↳ before `acVal = abs(AudioLinkLerp(ALPASS_AUTOCORRELATOR + float2(acPos * AUDIOLINK_WIDTH, 0)).r * 0.007);`</sub>
- **L2730** — Per-effect drivers: each effect listens to its OWN AudioLink band, so the user can route  <br/><sub>↳ before `float shimmerAmp   = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Shimmer_Band)   : 0.6;`</sub>
- **L2731** — bass / low-mid / high-mid / treble to shimmer / pop / sizzle / electrify independently, and  <br/><sub>↳ before `float shimmerAmp   = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Shimmer_Band)   : 0.6;`</sub>
- **L2732** — each is gated by its toggle. With no live AudioLink we fall back to an idle animated level so  <br/><sub>↳ before `float shimmerAmp   = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Shimmer_Band)   : 0.6;`</sub>
- **L2733** — every enabled effect stays visible while authoring in the editor.  <br/><sub>↳ before `float shimmerAmp   = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Shimmer_Band)   : 0.6;`</sub>
- **L2743** — POP: sharp beat flash that swells the ring and goes white-hot, driven by its band.  <br/><sub>↳ before `float pop = pow(saturate(popAmp), 3.0);`</sub>
- **L2747** — SIZZLE: crackling noise jitters the swell radius so the trace spits, scaled by its band.  <br/><sub>↳ before `float crackle = hudHash21(float2(floor(acPos * 90.0), floor(_Time.y * 28.0))) - 0.5;`</sub>
- **L2751** — Soft filled band around the baseline radius - the ring equivalent of the panel  <br/><sub>↳ before `const float baselineR = 0.6;`</sub>
- **L2752** — trace that swells out from its centerline as the correlation grows.  <br/><sub>↳ before `const float baselineR = 0.6;`</sub>
- **L2757** — SHIMMER: thin highlight bands chasing around the ring, intensity tied to its band.  <br/><sub>↳ before `float shimmer = pow(0.5 + 0.5 * sin(acPos * 36.0 - _Time.y * 6.0 + acVal * 400.0), 4.0) * shimmerAmp;`</sub>
- **L2760** — ELECTRIFY: lightning arc filaments crossing the disc, brightening with its band.  <br/><sub>↳ before `float arcField = sin(acPos * 64.0 + _Time.y * 9.0) + sin(r * 26.0 - _Time.y * 7.0 + acPos * 12.0);`</sub>
- **L2764** — POP blooms a soft halo just off the trace.  <br/><sub>↳ before `float halo = smoothstep(0.06 + pop * 0.06, 0.0, abs(bandDist)) * pop;`</sub>
- **L2767** — Base ring brightness; shimmer lifts it, pop punches it.  <br/><sub>↳ before `float bright = lerp(0.15, 1.0, max(vuI, autoBand));`</sub>
- **L2772** — SIZZLE sparks: rare bright specks skittering along the trace edge, density on its band.  <br/><sub>↳ before `float spark = pow(hudHash21(float2(floor(acPos * 160.0), floor(_Time.y * 36.0))), 9.0);`</sub>
- **L2777** *(inline)* — POP white-hot core
- **L2778** *(inline)* — SIZZLE electric-blue sparks
- **L2779** *(inline)* — ELECTRIFY arc filaments

### `ENDCG`
<sub>L2792</sub>

- **L2792** — PASS 4: FRACTURE SHARDS (geometry-amplified solid chunks) - PC only. Each triangle in the fracturing region (manual _Vtx_Fracture_Amount + AudioLink jitter) detaches as a real tetrahedral shard that tumbles around its centroid and flies outward along its face normal to a hover distance, while the main pass clips that region of the body away so the suit appears to break apart. Surface shaders cannot host a geometry stage, so this is its own vert/geom/frag pass, gated by _UseVtxKinetic and per-shard progress so it emits nothing where the suit is still intact. Kept off the SPS variant because VRCFury's SPS patcher rewrites the vertex stage.  <br/><sub>↳ before `Pass`</sub>

### `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`
<sub>L2831</sub>

- **L2831** — Rotate vector v around unit axis by angle (Rodrigues).  <br/><sub>↳ before `float3 shardRotate(float3 v, float3 axis, float angle)`</sub>

### `float3 shardRotate(float3 v, float3 axis, float angle)`
<sub>L2838</sub>

- **L2838** — Packed-map channel picker (mirror of the surf-pass ChannelPick - this pass is its own program).  <br/><sub>↳ before `inline float shardChannel(fixed4 packed, float ch)`</sub>

### `inline float shardChannel(fixed4 packed, float ch)`
<sub>L2844</sub>

- **L2844** — Hue-rotate an RGB color by 'angle' radians in YIQ space (cheap, no HSV stack). Drives shard color-mod.  <br/><sub>↳ before `float3 shardHueRotate(float3 col, float angle)`</sub>

### `float3 shardHueRotate(float3 col, float angle)`
<sub>L2855–L2856</sub>

- **L2855** — Shared shard motion: returns object-space displacement (push) for a chunk and outputs its tumble axis/angle and velocity direction.  <br/><sub>↳ before `void shardMotion(float3 center, float3 faceN, float h, float shardProg,`</sub>
- **L2856** — Keeps PASS 4 (solid shards) and PASS 5 (trails) in lockstep so a tail always trails its own shard.  <br/><sub>↳ before `void shardMotion(float3 center, float3 faceN, float h, float shardProg,`</sub>

### `void shardMotion(float3 center, float3 faceN, float h, float shardProg,`
<sub>L2863–L2878</sub>

- **L2863** — Outward fly-out, eased (sqrt pops fast then holds = hover), with a subtle bob.  <br/><sub>↳ before `float travel = sqrt(shardProg) * _Vtx_Fracture_Dist + sin(_Time.y * 1.3 + h * 6.2831) * 0.01 * shardProg;`</sub>
- **L2866** — Spiral: orbit the fly-out direction around object-up and add a helical rise.  <br/><sub>↳ before `const float3 up = float3(0.0, 1.0, 0.0);`</sub>
- **L2873** — Float: per-shard buoyant low-frequency drift on all axes.  <br/><sub>↳ before `push += float3(sin(_Time.y * 0.8 + h * 6.2831),`</sub>
- **L2878** — Lift: net vertical offset (animatable up/down).  <br/><sub>↳ before `push += up * (_Vtx_Fracture_Lift * shardProg);`</sub>

### `struct shardG2F`
<sub>L2907</sub>

- **L2907** *(inline)* — x = per-shard hash, y = detach progress

### `void shardGeom(triangle shardV2G p[3], inout TriangleStream<shardG2F> stream)`
<sub>L2933–L3010</sub>

- **L2933** — Per-shard hash from the grid-snapped centroid (stable per chunk).  <br/><sub>↳ before `float h = frac(sin(dot(floor(center * 23.0), float3(12.9898, 78.233, 37.719))) * 43758.5453);`</sub>
- **L2936** — AudioLink jitter layered on the manual amount.  <br/><sub>↳ before `float jitter = 0;`</sub>
- **L2947** — Stagger onset per shard; emit nothing until this shard detaches (the body still covers it).  <br/><sub>↳ before `float onset = h * 0.35;`</sub>
- **L2952** — Tumble + fly-out + spiral/float/lift (shared with the trail pass so a tail always follows its shard).  <br/><sub>↳ before `float3 push, axis, velDir; float ang;`</sub>
- **L2956** — Rotated/translated base verts (object space).  <br/><sub>↳ before `float3 v0 = center + shardRotate(p[0].opos - center, axis, ang) + push;`</sub>
- **L2961** — Tetra apex for thickness (along the rotated face normal).  <br/><sub>↳ before `float3 rotN = shardRotate(faceN, axis, ang);`</sub>
- **L2966** — Tangent basis from the base-tri UV gradient (rotated with the shard), reused for all faces - good enough for small tumbling chunks.  <br/><sub>↳ before `float3 te1 = p[1].opos - p[0].opos;`</sub>
- **L2977** — World-space verts.  <br/><sub>↳ before `float3 wv0 = mul(unity_ObjectToWorld, float4(v0, 1.0)).xyz;`</sub>
- **L2989** — Base  <br/><sub>↳ before `o.worldNormal = normalize(cross(wv1 - wv0, wv2 - wv0));`</sub>
- **L2996** — Side 1  <br/><sub>↳ before `o.worldNormal = normalize(cross(wv1 - wv0, wap - wv0));`</sub>
- **L3003** — Side 2  <br/><sub>↳ before `o.worldNormal = normalize(cross(wv2 - wv1, wap - wv1));`</sub>
- **L3010** — Side 3  <br/><sub>↳ before `o.worldNormal = normalize(cross(wv0 - wv2, wap - wv2));`</sub>

### `#endif`
<sub>L3030–L3079</sub>

- **L3030** — Region tints + region emission boost (mirror of the body surface).  <br/><sub>↳ before `float regionEmis = 0.0;`</sub>
- **L3041** — Metallic / smoothness from the packed PBR map (Poiyomi-style channel pick + invert).  <br/><sub>↳ before `fixed4 mg = tex2D(_MetallicGlossMap, uv);`</sub>
- **L3048** — Two-sided geometric normal (flip toward camera under Cull Off), then apply the tangent-space normal map.  <br/><sub>↳ before `float3 N = normalize(i.worldNormal);`</sub>
- **L3058** — Emission (map * color + region boost).  <br/><sub>↳ before `float3 emis = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _Emis_Exp;`</sub>
- **L3062** — Color-mod: per-shard hue cycle (speed 0 = static per-shard offset = shattered rainbow).  <br/><sub>↳ before `if (_Shard_ColorMod > 0.001)`</sub>
- **L3070** — AudioLink ColorChord: each shard takes a different live note color from the CC strip.  <br/><sub>↳ before `if (_UseShardCC > 0.5 && _UseAudioLink > 0.5 && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5) && AudioLinkIsAvailable())`</sub>
- **L3079** — Compact metallic-workflow BRDF + SH9 ambient - keeps shards consistent with the body without the full surface stack.  <br/><sub>↳ before `float3 Ldir = normalize(_WorldSpaceLightPos0.xyz);`</sub>

### `ENDCG`
<sub>L3096</sub>

- **L3096** — PASS 5: FRACTURE SHARD TRAILS (additive comet tails) - PC only. Optional per-shard streak trailing each flying chunk along its velocity, gated by _Vtx_Fracture_Trail (0 = off, emits nothing). Re-derives the exact PASS 4 motion via shardMotion so a tail always follows its own shard, and inherits the shard's hue-mod / ColorChord color. Separate additive pass so tails glow without disturbing the solid shards. Kept off the SPS variant for the same reason as the shard pass.  <br/><sub>↳ before `Pass`</sub>

### `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`
<sub>L3115–L3125</sub>

- **L3115** *(inline)* — plain sampler here (own program) so the geometry stage can use tex2Dlod - no derivatives in a geom shader.
- **L3125** — Duplicated from the shard pass - separate CGPROGRAMs cannot share functions; kept byte-for-byte identical so trails track shards exactly.  <br/><sub>↳ before `float3 shardRotate(float3 v, float3 axis, float angle)`</sub>

### `struct trailG2F`
<sub>L3180</sub>

- **L3180** *(inline)* — x = cross (-1..1), y = lengthwise (1 head -> 0 tail)

---

## `Shaders/VixenWear Latex SPS.shader`

*218 comment(s).*


### `(file scope)`
<sub>L1–L2</sub>

- **L1** — SPS-compatible variant of "VixenWear/Latex Ultra". Tessellation is removed because VRCFury's SPS patcher rewrites the surface pragma's vertex function to use SpsInputs but leaves tessellate: untouched, causing a "wrong parameter type" compile error. Keep in sync with "VixenWear Latex.shader" for any non-tess changes.  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra SPS"`</sub>
- **L2** — Built-in Render Pipeline only (VRChat targets Built-in); a #pragma surface shader cannot compile under HDRP/URP. World-lighting integrations (AudioLink, LTCGI, AreaLit, VRSL + VRSL GI, VRC Light Volumes) are all fail-safe: keyword-stripped or runtime-gated, each probing its data source for liveness so a world without a given system costs nothing.  <br/><sub>↳ before `Shader "VixenWear/Latex Ultra SPS"`</sub>

### `Properties`
<sub>L7</sub>

- **L7** — Rendering mode drives the alpha workflow - Opaque (no clip/blend), Cutout (clip on _CutOff), Fade (straight alpha - everything fades), Transparent (premultiplied - specular survives); defaults to Cutout for historical clip(c.a - _CutOff) behavior.  <br/><sub>↳ before `[Enum(Opaque,0,Cutout,1,Fade,2,Transparent,3)] _Mode ("Rendering Mode", Float) = 1`</sub>

### `[NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}`
<sub>L26</sub>

- **L26** — Poiyomi PBR Mask compatibility - per-channel selectors so Poiyomi/Substance/Marmoset-packed masks drop in without re-authoring; defaults match VixenWear's native packing (R:Met G:AO B:Disp A:Smooth).  <br/><sub>↳ before `[Enum(R,0,G,1,B,2,A,3)] _PBR_Met_Ch ("Metallic Channel", Float) = 0`</sub>

### `[Enum(R,0,G,1,B,2,A,3)] _PBR_Height_Ch ("Height Channel", Float) = 2`
<sub>L34</sub>

- **L34** — Poiyomi/Mochie packed-map masks - reflection mask dims environment/probe reflections, specular mask dims direct highlights. Channel defaults (B/A) match Mochie "Metallic Maps" packing (R:Met G:Smooth B:ReflMask A:SpecMask). Default off so existing materials are unchanged.  <br/><sub>↳ before `[Toggle] _UsePackedMasks ("Enable Reflection / Specular Masks", Float) = 0`</sub>

### `[Toggle] _UseMultiScatter ("Multi-Scatter Energy Compensation", Float) = 1`
<sub>L77</sub>

- **L77** — Polish layer master gate + B&W mask - scales the entire polish lighting layer (clearcoat, thin film, SSS, transmission, anisotropy, rim, multi-scatter) per-pixel. Toggle on + white mask preserves the historical look; runtime-gated (no keyword) so VRCFury can animate it.  <br/><sub>↳ before `[Toggle] _UsePolish ("Enable Polish Layer", Float) = 1`</sub>

### `[Enum(R,0,G,1,B,2,A,3)] _PolishMaskCh ("Polish Mask Channel", Float) = 0`
<sub>L82</sub>

- **L82** — Drip - procedural vertical rivulets that mimic water running off the latex (per-pixel wet streaks). Own toggle so off = no cost.  <br/><sub>↳ before `[Toggle] _UseDrip ("Enable Drip (Water Run-Off)", Float) = 0`</sub>

### `_Drip_Normal ("Drip Normal Bump", Range(0, 1)) = 0.5`
<sub>L93</sub>

- **L93** — Wet soak - global "just out of the shower/pool" wetness layered under the run-off rivulets above.  <br/><sub>↳ before `_Wet_Amount ("Wetness (Soaked)", Range(0, 1)) = 0.7`</sub>

### `_Wet_Flatten ("Wet Normal Flatten", Range(0, 1)) = 0.5`
<sub>L100</sub>

- **L100** — Goo - gravity-aligned vertex sag that mimics melting/runny latex or wax. Runs in disp(); own toggle.  <br/><sub>↳ before `[Toggle] _UseGoo ("Enable Goo (Melting Sag)", Float) = 0`</sub>

### `_Goo_GroundY ("Goo Ground Height (World Y)", Float) = 0`
<sub>L113</sub>

- **L113** — Goo physics + collision - ambient pendulum sway, surface-follow body collision, and a floor clamp with pooling. All default off so existing materials are unchanged; _Goo_GroundY is the shared world floor.  <br/><sub>↳ before `_Goo_Sway ("Goo Sway Amount", Range(0, 1)) = 0`</sub>

### `[NoScaleOffset] _EmissionMap ("Emission Map (RGB tint, A mask)", 2D) = "black" {}`
<sub>L129</sub>

- **L129** — Poiyomi-style secondary emission layer - independent texture, color, mask, and AL band reactor.  <br/><sub>↳ before `[Toggle] _UseEmission2 ("Enable Secondary Emission Layer", Float) = 0`</sub>

### `_AL_Emis2_Mod ("Emission 2 AL Amplitude", Range(0,1)) = 0.0`
<sub>L137</sub>

- **L137** — Poiyomi-style multi-region color mask - RGB zones each drive an albedo tint and emission boost.  <br/><sub>↳ before `[Toggle] _UseRegionMask ("Enable Multi-Region Color Mask", Float) = 0`</sub>

### `[NoScaleOffset] _MatCapMask ("MatCap 1 Mask", 2D) = "white" {}`
<sub>L149</sub>

- **L149** — Mask channel pick - defaults to R for single-channel mask compat; set to G/B/A to drive layer 1 from a different channel of an RGB region mask.  <br/><sub>↳ before `[Enum(R,0,G,1,B,2,A,3)] _MatCap_MaskCh ("MatCap 1 Mask Channel", Float) = 0`</sub>

### `_MatCap_Lit ("MatCap 1 Lighting Mix", Range(0,1)) = 1.0`
<sub>L156</sub>

- **L156** — Second matcap layer - own texture/mask/channel/tint/intensity/rotation/blend mode; common workflow drops the same red/blue/black region mask into both layers and picks R for layer 1, B for layer 2 so each zone shows a different matcap.  <br/><sub>↳ before `[Toggle] _UseMatCap2 ("Enable MatCap 2 Layer", Float) = 0`</sub>

### `_LTCGI_Diff_Mix ("LTCGI Diffuse Mix", Range(0,2)) = 1.0`
<sub>L177</sub>

- **L177** — AreaLit (PiMaker area lights) - point the two slots at the world's AreaLit LightMesh + video RenderTexture (AreaLit data is per-material, not a scene global). Keyword-gated by _AreaLit_Int > 0 via the editor.  <br/><sub>↳ before `[NoScaleOffset] _AreaLit_LightMesh ("AreaLit LightMesh RT", 2D) = "black" {}`</sub>

### `[VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Auto_Transform ("Autocorrelator Transform", Vector) = (0,0,1,0)`
<sub>L230</sub>

- **L230** — Per-effect reactors for the Autocorrelator HUD ring (the geometry HUD pass ships on the non-SPS shader; these keep the inspector and material copy/paste parallel between variants).  <br/><sub>↳ before `[Toggle] _Cyber_Auto_Shimmer ("AC Shimmer Effect", Float) = 1`</sub>

### `_AL_Glitch_Mod ("Digital Glitch Tear", Range(0,1)) = 0.0`
<sub>L293</sub>

- **L293** — Outline pass - Sylva-style Cull Front backface extrusion; toggle gates the entire variant so off = zero runtime cost.  <br/><sub>↳ before `[Toggle(_OUTLINE_ON)] _UseOutline ("Enable Outline", Float) = 0`</sub>

### `SubShader`
<sub>L308</sub>

- **L308** — Tags listed here are SubShader defaults - VixenWearEditor overrides RenderType/Queue/VRCFallback per material via SetOverrideTag to match the selected _Mode (Opaque/Cutout/Fade/Transparent).  <br/><sub>↳ before `Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }`</sub>

### `Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }`
<sub>L312</sub>

- **L312** — PASS 0: OUTLINE (Cull Front backface extrusion - Sylva-style). Keyword-gated by _OUTLINE_ON so the unused variant is the no-keyword default and costs nothing at runtime. Always-opaque blend so the outline is solid regardless of the material's selected alpha mode.  <br/><sub>↳ before `Cull Front`</sub>

### `CGPROGRAM`
<sub>L319</sub>

- **L319** — Minimal surface shader: no GI, no extra lights, no shadow/lightmap variants. Outline color goes to Emission; lighting fn returns black so the only contribution is the emission tint.  <br/><sub>↳ before `#pragma surface outlineSurf Outline keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap noshadowmask nometa …`</sub>

### `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRE…`
<sub>L324</sub>

- **L324** — Outline master toggle - when off, vertex skips extrusion and surface clips the pixel so the pass is effectively dead. Alpha keywords mirror the main pass so cutout textures don't cause outlines to float in transparent regions.  <br/><sub>↳ before `#pragma shader_feature_local _OUTLINE_ON`</sub>

### `#include "UnityCG.cginc"`
<sub>L331</sub>

- **L331** — AudioLink for optional emission boost - runtime-gated by _UseAudioLink so it costs nothing when AL isn't in scene.  <br/><sub>↳ before `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`</sub>

### `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`
<sub>L334</sub>

- **L334** — _MainTex_ST is auto-declared by the surface compiler because Input.uv_MainTex is present; redeclaring it (or any *_ST for a used uv) collides at the FORWARD pass.  <br/><sub>↳ before `sampler2D _MainTex;`</sub>

### `struct Input`
<sub>L351</sub>

- **L351** — None=0 (full strength), R/G/B/A=1..4 (matches inspector enum). Mirrored from main pass ChannelPick with the extra None slot for "no mask, just use everywhere".  <br/><sub>↳ before `inline float OL_ChannelPick(fixed4 packed, float ch)`</sub>

### `#if defined(_OUTLINE_ON)`
<sub>L364–L381</sub>

- **L364** — Eye-depth scaling keeps the outline a visually constant thickness at distance instead of vanishing.  <br/><sub>↳ before `float eyeDepth = -UnityObjectToViewPos(v.vertex.xyz).z;`</sub>
- **L368** — 0.0001 scale converts the 0-1000 slider into reasonable world-units; min() clamps so the outline doesn't blow up at far distance.  <br/><sub>↳ before `float wBase = lerp(0.0, _OutlineWidth    * 0.0001, saturate(_OutlineWidth));`</sub>
- **L376** — View fudge nudges the extruded shell toward the camera to mitigate z-fighting against the main pass when ZWrite is on for both.  <br/><sub>↳ before `float3 worldPos  = mul(unity_ObjectToWorld, v.vertex).xyz;`</sub>
- **L381** — Convert world-space offset back to object space without translation.  <br/><sub>↳ before `v.vertex.xyz += mul((float3x3)unity_WorldToObject, worldOffset);`</sub>

### `#endif`
<sub>L386</sub>

- **L386** — Black direct lighting - emission carries the visible color so the outline doesn't pick up scene lighting.  <br/><sub>↳ before `inline half4 LightingOutline(SurfaceOutput s, half3 lightDir, half atten)`</sub>

### `#if !defined(_OUTLINE_ON)`
<sub>L395</sub>

- **L395** — Toggle off: kill every fragment. Cheaper than letting the BRDF math run; the un-extruded backfaces would z-fight with the main pass anyway.  <br/><sub>↳ before `clip(-1);`</sub>

### `#endif`
<sub>L401–L406</sub>

- **L401** — Match the main pass cutout behavior so the outline respects the same alpha test.  <br/><sub>↳ before `#if defined(_ALPHATEST_ON)`</sub>
- **L406** — Optional AL emission boost - runtime-gated, no keyword variant. Uses raw band amplitude (no Chronotensity) to keep this pass cheap.  <br/><sub>↳ before `half3 alBoost = 0;`</sub>

### `ENDCG`
<sub>L422–L427</sub>

- **L422** — Blend/ZWrite are property-driven so the editor flips them per-material without a recompile - Opaque/Cutout use One/Zero/ZWrite On; Fade uses SrcAlpha/OneMinusSrcAlpha/ZWrite Off; Transparent uses One/OneMinusSrcAlpha/ZWrite Off.  <br/><sub>↳ before `Cull Off`</sub>
- **L427** — PASS 1: CORE PBR SURFACE (BASE SUIT, FRACTURE CLIP)  <br/><sub>↳ before `CGPROGRAM`</sub>

### `CGPROGRAM`
<sub>L429–L431</sub>

- **L429** — Surface pragma drops Deferred/Meta + LIGHTMAP/DIRLIGHTMAP/SHADOWMASK/LPPV variants (VRChat forward-only, avatar clothing never lightmapped); keepalpha preserves LightingStandardLatex alpha so Fade/Transparent get real alpha. noforwardadd skips the ForwardAdd pass entirely (avatar gets directional + probes + LV + LTCGI; loses realtime per-light additive contributions) - critical for ps_5_0 sampler budget because ForwardAdd's POINT/POINT_COOKIE + SHADOWS_CUBE built-in samplers stacked on our 13 texture samplers blew past the 16-register cap.  <br/><sub>↳ before `#pragma surface surf StandardLatex keepalpha addshadow noforwardadd vertex:disp exclude_path:deferred exclude_path:prepass nolightmap nodyn…`</sub>
- **L430** — Tessellation removed for SPS compatibility - VRCFury's SPS patcher rewrites vertex:disp but cannot rewrite tessellate:tessEdge, causing a struct type mismatch. Displacement still happens at vertex resolution via disp() and per-pixel via parallax raymarching.  <br/><sub>↳ before `#pragma surface surf StandardLatex keepalpha addshadow noforwardadd vertex:disp exclude_path:deferred exclude_path:prepass nolightmap nodyn…`</sub>
- **L431** — SPS variant intentionally drops fullforwardshadows: this is a (usually body-hidden) penetrator mesh, so soft/point/spot shadow-receiving variants aren't worth the per-variant compile cost. Main directional shadow still received. addshadow kept so cutout silhouettes still cast (cheap now - surf early-outs in the depth pass).  <br/><sub>↳ before `#pragma surface surf StandardLatex keepalpha addshadow noforwardadd vertex:disp exclude_path:deferred exclude_path:prepass nolightmap nodyn…`</sub>

### `#pragma target 5.0`
<sub>L435</sub>

- **L435** — Defensive against Unity 2022.3.x emitting lightmap/LOD variants despite the no* directives above. Cookie + cube-shadow variants are also skipped for sampler budget - any directional cookie / point cube shadow would add 1-2 samplers, and avatars don't typically use them.  <br/><sub>↳ before `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRE…`</sub>
- **Import-time trims (`only_renderers` + `SHADOWS_SOFT`)** — `only_renderers d3d11` follows every `#pragma target 5.0` (outline + surface) so Unity compiles one graphics API instead of the desktop set (gles3/metal/vulkan/glcore). This is the main lever for SPS import time: per `SpsPatcher.cs` the patched shader is compiled for every pass twice (`ShaderUtil.CompilePass` precheck + `ForceSynchronousImport`), so cutting the renderer count cuts that whole operation, and it is hash-cached so the cost lands once per shader edit. Tradeoff: a player on `-vulkan` / `-dx12` gets a broken shader (rare). `SHADOWS_SOFT` joins the skip_variants list to halve the ForwardBase shadow-receiving set. Do NOT skip `VERTEXLIGHT_ON`: `sps_light.cginc` needs the per-vertex light arrays (populated only in ForwardBase under VERTEXLIGHT_ON) for socket detection. Keep this in sync with the base shader (which also applies it to its PC-only geometry effect passes).  <br/><sub>↳ before `#pragma only_renderers d3d11`</sub>

### `#pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRE…`
<sub>L438</sub>

- **L438** — VRChat single-pass stereo / GPU instancing - required for avatar batching in VR.  <br/><sub>↳ before `#pragma multi_compile_instancing`</sub>

### `#pragma multi_compile_instancing`
<sub>L440–L447</sub>

- **L440** — SPS variant drops all world-lighting + detail keyword features (VRSL / LightVolumes / LTCGI /  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L441** — AreaLit / DetailNormal). Each was a shader_feature that multiplied the compiled variant count,  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L442** — and LightVolumes/LTCGI/AreaLit also dragged their heavy .cginc includes into every variant -  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L443** — the dominant cause of the ~225s import. A penetrator mesh doesn't need world reflections / DMX /  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L444** — micro-detail, so they're cut here. All their code is #if defined()-gated, so removing the pragmas  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L445** — compiles it out cleanly. (Re-add a line here if a given world system is ever wanted on this mesh.)  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L446** — AudioLink stays always-compiled + runtime-gated (no keyword variant) so VRCFury toggles still work.  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>
- **L447** — Alpha workflow keywords - set by VixenWearEditor based on _Mode. Mutually exclusive; Opaque mode = none on.  <br/><sub>↳ before `#pragma shader_feature_local _ALPHATEST_ON`</sub>

### `#endif`
<sub>L458–L464</sub>

- **L458** — AudioLink.cginc is always included (runtime-gated by _UseAudioLink) so VRCFury toggles work without keyword variants.  <br/><sub>↳ before `#include "Assets/VixenWear/Editor/cginc/AudioLink.cginc"`</sub>
- **L464** — VRChat mirror cameras leave _WorldSpaceCameraPos at the player's head - view-dependent math (specular, parallax, cubemap) renders wrong in the mirror; UNITY_MATRIX_I_V._m03_m13_m23 is the actual rendering camera world pos (per-eye correct under single-pass instanced).  <br/><sub>↳ before `float3 vw_CameraPos()    { return UNITY_MATRIX_I_V._m03_m13_m23; }`</sub>

### `struct Input`
<sub>L517–L555</sub>

- **L517** — _MainTex uses an explicit texture + sampler so the fragment-stage B&W masks (_PolishMask, _DripMask, _CyberMask) can borrow its sampler instead of each consuming one of the 16 ps_5_0 sampler registers. A borrowed sampler only resolves in a stage where its donor texture is actually sampled, so _GooMask keeps its own combined sampler: it is read in the vertex/displacement stage (and the auto-generated shadow caster), where _MainTex is not sampled. Net sampler count is unchanged versus before these effects: _CyberMask gives up its register, _GooMask takes one.  <br/><sub>↳ before `UNITY_DECLARE_TEX2D(_MainTex);`</sub>
- **L530** — Poiyomi compat: PBR mask channel selectors + invert toggles.  <br/><sub>↳ before `float _PBR_Met_Ch, _PBR_Met_Inv, _PBR_Smooth_Ch, _PBR_Smooth_Inv, _PBR_AO_Ch, _PBR_Height_Ch;`</sub>
- **L533** — Poiyomi compat: secondary emission layer + multi-region color mask.  <br/><sub>↳ before `float _UseEmission2, _Emis2_MaskCh, _AL_Band_Emis2, _AL_Emis2_Mod;`</sub>
- **L542** — Polish master gate + B&W mask, plus the drip (surface) and goo (vertex) latex effects.  <br/><sub>↳ before `float _UsePolish, _PolishMaskCh;`</sub>
- **L555** — AreaLit area lights (analytic LTC). Mix floats always declared (cheap); the data textures + math live in the keyword-gated include so they strip when unused. Included here - AFTER UNITY_DECLARE_TEX2D(_MainTex) above - because the vendored sampler borrows sampler_MainTex.  <br/><sub>↳ before `float _AreaLit_Int, _AreaLit_Spec_Mix, _AreaLit_Diff_Mix;`</sub>

### `#endif`
<sub>L587–L591</sub>

- **L587** — _Udon_DMXGridStrobeOutput dropped - declared but never sampled in this shader, just consumed a sampler register.  <br/><sub>↳ before `uniform sampler2D _Udon_DMXGridRenderTextureMovement;`</sub>
- **L591** — HELPERS  <br/><sub>↳ before `float FetchVRSLChannel(uint absoluteChannel, sampler2D tex, float4 texelSize)`</sub>

### `float2 RotateUVDeg(float2 uv, float deg)`
<sub>L647</sub>

- **L647** — Hue (0..1) to RGB - cheap triangle-wave approximation, no HSV stack required.  <br/><sub>↳ before `inline float3 HUEtoRGB(float h)`</sub>

### `inline float3 HUEtoRGB(float h)`
<sub>L657–L659</sub>

- **L657** — tessEdge() removed for SPS compatibility - see pragma comment above.  <br/><sub>↳ before `inline float ChannelPick(fixed4 packed, float ch)`</sub>
- **L659** — Poiyomi-style packed PBR channel picker. Channel index: 0=R, 1=G, 2=B, 3=A.  <br/><sub>↳ before `inline float ChannelPick(fixed4 packed, float ch)`</sub>

### `inline float ChannelPick(fixed4 packed, float ch)`
<sub>L668</sub>

- **L668** — Hash + smooth 3D value noise (0..1) driving the Goo melt's procedural per-strand variation.  <br/><sub>↳ before `float gooHash3(float3 p) { return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453); }`</sub>

### `float gooNoise3(float3 p)`
<sub>L690</sub>

- **L690** — Returns true if AudioLink should be considered active for this frame.  <br/><sub>↳ before `bool AL_Active()`</sub>

### `void FetchAudioLinkBands(out float4 amps, out float4 chronos, out float4 al_color, out float raw_waveform, out float autoCorr, float2 uv)`
<sub>L714–L756</sub>

- **L714** — stronger mapping for visible reaction  <br/><sub>↳ before `amps.x = saturate(pow(al_amps.x * 4.0, 0.35));`</sub>
- **L720** — Chronotensity is opt-in via _UseChronoFX to avoid 4 extra texture samples for amplitude-only users.  <br/><sub>↳ before `if (_UseChronoFX > 0.5)`</sub>
- **L731** — CCCOLORS index 0 is always black, so band → note is offset by +1.  <br/><sub>↳ before `if (colorMode == 1)`</sub>
- **L734** — Theme 0..3 live at uint2(0..3, 23), not CCCOLORS row+1.  <br/><sub>↳ before `else if (colorMode >= 2 && colorMode <= 5)`</sub>
- **L745** — Respect media state: when enabled, mute effects if media is NOT playing  <br/><sub>↳ before `if (_UseMediaState > 0.5 && _MediaPlaying < 0.5)`</sub>
- **L756** — Vertex displacement + AudioLink-driven pump/fracture/autocorrelator.  <br/><sub>↳ before `void disp(inout appdata_full v)`</sub>

### `void disp(inout appdata_full v)`
<sub>L761–L765</sub>

- **L761** — Base displacement from packed PBR map (channel chosen by _PBR_Height_Ch for Poiyomi-pack compat).  <br/><sub>↳ before `float dispHeight = ChannelPick(tex2Dlod(_MetallicGlossMap, float4(uv, 0, 0)), _PBR_Height_Ch);`</sub>
- **L765** — VRSL geometric warp  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>

### `#endif`
<sub>L777–L856</sub>

- **L777** — SPS variant: AudioLink vertex kinetics (pump / autocorrelator ripple) removed - they're vertex  <br/><sub>↳ before `if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)`</sub>
- **L778** — manipulation that conflicts with the injected SPS deformation on this mesh and added needless  <br/><sub>↳ before `if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)`</sub>
- **L779** — vertex compile cost. The matching _Use*/Vtx_* properties stay in the inspector for material  <br/><sub>↳ before `if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)`</sub>
- **L780** — copy/paste parity with the non-SPS shader; they're simply inert on this variant.  <br/><sub>↳ before `if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)`</sub>
- **L782** — GOO - melting/runny latex. Gravity-aligned, masked, and procedurally varied so it forms uneven runny tendrils. Range is dramatically extendable via _Goo_Reach, and it can optionally melt all the way down to the world ground plane (_Goo_ToGround). Runs in disp(); own toggle, independent of the AL kinetic gate.  <br/><sub>↳ before `if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)`</sub>
- **L788** — World position (for melt-to-ground) and world normal (downward-facing surfaces melt more).  <br/><sub>↳ before `float3 gooWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;`</sub>
- **L794** — PROCEDURAL GENERATION - coarse per-strand identity (coherent tendrils) plus two octaves of value noise for organic, uneven melting. _Goo_Variation blends from a uniform melt (0) to wildly varying strand lengths (1).  <br/><sub>↳ before `float3 gooNP = v.vertex.xyz * _Goo_Noise;`</sub>
- **L802** — Slow time wobble so the melt stays alive and runny; staggered per strand.  <br/><sub>↳ before `float wobble = 0.75 + 0.25 * sin(_Time.y * _Goo_Speed * 6.2831 + strandHash * 6.2831);`</sub>
- **L805** — Common melt weight (0..~1.5); some strands reach further than others.  <br/><sub>↳ before `float meltWeight = gooMask * faceWeight * strandReach * wobble * saturate(_Goo_Strength);`</sub>
- **L808** — DRAMATICALLY EXTENDED RANGE. Distance mode stretches down a large, settable distance (_Goo_Reach world units). Ground mode pulls each vertex down toward the world ground plane (Y = _Goo_GroundY) so strands reach the floor regardless of avatar height. Computed in world space, then converted to object space so non-uniform scale is handled.  <br/><sub>↳ before `float distDown   = _Goo_Reach * meltWeight;`</sub>
- **L813** — PHYSICS - lateral pendulum sway, growing with how far the strand has melted so the tip swings most, like a weighted strand. Staggered per strand so tendrils never move in lock-step.  <br/><sub>↳ before `float3 lateral = 0;`</sub>
- **L822** — BODY COLLISION (best-effort) - project the melt onto the surface tangent plane so goo flows ALONG the body instead of tunnelling straight through it (1 = pure surface flow, 0 = straight gravity).  <br/><sub>↳ before `if (_Goo_BodyFollow > 0.0001)`</sub>
- **L832** — FLOOR COLLISION - clamp the melted world position to the floor plane (_Goo_GroundY) and splay sideways into a shallow pool where it lands.  <br/><sub>↳ before `float3 meltedWP = gooWorldPos + meltWorld;`</sub>
- **L847** — Back to object space (handles non-uniform scale).  <br/><sub>↳ before `v.vertex.xyz += mul((float3x3)unity_WorldToObject, meltedWP - gooWorldPos);`</sub>
- **L852** — Static displacement  <br/><sub>↳ before `v.vertex.xyz += v.normal * d;`</sub>
- **L856** — PBR HELPERS  <br/><sub>↳ before `float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)`</sub>

### `float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)`
<sub>L859–L864</sub>

- **L859** — Derivatives are taken up front in uniform control flow so the tex2Dgrad calls inside the dynamic loop stay valid, and the function uses a single return path so FXC can prove every local is initialized (silences the "potentially uninitialized variable" warning in the shadow caster).  <br/><sub>↳ before `float2 dx = ddx(uv);`</sub>
- **L864** — Early-out when depth ~= 0 - otherwise the loop below re-samples the same texel up to 50 times (stepUVOffset collapses to zero) and exits only when the heightmap value rises above the descending layer height, burning ~35 tex2Dgrad samples per pixel on any non-white surface map.  <br/><sub>↳ before `[branch] if (parallaxDepth >= 1e-4)`</sub>

### `inline half HDRPSpecularOcclusion(half NdotV, half AO, half roughness)`
<sub>L902</sub>

- **L902** — Geometric specular AA - Toksvig-style filtering on screen-space normal derivative variance.  <br/><sub>↳ before `inline half GeometricSpecAA(float3 worldNormal, half roughness, half strength)`</sub>

### `inline half GeometricSpecAA(float3 worldNormal, half roughness, half strength)`
<sub>L914</sub>

- **L914** — GGX BRDF HELPERS: D=Trowbridge-Reitz, V=Smith Joint, F=Schlick, Diffuse=Burley, Indirect=Karis split-sum, MS=Filament.  <br/><sub>↳ before `inline float D_GGX(float NdotH, float a2)`</sub>

### `inline float V_SmithJointGGX(float NdotL, float NdotV, float a2)`
<sub>L928</sub>

- **L928** — Anisotropic GGX (Burley 2012)  <br/><sub>↳ before `inline float D_GGX_Aniso(float NdotH, float TdotH, float BdotH, float ax, float ay)`</sub>

### `inline float3 F_Schlick(float u, float3 F0)`
<sub>L955</sub>

- **L955** — Burley/Disney diffuse. Returns scalar (caller multiplies by NdotL and color).  <br/><sub>↳ before `inline float Burley_Diffuse(float NdotV, float NdotL, float LdotH, float roughness)`</sub>

### `inline float Burley_Diffuse(float NdotV, float NdotL, float LdotH, float roughness)`
<sub>L964</sub>

- **L964** — Karis split-sum env BRDF: AB.x = F0 scale, AB.y = bias; env_brdf = F0*AB.x + AB.y.  <br/><sub>↳ before `inline float2 EnvBRDFApprox_AB(float roughness, float NdotV)`</sub>

### `inline float3 EnvBRDFApprox(float3 F0, float roughness, float NdotV)`
<sub>L980</sub>

- **L980** — Filament/Frostbite multi-scatter compensation. Returns 1 + F0*((1-E)/E), E≈dfg_AB.x+dfg_AB.y.  <br/><sub>↳ before `inline float3 EnergyCompensation(float3 F0, float2 dfg_AB)`</sub>

### `inline float3 EnergyCompensation(float3 F0, float2 dfg_AB)`
<sub>L987</sub>

- **L987** — BRDF: GGX base + clearcoat, optional anisotropy/MS-compensation, Burley diffuse/transmission/SSS, parallax shadow, thin film, rim, LTCGI, matcap.  <br/><sub>↳ before `half4 BRDF_Latex_GGX(`</sub>

### `half4 BRDF_Latex_GGX(`
<sub>L1015–L1176</sub>

- **L1015** — Polish layer master gate + per-pixel B&W mask. polish=0 collapses the whole polish layer to a flat GGX base: clearcoat off (so baseEnergy returns to 1), thin film neutral, no transmission, isotropic spec. Clearcoat/film/transmission/aniso scale here; SSS, rim, and multi-scatter pick it up below.  <br/><sub>↳ before `half polish = saturate(s.PolishMask);`</sub>
- **L1022** — Geometric specular AA: roughens normals based on screen-space variance.  <br/><sub>↳ before `half aBase   = GeometricSpecAA(N,  s.BaseRoughness, s.SpecAA);`</sub>
- **L1027** — Roughness squared (alpha2) - used in GGX D/V.  <br/><sub>↳ before `half a2_base = max(aBase   * aBase,   1e-5);`</sub>
- **L1034** — Thin film (Schlick base reflectance, wavelength-dependent phase).  <br/><sub>↳ before `half3 thinFilmColor = 1.0;`</sub>
- **L1046** — Parallax shadowing (POM-coupled self-shadowing) - gated on ParallaxDepth so a bound surface map with parallax disabled skips the tex2Dlod entirely.  <br/><sub>↳ before `float shadowTrace = 1.0;`</sub>
- **L1056** — Tinted dielectric clearcoat - white tint at F0=0.04 reproduces standard dielectric exactly.  <br/><sub>↳ before `half3 ccF0      = _CC_F0 * _CC_Tint.rgb;`</sub>
- **L1061** — Per-channel base attenuation; with a tinted coat this gives the under-layer a complementary cast.  <br/><sub>↳ before `half3 baseEnergy = 1.0 - ccFresEnv;`</sub>
- **L1064** — BASE LAYER - direct specular (GGX, optionally anisotropic)  <br/><sub>↳ before `float D_base;`</sub>
- **L1071** — Rotate world tangent by AnisoRotation around N to align with stretch direction.  <br/><sub>↳ before `float3 worldTangent   = s.WorldToTangent[0];`</sub>
- **L1079** — Anisotropic alpha split (Burley) - pass aBase, not a2_base; D_GGX_Aniso squares internally.  <br/><sub>↳ before `float ax = max(aBase * (1.0 + aniso), 1e-4);`</sub>
- **L1102** — BASE LAYER - direct diffuse (Burley)  <br/><sub>↳ before `float burley     = Burley_Diffuse(NdotV, NdotL, LdotH, aBase);`</sub>
- **L1106** — CLEARCOAT - direct specular (GGX isotropic)  <br/><sub>↳ before `float D_cc = D_GGX(NcH, a2_cc);`</sub>
- **L1112** — SSS - wrap + back-scatter  <br/><sub>↳ before `float wrap = saturate((NdotL + _SSS_Dist) / max(1e-5, 1.0 + _SSS_Dist));`</sub>
- **L1120** — Transmission - back-light through thin parts (Burley/Filament)  <br/><sub>↳ before `half3 transmission = 0;`</sub>
- **L1124** *(inline)* — back-side illumination via flipped normal
- **L1125** *(inline)* — Beer-Lambert absorption
- **L1126** *(inline)* — view-aligned back-light falloff
- **L1132** — Rim - fake atmospheric edge  <br/><sub>↳ before `half rimExponent = lerp(30.0, 0.1, saturate(_Rim_Power / 10.0));`</sub>
- **L1138** — Indirect - Karis split-sum env BRDF. gi.specular is raw IBL (no Fresnel); we multiply F here.  <br/><sub>↳ before `float2 dfg_base = EnvBRDFApprox_AB(aBase,   NdotV);`</sub>
- **L1144** — Multi-scatter compensation (Filament). Skipped when toggle off.  <br/><sub>↳ before `half3 baseMS = 1.0;`</sub>
- **L1152** — Indirect base specular (energy-attenuated by clearcoat).  <br/><sub>↳ before `half3 indirectBaseSpec = gi.specular * envBRDF_base * baseEnergy * baseSpecOcc * baseMS;`</sub>
- **L1155** — Indirect clearcoat specular (uses its own roughness-mip env color).  <br/><sub>↳ before `half3 indirectCCSpec = clearcoatEnv * envBRDF_cc * thinFilmColor * ccSpecOcc;`</sub>
- **L1158** — Poiyomi/Mochie packed-map masks - specular mask dims direct light highlights, reflection mask dims environment/probe reflections (incl. clearcoat env, Light Volume, and LTCGI specular). Both are 1.0 (no effect) unless _UsePackedMasks is on.  <br/><sub>↳ before `half specMask = s.SpecularMask;`</sub>
- **L1162** — Combine  <br/><sub>↳ before `half3 finalColor =`</sub>
- **L1164** *(inline)* — indirect diffuse (Poiyomi-realistic: raw scalar AO, no multi-bounce)
- **L1165** *(inline)* — direct diffuse (Burley)
- **L1176** — LTCGI (area lights)  <br/><sub>↳ before `#if defined(LTCGI_ENABLE)`</sub>

### `#endif`
<sub>L1195–L1197</sub>

- **L1195** — === WORLD-LIGHTING INTEGRATIONS ===  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>
- **L1197** — VRSL GI WASH - the DMX fixtures' colour spilling onto the suit as real additive light (a stage wash), distinct from the emission "stage hijack" in surf(). Reuses the same DMX grid + channel offsets (base+3/4/5 RGB) the hijack reads, so wash and hijack agree. Keyword-gated (heavy, stripped when VRSL unused) + runtime float gate (VRCFury) + a liveness probe on the grid's TexelSize so a world with no DMX node contributes nothing.  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>

### `#if defined(VRSL_ENABLE)`
<sub>L1206</sub>

- **L1206** — Desaturate toward luma so the wash tints the suit to the stage colour without nuking its own design (_VRSL_GI_Sat=1 keeps full DMX colour).  <br/><sub>↳ before `half vrslLum = dot(vrslCol, half3(0.299, 0.587, 0.114));`</sub>

### `#endif`
<sub>L1217–L1233</sub>

- **L1217** — AREALIT (PiMaker area lights) - analytic LTC, same role as LTCGI but the data is per-material: point _AreaLit_LightMesh + _AreaLit_LightTex0 at the world's AreaLit RTs. Keyword-gated (heavy 16-quad loop, stripped when _AreaLit_Int==0 via the editor). With no LightMesh assigned, ShadeAreaLitLatex's first .Load is 0 and it contributes nothing.  <br/><sub>↳ before `#if defined(AREALIT_ENABLE)`</sub>
- **L1229** — Matcap  <br/><sub>↳ before `half3 matcapEval = matcap * saturate(gi.diffuse + light.color * smoothstep(0.0, 0.15, NcL)) * baseSpecOcc;`</sub>
- **L1233** — Emission + AL neon overlay  <br/><sub>↳ before `finalColor += s.Emission * _Emis_Exp;`</sub>

### `void LightingStandardLatex_GI(SurfaceOutputStandardLatex s, UnityGIInput data, inout UnityGI gi)`
<sub>L1241–L1255</sub>

- **L1241** — Same mirror-camera fix as LightingStandardLatex - UnityGIInput.worldViewDir was filled from _WorldSpaceCameraPos and drives the indirect specular reflection direction below.  <br/><sub>↳ before `data.worldViewDir = vw_WorldViewDir(s.WorldPos);`</sub>
- **L1246** — Light Volume diffuse (pre-baked into s.LVDiffuse in surf) - Additive mode ADDs to Unity's probe diffuse (volumes layer on top); Full/deringed mode REPLACES it (LV is the authoritative SH source).  <br/><sub>↳ before `if (s.LVActive > 0.5)`</sub>
- **L1255** — Roughness-blurred IBL (no Fresnel - applied per-layer in BRDF). Occlusion=1 here; specOcc is per-layer.  <br/><sub>↳ before `Unity_GlossyEnvironmentData g =`</sub>

### `inline half4 LightingStandardLatex(SurfaceOutputStandardLatex s, half3 viewDir, UnityGI gi)`
<sub>L1264</sub>

- **L1264** — Unity's surface-shader plumbing computes incoming viewDir from _WorldSpaceCameraPos in the generated vertex stage (wrong in VRChat mirrors); reproject from the actual rendering camera so clearcoat reflections and BRDF NdotV are correct.  <br/><sub>↳ before `viewDir = vw_WorldViewDir(s.WorldPos);`</sub>

### `#endif`
<sub>L1279–L1292</sub>

- **L1279** — Alpha workflow branches by mode keyword - Opaque+Cutout force outputAlpha=1 (SubShader Blend is One/Zero so value would be discarded, but explicit avoids surprises); Fade uses straight alpha (SrcAlpha/OneMinusSrcAlpha); Transparent uses Unity's PreMultiplyAlpha so specular survives at low opacity.  <br/><sub>↳ before `half outputAlpha = 1.0;`</sub>
- **L1292** — Safe vector indexing macro to bypass HLSL arrayification bugs  <br/><sub>↳ before `#define GET_AL_BAND(vec, bandIdx) ( \`</sub>

### `#define GET_AL_BAND(vec, bandIdx) ( \`
<sub>L1299</sub>

- **L1299** — SURFACE FUNCTION  <br/><sub>↳ before `void surf (Input IN, inout SurfaceOutputStandardLatex o)`</sub>

### `void surf (Input IN, inout SurfaceOutputStandardLatex o)`
<sub>L1309–L1366</sub>

- **L1309** — Animation time stays on real time; chronotensity is opt-in per FX via _UseChronoFX.  <br/><sub>↳ before `float animTime = _Time.y;`</sub>
- **L1314** — AudioLink bands (zeroed by default; FetchAudioLinkBands only runs when the master toggle is on).  <br/><sub>↳ before `float4 amps = float4(0,0,0,0);`</sub>
- **L1326** — DFT note pull-out (across all octaves), used to bias emission  <br/><sub>↳ before `float dftAmp = 0.0;`</sub>
- **L1347** — Standard time-driven UV scroll (chronotensity drive removed - was unpredictable).  <br/><sub>↳ before `baseUV += float2(_SpeedX, _SpeedY) * _Time.y;`</sub>
- **L1350** — Bio pulse  <br/><sub>↳ before `half heartbeat  = amps.x * 0.65 + amp_emis * 0.35;`</sub>
- **L1358** — Audio Color Blend cycles AL tint through rainbow (time + bio + worldPos.y). Applied before VRSL hijack.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && _AL_Col_Blend > 0.001)`</sub>
- **L1366** — VRSL color hijack (DMX colour wash override for AL color)  <br/><sub>↳ before `#if defined(VRSL_ENABLE)`</sub>

### `#endif`
<sub>L1379–L1761</sub>

- **L1379** — (Geometry-level primID fracture clip removed - broke under tessellation. Per-pixel noise clip below handles shards.)  <br/><sub>↳ before `float2 cUV = baseUV;`</sub>
- **L1381** — UV AUDIO DISTORTION CHAIN: vortex → pump → fracture → rotation → glitch tear → parallax (compounding).  <br/><sub>↳ before `float2 cUV = baseUV;`</sub>
- **L1384** — Per-fragment fracture pop mask - read by parallax stage; declared outside AL guard.  <br/><sub>↳ before `float fracturePop = 0;`</sub>
- **L1387** — UV distortion effects all funnel through band amplitudes which are zero when _UseAudioLink is off.  <br/><sub>↳ before `if (_UseALVortex > 0.5)`</sub>
- **L1395** — Radial falloff - centre twists hardest. Chrono FX adds an oscillating breath.  <br/><sub>↳ before `float chronoMod = (_UseChronoFX > 0.5) ? sin(GET_AL_BAND(chronos, _AL_Vortex_Band) * UNITY_PI) : 1.0;`</sub>
- **L1404** — Radial scale around pump centre: pump<1 zooms in, pump>1 zooms out.  <br/><sub>↳ before `float bandAmp = GET_AL_BAND(amps, _AL_Pump_Band);`</sub>
- **L1416** — Two-axis slice hash advancing with time so shards re-roll instead of locking.  <br/><sub>↳ before `float2 fUV = TransformUV(cUV, _AL_Fracture_UV);`</sub>
- **L1428** — Shard mask drives a tiny parallax pop (read at o.ParallaxDepth below).  <br/><sub>↳ before `fracturePop = fractureMask;`</sub>
- **L1433** — UV rotation applied after audio distortions so it composes with vortex/pump. Vortex+ChronoFX adds an audio-driven spin (~8.6 deg/unit).  <br/><sub>↳ before `float uvRotDeg = _UV_Rot;`</sub>
- **L1440** — Glitch UV tear - X skews with live waveform, Y micro-wobble reads as VHS tracking.  <br/><sub>↳ before `float2 glitchOffset = 0;`</sub>
- **L1460** — Parallax over audio-distorted UV (fracturePop pushes shards a hair off the surface) - IN.viewDir would derive from _WorldSpaceCameraPos and break parallax in VRChat mirrors; vw_WorldViewDir reads the actual rendering camera via UNITY_MATRIX_I_V instead.  <br/><sub>↳ before `float3 viewDirWorld   = vw_WorldViewDir(IN.worldPos);`</sub>
- **L1466** — Base textures  <br/><sub>↳ before `fixed4 c      = UNITY_SAMPLE_TEX2D(_MainTex, finalUV) * _Color;`</sub>
- **L1470** — Fracture dissolve clip - the body opens up as the fracture progresses (manual _Vtx_Fracture_Amount plus AudioLink jitter). SPS dissolves only (no shard pass); non-SPS additionally flies the region off as shards.  <br/><sub>↳ before `float fracProg = saturate(_Vtx_Fracture_Amount + (_UseAudioLink > 0.5 ? GET_AL_BAND(amps, _Vtx_Fracture_Band) * _Vtx_Fracture_Str * 0.2 : 0…`</sub>
- **L1478** — Alpha workflow - Cutout: hard clip on _CutOff (also clips addshadow so shadows match silhouette); Fade/Transparent: discard fully invisible pixels so the shadow caster doesn't punch opaque shadow holes; Opaque: no clip, alpha ignored.  <br/><sub>↳ before `#if defined(_ALPHATEST_ON)`</sub>
- **L1486** — ShadowCaster/depth passes only need alpha for the cutout clips handled above. Everything  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1487** — below is per-pixel surface + world-light prep that is dead code in those passes - but with  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1488** — SPS injected, `addshadow` compiles this entire surf (plus the SPS vertex) into the generated  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1489** — ShadowCaster, ballooning that snippet to tens of MB and OOM-crashing UnityShaderCompiler.exe  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1490** — on import (the Editor then hangs waiting on the dead worker). Bail out so depth stays cheap.  <br/><sub>↳ before `#if defined(UNITY_PASS_SHADOWCASTER)`</sub>
- **L1495** — Poiyomi-style multi-region color mask - RGB zones each multiply a tint into albedo and contribute emission boost later; channels are independent so overlapping zones stack.  <br/><sub>↳ before `float regionEmis = 0;`</sub>
- **L1500** — Channels are independent masks (not blended) so authors can paint hard-edged feature zones.  <br/><sub>↳ before `float3 regionTint = lerp(float3(1,1,1), _Region_R_Tint.rgb, regionSample.r)`</sub>
- **L1512** — Metallic / smoothness with channel-selectable Poiyomi-pack support + AL modulation.  <br/><sub>↳ before `float pbrMet    = ChannelPick(packed, _PBR_Met_Ch);`</sub>
- **L1521** — AO (channel selectable); "None" (channel 4) yields a constant 1.0 so Poiyomi/Mochie packs without an AO channel don't read a wrong channel.  <br/><sub>↳ before `float pbrAO = (_PBR_AO_Ch > 3.5) ? 1.0 : ChannelPick(packed, _PBR_AO_Ch);`</sub>
- **L1527** — Height (channel selectable; parallax raymarch and BRDF shadow trace use the same channel).  <br/><sub>↳ before `float pbrHeight = ChannelPick(packed, _PBR_Height_Ch);`</sub>
- **L1531** — Poiyomi/Mochie packed-map masks - reads reflection + specular masks from the packed PBR map so a Mochie "Metallic Maps" texture (R:Met G:Smooth B:ReflMask A:SpecMask) drives our masking. Default off keeps both masks neutral (1.0); applied in the BRDF combine - reflection mask dims environment/probe specular, specular mask dims direct highlights.  <br/><sub>↳ before `o.ReflectionMask = 1.0;`</sub>
- **L1545** — Normals  <br/><sub>↳ before `float3 normalTS = UnpackNormal(tex2D(_BumpMap, finalUV));`</sub>
- **L1557** — Clearcoat + thin film with AL modulation  <br/><sub>↳ before `o.ClearcoatStrength   = saturate(_CC_Strength + amp_shat * _AL_CC_Shatter);`</sub>
- **L1564** — Thickness (SSS) from bio pulse  <br/><sub>↳ before `o.Thickness = bio;`</sub>
- **L1567** — Anisotropic specular controls (latex stretch direction).  <br/><sub>↳ before `o.Anisotropy    = _Aniso;`</sub>
- **L1571** — Transmission (thin-part back-light), modulated by bio so SSS bleeds through audio-reactive regions.  <br/><sub>↳ before `o.Transmission = saturate(_Trans_Str + bio * 0.1);`</sub>
- **L1574** — Polish layer master gate + B&W mask - sampled once here, applied to the whole polish layer in the BRDF. Default white mask + toggle on = 1 (full polish, historical look).  <br/><sub>↳ before `o.PolishMask = _UsePolish * ChannelPick(UNITY_SAMPLE_TEX2D_SAMPLER(_PolishMask, _MainTex, finalUV), _PolishMaskCh);`</sub>
- **L1577** — WET - full "soaked / just out of the shower" look plus run-off rivulets. The soak (darken + near-mirror gloss + water-film sheen + flattened micro-normal) covers the whole masked area; animated UV-vertical rivulets add concentrated run-off streaks on top. UV-space keeps it stable on skinned avatars. Own toggle so it costs nothing when off.  <br/><sub>↳ before `if (_UseDrip > 0.5)`</sub>
- **L1583** — Run-off rivulets: animated vertical streaks where extra water is pouring down. Computed first; the normal tilt is applied last so streaks still pop over the flattened film.  <br/><sub>↳ before `float rivulet = 0;`</sub>
- **L1591** — Coverage gate - only a fraction of columns carry a rivulet.  <br/><sub>↳ before `float hasCol  = step(1.0 - saturate(_Drip_Coverage), colHash);`</sub>
- **L1593** — Gaussian rivulet across the column (centre is wettest); higher _Drip_Width = thinner streak.  <br/><sub>↳ before `float xInCol  = frac(colF) - 0.5;`</sub>
- **L1596** — Downward flow - per-column speed/phase variance so streaks don't march in lockstep.  <br/><sub>↳ before `float flow    = finalUV.y - _Time.y * _Drip_Speed * (0.6 + colHash) - colHash * 7.0;`</sub>
- **L1598** — Travelling beads so it reads as running water; 0.35 floor keeps a continuous trickle between beads.  <br/><sub>↳ before `float bead    = sin(flow * 18.0) * 0.5 + 0.5;`</sub>
- **L1602** — Gaussian derivative across the streak - rounds it so it catches a glint.  <br/><sub>↳ before `rivuletSlope  = clamp(-2.0 * xInCol * _Drip_Width * ridge * hasCol, -4.0, 4.0);`</sub>
- **L1606** — Total wetness: global soak + rivulet streaks, masked and clamped.  <br/><sub>↳ before `float wetness = saturate(_Wet_Amount + rivulet) * wetMaskTex;`</sub>
- **L1610** — 1. Water absorption darkens the surface (deeper in the most-soaked areas).  <br/><sub>↳ before `o.Albedo *= lerp(1.0, 1.0 - _Wet_Darken * 0.65, wetness);`</sub>
- **L1612** — 2. A water film is near-mirror smooth - drive smoothness toward the wet target.  <br/><sub>↳ before `o.Smoothness    = lerp(o.Smoothness, _Wet_Smoothness, wetness);`</sub>
- **L1615** — 3. The film fills micro-detail, flattening the shading normal toward the surface.  <br/><sub>↳ before `o.Normal = normalize(lerp(o.Normal, float3(0,0,1), wetness * _Wet_Flatten));`</sub>
- **L1617** — 4. The thin water sheet reads as an extra dielectric clearcoat (F0~0.04 = water), giving the bright wet Fresnel sheen. Gated by the Polish layer in the BRDF.  <br/><sub>↳ before `o.ClearcoatStrength = saturate(o.ClearcoatStrength + wetness * _Wet_Sheen);`</sub>
- **L1619** — Run-off streak tilt applied last so it survives the film flattening.  <br/><sub>↳ before `o.Normal = normalize(o.Normal + float3(rivuletSlope * _Drip_Normal * 0.15, 0, 0));`</sub>
- **L1625** — Matcap - world-anchored sphere mapping. The basis vectors come from view-direction + world-up instead of UNITY_MATRIX_V, because UNITY_MATRIX_V carries the camera's full rotation including roll - head tilt in VR (or any camera roll) would spin the matcap pattern around the view axis, making highlights swim instead of staying world-locked the way a real metal/latex surface would behave. vw_WorldViewDir reads from the actual rendering camera (UNITY_MATRIX_I_V), so this stays mirror-correct.  <br/><sub>↳ before `float3 nWorld   = normalize(WorldNormalVector(IN, float3(0,0,1)));`</sub>
- **L1628** — Swap reference up when looking near-vertical so cross(refUp, viewDirW) doesn't collapse - using world Z as the fallback keeps the basis well-defined.  <br/><sub>↳ before `float3 refUp    = (abs(dot(viewDirW, float3(0,1,0))) > 0.999) ? float3(0,0,1) : float3(0,1,0);`</sub>
- **L1634** — Layer 1 - channel-selectable mask + per-layer tint.  <br/><sub>↳ before `float rad = _MatCap_Rot * (UNITY_PI / 180.0);`</sub>
- **Tiling + 3-axis scroll** — `_MatCap_Tiling.xy` repeats the matcap; `_MatCap_Scroll` drives smooth motion: `.x`/`.y` pan the UV (`+ _MatCap_Scroll.xy * _Time.y`) and `.z` is a continuous spin in degrees/sec folded into the rotation as `matcapSpin = _MatCap_Rot + fmod(_MatCap_Scroll.z * _Time.y, 360)`. A matcap is a 2D sphere projection with no real depth axis, so rotation is the only "third axis" that behaves like a scroll (continuous and one-directional); a zoom can't, because it would either run away or have to bounce. The rotation `mul` is split from the `+0.5` re-centre so tiling scales the rotated UV around the matcap centre (`* tiling + 0.5`) rather than the texture origin, otherwise tile != 1 pushes the highlight into the corner. The `fmod(..., 360)` keeps the spin angle bounded so sin/cos stay precise (no jitter) over long sessions. Defaults (Tiling `(1,1)`, Scroll `(0,0,0)`) reduce to the original static `mul(...) + 0.5`. Visible repeat at tile > 1 needs the matcap texture's Wrap Mode = Repeat.  <br/><sub>↳ before `matcapUV = matcapUV * _MatCap_Tiling.xy + 0.5 + _MatCap_Scroll.xy * _Time.y;`</sub>
- **L1642** — Matcap audio boost gated by the user emission amount - without it the surface still pulses when AL is on with all sliders at zero.  <br/><sub>↳ before `half3 matcap1 = matcapTex.rgb * _MatCap_Tint.rgb * matcap1Mask * _MatCap_Int * (1.0 + amp_emis * _AL_Emis_Mod * 0.5);`</sub>
- **L1646** — Layer 2 - independent matcap/mask channel/rotation/tint/blend mode; "Replace" blend uses the mask as a lerp so layer 2 takes over inside its mask zone.  <br/><sub>↳ before `if (_UseMatCap2 > 0.5)`</sub>
- **L1660** *(inline)* — Replace inside mask
- **L1662** *(inline)* — Multiply inside mask
- **L1664** *(inline)* — Add (default)
- **L1667** — EMISSION - autocorrelator vertically warps the emission UV so circuitry breathes without recolouring.  <br/><sub>↳ before `float2 emisUV = finalUV;`</sub>
- **L1671** — autoCorr is now zero-centered via the 0.007 scale; removed the -0.5 offset.  <br/><sub>↳ before `emisUV.y += autoCorr * _AL_AutoCorr_Mod * 0.2;`</sub>
- **L1677** — Manual surface emission: circuitry lines ONLY  <br/><sub>↳ before `float3 manualEmis = emisTex.rgb * _EmissionColor.rgb;`</sub>
- **L1684** — 1. BASE GLOW: Locked to circuitry lines  <br/><sub>↳ before `float3 emisBase = (manualEmis + alLayer) * emisMask;`</sub>
- **L1687** — Emission boost via bio pulse (heartbeat + tension + neuroSpike + chrono breath).  <br/><sub>↳ before `if (_UseAudioLink > 0.5)`</sub>
- **L1694** — Poiyomi-style secondary emission layer - independent texture/color/mask, optional AL band reactor.  <br/><sub>↳ before `if (_UseEmission2 > 0.5)`</sub>
- **L1701** — Pull a band amp specifically for this layer so the artist can route bass/treble independently.  <br/><sub>↳ before `float amp_emis2 = GET_AL_BAND(amps, _AL_Band_Emis2);`</sub>
- **L1709** — Region mask emission boost - each painted zone multiplies local emission so the user can brighten specific feature areas (panels, claws, paw-print decals) without a second map.  <br/><sub>↳ before `if (_UseRegionMask > 0.5 && regionEmis > 0.001)`</sub>
- **L1715** — Dynamic effects bleed onto the emisMask.  <br/><sub>↳ before `float effectMask = emisMask;`</sub>
- **L1720** — CRT-bar scanline: smoothstep wave multiplied through emission. chr_scan is 0 unless ChronoFX is enabled.  <br/><sub>↳ before `float scanTime = fmod((_Time.y * _AL_Scan_Speed * 1.8) + (chr_scan * _AL_Scan_React * 0.8), 628.318);`</sub>
- **L1729** — Faint highlight on waveform peaks so the UV warp reads on dim backgrounds (decoration, not the main effect).  <br/><sub>↳ before `float waveformRipple = raw_waveform * _AL_Waveform_Mod;`</sub>
- **L1736** — Autocorrelator ripple → EMISSION block; glitch tear → UV AUDIO DISTORTION CHAIN above.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)`</sub>
- **L1738** — CYBER HUD intentionally omitted on the SPS variant - geometry-shader HUD passes are incompatible with VRCFury's SPS vertex patcher, so the floating HUD ships on the non-SPS shader only.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)`</sub>
- **L1740** — Amplitude-driven flicker sparkle on top of the steady AL emission (decoration only) - gated by _AL_Emis_Mod so users can fully disable AL emission response with the slider.  <br/><sub>↳ before `if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)`</sub>
- **L1750** — Clearcoat normal - flatten lerps the normal-mapped "skin" toward the smooth geometric normal.  <br/><sub>↳ before `float3 nClearcoat = normalize(nWorld);`</sub>
- **L1751** — _CC_Flat = 1 -> fully flat glassy coat (geometric normal); _CC_Flat = 0 -> coat rides the normal map.  <br/><sub>↳ before `float3 nClearcoat = normalize(nWorld);`</sub>
- **L1752** — Early-out on the default (1.0) end skips the unneeded normal-map mul; the lerp runs all the way to 0.  <br/><sub>↳ before `float3 nClearcoat = normalize(nWorld);`</sub>
- **L1756** *(inline)* — tangent → world: row vec * matrix
- **L1761** — LIGHT VOLUMES (stashes diffuse + base/clearcoat specular) - _LV_AdditiveOnly samples only additive volumes (preserves Unity probe baseline); _LV_Bias pushes along world normal as worldPosOffset to fix light bleed at sharp edges (matches official LV PBR); _LV_PosOffset is a manual world-space offset for thin/sleeve geometry; _LV_ProbeDering is an opt-in Bakery L1 fallback that swaps Unity SH9 for dering'd L0+L1 (without it, non-LV worlds keep Unity's full probe path preserving L2 detail and avoiding black-out from negative L1 reconstruction).  <br/><sub>↳ before `o.LVDiffuse = 0;`</sub>

### `#if defined(LIGHTVOLUMES_ENABLE)`
<sub>L1774–L1793</sub>

- **L1774** — World-space shaded normal (with normalmap) for diffuse fidelity.  <br/><sub>↳ before `float3 nWorldShaded = normalize(mul(o.Normal, o.WorldToTangent));`</sub>
- **L1777** — Normal-bias offset + user-provided manual offset.  <br/><sub>↳ before `float3 lvOffset = nWorldShaded * _LV_Bias + _LV_PosOffset.xyz;`</sub>
- **L1786** — Clamp evaluated diffuse to 0 - probe SH (especially Bakery's dering path) can produce negative values when L1 magnitude > L0, blacking out the avatar on default worlds.  <br/><sub>↳ before `o.LVDiffuse = max(LightVolumeEvaluate(nWorldShaded, lv_L0, lv_L1r, lv_L1g, lv_L1b), 0);`</sub>
- **L1790** — _WorldSpaceCameraPos is the player's head, not the mirror camera - route through the helper.  <br/><sub>↳ before `float3 worldViewDir = vw_WorldViewDir(IN.worldPos);`</sub>
- **L1793** — LV specular layers only fire when an actual LV system is in the scene - they need real L1 directionality, not dering'd probes which would duplicate Unity's reflection probes.  <br/><sub>↳ before `if (lvAvailable && _LV_Spec_Mix > 0.001)`</sub>

### `#endif`
<sub>L1814</sub>

- **L1814** — Store UV  <br/><sub>↳ before `o.UV = finalUV;`</sub>

---

## `Editor/VixenWearEditor.cs`

*79 comment(s).*


### `(file scope)`
<sub>L1</sub>

- **L1** — VIXEN WEAR - NATIVE SHADERGUI INSPECTOR (LATEX ULTRA - SYNCED). Place in Editor folder. Matches shader properties and updates shader keywords.  <br/><sub>↳ before `using System;`</sub>

### `private string Sanitize(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().Replace("_", " ");`
<sub>L39</sub>

- **L39** — Foldout state per material-property, persisted across domain reloads.  <br/><sub>↳ before `private static readonly Dictionary<string, bool> s_expanded = new Dictionary<string, bool>();`</sub>

### `public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)`
<sub>L59–L68</sub>

- **L59** — Non-vector, empty, or single-component: a single normal row (no foldout).  <br/><sub>↳ before `if (prop.type != MaterialProperty.PropType.Vector \|\| visibleCount <= 1)`</sub>
- **L62** — Multi-component: collapsed = header only; expanded = header + one row per component.  <br/><sub>↳ before `if (!IsExpanded(prop.name))`</sub>
- **L68** — Short tags for the collapsed-row value summary.  <br/><sub>↳ before `private static readonly Dictionary<string, string> ShortLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)`</sub>

### `public override void OnGUI(Rect pos, MaterialProperty prop, GUIContent label, MaterialEditor editor)`
<sub>L93–L127</sub>

- **L93** — Single visible component: a normal labelled field, no foldout needed.  <br/><sub>↳ before `if (visibleCount == 1)`</sub>
- **L101** — Collapsible header: foldout + label. Collapsed shows a dimmed value summary on the  <br/><sub>↳ before `Rect foldRect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, line);`</sub>
- **L102** — right; expanded shows one full-width labelled float field per component below.  <br/><sub>↳ before `Rect foldRect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, line);`</sub>
- **L125** — One component as a full-width labelled float field, with per-component, per-material write  <br/><sub>↳ before `private void DrawComponentRow(Rect rect, MaterialProperty prop, UnityEngine.Object[] targets, int i, bool isMixed, ref Vector4 v)`</sub>
- **L126** — (preserves the other components on every selected material - the prop.vectorValue path  <br/><sub>↳ before `private void DrawComponentRow(Rect rect, MaterialProperty prop, UnityEngine.Object[] targets, int i, bool isMixed, ref Vector4 v)`</sub>
- **L127** — would propagate the first material's whole vector to all selected, the original bug).  <br/><sub>↳ before `private void DrawComponentRow(Rect rect, MaterialProperty prop, UnityEngine.Object[] targets, int i, bool isMixed, ref Vector4 v)`</sub>

### `private void DrawComponentRow(Rect rect, MaterialProperty prop, UnityEngine.Object[] targets, int i, bool isMixed, ref Vector4 v)`
<sub>L155</sub>

- **L155** — Dimmed, right-aligned "X 0   Y 0   Scl 1   Rot 0" preview shown on the collapsed row.  <br/><sub>↳ before `private void DrawSummary(Rect rect, Vector4 v, bool[] mixed)`</sub>

### `private void DrawSummary(Rect rect, Vector4 v, bool[] mixed)`
<sub>L174–L175</sub>

- **L174** — Per-component mixed-value detection across multi-selected materials (each X/Y/Z/W shows  <br/><sub>↳ before `private bool[] ComputeMixed(MaterialProperty prop, MaterialEditor editor, out UnityEngine.Object[] targets, out Vector4 v)`</sub>
- **L175** — "-" independently like a Unity Vector4Field, instead of prop.hasMixedValue's all-or-nothing).  <br/><sub>↳ before `private bool[] ComputeMixed(MaterialProperty prop, MaterialEditor editor, out UnityEngine.Object[] targets, out Vector4 v)`</sub>

### `public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e)`
<sub>L220</sub>

- **L220** — Change-gate the write - unconditional p.floatValue = ... overwrites every selected material with the first material's value on every repaint, breaking multi-edit.  <br/><sub>↳ before `EditorGUI.BeginChangeCheck();`</sub>

### `private readonly string[] tabDesc =`
<sub>L265</sub>

- **L265** — Tab → property names (must match shader Properties)  <br/><sub>↳ before `private readonly string[][] tabProps = new string[][]`</sub>

### `private readonly string[][] tabProps = new string[][]`
<sub>L268–L534</sub>

- **L268** — BASE  <br/><sub>↳ before `new[]`</sub>
- **L280** — SURFACE  <br/><sub>↳ before `new[]`</sub>
- **L310** — POLISH  <br/><sub>↳ before `new[]`</sub>
- **L383** — INTEGRATION  <br/><sub>↳ before `new[]`</sub>
- **L435** — AUDIOLINK / KINETIC  <br/><sub>↳ before `new[]`</sub>
- **L534** — STAGE / VRSL  <br/><sub>↳ before `new[]`</sub>

### `private void DrawProp(MaterialEditor ed, MaterialProperty prop, string label)`
<sub>L599</sub>

- **L599** — Sets a float/range/enum property on all targets if it exists (used by one-click setup helpers). Null-safe so it no-ops on shader variants missing the property.  <br/><sub>↳ before `private void SetF(MaterialProperty[] p, string name, float value)`</sub>

### `private void PerformPaste(MaterialEditor ed, MaterialProperty[] p, int tabIndex, bool includeTextures)`
<sub>L654</sub>

- **L654** — BASE tab carries _Mode - re-run full blend/queue/tag setup so the destination material's blend state matches the pasted mode rather than the previous mode's leftover state.  <br/><sub>↳ before `if (tabIndex == 0 && _clipboard.Floats.ContainsKey("_Mode"))`</sub>

### `private void PerformReset(MaterialEditor ed, MaterialProperty[] p, int tabIndex)`
<sub>L670–L727</sub>

- **L670** — A fresh material built from the same shader carries all shader-declared defaults (floats, colors, vectors, and Unity's built-in white/black/bump/gray textures).  <br/><sub>↳ before `Material defaults = new Material(sourceMat.shader) { hideFlags = HideFlags.HideAndDontSave };`</sub>
- **L711** — BASE tab carries _Mode - re-apply full blend/queue/tag state so the reset value of _Mode actually takes visual effect (otherwise blend state would lag behind the property).  <br/><sub>↳ before `if (tabIndex == 0)`</sub>
- **L727** — Helper: convert targets to Material[] safely  <br/><sub>↳ before `private Material[] GetMaterialsFromTargets(UnityEngine.Object[] targets)`</sub>

### `private Material[] GetMaterialsFromTargets(UnityEngine.Object[] targets)`
<sub>L739</sub>

- **L739** — Update shader keywords for all selected materials  <br/><sub>↳ before `private void UpdateKeywordsForTargets(UnityEngine.Object[] targets)`</sub>

### `private void UpdateKeywordsForTargets(UnityEngine.Object[] targets)`
<sub>L746</sub>

- **L746** — Sync shader keywords to material toggle properties. Public/static so the build preprocessor can call it.  <br/><sub>↳ before `public static void SyncKeywords(Material mat)`</sub>

### `public static void SyncKeywords(Material mat)`
<sub>L755–L781</sub>

- **L755** — AreaLit is a heavy 16-quad LTC loop - compile it in whenever Intensity is up. The light data can come from the scene-global broadcaster (_Udon_AreaLit_*) OR the per-material slots, so we no longer require a manual LightMesh here; the runtime liveness probe (_Udon_AreaLit_Enable / first .Load) handles the empty case.  <br/><sub>↳ before `bool areaLit = mat.HasProperty("_AreaLit_Int")  && mat.GetFloat("_AreaLit_Int")    > 0.001f;`</sub>
- **L763** — AudioLink is runtime-gated by _UseAudioLink (no build-time keyword) so VRCFury material-toggle animations can flip it without a compiled variant - strip the stale keyword.  <br/><sub>↳ before `mat.DisableKeyword("AL_ENABLE");`</sub>
- **L765** — Force-disable CYBER_ENABLE - shader never #if-gates on it, so the 2x variant set is dead.  <br/><sub>↳ before `mat.DisableKeyword("CYBER_ENABLE");`</sub>
- **L768** — Clear EmissiveIsBlack so Unity's build pipeline doesn't strip _EmissionColor/_EmissionMap/_EmissionColor2 from materials whose flag was never updated (default on freshly cloned mats, e.g. VRCFury swap targets).  <br/><sub>↳ before `mat.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;`</sub>
- **L771** — Alpha workflow keywords mirror _Mode - done here (not just in SetupMaterialWithBlendMode) so upgraded materials pick up the right keyword on the next build/play-mode transition without an inspector visit.  <br/><sub>↳ before `if (mat.HasProperty("_Mode"))`</sub>
- **L781** — Full alpha-workflow setup (blend state, ZWrite, render queue, RenderType + VRCFallback tags, keywords) - called on _Mode change or shader assignment; SyncKeywords handles the lighter keyword-only case.  <br/><sub>↳ before `public static void SetupMaterialWithBlendMode(Material material, int blendMode)`</sub>

### `public static void SetupMaterialWithBlendMode(Material material, int blendMode)`
<sub>L788</sub>

- **L788** *(inline)* — Opaque

### `case 0: // Opaque`
<sub>L799</sub>

- **L799** *(inline)* — Cutout

### `case 1: // Cutout`
<sub>L810</sub>

- **L810** *(inline)* — Fade - straight alpha, everything (including specular) fades out together.

### `case 2: // Fade - straight alpha, everything (including specular) fades out together.`
<sub>L821</sub>

- **L821** *(inline)* — Transparent - premultiplied alpha; specular highlights survive at low opacity (glass/latex).

### `case 3: // Transparent - premultiplied alpha; specular highlights survive at low opacity (glass/latex).`
<sub>L835</sub>

- **L835** — Initialize blend/queue/tag state when the shader is first applied so newly-created materials don't render with stale queue/blend from whatever shader was previously assigned.  <br/><sub>↳ before `public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)`</sub>

### `public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)`
<sub>L841</sub>

- **L841** — Clear EmissiveIsBlack on first shader assignment so Unity's build pipeline can't strip emission properties from this material later.  <br/><sub>↳ before `if (material != null)`</sub>

### `private void UpdateKeywords(Material mat) => SyncKeywords(mat);`
<sub>L848</sub>

- **L848** — Small helper to set keywords safely  <br/><sub>↳ before `private static void SetKeyword(Material mat, string keyword, bool enabled)`</sub>

### `public override void OnGUI(MaterialEditor ed, MaterialProperty[] p)`
<sub>L861–L987</sub>

- **L861** — Banner  <br/><sub>↳ before `Rect banner = GUILayoutUtility.GetRect(100, 36);`</sub>
- **L875** — Tabs  <br/><sub>↳ before `Rect tabGroupRect = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));`</sub>
- **L888** — Context menu for copy/paste tab  <br/><sub>↳ before `if (Event.current.type == EventType.ContextClick && btnRect.Contains(Event.current.mousePosition))`</sub>
- **L987** — BASE  <br/><sub>↳ before `if (ActiveTab == 0)`</sub>

### `if (ActiveTab == 0)`
<sub>L994–L1026</sub>

- **L994** — Render the dropdown ourselves so we can fire SetupMaterialWithBlendMode on change - DrawProp's inner change-check still fires SyncKeywords, and the outer check here applies the full blend/queue/tag state.  <br/><sub>↳ before `EditorGUI.BeginChangeCheck();`</sub>
- **L1007** — Cutout is the only mode that uses _CutOff - fade/transparent ignore it.  <br/><sub>↳ before `DrawProp(ed, FindProperty("_CutOff", p, false), "Alpha Cutoff");`</sub>
- **L1026** — SURFACE  <br/><sub>↳ before `else if (ActiveTab == 1)`</sub>

### `else if (ActiveTab == 1)`
<sub>L1041–L1096</sub>

- **L1041** — Poiyomi/Mochie reflection + specular masks, sampled from the packed PBR map above.  <br/><sub>↳ before `var _UsePM = FindProperty("_UsePackedMasks", p, false);`</sub>
- **L1096** — POLISH  <br/><sub>↳ before `else if (ActiveTab == 2)`</sub>

### `else if (ActiveTab == 2)`
<sub>L1145–L1244</sub>

- **L1145** — Wet - full soaked look plus run-off rivulets.  <br/><sub>↳ before `EditorGUILayout.LabelField("Wet & Run-Off", EditorStyles.boldLabel);`</sub>
- **L1192** — Goo - melting/runny vertex sag.  <br/><sub>↳ before `EditorGUILayout.LabelField("Goo (Melting Sag)", EditorStyles.boldLabel);`</sub>
- **L1244** — INTEGRATION  <br/><sub>↳ before `else if (ActiveTab == 3)`</sub>

### `else if (ActiveTab == 3)`
<sub>L1346</sub>

- **L1346** — AUDIOLINK / KINETIC  <br/><sub>↳ before `else if (ActiveTab == 4)`</sub>

### `else if (ActiveTab == 4)`
<sub>L1543</sub>

- **L1543** — STAGE / VRSL  <br/><sub>↳ before `else if (ActiveTab == 5)`</sub>

### `else if (ActiveTab == 5)`
<sub>L1572–L1613</sub>

- **L1572** — Per-tab "Reset to Defaults" - visible companion to the right-click menu entry.  <br/><sub>↳ before `using (new EditorGUILayout.HorizontalScope())`</sub>
- **L1603** — Render queue / instancing / double sided GI  <br/><sub>↳ before `ed.RenderQueueField();`</sub>
- **L1608** — Ensure keywords are synced for all selected materials at end of GUI pass  <br/><sub>↳ before `UpdateKeywordsForTargets(ed.targets);`</sub>
- **L1613** — BUILD-TIME KEYWORD CLEANUP - syncs material keywords to property toggles before variant stripping so stale keywords don't preserve dead variants.  <br/><sub>↳ before `public class VixenWearBuildPreprocessor : IPreprocessBuildWithReport`</sub>

### `public const string SHADER_NAME_SPS = "VixenWear/Latex Ultra SPS";`
<sub>L1619</sub>

- **L1619** — Both variants share the same property layout and editor; the SPS variant drops tessellation so VRCFury's SPS patcher can wrap the vertex function without hitting a struct type mismatch in tessEdge.  <br/><sub>↳ before `public static bool IsVixenWearShader(Shader s)`</sub>

### `public static void CleanFromMenu()`
<sub>L1638</sub>

- **L1638** — Promotes the current Hierarchy GameObject selection to its underlying VixenWear material assets - works around Unity's "-" inspector when renderers reference different .mat files, by walking children (incl. disabled wardrobe toggles), gathering unique materials, and swapping Selection.objects.  <br/><sub>↳ before `[MenuItem("VixenTools/VixenWear/Edit Materials From Selection %#m")]`</sub>

### `public static void EditMaterialsFromSelection()`
<sub>L1660–L1689</sub>

- **L1660** — includeInactive=true picks up wardrobe layers that are toggled off (very common for VRC clothing).  <br/><sub>↳ before `Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);`</sub>
- **L1689** — Greys out the menu item when no GameObjects are selected so the affordance matches the actual capability.  <br/><sub>↳ before `[MenuItem("VixenTools/VixenWear/Edit Materials From Selection %#m", true)]`</sub>

### `public static void CleanAllMaterials(bool verbose, bool saveToDisk)`
<sub>L1738</sub>

- **L1738** — Persist either change - GI flag drift alone (the EmissiveIsBlack clear) still needs to hit disk so Unity's build pipeline doesn't strip _EmissionColor from VRCFury swap-target materials whose keywords were already in sync.  <br/><sub>↳ before `if (!KeywordsEqual(before, after) \|\| giBefore != giAfter)`</sub>

### `private static bool KeywordsEqual(string[] a, string[] b)`
<sub>L1765</sub>

- **L1765** — PLAY-MODE KEYWORD SYNC - force keyword state on every VixenWear material before play so a stale toggle doesn't no-op on first frame.  <br/><sub>↳ before `[InitializeOnLoad]`</sub>

### `private static void OnPlayModeChanged(PlayModeStateChange change)`
<sub>L1777–L1786</sub>

- **L1777** — Sync just before we leave edit mode so the play-mode renderer sees current state.  <br/><sub>↳ before `if (change == PlayModeStateChange.ExitingEditMode)`</sub>
- **L1780** — In-memory sync only - don't dirty assets while transitioning play mode.  <br/><sub>↳ before `VixenWearBuildPreprocessor.CleanAllMaterials(verbose: false, saveToDisk: false);`</sub>
- **L1786** — VARIANT STRIPPER - drops unused variants in 3 layers: (1) managed feature kw not used by any material, (2) Deferred/Meta/MotionVectors passes, (3) built-in lightmap/LPPV keywords leaking past the pragma.  <br/><sub>↳ before `public class VixenWearVariantStripper : IPreprocessShaders`</sub>

### `public int callbackOrder => 100;`
<sub>L1791</sub>

- **L1791** — Lazy-cached set of keywords still enabled on any VixenWear material.  <br/><sub>↳ before `private static HashSet<string> _liveKeywords;`</sub>

### `internal static int s_kept;`
<sub>L1796</sub>

- **L1796** — Managed shader_feature_local kws - drop variants where no material has them on (AL_ENABLE/CYBER_ENABLE removed: those paths are runtime-branched for VRCFury; alpha workflow kws _ALPHATEST_ON/_ALPHABLEND_ON/_ALPHAPREMULTIPLY_ON are also stripped per-mode).  <br/><sub>↳ before `private static readonly string[] s_managedKeywords =`</sub>

### `private static readonly string[] s_managedKeywords =`
<sub>L1803</sub>

- **L1803** — Built-in keywords avatar clothing never uses. Belt-and-suspenders against Unity versions emitting variants the pragma already disabled.  <br/><sub>↳ before `private static readonly string[] s_deadBuiltinKeywords =`</sub>

### `private static readonly string[] s_deadBuiltinKeywords =`
<sub>L1811–L1812</sub>

- **L1811** *(inline)* — only matters in LPPV context, which we don't support
- **L1812** *(inline)* — avatar skinned meshes don't sit in LOD groups

### `public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)`
<sub>L1823–L1849</sub>

- **L1823** — Layer 2: drop Deferred/Meta/MotionVectors passes (Unity 2022.3.x has emitted them even with `nometa` - defensive strip).  <br/><sub>↳ before `if (snippet.passType == PassType.Deferred \|\|`</sub>
- **L1833** — Layers 1 + 3: per-variant keyword checks.  <br/><sub>↳ before `for (int i = data.Count - 1; i >= 0; i--)`</sub>
- **L1839** — Managed feature keywords: drop if no material has the keyword on.  <br/><sub>↳ before `foreach (string kw in s_managedKeywords)`</sub>
- **L1849** — Built-in dead keywords: drop any variant that has one of them set.  <br/><sub>↳ before `if (!drop)`</sub>

### `private static void ClearCache()`
<sub>L1893</sub>

- **L1893** — Post-build report so users can see the strip count and verify the speedup.  <br/><sub>↳ before `public class VixenWearVariantStripReporter : IPostprocessBuildWithReport`</sub>

---

## `Editor/VixenWearHub.cs`

*12 comment(s).*


### `(file scope)`
<sub>L10–L13</sub>

- **L10** — Trimmed standalone companion to the full VixForge Hub. Renders the VixenWear  <br/><sub>↳ before `public class VixenWearHub : EditorWindow`</sub>
- **L11** — documentation (How To Use, Shader Pipeline, Changelog) inside the editor using  <br/><sub>↳ before `public class VixenWearHub : EditorWindow`</sub>
- **L12** — the same Markdown-to-UIElements parser and cyber styling, repointed at the  <br/><sub>↳ before `public class VixenWearHub : EditorWindow`</sub>
- **L13** — flat Assets/VixenWear/ install layout. No VPM package, no update notifier.  <br/><sub>↳ before `public class VixenWearHub : EditorWindow`</sub>

### `private string _version = "";`
<sub>L26</sub>

- **L26** — --- Changelog pagination state ---  <br/><sub>↳ before `private class ChangelogEntry`</sub>

### `private void OnEnable()`
<sub>L56</sub>

- **L56** — No package.json in the standalone, so derive the version from the newest changelog entry.  <br/><sub>↳ before `private void LoadVersion()`</sub>

### `private void CreateGUI()`
<sub>L100–L143</sub>

- **L100** — --- HEADER BANNER ---  <br/><sub>↳ before `var headerRect = new VisualElement { name = "hub-header" };`</sub>
- **L118** — --- TABS NAVIGATION ---  <br/><sub>↳ before `var tabContainer = new VisualElement { name = "tab-container" };`</sub>
- **L136** — --- TAB DESCRIPTION BOX ---  <br/><sub>↳ before `var descContainer = new VisualElement { name = "desc-container" };`</sub>
- **L143** — --- CONTENT AREA ---  <br/><sub>↳ before `_contentScroll = new ScrollView(ScrollViewMode.Vertical) { name = "main-scroll" };`</sub>

### `private void ParseMarkdownAndInject(string text, VisualElement container)`
<sub>L267–L275</sub>

- **L267** — Skip the markdown alignment row (\|---\|:--:\|).  <br/><sub>↳ before `bool isSeparator = true;`</sub>
- **L275** — A row is a header if the next line is an alignment row.  <br/><sub>↳ before `string next = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";`</sub>

---