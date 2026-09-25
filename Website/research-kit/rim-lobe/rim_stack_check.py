"""Numbers for the 2026-09-23 rim and light stack audit: rim width at the default, Follow Light masking, and the Sharper Probes L2 double count."""
import math

def rim_exp(width):
    return 30.0 + (0.1 - 30.0) * min(max(width / 10.0, 0.0), 1.0)

print("Rim Width -> exponent, rim value at NdotV, silhouette band where rim > 0.1 (share of a cylinder's radius)")
for w in (2, 4, 6, 8, 9, 10):
    e = rim_exp(w)
    vals = [pow(1 - n, e) for n in (0.05, 0.1, 0.2, 0.3, 0.5)]
    ndv = 1 - pow(0.1, 1 / e)
    band = 1 - math.sqrt(max(0.0, 1 - ndv * ndv))
    print(f"  width {w:>2}: exp {e:6.2f}  " + "  ".join(f"{v:.3f}" for v in vals) + f"   band {band*100:6.2f}%")

def wrap_lit(ndl, wrap):
    return min(max((ndl + wrap) / (1 + wrap), 0.0), 1.0)

print("\nFollow Light at 1, Wrap 0.5. Edge normal faces a screen on the left; sun comes from the right.")
N = (-1.0, 0.0, 0.0)
sun = (1.0, 0.0, 0.0)
dot = lambda a, b: sum(x * y for x, y in zip(a, b))
old_mask = wrap_lit(dot(N, sun), 0.5)
print(f"  old: the LTCGI edge term was multiplied by {old_mask:.3f} (steered by the sun)")
print(f"  new: the LTCGI edge term is multiplied by 1.000; LTCGI's own light at this normal decides it")
N2 = (1.0, 0.0, 0.0)
print(f"  sun-facing edge: old {wrap_lit(dot(N2, sun), 0.5):.3f}, new {wrap_lit(dot(N2, sun), 0.5):.3f} (unchanged)")

print("\nSharper Probes, one light in the probe (Unity irradiance bands: L0 0.25, L1 0.5 cos, L2 0.3125 P2(cos))")
for deg in (0, 45, 70, 90, 120):
    c = math.cos(math.radians(deg))
    l0l1 = 0.25 + 0.5 * c
    l2 = 0.3125 * (3 * c * c - 1) / 2
    right = max(0.0, l0l1 + l2)
    doubled = max(0.0, l0l1 + 2 * l2)
    print(f"  {deg:>3} deg from the light: correct {right:.3f}  doubled L2 {doubled:.3f}  ({(doubled - right) / max(right, 1e-6) * 100:+.0f}%)")
