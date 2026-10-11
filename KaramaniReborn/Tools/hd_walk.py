"""Deneme: tek HD resimden (256 px Karamani) yürüyüş döngüsü — doğrusal karışımlı deri bükme (skinning).

Resim parçalara bölünüp ayrı ayrı döndürülmez: her bacak bölgesi, kemiklere uzaklığa göre yumuşak ağırlıklarla
bükülür (kalça, diz) ve ters eşleme sabit nokta yinelemesiyle bulunur; en yakın komşu örnekleme pikselleri
keskin tutar. Bacaklar üst üste binerken karışmasın diye arka bacak, gövde ve ön bacak ayrı katmanlarda bükülüp
birleştirilir; çıktı her karede tek parça bir resimdir. Yere basan ayak zeminde kalır (gövde iner / kalkar),
kaftan etekleri uyluğu biraz izler, pelerin rüzgârda salınır.

Kullanım: python -I Tools/hd_walk.py  → Review/characters/from_art/walk/karamani-NN.png, walk-karamani.gif
"""
import math
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, 'Review', 'characters', 'from_art')
OUT = os.path.join(SRC, 'walk')

# Karamani (222×258) dinlenme iskeleti: kalça, diz, ayak bileği; bacak katmanlarının çokgenleri
LEGS = {
    'far':  dict(hip=(72, 165), knee=(63, 208), ankle=(60, 246),
                 poly=[(40, 192), (90, 192), (90, 232), (84, 258), (34, 258), (38, 225)]),
    'near': dict(hip=(114, 165), knee=(122, 208), ankle=(126, 246),
                 poly=[(97, 192), (143, 192), (150, 232), (154, 258), (100, 258), (98, 225)]),
}
PANELS = {'far': (40, 96, 150, 194), 'near': (96, 146, 150, 194)}     # kaftan etekleri (x0, x1, y0, y1)
CLOTH_X, SHOULDER = .30, 60                                            # pelerin: bu sütunun solu, omuzdan aşağı
FRAMES = 8


def inside_poly(xs, ys, pts):
    inside = np.zeros(xs.shape, bool)
    n = len(pts)
    for i in range(n):
        x1, y1 = pts[i]; x2, y2 = pts[(i + 1) % n]
        if y1 == y2:
            continue
        inside ^= ((y1 > ys) != (y2 > ys)) & (xs < (x2 - x1) * (ys - y1) / (y2 - y1) + x1)
    return inside


def rot(p, c, a):
    ca, sa = math.cos(a), math.sin(a)
    x, y = p[0] - c[0], p[1] - c[1]
    return (c[0] + x * ca - y * sa, c[1] + x * sa + y * ca)


def smooth(t):
    t = np.clip(t, 0, 1); return t * t * (3 - 2 * t)


def leg_forward(px, py, leg, thigh, knee, partial=1.0):
    """Bir noktayı bacak kemiklerine göre ileri dönüştürür (ekran: y aşağı; açı + = ileri / sağa)."""
    hip, kn = leg['hip'], leg['knee']
    # ağırlıklar: kalçanın üstünde 0, uylukta 1; dizden aşağısı ayrıca diz bükümü alır
    w_leg = smooth((py - hip[1] + 6) / 18) * partial
    w_shin = smooth((py - kn[1] + 6) / 12) * partial
    a = -thigh * w_leg                                   # ekran koordinatında saat yönü negatif
    ca, sa = np.cos(a), np.sin(a)
    x1 = hip[0] + (px - hip[0]) * ca - (py - hip[1]) * sa
    y1 = hip[1] + (px - hip[0]) * sa + (py - hip[1]) * ca
    k2 = rot(kn, hip, -thigh)                            # dizin yeni yeri
    b = knee * w_shin
    cb, sb = np.cos(b), np.sin(b)
    x2 = k2[0] + (x1 - k2[0]) * cb - (y1 - k2[1]) * sb
    y2 = k2[1] + (x1 - k2[0]) * sb + (y1 - k2[1]) * cb
    return x2, y2


def inverse_warp(src, mask, fwd, iters=10):
    """Ters eşleme: her çıktı pikseli için fwd(p) = q denklemini sabit nokta yinelemesiyle çözer."""
    h, w = mask.shape
    qy, qx = np.mgrid[0:h, 0:w].astype(np.float64)
    px, py = qx.copy(), qy.copy()
    for _ in range(iters):
        fx, fy = fwd(px, py)
        px += qx - fx; py += qy - fy
    ix = np.rint(px).astype(int); iy = np.rint(py).astype(int)
    ok = (ix >= 0) & (ix < w) & (iy >= 0) & (iy < h)
    out = np.zeros_like(src)
    ixc, iyc = np.clip(ix, 0, w - 1), np.clip(iy, 0, h - 1)
    take = ok & mask[iyc, ixc]
    out[take] = src[iyc[take], ixc[take]]
    return out


