# One Light, Every Route: research kit

The scripts behind every table in our lighting paper, [One Light, Every Route](https://vixencreations.github.io/VixenToolBox/research.html). They're the same scripts we ran, and each one prints the numbers the paper reports.

## What you need

- Python 3.8 or newer
- NumPy: `python -m pip install numpy`

Nothing else. No Unity, no GPU, no data files.

## Run everything

```
python run_all.py
```

It runs each experiment's scripts in order, saves what they print to `<experiment>/output/`, and compares it with our logs in `<experiment>/expected/`. A small rounding difference in the last digit still counts as a match. The full run takes about 11 minutes on a desktop CPU; `unified-light/route_match.py` is most of that.

Run one experiment: `python run_all.py lv-lights`. See the names: `python run_all.py --list`.

## Run one script

```
cd rim-lobe
python rim_lobe_research.py
```

Each script prints a report. The scripts in `lv-lights` import each other, so run them from inside that folder, and they also save their report next to themselves.

## The folders

| Folder | Paper section |
|---|---|
| `rim-lobe` | 4. The rim as a light lobe |
| `unified-light` | 5. One light on every route |
| `camera-look` | 6. What the camera keeps |
| `baker-hd` | 7. Baking at high definition |
| `nextgen-lighting` | 8. Toward the target look |
| `lv-lights` | 9. Light Volume lights, one at a time |
