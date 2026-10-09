"""What a shader that decodes lightmapDir the Unity way sees when the baker writes MonoSH ratio data as rgb = V/4 + 0.5, a = 0.5 + dot(n0, V/4)."""
import math
import numpy as np
from monosh_estimators import SCENES, N, W, exact, exact_e0, rel_rms, DF, DW


def exact_v(scene):
    f = scene["field"](DF)
    lum = f @ W
    A = lum.sum() * DW
    B = (DF * lum[:, None]).sum(0) * DW
    for w, phi in scene["direct"]:
        A += phi @ W
        B = B + (phi @ W) * w
    return 2 * B / A


def unity_dir(scene):
    f = scene["field"](DF)
    lum = f @ W
    cosw = lum * DF[:, 2]
    acc = (DF * cosw[:, None]).sum(0) * DW
    wsum = cosw.sum() * DW
    for w, phi in scene["direct"]:
        c = (phi @ W) * max(w[2], 0)
        acc = acc + w * c
        wsum += c
    dom = acc / wsum
    return 0.5 * dom / max(np.linalg.norm(dom), 1e-9) * min(np.linalg.norm(dom), 1.0)


if __name__ == "__main__":
    print("error = RMS of luminance over 400 normals within 45 degrees / mean; exact data, 8-bit storage")
    print("%-24s %-10s %-12s %-16s %-18s" % ("scene", "flat", "Unity dir", "MonoSH (ours)", "MonoSH, Unity read"))
    for name, scene in SCENES.items():
        ref = exact(scene)
        e0 = exact_e0(scene)
        V = exact_v(scene)
        q = lambda x: np.round(np.clip(x, 0, 1) * 255) / 255
        rgb = q(V / 4 + 0.5)
        a = q(np.array([0.5 + V[2] / 4]))[0]
        Vq = (rgb - 0.5) * 4
        ours = e0[None, :] * (np.maximum(1 + N @ Vq, 0) / max(1 + Vq[2], 1e-3))[:, None]
        unity_read = e0[None, :] * ((N @ (rgb - 0.5) + 0.5) / max(a, 1e-4))[:, None]
        D = unity_dir(scene)
        Dq = q(D + 0.5) - 0.5
        aq = q(np.array([D[2] + 0.5]))[0]
        unity = e0[None, :] * ((N @ Dq + 0.5) / max(aq, 1e-4))[:, None]
        flat = np.tile(e0, (len(N), 1))
        print("%-24s %-10.4f %-12.4f %-16.4f %-18.4f" % (name, rel_rms(flat, ref), rel_rms(unity, ref), rel_rms(ours, ref), rel_rms(unity_read, ref)))