def drop_specks(img, min_px):
    """8-bağlı bileşenlerden min_px'ten küçük olanları siler (en büyük bileşen hep kalır)."""
    from collections import deque
    solid = img[..., 3] > 0
    h, w = solid.shape
    lab = np.full((h, w), -1, int); sizes = []
    for y0, x0 in zip(*np.nonzero(solid)):
        if lab[y0, x0] >= 0:
            continue
        n = len(sizes); lab[y0, x0] = n; q = deque([(y0, x0)]); cnt = 0
        while q:
            y, x = q.popleft(); cnt += 1
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < h and 0 <= nx < w and solid[ny, nx] and lab[ny, nx] < 0:
                        lab[ny, nx] = n; q.append((ny, nx))
        sizes.append(cnt)
    if not sizes:
        return img
    big = int(np.argmax(sizes)); out = img.copy()
    for i, c in enumerate(sizes):
        if i != big and c < min_px:
            out[lab == i] = 0
    return out


def pose(phase):
    """Yürüyüş: uyluk ±18°, öne salınan bacakta diz bükümü (radyan)."""
    def leg(p):
        thigh = math.radians(18) * math.sin(p)
        knee = math.radians(30) * max(0.0, math.sin(p + math.pi / 2)) ** 1.5    # öne salınırken bükülür
        return thigh, knee
    return {'near': leg(phase), 'far': leg(phase + math.pi)}


def frame(src, phase):
    h, w = src.shape[:2]
    ys, xs = np.mgrid[0:h, 0:w].astype(np.float64)
    alpha = src[..., 3] > 0
    masks = {k: inside_poly(xs + .5, ys + .5, v['poly']) & alpha for k, v in LEGS.items()}
    # bacakların arkasında kalan parlak kızıl pelerin / kaftan pikselleri gövde katmanında kalır
    c = src[..., :3].astype(int)
    cape_like = (c[..., 0] > 150) & (c[..., 1] < 80) & (ys < 232)            # yalnız saf kızıl
    for k in masks:
        masks[k] &= ~cape_like
    body_mask = alpha & ~masks['far'] & ~masks['near']
    angles = pose(phase)

    def body_fwd(px, py):
        x, y = px.copy(), py.copy()
        # kaftan etekleri uyluğu kısmen izler
        for k, (x0, x1, y0, y1) in PANELS.items():
            inpanel = (px >= x0) & (px <= x1) & (py >= y0)
            part = smooth((py - y0) / (y1 - y0 - 4)) * inpanel                # etek ucu uyluğu tam izler
            fx, fy = leg_forward(px, py, LEGS[k], angles[k][0], 0.0, part)
            x = np.where(inpanel, fx, x); y = np.where(inpanel, fy, y)
        # pelerin rüzgârda (gövde çizgisinin solu, omuzdan aşağı)
        away = np.clip((CLOTH_X * w - px) / (w * .3), 0, 1)
        down = np.clip((py - SHOULDER) / (h - SHOULDER), 0, 1)
        x = x + 3.0 * np.sin(phase * 2 + py * .05) * away * (.3 + .7 * down)
        return x, y

    layers = []
    for k in ('far', None, 'near'):
        if k is None:
            layers.append(inverse_warp(src, body_mask, body_fwd))
        else:
            leg = LEGS[k]; th, kn = angles[k]
            layers.append(inverse_warp(src, masks[k], lambda px, py, leg=leg, th=th, kn=kn: leg_forward(px, py, leg, th, kn)))
    out = np.zeros_like(src)
    for lay in layers:
        m = lay[..., 3] > 0
        out[m] = lay[m]
    # ana gövdeden kopuk küçük kırıntılar (bacakların arkasında açıkta kalan pelerin deseni) silinir
    out = drop_specks(out, 40)
    # yere basan ayak zeminde kalsın: en alttaki dolu satır dinlenme pozundakiyle aynı olacak şekilde kaydır
    rest_bottom = np.nonzero(alpha.any(1))[0].max()
    rows = np.nonzero((out[..., 3] > 0).any(1))[0]
    shift = rest_bottom - rows.max()
    out = np.roll(out, shift, axis=0)
    if shift > 0:
        out[:shift] = 0
    return out


def main():
    os.makedirs(OUT, exist_ok=True)
    src = np.asarray(Image.open(os.path.join(SRC, 'karamani-256.png')).convert('RGBA'))
    frames = []
    for i in range(FRAMES):
        f = frame(src, 2 * math.pi * i / FRAMES)
        Image.fromarray(f).save(os.path.join(OUT, f'karamani-{i:02d}.png'))
        frames.append(f)
    h, w = src.shape[:2]
    gif = []
    for f in frames:
        c = np.zeros((h + 20, w + 20, 3), np.uint8); c[:] = (58, 40, 64)
        a = f[..., 3:4] / 255.0
        c[10:10 + h, 10:10 + w] = (f[..., :3] * a + c[10:10 + h, 10:10 + w] * (1 - a)).astype(np.uint8)
        gif.append(Image.fromarray(c).resize(((w + 20) * 2, (h + 20) * 2), Image.NEAREST))
    path = os.path.join(SRC, 'walk-karamani.gif')
    gif[0].save(path, save_all=True, append_images=gif[1:], duration=110, loop=0)
    # inceleme şeridi: 8 kare yan yana
    strip = np.zeros((h, (w + 6) * FRAMES, 3), np.uint8); strip[:] = (58, 40, 64)
    for i, f in enumerate(frames):
        a = f[..., 3:4] / 255.0; x = i * (w + 6)
        strip[:, x:x + w] = (f[..., :3] * a + strip[:, x:x + w] * (1 - a)).astype(np.uint8)
    Image.fromarray(strip).save(os.path.join(SRC, 'walk-karamani-strip.png'))
    print(path)


if __name__ == '__main__':
    main()
