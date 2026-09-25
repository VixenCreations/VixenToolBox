"""Experiment 2, part A: one directional light delivered through each route Unity or a VRChat system can use (per pixel, per vertex, Unity SH light or probe, Light Volume L1, Sharper Probes), measured on a Fibonacci lattice against the per-pixel look, with and without the Soften and Hardness terminator controls."""
import math
import numpy as np

N_PTS = 100000
i = np.arange(N_PTS)
z = 1.0 - (2.0 * i + 1.0) / N_PTS
r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
th = math.pi * (3.0 - math.sqrt(5.0)) * i
P = np.stack([r * np.cos(th), r * np.sin(th), z], axis=1)
C = P[:, 2]


def shaped(c, soften, hard):
    x = np.clip((c + soften) / (1.0 + soften), 0.0, 1.0)
    return x ** (1.0 + (0.15 - 1.0) * hard)


def P2(c):
    return 0.5 * (3.0 * c * c - 1.0)


def sh_l2(c):
    return np.maximum(0.25 + 0.5 * c + 0.3125 * P2(c), 0.0)


def sh_l1(c):
    return np.maximum(0.25 + 0.5 * c, 0.0)


def sh_l1_nonlinear(c):
    L0, rr = 0.25, 1.0
    q = np.clip(0.5 * (1.0 + c), 0.0, 1.0)
    p = 1.0 + 2.0 * rr
    a = (1.0 - rr) / (1.0 + rr)
    return L0 * (a + (1.0 - a) * (p + 1.0) * q ** p)


def sh_l2_nonlinear(c):
    return np.maximum(sh_l1_nonlinear(c) + 0.3125 * P2(c), 0.0)


def stats(ref, got):
    d = got - ref
    rms = math.sqrt(np.mean(d * d))
    face = got[np.argmax(C)]
    back = float(np.interp(math.cos(math.radians(110)), C[::-1], got[::-1]))
    term = float(np.degrees(np.arccos(np.clip(C[got > 0.01 * got.max()].min(), -1, 1))))
    return rms, face, back, term


routes = [
    ("per pixel (the reference)", None),
    ("per vertex (Shade4PointLights)", lambda c: np.maximum(c, 0.0)),
    ("Unity SH light or probe, L2", sh_l2),
    ("Sharper Probes, L2", sh_l2_nonlinear),
    ("Light Volume, L1", sh_l1),
    ("Light Volume point light, L1", sh_l1),
]

for soften, hard in ((0.0, 0.0), (0.5, 0.0), (0.0, 0.5), (1.0, 0.0)):
    ref = shaped(C, soften, hard)
    print("Soften %.1f, Hardness %.1f: facing %.3f, at 110 deg %.3f, lit to %.0f deg" %
          (soften, hard, ref[np.argmax(C)], float(np.interp(math.cos(math.radians(110)), C[::-1], ref[::-1])),
           float(np.degrees(np.arccos(np.clip(C[ref > 0.01 * ref.max()].min(), -1, 1))))))
    print("   %-32s %-8s %-8s %-10s %s" % ("route", "rms", "facing", "at 110 deg", "lit to"))
    for name, fn in routes[1:]:
        rms, face, back, term = stats(ref, fn(C))
        print("   %-32s %-8.3f %-8.3f %-10.3f %.0f deg" % (name, rms, face, back, term))
    print()

print("Energy over the sphere (mean response, per pixel Lambert = %.4f):" % np.mean(np.maximum(C, 0)))
for name, fn in routes[1:]:
    print("   %-32s %.4f" % (name, np.mean(fn(C))))
