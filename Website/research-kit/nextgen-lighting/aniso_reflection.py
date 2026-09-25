"""Experiment 5 (v2): anisotropic reflections of a studio environment (two softboxes and a neon band) through GGX, sampled with rotated Fibonacci lattices and with a measured noise floor (the reference computed twice). Reference: the anisotropic GGX lobe for the real view (Filament roughness at = a(1+k), ab = a(1-k)). Compared: the isotropic probe lookup the shaders use today, Filament getReflectedVector (bent normal), and the bent normal with the lookup roughness raised to the mean of at and ab."""
import math
import numpy as np

rng = np.random.default_rng(3)
GOLD = (math.sqrt(5.0) - 1.0) / 2.0


def norm(v):
    return v / np.linalg.norm(v, axis=-1, keepdims=True)


def lattice(count):
    k = np.arange(count)
    return np.mod((k + 0.5) / count + rng.random(), 1.0), np.mod(k * GOLD + rng.random(), 1.0)


def box(center, up, hx_deg, hy_deg, lum):
    c = norm(np.array(center, float))
    u = norm(np.array(up, float) - np.dot(up, c) * c)
    return c, u, np.cross(c, u), math.radians(hx_deg), math.radians(hy_deg), lum


BOXES = [box((0.5, 0.4, 0.75), (0, 0, 1), 16, 9, 20.0), box((-0.7, 0.2, 0.3), (0, 0, 1), 10, 24, 12.0)]


def env(d):
    lum = np.full(d.shape[:-1], 0.02)
    for c, u, w, hx, hy, l in BOXES:
        cz = d @ c
        x = np.arctan2(d @ u, cz)
        y = np.arctan2(d @ w, cz)
        lum = np.where((cz > 0) & (np.abs(x) < hx) & (np.abs(y) < hy), l, lum)
    band = (np.abs(d[..., 2]) < math.sin(math.radians(2.0))) & (np.arctan2(d[..., 1], d[..., 0]) > 0)
    return np.where(band, 6.0, lum)


def aniso_halfvectors(ax, ay, u1, u2):
    phi = np.arctan2(ay * np.sin(2 * np.pi * u2), ax * np.cos(2 * np.pi * u2))
    cp, sp = np.cos(phi), np.sin(phi)
    tan2 = u1 / np.maximum(1.0 - u1, 1e-9) / (cp * cp / (ax * ax) + sp * sp / (ay * ay))
    ct = 1.0 / np.sqrt(1.0 + tan2)
    st = np.sqrt(np.maximum(0.0, 1.0 - ct * ct))
    return np.stack([st * cp, st * sp, ct], axis=-1)


def lobe(v, t, b, n, ax, ay, count):
    u1, u2 = lattice(count)
    hl = aniso_halfvectors(ax, ay, u1, u2)
    h = hl[:, :1] * t + hl[:, 1:2] * b + hl[:, 2:3] * n
    l = 2.0 * (h @ v)[:, None] * h - v
    w = np.maximum(l @ n, 0.0)
    return float(np.sum(env(l) * w) / max(np.sum(w), 1e-12))


def any_perp(r):
    a = np.array([1.0, 0.0, 0.0]) if abs(r[0]) < 0.9 else np.array([0.0, 1.0, 0.0])
    t = norm(np.cross(r, a))
    return t, np.cross(r, t)


def probe_lookup(r, alpha, count):
    t, b = any_perp(r)
    return lobe(r, t, b, r, alpha, alpha, count)


CONFIGS = 200
COUNT = 4096
print("anisotropic GGX reflections of a studio environment, %d random surfaces and views each, %d lattice samples" % (CONFIGS, COUNT))
print("each cell: relative RMS error (RMS / mean reference) | median relative error")
print("noise = the reference against a second reference with another lattice rotation")
print("tapN = N probe lookups at the real lobe's half-vector median ring, each at roughness max(ab, 0.35 a), weighted by n.l")
print("p.rough k    noise          isotropic today   Filament bent    bent + mean(at,ab)  4 taps          8 taps")
for pr in (0.3, 0.5, 0.7):
    alpha = pr * pr
    for k in (0.0, 0.5, 0.9):
        at = max(alpha * (1 + k), 1e-3)
        ab = max(alpha * (1 - k), 1e-3)
        cols = {"ref": [], "ref2": [], "iso": [], "bent": [], "bent2": [], "tap4": [], "tap8": []}
        for _ in range(CONFIGS):
            n = norm(rng.normal(size=3))
            t = norm(np.cross(n, rng.normal(size=3)))
            b = np.cross(n, t)
            while True:
                v = norm(rng.normal(size=3))
                if v @ n > 0.2:
                    break
            cols["ref"].append(lobe(v, t, b, n, at, ab, COUNT))
            cols["ref2"].append(lobe(v, t, b, n, at, ab, COUNT))
            r = norm(2.0 * (n @ v) * n - v)
            cols["iso"].append(probe_lookup(r, alpha, COUNT))
            direction = b if k >= 0 else t
            a_n = norm(np.cross(np.cross(direction, v), direction))
            bend = abs(k) * min(1.0, max(0.0, 5.0 * pr))
            bn = norm((1 - bend) * n + bend * a_n)
            rb = norm(2.0 * (bn @ v) * bn - v)
            cols["bent"].append(probe_lookup(rb, alpha, COUNT))
            cols["bent2"].append(probe_lookup(rb, 0.5 * (at + ab), COUNT))
            for taps in (4, 8):
                ku = (np.arange(taps) + 0.5) / taps
                hl = aniso_halfvectors(at, ab, np.full(taps, 0.5), ku)
                h = hl[:, :1] * t + hl[:, 1:2] * b + hl[:, 2:3] * n
                ls = 2.0 * (h @ v)[:, None] * h - v
                ws = np.maximum(ls @ n, 0.0)
                vals = [probe_lookup(norm(l), max(ab, 0.35 * alpha), COUNT // 4) for l in ls]
                cols["tap%d" % taps].append(float(np.sum(np.array(vals) * ws) / max(ws.sum(), 1e-9)))
        ref = np.array(cols["ref"])

        def err(key):
            e = np.array(cols[key]) - ref
            return "%.3f | %.3f" % (math.sqrt(np.mean(e ** 2)) / ref.mean(), np.median(np.abs(e) / np.maximum(ref, 1e-3)))

        print("%-7.1f %-4.1f %-14s %-17s %-16s %-19s %-15s %s" % (pr, k, err("ref2"), err("iso"), err("bent"), err("bent2"),
                                                              err("tap4"), err("tap8")))
