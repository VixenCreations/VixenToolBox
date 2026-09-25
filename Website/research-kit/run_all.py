"""Run the One Light, Every Route research scripts and compare each result with the published log.

Usage:
    python run_all.py                 run every experiment
    python run_all.py rim-lobe        run one experiment folder
    python run_all.py --list          list the experiments and their scripts

Needs Python 3.8 or newer and NumPy. Each script's output is saved to <experiment>/output/<script>.txt and
compared with <experiment>/expected/<script>.txt, allowing for rounding in the last printed digit.
"""
import os
import re
import subprocess
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))

EXPERIMENTS = [
    ("rim-lobe", "Section 4, the rim as a light lobe", [
        "rim_stack_check.py", "rim_shape_check2.py", "rim_lobe_research.py", "rim_asbuilt_check.py",
        "verify_nonlinear_sh_2026-09-12.py"]),
    ("unified-light", "Section 5, one light on every route", [
        "route_mismatch.py", "route_match.py", "l2_recovery.py", "l2_recovery_confidence.py"]),
    ("camera-look", "Section 6, what the camera keeps", [
        "route_highlights.py", "camera_chain.py", "light_size_highlight.py"]),
    ("baker-hd", "Section 7, baking at high definition", [
        "baker_sampling.py"]),
    ("nextgen-lighting", "Section 8, toward the target look", [
        "light_units.py", "aniso_reflection.py", "probe_normalization.py", "sh_band_fit.py", "sh_specular.py",
        "lightmap_encoding.py"]),
    ("lv-lights", "Section 9, Light Volume lights one at a time", [
        "lv_light_shapes.py", "lv_light_shaping.py", "lv_asbuilt_check.py"]),
]

NUM = re.compile(r"[-+]?(?:\d+\.\d*|\.\d+|\d+)(?:[eE][-+]?\d+)?")


def split_line(line):
    nums = NUM.findall(line)
    return NUM.sub("#", line).strip(), nums


def close(a, b):
    try:
        x, y = float(a), float(b)
    except ValueError:
        return a == b
    decimals = max(len(a.split(".")[1].split("e")[0].split("E")[0]) if "." in a else 0,
                   len(b.split(".")[1].split("e")[0].split("E")[0]) if "." in b else 0)
    step = 10.0 ** (-decimals) if "e" not in a.lower() and "e" not in b.lower() else 0.0
    return abs(x - y) <= max(1.5 * step, 0.005 * max(abs(x), abs(y)), 1e-9)


def compare(got, want):
    g = [l for l in got.splitlines() if l.strip()]
    w = [l for l in want.splitlines() if l.strip()]
    if len(g) != len(w):
        return False, f"{len(g)} lines, expected {len(w)}"
    for i, (a, b) in enumerate(zip(g, w)):
        sa, na = split_line(a)
        sb, nb = split_line(b)
        if sa != sb or len(na) != len(nb) or not all(close(p, q) for p, q in zip(na, nb)):
            return False, f"line {i + 1} differs:\n      yours:    {a.strip()}\n      expected: {b.strip()}"
    return True, ""


def run(folder, scripts, update):
    base = os.path.join(HERE, folder)
    os.makedirs(os.path.join(base, "output"), exist_ok=True)
    results = []
    for s in scripts:
        t0 = time.time()
        p = subprocess.run([sys.executable, s], cwd=base, capture_output=True, text=True)
        dt = time.time() - t0
        name = os.path.splitext(s)[0] + ".txt"
        open(os.path.join(base, "output", name), "w", encoding="utf-8").write(p.stdout)
        if p.returncode != 0:
            results.append((s, "ERROR", dt, p.stderr.strip().splitlines()[-1] if p.stderr.strip() else "exit " + str(p.returncode)))
            continue
        exp_path = os.path.join(base, "expected", name)
        if update:
            os.makedirs(os.path.dirname(exp_path), exist_ok=True)
            open(exp_path, "w", encoding="utf-8").write(p.stdout)
            results.append((s, "SAVED", dt, ""))
        elif not os.path.exists(exp_path):
            results.append((s, "NO LOG", dt, "no expected/" + name + " to compare with"))
        else:
            ok, why = compare(p.stdout, open(exp_path, encoding="utf-8").read())
            results.append((s, "MATCH" if ok else "DIFFERS", dt, why))
    return results


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    if "--list" in sys.argv:
        for folder, title, scripts in EXPERIMENTS:
            print(f"{folder:18s} {title}")
            for s in scripts:
                print(f"    {s}")
        return 0
    try:
        import numpy
    except ImportError:
        print("These scripts need NumPy. Install it with:  python -m pip install numpy")
        return 1
    chosen = [e for e in EXPERIMENTS if not args or e[0] in args]
    if not chosen:
        print("Unknown experiment. Run with --list to see the names.")
        return 1
    print(f"Python {sys.version.split()[0]}, NumPy {numpy.__version__}")
    bad = 0
    for folder, title, scripts in chosen:
        print(f"\n{title}  ({folder})")
        for s, state, dt, why in run(folder, scripts, "--update-expected" in sys.argv):
            print(f"  {state:8s} {s:36s} {dt:6.1f} s")
            if why:
                print("      " + why)
            bad += state in ("ERROR", "DIFFERS")
    print("\nAll results match the published logs." if not bad else f"\n{bad} script(s) did not match. Their output is in the output folders.")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
