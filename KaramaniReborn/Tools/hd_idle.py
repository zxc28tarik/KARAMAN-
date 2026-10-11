"""HD karakter (256 px) için bekleme animasyonu: tek parça görüntünün bütün olarak hafifçe bükülmesi.

Parça ayrımı yoktur: her kare, kaynak görüntünün en yakın komşu ters eşlemesiyle yeniden örneklenmiş tek bir
çizimdir (pikseller bulanıklaşmaz). İki hareket üst üste biner:
  - nefes: kalçanın üstündeki satırlar yukarı kalkar (başta en çok, kalçada sıfır)
  - kumaş salınımı: gövdenin arkasındaki pelerin / atkı / palto eteği yatay dalgalanır; uzaklaştıkça ve
    aşağı indikçe genlik artar (rüzgâr)
Kullanım: python -I Tools/hd_idle.py   → Review/characters/from_art/idle/<ad>-NN.png, idle-all.gif
"""
import math
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, 'Review', 'characters', 'from_art')
OUT = os.path.join(SRC, 'idle')

# karakter başına: kumaşın başladığı gövde çizgisi (x, bu sütunun solu salınır), omuz ve kalça satırı (oran),
# nefes genliği, salınım genliği (piksel)
CFG = {
    'karamani-profile': dict(cloth_x=.42, shoulder=.24, hip=.50, breath=1.6, sway=2.6),
    'karamani': dict(cloth_x=.36, shoulder=.22, hip=.50, breath=1.6, sway=2.6),
    'ludwig':   dict(cloth_x=.32, shoulder=.20, hip=.50, breath=1.4, sway=2.2),
    'almos':    dict(cloth_x=.30, shoulder=.18, hip=.48, breath=1.4, sway=2.6),
    'kutluca':  dict(cloth_x=.40, shoulder=.24, hip=.52, breath=1.2, sway=3.0),
    'kismet':   dict(cloth_x=.34, shoulder=.22, hip=.50, breath=1.4, sway=2.6),
}
FRAMES = 8


def frame(img, cfg, phase):
    h, w = img.shape[:2]
    ys, xs = np.mgrid[0:h, 0:w].astype(np.float64)
    hip, shoulder, top = cfg['hip'] * h, cfg['shoulder'] * h, 0.0
    # nefes: kalçanın üstü yukarı (çıktı satırı y, kaynaktan y + d alır)
    lift = cfg['breath'] * (.5 - .5 * math.cos(phase))
    d = lift * np.clip((hip - ys) / (hip - top), 0, 1)
    sy = ys + d
    # kumaş salınımı: gövde çizgisinin solunda, omuzdan aşağı
    cloth_x = cfg['cloth_x'] * w
    away = np.clip((cloth_x - xs) / (w * .3), 0, 1)
    down = np.clip((ys - shoulder) / (h - shoulder), 0, 1)
    wave = np.sin(phase + ys * .045)
    sx = xs - cfg['sway'] * wave * away * (.35 + .65 * down)
    sx = np.clip(np.rint(sx), 0, w - 1).astype(int); sy = np.clip(np.rint(sy), 0, h - 1).astype(int)
    return img[sy, sx]


def main():
    os.makedirs(OUT, exist_ok=True)
    import sys
    names = sys.argv[1:] or list(CFG)
    def path(n):
        for suf in ('256', '258'):
            q = os.path.join(SRC, f'{n}-{suf}.png')
            if os.path.exists(q):
                return q
    sprites = {n: np.asarray(Image.open(path(n)).convert('RGBA')) for n in names}
    seqs = {}
    for n in names:
        seqs[n] = []
        for i in range(FRAMES):
            f = frame(sprites[n], CFG[n], 2 * math.pi * i / FRAMES)
            Image.fromarray(f).save(os.path.join(OUT, f'{n}-{i:02d}.png'))
            seqs[n].append(f)
    # tüm karakterler yan yana, döngü GIF'i (2× büyütülmüş, oyun zemini rengi)
    pad = 10; hmax = max(s.shape[0] for s in sprites.values()); wsum = sum(s.shape[1] for s in sprites.values()) + pad * (len(names) + 1)
    gif = []
    for i in range(FRAMES):
        canvas = np.zeros((hmax + 2 * pad, wsum, 3), np.uint8); canvas[:] = (58, 40, 64)
        x = pad
        for n in names:
            f = seqs[n][i]; y = hmax + pad - f.shape[0]; a = f[..., 3:4] / 255.0
            canvas[y:y + f.shape[0], x:x + f.shape[1]] = (f[..., :3] * a + canvas[y:y + f.shape[0], x:x + f.shape[1]] * (1 - a)).astype(np.uint8)
            x += f.shape[1] + pad
        gif.append(Image.fromarray(canvas).resize((canvas.shape[1] * 2, canvas.shape[0] * 2), Image.NEAREST))
    path = os.path.join(SRC, 'idle-' + ('all' if len(names) > 1 else names[0]) + '.gif')
    gif[0].save(path, save_all=True, append_images=gif[1:], duration=120, loop=0)
    print(path)


if __name__ == '__main__':
    main()
