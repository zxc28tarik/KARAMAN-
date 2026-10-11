"""Objective measurements for rendered Karamani frames (Gate D/E evidence).

Measures what can be measured so reviews separate facts from taste:
  - size / aspect, letterbox margins
  - pixel-grid integrity: share of NxN blocks (N = width / 360) that are one flat colour
  - colour count on the 360x270 game-pixel grid (exact palette when given a raw game buffer,
    i.e. Review/pixel/*.png at 360x270 + margin; final frames also contain HUD/vignette colours)
  - luminance: mean and 5/50/95 percentiles (Rec.601, 0..1)
  - optional --mask actor.png (same size; white/opaque = actor): O5 silhouette separation
    (Weber vs an 8-px ring) and interior legibility (median luminance, P95-P5 spread)
  - optional --box x,y,w,h (source-image pixels, top-left origin): rough fallback when no mask
    exists; the box mixes background into the figure, so it under-reports contrast

Usage:
  python -I Tools/frame_metrics.py Review/player-chapter-1.png [more.png ...]
  python -I Tools/frame_metrics.py Review/player-chapter-1.png --box 450,480,90,240
  python -I Tools/frame_metrics.py Review/*.png --json      (machine-readable)
"""
import argparse
import glob
import json
import os
import sys

import numpy as np
from PIL import Image

GAME_W, GAME_H = 360, 270


def luminance(rgb):
    return rgb[..., 0] * .299 + rgb[..., 1] * .587 + rgb[..., 2] * .114


def letterbox(rgb):
    """Columns/rows at the edges that are pure black (aspect margins)."""
    dark = rgb.max(axis=2) < 3 / 255
    cols = dark.all(axis=0)
    rows = dark.all(axis=1)
    left = int(np.argmin(cols)) if not cols.all() else rgb.shape[1]
    right = int(np.argmin(cols[::-1])) if not cols.all() else 0
    top = int(np.argmin(rows)) if not rows.all() else rgb.shape[0]
    bottom = int(np.argmin(rows[::-1])) if not rows.all() else 0
    return left, right, top, bottom


