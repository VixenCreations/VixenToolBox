#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && UDON
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    public class VixenHeuristicsDashboard : EditorWindow
    {
        private HashSet<Texture> _detectedTextures = new HashSet<Texture>();
        private HashSet<Mesh> _detectedMeshes = new HashSet<Mesh>();
        private HashSet<AudioClip> _detectedAudio = new HashSet<AudioClip>();
        private HashSet<Texture> _detectedUITextures = new HashSet<Texture>();

        // 4D-Chess: Static reflection cache. Eliminates the massive AppDomain assembly sweep bottleneck.
        private static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
        
        // 4D-Chess: Scene Object Cache. Eliminates O(N) scene traversal freezes during UI construction.
        private Dictionary<Type, UnityEngine.Object[]> _sceneObjectCache = new Dictionary<Type, UnityEngine.Object[]>();

        private T[] GetCachedObjects<T>(bool includeInactive = true) where T : UnityEngine.Object
        {
            Type t = typeof(T);
            if (_sceneObjectCache.TryGetValue(t, out var cached)) return cached as T[];
            var objs = FindObjectsOfType<T>(includeInactive);
            _sceneObjectCache[t] = objs;
            return objs;
        }

        private UnityEngine.Object[] GetCachedObjects(Type t, bool includeInactive = true)
        {
            if (t == null) return new UnityEngine.Object[0];
            if (_sceneObjectCache.TryGetValue(t, out var cached)) return cached;
            var objs = FindObjectsOfType(t, includeInactive);
            _sceneObjectCache[t] = objs;
            return objs;
        }

        public static void Open(HashSet<Texture> textures, HashSet<Mesh> meshes, HashSet<AudioClip> audio, HashSet<Texture> uiTextures)
        {
            var window = GetWindow<VixenHeuristicsDashboard>("WORLD PROFILER", true);

            // Set the strict minimum bounds
            window.minSize = new Vector2(385, 625);

            // Force the starting open window size to accommodate the new systems
            var pos = window.position;
            window.position = new Rect(pos.x, pos.y, 385, 625);

            // Pass the Spider's collected data into the standalone dashboard
            window._detectedTextures = textures ?? new HashSet<Texture>();
            window._detectedMeshes = meshes ?? new HashSet<Mesh>();
            window._detectedAudio = audio ?? new HashSet<AudioClip>();
            window._detectedUITextures = uiTextures ?? new HashSet<Texture>();

            window.RenderDashboard();
            window.ShowUtility();
        }

        private void RenderDashboard()
        {
            rootVisualElement.Clear();
            _sceneObjectCache.Clear(); // Purge cache so metrics are fresh each time the dashboard opens

            // Load the existing Vixen styles so the dashboard looks identical
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenWorldSpider.uss");
            if (styleSheet != null) rootVisualElement.styleSheets.Add(styleSheet);

            rootVisualElement.style.backgroundColor = new StyleColor(new Color(0.04f, 0.04f, 0.06f)); // Vixen Dark
            var scroll = new ScrollView { style = { flexGrow = 1 } };
            rootVisualElement.Add(scroll);

            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            // === BASE VRAM SWEEPS ===
            long texBytes = _detectedTextures.Sum(t => t != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t) : 0);
            long meshBytes = _detectedMeshes.Sum(m => m != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(m) : 0);
            long audioBytes = _detectedAudio.Sum(a => a != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(a) : 0);
            long uiBytes = _detectedUITextures.Sum(t => t != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t) : 0);

            // === AUDIOLINK VRAM ===
            long audioLinkBytes = 0;
            int audioLinkActive = 0;
            
            Type audioLinkType = GetTypeSafe("AudioLink.AudioLink");
            if (audioLinkType != null)
            {
                var alInstances = GetCachedObjects(audioLinkType, true);
                audioLinkActive = alInstances.Length;
                foreach (var al in alInstances)
                {
                    var rtField = audioLinkType.GetField("audioData", flags);
                    if (rtField != null && rtField.GetValue(al) is Texture rt && rt != null)
                        audioLinkBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(rt);
                        
                    var rt2DField = audioLinkType.GetField("audioData2D", flags);
                    if (rt2DField != null && rt2DField.GetValue(al) is Texture rt2d && rt2d != null)
                        audioLinkBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(rt2d);
                }
            }

            // === LTCGI VRAM & COMPUTE ===
            long ltcgiBytes = 0;
            int ltcgiScreens = 0;
            Type ltcgiAdapterType = GetTypeSafe("LTCGI_UdonAdapter");
            if (ltcgiAdapterType != null)
            {
                foreach (var adapter in GetCachedObjects(ltcgiAdapterType, true))
                {
                    // Core LUTs
                    var luts = new[] { "_LTCGI_lut1", "_LTCGI_lut2", "_LTCGI_DefaultLightmap" };
                    foreach (var lut in luts)
                    {
                        var f = ltcgiAdapterType.GetField(lut, flags);
                        if (f != null && f.GetValue(adapter) is Texture t && t != null)
                            ltcgiBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t);
                    }

                    // Static Precomputed LODs
                    var lods = new[] { "_LTCGI_Static_LODs_0", "_LTCGI_Static_LODs_1", "_LTCGI_Static_LODs_2", "_LTCGI_Static_LODs_3" };
                    foreach (var lod in lods)
                    {
                        var f = ltcgiAdapterType.GetField(lod, flags);
                        if (f != null && f.GetValue(adapter) is Texture t && t != null)
                            ltcgiBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t);
                    }

                    // Dynamic Video LODs
                    var dynLodsField = ltcgiAdapterType.GetField("_LTCGI_LODs", flags);
                    if (dynLodsField != null && dynLodsField.GetValue(adapter) is Texture[] dynLods)
                    {
                        foreach (var l in dynLods) if (l != null) ltcgiBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(l);
                    }

                    // Lightmaps
                    var lmapsField = ltcgiAdapterType.GetField("_LTCGI_Lightmaps", flags);
                    if (lmapsField != null && lmapsField.GetValue(adapter) is Texture[] lmaps)
                    {
                        foreach (var lm in lmaps) if (lm != null) ltcgiBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(lm);
                    }

                    // Active Screens Compute Load
                    var countField = ltcgiAdapterType.GetField("_LTCGI_ScreenCount", flags);
                    if (countField != null) ltcgiScreens += Convert.ToInt32(countField.GetValue(adapter));
                }
            }

            // === LIGHTMAP VRAM ===
            long lightmapBytes = 0;
            if (LightmapSettings.lightmaps != null)
            {
                foreach (var lm in LightmapSettings.lightmaps)
                {
                    if (lm.lightmapColor != null) lightmapBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(lm.lightmapColor);
                    if (lm.lightmapDir != null) lightmapBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(lm.lightmapDir);
                    if (lm.shadowMask != null) lightmapBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(lm.shadowMask);
                }
            }

            // === LIGHT VOLUMES VRAM ===
            long lvBytes = 0;
            Type lvManagerType = GetTypeSafe("VRCLightVolumes.LightVolumeManager");
            if (lvManagerType != null)
            {
                foreach (var manager in GetCachedObjects(lvManagerType, true))
                {
                    var atlasField = lvManagerType.GetField("LightVolumeAtlas", flags);
                    if (atlasField != null && atlasField.GetValue(manager) is Texture tex && tex != null)
                        lvBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);

                    var baseAtlasField = lvManagerType.GetField("LightVolumeAtlasBase", flags);
                    if (baseAtlasField != null && baseAtlasField.GetValue(manager) is Texture3D tex3d && tex3d != null)
                        lvBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex3d);

                    var customTexField = lvManagerType.GetField("CustomTextures", flags);
                    if (customTexField != null && customTexField.GetValue(manager) is Texture2DArray texArr && texArr != null)
                        lvBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texArr);
                }
            }

            float texMB = texBytes / 1048576f;
            float meshMB = meshBytes / 1048576f;
            float audioMB = audioBytes / 1048576f;
            float uiMB = uiBytes / 1048576f;
            float lightmapMB = lightmapBytes / 1048576f;
            float lvMB = lvBytes / 1048576f;
            float audioLinkMB = audioLinkBytes / 1048576f;
            float ltcgiMB = ltcgiBytes / 1048576f;

            float totalVramMB = texMB + meshMB + uiMB + lightmapMB + lvMB + audioLinkMB + ltcgiMB;

            // === SCENE METRICS ===
            var renderers = GetCachedObjects<Renderer>(true);
            int estDrawCalls = renderers.Sum(r => r.sharedMaterials.Length);
            int rigidbodies = GetCachedObjects<Rigidbody>(true).Length;

            int realtimeShadowCasters = GetCachedObjects<Light>(true).Count(l =>
                l.lightmapBakeType != LightmapBakeType.Baked &&
                l.shadows != LightShadows.None);

            int audioSourceCount = GetCachedObjects<AudioSource>(true).Length;

            // === VIDEO PLAYER DETECTION ===
            HashSet<Component> logicalPlayers = new HashSet<Component>();
            Type tvManagerType = GetTypeSafe("ArchiTech.ProTV.TVManager");
            Type iwaSyncType = GetTypeSafe("HoshinoLabs.IwaSync3.IwaSync3");
            Type txlPlayerType = GetTypeSafe("Texel.TXLVideoPlayer");
            Type vvmwCoreType = GetTypeSafe("JLChnToZ.VRC.VVMW.Core");

            if (tvManagerType != null) foreach (var t in GetCachedObjects(tvManagerType, true)) logicalPlayers.Add((Component)t);
            if (iwaSyncType != null) foreach (var t in GetCachedObjects(iwaSyncType, true)) logicalPlayers.Add((Component)t);
            if (txlPlayerType != null) foreach (var t in GetCachedObjects(txlPlayerType, true)) logicalPlayers.Add((Component)t);
            if (vvmwCoreType != null) foreach (var t in GetCachedObjects(vvmwCoreType, true)) logicalPlayers.Add((Component)t);

            Type avproPlayerType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer");
            Type unityPlayerType = GetTypeSafe("VRC.SDK3.Video.Components.VRCUnityVideoPlayer");

            if (avproPlayerType != null)
            {
                foreach (var p in GetCachedObjects(avproPlayerType, true))
                {
                    var comp = (Component)p;
                    if ((tvManagerType == null || comp.GetComponentInParent(tvManagerType, true) == null) &&
                        (iwaSyncType == null || comp.GetComponentInParent(iwaSyncType, true) == null) &&
                        (txlPlayerType == null || comp.GetComponentInParent(txlPlayerType, true) == null) &&
                        (vvmwCoreType == null || comp.GetComponentInParent(vvmwCoreType, true) == null))
                        logicalPlayers.Add(comp);
                }
            }

            if (unityPlayerType != null)
            {
                foreach (var p in GetCachedObjects(unityPlayerType, true))
                {
                    var comp = (Component)p;
                    if ((tvManagerType == null || comp.GetComponentInParent(tvManagerType, true) == null) &&
                        (iwaSyncType == null || comp.GetComponentInParent(iwaSyncType, true) == null) &&
                        (txlPlayerType == null || comp.GetComponentInParent(txlPlayerType, true) == null) &&
                        (vvmwCoreType == null || comp.GetComponentInParent(vvmwCoreType, true) == null))
                        logicalPlayers.Add(comp);
                }
            }

            int totalVideoPlayers = logicalPlayers.Count;

            // === SCREEN DETECTION ===
            HashSet<GameObject> uniqueScreens = new HashSet<GameObject>();

            // PROTV SCREENS
            Type vpmType = GetTypeSafe("ArchiTech.ProTV.VPManager");
            if (vpmType != null)
            {
                foreach (var vpm in GetCachedObjects(vpmType, true))
                {
                    var comp = (Component)vpm;
                    var r = comp.GetComponent<Renderer>();
                    if (r != null) uniqueScreens.Add(r.gameObject);

                    var customMatsField = vpmType.GetField("customMaterials", flags);
                    if (customMatsField != null && customMatsField.GetValue(vpm) is Renderer[] customMats)
                        foreach (var cm in customMats) if (cm != null) uniqueScreens.Add(cm.gameObject);

                    var screensField = vpmType.GetField("screens", flags);
                    if (screensField != null && screensField.GetValue(vpm) is GameObject[] screens)
                        foreach (var s in screens) if (s != null) uniqueScreens.Add(s);
                }
            }

            // AVPRO & IWA SCREENS
            Type avproScreenType = GetTypeSafe("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoScreen");
            if (avproScreenType != null)
                foreach (var s in GetCachedObjects(avproScreenType, true)) uniqueScreens.Add(((Component)s).gameObject);

            Type iwaScreenType = GetTypeSafe("HoshinoLabs.IwaSync3.Screen");
            if (iwaScreenType != null)
                foreach (var s in GetCachedObjects(iwaScreenType, true)) uniqueScreens.Add(((Component)s).gameObject);

            Type iwaUdonScreenType = GetTypeSafe("HoshinoLabs.IwaSync3.Udon.VideoScreen");
            if (iwaUdonScreenType != null)
                foreach (var s in GetCachedObjects(iwaUdonScreenType, true)) uniqueScreens.Add(((Component)s).gameObject);

            // VIZVID (VVMW) SCREENS
            if (vvmwCoreType != null)
            {
                foreach (var core in GetCachedObjects(vvmwCoreType, true))
                {
                    var screenTargetsField = vvmwCoreType.GetField("screenTargets", flags);
                    if (screenTargetsField != null && screenTargetsField.GetValue(core) is UnityEngine.Object[] targets)
                    {
                        foreach (var target in targets)
                        {
                            if (target is Renderer rend && rend != null) uniqueScreens.Add(rend.gameObject);
                            else if (target is GameObject go && go != null) uniqueScreens.Add(go);
                        }
                    }
                }
            }

            // DEFAULT UNITY SCREENS
            if (unityPlayerType != null)
            {
                foreach (var uvp in GetCachedObjects(unityPlayerType, true))
                {
                    var targetRendererField = unityPlayerType.GetField("targetMaterialRenderer", flags);
                    if (targetRendererField != null && targetRendererField.GetValue(uvp) is Renderer r && r != null)
                        uniqueScreens.Add(r.gameObject);
                }
            }

            int screenCount = uniqueScreens.Count;

            // === SCENE OBJECT METRICS ===
            int activeCameras = GetCachedObjects<Camera>(true).Count(c =>
                c.gameObject.activeInHierarchy &&
                c.name != "VRCCam" &&
                c.gameObject.tag != "MainCamera");

            int reflectionProbes = GetCachedObjects<ReflectionProbe>(true).Length;
            int meshColliders = GetCachedObjects<MeshCollider>(true).Length;
            int terrains = GetCachedObjects<Terrain>(true).Length;
            int lightmapCount = LightmapSettings.lightmaps != null ? LightmapSettings.lightmaps.Length : 0;

            Type lvType = GetTypeSafe("VRCLightVolumes.LightVolume");
            Type pointLvType = GetTypeSafe("VRCLightVolumes.PointLightVolume");

            int staticLightVolumes = lvType != null ? GetCachedObjects(lvType, true).Length : 0;
            int pointLightVolumes = pointLvType != null ? GetCachedObjects(pointLvType, true).Length : 0;

            // === COMPUTE SCORE ===
            float computeScore =
                (estDrawCalls * 0.5f) +
                (rigidbodies * 2.0f) +
                (realtimeShadowCasters * 80.0f) +
                (audioSourceCount * 1.5f) +
                (activeCameras * 50.0f) +
                (reflectionProbes * 10.0f) +
                (meshColliders * 0.5f) +
                (staticLightVolumes * 1.5f) +
                (pointLightVolumes * 4.0f) +
                (audioLinkActive * 150.0f) +   // AudioLink FFT is extremely heavy on compute
                (ltcgiScreens * 15.0f);        // LTCGI evaluates complex intersection models per-fragment per-screen

            string threatLevel =
                computeScore < 100 ? "<color=#00ff88>OPTIMAL</color>" :
                computeScore < 250 ? "<color=#ffaa00>MODERATE</color>" :
                computeScore < 500 ? "<color=#ff5555>HIGH</color>" :
                "<color=#ff00aa>SEVERE</color>";

            // === UI PANEL ===
            var dash = new VisualElement();
            dash.AddToClassList("dashboard-panel");

            dash.Add(new Label("WORLD PROFILER : MEMORY & COMPUTE") { name = "dash-header" });

            dash.Add(CreateDashStat("TOTAL ESTIMATED VRAM", $"{totalVramMB:F2} MB", "#00e5ff"));
            dash.Add(CreateDashStat("  ■ TEXTURE MEMORY", $"{texMB:F2} MB", "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ MESH GEOMETRY", $"{meshMB:F2} MB", "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ UI/TMP MEMORY", $"{uiMB:F2} MB", "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ LIGHTMAP DATA", $"{lightmapMB:F2} MB", lightmapMB > 100 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ VOLUMETRIC DATA", $"{lvMB:F2} MB", lvMB > 50 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ AUDIOLINK DATA", $"{audioLinkMB:F2} MB", audioLinkMB > 30 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ LTCGI DATA", $"{ltcgiMB:F2} MB", ltcgiMB > 50 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("AUDIO RAM FOOTPRINT", $"{audioMB:F2} MB", "#ffaa00"));

            dash.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f)), marginTop = 8, marginBottom = 8 } });

            dash.Add(CreateDashStat("  ■ ESTIMATED DRAW CALLS", $"{estDrawCalls}", "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ RT SHADOW LIGHTS", $"{realtimeShadowCasters}", realtimeShadowCasters > 1 ? "#ff00aa" : "#ffaa00"));
            dash.Add(CreateDashStat("  ■ BAKED LIGHTMAPS", $"{lightmapCount}", lightmapCount > 5 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ REFLECTION PROBES", $"{reflectionProbes}", reflectionProbes > 3 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ ACTIVE ROGUE CAMERAS", $"{activeCameras}", activeCameras > 0 ? "#ff00aa" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ STATIC LIGHT VOLUMES", $"{staticLightVolumes}", staticLightVolumes > 15 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ POINT LIGHT VOLUMES", $"{pointLightVolumes}", pointLightVolumes > 5 ? "#ffaa00" : "#e0e0e0"));

            dash.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f)), marginTop = 8, marginBottom = 8 } });

            dash.Add(CreateDashStat("  ■ PHYSICS RIGIDBODIES", $"{rigidbodies}", rigidbodies > 20 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ MESH COLLIDERS", $"{meshColliders}", meshColliders > 10 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ TERRAIN GRIDS", $"{terrains}", "#e0e0e0"));

            dash.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f)), marginTop = 8, marginBottom = 8 } });

            dash.Add(CreateDashStat("  ■ ACTIVE AUDIO SOURCES", $"{audioSourceCount}", audioSourceCount > 20 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ ACTIVE AUDIOLINK CORES", $"{audioLinkActive}", audioLinkActive > 1 ? "#ff00aa" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ LTCGI SCREENS", $"{ltcgiScreens}", ltcgiScreens > 4 ? "#ffaa00" : "#e0e0e0"));
            dash.Add(CreateDashStat("  ■ VIDEO SCREENS", $"{screenCount}", "#ff00aa"));
            dash.Add(CreateDashStat("  ■ VIDEO PLAYERS", $"{totalVideoPlayers}", "#e0e0e0"));

            dash.Add(new VisualElement { style = { height = 1, backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f)), marginTop = 8, marginBottom = 8 } });

            dash.Add(CreateDashStat("COMPUTE THREAT LEVEL", $"{threatLevel}", "#ffffff", true));

            scroll.Add(dash);
        }

        private VisualElement CreateDashStat(string title, string value, string hexCol, bool richText = false)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, paddingTop = 2, paddingBottom = 2 } };
            ColorUtility.TryParseHtmlString("#00e5ff", out Color titleColor);
            row.Add(new Label(title) { style = { color = new StyleColor(titleColor), fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });

            var valLabel = new Label(value) { enableRichText = richText, style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } };
            if (!richText)
            {
                if (ColorUtility.TryParseHtmlString(hexCol, out Color c))
                    valLabel.style.color = new StyleColor(c);
                else
                    valLabel.style.color = new StyleColor(Color.white);
            }
            row.Add(valLabel);
            return row;
        }

        private Type GetTypeSafe(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            
            if (_typeCache.TryGetValue(typeName, out Type cachedType)) 
                return cachedType;

            Type t = Type.GetType(typeName);
            if (t != null)
            {
                _typeCache[typeName] = t;
                return t;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = assembly.GetType(typeName);
                if (t != null)
                {
                    _typeCache[typeName] = t;
                    return t;
                }
            }

            _typeCache[typeName] = null;
            return null;
        }
    }
}
#endif