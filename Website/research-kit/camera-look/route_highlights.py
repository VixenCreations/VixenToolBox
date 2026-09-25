"""Experiment 3, part A: does a light get a highlight on every route? Per pixel and per vertex give GGX; probes give none (Unity: SH is diffuse only). Test a highlight from the probe's own L1 (direction L1/|L1|, strength 2|L1| x d^k) against the true sun on random sun-and-sky probes, and how much of the sky it double counts."""
import math
import numpy as np

N_PTS = 20000
i = np.arange(N_PTS)
z = 1.0 - (2.0 * i + 1.0) / N_PTS
r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
th = math.pi * (3.0 - math.sqrt(5.0)) * i
P = np.stack([r * np.cos(th), r * np.sin(th), z], axis=1)
DW = 4.0 * math.pi / N_PTS
rng = np.random.default_rng(3)


def bands(rad):
    L0 = np.sum(rad) * DW / 4.0
    L1 = (P.T @ rad) * DW / 2.0
    return L0, L1


def power(rad):
    return float(np.sum(rad) * DW)


def unit(v):
    return v / np.linalg.norm(v)


def sun(d, deg, powr):
    c = math.cos(math.radians(deg))
    return ((P @ d) > c) * powr / (2 * math.pi * (1 - c))


d = unit(np.array([0.3, 0.2, 1.0]))
s1 = sun(d, 2.0, 1.0)
L0, L1 = bands(s1)
print("check: one small light, 2|L1| / its power on the lattice = %.4f, direction error %.2f deg" %
      (2 * np.linalg.norm(L1) / power(s1), math.degrees(math.acos(np.clip(np.dot(unit(L1), d), -1, 1)))))

print("\nRandom probes: a sun (1-6 deg, irradiance 0.3-3) plus a sky (0.02-0.4, brighter above)")
print("  k    strength error (median / 90%)   direction error (median / 90%)   fake light from a sky-only probe")
for k in (0.0, 0.5, 1.0, 2.0):
    se, de, skyonly = [], [], []
    for _ in range(300):
        sd = unit(rng.normal(size=3))
        srad = sun(sd, rng.uniform(1, 6), rng.uniform(0.3, 3.0))
        E = power(srad)
        skyL = rng.uniform(0.02, 0.4)
        sky = skyL * (0.6 + 0.4 * P[:, 1])
        rad = srad + sky
        L0, L1 = bands(rad)
        dd = min(np.linalg.norm(L1) / (2 * L0), 1.0)
        est = 2 * np.linalg.norm(L1) * dd ** k
        se.append(abs(est - E) / E)
        de.append(math.degrees(math.acos(np.clip(np.dot(unit(L1), sd), -1, 1))))
        s0, s1 = bands(sky)
        ds = min(np.linalg.norm(s1) / (2 * s0), 1.0)
        skyonly.append(2 * np.linalg.norm(s1) * ds ** k / power(sky))
    print("  %.1f  %.3f / %.3f                     %.1f / %.1f deg                    %.3f of the sky power" %
          (k, np.median(se), np.percentile(se, 90), np.median(de), np.percentile(de, 90), np.median(skyonly)))
