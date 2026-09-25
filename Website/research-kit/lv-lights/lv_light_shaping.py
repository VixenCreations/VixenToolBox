"""Experiment 6 parts B, C and E: terminator controls on sized lights, panel highlights, and the normal strength fix."""
import numpy as np
from lv_light_shapes import fib_sphere, cap_lit, cap_dirs, kernel, rect_samples, vector_ff, solid_angle_rect, corners, PI, log_path

out = []
pr = out.append


def l_signed(c, s2):
    ss = np.sqrt(s2)
    return np.where(c >= -ss, cap_lit(c, s2), c + ss)


def cand_size_ignored(c, s2, s, p):
    return kernel(c, s, p)


def cand_rep(c, s2, s, p):
    th = np.arccos(np.clip(c, -1, 1))
    return kernel(np.cos(np.maximum(0.0, th - np.arcsin(np.sqrt(s2)))), s, p)


def cand_signed_cap(c, s2, s, p):
    return kernel(l_signed(c, s2), s, p)


def cand_add(c, s2, s, p):
    return cap_lit(c, s2) + kernel(c, s, p) - np.clip(c, 0, 1)


def cand_max(c, s2, s, p):
    return np.maximum(cap_lit(c, s2), kernel(c, s, p))


def cand_two_tap(c, s2, s, p):
    d = 0.5 * np.arcsin(np.sqrt(s2))
    sn = np.sqrt(np.clip(1 - c * c, 0, 1))
    cm, cp = c * np.cos(d) + sn * np.sin(d), c * np.cos(d) - sn * np.sin(d)
    cm = np.where(np.arccos(np.clip(c, -1, 1)) < d, np.cos(d - np.arccos(np.clip(c, -1, 1))), cm)
    kk = 0.5 * (kernel(cm, s, p) + kernel(cp, s, p))
    ll = 0.5 * (np.clip(cm, 0, 1) + np.clip(cp, 0, 1))
    return cap_lit(c, s2) + kk - ll


