"""As-built check of the 2026-09-23 rim: a line-for-line Python copy of the HLSL (band, vw_CapLit, vw_RimA0, vw_RimDir, vw_RimSH) run on real scenes against the old rim, plus agreement between a realtime light and the same light baked into SH."""
import math
import numpy as np


def cap_lit(c, wrap):
    s2 = math.sin(min(max(wrap, 0.0), 1.0) * math.pi * 0.5) ** 2
    s2 = min(s2, 0.999999)
    c = min(max(c, -1.0), 1.0)
    if c * c >= s2 or s2 < 1e-6:
        return max(c, 0.0)
    sinT = math.sqrt(max(1.0 - c * c, 1e-6))
    x = math.sqrt(1.0 / s2 - 1.0)
    y = -x * c / sinT
    sy = sinT * math.sqrt(min(max(1.0 - y * y, 0.0), 1.0))
    e = (c * math.acos(min(max(y, -1.0), 1.0)) - x * sy) * s2 + math.atan(sy / x)
    return min(max(e / (math.pi * s2), 0.0), 1.0)


def rim_a0(wrap):
    return 2.0 / (1.0 + math.cos(min(max(wrap, 0.0), 1.0) * math.pi * 0.5))


def norm(v):
    v = np.asarray(v, dtype=float)
    return v / max(np.linalg.norm(v), 1e-12)


def rim_dir(N, V):
    b = N - V
    bb = float(np.dot(b, b))
    return b / math.sqrt(bb) if bb > 1e-4 else N


def nonlinear(L0, L1, n):
    if L0 <= 1e-5:
        return 0.0
    ln = np.linalg.norm(L1)
    if ln <= 1e-5:
        return L0
    rr = min(ln / L0, 1.0)
    q = min(max(0.5 * (1.0 + np.dot(L1 / ln, n)), 0.0), 1.0)
    p = 1.0 + 2.0 * rr
    a = (1.0 - rr) / (1.0 + rr)
    return L0 * (a + (1.0 - a) * (p + 1.0) * q ** p)


def rim_sh(L0, L1, d, a0, follow):
    lobe = nonlinear(L0 * a0, L1, d)
    return (1 - follow) * max(L0 * a0, 0.0) + follow * lobe


def band(ndv, width, blur, fw=0.0):
    u = 1.0 - math.sqrt(min(max(1.0 - ndv * ndv, 0.0), 1.0))
    lo = width * (1.0 - blur) - fw
    t = min(max((u - lo) / max(width + fw - lo, 1e-4), 0.0), 1.0)
    return (1.0 - t * t * (3.0 - 2.0 * t)) if width > 0 else 0.0


def sh_of_light(direction, power=1.0):
    return 0.25 * power, 0.5 * power * np.asarray(direction, dtype=float)


V = np.array([0.0, 0.0, 1.0])
WRAP, FOLLOW, WIDTH, BLUR = 0.5, 1.0, 0.4, 0.8


def old_rim(ndv, n, amb_sh, sun_color):
    k = 30.0 + (0.1 - 30.0) * 0.4
    shape = (1.0 - ndv) ** k
    amb = max(amb_sh[0] + np.dot(amb_sh[1], n), 0.0) if amb_sh else 0.0
    return shape * (amb + sun_color * 0.5 + 0.1)


def new_rim(ndv, n, amb_sh, sun_dir, sun_color):
    d = rim_dir(n, V)
    a0 = rim_a0(WRAP)
    amb = rim_sh(amb_sh[0], amb_sh[1], d, a0, FOLLOW) if amb_sh else 0.0
    flat = a0 * 0.25
    lit = sun_color * ((1 - FOLLOW) * flat + FOLLOW * cap_lit(np.dot(d, sun_dir), WRAP)) if sun_color else 0.0
    return band(ndv, WIDTH, BLUR) * (amb + lit)


print("Rim at the default Width 4 (old: Follow 0; new: Blur 0.8, Follow 1, Wrap 0.5), Strength 1, white material.")
print("A point on the right edge of a limb; 'in' is how far in from the outline as a share of the limb radius.\n")
scenes = [
    ("Light Volume, light behind you", sh_of_light(-V), None, 0.0),
    ("Light Volume, light beside the edge", sh_of_light([1, 0, 0]), None, 0.0),
    ("Light Volume, light at the camera", sh_of_light(V), None, 0.0),
    ("Realtime sun behind you", None, -V, 1.0),
    ("Realtime sun at the camera", None, V, 1.0),
    ("Dark room, no light", None, None, 0.0),
]
print("  %-38s %-22s %s" % ("scene", "old  in 2% / 10% / 25%", "new  in 2% / 10% / 25%"))
for name, sh, sdir, scol in scenes:
    olds, news = [], []
    for u in (0.02, 0.10, 0.25):
        s = 1.0 - u
        ndv = math.sqrt(max(0.0, 1.0 - s * s))
        n = np.array([s, 0.0, ndv])
        olds.append(old_rim(ndv, n, sh, scol))
        news.append(new_rim(ndv, n, sh, sdir if sdir is not None else V, scol))
    print("  %-38s %s   %s" % (name, " ".join("%.3f" % v for v in olds), " ".join("%.3f" % v for v in news)))

print("\nThe same light, realtime or baked into SH, at Wrap 0.5: rim lobe response by angle between the rim direction and the light")
print("  angle   realtime (cap)   baked L1 SH (non-linear)")
for deg in (0, 30, 60, 90, 120, 150):
    c = math.cos(math.radians(deg))
    L0, L1 = sh_of_light([0, 0, 1])
    dvec = np.array([math.sin(math.radians(deg)), 0.0, c])
    print("  %3d     %.3f            %.3f" % (deg, cap_lit(c, WRAP), rim_sh(L0, L1, dvec, rim_a0(WRAP), 1.0)))

flat_rt = rim_a0(WRAP) * 0.25
L0, L1 = sh_of_light([0, 0, 1])
print("\nFollow 0 (flat): realtime %.4f, baked %.4f" % (flat_rt, rim_sh(L0, L1, np.array([1.0, 0, 0]), rim_a0(WRAP), 0.0)))

bad = 0
for w in np.linspace(0, 1, 11):
    for deg in range(0, 181, 3):
        v = cap_lit(math.cos(math.radians(deg)), w)
        if not math.isfinite(v) or v < 0 or v > 1:
            bad += 1
for ndv in np.linspace(0, 1.00001, 200):
    for wd in (0.0, 0.05, 0.4, 1.0):
        for bl in (0.0, 0.5, 1.0):
            v = band(ndv, wd, bl)
            if not math.isfinite(v) or v < 0 or v > 1:
                bad += 1
print("out-of-range or non-finite values over the whole control range:", bad)
