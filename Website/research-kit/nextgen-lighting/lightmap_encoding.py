"""Experiment 5: what a lightmap texel should store for normal-mapped surfaces. Reference: irradiance (Unity convention, E/pi) for normals tilted up to 45 degrees, light from below the surface blocked. Compared: non-directional, Unity's directional encoding as the baker writes it (dominant direction times directionality, Unity's half-Lambert rebalance decode), MonoSH (L0 RGB plus L1 of luminance, fits Unity's two lightmap slots) and full L1 SH (RGB, three more textures)."""
import math
import numpy as np

GOLD = (math.sqrt(5.0) - 1.0) / 2.0
W = np.array([0.2126, 0.7152, 0.0722])


def norm(v):
    return v / np.linalg.norm(v, axis=-1, keepdims=True)


def hemi(count):
    k = np.arange(count) + 0.5
    z = 1 - k / count
    phi = 2 * np.pi * np.mod(k * GOLD, 1.0)
    s = np.sqrt(1 - z * z)
    return np.stack([s * np.cos(phi), s * np.sin(phi), z], axis=-1)


def rect(d, az0, az1, el0, el1):
    az = np.degrees(np.arctan2(d[:, 1], d[:, 0]))
    el = np.degrees(np.arcsin(np.clip(d[:, 2], -1, 1)))
    return (az > az0) & (az < az1) & (el > el0) & (el < el1)


def screen_downlight(d):
    L = np.tile(np.array([0.08, 0.07, 0.06]), (len(d), 1))
    L[rect(d, -30, 30, 5, 35)] = np.array([0.2, 1.0, 0.25]) * 3.0
    L[d[:, 2] > math.cos(math.radians(6))] = np.array([1.0, 0.75, 0.45]) * 30.0
    return L


def sun_sky(d):
    sun = norm(np.array([0.6, 0.1, 0.64]))
    L = (0.3 + 0.5 * d[:, 2])[:, None] * np.array([0.55, 0.7, 1.0])
    L[d @ sun > math.cos(math.radians(3.0))] = np.array([1.0, 0.95, 0.85]) * 400.0
    return L


def two_lights(d):
    L = np.tile(np.array([0.03, 0.03, 0.03]), (len(d), 1))
    L[rect(d, -15, 15, 10, 30)] = np.array([1.0, 0.15, 0.1]) * 5.0
    L[rect(d, 165, 180, 10, 30) | rect(d, -180, -165, 10, 30)] = np.array([0.1, 0.25, 1.0]) * 5.0
    return L


def normals(count, max_deg=45.0):
    k = np.arange(count) + 0.5
    c = 1 - (k / count) * (1 - math.cos(math.radians(max_deg)))
    phi = 2 * np.pi * np.mod(k * GOLD, 1.0)
    s = np.sqrt(1 - c * c)
    return np.stack([s * np.cos(phi), s * np.sin(phi), c], axis=-1)


D = hemi(200000)
dw = 2 * np.pi / len(D)
N = normals(400)
n0 = np.array([0.0, 0.0, 1.0])
Y0 = 0.282095
Y1 = 0.488603
print("irradiance (E/pi) for 400 normals within 45 degrees; errors are RMS / mean of luminance | mean chroma shift")
print("MonoSH ratio = the lightmap colour at the surface normal times the L1 luminance ratio (same two textures as MonoSH)")
print("scene              non-directional   Unity directional   MonoSH            MonoSH ratio      L1 SH RGB")
for name, scene in (("screen+downlight", screen_downlight), ("sun+sky", sun_sky), ("two lights", two_lights)):
    L = scene(D)
    ref = np.maximum(N @ D.T, 0.0) @ L * dw / np.pi
    cos0 = D[:, 2]
    e0 = (L * cos0[:, None]).sum(axis=0) * dw / np.pi
    lum = L @ W
    dom = (D * (lum * cos0)[:, None]).sum(axis=0) / max((lum * cos0).sum(), 1e-12)
    dprime = 0.5 * dom
    unity = e0[None, :] * ((N @ dprime + 0.5) / max(dprime @ n0 + 0.5, 1e-4))[:, None]
    c0 = (L * Y0).sum(axis=0) * dw
    c1 = (D[:, :, None] * (L * Y1)[:, None, :]).sum(axis=0) * dw
    irr_l0 = math.pi * c0 * Y0
    l1rgb = np.maximum(irr_l0[None, :] + (2 * math.pi / 3) * Y1 * (N @ c1), 0.0) / math.pi
    c1lum = c1 @ W
    tint = c0 / max(c0 @ W, 1e-12)
    mono = np.maximum((irr_l0 @ W) + (2 * math.pi / 3) * Y1 * (N @ c1lum), 0.0)[:, None] * tint[None, :] / math.pi
    flat = np.tile(e0, (len(N), 1))
    sh_lum = np.maximum((irr_l0 @ W) + (2 * math.pi / 3) * Y1 * (N @ c1lum), 0.0)
    sh_lum0 = max((irr_l0 @ W) + (2 * math.pi / 3) * Y1 * (n0 @ c1lum), 1e-9)
    mono_ratio = flat * (sh_lum / sh_lum0)[:, None]

    def err(est):
        rl, el = ref @ W, est @ W
        chroma = np.abs(est / np.maximum(el, 1e-9)[:, None] - ref / np.maximum(rl, 1e-9)[:, None]).sum(axis=1).mean()
        return "%.3f | %.3f" % (math.sqrt(np.mean((el - rl) ** 2)) / rl.mean(), chroma)

    print("%-18s %-17s %-19s %-17s %-17s %s" % (name, err(flat), err(unity), err(mono), err(mono_ratio), err(l1rgb)))