def main():
    normals = fib_sphere(4000)
    c = normals[:, 2]
    pr("=== B. Soften and Hardness on a sized light (sphere cap). Truth: each direction of the source shaded by the kernel, integrated ===")
    pr("  Normalised so that Lambert facing the light square on is 1 (the capLit convention). RMS as a share of the truth peak.")
    pr("  add = capLit + K(c) - sat(c); max = max(capLit, K(c)); two-tap = capLit + mean over c at theta -+ size/2 of (K - sat).")
    cands = [("size ignored K(c)", cand_size_ignored), ("representative cos", cand_rep), ("signed cap K(L)", cand_signed_cap),
             ("add", cand_add), ("max", cand_max), ("two-tap", cand_two_tap)]
    agg = {n: [] for n, _ in cands}
    en = {n: [] for n, _ in cands}
    for sd in [5.0, 15.0, 30.0, 45.0, 70.0]:
        ss = np.sin(np.radians(sd))
        dirs, dw = cap_dirs(np.array([0, 0, 1.0]), ss, 6000)
        line = f"  source {sd:4.1f} deg:"
        for s in [0.0, 0.25, 0.5, 1.0]:
            for h in [0.0, 0.5, 1.0]:
                p = 1.0 + (0.15 - 1.0) * h
                T = kernel(normals @ dirs.T, s, p).sum(1) * dw / (PI * ss * ss)
                for n, f in cands:
                    v = f(c, ss * ss, s, p)
                    agg[n].append(np.sqrt(np.mean((v - T) ** 2)) / T.max())
                    en[n].append(v.mean() / T.mean())
        pr(line)
        for n, _ in cands:
            pr(f"      {n:20s} RMS mean {np.mean(agg[n][-12:]):.3f} worst {np.max(agg[n][-12:]):.3f} energy {np.min(en[n][-12:]):.2f}-{np.max(en[n][-12:]):.2f}")
    pr("  all sizes: " + "  ".join(f"{n}: mean {np.mean(agg[n]):.3f} worst {np.max(agg[n]):.3f}" for n, _ in cands))
    for n, f in cands[2:]:
        v0 = f(c, 1e-12, 0.5, 0.4)
        v1 = f(c, np.sin(np.radians(30)) ** 2, 0.0, 1.0)
        pr(f"  {n}: zero size against the per-pixel kernel {np.abs(v0 - kernel(c, 0.5, 0.4)).max():.2e}, Soften 0 Hardness 0 against the exact cap {np.abs(v1 - cap_lit(c, np.sin(np.radians(30)) ** 2)).max():.2e} (both must be 0)")

    pr("")
    pr("=== C. Panel highlights: GGX integrated over the panel, against two sized-light approximations ===")
    pr("  Shading point at the origin, normal +Z, views over the upper hemisphere. Light radiance 1, so the approximations get E0 = solid angle.")
    pr("  centre+LV: direction to the panel centre, upstream widening with spread = half diagonal^2 / d^2 (what upstream does).")
    pr("  rep+cap: closest point on the panel to the reflected ray, widening with spread = |F| (the equivalent cap), E0 = pi |F|.")


    def ggx_d(nh, a2):
        d = nh * nh * (a2 - 1.0) + 1.0
        return a2 / (PI * d * d)


    def smith(nl, nv, a2):
        lv = nl * np.sqrt(nv * nv * (1 - a2) + a2)
        ll = nv * np.sqrt(nl * nl * (1 - a2) + a2)
        return 0.5 / np.maximum(lv + ll, 1e-7)


    def sized_spec(N, V, L, E0, a, spread):
        a2 = a ** 4
        H = L + V
        H /= np.linalg.norm(H, axis=-1, keepdims=True)
        nh = np.clip(np.sum(N * H, -1), 0, 1)
        lov = np.sum(L * V, -1)
        w = np.maximum(2.0 + 2.0 * lov, 1e-4)
        ae = (a2 * w + spread) / (w + spread)
        ls = np.sqrt(np.clip(spread, 0, 1))
        nl = np.clip((np.sum(N * L, -1) + ls) / (1 + ls), 0, 1)
        nv = np.abs(np.sum(N * V, -1)) + 1e-5
        return E0 * ggx_d(nh, ae) * smith(nl, nv, a2) * nl


    CB = []
    COMB = []
    Nz = np.array([0, 0, 1.0])
    views = fib_sphere(3000)
    views = views[views[:, 2] > 0.05]
    Zp = np.array([0, 0, -1.0])
    panels = [
        ("1x1 m, 1.5 m overhead, offset", np.array([0.8, 0.0, 1.5]), np.array([1.0, 0, 0]), np.array([0, 1.0, 0]), 1.0, 1.0),
        ("strip 3x0.1 m, 1 m overhead", np.array([0.3, 0.0, 1.0]), np.array([1.0, 0, 0]), np.array([0, 1.0, 0]), 3.0, 0.1),
        ("2x1 m, 0.6 m overhead (close)", np.array([0.2, 0.3, 0.6]), np.array([1.0, 0, 0]), np.array([0, 1.0, 0]), 2.0, 1.0),
    ]
    for name, C, X, Y, w, h in panels:
        pts, dA = rect_samples(C, X, Y, w, h, 140)
        d = pts
        r2 = np.sum(d * d, 1)
        om = d / np.sqrt(r2)[:, None]
        dw = np.clip(-(om @ Zp), 0, None) * dA / r2
        Om = solid_angle_rect(np.zeros(3), C, X, Y, w, h)
        F = vector_ff(np.zeros(3), C, X, Y, w, h)
        fl = np.linalg.norm(F)
        hd = (w * w + h * h) * 0.25 / (C @ C)
        for rough in [0.1, 0.3, 0.6]:
            a2 = rough ** 4
            T = np.zeros(len(views))
            for k in range(0, len(views), 200):
                V = views[k:k + 200]
                H = om[None, :, :] + V[:, None, :]
                H /= np.linalg.norm(H, axis=-1, keepdims=True)
                nh = np.clip(H[..., 2], 0, 1)
                nl = np.clip(om[:, 2], 0, 1)[None, :]
                nv = V[:, 2:3] + 1e-5
                T[k:k + 200] = (ggx_d(nh, a2) * smith(nl, nv, a2) * nl * dw[None, :]).sum(1)
            Lc = C / np.linalg.norm(C)
            A1 = sized_spec(Nz, views, np.broadcast_to(Lc, views.shape), Om, rough, hd)
            R = 2 * views[:, 2:3] * Nz - views
            t = (C @ Zp) / np.minimum(R @ Zp, -1e-4)
            hit = R * t[:, None]
            loc = np.stack([(hit - C) @ X, (hit - C) @ Y], 1)
            loc = np.clip(loc, [-w / 2, -h / 2], [w / 2, h / 2])
            rep = C + loc[:, :1] * X + loc[:, 1:] * Y
            Lr = rep / np.linalg.norm(rep, axis=1, keepdims=True)
            dr = np.linalg.norm(rep, axis=1)
            al = rough * rough
            H = Lr + views
            H /= np.linalg.norm(H, axis=1, keepdims=True)
            nh = np.clip(H[:, 2], 0, 1)
            nl = np.clip(Lr[:, 2], 0, 1)
            nv = views[:, 2] + 1e-5
            pk = T.max()
            line = f"  {name}, roughness {rough}: truth peak {pk:.3f} | centre+LV RMS {np.sqrt(np.mean((A1 - T) ** 2)) / pk:.3f} energy {A1.mean() / T.mean():.2f}x"
            for k in [0.35, 0.5, 0.7]:
                ax = np.minimum(1.0, al + k * 0.5 * w / dr)
                ay = np.minimum(1.0, al + k * 0.5 * h / dr)
                A3 = Om * ggx_d(nh, al * al) * (al * al / (ax * ay)) * smith(nl, nv, al * al) * nl
                pr_k = f" | Karis k {k}: RMS {np.sqrt(np.mean((A3 - T) ** 2)) / pk:.3f} peak {A3.max() / pk:.2f}x energy {A3.mean() / T.mean():.2f}x"
                line += pr_k
                if k == 0.5:
                    A3k = A3
            pr(line)
            A4 = np.minimum(A3k, 1.0)
            A5 = np.minimum(A1, 1.0)
            A6 = 0.5 * (A4 + A5)
            pr(f"      capped at the panel radiance: Karis 0.5 RMS {np.sqrt(np.mean((A4 - T) ** 2)) / pk:.3f} energy {A4.mean() / T.mean():.2f}x"
               f" | centre+LV RMS {np.sqrt(np.mean((A5 - T) ** 2)) / pk:.3f} energy {A5.mean() / T.mean():.2f}x"
               f" | mean of both RMS {np.sqrt(np.mean((A6 - T) ** 2)) / pk:.3f} energy {A6.mean() / T.mean():.2f}x")
            CB.append((np.sqrt(np.mean((A5 - T) ** 2)) / pk, np.sqrt(np.mean((A4 - T) ** 2)) / pk, np.sqrt(np.mean((A6 - T) ** 2)) / pk, np.sqrt(np.mean((A1 - T) ** 2)) / pk))
            tb = np.clip(np.sqrt(fl) * 2.5, 0, 1)
            A7 = A5 + (A4 - A5) * tb
            COMB.append((name, rough, np.sqrt(np.mean((A7 - T) ** 2)) / pk, A7.mean() / T.mean(), tb))

    CBa = np.array(CB)
    pr(f"  mean RMS over all nine cases: upstream centre {CBa[:, 3].mean():.3f}, centre capped {CBa[:, 0].mean():.3f}, Karis capped {CBa[:, 1].mean():.3f}, mean of both capped {CBa[:, 2].mean():.3f}")
    pr(f"  worst RMS: upstream centre {CBa[:, 3].max():.3f}, centre capped {CBa[:, 0].max():.3f}, Karis capped {CBa[:, 1].max():.3f}, mean of both capped {CBa[:, 2].max():.3f}")

    pr("")
    pr("=== C2. Sphere highlights (point and spot lights with a source size) ===")
    SB = []
    for r, C in [(0.1, np.array([0.6, 0.0, 1.5])), (0.3, np.array([0.5, 0.2, 1.0])), (0.6, np.array([0.3, 0.0, 0.8]))]:
        d = np.linalg.norm(C)
        ss = min(r / d, 0.999)
        dirs, dwc = cap_dirs(C, ss, 12000)
        for rough in [0.1, 0.3, 0.6]:
            al = rough * rough
            a2 = al * al
            T = np.zeros(len(views))
            for k in range(0, len(views), 200):
                V = views[k:k + 200]
                H = dirs[None, :, :] + V[:, None, :]
                H /= np.linalg.norm(H, axis=-1, keepdims=True)
                T[k:k + 200] = (ggx_d(np.clip(H[..., 2], 0, 1), a2) * smith(np.clip(dirs[:, 2], 0, 1)[None, :], V[:, 2:3] + 1e-5, a2)
                                * np.clip(dirs[:, 2], 0, 1)[None, :]).sum(1) * dwc
            Om = 2 * PI * (1 - np.sqrt(1 - ss * ss))
            Lc = C / d
            A1 = sized_spec(Nz, views, np.broadcast_to(Lc, views.shape), Om, rough, ss * ss)
            R = 2 * views[:, 2:3] * Nz - views
            cvec = (R @ C)[:, None] * R - C
            cl = np.linalg.norm(cvec, axis=1, keepdims=True)
            rep = C + cvec * np.clip(r / np.maximum(cl, 1e-9), 0, 1)
            Lr = rep / np.linalg.norm(rep, axis=1, keepdims=True)
            H = Lr + views
            H /= np.linalg.norm(H, axis=1, keepdims=True)
            ap = np.minimum(1.0, al + 0.5 * ss)
            A3 = np.minimum(Om * ggx_d(np.clip(H[:, 2], 0, 1), a2) * (al / ap) ** 2 * smith(np.clip(Lr[:, 2], 0, 1), views[:, 2] + 1e-5, a2) * np.clip(Lr[:, 2], 0, 1), 1.0)
            pk = T.max()
            e1, e3 = np.sqrt(np.mean((A1 - T) ** 2)) / pk, np.sqrt(np.mean((A3 - T) ** 2)) / pk
            SB.append((e1, e3))
            tb = np.clip(ss * 2.5, 0, 1)
            A7 = np.minimum(A1, 1.0) + (A3 - np.minimum(A1, 1.0)) * tb
            COMB.append((f'sphere r {r}', rough, np.sqrt(np.mean((A7 - T) ** 2)) / pk, A7.mean() / T.mean(), tb))
            pr(f"  sphere r {r} at {d:.2f} m ({np.degrees(np.arcsin(ss)):4.1f} deg), roughness {rough}: truth peak {pk:.3f} | upstream widening RMS {e1:.3f} energy {A1.mean() / T.mean():.2f}x"
               f" | Karis sphere capped RMS {e3:.3f} energy {A3.mean() / T.mean():.2f}x")
    SBa = np.array(SB)
    pr(f"  mean RMS: upstream widening {SBa[:, 0].mean():.3f}, Karis sphere capped {SBa[:, 1].mean():.3f}; worst {SBa[:, 0].max():.3f} against {SBa[:, 1].max():.3f}")

    pr("")
    pr("=== C3. Combined highlight: upstream widening (capped) blended into Karis (capped) by source size, t = saturate(2.5 sin(size)) ===")
    for n_, r_, e_, en_, tb_ in COMB:
        pr(f"  {n_}, roughness {r_}: blend {tb_:.2f} RMS {e_:.3f} energy {en_:.2f}x")
    CMa = np.array([[e for _, _, e, _, _ in COMB]])
    pr(f"  combined over all 18 cases: mean RMS {CMa.mean():.3f}, worst {CMa.max():.3f}")

    pr("")
    pr("=== E. Normal strength: old lerp form against the slope form above 1 ===")
    rng = np.random.default_rng(7)
    tilt = np.radians(rng.uniform(0, 45, 20000))
    az = rng.uniform(0, 2 * PI, 20000)
    n = np.stack([np.sin(tilt) * np.cos(az), np.sin(tilt) * np.sin(az), np.cos(tilt)], 1)


    def old(n, k):
        v = np.array([0, 0, 1.0]) + (n - np.array([0, 0, 1.0])) * k
        return v / np.linalg.norm(v, axis=1, keepdims=True)


    def new(n, k):
        if k > 1.0:
            v = np.stack([n[:, 0] * k, n[:, 1] * k, np.maximum(n[:, 2], 1e-3)], 1)
        else:
            v = np.array([0, 0, 1.0]) + (n - np.array([0, 0, 1.0])) * k
        return v / np.linalg.norm(v, axis=1, keepdims=True)


    for k in [0.5, 1.0, 1.5, 2.0, 3.0, 5.0]:
        o, w_ = old(n, k), new(n, k)
        pr(f"  strength {k}: below the surface old {np.mean(o[:, 2] < 0) * 100:5.1f}% new {np.mean(w_[:, 2] < 0) * 100:4.1f}% | "
           f"median tilt old {np.degrees(np.median(np.arccos(np.clip(o[:, 2], -1, 1)))):5.1f} new {np.degrees(np.median(np.arccos(w_[:, 2]))):5.1f} deg | "
           f"max difference {np.abs(o - w_).max():.2e} | tilt order kept (new) {np.all(np.diff(np.arccos(w_[np.argsort(tilt), 2])) > -1e-9)}")

    open(log_path(__file__), "w").write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
