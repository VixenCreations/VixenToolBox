"""MonoSH ratio lightmaps: the per-texel estimators at 64 samples per texel and 150 texels per scene (Table 21, S = 64)."""
from monosh_estimators import run

if __name__ == "__main__":
    run(S=64, trials=150)
