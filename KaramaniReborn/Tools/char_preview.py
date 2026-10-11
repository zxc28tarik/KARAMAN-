"""Hızlı karakter önizlemesi: tüm sayfayı üretmeden seçili kareleri çizer.

Üst sıra: üreticinin ham çıktısı (nötr gri zemin). Alt sıra: aynı kareler bölge paletine
(PixelPipeline.cs → PixelLook.For) en yakın OKLab rengiyle indirgenmiş hâli, oyundaki görünüme yakın.
Büyütme en yakın komşu; her oyun pikseli SCALE×SCALE kare.

Kullanım:
  python -I Tools/char_preview.py hero idle:0 run:2 light1:3 heavy:5 --zone 0 --out Review/characters/preview.png
"""
import argparse
import math
import os
import re
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pixel_characters as pc  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def srgb_to_linear(c):
    c = c / 255.0
    return np.where(c <= .04045, c / 12.92, ((c + .055) / 1.055) ** 2.4)


def oklab(rgb255):
    c = srgb_to_linear(np.asarray(rgb255, np.float64))
    l = .4122214708 * c[..., 0] + .5363325363 * c[..., 1] + .0514459929 * c[..., 2]
    m = .2119034982 * c[..., 0] + .6806995451 * c[..., 1] + .1073969566 * c[..., 2]
    s = .0883024619 * c[..., 0] + .2817188376 * c[..., 1] + .6299787005 * c[..., 2]
    l, m, s = np.cbrt(l), np.cbrt(m), np.cbrt(s)
    return np.stack([.2104542553 * l + .7936177850 * m - .0040720468 * s,
                     1.9779984951 * l - 2.4285922050 * m + .4505937099 * s,
                     .0259040371 * l + .7827717662 * m - .8086757660 * s], -1)


def zone_look(zone):
    """PixelPipeline.cs → PixelLook.For: ana rampa + vurgu rampaları (cezalarıyla), renk ayarı ve tonlar."""
    src = open(os.path.join(ROOT, 'Assets', 'Scripts', 'PixelPipeline.cs'), encoding='utf-8').read()
    accents = re.findall(r'\("([0-9a-f]{6})", ([0-9.]+)f\)', src)
    ramps = re.findall(r'ramp = new\[\] \{([^}]*)\}', src)
    grades = re.findall(r'grade = new Vector4\(([^)]*)\), shadowTint = new Color\(([^)]*)\), lightTint = new Color\(([^)]*)\)', src)
    z = min(zone, len(ramps) - 1)
    main = re.findall(r'"([0-9a-f]{6})"', ramps[z])
    hexes = [(h, 0.0) for h in main] + [(h, float(b)) for h, b in accents]
    rgb = np.array([[int(h[i:i + 2], 16) for i in (0, 2, 4)] for h, _ in hexes], np.float64)
    nums = lambda t: [float(v.strip().rstrip('f')) for v in t.split(',')]
    g, st, lt = (nums(t) for t in grades[z])
    return rgb, np.array([b for _, b in hexes]), g, st, lt


def zone_palette(zone):
    rgb, bias, _, _, _ = zone_look(zone)
    return rgb, bias


def _sm(a, b, x):
    t = np.clip((x - a) / (b - a), 0, 1); return t * t * (3 - 2 * t)


def grade(rgb01, g, st, lt):
    """PixelPost.shader renk ayarı: pozlama, doygunluk (parlak doygunlar korunur), kontrast, gölge/ışık tonu."""
    c = rgb01 * g[0]
    luma = c @ np.array([.299, .587, .114])
    chroma = c.max(-1) - c.min(-1)
    keep = _sm(.42, .72, chroma) * _sm(.55, .88, luma)
    c = luma[..., None] + (c - luma[..., None]) * (g[1] + (1.1 - g[1]) * keep)[..., None]
    c = (c - g[3]) * g[2] + g[3]
    luma = np.clip(c @ np.array([.299, .587, .114]), 0, 1)
    tint = np.array(st) + (np.array(lt) - np.array(st)) * _sm(.04, .7, luma)[..., None]
    return np.clip(c * tint, 0, 1)


def quantize(rgba, zone, backdrop):
    """Oyundaki son görünüme yakın: renk ayarı + OKLab en yakın palet rengi (kroma ağırlığı 1.1, vurgu cezası)."""
    rgb, bias, g, st, lt = zone_look(zone)
    lab_pal = oklab(rgb)
    img = rgba[..., :3].astype(np.float64)
    a = rgba[..., 3:4] / 255.0
    comp = (img * a + np.asarray(backdrop, np.float64) * (1 - a)) / 255.0
    lab = oklab(grade(comp, g, st, lt) * 255.0)
    e = lab[..., None, :] - lab_pal[None, None, :, :]
    d = e[..., 0] ** 2 + 1.1 * (e[..., 1] ** 2 + e[..., 2] ** 2) + bias[None, None, :]
    return rgb[d.argmin(-1)].astype(np.uint8)


def render(variant, picks):
    ramps = pc.ramps_for(variant)
    anims, _, _ = pc.make_animations(variant)
    frames = []
    for pick in picks:
        name, _, idx = pick.partition(':')
        pose, _extra = anims[name][int(idx or 0)]
        rgba, _meta, _sk = pc.render_pose(pose, variant, ramps)
        frames.append(rgba)
    return frames


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('variant')
    ap.add_argument('picks', nargs='+', help='clip:frame, e.g. idle:0 light1:3')
    ap.add_argument('--zone', type=int, default=0)
    ap.add_argument('--scale', type=int, default=6)
    ap.add_argument('--crop', default='24,16,104,96', help='x0,y0,x1,y1 inside the 128 px frame')
    ap.add_argument('--out', default=os.path.join(ROOT, 'Review', 'characters', 'preview.png'))
    args = ap.parse_args()
    x0, y0, x1, y1 = (int(v) for v in args.crop.split(','))
    frames = render(args.variant, args.picks)
    w, h = x1 - x0, y1 - y0
    neutral = (58, 64, 78)
    backdrop = {0: (78, 52, 82), 1: (52, 46, 46), 2: (40, 56, 70), 3: (110, 106, 120)}.get(args.zone, neutral)
    sheet = np.zeros((h * 2, w * len(frames), 3), np.uint8)
    for i, f in enumerate(frames):
        f = f[y0:y1, x0:x1]
        a = f[..., 3:4] / 255.0
        sheet[:h, i * w:(i + 1) * w] = (f[..., :3] * a + np.asarray(neutral) * (1 - a)).astype(np.uint8)
        sheet[h:, i * w:(i + 1) * w] = quantize(f, args.zone, backdrop)
    img = Image.fromarray(sheet).resize((sheet.shape[1] * args.scale, sheet.shape[0] * args.scale), Image.NEAREST)
    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    img.save(args.out)
    print(args.out, img.size)


if __name__ == '__main__':
    main()
