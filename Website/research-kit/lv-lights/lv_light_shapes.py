"""Experiment 6 part A: Light Volume point, spot and area lights - upstream SH result against the exact per-light result, the combined operator, and every difference."""
import os
import numpy as np


def log_path(script):
    here = os.path.dirname(os.path.abspath(script))
    res = os.path.join(os.path.dirname(here), "results")
    name = os.path.splitext(os.path.basename(script))[0] + ".txt"
    return os.path.join(res if os.path.basename(here) == "scripts" and os.path.isdir(res) else here, name)

PI = np.pi


def fib_sphere(n):
    i = np.arange(n) + 0.5
    z = 1.0 - 2.0 * i / n
    r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
    phi = i * PI * (3.0 - np.sqrt(5.0))
    return np.stack([r * np.cos(phi), r * np.sin(phi), z], 1)


def smooth01(x):
    return x * x * (3.0 - 2.0 * x)


def lv_mask(c, s):
    if s <= 0.0:
        return np.ones_like(c)
    b = 0.5 + 0.5 * np.clip(1.0 - s, 0.0, 1.0)
    return smooth01(np.clip(c * s * 0.5 + b, 0.0, 1.0))


def mask_moments(s):
    if s <= 0.0:
        return 1.0, 0.0
    b = 0.5 + 0.5 * min(max(1.0 - s, 0.0), 1.0)
    h = 0.5 * s
    lo, hi = b - h, b + h
    def P(u):
        return u ** 3 - 0.5 * u ** 4
    def Q(u):
        return 0.75 * u ** 4 - 0.4 * u ** 5
    a0, a1 = max(lo, 0.0), min(hi, 1.0)
    A = B = 0.0
    if a1 > a0:
        A += P(a1) - P(a0)
        B += (Q(a1) - Q(a0)) - b * (P(a1) - P(a0))
    if hi > 1.0:
        u0 = max(lo, 1.0)
        A += hi - u0
        B += 0.5 * (hi * hi - u0 * u0) - b * (hi - u0)
    return A / (2.0 * h), B / (2.0 * h * h)


def cap_lit(c, s2):
    c = np.clip(c, -1.0, 1.0)
    s2 = min(s2, 0.999999)
    out = np.clip(c, 0.0, None).astype(float)
    if s2 < 1e-6:
        return out
    m = c * c < s2
    cm = c[m]
    sinT = np.sqrt(np.maximum(1.0 - cm * cm, 1e-6))
    x = np.sqrt(1.0 / s2 - 1.0)
    y = -x * cm / sinT
    sy = sinT * np.sqrt(np.clip(1.0 - y * y, 0.0, 1.0))
    e = (cm * np.arccos(np.clip(y, -1.0, 1.0)) - x * sy) * s2 + np.arctan(sy / x)
    out[m] = np.clip(e / (PI * s2), 0.0, 1.0)
    return out


def cap_dirs(center, sin_s, n):
    cos_s = np.sqrt(1.0 - sin_s ** 2)
    i = np.arange(n) + 0.5
    z = 1.0 - (1.0 - cos_s) * i / n
    r = np.sqrt(np.maximum(0.0, 1.0 - z * z))
    phi = i * PI * (3.0 - np.sqrt(5.0))
    loc = np.stack([r * np.cos(phi), r * np.sin(phi), z], 1)
    w = center / np.linalg.norm(center)
    a = np.array([1.0, 0, 0]) if abs(w[0]) < 0.9 else np.array([0, 1.0, 0])
    u = np.cross(a, w); u /= np.linalg.norm(u)
    v = np.cross(w, u)
    return loc[:, :1] * u + loc[:, 1:2] * v + loc[:, 2:3] * w, 2 * PI * (1.0 - cos_s) / n


def kernel(c, s, p):
    return np.clip((c + s) / (1.0 + s), 0.0, 1.0) ** p


def rect_samples(C, X, Y, w, h, n):
    g = (np.arange(n) + 0.5) / n - 0.5
    gx, gy = np.meshgrid(g * w, g * h)
    pts = C + gx.reshape(-1, 1) * X + gy.reshape(-1, 1) * Y
    return pts, (w * h) / (n * n)


def rect_truth(P, C, X, Y, Z, w, h, normals, s=0.0, p=1.0, n=160):
    pts, dA = rect_samples(C, X, Y, w, h, n)
    d = pts - P
    r2 = np.sum(d * d, 1)
    om = d / np.sqrt(r2)[:, None]
    cosL = np.clip(np.sum(-om * Z, 1), 0.0, None)
    dw = cosL * dA / r2
    E = np.zeros(len(normals))
    for k in range(0, len(normals), 400):
        cn = normals[k:k + 400] @ om.T
        E[k:k + 400] = (kernel(cn, s, p) * dw).sum(1)
    return E, dw.sum()


