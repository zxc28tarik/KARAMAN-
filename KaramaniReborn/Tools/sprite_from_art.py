"""Yüksek çözünürlüklü karakter görselinden (ör. yapay zekâ ile üretilmiş, piksel sanatı tarzında referans)
oyun boyunda tek parça piksel sprite üretir.

Adımlar: düz arka plan ve zemin gölgesi ayıklanır → karakter kırpılır → hedef boya alan ortalamasıyla
indirilir (kenar kapsaması %50) → karakter başına sınırlı palete (k-ortalama) indirgenir → tek kalan pikseller
çoğunluk rengine çekilir → seçici dış çizgi (komşu rengin koyulaştırılmışı) eklenir.

Kullanım:
  python -I Tools/sprite_from_art.py <görsel> --boxes "ad:x0,x1;ad:x0,x1" --height 62 --colors 24
Çıktı: Review/characters/from_art/<ad>.png (1×) ve önizleme (ham + bölge paleti).
"""
import argparse
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from char_preview import quantize  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Review', 'characters', 'from_art')


def background_mask(rgb, bg):
    """Arka plan: görüntü kenarından başlayıp yalnız arka plan rengine ya da aynı tonlu (daha koyu) zemin
    gölgesine yayılan bölge doldurma. Karakterin içindeki koyu renkler (siyah palto) kenara bağlı olmadığı ya da
    çok koyu olduğu için arka plan sayılmaz."""
    from collections import deque
    f = rgb.astype(np.float64); b = np.asarray(bg, np.float64)
    cos = (f @ b) / (np.linalg.norm(f, axis=-1) * np.linalg.norm(b) + 1e-6)
    lum = f @ np.array([.299, .587, .114]); blum = b @ np.array([.299, .587, .114])
    ok = (np.abs(f - b).sum(-1) < 36) | ((cos > .993) & (lum > blum * .55) & (lum < blum * 1.1))
    h, w = ok.shape
    ground = np.zeros((h, w), bool); ground[int(h * .86):] = True             # ayak altındaki koyu zemin gölgesi
    ok |= ground & (cos > .985) & (lum < blum * 1.1)
    seen = np.zeros((h, w), bool)
    q = deque()
    for x in range(w):
        for y in (0, h - 1):
            if ok[y, x] and not seen[y, x]:
                seen[y, x] = True; q.append((y, x))
    for y in range(h):
        for x in (0, w - 1):
            if ok[y, x] and not seen[y, x]:
                seen[y, x] = True; q.append((y, x))
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w and ok[ny, nx] and not seen[ny, nx]:
                seen[ny, nx] = True; q.append((ny, nx))
    return seen


def components(fg):
    """8-bağlı bileşen etiketleri (BFS)."""
    from collections import deque
    h, w = fg.shape
    lab = np.full((h, w), -1, int); n = 0
    for y0, x0 in zip(*np.nonzero(fg)):
        if lab[y0, x0] >= 0:
            continue
        lab[y0, x0] = n; q = deque([(y0, x0)])
        while q:
            y, x = q.popleft()
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < h and 0 <= nx < w and fg[ny, nx] and lab[ny, nx] < 0:
                        lab[ny, nx] = n; q.append((ny, nx))
        n += 1
    return lab, n


def kmeans(colors, k, iters=12, seed=1):
    rng = np.random.default_rng(seed)
    centers = colors[rng.choice(len(colors), size=min(k, len(colors)), replace=False)].astype(np.float64)
    for _ in range(iters):
        d = ((colors[:, None, :] - centers[None]) ** 2).sum(-1)
        lab = d.argmin(1)
        for i in range(len(centers)):
            sel = colors[lab == i]
            if len(sel):
                centers[i] = sel.mean(0)
    return centers, lab


def isolate(img, x0, x1, drop=''):
    """Karakterin maskesi ve kırpılmış görüntüsü (kaynak çözünürlükte)."""
    full = np.asarray(img.convert('RGB'))
    rgb = full[:, x0:x1 + 1]
    fg = ~BG[:, x0:x1 + 1]
    lab = LAB[:, x0:x1 + 1]
    ids, cnt = np.unique(lab[fg], return_counts=True)
    main = ids[cnt.argmax()]
    # ana bileşen + kutunun içinde kalan, ana bileşenin sınır kutusuna yakın küçük parçalar (kopmuş saç, pelerin ucu)
    ys, xs = np.nonzero(lab == main); by0, by1, bx0, bx1 = ys.min(), ys.max(), xs.min(), xs.max()
    keep = lab == main
    for i, c in zip(ids, cnt):
        if i == main or c < 6:
            continue
        cy, cx = np.nonzero(lab == i)
        full_in = (LAB == i).sum() == c                       # parça tamamen kutunun içinde
        near = cy.min() >= by0 - 8 and cy.max() <= by1 + 8 and cx.min() >= bx0 - 8 and cx.max() <= bx1 + 8
        if full_in and near:
            keep |= lab == i
    fg = keep
    if drop == 'noblue':                                   # komşu karakterin mavi pelerininden taşan parça
        c = rgb.astype(int); fg &= ~((c[..., 2] > c[..., 0] + 25) & (c[..., 2] > c[..., 1] + 10))
        lab2, _ = components(fg); ids2, cnt2 = np.unique(lab2[fg], return_counts=True); fg = lab2 == ids2[cnt2.argmax()]
    ys, xs = np.where(fg)
    y0, y1 = ys.min(), ys.max(); cx0, cx1 = xs.min(), xs.max()
    return rgb[y0:y1 + 1, cx0:cx1 + 1], fg[y0:y1 + 1, cx0:cx1 + 1]


