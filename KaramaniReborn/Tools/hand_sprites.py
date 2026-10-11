"""Elle çizilmiş tek parça karakter sprite'ları (oyun çözünürlüğünde, 1 piksel = 1 oyun pikseli).

Çalışma biçimi bir piksel sanatçısınınki gibidir: önce büyük şekiller koordinatlarıyla bloklanır
(fill / ellipse / line / px), sonra gölge ve ışık kümeleri tek tek yerleştirilir; dış çizgi seçicidir
(komşu malzemenin koyu tonu, ışık alan kenarda bir ton açık). Sonuç Review/characters altına
büyütülmüş önizleme olarak yazılır: ham renkler ve bölge paletine indirgenmiş oyun görünümü.

Kullanım:  python -I Tools/hand_sprites.py [karakter ...]
"""
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REVIEW = os.path.join(ROOT, 'Review', 'characters', 'samples')

# Malzeme rampaları: koyu → açık. Dış çizgi rampanın ilk tonunu kullanır.
RAMPS = {
    'skin':   ['3b2321', '6e412f', '9c6648', 'c79068', 'e5b98f'],
    'pale':   ['2c2430', '564a5a', '8a7f8f', 'bdb2bf', 'e6dfe8'],
    'green':  ['17220f', '2c4220', '4b6b32', '75944a', 'a8c46e'],
    'beard':  ['161112', '2b2124', '45373a', '66564f', '8d7b6e'],
    'blond':  ['3d2a10', '6b4a1c', 'a37a32', 'd4ae5c', 'f2da92'],
    'black':  ['0b0a10', '17161f', '252431', '3a3949', '57566a'],
    'ivory':  ['3e352f', '6b5f52', 'a29277', 'cdbd9c', 'efe3c4'],
    'red':    ['22080d', '4a1018', '781c24', 'a7302d', 'd4553f'],
    'steel':  ['15171e', '2c313c', '4e5563', '7d8695', 'bcc4cf'],
    'bronze': ['2e1c0c', '5c3b16', '946628', 'c99a45', 'f1d48a'],
    'leather':['1c110b', '3a2416', '5e3c24', '865a36', 'ad7f50'],
    'wood':   ['1e130b', '3b2614', '5f3f22', '87603a', 'ab8456'],
    'navy':   ['0b0f1f', '172040', '27366a', '3e5596', '6a82c2'],
    'teal':   ['0a1a1b', '133234', '1f4f4f', '33726c', '5a9a8e'],
    'coat':   ['0b100d', '141e19', '22332a', '35503f', '527257'],
    'blade':  ['1d2129', '4f5866', '8d97a5', 'cad2dc', 'f6f8fb'],
    'eye':    ['070507', '0f0a0c', '1d1214', '1d1214', '1d1214'],
    'blood':  ['1a0306', '4a0710', '8a0e18', 'c4202a', 'ef5a4a'],
    # Karamani: bölge renk ayarından sonra doğru palet yuvasına düşen, önceden telafi edilmiş rampalar
    # (Docs/specs/CHAR-01-karamani-sprite-spec.md §5.1; gri zeminde fazla doygun görünür, bölge önizlemesine bak)
    'h_red':     ['360f0f', '5d1824', '962439', 'c9393f', 'dc8751'],
    'h_skin':    ['512118', '7e3924', 'b46039', 'db8751', 'db8751'],
    'h_bronze':  ['2a1f10', '7b5700', 'a58106', 'd5a803', 'f9e47b'],
    'h_ivory':   ['3a332c', '6e6556', '9a907c', 'c4b89c', 'ece2c8'],
    'h_steel':   ['101218', '272b33', '3f4550', '646b78', '98a0ad'],
    'h_mail':    ['0e0f13', '1e2127', '343840', '4a4f59', '646a75'],
    'h_beard':   ['0c0a0a', '1a1615', '2a2321', '3e3532', '5a504b'],
    'h_leather': ['1a1411', '2e231d', '46362c', '624c3d', 'b16c33'],
    'h_blade':   ['1c2026', '4a515c', '858e9b', 'c3cad3', 'f4f6f8'],
    'rim':       ['3a3f4d', '5b6172', '7d8499', '7d8499', '7d8499'],
    'fair':      ['3a2522', '7a4c3e', 'b77f68', 'dcaa8e', 'f2d0b5'],
    'plate':     ['14161d', '2d323e', '505867', '8790a0', 'd0d7e0'],
    'amber':     ['3b1d06', '7a4210', 'b9741f', 'e6a640', 'ffd98a'],
    'mustard':   ['2e2208', '5e4a14', '8f7424', 'c2a23a', 'e8cc68'],
    'brown':     ['1d140e', '3b2a1d', '5c432e', '7e5e41', 'a07c58'],
    'turk':      ['4a2a20', '875239', 'bd8460', 'dfae88', 'f3cfa8'],
    'oguz':      ['4a3029', '8f604f', 'cc9e86', 'ebc8b0', 'fae4d2'],      # Oğuz: beyaz ten
    'kumral':    ['2b1b11', '553722', '80583a', 'a87a50', 'cfa06c'],      # kumral saç, kaş, bıyık
}
MATS = list(RAMPS)


