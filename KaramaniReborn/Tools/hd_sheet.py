"""HD karakter kare sayfası: dönüştürülmüş tek parça karelerden (Review/characters/from_art) oyunun okuduğu
Assets/Resources/Characters/hd/<tür>.png + .json üretir (PixelCharacter ile aynı biçim, 100 piksel = 1 birim).

Her kare gövde merkezinden (kare yüksekliğinin %30-55'indeki doluluğun ortancası) ve ayak çizgisinden hizalanır.
Kare verisi karelerden ölçülür:
  - kılıç: parlak, doygunluğu düşük piksellerden gövde merkezine en uzak olan uç, en yakın olan kök
  - basan ayak: en alt 3 satırdaki dolu küme; ardışık karelerde geriye kayan küme aynı ayaktır
  - adım uzunluğu (stride): basan ayağın kare başına geriye kayışının ortalaması × kare sayısı
Kullanım: python -I Tools/hd_sheet.py
"""
import glob
import json
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, 'Review', 'characters', 'from_art')
OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Characters', 'hd')
PPU = 50.0                 # 1080 satırlık ekranda 1 birim = 1080 / 21,6 = 50 piksel
SCALE = PPU / 100.0        # kaynak kareler 100 piksel/birim ölçeğinde (258 px Karamani)
def frames(name, pick=None):
    files = sorted(glob.glob(os.path.join(SRC, f'karamani-{name}-0*-258.png')))
    return [files[i] for i in pick] if pick is not None else files


CLIPS = {   # tür -> klip -> kare dosyaları
    'hero': {
        'idle': sorted(glob.glob(os.path.join(SRC, 'idle', 'karamani-profile-0*.png'))),
        'walk': sorted(glob.glob(os.path.join(SRC, 'karamani-walk-b-0*-258.png'))),
        'run': sorted(glob.glob(os.path.join(SRC, 'karamani-run-0*-258.png'))),
        'light1': sorted(glob.glob(os.path.join(SRC, 'karamani-attack-0*-258.png'))),
        'light3': frames('light3'),
        'heavy': frames('heavy'),
        'rise': frames('jump', [1, 2]), 'apex': frames('jump', [3]), 'fall': frames('jump', [4]), 'land': frames('jump', [5]),
        'flip': frames('roll', [1, 2, 3]),          # çift zıplamadaki takla: yuvarlanmanın havadaki kareleri
        'roll': frames('roll'),
        'hurt': frames('hurt'),
        'block': frames('block', [0, 1]), 'parry': frames('block', [2, 3]),
        'shoot': frames('shoot'),
        'heal': frames('heal'),
        'dead': frames('death'),
        'light2': frames('light2'),
        'spirit': frames('spirit'),
        'dive': frames('dive'),
        'backdash': frames('backdash'),
        'hang': frames('hang'),
        'climb': frames('climb'),
    }
}
BOUNDS = {   # oyunun zamanlamasına göre kare sınırları (Actor.Progress)
    'light1': [0, .08, .17, .27, .52, .66, .82, 1.0],    # 0-2 hazırlık, 3 savuruş (vuruş penceresi .27-.58), 4-6 bitiş
    'light3': [0, .12, .27, .45, .58, .70, .85, 1.0],    # 2-3 hamle vuruş penceresinde
    'heavy': [0, .14, .30, .44, .56, .69, .80, .90, 1.0],  # 3-4 iniş (ağır vuruş penceresi .44-.69)
    'shoot': [0, .15, .35, .50, .70, 1.0],                # ateş .35'te: 2. kare nişan, 3. kare geri tepme
    'heal': [0, .20, .40, .75, 1.0],                      # etki .55'te: 2. kare içerken
    'dead': [0, .12, .30, .50, .70, 1.0],
    'light2': [0, .12, .27, .42, .58, .70, .85, 1.0],    # 2-3 yukarı kesik vuruş penceresinde
    'spirit': [0, .12, .30, .55, .80, 1.0],               # kurt atılışı: etkin .15-.85
    'climb': [0, .20, .45, .75, 1.0],
}
# Asılma / tırmanma: dayanak noktası ellerin tuttuğu kenardır (oyun bu kliplerde karakteri kenar noktasında çizer).
# Değerler 258 px ölçekli karelerde elle işaretlendi (x, y: kenarın köşesi); ('body', dx): ayakta kare, kenar
# gövde merkezinin dx piksel solunda (tırmanış sonunda oyun karakteri kenarın 0,28 birim önüne koyar).
GRIP = {
    'hang': [(110, 6), (112, 6)],
    'climb': [(110, 6), (100, 108), (70, 150), ('body', -28)],
}
FEET = {'dive'}   # alt sınır kılıç ucu değil çizmeler: karakterin konumu ayaklarıdır
# kaynak sayfada komşu karenin kılıç ucu bu karelerin sol kenarına biniyor (klip -> kare sırası; None: hepsi)
EDGE_BLADE = {'heavy': None, 'spirit': [4], 'walk': None}
# komşu karenin pelerin parçası sağ kenara biniyor: (klip, kare) -> sağdan temizlenecek sütun (258 px ölçeğinde)
EDGE_RED = {('spirit', 3): 40}
MUZZLE = {'shoot'}   # bu kliplerde kılıç kökü yerine tabanca: mermi BladeRoot + (0,46; 0,06) birimden çıkar
LOCOMOTION = {'walk', 'run'}
STRIDE = {'walk': 184.0, 'run': 230.0}   # piksel / döngü; yapay zekâ karelerinde ayak teması tutarsız, ayak kilidi kullanılmaz


