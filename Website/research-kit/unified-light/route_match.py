"""Experiment 2, part B: closed-form SH band weights of the Soften/Hardness shaping kernel (checked on a Fibonacci lattice), and how close each SH route gets to the per-pixel look with them, for single lights and for random skies with a sun."""
import math
import numpy as np

N_PTS = 60000
i = np.arange(N_PTS)
z = 1.0 - (2.0 * i + 1.0) / N_PTS
r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
th = math.pi * (3.0 - math.sqrt(5.0)) * i
P = np.stack([r * np.cos(th), r * np.sin(th), z], axis=1)
DW = 4.0 * math.pi / N_PTS


def expo(hard):
    return 1.0 + (0.15 - 1.0) * hard


def kernel(c, s, h):
    return np.clip((c + s) / (1.0 + s), 0.0, 1.0) ** expo(h)


def k_closed(s, h):
    p = expo(h)
    k0 = 2 * math.pi * (1 + s) / (p + 1)
    k1 = 2 * math.pi * (1 + s) * ((1 + s) / (p + 2) - s / (p + 1))
    c2 = (1 + s) ** 2 / (p + 3) - 2 * s * (1 + s) / (p + 2) + s * s / (p + 1)
    k2 = 2 * math.pi * (1 + s) * 0.5 * (3 * c2 - 1 / (p + 1))
    return np.array([k0, k1, k2])


K_COS = np.array([math.pi, 2 * math.pi / 3, math.pi / 4])


def weights(s, h):
    return k_closed(s, h) / K_COS


def leg(l, c):
    return [np.ones_like(c), c, 0.5 * (3 * c * c - 1)][l]


print("1. Band weights a0 a1 a2 of the shaping kernel, closed form against the lattice")
worst = 0.0
for s in (0.0, 0.25, 0.5, 1.0):
    for h in (0.0, 0.5, 1.0):
        kl = np.array([np.sum(kernel(P[:, 2], s, h) * leg(l, P[:, 2])) * DW for l in range(3)])
        a_lat = kl / K_COS
        a_cf = weights(s, h)
        worst = max(worst, float(np.max(np.abs(a_lat - a_cf))))
        print("   Soften %.2f Hardness %.1f: closed %s  lattice %s" % (s, h, np.round(a_cf, 4), np.round(a_lat, 4)))
print("   worst difference: %.5f" % worst)

Y = lambda v: np.stack([np.full(len(v), 0.2820948), 0.4886025 * v[:, 1], 0.4886025 * v[:, 2], 0.4886025 * v[:, 0],
                        1.0925484 * v[:, 0] * v[:, 1], 1.0925484 * v[:, 1] * v[:, 2], 0.3153916 * (3 * v[:, 2] ** 2 - 1),
                        1.0925484 * v[:, 0] * v[:, 2], 0.5462742 * (v[:, 0] ** 2 - v[:, 1] ** 2)], axis=1)
YP = Y(P)
BAND = np.array([0, 1, 1, 1, 2, 2, 2, 2, 2])


def unity_bands(rad):
    coeff = YP.T @ rad * DW
    L = coeff * np.array([math.pi, *(2 * math.pi / 3,) * 3, *(math.pi / 4,) * 5]) / math.pi
    L0 = L[0] * 0.2820948
    L1 = np.array([L[3], L[1], L[2]]) * 0.4886025
    return L0, L1, L


def eval_linear(L, a, n, order):
    y = Y(n)
    w = np.array([a[0]] + [a[1]] * 3 + [a[2]] * 5)
    if order == 1:
        w[4:] = 0
    return np.maximum((y * (L * w)).sum(axis=1), 0.0)


