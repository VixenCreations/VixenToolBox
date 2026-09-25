"""Experiment 5: fit the GGX prefilter lobe's zonal band weights k1, k2 against perceptual roughness 0.5 to 1.0 with quadratics, and print the shader multipliers on Unity's cosine-convolved SH bands (1, 1.5 k1, 4 k2) with the worst fit error."""
import math
import numpy as np

GOLD = (math.sqrt(5.0) - 1.0) / 2.0


def zh_weights(alpha, count=262144):
    k = np.arange(count)
    u1 = (k + 0.5) / count
    u2 = np.mod(k * GOLD, 1.0)
    phi = 2 * np.pi * u2
    ct = np.sqrt((1 - u1) / (1 + (alpha * alpha - 1) * u1))
    st = np.sqrt(1 - ct * ct)
    h = np.stack([st * np.cos(phi), st * np.sin(phi), ct], axis=-1)
    x = 2 * h[:, 2] * h[:, 2] - 1
    w = np.maximum(x, 0.0)
    return float(np.sum(x * w) / np.sum(w)), float(np.sum(0.5 * (3 * x * x - 1) * w) / np.sum(w))


rough = np.linspace(0.5, 1.0, 11)
k1, k2 = np.array([zh_weights(r * r) for r in rough]).T
c1 = np.polyfit(rough, 1.5 * k1, 2)
c2 = np.polyfit(rough, 4.0 * k2, 2)
print("perceptual roughness, k1, k2, shader multipliers 1.5 k1 and 4 k2")
for r, a, b in zip(rough, k1, k2):
    print("  %.2f  %.4f  %.4f   %.4f  %.4f" % (r, a, b, 1.5 * a, 4 * b))
print("\nquadratic fits in perceptual roughness r (highest power first):")
print("  1.5 k1 = %.5f r^2 + %.5f r + %.5f" % tuple(c1))
print("  4 k2   = %.5f r^2 + %.5f r + %.5f" % tuple(c2))
print("  worst fit error: %.5f and %.5f" % (np.max(np.abs(np.polyval(c1, rough) - 1.5 * k1)),
                                            np.max(np.abs(np.polyval(c2, rough) - 4.0 * k2))))
print("  at r = 1: %.4f and %.4f (should be 1 and 1)" % (np.polyval(c1, 1.0), np.polyval(c2, 1.0)))
