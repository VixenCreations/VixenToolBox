"""Second pass on the rim redesign: exact Wrap endpoints in float32, the kink of the old linear wrap, and the Width band profile."""
import math
import numpy as np
f = np.float32

def cap_lit(c, wrap):
    sig = f(min(max(wrap, 0.0), 1.0)) * f(math.pi * 0.5)
    s2 = f(np.sin(sig)) ** 2
    s2 = min(s2, f(0.999999))
    c = f(min(max(c, -1.0), 1.0))
    if c * c >= s2 or s2 < f(1e-6):
        return float(max(c, f(0)))
    sinT = f(np.sqrt(max(f(1) - c * c, f(1e-6))))
    x = f(np.sqrt(f(1) / s2 - f(1)))
    y = f(-x * c / sinT)
    sy = f(sinT * np.sqrt(max(f(0), f(1) - y * y)))
    e = (c * f(np.arccos(min(max(y, f(-1)), f(1)))) - x * sy) * s2 + f(np.arctan(sy / x))
    return float(min(max(e / (f(math.pi) * s2), f(0)), f(1)))

degs = np.arange(0, 180.01, 0.25)
lam = max(abs(cap_lit(math.cos(math.radians(d)), 0.0) - max(math.cos(math.radians(d)), 0)) for d in degs)
half = max(abs(cap_lit(math.cos(math.radians(d)), 1.0) - (1 + math.cos(math.radians(d))) / 2) for d in degs)
print(f"float32: wrap 0 vs Lambert max diff {lam:.2e}; wrap 1 vs half Lambert max diff {half:.2e}")
nan = any(not math.isfinite(cap_lit(math.cos(math.radians(d)), w)) for d in degs for w in np.linspace(0, 1, 41))
print("any non-finite over 41 wraps x 721 angles:", nan)

def slopes(fn, deg, h=0.05):
    l = (fn(deg) - fn(deg - h)) / h
    r = (fn(deg + h) - fn(deg)) / h
    return l, r
old = lambda d: min(max((math.cos(math.radians(d)) + 0.5) / 1.5, 0), 1)
new = lambda d: cap_lit(math.cos(math.radians(d)), 0.5)
print("old wrap 0.5 slope either side of 120 deg: %.5f / %.5f per degree" % slopes(old, 120.0))
print("new wrap 0.5 slope either side of 135 deg: %.5f / %.5f per degree" % slopes(new, 135.0))
print("new wrap 0.5 slope either side of  45 deg: %.5f / %.5f per degree" % slopes(new, 45.0))

def band(u, width, blur):
    if width <= 0:
        return 0.0
    lo = width * (1 - blur)
    t = min(max((u - lo) / max(width - lo, 1e-4), 0), 1)
    return 1 - t * t * (3 - 2 * t)
print("\nWidth 4 (outer 40%), rim value by distance in from the outline, as a share of the limb radius")
print("  in:     " + "  ".join(f"{u:4.2f}" for u in (0, .05, .1, .15, .2, .25, .3, .35, .4)))
for blur in (0.0, 0.5, 1.0):
    print(f"  blur {blur}: " + "  ".join(f"{band(u, 0.4, blur):4.2f}" for u in (0, .05, .1, .15, .2, .25, .3, .35, .4)))