def eval_nonlinear(L0, L1, n, a, l2=None, a2w=0.0):
    L0 = L0 * a[0]
    L1 = L1 * a[1]
    ln = np.linalg.norm(L1)
    if L0 <= 1e-8:
        return np.zeros(len(n))
    if ln <= 1e-8:
        out = np.full(len(n), L0)
    else:
        rr = min(ln / L0, 1.0)
        q = np.clip(0.5 * (1 + n @ (L1 / ln)), 0, 1)
        p = 1 + 2 * rr
        aa = (1 - rr) / (1 + rr)
        out = L0 * (aa + (1 - aa) * (p + 1) * q ** p)
    if l2 is not None and a2w != 0.0:
        y = Y(n)
        out = out + a2w * (y[:, 4:] * l2[4:]).sum(axis=1)
    return np.maximum(out, 0.0)


def truth(rad, n, s, h):
    out = np.empty(len(n))
    for j in range(0, len(n), 400):
        cc = n[j:j + 400] @ P.T
        out[j:j + 400] = (kernel(cc, s, h) * rad[None, :]).sum(axis=1) * DW / math.pi
    return out


rng = np.random.default_rng(11)
NS = rng.normal(size=(1500, 3))
NS /= np.linalg.norm(NS, axis=1, keepdims=True)


def sun_sky(sdir, sun_deg, sun_pow, sky):
    c = math.cos(math.radians(sun_deg))
    lit = (P @ sdir) > c
    return lit * sun_pow / (2 * math.pi * (1 - c)) + sky * (0.6 + 0.4 * P[:, 1])


print("\n2. Route error against the shaped per-pixel look (rms as a share of the peak), 60 random scenes each")
print("   scene type: sun of 1-6 degrees with sky 0.02-0.3 (L2 probe and L1 volume)")
cases = [(0.0, 0.0), (0.5, 0.0), (1.0, 0.0), (0.0, 0.5), (0.5, 0.5)]
methods = [
    ("probe L2 linear, today", lambda L0, L1, L, s, h: eval_linear(L, np.ones(3), NS, 2)),
    ("probe Sharper Probes, today", lambda L0, L1, L, s, h: eval_nonlinear(L0, L1, NS, np.ones(2), L, 1.0)),
    ("probe L2 linear + weights", lambda L0, L1, L, s, h: eval_linear(L, weights(s, h), NS, 2)),
    ("probe non-linear L1 + weights", lambda L0, L1, L, s, h: eval_nonlinear(L0, L1, NS, weights(s, h), None, 0.0)),
    ("probe non-linear + weights + a2/2 L2", lambda L0, L1, L, s, h: eval_nonlinear(L0, L1, NS, weights(s, h), L, 0.5 * weights(s, h)[2])),
    ("volume L1 linear, today", lambda L0, L1, L, s, h: eval_linear(L, np.ones(3), NS, 1)),
    ("volume L1 linear + weights", lambda L0, L1, L, s, h: eval_linear(L, weights(s, h), NS, 1)),
    ("volume non-linear L1 + weights", lambda L0, L1, L, s, h: eval_nonlinear(L0, L1, NS, weights(s, h), None, 0.0)),
]
res = {m[0]: [] for m in methods}
for (s, h) in cases:
    errs = {m[0]: [] for m in methods}
    for k in range(60):
        sd = rng.normal(size=3); sd /= np.linalg.norm(sd)
        rad = sun_sky(sd, rng.uniform(1, 6), rng.uniform(0.5, 3.0), rng.uniform(0.02, 0.3))
        L0, L1, L = unity_bands(rad)
        t = truth(rad, NS, s, h)
        pk = max(t.max(), 1e-6)
        for name, fn in methods:
            g = fn(L0, L1, L, s, h)
            errs[name].append(math.sqrt(np.mean((g - t) ** 2)) / pk)
    for name in errs:
        res[name].append(np.mean(errs[name]))
print("   %-40s" % "method" + "".join("  s%.1f h%.1f" % c for c in cases))
for name, _ in methods:
    print("   %-40s" % name + "".join("   %7.3f " % v for v in res[name]))
