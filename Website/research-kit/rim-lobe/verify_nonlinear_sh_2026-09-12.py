"""Check the Geomerics non-linear L1 spherical harmonics reconstruction before it goes into HLSL: that it returns the ambient term when the probe is uniform, that it never goes negative, that it integrates over the sphere to the same energy as the linear reconstruction, and that it rises monotonically toward the dominant light direction."""

import math

SAMPLES = 20000


def sphere_points(n):
    # Fibonacci sphere, an even distribution without pole clustering
    pts = []
    ga = math.pi * (3.0 - math.sqrt(5.0))
    for i in range(n):
        z = 1.0 - (2.0 * i + 1.0) / n
        r = math.sqrt(max(0.0, 1.0 - z * z))
        th = ga * i
        pts.append((r * math.cos(th), r * math.sin(th), z))
    return pts


PTS = sphere_points(SAMPLES)


def linear(L0, L1, N):
    return L0 + (L1[0] * N[0] + L1[1] * N[1] + L1[2] * N[2])


def nonlinear(L0, L1, N):
    if L0 <= 1e-8:
        return 0.0
    lenR1 = math.sqrt(L1[0] ** 2 + L1[1] ** 2 + L1[2] ** 2)
    if lenR1 <= 1e-8:
        return L0
    rr = min(lenR1 / L0, 1.0)
    d = (L1[0] * N[0] + L1[1] * N[1] + L1[2] * N[2]) / lenR1
    q = 0.5 * (1.0 + d)
    q = max(q, 0.0)
    p = 1.0 + 2.0 * rr
    a = (1.0 - rr) / (1.0 + rr)
    return L0 * (a + (1.0 - a) * (p + 1.0) * (q ** p))


def integrate(fn, L0, L1):
    return sum(fn(L0, L1, N) for N in PTS) / len(PTS)


print("check 1: a uniform probe returns the ambient term")
worst = 0.0
for L0 in (0.05, 0.5, 1.0, 4.0):
    for N in PTS[:200]:
        worst = max(worst, abs(nonlinear(L0, (0.0, 0.0, 0.0), N) - L0))
print("   largest error against L0: %.3e" % worst)
print("   PASS" if worst < 1e-9 else "   FAIL")

print()
print("check 2: never negative, where the linear form does go negative")
neg_nl = 0
neg_lin = 0
cases = []
for rr in [0.0, 0.25, 0.5, 0.75, 1.0, 1.5, 2.0]:
    L0 = 1.0
    L1 = (rr, 0.0, 0.0)
    for N in PTS:
        if nonlinear(L0, L1, N) < -1e-9:
            neg_nl += 1
        if linear(L0, L1, N) < -1e-9:
            neg_lin += 1
    cases.append(rr)
print("   ratios tested: %s" % cases)
print("   negative samples, non-linear: %d" % neg_nl)
print("   negative samples, linear:     %d" % neg_lin)
print("   PASS" if neg_nl == 0 else "   FAIL")

print()
print("check 3: energy over the sphere matches the linear form")
print("   %-8s %-14s %-14s %s" % ("ratio", "linear mean", "non-linear", "error"))
ok = True
for rr in (0.0, 0.2, 0.4, 0.6, 0.8, 1.0):
    L0 = 1.0
    L1 = (0.0, 0.0, rr)
    a = integrate(linear, L0, L1)
    b = integrate(nonlinear, L0, L1)
    err = abs(a - b)
    if err > 2e-3:
        ok = False
    print("   %-8.2f %-14.6f %-14.6f %.2e" % (rr, a, b, err))
print("   PASS" if ok else "   FAIL")

print()
print("check 4: peak and shape against the linear form")
print("   %-8s %-12s %-12s %-12s %s" % ("ratio", "lin at peak", "nl at peak", "lin opposite", "nl opposite"))
for rr in (0.0, 0.25, 0.5, 0.75, 1.0):
    L0 = 1.0
    L1 = (0.0, 0.0, rr)
    up = (0.0, 0.0, 1.0)
    dn = (0.0, 0.0, -1.0)
    print("   %-8.2f %-12.4f %-12.4f %-12.4f %.4f"
          % (rr, linear(L0, L1, up), nonlinear(L0, L1, up),
             linear(L0, L1, dn), nonlinear(L0, L1, dn)))

print()
print("check 5: monotonic from the dark side to the lit side")
bad = 0
for rr in (0.2, 0.5, 0.9):
    L0 = 1.0
    L1 = (0.0, 0.0, rr)
    prev = None
    for i in range(801):
        c = -1.0 + 2.0 * i / 800.0
        s = math.sqrt(max(0.0, 1.0 - c * c))
        v = nonlinear(L0, L1, (s, 0.0, c))
        if prev is not None and v < prev - 1e-12:
            bad += 1
        prev = v
print("   decreasing steps found: %d" % bad)
print("   PASS" if bad == 0 else "   FAIL")
