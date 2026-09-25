"""Experiment 5 (v2): reflection probe light leaks in a room with a window and an alcove the window cannot see, sampled with rotated Fibonacci lattices and a measured noise floor. Reference: GGX lobe traced from the surface. Compared: the probe from the room centre raw and box projected, each also normalised by local over probe irradiance, and a normalisation that may only darken (clamped at 1)."""
import math
import numpy as np

rng = np.random.default_rng(11)
GOLD = (math.sqrt(5.0) - 1.0) / 2.0
ROOM = np.array([8.0, 3.0, 6.0])
PART_X, PART_Z = 3.0, 2.5
PROBE = np.array([5.0, 1.5, 3.0])


def norm(v):
    return v / np.linalg.norm(v, axis=-1, keepdims=True)


def lattice(count):
    k = np.arange(count)
    return np.mod((k + 0.5) / count + rng.random(), 1.0), np.mod(k * GOLD + rng.random(), 1.0)


def basis(n):
    a = np.array([1.0, 0.0, 0.0]) if abs(n[0]) < 0.9 else np.array([0.0, 1.0, 0.0])
    t = norm(np.cross(n, a))
    return t, np.cross(n, t)


def trace(p, d):
    d = np.where(np.abs(d) < 1e-9, 1e-9, d)
    tp = np.where(d > 0, (ROOM - p) / d, -p / d)
    axis = np.argmin(tp, axis=-1)
    t = np.take_along_axis(tp, axis[..., None], axis=-1)[..., 0]
    hit = p + d * t[..., None]
    sign = np.take_along_axis(d, axis[..., None], axis=-1)[..., 0] > 0
    lum = np.where(axis == 1, np.where(sign, 0.35, 0.15), 0.25)
    window = (axis == 0) & sign & (hit[..., 1] > 0.8) & (hit[..., 1] < 2.4) & (hit[..., 2] > 1.5) & (hit[..., 2] < 4.5)
    lum = np.where(window, 12.0, lum)
    tq = (PART_X - p[..., 0]) / d[..., 0]
    q = p + d * tq[..., None]
    blocked = (tq > 1e-5) & (tq < t) & (q[..., 2] >= 0) & (q[..., 2] <= PART_Z)
    return np.where(blocked, 0.2, lum), hit


def lobe_from(p, v, n, alpha, count=4096):
    u1, u2 = lattice(count)
    phi = 2 * np.pi * u2
    ct = np.sqrt((1 - u1) / (1 + (alpha * alpha - 1) * u1))
    st = np.sqrt(1 - ct * ct)
    t, b = basis(n)
    h = (st * np.cos(phi))[:, None] * t + (st * np.sin(phi))[:, None] * b + ct[:, None] * n
    l = 2 * (h @ v)[:, None] * h - v
    w = np.maximum(l @ n, 0.0)
    lum, _ = trace(np.broadcast_to(p, l.shape), l)
    return float(np.sum(lum * w) / max(w.sum(), 1e-12))


def irradiance(p, n, count=4096):
    u1, u2 = lattice(count)
    r, phi = np.sqrt(u1), 2 * np.pi * u2
    t, b = basis(n)
    d = (r * np.cos(phi))[:, None] * t + (r * np.sin(phi))[:, None] * b + np.sqrt(1 - u1)[:, None] * n
    lum, _ = trace(np.broadcast_to(p, d.shape), d)
    return float(lum.mean())


def box_project(p, r):
    _, hit = trace(p[None, :], r[None, :])
    return norm(hit[0] - PROBE)


n = np.array([0.0, 1.0, 0.0])
e_probe = irradiance(PROBE, n, 16384)
regions = {
    "alcove (window hidden)": lambda: np.array([rng.uniform(0.4, 2.6), 0.001, rng.uniform(0.3, 2.2)]),
    "open room (window seen)": lambda: np.array([rng.uniform(4.0, 7.5), 0.001, rng.uniform(1.0, 5.0)]),
}
keys = ["noise", "raw", "box", "raw+norm", "box+norm", "box+darken"]
print("floor points, views from above, 60 points x 3 views per region, 4096 lattice samples per lobe")
print("each cell: relative RMS error | median relative error; probe floor irradiance %.3f" % e_probe)
print("region                   rough  " + "  ".join("%-15s" % k for k in keys) + " mean ref")
for name, pick in regions.items():
    for pr in (0.2, 0.5, 0.8):
        alpha = pr * pr
        cols = {k: [] for k in keys}
        ref = []
        for _ in range(60):
            p = pick()
            scale = irradiance(p, n) / max(e_probe, 1e-6)
            for _ in range(3):
                while True:
                    v = norm(rng.normal(size=3))
                    if v @ n > 0.3:
                        break
                r = norm(2 * (n @ v) * n - v)
                ref.append(lobe_from(p, v, n, alpha))
                cols["noise"].append(lobe_from(p, v, n, alpha))
                a = lobe_from(PROBE, r, r, alpha)
                rb = box_project(p, r)
                c = lobe_from(PROBE, rb, rb, alpha)
                cols["raw"].append(a)
                cols["box"].append(c)
                cols["raw+norm"].append(a * scale)
                cols["box+norm"].append(c * scale)
                cols["box+darken"].append(c * min(scale, 1.0))
        ref = np.array(ref)

        def err(key):
            e = np.array(cols[key]) - ref
            return "%.3f | %.3f" % (math.sqrt(np.mean(e ** 2)) / ref.mean(), np.median(np.abs(e) / np.maximum(ref, 1e-3)))

        print("%-24s %-5.1f  " % (name, pr) + "  ".join("%-15s" % err(k) for k in keys) + " %.3f" % ref.mean())
