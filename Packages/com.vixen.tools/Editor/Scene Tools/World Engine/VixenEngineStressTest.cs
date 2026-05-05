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
using System.IO;
using System.Reflection;

namespace VixenTools.Editor.QA
{
    public class VixenEngineStressTest : EditorWindow
    {
        private const string TestTexturePath = "Assets/Vixen_VRAM_Nuke_4K.png";
        private const string TestMeshPath = "Assets/Vixen_Poly_Nuke.asset";
        private const string SceneName = "Stress Test";
        private const string ScenePath = "Assets/Stress Test.unity";

        [MenuItem("VixenTools/QA/Generate Omni-Chaos Environment")]
        public static void GenerateChaos()
        {
            // --- SCENE MANAGEMENT PROTOCOL ---
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
                    return; // User aborted
                }
            }

            // --- VRC BASE WORLD GENERATION ---
            GenerateVRChatBaseArchitecture();

            GameObject root = new GameObject("== VIXEN OMNI-CHAOS ROOT ==");
            
            // Deploy Base Pods (Always Available via Unity/VRCSDK)
            CreateStandardPerformanceIssues(root.transform, "1. Performance & Geometry Pit", 0, 0);
            CreateUIAndCanvasIssues(root.transform, "2. UI Void & Rebuild Cascades", 1, 0);
            CreateVramNightmare(root.transform, "5. VRAM Nightmare", 4, 0);
            CreatePersistenceAndNetworkIssues(root.transform, "7. Network & Persistence Void", 1, 1);
            CreateVideoPipelineIssues(root.transform, "8. Video Pipeline Collapse", 2, 1);

            // Deploy Third-Party Pods (Verified via Reflection first)
            CreateProTVIssues(root.transform, "3. ProTV Sink", 2, 0);
            CreateTXLIssues(root.transform, "4. TXL Death-Trap", 3, 0);
            CreateIwaSyncIssues(root.transform, "6. IwaSync3 Apocalypse", 0, 1);

            Debug.Log("<color=#ff00aa>[Vixen QA]</color> Omni-Chaos Environment Generated. The engine will now light up like a Christmas tree.");
            Selection.activeGameObject = root;
        }

        private static void GenerateVRChatBaseArchitecture()
        {
            // 1. Lighting
            if (RenderSettings.sun == null && !GameObject.Find("Directional Light"))
            {
                GameObject dirLightObj = new GameObject("Directional Light");
                var dirLight = dirLightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.color = new Color(1f, 0.95f, 0.85f);
                dirLight.intensity = 1f;
                dirLightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            // 2. Floor
            if (!GameObject.Find("Floor"))
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.localScale = new Vector3(10, 1, 10);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = Color.gray;
                floor.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            // 3. VRC Scene Descriptor
            if (Object.FindObjectOfType<VRCSceneDescriptor>() == null)
            {
                GameObject vrcWorldObj = new GameObject("VRCWorld");
                var descriptor = vrcWorldObj.AddComponent<VRCSceneDescriptor>();
                
                GameObject spawnPoint = new GameObject("Spawn Point");
                spawnPoint.transform.SetParent(vrcWorldObj.transform);
                spawnPoint.transform.position = new Vector3(0, 0, -5); // Set slightly back from origin
                
                descriptor.spawns = new Transform[] { spawnPoint.transform };
                descriptor.ReferenceCamera = GameObject.FindObjectOfType<Camera>()?.gameObject;
            }
        }

        private static Transform DeployPod(Transform root, string name, int x, int z)
        {
            GameObject pod = new GameObject(name);
            pod.transform.SetParent(root);
            pod.transform.position = new Vector3(x * 20f, 0, z * 20f); // 20-meter spread
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

            GameObject audioObj = new GameObject("2D Audio Source");
            audioObj.transform.SetParent(parent);
            var source = audioObj.AddComponent<AudioSource>();
            source.spatialBlend = 0f; 

            if (!File.Exists(TestMeshPath))
            {
                Mesh heavyMesh = new Mesh { name = "Vixen_Poly_Nuke" };
                Vector3[] verts = new Vector3[66000];
                int[] tris = new int[3]; 
                heavyMesh.vertices = verts;
                heavyMesh.triangles = tris;
                AssetDatabase.CreateAsset(heavyMesh, TestMeshPath);
            }

            GameObject heavyObj = new GameObject("66k Vert Mesh (No LOD)");
            heavyObj.transform.SetParent(parent);
            var mf = heavyObj.AddComponent<MeshFilter>();
            var mr = heavyObj.AddComponent<MeshRenderer>();
            
            Mesh loadedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(TestMeshPath);
            mf.sharedMesh = loadedMesh;
            
            ModelImporter imp = AssetImporter.GetAtPath(TestMeshPath) as ModelImporter;
            if (imp != null) { imp.isReadable = true; imp.SaveAndReimport(); } 
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
            if (tvType == null) return; // Verify Addon Exists

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
                SetField(search, "searchAggressionLevel", 20); 
            }

            System.Type queueType = GetTypeSafe("ArchiTech.ProTV.QueueUI");
            if (queueType != null)
            {
                GameObject rootCanvas = new GameObject("Root Canvas (Rebuild Cascade)");
                rootCanvas.transform.SetParent(parent);
                var c = rootCanvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject queueObj = new GameObject("QueueUI");
                queueObj.transform.SetParent(rootCanvas.transform);
                queueObj.AddComponent(queueType);
            }
        }

        private static void CreateTXLIssues(Transform root, string name, int x, int z)
        {
            System.Type tztType = GetTypeSafe("Texel.TrackedZoneTrigger");
            if (tztType == null) return; // Verify Addon Exists

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
        }

        private static void CreateIwaSyncIssues(Transform root, string name, int x, int z)
        {
            System.Type iwaType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
            if (iwaType == null) return; // Verify Addon Exists

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
                GameObject tvObj = new GameObject("ProTV VRAM Nuke");
                tvObj.transform.SetParent(parent);
                var tv = tvObj.AddComponent(proTvType);
                
                RenderTexture massiveTex = new RenderTexture(4096, 4096, 0);
                SetField(tv, "customTexture", massiveTex);

                GameObject vpmObj = new GameObject("VPManager (GI Sink)");
                vpmObj.transform.SetParent(parent);
                var vpm = vpmObj.AddComponent(vpmType);
                
                GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                screenObj.name = "Realtime Screen Surface";
                screenObj.transform.SetParent(vpmObj.transform);
                var mat = new Material(Shader.Find("Standard"));
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive; 
                screenObj.GetComponent<MeshRenderer>().sharedMaterial = mat;

                SetField(vpm, "screens", new GameObject[] { screenObj });
            }
        }

        private static void CreateVramNightmare(Transform root, string name, int x, int z)
        {
            Transform parent = DeployPod(root, name, x, z);

            if (!File.Exists(TestTexturePath))
            {
                Texture2D nukeTex = new Texture2D(4096, 4096, TextureFormat.RGBA32, false);
                Color[] colors = new Color[4096 * 4096];
                for (int i = 0; i < colors.Length; i++) colors[i] = new Color(Random.value, 0, Random.value, 1);
                nukeTex.SetPixels(colors);
                nukeTex.Apply();

                byte[] bytes = nukeTex.EncodeToPNG();
                File.WriteAllBytes(TestTexturePath, bytes);
                AssetDatabase.ImportAsset(TestTexturePath);
            }

            TextureImporter importer = AssetImporter.GetAtPath(TestTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.maxTextureSize = 4096;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = true; 
                importer.SaveAndReimport();
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
            if (target == null) return; 
            
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) field.SetValue(target, value);
        }
    }
}
#endif