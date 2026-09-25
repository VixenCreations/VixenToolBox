"""Baker HD experiment: the baker's white-noise sampling against per-texel rotated low-discrepancy sets (Fibonacci lattice for the hemisphere, R4 Kronecker points for sub-texel jitter and the light disc), the square-versus-disc shadow sample shape, point-sampled texels against jittered ones, and the emissive direct estimator's units against a path-traced reference."""
import math
import numpy as np

rng = np.random.default_rng(7)
M32 = 1 << 32


def r_alphas(d):
    x = 2.0
    for _ in range(500):
        x = (1.0 + x) ** (1.0 / (d + 1))
    return [((1.0 / x) ** (i + 1)) % 1.0 for i in range(d)], x


A4, PHI4 = r_alphas(4)
GOLD = (math.sqrt(5.0) - 1.0) / 2.0
FIX_GOLD = int(round(GOLD * M32)) % M32
FIX_R4 = [int(round(a * M32)) % M32 for a in A4]


def fixed(k, c, off):
    v = (k.astype(np.uint64) * np.uint64(c) + off.astype(np.uint64)) % np.uint64(M32)
    return (v >> np.uint64(8)).astype(np.float64) / 16777216.0


def offsets(n):
    return rng.integers(0, M32, size=(n, 1), dtype=np.uint64)


def fib(n_tex, n):
    k = np.arange(n, dtype=np.uint64)[None, :]
    o1 = rng.random((n_tex, 1))
    u1 = np.mod((k.astype(np.float64) + 0.5) / n + o1, 1.0)
    u2 = fixed(k, FIX_GOLD, offsets(n_tex))
    return u1, u2


def r4(n_tex, n, dims):
    k = np.arange(n, dtype=np.uint64)[None, :]
    return [fixed(k, FIX_R4[d], offsets(n_tex)) for d in dims]


def white(n_tex, n, count):
    return [rng.random((n_tex, n)) for _ in range(count)]


def report(name, est, ref):
    err = est - ref
    return "%-34s rms %.5f   mean %+.5f" % (name, math.sqrt(np.mean(err ** 2)), np.mean(err))


print("constants")
print("  R4 root phi_4 = %.16f (x^5 = x + 1)" % PHI4)
print("  golden 1/phi fixed 0.32: %du" % FIX_GOLD)
for i, (a, f) in enumerate(zip(A4, FIX_R4)):
    print("  R4 alpha%d = %.16f  fixed %du" % (i + 1, a, f))

CONFIGS = 40
ROTS = 100
walls = [(rng.random() * 2 * math.pi, 0.3 + rng.random() * 2.0, rng.random() * 1.2) for _ in range(CONFIGS)]


def sky_wall(u1, u2, cfg):
    phi0, width, top = cfg
    cz = np.sqrt(np.maximum(0.0, 1.0 - u1))
    elev = np.arcsin(np.clip(cz, 0.0, 1.0))
    phi = 2.0 * np.pi * u2
    rel = np.mod(phi - phi0, 2.0 * np.pi)
    blocked = (rel < width) & (elev < top)
    y = np.sqrt(u1) * np.sin(phi)
    sky = 0.35 + 0.65 * np.clip(0.5 + 0.5 * y, 0.0, 1.0)
    return np.where(blocked, 0.2, sky)


def ref_sky_wall(cfg, g=1500):
    u = (np.arange(g) + 0.5) / g
    return float(np.mean(sky_wall(u[:, None], u[None, :], cfg)))


refs = np.array([ref_sky_wall(c) for c in walls])

print("\nA. irradiance under a sky with a wall (cosine-weighted, %d scenes x %d texels)" % (CONFIGS, ROTS))
print("   reference is a 1500 x 1500 midpoint grid per scene")
for n in (16, 64, 256, 1024):
    ests_w, ests_f = [], []
    for c, cfg in enumerate(walls):
        u1, u2 = white(ROTS, n, 2)
        ests_w.append(np.mean(sky_wall(u1, u2, cfg), axis=1))
        f1, f2 = fib(ROTS, n)
        ests_f.append(np.mean(sky_wall(f1, f2, cfg), axis=1))
    ref = np.repeat(refs, ROTS)
    ew = np.concatenate(ests_w)
    ef = np.concatenate(ests_f)
    rw = math.sqrt(np.mean((ew - ref) ** 2))
    rf = math.sqrt(np.mean((ef - ref) ** 2))
    print("  N=%-5d white rms %.5f   Fibonacci rms %.5f   ratio %.1fx   mean err white %+.5f fib %+.5f" %
          (n, rw, rf, rw / rf, np.mean(ew - ref), np.mean(ef - ref)))


def disc_true(d):
    d = np.clip(d, -1.0, 1.0)
    seg = np.arccos(d) - d * np.sqrt(1.0 - d * d)
    return 1.0 - seg / np.pi


