"""Experiment 6 part D: a line-for-line float32 copy of the as-built HLSL (vw_ULLVLight and helpers), checked against the research operators and on extreme inputs."""
import numpy as np
from lv_light_shapes import fib_sphere, cap_lit, mask_moments, vector_ff, lv_mask, PI, log_path
from lv_light_shaping import cand_two_tap, kernel

f32 = np.float32
out = []
pr = out.append


def sat(x):
    return np.clip(x, 0.0, 1.0)


def hl_caplit_s2(c, sin2):
    s2 = np.clip(sin2, 0.0, 0.999999).astype(f32)
    c = np.clip(c, -1.0, 1.0).astype(f32)
    r = sat(c)
    m = (c * c < s2) & (s2 >= 1e-6)
    sinT = np.sqrt(np.maximum(1.0 - c * c, 1e-6))
    x = np.sqrt(1.0 / np.maximum(s2, 1e-12) - 1.0)
    y = -x * c / sinT
    sy = sinT * np.sqrt(sat(1.0 - y * y))
    e = (c * np.arccos(np.clip(y, -1, 1)) - x * sy) * s2 + np.arctan(sy / np.maximum(x, 1e-12))
    return np.where(m, sat(e / (PI * np.maximum(s2, 1e-12))), r).astype(f32)


def hl_kernel(c, soft, hardP):
    return np.power(sat((c + soft) / (1.0 + soft)), hardP).astype(f32)


def hl_sized_shape(c, sin2, soft, hardP):
    cs = np.sqrt(sat(1.0 - sin2))
    cd = np.sqrt(0.5 + 0.5 * cs)
    sd = np.sqrt(sat(0.5 - 0.5 * cs))
    sn = np.sqrt(sat(1.0 - c * c))
    cm = c * cd + sn * sd
    cp = c * cd - sn * sd
    k = 0.5 * (hl_kernel(cm, soft, hardP) + hl_kernel(cp, soft, hardP))
    l = 0.5 * (sat(cm) + sat(cp))
    return np.maximum(hl_caplit_s2(c, sin2) + k - l, 0.0).astype(f32)


def hl_mask_moments(pls):
    s = max(pls, 0.0)
    b = 0.5 + 0.5 * min(max(1.0 - s, 0.0), 1.0)
    h = max(0.5 * s, 1e-4)
    lo, hi = b - h, b + h
    a0, a1 = min(max(lo, 0), 1), min(max(hi, 0), 1)
    p0 = a0 ** 3 * (1.0 - 0.5 * a0)
    p1 = a1 ** 3 * (1.0 - 0.5 * a1)
    q0 = a0 ** 4 * (0.75 - 0.4 * a0)
    q1 = a1 ** 4 * (0.75 - 0.4 * a1)
    u0 = max(lo, 1.0)
    A = (p1 - p0) + max(hi - u0, 0.0)
    B = (q1 - q0) - b * (p1 - p0) + (0.5 * (hi * hi - u0 * u0) - b * (hi - u0)) * (1.0 if hi >= 1.0 else 0.0)
    return (A / (2 * h), B / (2 * h * h)) if s > 0 else (1.0, 0.0)


