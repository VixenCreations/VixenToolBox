#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;
using VRC.SDK3.Persistence;
using VRC.SDK3.Components;
using VRC.SDKBase;
using TMPro;
using System;
using System.IO;
using System.Reflection;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VixenTools.Editor.QA
{
    public class VixenEngineStressTest : EditorWindow
    {
        private const string TestTexturePath = "Assets/Vixen_VRAM_Nuke_4K.png";
        private const string TestMeshPath = "Assets/Vixen_Poly_Nuke.asset";
        private const string TestTerrainPath = "Assets/Vixen_Terrain_Nuke.asset";
        private const string SceneName = "Stress Test";
        private const string ScenePath = "Assets/Stress Test.unity";

        [MenuItem("VixenTools/QA/Generate Omni-Chaos Environment")]
        public static void GenerateChaos()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != SceneName)
            {
                if (EditorUtility.DisplayDialog("Vixen QA Environment",
                    "You are not in the 'Stress Test' scene.\n\nWould you like to save your current scene, unload it, and generate a dedicated VRChat Stress Test environment?",
                    "Save & Generate", "Abort"))
                {
                    EditorSceneManager.SaveOpenScenes();
                    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(newScene, ScenePath);
                }
                else
                {
                    return;
                }
            }

            GenerateVRChatBaseArchitecture();

            GameObject root = new GameObject("== VIXEN OMNI-CHAOS ROOT ==");

            CreateStandardPerformanceIssues(root.transform, "1. Performance & Physics Pit", 0, 0);
            CreateUIAndCanvasIssues(root.transform, "2. UI Void & Rebuild Cascades", 1, 0);
            CreateVramNightmare(root.transform, "5. VRAM Nightmare", 4, 0);
            CreatePersistenceAndNetworkIssues(root.transform, "7. Network & Persistence Void", 1, 1);
            CreateVideoPipelineIssues(root.transform, "8. Video Pipeline Collapse", 2, 1);

            CreateProTVIssues(root.transform, "3. ProTV Logic Sink", 2, 0);
            CreateTXLIssues(root.transform, "4. TXL Death-Trap", 3, 0);
            CreateIwaSyncIssues(root.transform, "6. IwaSync3 Apocalypse", 0, 1);
            CreateUmbrellaIssues(root.transform, "9. Umbrella Logic Collapse", 3, 1);
            CreateExtrasIssues(root.transform, "10. Extras Proxy Desyncs", 4, 1);

            CreateGeometryAndMaterialNightmare(root.transform, "11. Geometry & Material Hell", 0, 2);
            CreateLightingAndEnvironmentApocalypse(root.transform, "12. Lighting & Environment Nuke", 1, 2);
            CreateVizVidIssues(root.transform, "13. VizVid (VVMW) Ecosystem Collapse", 2, 2);
            CreateAudioLinkAndLightVolumeIssues(root.transform, "14. AL & Light Volumes Mayhem", 3, 2);
            CreateRinvoIssues(root.transform, "15. Rinvo Search Bounds Failure", 4, 2);

            Debug.Log("<color=#ff00aa>[Vixen QA]</color> Omni-Chaos Environment Generated. The engine will now light up like a Christmas tree.");
            Selection.activeGameObject = root;
        }

        private static void GenerateVRChatBaseArchitecture()
        {
            if (RenderSettings.sun == null && !GameObject.Find("Directional Light"))
            {
                GameObject dirLightObj = new GameObject("Directional Light");
                var dirLight = dirLightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.color = new Color(1f, 0.95f, 0.85f);
                dirLight.intensity = 1f;
                dirLightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            if (!GameObject.Find("Floor"))
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.localScale = new Vector3(10, 1, 10);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = Color.gray;
                floor.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            if (Object.FindObjectOfType<VRCSceneDescriptor>() == null)
            {
                GameObject vrcWorldObj = new GameObject("VRCWorld");
                var descriptor = vrcWorldObj.AddComponent<VRCSceneDescriptor>();

                GameObject spawnPoint = new GameObject("Spawn Point");
                spawnPoint.transform.SetParent(vrcWorldObj.transform);
                spawnPoint.transform.position = new Vector3(0, 0, -5);

                descriptor.spawns = new Transform[] { spawnPoint.transform };
                descriptor.ReferenceCamera = GameObject.FindObjectOfType<Camera>()?.gameObject;
            }
        }

        private static Transform DeployPod(Transform root, string name, int x, int z)
        {
            GameObject pod = new GameObject(name);
            pod.transform.SetParent(root);
            pod.transform.position = new Vector3(x * 20f, 0, z * 20f);
            return pod.transform;
        }

        private static void CreateStandardPerformanceIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            GameObject lightObj = new GameObject("Expensive Realtime Light");
            lightObj.transform.SetParent(parent);
            var light = lightObj.AddComponent<Light>();
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.shadows = LightShadows.Soft;

            GameObject pLightObj = new GameObject("Massive Range Point Light");
            pLightObj.transform.SetParent(parent);
            var pLight = pLightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.range = 500f;

            GameObject probeObj = new GameObject("Per-Frame Reflection Probe");
            probeObj.transform.SetParent(parent);
            var probe = probeObj.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;

            GameObject physObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            physObj.name = "Non-Convex Physics Drag";
            physObj.transform.SetParent(parent);
            physObj.transform.localPosition = new Vector3(0, 5, 0);
            Object.DestroyImmediate(physObj.GetComponent<SphereCollider>());
            var mc = physObj.AddComponent<MeshCollider>();
            mc.convex = false;

            GameObject rbObj = new GameObject("Continuous Dynamic RB");
            rbObj.transform.SetParent(parent);
            var rb = rbObj.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            GameObject audioObj = new GameObject("2D Audio Source");
            audioObj.transform.SetParent(parent);
            var source = audioObj.AddComponent<AudioSource>();
            source.spatialBlend = 0f;

            if (!File.Exists(TestMeshPath))
            {
                Mesh heavyMesh = new Mesh { name = "Vixen_Poly_Nuke" };

                Vector3[] baseVerts = new Vector3[4];
                baseVerts[0] = new Vector3(0, 0, 0);
                baseVerts[1] = new Vector3(1, 0, 0);
                baseVerts[2] = new Vector3(0, 1, 0);
                baseVerts[3] = new Vector3(0, 0, 1);

                Vector3[] verts = new Vector3[66000];
                for (int i = 0; i < verts.Length; i++)
                {
                    if (i < 4) verts[i] = baseVerts[i];
                    else verts[i] = Random.insideUnitSphere * 5f;
                }

                int[] tris = new int[] { 0, 1, 2,  0, 2, 3,  0, 3, 1,  1, 3, 2 };

                heavyMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                heavyMesh.vertices = verts;
                heavyMesh.triangles = tris;
                heavyMesh.RecalculateNormals();
                heavyMesh.RecalculateBounds();

                var dir = Path.GetDirectoryName(TestMeshPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                AssetDatabase.CreateAsset(heavyMesh, TestMeshPath);
                AssetDatabase.SaveAssets();
            }

            GameObject heavyObj = new GameObject("66k Vert Mesh (No LOD)");
            heavyObj.transform.SetParent(parent);
            var mf = heavyObj.AddComponent<MeshFilter>();
            var mr = heavyObj.AddComponent<MeshRenderer>();

            Mesh loadedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(TestMeshPath);
            mf.sharedMesh = loadedMesh;

            ModelImporter imp = AssetImporter.GetAtPath(TestMeshPath) as ModelImporter;
            if (imp != null)
            {
                imp.isReadable = true;
                imp.meshCompression = ModelImporterMeshCompression.Off;
                imp.SaveAndReimport();
            }

            GameObject cvxObj = new GameObject("Convex High Poly Collider");
            cvxObj.transform.SetParent(parent);
            var cvxMc = cvxObj.AddComponent<MeshCollider>();
            cvxMc.sharedMesh = loadedMesh;
            cvxMc.convex = true;
        }

        private static void CreateGeometryAndMaterialNightmare(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);
            Mesh loadedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(TestMeshPath);

            GameObject smrObj = new GameObject("Always-Updating SMR");
            smrObj.transform.SetParent(parent);
            var smr = smrObj.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = loadedMesh;
            smr.updateWhenOffscreen = true;

            GameObject matBloatObj = new GameObject("Material Slot Bloat");
            matBloatObj.transform.SetParent(parent);
            var mrBloat = matBloatObj.AddComponent<MeshRenderer>();
            matBloatObj.AddComponent<MeshFilter>().sharedMesh = loadedMesh;
            mrBloat.sharedMaterials = new Material[] { new Material(Shader.Find("Standard")), new Material(Shader.Find("Standard")), new Material(Shader.Find("Standard")) };

            GameObject dynamicObj = new GameObject("Unprotected Dynamic Mesh");
            dynamicObj.transform.SetParent(parent);
            dynamicObj.AddComponent<MeshFilter>().sharedMesh = loadedMesh;
            dynamicObj.AddComponent<MeshRenderer>();
            dynamicObj.isStatic = false;
        }

        private static void CreateLightingAndEnvironmentApocalypse(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            if (!File.Exists(TestTerrainPath))
            {
                TerrainData td = new TerrainData();
                td.heightmapResolution = 2049;
                AssetDatabase.CreateAsset(td, TestTerrainPath);
            }

            GameObject terrainObj = new GameObject("Nuke Terrain");
            terrainObj.transform.SetParent(parent);
            var terrain = terrainObj.AddComponent<Terrain>();
            terrain.terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(TestTerrainPath);
            terrain.drawInstanced = false;
            terrain.heightmapPixelError = 1f;

            GameObject camObj = new GameObject("Rogue Active Screen Camera");
            camObj.transform.SetParent(parent);
            var cam = camObj.AddComponent<Camera>();
            cam.targetTexture = null;
            cam.cullingMask = -1;

            Lightmapping.realtimeGI = true;
            LightingSettings lightingSettings = new LightingSettings();
            lightingSettings.lightmapMaxSize = 4096;
            lightingSettings.directionalityMode = LightmapsMode.CombinedDirectional;
            lightingSettings.lightmapResolution = 100f;
            Lightmapping.lightingSettings = lightingSettings;
        }

        private static void CreateUIAndCanvasIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            GameObject canvasObj = new GameObject("Broken World Canvas");
            canvasObj.transform.SetParent(parent);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject nestedCanvasObj = new GameObject("Nested GraphicRaycaster");
            nestedCanvasObj.transform.SetParent(canvasObj.transform);
            nestedCanvasObj.AddComponent<GraphicRaycaster>();

            GameObject legacyText = new GameObject("Legacy Font");
            legacyText.transform.SetParent(canvasObj.transform);
            var text = legacyText.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject tmpText = new GameObject("Mismatched TMP Font");
            tmpText.transform.SetParent(canvasObj.transform);
            tmpText.AddComponent<TextMeshProUGUI>();
        }

        private static void CreateProTVIssues(Transform root, string name, int x, int z)
        {
            System.Type tvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
            if (tvType == null) return;

            Transform parent = DeployPod(root, name, x, z);

            for (int i = 1; i <= 2; i++)
            {
                GameObject tvObj = new GameObject($"ProTV Fighting Instance {i}");
                tvObj.transform.SetParent(parent);
                var tv = tvObj.AddComponent(tvType);

                SetField(tv, "enableHDR", true);
                SetField(tv, "bakeGlobalVideoTexture", true);
                SetField(tv, "enableGSV", true);
                SetField(tv, "preferAlternateUrlForQuest", false);
            }

            System.Type rtgiType = GetTypeSafe("ArchiTech.ProTV.RTGIUpdater");
            if (rtgiType != null)
            {
                GameObject rtgiObj = new GameObject("ProTV RTGI (Mobile Sink)");
                rtgiObj.transform.SetParent(parent);
                rtgiObj.AddComponent<MeshRenderer>();
                var rtgi = rtgiObj.AddComponent(rtgiType);
                SetField(rtgi, "runOnMobile", true);
            }

            System.Type searchType = GetTypeSafe("ArchiTech.ProTV.PlaylistSearch");
            if (searchType != null)
            {
                GameObject searchObj = new GameObject("Aggressive Playlist Search");
                searchObj.transform.SetParent(parent);
                var search = searchObj.AddComponent(searchType);
                SetField(search, "searchAggressionLevel", (byte)20);
            }

            System.Type queueType = GetTypeSafe("ArchiTech.ProTV.Queue");
            if (queueType != null)
            {
                GameObject qObj = new GameObject("Queue Spam Limit");
                qObj.transform.SetParent(parent);
                var q = qObj.AddComponent(queueType);
                SetField(q, "maxEntriesPerPlayer", 25);
                SetField(q, "maxBurstEntriesPerPlayer", 10);
            }

            System.Type pdType = GetTypeSafe("ArchiTech.ProTV.PlaylistData");
            if (pdType != null)
            {
                GameObject pdObj = new GameObject("Playlist Thumbnail Bloat");
                pdObj.transform.SetParent(parent);
                var pd = pdObj.AddComponent(pdType);
                SetField(pd, "images", new Sprite[50]);
            }

            System.Type togglesType = GetTypeSafe("ArchiTech.ProTV.TVToggles");
            if (togglesType != null)
            {
                GameObject tObj = new GameObject("Massive TV Toggles Array");
                tObj.transform.SetParent(parent);
                var tgs = tObj.AddComponent(togglesType);
                SetField(tgs, "superGameObjects", new GameObject[25]);
            }

            System.Type queueUiType = GetTypeSafe("ArchiTech.ProTV.QueueUI");
            if (queueUiType != null)
            {
                GameObject rootCanvas = new GameObject("Root Canvas (Rebuild Cascade)");
                rootCanvas.transform.SetParent(parent);
                var c = rootCanvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject queueObj = new GameObject("QueueUI");
                queueObj.transform.SetParent(rootCanvas.transform);
                queueObj.AddComponent(queueUiType);
            }
        }

        private static void CreateUmbrellaIssues(Transform root, string name, int x, int z)
        {
            System.Type toggleType = GetTypeSafe("ArchiTech.Umbrella.ATToggle");
            if (toggleType == null) return;

            Transform parent = DeployPod(root, name, x, z);

            GameObject toggleObj = new GameObject("Massive ATToggle Event");
            toggleObj.transform.SetParent(parent);
            var toggle = toggleObj.AddComponent(toggleType);
            SetField(toggle, "actions", new int[20]);

            System.Type ztType = GetTypeSafe("ArchiTech.Umbrella.ZoneTrigger");
            if (ztType != null)
            {
                GameObject ztObj = new GameObject("Empty ZoneTrigger Collider");
                ztObj.transform.SetParent(parent);
                var zt = ztObj.AddComponent(ztType);
                SetField(zt, "triggerType", 2);
            }

            System.Type proxyType = GetTypeSafe("ArchiTech.Umbrella.ColliderActionProxy");
            if (proxyType != null)
            {
                GameObject proxyObj = new GameObject("Dead ColliderActionProxy");
                proxyObj.transform.SetParent(parent);
                proxyObj.AddComponent<BoxCollider>();
                var proxy = proxyObj.AddComponent(proxyType);
                SetField(proxy, "eventTarget", null);
            }
        }

        private static void CreateExtrasIssues(Transform root, string name, int x, int z)
        {
            System.Type proxyType = GetTypeSafe("ArchiTech.ProTV.Extras.UIToAnimatorProxy");
            if (proxyType == null) return;

            Transform parent = DeployPod(root, name, x, z);

            GameObject proxyObj = new GameObject("Unmapped Animator Proxy");
            proxyObj.transform.SetParent(parent);
            var proxy = proxyObj.AddComponent(proxyType);

            GameObject animObj = new GameObject("Target Animator");
            animObj.transform.SetParent(proxyObj.transform);
            var animator = animObj.AddComponent<Animator>();

            SetField(proxy, "animators", new Animator[] { animator, animator });
            SetField(proxy, "parameters", new string[] { "ValidParam", "" });
        }

        private static void CreateTXLIssues(Transform root, string name, int x, int z)
        {
            System.Type tztType = GetTypeSafe("Texel.TrackedZoneTrigger");
            if (tztType == null) return;

            Transform parent = DeployPod(root, name, x, z);

            GameObject orphan = new GameObject("Orphaned UdonComponent");
            orphan.transform.SetParent(parent);
            orphan.AddComponent<UdonBehaviour>();

            GameObject tztObj = new GameObject("Starvation Polling");
            tztObj.transform.SetParent(parent);
            var tzt = tztObj.AddComponent(tztType);
            SetField(tzt, "monitorTriggerInterval", 0.01f);

            System.Type cztType = GetTypeSafe("Texel.CompoundZoneTrigger");
            if (cztType != null)
            {
                GameObject cztObj = new GameObject("Compound Drag Trigger");
                cztObj.transform.SetParent(parent);
                cztObj.AddComponent<MeshCollider>();
                var czt = cztObj.AddComponent(cztType);
                SetField(czt, "forceColliderCheck", true);
            }

            System.Type transType = GetTypeSafe("Texel.TranslationTable");
            if (transType != null)
            {
                GameObject transObj = new GameObject("Collapsed Translation");
                transObj.transform.SetParent(parent);
                var trans = transObj.AddComponent(transType);
                SetField(trans, "languages", new string[] { "en", "jp" });
                SetField(trans, "keys", new string[] { "key1", "key2" });
                SetField(trans, "values", new string[] { "val1" });
            }

            System.Type digestType = GetTypeSafe("Texel.DigestValidator");
            if (digestType != null)
            {
                GameObject digestObj = new GameObject("Heavy Cryptography Sink");
                digestObj.transform.SetParent(parent);
                digestObj.AddComponent(digestType);
            }

            System.Type dulType = GetTypeSafe("Texel.DebugUserList");
            if (dulType != null)
            {
                GameObject dulObj = new GameObject("TXL Debug GC Sink");
                dulObj.transform.SetParent(parent);
                dulObj.AddComponent(dulType);
            }

            System.Type aclType = GetTypeSafe("Texel.AccessControl");
            if (aclType != null)
            {
                GameObject aclObj = new GameObject("TXL Massive Whitelist");
                aclObj.transform.SetParent(parent);
                var acl = aclObj.AddComponent(aclType);
                SetField(acl, "userWhitelist", new string[60]);
            }

            System.Type screenMgrType = GetTypeSafe("Texel.ScreenManager");
            if (screenMgrType != null)
            {
                var crt = new CustomRenderTexture(256, 256);
                crt.updateMode = CustomRenderTextureUpdateMode.OnDemand;
                crt.doubleBuffered = false;

                GameObject smObj = new GameObject("TXL Screen Tearing CRT");
                smObj.transform.SetParent(parent);
                var sm = smObj.AddComponent(screenMgrType);
                SetField(sm, "outputCRT", crt);
            }
        }

        private static void CreateIwaSyncIssues(Transform root, string name, int x, int z)
        {
            System.Type iwaType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
            if (iwaType == null) return;

            Transform parent = DeployPod(root, name, x, z);

            GameObject iwaObj = new GameObject("IwaSync3 (1080p Blowout)");
            iwaObj.transform.SetParent(parent);
            var iwa = iwaObj.AddComponent(iwaType);
            SetField(iwa, "maximumResolution", 1080);

            System.Type playlistType = GetTypeSafe("HoshinoLabs.IwaSync3.Playlist");
            if (playlistType != null)
            {
                GameObject plObj = new GameObject("Unbounded Iwa Playlist");
                plObj.transform.SetParent(parent);
                var pl = plObj.AddComponent(playlistType);
                SetField(pl, "playlistLimitCount", 0);
            }

            System.Type speakerType = GetTypeSafe("HoshinoLabs.IwaSync3.Speaker");
            if (speakerType != null)
            {
                GameObject spkObj = new GameObject("Iwa Global Speaker");
                spkObj.transform.SetParent(parent);
                var spk = spkObj.AddComponent(speakerType);
                SetField(spk, "spatialize", false);
            }

            System.Type screenType = GetTypeSafe("HoshinoLabs.IwaSync3.Screen");
            System.Type udonScreenType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoScreen");
            if (screenType != null && udonScreenType != null)
            {
                GameObject scrObj = new GameObject("Iwa Blinding Screen");
                scrObj.transform.SetParent(parent);

                var rend = scrObj.AddComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                rend.sharedMaterial = mat;

                var scrEditor = scrObj.AddComponent(screenType);
                SetField(scrEditor, "screen", rend);

                var scrUdon = scrObj.AddComponent(udonScreenType);
                SetField(scrUdon, "defaultEmissiveBoost", 2.5f);
            }

            System.Type videoCoreType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoCore");
            if (videoCoreType != null)
            {
                GameObject coreObj = new GameObject("Iwa Aggressive Sync");
                coreObj.transform.SetParent(parent);
                var core = coreObj.AddComponent(videoCoreType);
                SetField(core, "syncFrequency", 1.0f);
            }

            System.Type eventType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.CustomEventInvoker");
            if (eventType != null)
            {
                GameObject evObj = new GameObject("Iwa GC Event Invoker");
                evObj.transform.SetParent(parent);
                evObj.AddComponent(eventType);
            }
        }

        private static void CreateVizVidIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            System.Type gsType = GetTypeSafe("JLChnToZ.VRC.VVMW.Designer.GlobalSettings");
            if (gsType != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    GameObject gsObj = new GameObject($"VVMW GlobalSettings {i}");
                    gsObj.transform.SetParent(parent);
                    gsObj.AddComponent(gsType);
                }
            }

            System.Type vvmwCoreType = GetTypeSafe("JLChnToZ.VRC.VVMW.Core");
            System.Type vpHandlerType = GetTypeSafe("JLChnToZ.VRC.VVMW.VideoPlayerHandler");
            if (vvmwCoreType != null && vpHandlerType != null)
            {
                GameObject coreObj = new GameObject("VVMW Core Orphaned");
                coreObj.transform.SetParent(parent);
                var core = coreObj.AddComponent(vvmwCoreType);
                SetField(core, "playerHandlers", new Component[0]);

                GameObject coreObj2 = new GameObject("VVMW AVPro No Fallback");
                coreObj2.transform.SetParent(parent);
                var core2 = coreObj2.AddComponent(vvmwCoreType);

                GameObject handlerObj = new GameObject("AVPro Handler");
                handlerObj.transform.SetParent(coreObj2.transform);
                var handler = handlerObj.AddComponent(vpHandlerType);
                SetField(handler, "isAvPro", true);
                SetField(handler, "fallbackHandler", null);
                SetField(core2, "playerHandlers", new Component[] { handler });

                var audio = coreObj2.AddComponent<AudioSource>();
                audio.spatialBlend = 0f;
                SetField(core2, "audioSources", new AudioSource[] { audio });
                SetField(core2, "audioLink", null);
            }

            System.Type frontendType = GetTypeSafe("JLChnToZ.VRC.VVMW.FrontendHandler");
            if (frontendType != null)
            {
                GameObject frontObj = new GameObject("VVMW Orphaned Frontend");
                frontObj.transform.SetParent(parent);
                var front = frontObj.AddComponent(frontendType);
                SetField(front, "core", null);
            }
        }

        private static void CreateAudioLinkAndLightVolumeIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            System.Type alType = GetTypeSafe("AudioLink.AudioLink");
            if (alType != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    GameObject alObj = new GameObject($"AudioLink Core {i}");
                    alObj.transform.SetParent(parent);
                    var al = alObj.AddComponent(alType);
                    SetField(al, "audioDataToggle", true);
                }

                System.Type reactType = GetTypeSafe("AudioLink.AudioReactive");
                if (reactType != null)
                {
                    GameObject reactObj = new GameObject("Orphaned AudioReactive");
                    reactObj.transform.SetParent(parent);
                    var react = reactObj.AddComponent(reactType);
                    SetField(react, "audioLink", null);
                }
            }

            System.Type lvMgrType = GetTypeSafe("VRCLightVolumes.LightVolumeManager");
            if (lvMgrType != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    GameObject lvMgr = new GameObject($"LV Manager {i}");
                    lvMgr.transform.SetParent(parent);
                    lvMgr.AddComponent(lvMgrType);
                }

                System.Type lvSetupType = GetTypeSafe("VRCLightVolumes.LightVolumeSetup");
                GameObject setupObj = new GameObject("LV Setup Cutoff Nuke");
                setupObj.transform.SetParent(parent);
                var setup = setupObj.AddComponent(lvSetupType);
                SetField(setup, "LightsBrightnessCutoff", 0.05f);

                System.Type plvType = GetTypeSafe("VRCLightVolumes.PointLightVolume");
                GameObject plvObj = new GameObject("Area Light Volume");
                plvObj.transform.SetParent(parent);
                var plv = plvObj.AddComponent(plvType);
                SetField(plv, "Type", 2);

                System.Type tvgiType = GetTypeSafe("VRCLightVolumes.LightVolumeTVGI");
                GameObject tvgiObj = new GameObject("TVGI Seizure Risk");
                tvgiObj.transform.SetParent(parent);
                var tvgi = tvgiObj.AddComponent(tvgiType);
                SetField(tvgi, "TargetRenderTexture", null);
                SetField(tvgi, "AntiFlickering", false);

                System.Type alLvType = GetTypeSafe("VRCLightVolumes.LightVolumeAudioLink");
                GameObject alLvObj = new GameObject("LV AudioLink Flicker Risk");
                alLvObj.transform.SetParent(parent);
                var alLv = alLvObj.AddComponent(alLvType);
                SetField(alLv, "SmoothingEnabled", false);
            }
        }

        private static void CreateRinvoIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            System.Type rinvoType = GetTypeSafe("Rinvo.YoutubeSearchManager");
            if (rinvoType != null)
            {
                GameObject rinvoObj = new GameObject("Rinvo Search Bounds Failure");
                rinvoObj.transform.SetParent(parent);
                var rinvo = rinvoObj.AddComponent(rinvoType);
                SetField(rinvo, "poolSize", 500000);
                SetField(rinvo, "VideoPlayerUIController", null);
                SetField(rinvo, "UrlInputField", null);
            }
        }

        private static void CreatePersistenceAndNetworkIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            System.Type poType = GetTypeSafe("VRC.SDK3.Persistence.VRCPlayerObject");
            if (poType != null)
            {
                GameObject hollowObj = new GameObject("Hollow Player Object");
                hollowObj.transform.SetParent(parent);
                hollowObj.AddComponent(poType);

                GameObject syncObj = new GameObject("Continuous Player Object");
                syncObj.transform.SetParent(parent);
                syncObj.AddComponent(poType);
                var udon = syncObj.AddComponent<UdonBehaviour>();
                udon.SyncMethod = VRC.SDKBase.Networking.SyncType.Continuous;
            }

            GameObject contObj = new GameObject("Continuous Sync Generic");
            contObj.transform.SetParent(parent);
            var contUdon = contObj.AddComponent<UdonBehaviour>();
            contUdon.SyncMethod = VRC.SDKBase.Networking.SyncType.Continuous;

            GameObject physSyncObj = new GameObject("VRC Object Sync Drag");
            physSyncObj.transform.SetParent(parent);
            physSyncObj.AddComponent<VRCObjectSync>();
        }

        private static void CreateVideoPipelineIssues(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            System.Type avProType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer");
            if (avProType != null)
            {
                GameObject avObj = new GameObject("AVPro 4K Unlimited Sink");
                avObj.transform.SetParent(parent);
                var avPro = avObj.AddComponent(avProType);
                SetField(avPro, "maximumResolution", 0);
                SetField(avPro, "useLowLatency", true);
            }

            System.Type proTvType = GetTypeSafe("ArchiTech.ProTV.TVManager");
            System.Type vpmType = GetTypeSafe("ArchiTech.ProTV.VPManager");
            if (proTvType != null && vpmType != null)
            {
                GameObject tvObj = new GameObject("ProTV VRAM Nuke & GSV Desync");
                tvObj.transform.SetParent(parent);
                var tv = tvObj.AddComponent(proTvType);
                SetField(tv, "enableGSV", true);

                RenderTexture massiveTex = new RenderTexture(4096, 4096, 0);
                SetField(tv, "customTexture", massiveTex);

                GameObject vpmObj = new GameObject("VPManager (GI Sink & Uncalibrated & Bleed)");
                vpmObj.transform.SetParent(tvObj.transform);
                var vpm = vpmObj.AddComponent(vpmType);

                GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                screenObj.name = "1:1 Uncalibrated Screen Surface";
                screenObj.transform.SetParent(vpmObj.transform);
                screenObj.transform.localScale = new Vector3(1, 1, 1);

                var mat = new Material(Shader.Find("Standard"));
                Shader proTvShader = Shader.Find("ProTV/VideoScreen");
                if (proTvShader != null)
                {
                    mat = new Material(proTvShader);
                    mat.DisableKeyword("_USEGLOBALTEXTURE");
                }

                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                screenObj.GetComponent<MeshRenderer>().sharedMaterial = mat;

                GameObject audioObj = new GameObject("Spatialization Bleed Speaker");
                audioObj.transform.SetParent(vpmObj.transform);
                var audioSrc = audioObj.AddComponent<AudioSource>();
                audioSrc.spatialBlend = 1.0f;
                audioSrc.maxDistance = 500f;

                SetField(vpm, "screens", new GameObject[] { screenObj });
                SetField(vpm, "speakers", new AudioSource[] { audioSrc });
            }
        }

        private static void CreateVramNightmare(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            if (!File.Exists(TestTexturePath))
            {
                var texDir = Path.GetDirectoryName(TestTexturePath);
                if (!string.IsNullOrEmpty(texDir) && !Directory.Exists(texDir)) Directory.CreateDirectory(texDir);

                Texture2D nukeTex = new Texture2D(4096, 4096, TextureFormat.RGBA32, false);
                Color[] colors = new Color[4096 * 4096];
                for (int i = 0; i < colors.Length; i++) colors[i] = new Color(Random.value, 0, Random.value, 1);
                nukeTex.SetPixels(colors);
                nukeTex.Apply();

                try
                {
                    byte[] bytes = nukeTex.EncodeToPNG();
                    File.WriteAllBytes(TestTexturePath, bytes);

                    AssetDatabase.ImportAsset(TestTexturePath, ImportAssetOptions.ForceUpdate);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to create/import texture at {TestTexturePath}: {ex.Message}");
                    AssetDatabase.CreateAsset(nukeTex, TestTexturePath);
                }
            }

            TextureImporter importer = AssetImporter.GetAtPath(TestTexturePath) as TextureImporter;
            if (importer != null)
            {
                try
                {
                    importer.maxTextureSize = 4096;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Texture importer SaveAndReimport failed for {TestTexturePath}: {ex.Message}");
                }
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "VRAM_NUKE_DISPLAY";
            cube.transform.SetParent(parent);
            cube.transform.localScale = new Vector3(5, 5, 5);

            Material nukeMat = new Material(Shader.Find("Standard"));
            nukeMat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TestTexturePath);
            cube.GetComponent<MeshRenderer>().material = nukeMat;
        }

        private static System.Type GetTypeSafe(string typeName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = assembly.GetType(typeName);
                if (t != null) return t;
            }
            return null;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null || string.IsNullOrEmpty(fieldName)) return;

            Type type = target.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type?.BaseType;
            }
            if (field == null) return;

            try
            {
                Type fieldType = field.FieldType;

                if (value == null)
                {
                    if (fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) == null)
                    {
                        return;
                    }
                    field.SetValue(target, null);
                    return;
                }

                Type valueType = value.GetType();

                if (fieldType.IsAssignableFrom(valueType))
                {
                    field.SetValue(target, value);
                    return;
                }

                if (fieldType.IsEnum)
                {
                    object enumVal = null;
                    if (value is string s) enumVal = Enum.Parse(fieldType, s);
                    else enumVal = Enum.ToObject(fieldType, value);
                    field.SetValue(target, enumVal);
                    return;
                }

                if (IsNumericType(fieldType) && IsNumericType(valueType))
                {
                    object converted = Convert.ChangeType(value, fieldType);
                    field.SetValue(target, converted);
                    return;
                }

                if (fieldType.IsArray && valueType.IsArray)
                {
                    var elemType = fieldType.GetElementType();
                    var src = (System.Array)value;
                    var dst = System.Array.CreateInstance(elemType, src.Length);
                    for (int i = 0; i < src.Length; i++)
                    {
                        var item = src.GetValue(i);
                        if (item == null) { dst.SetValue(null, i); continue; }
                        if (elemType.IsAssignableFrom(item.GetType())) dst.SetValue(item, i);
                        else if (IsNumericType(elemType) && IsNumericType(item.GetType()))
                        {
                            dst.SetValue(Convert.ChangeType(item, elemType), i);
                        }
                        else dst.SetValue(null, i);
                    }
                    field.SetValue(target, dst);
                    return;
                }

                try
                {
                    var converted = Convert.ChangeType(value, fieldType);
                    field.SetValue(target, converted);
                    return;
                }
                catch { }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SetField failed for {target.GetType().Name}.{fieldName}: {ex.Message}");
            }
        }

        private static bool IsNumericType(Type t)
        {
            if (t == null) return false;
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
#endif