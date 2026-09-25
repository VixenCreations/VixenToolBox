"""Experiment 3, part C: how much of a glossy highlight lands in AgX's visible detail band (linear 1 to 7.2, part B) for a point light against lights with a size, using GGX widened by Karis' sphere-light roughness (alpha' = alpha + tan(r)/2); the widened NDF still integrates to 1, so no extra energy factor (Karis' (alpha/alpha')^2 belongs to his representative-point method), on black latex (F0 0.04) with a light of 1."""
import math
import numpy as np

N = 400000
u = (np.arange(N) + 0.5) / N
ct = 1.0 - u * (1.0 - math.cos(math.radians(60)))
DW = 2 * math.pi * (1 - math.cos(math.radians(60))) / N


def ggx(nh, a):
    a2 = a * a
    d = nh * nh * (a2 - 1.0) + 1.0
    return a2 / (math.pi * d * d)


print("black latex, F0 0.04, light of 1 facing the surface, view along the reflection")
print("smooth  light size   peak     solid angle of the highlight (sr x 1e-3)")
print("                              above 1    in AgX detail band 1-7.2   above 7.2 (flat white)")
for smooth in (0.9, 0.95, 0.97):
    a = (1.0 - smooth) ** 2
    for size_deg in (0.0, 5.0, 10.0, 20.0):
        rr = math.radians(size_deg)
        a2 = min(a + math.tan(rr) / 2.0, 1.0)
        norm = 1.0
        # nh sampled over the reflection lobe, highlight radiance ~ D * F0 / 4 at normal incidence
        val = ggx(ct, a2) * norm * 0.04 * 0.25 * math.pi
        s_above = np.sum(val > 1.0) * DW * 4
        s_band = np.sum((val > 1.0) & (val <= 7.2)) * DW * 4
        s_white = np.sum(val > 7.2) * DW * 4
        print("%-7.2f %5.0f deg   %8.1f   %7.3f    %7.3f                    %7.3f" %
              (smooth, size_deg, val.max(), s_above * 1e3, s_band * 1e3, s_white * 1e3))