def anchor(a):
    h = a.shape[0]
    band = a[int(h * .3):int(h * .55)]
    return int(np.median(np.nonzero(band)[1]))


def blade(img, ax):
    """Kılıç: çok parlak, doygunluğu düşük, baş (miğfer) bölgesinin ve ayak şeridinin dışındaki pikseller;
    gövde merkezine en uzak olan uç, kılıç kümesinde gövdeye en yakın olan kök."""
    rgb = img[..., :3].astype(float); a = img[..., 3] > 0
    mx, mn = rgb.max(-1), rgb.min(-1)
    h, w = a.shape
    ys, xs = np.mgrid[0:h, 0:w]
    top = np.nonzero(a.any(1))[0].min()
    head_x = np.median(np.nonzero(a[top:top + 25])[1])
    head = np.hypot(xs - head_x, ys - (top + 22 * SCALE)) < 34 * SCALE
    m = a & (mx > 185) & ((mx - mn) / (mx + 1) < .14) & ~head & (ys < h - 14 * SCALE)
    m = largest(m)
    ys_, xs_ = np.nonzero(m)
    cy = h * .45
    if len(xs_) < 10:
        return (ax + 30.0, h * .55), (ax + 60.0, h * .55)
    d = np.hypot(xs_ - ax, ys_ - cy)
    tip = int(np.argmax(d))
    near = d > np.percentile(d, 30)
    root = int(np.argmin(np.where(near, d, 1e9)))
    return (float(xs_[root]), float(ys_[root])), (float(xs_[tip]), float(ys_[tip]))


def feet(img):
    """Kılıç pikselleri (parlak, doygunluğu düşük) dışındaki en alt dolu satır: çizmelerin tabanı."""
    rgb = img[..., :3].astype(float); a = img[..., 3] > 0
    mx, mn = rgb.max(-1), rgb.min(-1)
    body = a & ~((mx > 120) & ((mx - mn) / (mx + 1) < .22))
    rows = np.nonzero(body.sum(1) >= 3)[0]
    return int(rows.max()) + 1