print("\nB. soft shadow: visible share of a round light behind a straight edge, 16 samples, 20000 texels")
T = 20000
d = rng.uniform(-1.0, 1.0, size=(T, 1))
ang = rng.uniform(0.0, 2 * np.pi, size=(T, 1))
truth = disc_true(d[:, 0])


def edge_vis(x, y):
    xr = x * np.cos(ang) + y * np.sin(ang)
    return np.mean(xr < d, axis=1)


sx, sy = white(T, 16, 2)
print("  " + report("square, white (ShadowMask today)", edge_vis(sx * 2 - 1, sy * 2 - 1), truth))
w1, w2 = white(T, 16, 2)
r = np.sqrt(w1)
print("  " + report("disc, white", edge_vis(r * np.cos(2 * np.pi * w2), r * np.sin(2 * np.pi * w2)), truth))
q1, q2 = r4(T, 16, (2, 3))
r = np.sqrt(q1)
print("  " + report("disc, R4 dims 3-4 rotated", edge_vis(r * np.cos(2 * np.pi * q2), r * np.sin(2 * np.pi * q2)), truth))


def coverage_true(nx, ny, off, g=400):
    u = (np.arange(g) + 0.5) / g - 0.5
    X, Y = np.meshgrid(u, u)
    return np.array([np.mean(X * a + Y * b < o) for a, b, o in zip(nx, ny, off)])


print("\nC. shadow edge through a texel: lit share of the texel, 4000 texels")
T = 4000
th = rng.uniform(0, 2 * np.pi, T)
nx, ny = np.cos(th), np.sin(th)
off = rng.uniform(-0.6, 0.6, T)
truth = coverage_true(nx, ny, off)
centre = (0.0 < off).astype(np.float64)
print("  " + report("texel centre only (today)", centre, truth))
for n in (16, 64, 256):
    jx, jy = white(T, n, 2)
    ew = np.mean((jx - 0.5) * nx[:, None] + (jy - 0.5) * ny[:, None] < off[:, None], axis=1)
    qx, qy = r4(T, n, (0, 1))
    eq = np.mean((qx - 0.5) * nx[:, None] + (qy - 0.5) * ny[:, None] < off[:, None], axis=1)
    print("  N=%-4d %s | %s" % (n, report("jitter white", ew, truth), report("jitter R4 dims 1-2", eq, truth)))

print("\nD. jitter and direction sharing one sample index (texel edge x wall), 64 samples")
T = 4000
cfg_i = rng.integers(0, CONFIGS, T)
th = rng.uniform(0, 2 * np.pi, T)
nx, ny = np.cos(th), np.sin(th)
off = rng.uniform(-0.6, 0.6, T)
cov = coverage_true(nx, ny, off)
truth = cov * refs[cfg_i]
n = 64
jx, jy, u1, u2 = white(T, n, 4)
f_w = np.zeros(T)
f_l = np.zeros(T)
qx, qy = r4(T, n, (0, 1))
f1, f2 = fib(T, n)
for i in range(T):
    cfg = walls[cfg_i[i]]
    lw = (jx[i] - 0.5) * nx[i] + (jy[i] - 0.5) * ny[i] < off[i]
    f_w[i] = np.mean(lw * sky_wall(u1[i], u2[i], cfg))
    ll = (qx[i] - 0.5) * nx[i] + (qy[i] - 0.5) * ny[i] < off[i]
    f_l[i] = np.mean(ll * sky_wall(f1[i], f2[i], cfg))
print("  " + report("white, independent", f_w, truth))
print("  " + report("R4 jitter + Fibonacci, same k", f_l, truth))

print("\nE. emissive units: receiver facing up, square emitter of radiance 1 at height h")
for h, s in ((0.5, 1.0), (1.0, 1.0), (2.0, 4.0)):
    n = 400000
    u1, u2 = rng.random(n), rng.random(n)
    r = np.sqrt(u1)
    dx, dy, dz = r * np.cos(2 * np.pi * u2), r * np.sin(2 * np.pi * u2), np.sqrt(1 - u1)
    t = h / dz
    hx, hy = dx * t, dy * t
    hit = (np.abs(hx) < s / 2) & (np.abs(hy) < s / 2)
    path = np.mean(hit * 1.0)
    px, py = rng.uniform(-s / 2, s / 2, n), rng.uniform(-s / 2, s / 2, n)
    d2 = px * px + py * py + h * h
    cosx = h / np.sqrt(d2)
    cosl = h / np.sqrt(d2)
    nee = np.mean(1.0 * cosx * cosl * (s * s) / d2)
    print("  h=%.1f size=%.1f  path (bounce ray lands on it) %.5f   baker direct formula %.5f   ratio %.4f (pi = %.4f)" %
          (h, s, path, nee, nee / path, math.pi))
