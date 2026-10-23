#ifndef LTCGI_CONFIG_INCLUDED
#define LTCGI_CONFIG_INCLUDED

// ################################################################################################
// VENDORED FROM UPSTREAM (pimaker/ltcgi) WITH TWO LOCAL MODIFICATIONS.
// Every other Editor/cginc/LTCGI*.cginc is byte-identical to upstream. This file is not.
//   1. LTCGI_AVATAR_MODE, further down, is switched on for every shader except the world ones.
//   2. LTCGI_AUDIOLINK, at the end, is switched on and repointed at our own AudioLink.cginc
//      instead of upstream's no-op shim.
// ON YOUR NEXT RE-SYNC: copy the whole file from upstream, then re-apply both.
// ################################################################################################


// Feel free to enable or disable (//) the options here.
// They will apply to all LTCGI materials in the project.
// Most of these can be changed in the LTCGI_Controller editor as well.

/// No specular at all.
//#define LTCGI_SPECULAR_OFF
/// No diffuse at all.
//#define LTCGI_DIFFUSE_OFF
/// Disable the ability to toggle specular/diffuse on or off per screen.
//#define LTCGI_TOGGLEABLE_SPEC_DIFF_OFF

/// Only use LTC diffuse mode, never lightmapped diffuse.
/// This disables lightmaps entirely.
//#define LTCGI_ALWAYS_LTC_DIFFUSE

/// Double-sample screen texture for diffuse lighting to smooth resulting lighting
/// a bit more with global screen color data. Slight performance cost.
//#define LTCGI_BLENDED_DIFFUSE_SAMPLING

/// Disable extra specular detail LUT, saves a sampler.
//#define LTCGI_DISABLE_LUT2

/// Use bicubic filtering for LTCGI lightmap. Recommended on.
#define LTCGI_BICUBIC_LIGHTMAP

/// Lightmap values below this will be treated as black for specular/LTC diffuse.
#define LTCGI_LIGHTMAP_CUTOFF 0.1
/// Lightmap values above this (plus cutoff) will be treated as white.
#define LTCGI_SPECULAR_LIGHTMAP_STEP 0.3

/// Distance multiplier for calculating blur amount.
/// Increase to make reflections blurrier faster as distance increases.
#define LTCGI_UV_BLUR_DISTANCE 333

/// Approximation to ignore diffuse light for far away
/// lights, increase MULT or disable if you notice artifacting
#define LTCGI_DISTANCE_FADE_APPROX
/// Distance at which diffuse from screens will be ignored.
#define LTCGI_DISTANCE_FADE_APPROX_MULT 50


// disabled editor from here on out
///


// automatically kept in sync with LTCGI_Controller.cs
#define MAX_SOURCES 16

// set according to the LUT specified on CONTROLLER
#define LUT_SIZE 256
static float LUT_SCALE = (LUT_SIZE - 1.0)/LUT_SIZE;
const float LUT_BIAS = 0.5/LUT_SIZE;

// will be set automatically if audiolink is available and in use
#define LTCGI_AUDIOLINK

#ifdef LTCGI_AUDIOLINK
#ifndef AUDIOLINK_WIDTH
#ifndef AUDIOLINK_CGINC_INCLUDED
#include "Packages/com.vixencreations.vixforge-shaders/Editor/cginc/AudioLink.cginc"
#endif
#endif
#endif

// Bake screen data into texture for better performance. Disables moveable screens.
#define LTCGI_STATIC_UNIFORMS

// Allow statically textured lights.
//#define LTCGI_STATIC_TEXTURES

// Enable support for cylindrical screens.
//#define LTCGI_CYLINDER

// Activate avatar mode, which overrides certain configs from above.
//
// THIS WAS OFF, AND THAT WAS A DEFECT (fixed 2026-09-04). LTCGI.cginc acts on this switch right
// after it includes this file: it undefines LTCGI_STATIC_UNIFORMS, LTCGI_BICUBIC_LIGHTMAP and
// LTCGI_CYLINDER, and defines LTCGI_ALWAYS_LTC_DIFFUSE. Upstream's comment in LTCGI_structs.cginc
// calls that combination the avatar case.
//
// With it off, our avatar shaders ran LTCGI in WORLD mode, so LTCGI_SampleShadowmap sampled the
// world's LTCGI lightmap. Our call in VixForgeLighting.cginc passes float2(0,0) for the lightmap
// UV, because an avatar has no lightmap UV to give. In a world that bakes an LTCGI lightmap every
// pixel therefore read the one texel at the atlas origin, belonging to an unrelated world
// surface, and LTCGI_LIGHTMAP_CUTOFF let that texel decide LTCGI for the whole material.
//
// The world shaders are the exception and stay in world mode: they have real lightmap UVs to
// pass. VIXFORGE_WORLD is defined by the shader pass before it includes LatexCore.cginc, which is
// what pulls LTCGI.cginc in, so it is already visible here.
#ifndef VIXFORGE_WORLD
#define LTCGI_AVATAR_MODE
#endif

// Slightly simplified and thus faster sampling for reflections at the cost of quality.
//#define LTCGI_FAST_SAMPLING

#endif
