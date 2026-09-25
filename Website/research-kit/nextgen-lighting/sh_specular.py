"""Experiment 5: specular from light probe SH for rough surfaces, for a world without a useful reflection probe (only the skybox indoors). Reference: GGX prefiltered radiance (n = v = r) of three environments. Compared: L2 SH convolved with the GGX lobe's own zonal harmonic weights (exact band-limited convolution on top of the SH evaluation the shaders already do), and L1 only."""
import math
import numpy as np

rng = np.random.default_rng(5)
GOLD = (math.sqrt(5.0) - 1.0) / 2.0


def norm(v):
    return v / np.linalg.norm(v, axis=-1, keepdims=True)


def fib_sphere(count):
    k = np.arange(count) + 0.5
    z = 1 - 2 * k / count
    phi = 2 * np.pi * np.mod(k * GOLD, 1.0)
    s = np.sqrt(1 - z * z)
    return np.stack([s * np.cos(phi), s * np.sin(phi), z], axis=-1)


def sh9(d):
    x, y, z = d[..., 0], d[..., 1], d[..., 2]
    return np.stack([np.full_like(x, 0.282095), 0.488603 * y, 0.488603 * z, 0.488603 * x,
                     1.092548 * x * y, 1.092548 * y * z, 0.315392 * (3 * z * z - 1),
                     1.092548 * x * z, 0.546274 * (x * x - y * y)], axis=-1)


def room(d):
    az = np.degrees(np.arctan2(d[..., 1], d[..., 0]))
    el = np.degrees(np.arcsin(np.clip(d[..., 2], -1, 1)))
    lum = np.where(d[..., 2] > 0.95, 0.35, np.where(d[..., 2] < -0.3, 0.15, 0.25))
    return np.where((np.abs(az) < 25) & (el > 0) & (el < 30), 12.0, lum)


def sun_sky(d):
    sun = norm(np.array([0.5, 0.2, 0.6]))
    sky = np.where(d[..., 2] > 0, 0.4 + 0.6 * d[..., 2], 0.1)
    return np.where(d @ sun > math.cos(math.radians(2.0)), 400.0, sky)


def studio(d):
    c1 = norm(np.array([0.5, 0.4, 0.75]))
    c2 = norm(np.array([-0.7, 0.2, 0.3]))
    lum = np.full(d.shape[:-1], 0.02)
    lum = np.where(d @ c1 > math.cos(math.radians(14)), 20.0, lum)
    return np.where(d @ c2 > math.cos(math.radians(15)), 12.0, lum)


def lattice(count):
    k = np.arange(count)
    return np.mod((k + 0.5) / count + rng.random(), 1.0), np.mod(k * GOLD + rng.random(), 1.0)


def lobe_dirs(r, alpha, count):
    u1, u2 = lattice(count)
    phi = 2 * np.pi * u2
    ct = np.sqrt((1 - u1) / (1 + (alpha * alpha - 1) * u1))
    st = np.sqrt(1 - ct * ct)
    a = np.array([1.0, 0.0, 0.0]) if abs(r[0]) < 0.9 else np.array([0.0, 1.0, 0.0])
    t = norm(np.cross(r, a))
    b = np.cross(r, t)
    h = (st * np.cos(phi))[:, None] * t + (st * np.sin(phi))[:, None] * b + ct[:, None] * r
    l = 2 * (h @ r)[:, None] * h - r
    return l, np.maximum(l @ r, 0.0)


def zh_weights(alpha):
    l, w = lobe_dirs(np.array([0.0, 0.0, 1.0]), alpha, 65536)
    x = l[:, 2]
    p = [np.ones_like(x), x, 0.5 * (3 * x * x - 1)]
    return [float(np.sum(pi * w) / np.sum(w)) for pi in p]


dirs = fib_sphere(400000)
receivers = fib_sphere(300)
print("GGX prefiltered radiance against SH-convolved specular; 300 reflection directions; RMS / mean | median relative error")
print("env       p.rough  L2 zonal convolution   L1 only          (band weights k1, k2)")
for name, env in (("room", room), ("sun+sky", sun_sky), ("studio", studio)):
    coef = (4 * np.pi / len(dirs)) * (env(dirs)[:, None] * sh9(dirs)).sum(axis=0)
    for pr in (0.4, 0.6, 0.8, 1.0):
        alpha = pr * pr
        k = zh_weights(alpha)
        ref, l2, l1 = [], [], []
        for r in receivers:
            ls, w = lobe_dirs(r, alpha, 4096)
            ref.append(float(np.sum(env(ls) * w) / np.sum(w)))
            y = sh9(r)
            l2.append(float(k[0] * coef[0] * y[0] + k[1] * (coef[1:4] @ y[1:4]) + k[2] * (coef[4:9] @ y[4:9])))
            l1.append(float(k[0] * coef[0] * y[0] + k[1] * (coef[1:4] @ y[1:4])))
        ref = np.array(ref)

        def err(a):
            e = np.array(a) - ref
            return "%.3f | %.3f" % (math.sqrt(np.mean(e ** 2)) / ref.mean(), np.median(np.abs(e) / np.maximum(ref, 1e-3)))

        print("%-9s %-7.1f  %-21s  %-15s  (%.3f, %.3f)" % (name, pr, err(l2), err(l1), k[1], k[2]))
