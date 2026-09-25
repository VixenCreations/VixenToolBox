"""Experiment 3, part B: run the shader's output through Camera Pro's own chain (exposure exp2(EV), then AgX, ACES or Khronos PBR Neutral, copied from VFCamColor.cginc) to see what the camera shows of Sharper Probes' contrast and of highlights and neon, in an HDR world and in one that clips at 1 before Camera Pro reads the frame."""
import numpy as np

AGX_IN = np.array([[0.842479062253094, 0.0784335999999992, 0.0792237451477643],
                   [0.0423282422610123, 0.878468636469772, 0.0791661274605434],
                   [0.0423756549057051, 0.0784336, 0.879142973793104]])
AGX_OUT = np.array([[1.19687900512017, -0.0980208811401368, -0.0990297440797205],
                    [-0.0528968517574562, 1.15190312990417, -0.0989611768448433],
                    [-0.0529716355144438, -0.0980434501171241, 1.15107367264116]])
LUMA = np.array([0.2126, 0.7152, 0.0722])


def agx(c):
    c = AGX_IN @ np.maximum(c, 0.0)
    c = np.clip(np.log2(np.maximum(c, 1e-10)), -12.47393, 4.026069)
    x = (c + 12.47393) / (4.026069 + 12.47393)
    x2 = x * x
    x4 = x2 * x2
    y = 15.5 * x4 * x2 - 40.14 * x4 * x + 31.96 * x4 - 6.868 * x2 * x + 0.4298 * x2 + 0.1191 * x - 0.00232
    y = AGX_OUT @ y
    return np.power(np.maximum(y, 0.0), 2.2)


def aces(x):
    x = x * 0.6
    return np.clip((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0, 1)


def neutral(c):
    c = np.array(c, dtype=float)
    x = c.min()
    off = x - 6.25 * x * x if x < 0.08 else 0.04
    c = c - off
    peak = c.max()
    s = 0.8 - 0.04
    if peak < s:
        return c
    d = 1.0 - s
    newpeak = 1.0 - d * d / (peak + d - s)
    c = c * newpeak / max(peak, 1e-6)
    g = 1.0 - 1.0 / (0.15 * (peak - newpeak) + 1.0)
    return c + (newpeak - c) * g


def to_srgb(v):
    v = np.clip(v, 0, 1)
    return np.where(v < 0.0031308, v * 12.92, 1.055 * np.power(v, 1 / 2.4) - 0.055)


def display(lin_grey, curve):
    c = np.array([lin_grey] * 3)
    out = {"AgX": agx, "ACES": aces, "Neutral": neutral}[curve](c)
    return float(to_srgb(LUMA @ out))


print("1. Sharper Probes on camera: display (sRGB 0-1) of probe light v and of the same light 31% brighter")
print("   (the Sharper Probes peak for one light), AgX, the Camera Pro default")
print("   v      EV   Unity    Sharper   difference")
for v in (0.05, 0.2, 0.5, 1.0, 2.0):
    for ev in (-1, 0, 1):
        a = display(v * 2 ** ev, "AgX")
        b = display(1.312 * v * 2 ** ev, "AgX")
        print("   %-6.2f %+d   %.3f    %.3f     %+.3f" % (v, ev, a, b, b - a))

print("\n2. A highlight or neon value, HDR world against a world that clips at 1 before Camera Pro reads it")
print("   value   EV    HDR AgX   clipped AgX   HDR Neutral   clipped Neutral")
for val in (1.0, 2.0, 4.0, 8.0, 16.0, 64.0):
    for ev in (-2, -1, 0):
        k = 2 ** ev
        print("   %-6.0f  %+d    %.3f     %.3f         %.3f         %.3f" %
              (val, ev, display(val * k, "AgX"), display(min(val, 1.0) * k, "AgX"),
               display(val * k, "Neutral"), display(min(val, 1.0) * k, "Neutral")))

print("\n3. Above mid grey, where each curve stops showing a 10% brighter highlight (display step below 1/255)")
for curve in ("AgX", "ACES", "Neutral"):
    flat = None
    for lv in np.geomspace(0.18, 256, 400):
        if display(lv * 1.1, curve) - display(lv, curve) < 1 / 255:
            flat = lv
            break
    print("   %-8s flat above linear %.2f (%.1f stops over mid grey)" % (curve, flat, np.log2(flat / 0.18)))
