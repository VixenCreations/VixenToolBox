"""Experiment 2, part C: rebuild the missing L2 band of L1-only data (Light Volumes, their point lights, proxy volumes) from the L1 band itself, 0.625 * |L1| * P2(n . L1dir) per channel, and measure it against the true shaped response on one-sun, two-light and many-light scenes."""
import math
import numpy as np

N_PTS = 16000
i = np.arange(N_PTS)
z = 1.0 - (2.0 * i + 1.0) / N_PTS
r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
th = math.pi * (3.0 - math.sqrt(5.0)) * i
P = np.stack([r * np.cos(th), r * np.sin(th), z], axis=1)
DW = 4.0 * math.pi / N_PTS


def expo(h):
    return 1.0 + (0.15 - 1.0) * h


def kernel(c, s, h):
    return np.clip((c + s) / (1.0 + s), 0.0, 1.0) ** expo(h)


def weights(s, h):
    p = expo(h)
    k0 = 2 * math.pi * (1 + s) / (p + 1)
    k1 = 2 * math.pi * (1 + s) * ((1 + s) / (p + 2) - s / (p + 1))
    c2 = (1 + s) ** 2 / (p + 3) - 2 * s * (1 + s) / (p + 2) + s * s / (p + 1)
    k2 = 2 * math.pi * (1 + s) * 0.5 * (3 * c2 - 1 / (p + 1))
    return np.array([k0, k1, k2]) / np.array([math.pi, 2 * math.pi / 3, math.pi / 4])


def bands(rad):
    L0 = np.sum(rad) * DW / (4 * math.pi)
    L1 = 3.0 / (4 * math.pi) * (P.T @ rad) * DW * (2.0 / 3.0)
    return L0, L1


def P2(c):
    return 0.5 * (3 * c * c - 1)


def recon(L0, L1, n, a, mode):
    base = a[0] * L0 + a[1] * (n @ L1)
    if mode == "l1":
        return np.maximum(base, 0.0)
    ln = np.linalg.norm(L1)
    if mode == "l1+l2rec" and ln > 1e-8:
        base = base + a[2] * 0.625 * ln * P2(n @ (L1 / ln))
    return np.maximum(base, 0.0)


def truth(rad, n, s, h):
    out = np.empty(len(n))
    for j in range(0, len(n), 500):
        cc = n[j:j + 500] @ P.T
        out[j:j + 500] = (kernel(cc, s, h) * rad[None, :]).sum(axis=1) * DW / math.pi
    return out


def sun(d, deg, powr):
    c = math.cos(math.radians(deg))
    return ((P @ d) > c) * powr / (2 * math.pi * (1 - c))


rng = np.random.default_rng(5)
NS = rng.normal(size=(500, 3))
NS /= np.linalg.norm(NS, axis=1, keepdims=True)


def rnd_dir():
    v = rng.normal(size=3)
    return v / np.linalg.norm(v)


def scene(kind):
    sky = rng.uniform(0.02, 0.3) * (0.6 + 0.4 * P[:, 1])
    if kind == "one sun":
        return sky + sun(rnd_dir(), rng.uniform(1, 8), rng.uniform(0.5, 3))
    if kind == "two lights":
        return sky + sun(rnd_dir(), rng.uniform(1, 8), rng.uniform(0.5, 3)) + sun(rnd_dir(), rng.uniform(1, 8), rng.uniform(0.2, 2))
    return sky + sum(sun(rnd_dir(), rng.uniform(2, 15), rng.uniform(0.1, 1.0)) for _ in range(6))


if __name__ == "__main__":
    print("rms error as a share of each scene's peak, mean of 25 scenes; L1 data only (a Light Volume)")
    cases = [(0.0, 0.0), (0.5, 0.0), (1.0, 0.0), (0.0, 0.5), (0.5, 0.5)]
    for kind in ("one sun", "two lights", "six lights"):
        print("  %s" % kind)
        print("    %-24s" % "reconstruction" + "".join("  s%.1f h%.1f" % c for c in cases))
        rows = {"L1 linear + weights": [], "L1 + rebuilt L2 + weights": []}
        for (s, h) in cases:
            a = weights(s, h)
            e1, e2 = [], []
            for _ in range(25):
                rad = scene(kind)
                L0, L1 = bands(rad)
                t = truth(rad, NS, s, h)
                pk = max(t.max(), 1e-6)
                e1.append(math.sqrt(np.mean((recon(L0, L1, NS, a, "l1") - t) ** 2)) / pk)
                e2.append(math.sqrt(np.mean((recon(L0, L1, NS, a, "l1+l2rec") - t) ** 2)) / pk)
            rows["L1 linear + weights"].append(np.mean(e1))
            rows["L1 + rebuilt L2 + weights"].append(np.mean(e2))
        for k, v in rows.items():
            print("    %-24s" % k + "".join("   %7.3f " % x for x in v))

    L0, L1 = bands(sun(np.array([0, 0, 1.0]), 3.0, 1.0))
    print("\none 3-degree light, L1 magnitude over L0: %.3f (2.0 for a point light); rebuilt L2 coefficient 0.625*|L1| = %.4f"
          % (np.linalg.norm(L1) / L0, 0.625 * np.linalg.norm(L1)))
