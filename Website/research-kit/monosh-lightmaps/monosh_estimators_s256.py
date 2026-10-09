"""MonoSH ratio lightmaps: the per-texel estimators at 256 samples per texel and 100 texels per scene (Table 21, S = 256)."""
from monosh_estimators import run

if __name__ == "__main__":
    run(S=256, trials=100)