def corners(C, X, Y, w, h):
    hx, hy = 0.5 * w * X, 0.5 * h * Y
    return [C - hx - hy, C + hx - hy, C + hx + hy, C - hx + hy]


def edge_fit(v1, v2):
    x = np.dot(v1, v2)
    y = abs(x)
    a = 0.8543985 + (0.4965155 + 0.0145206 * y) * y
    b = 3.4175940 + (4.1616724 + y) * y
    v = a / b
    ts = v if x > 0.0 else 0.5 / np.sqrt(max(1.0 - x * x, 1e-7)) - v
    return np.cross(v1, v2) * ts


def vector_ff(P, C, X, Y, w, h, fit=True):
    vs = [q - P for q in corners(C, X, Y, w, h)]
    vs = [q / np.linalg.norm(q) for q in vs]
    F = np.zeros(3)
    for i in range(4):
        a, b = vs[i], vs[(i + 1) % 4]
        if fit:
            F += edge_fit(a, b)
        else:
            th = np.arccos(np.clip(np.dot(a, b), -1, 1))
            cr = np.cross(a, b)
            F += cr / max(np.linalg.norm(cr), 1e-12) * th
    if not fit:
        F /= 2.0 * PI
    toC = C - P
    if np.dot(F, toC) < 0.0:
        F = -F
    return F


def solid_angle_rect(P, C, X, Y, w, h):
    vs = [q - P for q in corners(C, X, Y, w, h)]
    def tri(a, b, c):
        la, lb, lc = np.linalg.norm(a), np.linalg.norm(b), np.linalg.norm(c)
        num = abs(np.dot(a, np.cross(b, c)))
        den = la * lb * lc + np.dot(a, b) * lc + np.dot(a, c) * lb + np.dot(b, c) * la
        return 2.0 * np.arctan2(num, den)
    return tri(vs[0], vs[1], vs[2]) + tri(vs[0], vs[2], vs[3])


def lv_fast_quad(P, C, X, Y, Z, w, h):
    ltw = P - C
    loc = np.array([ltw @ X, ltw @ Y, ltw @ Z])
    half = np.array([w, h]) * 0.5
    area = max(w * h, 1e-6)
    ext = max(half @ half, 1e-6)
    cxy = np.clip(loc[:2], -half, half)
    rd = loc[:2] - cxy
    prs = rd @ rd + loc[2] ** 2
    csd = max(prs, 1e-6)
    blend = prs / (prs + ext)
    centerSq = max(ltw @ ltw, 1e-6)
    ssd = csd + (centerSq - csd) * blend
    isd = 1.0 / np.sqrt(ssd)
    ied = 1.0 / np.sqrt(ssd + ext)
    sa = np.arctan(area * loc[2] * isd * isd * ied * 0.25)
    l0 = sa / PI
    rep = cxy * (1.0 - blend)
    d = X * rep[0] + Y * rep[1] - ltw
    d /= np.linalg.norm(d)
    return d, l0, sa


def rms(a, b, peak):
    return float(np.sqrt(np.mean((a - b) ** 2)) / peak)