def grid_metrics(rgb):
    h, w = rgb.shape[:2]
    margin = (w - GAME_W) // 2
    if 0 <= margin <= 4 and w == GAME_W + 2 * margin and h == GAME_H + 2 * margin:
        # raw game buffer (Review/pixel/*.png): the true palette before HUD, vignette and sub-pixel scroll
        q = (rgb[margin:h - margin or h, margin:w - margin or w] * 255 + .5).astype(np.int32)
        packed = q[..., 0] << 16 | q[..., 1] << 8 | q[..., 2]
        return {'gridScale': 1, 'flatBlockShare': 1.0, 'gameColours': int(np.unique(packed).size), 'rawBuffer': True}
    n = w // GAME_W
    if n < 1 or w % GAME_W or h != GAME_H * n:
        return {'gridScale': None, 'flatBlockShare': None, 'gameColours': None}
    q = (rgb * 255 + .5).astype(np.int32)
    blocks = q.reshape(GAME_H, n, GAME_W, n, 3)
    flat = (blocks == blocks[:, :1, :, :1, :]).all(axis=(1, 3, 4))
    centres = q[n // 2::n, n // 2::n]
    packed = centres[..., 0] << 16 | centres[..., 1] << 8 | centres[..., 2]
    return {'gridScale': n, 'flatBlockShare': round(float(flat.mean()), 4),
            'gameColours': int(np.unique(packed).size)}


def box_contrast(rgb, box):
    x, y, w, h = box
    H, W = rgb.shape[:2]
    pad = max(w, h) // 2
    inner = np.zeros((H, W), bool)
    inner[y:y + h, x:x + w] = True
    outer = np.zeros((H, W), bool)
    outer[max(0, y - pad):min(H, y + h + pad), max(0, x - pad):min(W, x + w + pad)] = True
    ring = outer & ~inner
    lum = luminance(rgb)
    li, lr = float(lum[inner].mean()), float(lum[ring].mean())
    ci, cr = rgb[inner].mean(axis=0), rgb[ring].mean(axis=0)
    # Weber-style contrast of the region against its surround, and mean RGB distance
    return {'box': list(box), 'boxLuminance': round(li, 4), 'surroundLuminance': round(lr, 4),
            'weberContrast': round((li - lr) / max(lr, 1e-3), 3),
            'rgbDistance': round(float(np.linalg.norm(ci - cr)), 4),
            'boxP95Luminance': round(float(np.percentile(lum[inner], 95)), 4)}


def dilate(mask, radius):
    out = mask.copy()
    for _ in range(radius):
        grown = out.copy()
        grown[1:] |= out[:-1]; grown[:-1] |= out[1:]; grown[:, 1:] |= out[:, :-1]; grown[:, :-1] |= out[:, 1:]
        out = grown
    return out


def mask_metrics(rgb, mask_path, ring_px=8):
    """O5 on a silhouette mask (white or opaque = actor): separation from the ring around it, and
    whether the figure's interior has enough mid/high tones to read (not a black cut-out)."""
    m = np.asarray(Image.open(mask_path).convert('RGBA'))
    inside = (m[..., 3] > 127) & (m[..., :3].max(axis=2) > 127)
    if inside.shape != rgb.shape[:2]:
        raise SystemExit(f'mask {m.shape[1]}x{m.shape[0]} does not match image {rgb.shape[1]}x{rgb.shape[0]}')
    ring = dilate(inside, ring_px) & ~inside
    lum = luminance(rgb)
    li, lr = float(lum[inside].mean()), float(lum[ring].mean())
    p5, p50, p95 = (float(v) for v in np.percentile(lum[inside], [5, 50, 95]))
    return {'mask': mask_path.replace('\\', '/'), 'figurePixels': int(inside.sum()), 'ringPx': ring_px,
            'silhouetteWeber': round((li - lr) / max(lr, 1e-3), 3),
            'rgbDistance': round(float(np.linalg.norm(rgb[inside].mean(axis=0) - rgb[ring].mean(axis=0))), 4),
            'figureMedianLuminance': round(p50, 4), 'figureSpreadP95_P5': round(p95 - p5, 4),
            'figureP5_P50_P95': [round(p5, 4), round(p50, 4), round(p95, 4)]}


def measure(path, box=None, mask=None):
    rgb = np.asarray(Image.open(path).convert('RGB'), dtype=np.float32) / 255
    h, w = rgb.shape[:2]
    left, right, top, bottom = letterbox(rgb)
    content = rgb[top:h - bottom or h, left:w - right or w] if (left + right < w and top + bottom < h) else rgb
    lum = luminance(content)
    result = {'file': os.path.relpath(path).replace('\\', '/'), 'size': [w, h],
              'aspect': round(w / h, 4), 'letterbox': {'left': left, 'right': right, 'top': top, 'bottom': bottom},
              'contentAspect': round(content.shape[1] / content.shape[0], 4),
              'meanLuminance': round(float(lum.mean()), 4),
              'luminanceP5_P50_P95': [round(float(v), 4) for v in np.percentile(lum, [5, 50, 95])],
              'meanSaturation': round(float((content.max(axis=2) - content.min(axis=2)).mean()), 4)}
    result.update(grid_metrics(content))
    if box:
        result['readability'] = box_contrast(rgb, box)
    if mask:
        result['silhouette'] = mask_metrics(rgb, mask)
    return result


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('images', nargs='+')
    ap.add_argument('--box', help='x,y,w,h region in source pixels (top-left origin); approximate, mixes in background')
    ap.add_argument('--mask', help='PNG of the same size, white/opaque = actor silhouette (preferred for O5)')
    ap.add_argument('--json', action='store_true')
    args = ap.parse_args()
    box = tuple(int(v) for v in args.box.split(',')) if args.box else None
    paths = [p for pattern in args.images for p in (glob.glob(pattern) or [pattern])]
    results = []
    for p in paths:
        if not os.path.isfile(p):
            print(f'MISSING: {p}', file=sys.stderr)
            continue
        results.append(measure(p, box, args.mask))
    if args.json:
        print(json.dumps(results, indent=1))
        return
    for r in results:
        print(f"{r['file']}: {r['size'][0]}x{r['size'][1]} content-aspect {r['contentAspect']} "
              f"lum mean {r['meanLuminance']} p5/50/95 {r['luminanceP5_P50_P95']} sat {r['meanSaturation']} "
              f"grid x{r['gridScale']} flat {r['flatBlockShare']} colours {r['gameColours']}")
        if 'readability' in r:
            print(f"  box {r['readability']}")
        if 'silhouette' in r:
            print(f"  mask {r['silhouette']}")


if __name__ == '__main__':
    main()
