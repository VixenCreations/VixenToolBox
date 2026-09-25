"""Experiment 5: the baker's light conversion against Unity's lightmapper in a Built-in project (Lightmapping.cs LightmapperUtils: LinearColor.Convert with lightsUseLinearIntensity off, FalloffType.Legacy): intensity, distance falloff and the bounce multiplier."""
import numpy as np


def srgb_to_linear(c):
    c = np.asarray(c, dtype=np.float64)
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


W = np.array([0.2126, 0.7152, 0.0722])
print("1. Intensity. Unity (Built-in, lightsUseLinearIntensity off): (color * intensity).linear")
print("   Baker today: color.linear * intensity")
for name, col in (("white", (1.0, 1.0, 1.0)), ("warm 1.0/0.8/0.6", (1.0, 0.8, 0.6))):
    print("   %s" % name)
    for inten in (0.25, 0.5, 1.0, 1.5, 2.0, 4.0, 8.0):
        unity = srgb_to_linear(np.array(col) * inten) @ W
        ours = (srgb_to_linear(np.array(col)) * inten) @ W
        print("     intensity %-5s Unity %8.3f   baker %8.3f   baker / Unity %.2f" % (inten, unity, ours, ours / unity))

print("\n2. Distance falloff, range 10 m, light of 1")
print("   Unity Legacy (the Built-in default): 1 / (1 + 25 (d/r)^2), the vertex-light form of the attenuation texture")
print("   (the texture also fades to 0 at the range; that fade is not reproduced here)")
print("   Baker today: (1 - (d/r)^4)^2 / (d^2 + 1)")
print("   Inverse square with Unity's smooth window, for reference: (1 - (d/r)^4)^2 / d^2")
r = 10.0
for d in (0.25, 0.5, 1.0, 2.0, 3.0, 5.0, 7.0, 9.0):
    x = d / r
    legacy = 1.0 / (1.0 + 25.0 * x * x)
    ours = (1.0 - x ** 4) ** 2 / (d * d + 1.0)
    inv = (1.0 - x ** 4) ** 2 / (d * d)
    print("   d %-5s Legacy %.4f   baker %.4f (%.2fx)   inverse square %.4f" % (d, legacy, ours, ours / legacy, inv))

print("\n3. Bounce. Unity: indirect colour = direct colour * Light.bounceIntensity (Indirect Multiplier).")
print("   Baker today: every light bounces at 1. A light at Indirect Multiplier 0 still bounces fully; one at 2 bounces half as much as in Unity.")

print("\n4. Soft shadows. Unity: sphereRadius = shadowRadius only when Shadow Type is Soft (angularDiameter the same for")
print("   directional). Baker today: uses shadowRadius / shadowAngle for any shadowed light, so a Hard-shadow light bakes soft.")