def edge_blade(img):
    """Karenin sol kenarından içeri giren, alt yarıdaki kılıç şeridini (komşu karenin kılıcı) siler; şeridin
    kapattığı bot / pelerin pikselleri aynı sütunda en yakın sağlam pikselle doldurulur."""
    from collections import deque
    rgb = img[..., :3].astype(float); a = img[..., 3] > 0
    mx, mn = rgb.max(-1), rgb.min(-1)
    h, w = a.shape
    blade = a & (mx > 120) & ((mx - mn) / (mx + 1) < .22)
    blade[:int(h * .55)] = False
    seen = np.zeros_like(blade); q = deque((y, 0) for y in range(h) if blade[y, 0] or blade[y, 1])
    for y, x in q:
        seen[y, x] = True
    while q:
        y, x = q.popleft()
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                ny, nx = y + dy, x + dx
                if 0 <= ny < h and 0 <= nx < w and blade[ny, nx] and not seen[ny, nx]:
                    seen[ny, nx] = True; q.append((ny, nx))
    if seen.sum() < 20:
        return img
    # şeridi 1 piksel genişlet (kenar yumuşatma pikselleri)
    g = seen.copy(); g[1:] |= seen[:-1]; g[:-1] |= seen[1:]; g[:, 1:] |= seen[:, :-1]
    out = img.copy()
    for y, x in zip(*np.nonzero(g)):
        best = None
        for d in range(1, 12):
            for yy in (y - d, y + d):
                if 0 <= yy < h and a[yy, x] and not g[yy, x]:
                    best = yy; break
            if best is not None:
                break
        # yalnız şeridin hem üstünde hem altında gövde varsa doldur (botun içi); dışarıda kalan saydam olur
        above = any(a[yy, x] and not g[yy, x] for yy in range(max(0, y - 12), y))
        below = any(a[yy, x] and not g[yy, x] for yy in range(y + 1, min(h, y + 13)))
        out[y, x] = img[best, x] if best is not None and above and below else 0
    return out


def muzzle(img, fallback):
    """Tabancalı karede: göğüs-omuz yüksekliğindeki en öndeki piksel namlu ağzıdır. Oyun mermiyi kökün
    0,46 birim önünden, 0,06 birim yukarısından çıkarır; kök buna göre geri alınır."""
    a = img[..., 3] > 0; h = a.shape[0]
    band = a[int(h * .2):int(h * .45)]
    xs = np.nonzero(band.any(0))[0]
    if not len(xs):
        return fallback
    x = xs.max(); ys = np.nonzero(band[:, x])[0]
    y = int(h * .2) + float(np.mean(ys))
    return (float(x - .46 * PPU), float(y + .06 * PPU))


def largest(m):
    """Kılıç uzun, kesintisiz bir parlak şerittir: 1 piksellik boşluklar kapatılıp en büyük 8-bağlı küme alınır
    (bot, kemer tokası gibi küçük parlaklıklar elenir)."""
    from collections import deque
    g = m.copy()
    g[1:] |= m[:-1]; g[:-1] |= m[1:]; g[:, 1:] |= m[:, :-1]; g[:, :-1] |= m[:, 1:]
    h, w = g.shape
    lab = np.full((h, w), -1, int); sizes = []
    for y0, x0 in zip(*np.nonzero(g)):
        if lab[y0, x0] >= 0:
            continue
        n = len(sizes); lab[y0, x0] = n; q = deque([(y0, x0)]); cnt = 0
        while q:
            y, x = q.popleft(); cnt += 1
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < h and 0 <= nx < w and g[ny, nx] and lab[ny, nx] < 0:
                        lab[ny, nx] = n; q.append((ny, nx))
        sizes.append(cnt)
    if not sizes:
        return m
    return m & (lab == int(np.argmax(sizes)))


def shrink(im):
    """Ekran ölçeğine küçültme: yüksek kaliteli süzgeç, ardından kenarlar yeniden keskin (alfa eşiği)."""
    if SCALE != 1:
        im = im.resize((round(im.width * SCALE), round(im.height * SCALE)), Image.LANCZOS)
    a = np.asarray(im).copy()
    a[..., 3] = np.where(a[..., 3] >= 128, 255, 0)
    return a