def main():
    normals = fib_sphere(6000)
    out = []
    pr = out.append

    pr("=== A1. Mask moments: closed form against the lattice (mean over normals of mask and c*mask) ===")
    worst = 0.0
    for s in [0.0, 0.25, 0.5, 1.0, 1.5, 2.0, 3.0, 5.0, 8.0]:
        A, B = mask_moments(s)
        c = normals[:, 2]
        m = lv_mask(c, s)
        Al, Bl = m.mean(), (c * m).mean()
        worst = max(worst, abs(A - Al), abs(B - Bl))
        pr(f"  PLS {s:4.2f}: A {A:.5f} (lattice {Al:.5f})  B {B:.5f} (lattice {Bl:.5f})  LV energy for kappa 1: {A + B:.4f} x l0")
    pr(f"  worst difference {worst:.2e}")

    pr("")
    pr("=== A2. Sphere cap closed form against lattice integration over the cap ===")
    worst = 0.0
    for sd in [2.0, 10.0, 30.0, 60.0, 85.0]:
        ss = np.sin(np.radians(sd))
        dirs, dw = cap_dirs(np.array([0, 0, 1.0]), ss, 20000)
        E = np.clip(normals @ dirs.T, 0, None).sum(1) * dw / (PI * ss * ss)
        cf = cap_lit(normals[:, 2], ss * ss)
        worst = max(worst, np.abs(E - cf).max())
    pr(f"  worst difference over 6000 normals and 5 sizes: {worst:.4f} (of a peak of 1)")

    pr("")
    pr("=== A3. Point light: upstream SH against the exact shape with the same energy ===")
    pr("  LV result = l0 * mask(PLS) * (1 + kappa c).  Combined = l0 * M(PLS,kappa) * 2(1+cos s) * capLit(c, s).")
    pr("  Energy of both is l0 * M by construction. Columns: facing / 60 deg / 90 deg / 120 deg / RMS difference (share of the combined peak)")
    for r, d in [(0.1, 3.0), (0.25, 2.0), (0.5, 1.0), (1.0, 1.0)]:
        s2 = r * r / (d * d + r * r)
        kap = d / np.sqrt(d * d + r * r)
        cs = np.sqrt(1 - s2)
        for s in [0.0, 3.0]:
            A, B = mask_moments(s)
            M = A + kap * B
            c = normals[:, 2]
            lv = lv_mask(c, s) * (1 + kap * c)
            comb = M * 2 * (1 + cs) * cap_lit(c, s2)
            at = lambda f, deg: float(f(np.array([np.cos(np.radians(deg))]))[0])
            f_lv = lambda cc: lv_mask(cc, s) * (1 + kap * cc)
            f_cb = lambda cc: M * 2 * (1 + cs) * cap_lit(cc, s2)
            pr(f"  r {r:4.2f} d {d:3.1f} (source {np.degrees(np.arcsin(np.sqrt(s2))):5.1f} deg) PLS {s:3.1f}: "
               f"LV {at(f_lv,0):.3f}/{at(f_lv,60):.3f}/{at(f_lv,90):.3f}/{at(f_lv,120):.3f}  "
               f"combined {at(f_cb,0):.3f}/{at(f_cb,60):.3f}/{at(f_cb,90):.3f}/{at(f_cb,120):.3f}  "
               f"energy LV {lv.mean():.4f} combined {comb.mean():.4f}  RMS diff {rms(lv, comb, comb.max()):.3f}")

    pr("")
    pr("=== A4. Area light: upstream SH, exact irradiance (lattice over the panel), vector form factor + sphere horizon ===")
    Z = np.array([0, 0, -1.0]); X = np.array([1.0, 0, 0]); Y = np.array([0, 1.0, 0])
    scenes = [
        ("small panel, 3 m, face on", np.array([0, 0, 3.0]), 0.5, 0.5),
        ("1x1 m panel, 1 m, face on", np.array([0, 0, 1.0]), 1.0, 1.0),
        ("2x1 m panel, 0.5 m (close)", np.array([0, 0, 0.5]), 2.0, 1.0),
        ("strip 3x0.1 m, 1 m", np.array([0, 0, 1.0]), 3.0, 0.1),
        ("1x1 m panel, 1 m, 60 deg off axis", np.array([1.5, 0, 0.9]), 1.0, 1.0),
        ("4x4 m ceiling, 0.3 m (huge)", np.array([0, 0, 0.3]), 4.0, 4.0),
        ("1x1 m panel, receiver nearly in its plane", np.array([1.5, 0, 0.1]), 1.0, 1.0),
    ]
    for name, C, w, h in scenes:
        P = np.zeros(3)
        Et, _ = rect_truth(P, C, X, Y, Z, w, h, normals)
        Om = solid_angle_rect(P, C, X, Y, w, h)
        F = vector_ff(P, C, X, Y, w, h)
        Fe = vector_ff(P, C, X, Y, w, h, fit=False)
        fl = np.linalg.norm(F)
        Fh = F / fl
        ours = fl * cap_lit(normals @ Fh, fl)
        dlv, l0lv, sa = lv_fast_quad(P, C, X, Y, Z, w, h)
        for s in [0.0, 3.0]:
            A, B = mask_moments(s)
            kap = 1 - l0lv
            M = A + kap * B
            l0 = PI * l0lv
            c_lv = normals @ dlv
            lv = l0 * lv_mask(c_lv, s) * (1 + kap * c_lv)
            cs = np.sqrt(1 - fl)
            comb = l0 * M * 2 * (1 + cs) / fl * ours
            truth = l0 * M * 4 * PI / Om * Et / PI
            pk = truth.max()
            if s == 0.0:
                pr(f"  {name}: solid angle exact {Om:.4f}, upstream 4 x atan {4 * sa:.4f} ({(4 * sa / Om - 1) * 100:+.1f}%), "
                   f"|F| fit {fl:.4f} exact {np.linalg.norm(Fe):.4f} truth-normal {Et.max() / PI:.4f}")
            pr(f"     PLS {s:3.1f}: energy truth {truth.mean():.5f} LV {lv.mean():.5f} combined {comb.mean():.5f} | "
               f"RMS vs truth (share of peak): LV {rms(lv, truth, pk):.3f}  combined {rms(comb, truth, pk):.3f} | "
               f"peak LV {lv.max() / pk:.2f}x combined {comb.max() / pk:.2f}x | lit below horizon of the truth: LV {np.mean((lv > 0.02 * pk) & (truth < 1e-6)) * 100:.1f}% of normals, combined {np.mean((comb > 0.02 * pk) & (truth < 1e-6)) * 100:.1f}%")

    open(log_path(__file__), "w").write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