def convert(rgb, fg, factor, colors, outline=True):
    """Kırpılmış karakteri ortak ölçekle (factor) indirir, sınırlı palete çeker, temizler."""
    height = max(1, round(rgb.shape[0] * factor))
    w = max(1, round(rgb.shape[1] * factor))
    # alan ortalaması: önplan renklerinin ağırlıklı ortalaması, kapsama alfa olur
    premul = np.dstack([rgb * fg[..., None], fg * 255]).astype(np.float64)
    small = np.asarray(Image.fromarray(premul[..., :3].astype(np.uint8)).resize((w, height), Image.BOX)).astype(np.float64)
    cov = np.asarray(Image.fromarray((fg * 255).astype(np.uint8)).resize((w, height), Image.BOX)).astype(np.float64) / 255
    solid = cov >= .5
    col = np.where(cov[..., None] > 0, small / np.maximum(cov[..., None], 1e-6), 0)
    col = np.clip(col, 0, 255)
    # karakter başına sınırlı palet
    pts = col[solid]
    centers, lab = kmeans(pts, colors)
    idx = np.full(solid.shape, -1); idx[solid] = lab
    # tek kalan pikseller: dört komşudan hiçbiri aynı değilse 8 komşunun çoğunluğuna
    h, wd = idx.shape
    pad = np.pad(idx, 1, constant_values=-1)
    out = idx.copy()
    for y in range(h):
        for x in range(wd):
            v = idx[y, x]
            if v < 0:
                continue
            n4 = [pad[y, x + 1], pad[y + 2, x + 1], pad[y + 1, x], pad[y + 1, x + 2]]
            if v in n4:
                continue
            n8 = [pad[y + dy, x + dx] for dy in range(3) for dx in range(3) if (dy, dx) != (1, 1)]
            n8 = [n for n in n8 if n >= 0]
            if n8:
                vals, cnt = np.unique(n8, return_counts=True)
                if cnt.max() >= 4:
                    out[y, x] = vals[cnt.argmax()]
    idx = out
    # kırıntılar: dört komşusunda hiç dolu piksel olmayan tekil pikseller atılır
    filled = idx >= 0
    p2 = np.pad(filled, 1)
    lonely = filled & ~(p2[:-2, 1:-1] | p2[2:, 1:-1] | p2[1:-1, :-2] | p2[1:-1, 2:])
    idx[lonely] = -1
    rgba = np.zeros((h + 2, wd + 2, 4), np.uint8)
    body = np.zeros((h + 2, wd + 2), bool); body[1:-1, 1:-1] = idx >= 0
    cols = np.zeros((h + 2, wd + 2, 3)); cols[1:-1, 1:-1][idx >= 0] = centers[idx[idx >= 0]]
    rgba[body, :3] = cols[body].astype(np.uint8); rgba[body, 3] = 255
    # seçici dış çizgi: komşu rengin koyulaştırılmışı; üst / ön (ışık) tarafta daha az koyu
    for dy, dx, k in (((1, 0, .25), (0, -1, .25), (-1, 0, .45), (0, 1, .45)) if outline else ()):
        nb = np.roll(body, (dy, dx), axis=(0, 1)); nc = np.roll(cols, (dy, dx), axis=(0, 1))
        take = ~body & nb & (rgba[..., 3] == 0)
        rgba[take, :3] = (nc[take] * k).astype(np.uint8); rgba[take, 3] = 255
    return rgba


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('image')
    ap.add_argument('--boxes', default=None, help='ad:x0,x1[:noblue];... (kaynak görselde sütun aralıkları)')
    ap.add_argument('--auto', default=None, help='ad:N — N kareyi en boş sütunlardan kesip bul (ad-00 …)')
    ap.add_argument('--grid', default=None, help='ad:N — tek satırlık kare dizisini N eşit hücreye böl (ad-00 … ad-NN)')
    ap.add_argument('--height', type=int, default=62, help='referans karakterin (--ref) hedef yüksekliği; diğerleri aynı ölçekle')
    ap.add_argument('--src-height', type=float, default=None, help='kaynakta karakterin ayakta boyu (piksel); ölçek = height / bu')
    ap.add_argument('--ref', default=None, help='ölçeğin alındığı karakter (varsayılan: ilk kutu)')
    ap.add_argument('--colors', type=int, default=24)
    ap.add_argument('--no-outline', action='store_true', help='kaynakta çizgi zaten varsa (yüksek çözünürlük)')
    ap.add_argument('--grade-only', action='store_true', help='alt sıra: palete indirgemeden yalnız bölge renk ayarı')
    ap.add_argument('--zone', type=int, default=0)
    ap.add_argument('--scale', type=int, default=6)
    args = ap.parse_args()
    img = Image.open(args.image)
    bg = np.asarray(img.convert('RGB'))[5, 5]
    global BG, LAB
    BG = background_mask(np.asarray(img.convert('RGB')), bg)
    LAB, _ = components(~BG)
    os.makedirs(OUT, exist_ok=True)
    items = []
    specs = []
    if args.auto:
        name, n = args.auto.split(':'); n = int(n)
        occ = (~BG).sum(0).astype(float)
        occ = np.convolve(occ, np.ones(5) / 5, mode='same')
        cols = np.nonzero(occ > 1)[0]; left, right = cols.min(), cols.max(); step = (right - left) / n
        cuts = [left]
        for i in range(1, n):
            c = left + i * step; lo, hi = int(c - step * .3), int(c + step * .3)
            cuts.append(lo + int(np.argmin(occ[lo:hi])))
        cuts.append(right + 1)
        specs = [(f'{name}-{i:02d}', cuts[i], cuts[i + 1] - 1, '') for i in range(n)]
    if args.grid:
        gname, n = args.grid.split(':'); n = int(n); cw = img.size[0] / n
        specs = [(f'{gname}-{i:02d}', round(i * cw), round((i + 1) * cw) - 1, '') for i in range(n)]
    for item in (args.boxes.split(';') if args.boxes else []):
        parts = item.split(':'); x0, x1 = (int(v) for v in parts[1].split(','))
        specs.append((parts[0], x0, x1, parts[2] if len(parts) > 2 else ''))
    for name, x0, x1, drop in specs:
        items.append((name, *isolate(img, x0, x1, drop)))
    if args.src_height:
        factor = args.height / args.src_height
    elif (args.grid or args.auto) and not args.ref:
        factor = args.height / max(r.shape[0] for n, r, f in items)      # kare dizisi: en uzun kare karakterin ayakta boyu
    else:
        ref = args.ref or items[0][0]
        factor = args.height / next(r.shape[0] for n, r, f in items if n == ref)
    sprites = []
    for name, rgb, fg in items:
        rgba = convert(rgb, fg, factor, args.colors, outline=not args.no_outline)
        Image.fromarray(rgba).save(os.path.join(OUT, f'{name}-{args.height}.png'))
        print(name, 'height', rgba.shape[0], 'width', rgba.shape[1])
        sprites.append(rgba)
    # önizleme: üst sıra ham, alt sıra bölge görünümü (palet ya da yalnız renk ayarı)
    from char_preview import zone_look, grade
    pad = 4; hmax = max(s_.shape[0] for s_ in sprites); wsum = sum(s_.shape[1] for s_ in sprites) + pad * (len(sprites) + 1)
    neutral = np.array((46, 44, 56), np.uint8); backdrop = {0: (78, 52, 82), 1: (52, 46, 46), 2: (40, 56, 70), 3: (110, 106, 120)}[args.zone]
    sheet = np.zeros((2 * (hmax + 2 * pad), wsum, 3), np.uint8); H2 = hmax + 2 * pad
    sheet[:H2] = neutral; sheet[H2:] = backdrop
    x = pad
    for s_ in sprites:
        y = H2 - pad - s_.shape[0]; a = s_[..., 3:4] / 255.0
        sheet[y:y + s_.shape[0], x:x + s_.shape[1]] = (s_[..., :3] * a + sheet[y:y + s_.shape[0], x:x + s_.shape[1]] * (1 - a)).astype(np.uint8)
        if args.grade_only:
            _, _, g, st, lt = zone_look(args.zone)
            q = (grade(s_[..., :3] / 255.0, g, st, lt) * 255).astype(np.uint8)
        else:
            q = quantize(s_, args.zone, backdrop)
        q[s_[..., 3] == 0] = backdrop
        sheet[H2 + y:H2 + y + s_.shape[0], x:x + s_.shape[1]] = q
        x += s_.shape[1] + pad
    path = os.path.join(OUT, f'lineup-{args.height}.png')
    Image.fromarray(sheet).resize((sheet.shape[1] * args.scale, sheet.shape[0] * args.scale), Image.NEAREST).save(path)
    print(path)


if __name__ == '__main__':
    main()