def detached(img, keep_ratio=.15):
    """Gövdeden kopuk parçaları siler: komşu karenin kılıç ucu, yapay zekâ çiziminde havada kalan kırıntılar.
    Ana gövdenin %15'inden küçük her ayrık parça gider (kılıç, sorguç gibi bağlı parçalar gövdeyle birlikte kalır)."""
    from collections import deque
    a = img[..., 3] > 0
    h, w = a.shape
    lab = np.full((h, w), -1, int); sizes = []
    for y0, x0 in zip(*np.nonzero(a)):
        if lab[y0, x0] >= 0:
            continue
        n = len(sizes); lab[y0, x0] = n; q = deque([(y0, x0)]); cnt = 0
        while q:
            y, x = q.popleft(); cnt += 1
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < h and 0 <= nx < w and a[ny, nx] and lab[ny, nx] < 0:
                        lab[ny, nx] = n; q.append((ny, nx))
        sizes.append(cnt)
    out = img.copy()
    big = max(sizes) if sizes else 0
    for i, c in enumerate(sizes):
        if c < big * keep_ratio:
            out[lab == i] = 0
    return out


def contacts(img):
    a = img[..., 3] > 0
    rows = np.nonzero(a.any(1))[0]; bottom = rows.max()
    band = a[bottom - 2:bottom + 1].any(0)
    xs = np.nonzero(band)[0]
    groups, start = [], None
    for i, x in enumerate(xs):
        if start is None:
            start = x
        if i == len(xs) - 1 or xs[i + 1] - x > 3:
            groups.append((start + x) / 2.0); start = None
    return groups