def hexrgb(h):
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


class Sprite:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.mat = np.full((h, w), -1, int)
        self.tone = np.zeros((h, w), int)

    def _put(self, mask, mat, tone):
        m = MATS.index(mat)
        self.mat[mask] = m; self.tone[mask] = tone

    def _grid(self):
        ys, xs = np.mgrid[0:self.h, 0:self.w]
        return xs + .5, ys + .5

    def fill(self, pts, mat, tone, clip=None):
        """Çokgen; piksel merkezi içerideyse boyanır. clip: yalnız zaten dolu (mask_of) yerlere."""
        X, Y = self._grid()
        inside = np.zeros((self.h, self.w), bool)
        n = len(pts)
        for i in range(n):
            x1, y1 = pts[i]; x2, y2 = pts[(i + 1) % n]
            if y1 == y2:
                continue
            cond = ((y1 > Y) != (y2 > Y)) & (X < (x2 - x1) * (Y - y1) / (y2 - y1) + x1)
            inside ^= cond
        if clip is not None:
            inside &= self.mask_of(clip)
        self._put(inside, mat, tone)
        return self

    def ellipse(self, cx, cy, rx, ry, mat, tone, clip=None):
        X, Y = self._grid()
        inside = ((X - cx) / rx) ** 2 + ((Y - cy) / ry) ** 2 <= 1
        if clip is not None:
            inside &= self.mask_of(clip)
        self._put(inside, mat, tone)
        return self

    def line(self, x0, y0, x1, y1, mat, tone, clip=None):
        """Bresenham çizgisi (piksel kenarları temiz kalır)."""
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
        err = dx + dy
        allowed = self.mask_of(clip) if clip is not None else None
        while True:
            if 0 <= x0 < self.w and 0 <= y0 < self.h and (allowed is None or allowed[y0, x0]):
                self.mat[y0, x0] = MATS.index(mat); self.tone[y0, x0] = tone
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 >= dy:
                err += dy; x0 += sx
            if e2 <= dx:
                err += dx; y0 += sy
        return self

    def px(self, pts, mat, tone):
        for x, y in pts:
            if 0 <= x < self.w and 0 <= y < self.h:
                self.mat[y, x] = MATS.index(mat); self.tone[y, x] = tone
        return self

    def erase(self, pts):
        for x, y in pts:
            self.mat[y, x] = -1
        return self

    def span(self, y, x0, x1, mat, mid, back=None, front=None, back_w=1, front_w=1):
        """Satır aralığı: arka (sol) kenar gölge tonunda, orta ana tonda, ön (sağ, ışık) kenar ışık tonunda."""
        if x1 < x0:
            return self
        cols = list(range(x0, x1 + 1))
        for i, x in enumerate(cols):
            t = mid
            if back is not None and i < back_w:
                t = back
            if front is not None and i >= len(cols) - front_w:
                t = front
            self.px([(x, y)], mat, t)
        return self

    def ascii(self, rows, legend):
        """rows: {y: [(x, "karakterler"), ...]}; boşluk atlanır. legend: karakter -> (malzeme, ton)."""
        for y, segs in rows.items():
            for x, text in segs:
                for i, ch in enumerate(text):
                    if ch == ' ':
                        continue
                    if ch == '.':
                        self.mat[y, x + i] = -1
                        continue
                    m, t = legend[ch]
                    self.px([(x + i, y)], m, t)
        return self

    def mask_of(self, mats):
        mats = [mats] if isinstance(mats, str) else mats
        return np.isin(self.mat, [MATS.index(m) for m in mats])

    def outline(self, light_side=True):
        """Seçici dış çizgi: siluetin dışındaki piksel, komşu malzemenin en koyu tonunu alır; üst / sağ
        (ışık) tarafta komşu ışıklıysa bir ton açık. Köşegen boşluklar kapatılmaz (yumuşak köşe)."""
        solid = self.mat >= 0
        out_m = np.full_like(self.mat, -1); out_t = np.zeros_like(self.tone)
        for dr, dc in ((1, 0), (0, -1), (-1, 0), (0, 1)):
            m2 = np.full_like(self.mat, -1); t2 = np.zeros_like(self.tone)
            rs, re_ = max(0, -dr), min(self.h, self.h - dr); cs, ce = max(0, -dc), min(self.w, self.w - dc)
            m2[rs:re_, cs:ce] = self.mat[rs + dr:re_ + dr, cs + dc:ce + dc]
            t2[rs:re_, cs:ce] = self.tone[rs + dr:re_ + dr, cs + dc:ce + dc]
            take = ~solid & (m2 >= 0) & (out_m < 0)
            lit = light_side and (dr == 1 or dc == -1)
            out_m[take] = m2[take]
            out_t[take] = np.where(lit & (t2[take] >= 3), 1, 0)
        self.mat = np.where(out_m >= 0, out_m, self.mat); self.tone = np.where(out_m >= 0, out_t, self.tone)
        return self

    def rgba(self):
        img = np.zeros((self.h, self.w, 4), np.uint8)
        for mi, name in enumerate(MATS):
            for t, hx in enumerate(RAMPS[name]):
                sel = (self.mat == mi) & (self.tone == t)
                img[sel, :3] = hexrgb(hx); img[sel, 3] = 255
        return img


