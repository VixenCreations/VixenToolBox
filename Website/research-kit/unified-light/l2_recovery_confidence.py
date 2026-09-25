"""Experiment 2, part C2: scale the rebuilt L2 band by a confidence term d^k, where d = |L1| / (2 L0) is how directional the L1 data is (1 for a point light, 0 for a uniform sky), to keep the one-light gain without the many-light loss."""
import math
import numpy as np
import l2_recovery as base

rng = np.random.default_rng(9)
a = base.weights(0.0, 0.0)


def recon_k(L0, L1, n, k):
    out = L0 + n @ L1
    ln = np.linalg.norm(L1)
    if ln > 1e-8 and L0 > 1e-8:
        d = min(ln / (2.0 * L0), 1.0)
        out = out + 0.625 * ln * (d ** k) * base.P2(n @ (L1 / ln))
    return np.maximum(out, 0.0)


print("default shaping, rms error as a share of each scene's peak, mean of 30 scenes")
print("  %-12s %-10s %-10s %-10s %-10s %-10s" % ("scene", "no L2", "k=0", "k=0.5", "k=1", "k=2"))
for kind in ("one sun", "two lights", "six lights"):
    rows = [[], [], [], [], []]
    for _ in range(30):
        rad = base.scene(kind)
        L0, L1 = base.bands(rad)
        t = base.truth(rad, base.NS, 0.0, 0.0)
        pk = max(t.max(), 1e-6)
        g = [np.maximum(L0 + base.NS @ L1, 0.0)] + [recon_k(L0, L1, base.NS, k) for k in (0.0, 0.5, 1.0, 2.0)]
        for j in range(5):
            rows[j].append(math.sqrt(np.mean((g[j] - t) ** 2)) / pk)
    print("  %-12s " % kind + " ".join("%-10.3f" % np.mean(r) for r in rows))