def main():
    pr("=== D1. vw_LVMaskMoments (as built) against the research closed form ===")
    w = 0.0
    for s in np.concatenate([[0.0], np.geomspace(0.01, 20.0, 60)]):
        a, b = hl_mask_moments(float(s))
        ra, rb = mask_moments(float(s))
        w = max(w, abs(a - ra), abs(b - rb))
    pr(f"  61 values of Point Light Shading from 0 to 20: worst difference {w:.2e}")

    pr("")
    pr("=== D2. vw_ULSizedShape (as built, float32) against the research two-tap form (float64) ===")
    normals = fib_sphere(4000)
    c = normals[:, 2].astype(f32)
    w = 0.0
    for sd in [0.0, 1.0, 5.0, 15.0, 30.0, 45.0, 70.0, 89.0]:
        s2 = f32(np.sin(np.radians(sd)) ** 2)
        for soft in [0.0, 0.3, 1.0]:
            for hp in [1.0, 0.575, 0.15]:
                a = hl_sized_shape(c, s2, f32(soft), f32(hp))
                r = cand_two_tap(normals[:, 2], max(float(s2), 1e-12), soft, hp)
                w = max(w, np.abs(a - r).max())
    pr(f"  8 sizes x 3 Soften x 3 Hardness x 4000 normals: worst difference {w:.2e}")
    a0 = hl_sized_shape(c, f32(0.0), f32(0.4), f32(0.3))
    pr(f"  zero size against the per-pixel terminator (same look as a Unity light): {np.abs(a0 - hl_kernel(c, f32(0.4), f32(0.3))).max():.2e}")

    pr("")
    pr("=== D3. One point light end to end: energy of the as-built diffuse against upstream's SH (same l0) ===")
    for r, d in [(0.1, 3.0), (0.5, 1.0), (1.0, 1.0)]:
        spread = r * r / (d * d)
        sin2 = spread / (1 + spread)
        kap = d / np.sqrt(d * d + r * r)
        for pls in [0.0, 3.0]:
            A, B = hl_mask_moments(pls)
            M = A + kap * B
            cs = np.sqrt(1 - sin2)
            W = M * 2 * (1 + cs)
            ours = W * hl_sized_shape(c, f32(sin2), f32(0), f32(1))
            lv = lv_mask(normals[:, 2], pls) * (1 + kap * normals[:, 2])
            pr(f"  r {r} d {d} PLS {pls}: energy as built {ours.mean():.4f} upstream {lv.mean():.4f} (ratio {ours.mean() / lv.mean():.4f}) | facing {ours.max():.3f} against {lv.max():.3f}")

    pr("")
    pr("=== D4. Extremes: every input at its limits, float32, looking for NaN, infinity or negatives ===")
    bad = 0
    tot = 0
    cc = np.linspace(-1, 1, 41).astype(f32)
    for s2 in [0.0, 1e-9, 1e-6, 1e-3, 0.5, 0.9999, 1.0]:
        for soft in [0.0, 1.0]:
            for hp in [0.15, 1.0]:
                v = hl_sized_shape(cc, f32(s2), f32(soft), f32(hp))
                tot += v.size
                bad += int(np.sum(~np.isfinite(v) | (v < 0) | (v > 2.0)))
    for s in [0.0, 1e-6, 0.5, 1.0, 1e3]:
        a, b = hl_mask_moments(s)
        tot += 2
        bad += int(not (np.isfinite(a) and np.isfinite(b) and a >= 0))
    for sin2 in [0.0, 1e-6, 0.5, 0.9999]:
        cs = np.sqrt(1 - sin2)
        capF = (1 + cs) / (2 * PI * max(sin2, 1e-8))
        tot += 1
        bad += int(not np.isfinite(np.float32(capF)))
    pr(f"  {tot} values checked, {bad} bad")

    pr("")
    pr("=== D5. Area light path as built: F from four corners with the fitted edge integral, against the exact acos form ===")
    X = np.array([1.0, 0, 0]); Y = np.array([0, 1.0, 0])
    w = 0.0
    for C, sz in [(np.array([0, 0, 3.0]), (0.5, 0.5)), (np.array([0.2, 0.1, 0.3]), (4.0, 4.0)), (np.array([1.5, 0, 0.1]), (1.0, 1.0)), (np.array([0.3, 0, 1.0]), (3.0, 0.1))]:
        Ff = vector_ff(np.zeros(3), C, X, Y, sz[0], sz[1], fit=True)
        Fe = vector_ff(np.zeros(3), C, X, Y, sz[0], sz[1], fit=False)
        w = max(w, np.linalg.norm(Ff - Fe) / np.linalg.norm(Fe))
    pr(f"  worst relative difference of the fitted vector form factor: {w * 100:.2f}%")

    open(log_path(__file__), "w").write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
