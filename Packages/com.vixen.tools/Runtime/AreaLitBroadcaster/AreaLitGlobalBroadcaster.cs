#if VW_UDONSHARP_READY
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if UDONSHARP
using static VRC.SDKBase.VRCShader;
#else
using static UnityEngine.Shader;
using UnityEngine.Rendering;
#endif

public class AreaLitGlobalBroadcaster : UdonSharpBehaviour
{
    [Header("AreaLit -> VixenWear avatar GI bridge")]
    [Space(4f)]
    [Tooltip("The AreaLit LightCam's LightMesh RenderTexture (the same RT assigned to AreaLit/Standard materials' Mesh slot).")]
    public RenderTexture LightMesh;
    [Tooltip("The area-light / video RenderTexture (AreaLit 'Texture 0').")]
    public RenderTexture LightTexture0;

    int _Udon_AreaLit_LightMesh;
    int _Udon_AreaLit_Tex0;
    int _Udon_AreaLit_Enable;

    void Start()
    {
        _Udon_AreaLit_LightMesh = PropertyToID("_Udon_AreaLit_LightMesh");
        _Udon_AreaLit_Tex0      = PropertyToID("_Udon_AreaLit_Tex0");
        _Udon_AreaLit_Enable    = PropertyToID("_Udon_AreaLit_Enable");

#if UDONSHARP
        VRCShader.SetGlobalTexture(_Udon_AreaLit_LightMesh, LightMesh);
        VRCShader.SetGlobalTexture(_Udon_AreaLit_Tex0, LightTexture0);
        VRCShader.SetGlobalFloat(_Udon_AreaLit_Enable, LightMesh != null ? 1f : 0f);
#else
        Shader.SetGlobalTexture(_Udon_AreaLit_LightMesh, LightMesh, Rendering.RenderTextureSubElement.Default);
        Shader.SetGlobalTexture(_Udon_AreaLit_Tex0, LightTexture0, Rendering.RenderTextureSubElement.Default);
        Shader.SetGlobalFloat(_Udon_AreaLit_Enable, LightMesh != null ? 1f : 0f);
#endif
    }
}
#endif
