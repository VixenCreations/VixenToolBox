"""MonoSH ratio lightmaps: compare per-texel estimators of the luminance SH (A, B) from the baker's lattice samples, against exact irradiance at tilted normals."""
import math
import numpy as np

W = np.array([0.2126, 0.7152, 0.0722])
GOLD = 0.6180339887498949


def norm(v):
    return v / np.linalg.norm(v, axis=-1, keepdims=True)


def sph(el_deg, az_deg):
    el, az = math.radians(el_deg), math.radians(az_deg)
    return np.array([math.cos(el) * math.cos(az), math.cos(el) * math.sin(az), math.sin(el)])


def fine_hemi(n):
    k = np.arange(n) + 0.5
    z = k / n
    r = np.sqrt(1 - z * z)
    phi = 2 * math.pi * ((np.arange(n) * GOLD) % 1.0)
    return np.stack([r * np.cos(phi), r * np.sin(phi), z], -1), 2 * math.pi / n


def region(d, az0, az1, el0, el1):
    el = np.degrees(np.arcsin(np.clip(d[..., 2], -1, 1)))
    az = np.degrees(np.arctan2(d[..., 1], d[..., 0])) % 360
    return (el >= el0) & (el <= el1) & (az >= az0) & (az <= az1)


SCENES = {
    "screen + downlight": dict(
        direct=[(sph(70, 200), np.array([1.0, 0.85, 0.6]) * 3.0)],
        field=lambda d: 0.05 + region(d, 0, 50, 5, 40)[:, None] * np.array([0.3, 0.5, 1.2]) * 4.0),
    "sun + sky": dict(
        direct=[(sph(35, 40), np.array([1.0, 0.9, 0.75]) * 4.0)],
        field=lambda d: np.array([0.35, 0.5, 0.8]) * (0.4 + 0.6 * d[..., 2:3])),
    "two opposed lights": dict(
        direct=[(sph(30, 0), np.array([1.0, 0.2, 0.1]) * 2.0), (sph(30, 180), np.array([0.1, 0.3, 1.0]) * 2.0)],
        field=lambda d: 0.03 + 0 * d),
    "grazing lamp": dict(
        direct=[(sph(8, 90), np.array([1.0, 0.95, 0.9]) * 6.0)],
        field=lambda d: 0.08 + 0 * d),
    "bright wall, no lights": dict(
        direct=[],
        field=lambda d: 0.04 + region(d, 300, 360, 0, 35)[:, None] * np.array([1.0, 0.9, 0.8]) * 1.5),
}


def tilted_normals(count, max_deg=45.0, seed=1):
    rng = np.random.default_rng(seed)
    out = []
    while len(out) < count:
        v = rng.normal(size=3)
        v /= np.linalg.norm(v)
        if v[2] >= math.cos(math.radians(max_deg)):
            out.append(v)
    return np.array(out)


N = tilted_normals(400)
DF, DW = fine_hemi(200000)


def exact(scene):
    f = scene["field"](DF)
    ref = np.maximum(N @ DF.T, 0.0) @ f * DW / math.pi
    for w, phi in scene["direct"]:
        ref += np.maximum(N @ w, 0.0)[:, None] * phi[None, :] / math.pi
    return ref


def mixture_mu(u, alpha):
    if alpha >= 1.0:
        return np.sqrt(np.clip(u, 0, 1))
    b = 1.0 - alpha
    return (-b + np.sqrt(b * b + 4 * alpha * u)) / (2 * alpha)


def directions(S, shift, turn, alpha):
    k = np.arange(S)
    u1 = ((k + 0.5) / S + shift) % 1.0
    u2 = (k * GOLD + turn) % 1.0
    mu = mixture_mu(u1, alpha)
    s = np.sqrt(np.clip(1 - mu * mu, 0, 1))
    phi = 2 * math.pi * u2
    d = np.stack([s * np.cos(phi), s * np.sin(phi), mu], -1)
    pdf = alpha * mu / math.pi + (1 - alpha) / (2 * math.pi)
    return d, pdf


