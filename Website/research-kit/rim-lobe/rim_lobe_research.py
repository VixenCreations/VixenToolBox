"""Rim lobe research: Fibonacci lattice integration of the Wrap lobe (clamped cosine over a spherical cap), its SH band weights for probes and Light Volumes, backlight capture against the old normal-centred rim, and the non-linear SH checks."""
import math
import numpy as np

N_PTS = 200000


def fib(n):
    i = np.arange(n)
    z = 1.0 - (2.0 * i + 1.0) / n
    r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
    th = math.pi * (3.0 - math.sqrt(5.0)) * i
    return np.stack([r * np.cos(th), r * np.sin(th), z], axis=1)


P = fib(N_PTS)
DW = 4.0 * math.pi / N_PTS


def cap_lit(c, wrap):
    c = np.clip(np.asarray(c, dtype=np.float64), -1.0, 1.0)
    s2 = min(math.sin(min(max(wrap, 0.0), 1.0) * math.pi * 0.5) ** 2, 0.999999)
    out = np.maximum(c, 0.0)
    if s2 < 1e-6:
        return out
    m = c * c < s2
    cc = c[m]
    sinT = np.sqrt(np.maximum(1.0 - cc * cc, 1e-6))
    x = math.sqrt(1.0 / s2 - 1.0)
    y = -x * cc / sinT
    sy = sinT * np.sqrt(np.maximum(0.0, 1.0 - y * y))
    e = (cc * np.arccos(np.clip(y, -1, 1)) - x * sy) * s2 + np.arctan(sy / x)
    out[m] = np.clip(e / (math.pi * s2), 0.0, 1.0)
    return out


def legendre(l, z):
    return [np.ones_like(z), z, 0.5 * (3 * z * z - 1)][l]


def zh(kernel_vals, l):
    return np.sum(kernel_vals * legendre(l, P[:, 2])) * DW


cos_k = np.maximum(P[:, 2], 0.0)
base = [zh(cos_k, l) for l in range(3)]
print("1. Wrap lobe band weights relative to the cosine lobe Unity stores, against the closed form")
print("   wrap   a0 lattice  a0 closed   a1 lattice  a1 closed   a2 lattice  a2 closed")
worst = 0.0
for wrap in (0.0, 0.25, 0.5, 0.75, 1.0):
    sig = wrap * math.pi / 2
    k = cap_lit(P[:, 2], wrap)
    a = [zh(k, l) / base[l] for l in range(3)]
    closed = [2.0 / (1.0 + math.cos(sig)), 1.0, math.cos(sig)]
    worst = max(worst, max(abs(a[i] - closed[i]) for i in range(3)))
    print("   %.2f   %.4f      %.4f      %.4f      %.4f      %.4f      %.4f" % (wrap, a[0], closed[0], a[1], closed[1], a[2], closed[2]))
print("   worst difference: %.4f" % worst)

Y = lambda v: np.stack([np.full(len(v), 0.2820948),
                        0.4886025 * v[:, 1], 0.4886025 * v[:, 2], 0.4886025 * v[:, 0],
                        1.0925484 * v[:, 0] * v[:, 1], 1.0925484 * v[:, 1] * v[:, 2],
                        0.3153916 * (3 * v[:, 2] ** 2 - 1), 1.0925484 * v[:, 0] * v[:, 2],
                        0.5462742 * (v[:, 0] ** 2 - v[:, 1] ** 2)], axis=1)
YP = Y(P)
A_COS = np.array([math.pi] + [2 * math.pi / 3] * 3 + [math.pi / 4] * 5)
BAND = np.array([0, 1, 1, 1, 2, 2, 2, 2, 2])


def rim_true(radiance, d, wrap):
    k = cap_lit(P @ d, wrap)
    return np.sum(radiance * k) * DW / math.pi


def rim_sh(radiance, d, wrap):
    coeff = YP.T @ radiance * DW
    sig = wrap * math.pi / 2
    a = np.array([2 / (1 + math.cos(sig)), 1.0, math.cos(sig)])[BAND]
    yd = Y(d[None, :])[0]
    return max(0.0, float(np.sum(coeff * A_COS / math.pi * a * yd)))


def sun(direction, radius_deg=3.0, power=1.0):
    c = math.cos(math.radians(radius_deg))
    lit = (P @ direction) > c
    return lit * power / (2 * math.pi * (1 - c))


def norm(v):
    v = np.asarray(v, dtype=float)
    return v / np.linalg.norm(v)


V = norm([0, 0, 1])
N = norm([1, 0, 0])
R = 2 * np.dot(N, V) * N - V
d = norm(N + R)
print("\n2. A silhouette pixel: normal to the side, camera in front. Rim direction d = normalize(N + reflect(-V, N)) = %s" % np.round(d, 3))
print("   light position            old rim (cosine at N)   new rim lobe, Wrap 0.5 (exact / from SH)")
for name, ldir in (("behind you", -V), ("beside the edge", N), ("behind and beside", norm(N - V)),
                   ("above", norm([0, 1, 0])), ("at the camera", V)):
    rad = sun(ldir) + 0.02
    old = rim_true(rad, N, 0.0)
    new_t = rim_true(rad, d, 0.5)
    new_s = rim_sh(rad, d, 0.5)
    print("   %-24s  %.3f                   %.3f / %.3f" % (name, old, new_t, new_s))

print("\n3. SH truncation error of the new lobe on 300 random skies with a sun (L2 probe, Wrap 0.5)")
rng = np.random.default_rng(7)
errs = []
for _ in range(300):
    sdir = norm(rng.normal(size=3))
    rad = sun(sdir, radius_deg=rng.uniform(2, 20), power=rng.uniform(0.5, 3)) + rng.uniform(0.05, 0.5) * (0.6 + 0.4 * P[:, 1])
    dd = norm(rng.normal(size=3))
    t = rim_true(rad, dd, 0.5)
    s = rim_sh(rad, dd, 0.5)
    peak = max(rim_true(rad, sdir, 0.5), 1e-6)
    errs.append(abs(t - s) / peak)
print("   mean error %.3f, 95th percentile %.3f, worst %.3f of each sky's peak" % (np.mean(errs), np.percentile(errs, 95), np.max(errs)))


def nonlinear(L0, L1, n):
    if L0 <= 1e-8:
        return np.zeros(len(n))
    ln = np.linalg.norm(L1)
    if ln <= 1e-8:
        return np.full(len(n), L0)
    rr = min(ln / L0, 1.0)
    q = np.clip(0.5 * (1 + n @ (L1 / ln)), 0, 1)
    p = 1 + 2 * rr
    a = (1 - rr) / (1 + rr)
    return L0 * (a + (1 - a) * (p + 1) * q ** p)

print("\n4. Non-linear reconstruction with the Wrap weight on L0: never negative, same energy as the linear lobe")
ok = True
for wrap in (0.0, 0.5, 1.0):
    a0 = 2 / (1 + math.cos(wrap * math.pi / 2))
    for rr in (0.2, 0.6, 1.0, 1.4):
        L0 = 1.0
        L1 = np.array([0, 0, rr])
        nl = nonlinear(a0 * L0, L1, P)
        lin = a0 * L0 + P @ L1
        e = abs(nl.mean() - lin.mean())
        neg = int(np.sum(nl < -1e-9))
        if e > 2e-3 or neg:
            ok = False
    print("   wrap %.1f: checked 4 ratios" % wrap)
print("   PASS" if ok else "   FAIL")