# ----------------------------------------------------------------------------------------
# Önizleme
# ----------------------------------------------------------------------------------------

def preview(sprites, path, scale=6, zone=0, pad=4):
    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
    from char_preview import quantize
    neutral = (46, 44, 56)
    backdrop = {0: (78, 52, 82), 1: (52, 46, 46), 2: (40, 56, 70), 3: (110, 106, 120)}[zone]
    h = max(s.h for s in sprites) + 2 * pad
    w = sum(s.w for s in sprites) + pad * (len(sprites) + 1)
    sheet = np.zeros((h * 2, w, 3), np.uint8)
    sheet[:h] = neutral; sheet[h:] = backdrop
    x = pad
    for s in sprites:
        img = s.rgba(); y = h - pad - s.h
        a = img[..., 3:4] / 255.0
        sheet[y:y + s.h, x:x + s.w] = (img[..., :3] * a + sheet[y:y + s.h, x:x + s.w] * (1 - a)).astype(np.uint8)
        sheet[h + y:h + y + s.h, x:x + s.w] = quantize(img, zone, backdrop)
        bg = np.array(backdrop, np.uint8)
        q = sheet[h + y:h + y + s.h, x:x + s.w]
        q[img[..., 3] == 0] = bg
        x += s.w + pad
    out = Image.fromarray(sheet).resize((sheet.shape[1] * scale, sheet.shape[0] * scale), Image.NEAREST)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    out.save(path)
    return path