def estimate(scene, S, rng, method, alpha=1.0, clamp=0.0, quant=True):
    d, pdf = directions(S, rng.random(), rng.random(), alpha)
    L = scene["field"](d)
    lum = L @ W
    mu = d[:, 2]
    e0 = (L * (mu / pdf)[:, None]).mean(0) / math.pi
    if method == "moment":
        wA = 1.0 / pdf
        M = (d * (lum * mu / pdf)[:, None]).mean(0)
        A = (lum * wA).mean()
        cdom = max(M[2] / max(np.linalg.norm(M), 1e-9), 0.2)
        B = np.array([M[0] / cdom, M[1] / cdom, M[2]])
    else:
        wA = 1.0 / np.maximum(pdf, clamp / math.pi) if clamp > 0 else 1.0 / pdf
        A = (lum * wA).mean()
        B = (d * (lum * wA)[:, None]).mean(0)
    dirAcc = (d * (lum * mu / pdf / math.pi * math.pi)[:, None]).mean(0)
    dirW = (lum * mu / pdf).mean()
    for w, phi in scene["direct"]:
        c = phi * max(w[2], 0.0)
        e0 = e0 + c / math.pi
        pl = phi @ W
        A += pl
        B = B + pl * w
        dirAcc = dirAcc + w * (c @ W)
        dirW += c @ W
    V = 2 * B / max(A, 1e-9)
    if quant:
        V = (np.round(np.clip(V / 4 + 0.5, 0, 1) * 255) / 255 - 0.5) * 4
    ratio = np.maximum(1 + N @ V, 0.0) / max(1 + V[2], 1e-3)
    mono = e0[None, :] * ratio[:, None]
    dom = dirAcc / max(dirW, 1e-9)
    directionality = min(np.linalg.norm(dom), 1.0)
    D = 0.5 * dom / max(np.linalg.norm(dom), 1e-9) * directionality
    unity = e0[None, :] * ((N @ D + 0.5) / max(D[2] + 0.5, 1e-4))[:, None]
    flat = np.tile(e0, (len(N), 1))
    return flat, unity, mono


def rel_rms(est, ref):
    el, rl = est @ W, ref @ W
    return math.sqrt(np.mean((el - rl) ** 2)) / rl.mean()


def run(S=256, trials=200):
    rng = np.random.default_rng(11)
    configs = [
        ("cosine, 1/cos", dict(method="plain", alpha=1.0)),
        ("cosine, cos floor 0.1", dict(method="plain", alpha=1.0, clamp=0.1)),
        ("cosine, cos floor 0.2", dict(method="plain", alpha=1.0, clamp=0.2)),
        ("mixture a=0.8", dict(method="plain", alpha=0.8)),
        ("mixture a=0.6", dict(method="plain", alpha=0.6)),
        ("mixture a=0.4", dict(method="plain", alpha=0.4)),
        ("cosine, 1/cos, no 8-bit", dict(method="plain", alpha=1.0, quant=False)),
    ]
    print(f"S = {S} indirect samples per texel, {trials} texels per cell, 400 normals within 45 degrees")
    print("error = RMS of luminance over the normals / mean, averaged over the texels (noise and bias together)")
    for name, scene in SCENES.items():
        ref = exact(scene)
        print(f"\n{name}")
        res_flat, res_unity = [], []
        _, _, mono_ex = estimate_exact(scene)
        for cname, cfg in configs:
            errs, e0err, noise = [], [], []
            for t in range(trials):
                flat, unity, mono = estimate(scene, S, rng, **cfg)
                errs.append(rel_rms(mono, ref))
                noise.append(rel_rms(mono / (flat[0] @ W) * (exact_e0(scene) @ W), mono_ex))
                e0err.append(abs((flat[0] @ W) - (exact_e0(scene) @ W)) / (exact_e0(scene) @ W))
                if cname == "cosine, 1/cos":
                    res_flat.append(rel_rms(flat, ref))
                    res_unity.append(rel_rms(unity, ref))
            print(f"  MonoSH ratio, {cname:24s} error mean {np.mean(errs):.4f} p95 {np.percentile(errs, 95):.4f} | direction noise mean {np.mean(noise):.4f} p95 {np.percentile(noise, 95):.4f} | colour noise {np.mean(e0err):.4f}")
        print(f"  non-directional                         mean {np.mean(res_flat):.4f}")
        print(f"  Unity directional (as the baker bakes)  mean {np.mean(res_unity):.4f}")
        _, _, mono_exact = estimate_exact(scene)
        print(f"  MonoSH ratio, exact A and B             mean {rel_rms(mono_exact, ref):.4f}")


def exact_e0(scene):
    f = scene["field"](DF)
    e0 = (f * DF[:, 2:3]).sum(0) * DW / math.pi
    for w, phi in scene["direct"]:
        e0 = e0 + phi * max(w[2], 0) / math.pi
    return e0


def estimate_exact(scene):
    f = scene["field"](DF)
    lum = f @ W
    A = lum.sum() * DW
    B = (DF * lum[:, None]).sum(0) * DW
    for w, phi in scene["direct"]:
        A += phi @ W
        B = B + (phi @ W) * w
    V = 2 * B / A
    e0 = exact_e0(scene)
    ratio = np.maximum(1 + N @ V, 0) / max(1 + V[2], 1e-3)
    return None, None, e0[None, :] * ratio[:, None]


if __name__ == "__main__":
    import sys
    run(S=int(sys.argv[1]) if len(sys.argv) > 1 else 256, trials=int(sys.argv[2]) if len(sys.argv) > 2 else 200)