def build(variant, clips):
    frames, records, out_clips = [], [], []
    for name, files in clips.items():
        if not files:
            raise SystemExit(f'{variant}/{name}: kare bulunamadı')
        imgs = [detached(shrink(Image.open(f).convert('RGBA'))) for f in files]
        if name in EDGE_BLADE:
            imgs = [edge_blade(im) if EDGE_BLADE[name] is None or k in EDGE_BLADE[name] else im for k, im in enumerate(imgs)]
        for k, im in enumerate(imgs):
            if (name, k) in EDGE_RED:
                im = im.copy(); c = im[..., :3].astype(int); n = int(EDGE_RED[(name, k)] * SCALE)
                red = (c[..., 0] > c[..., 1] + 40) & (c[..., 0] > c[..., 2] + 30)
                red[:, :-n] = False; im[red] = 0; imgs[k] = detached(im)
        start = len(frames)
        recs = []
        for img in imgs:
            ax = anchor(img[..., 3] > 0)
            bottom = np.nonzero((img[..., 3] > 0).any(1))[0].max() + 1
            k = len(recs)
            if name in GRIP:
                g = GRIP[name][k]
                if g[0] == 'body':
                    ax = ax + g[1] * SCALE
                else:
                    ax, bottom = g[0] * SCALE, g[1] * SCALE
            elif name in FEET:
                bottom = feet(img)
            root, tip = blade(img, ax)
            if name in MUZZLE:
                root = muzzle(img, root)
            recs.append(dict(img=img, ax=ax, bottom=bottom, root=root, tip=tip, contacts=[c - ax for c in contacts(img)]))
        stride = 0.0
        if name in LOCOMOTION:
            # basan ayak: ardışık karelerde geriye kayan temas kümesi; kayış ortalaması adım uzunluğunu verir
            moves = []
            for i in range(len(recs)):
                cur, nxt = recs[i]['contacts'], recs[(i + 1) % len(recs)]['contacts']
                best = None
                for c in cur:
                    for n in nxt:
                        d = c - n
                        if 0 < d < 40 and (best is None or d < best):
                            best = d
                if best is not None:
                    moves.append(best)
            step = float(np.median(moves)) if moves else 0.0
            stride = STRIDE[name] * SCALE if name in STRIDE else step * len(recs)
            slot, prev = 0, None
            for i, r in enumerate(recs):
                planted = [-999.0, -999.0]
                if r['contacts']:
                    if prev is None:
                        c = max(r['contacts'])
                    else:
                        c = min(r['contacts'], key=lambda c: abs(c - (prev - step)))
                        if abs(c - (prev - step)) > max(8.0, step * 1.5):
                            slot = 1 - slot; c = max(r['contacts'])
                    planted[slot] = c; prev = c
                else:
                    prev = None
                r['planted'] = [-999.0, -999.0]
        for r in recs:
            frames.append(r)
        out_clips.append(dict(name=name, start=int(start), count=len(recs), bounds=BOUNDS.get(name, []), stride=float(stride)))
    # ortak hücre: dayanak (gövde merkezi, ayak çizgisi) her hücrede aynı yerde
    left = max(r['ax'] for r in frames) + 4
    right = max(r['img'].shape[1] - r['ax'] for r in frames) + 4
    up = max(r['bottom'] for r in frames) + 4
    down = max(r['img'].shape[0] - r['bottom'] for r in frames) + 4   # dayanağın altında kalan kısım (asılma, saplama)
    up, down = int(np.ceil(up)), int(np.ceil(max(down, 4)))
    fw, fh = int(np.ceil(left + right)), up + down; left = int(np.ceil(left))
    cols = max(8, int(np.ceil(np.sqrt(len(frames) * fh / fw))))   # kareye yakın sayfa: Unity doku sınırı 4096
    rows_ = (len(frames) + cols - 1) // cols
    assert cols * fw <= 4096 and rows_ * fh <= 4096, (cols * fw, rows_ * fh)
    sheet = np.zeros((rows_ * fh, cols * fw, 4), np.uint8)
    pivot_x, pivot_y = left, down                    # dayanak: ayak tabanı (ya da kenar noktası)
    data_frames = []
    for i, r in enumerate(frames):
        row, col = divmod(i, cols)
        img = r['img']; x0 = int(round(col * fw + left - r['ax'])); y0 = int(round(row * fh + (fh - pivot_y) - r['bottom']))
        sheet[y0:y0 + img.shape[0], x0:x0 + img.shape[1]] = np.where(img[..., 3:4] > 0, img, sheet[y0:y0 + img.shape[0], x0:x0 + img.shape[1]])
        def rel(p):   # karedeki noktayı dayanağa göre (sağ +, yukarı +) piksele çevir
            return [round(float(p[0] - r["ax"]), 2), round(float(r["bottom"] - p[1]), 2)]
        pl = r.get('planted', [-999.0, -999.0])
        data_frames.append(dict(bladeRoot=rel(r['root']), bladeTip=rel(r['tip']),
                                plantedN=round(float(pl[0]), 2) if pl[0] > -998 else -999.0, plantedF=round(float(pl[1]), 2) if pl[1] > -998 else -999.0))
    os.makedirs(OUT, exist_ok=True)
    Image.fromarray(sheet).save(os.path.join(OUT, variant + '.png'))
    with open(os.path.join(OUT, variant + '.json'), 'w', encoding='utf-8') as f:
        json.dump(dict(frameWidth=fw, frameHeight=fh, pivotX=pivot_x, pivotY=pivot_y, columns=cols, pixelsPerUnit=PPU,
                       frames=data_frames, clips=out_clips), f)
    print(variant, sheet.shape, 'frames', len(frames), 'cell', fw, fh, [(c['name'], c['count'], round(c['stride'], 1)) for c in out_clips])


PORTRAIT = {'hero': ('idle', 0, (52, 0, 118, 80))}   # arayüz portresi: kaynak kare ve baş kırpması (258 px ölçeğinde)


def portrait(variant):
    clip, k, box = PORTRAIT[variant]
    im = Image.open(CLIPS[variant][clip][k]).convert('RGBA').crop(box)
    im.save(os.path.join(OUT, variant + '-portrait.png'))
    print(variant, 'portrait', im.size)


if __name__ == '__main__':
    for v, c in CLIPS.items():
        build(v, c)
        if v in PORTRAIT:
            portrait(v)
