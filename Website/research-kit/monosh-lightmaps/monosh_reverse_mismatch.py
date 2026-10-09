"""Reverse mismatch: a material set to read MonoSH lightmaps given a Unity-format directional map (V = 4 D)."""
import numpy as np
from monosh_estimators import SCENES, N, exact, exact_e0, rel_rms
from monosh_unity_reader import unity_dir

if __name__ == "__main__":
    print("error = RMS of luminance over 400 normals within 45 degrees / mean; exact data, 8-bit storage")
    print("%-24s %-14s %-22s" % ("scene", "Unity read", "read as MonoSH (4 D)"))
    for name, scene in SCENES.items():
        ref = exact(scene)
        e0 = exact_e0(scene)
        q = lambda x: np.round(np.clip(x, 0, 1) * 255) / 255
        D = unity_dir(scene)
        Dq = q(D + 0.5) - 0.5
        aq = q(np.array([D[2] + 0.5]))[0]
        unity = e0[None, :] * ((N @ Dq + 0.5) / max(aq, 1e-4))[:, None]
        V = 4 * Dq
        mis = e0[None, :] * (np.maximum(1 + N @ V, 0) / max(1 + V[2], 1e-3))[:, None]
        print("%-24s %-14.4f %-22.4f" % (name, rel_rms(unity, ref), rel_rms(mis, ref)))
