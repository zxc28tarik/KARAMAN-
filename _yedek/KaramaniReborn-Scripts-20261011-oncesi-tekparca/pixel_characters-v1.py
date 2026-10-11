"""Karamani piksel karakter üreticisi.

Her animasyon karesi, iskelet pozundan sıfırdan piksel ızgarasına çizilir. Hiçbir resim
parçası döndürülmez ya da esnetilmez: şekiller her karede yeniden taranır, gölgeler sınırlı
renk rampalarından seçilir, dış çizgi ve iç ayrım çizgileri karenin tamamına birlikte uygulanır.
Böylece eklem yerleri görünmez ve sonuç elle çizilmiş kare kare animasyon gibi okunur.

Çıktı: Assets/Resources/Characters/<tür>.png (kare ızgarası) ve <tür>.json (kare verisi:
kılıç kökü / ucu, basan ayaklar, animasyon listeleri). Unity tarafı PixelCharacter.cs.
Çalıştırma: python Tools/pixel_characters.py   (Review/characters altına büyütülmüş önizleme de yazar)
"""
import json
import math
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Characters')
REVIEW = os.path.join(ROOT, 'Review', 'characters')

W = H = 128          # kare boyutu (piksel)
PX, PY = 64, 40      # dayanak noktası: ayakların yere bastığı nokta
S = 3                # kenar örnekleme katı
COLUMNS = 16
UNIT = 25.0          # bir dünya birimi = 25 oyun pikseli (PixelPipeline.PixelSize = 0.04)

_ss = (np.arange(W * S) + .5) / S
GX = np.ascontiguousarray(np.broadcast_to(_ss[None, :] - PX, (H * S, W * S)))
GY = np.ascontiguousarray(np.broadcast_to((H - _ss[:, None]) - PY, (H * S, W * S)))
_c = np.arange(W) + .5
CX = np.ascontiguousarray(np.broadcast_to(_c[None, :] - PX, (H, W)))
CY = np.ascontiguousarray(np.broadcast_to((H - _c[:, None]) - PY, (H, W)))

LIGHT = np.array([.38, .62, .68]); LIGHT = LIGHT / np.linalg.norm(LIGHT)


def hexes(*codes):
    return [tuple(int(c[i:i + 2], 16) for i in (0, 2, 4)) for c in codes]


# Malzeme rampaları: 0 dış çizgi, 1 gölge, 2 orta, 3 ışık, 4 parlama
BASE_RAMPS = {
    'skin':    hexes('2a1712', '5a3326', '8c5840', 'b9805f', 'd8a582'),
    'beard':   hexes('0e0b0b', '1d1716', '322826', '4a3c37', '5e4e47'),
    'turban':  hexes('3a3027', '6e604e', 'a39177', 'cfbf9f', 'ece1c6'),
    'red':     hexes('1c070a', '4a1219', '7a1f27', 'a8383a', 'cc5a4a'),
    'steel':   hexes('101116', '262a33', '454b57', '737b89', 'acb4c0'),
    'mail':    hexes('101116', '2a2e37', '4c525e', '7a818e', '9aa1ad'),
    'bronze':  hexes('261a0b', '56391a', '8c6630', 'c39750', 'ecd087'),
    'leather': hexes('120c0a', '2c1d16', '4a3223', '6c4a32', '8b6444'),
    'blade':   hexes('20252c', '58616d', '96a1ae', 'd2d9e1', 'ffffff'),
    'cloth':   hexes('2a241f', '5d5245', '8f8270', 'bdb099', 'ddd3bd'),
    'wood':    hexes('1a100a', '3b2616', '5e3e23', '835a33', 'a57a4a'),
    'glass':   hexes('22070a', '5c0f16', '9a1d22', 'd34236', 'ffb39a'),
    'dark':    hexes('0c0b0e', '1b1a20', '2c2b33', '403f49', '57565f'),
}


def ramps_for(variant):
    r = dict(BASE_RAMPS)
    if variant == 'ghoul':
        r['skin'] = hexes('1c201d', '3e4740', '66726a', '8e9a90', 'b4bfb4')
        r['red'] = hexes('121010', '2a2524', '433b38', '5e5450', '7a706a')
        r['steel'] = hexes('140f0c', '33261d', '5a4130', '7d5c42', '9c7a5a')
        r['cloth'] = hexes('1d1b18', '3e3a33', '5f594f', '807a6e', '9f9a8d')
    if variant == 'captain':
        r['red'] = hexes('180508', '400c14', '6e1520', '9c2830', 'c44a40')
        r['steel'] = hexes('14110c', '30281a', '54452a', '84703f', 'b8a060')
    return r


# ----------------------------------------------------------------------------------------
# Şekiller: işaretli uzaklık (kapsama için) ve piksel merkezinde normal (gölge için)
# ----------------------------------------------------------------------------------------

class Part:
    __slots__ = ('mask', 'nx', 'ny', 'nz', 'u', 'v', 'material', 'group', 'far', 'pattern', 'flat')

    def __init__(self, mask, nx, ny, nz, u, v, material, group, far=False, pattern=None, flat=None):
        self.mask, self.nx, self.ny, self.nz, self.u, self.v = mask, nx, ny, nz, u, v
        self.material, self.group, self.far, self.pattern, self.flat = material, group, far, pattern, flat


def coverage(inside_ss):
    return inside_ss.reshape(H, S, W, S).mean(axis=(1, 3)) >= .5


def limb(a, b, ra, rb, material, group, far=False, pattern=None):
    """Konik kapsül: kol, bacak, kılıç sapı."""
    def field(px, py):
        bx, by = b[0] - a[0], b[1] - a[1]
        L2 = max(bx * bx + by * by, 1e-6)
        t = np.clip(((px - a[0]) * bx + (py - a[1]) * by) / L2, 0, 1)
        dx, dy = px - a[0] - bx * t, py - a[1] - by * t
        r = ra + (rb - ra) * t
        return dx, dy, r, t
    dx, dy, r, _ = field(GX, GY)
    mask = coverage(dx * dx + dy * dy < r * r)
    dx, dy, r, t = field(CX, CY)
    nx, ny = dx / r, dy / r
    k = np.clip(nx * nx + ny * ny, 0, 1)
    L = math.hypot(b[0] - a[0], b[1] - a[1]) + 1e-6
    across = (dx * -(b[1] - a[1]) + dy * (b[0] - a[0])) / L / r
    return Part(mask, nx, ny, np.sqrt(1 - k), t, across, material, group, far, pattern)


def blob(c, rx, ry, material, group, angle=0.0, far=False, pattern=None):
    """Elips: kafa, omuzluk, el, kalkan."""
    ca, sa = math.cos(math.radians(angle)), math.sin(math.radians(angle))

    def local(px, py):
        x, y = px - c[0], py - c[1]
        return (x * ca + y * sa) / rx, (-x * sa + y * ca) / ry
    lx, ly = local(GX, GY)
    mask = coverage(lx * lx + ly * ly < 1)
    lx, ly = local(CX, CY)
    k = np.clip(lx * lx + ly * ly, 0, 1)
    nx, ny = lx * ca - ly * sa, lx * sa + ly * ca
    return Part(mask, nx, ny, np.sqrt(1 - k), lx, ly, material, group, far, pattern)


def inside_polygon(px, py, pts):
    inside = np.zeros(px.shape, bool)
    n = len(pts)
    for i in range(n):
        x1, y1 = pts[i]; x2, y2 = pts[(i + 1) % n]
        if y1 == y2:
            continue
        cond = ((y1 > py) != (y2 > py)) & (px < (x2 - x1) * (py - y1) / (y2 - y1) + x1)
        inside ^= cond
    return inside


def band(spine, widths, material, group, far=False, pattern=None, cut=None):
    """Omurga çizgisi boyunca genişliği değişen şerit: gövde, pelerin, etek, kılıç ağzı.
    Gölge, şeridin enine kesitini silindir sayar. widths: her omurga noktası için (sol, sağ) yarı genişlik."""
    pts_l, pts_r = [], []
    n = len(spine)
    for i in range(n):
        ax, ay = spine[max(i - 1, 0)]; bx, by = spine[min(i + 1, n - 1)]
        dx, dy = bx - ax, by - ay; d = math.hypot(dx, dy) + 1e-9
        px_, py_ = -dy / d, dx / d   # sol normal
        wl, wr = widths[i] if isinstance(widths[i], tuple) else (widths[i], widths[i])
        pts_l.append((spine[i][0] + px_ * wl, spine[i][1] + py_ * wl))
        pts_r.append((spine[i][0] - px_ * wr, spine[i][1] - py_ * wr))
    poly = pts_l + pts_r[::-1]
    inside = inside_polygon(GX, GY, poly)
    if cut is not None:
        inside &= cut(GX, GY)
    mask = coverage(inside)
    # en yakın omurga parçasına göre uzunlamasına (u) ve enine (v) konum
    best = np.full(CX.shape, 1e9); U = np.zeros(CX.shape); V = np.zeros(CX.shape)
    total = sum(math.hypot(spine[i + 1][0] - spine[i][0], spine[i + 1][1] - spine[i][1]) for i in range(n - 1)) + 1e-9
    acc = 0.0
    for i in range(n - 1):
        (ax, ay), (bx, by) = spine[i], spine[i + 1]
        sx, sy = bx - ax, by - ay; L2 = sx * sx + sy * sy + 1e-9; L = math.sqrt(L2)
        t = np.clip(((CX - ax) * sx + (CY - ay) * sy) / L2, 0, 1)
        dx, dy = CX - ax - sx * t, CY - ay - sy * t
        d = dx * dx + dy * dy
        side = np.sign(dx * -sy + dy * sx)
        wl = widths[i] if not isinstance(widths[i], tuple) else widths[i][0]
        wr = widths[i] if not isinstance(widths[i], tuple) else widths[i][1]
        wl2 = widths[i + 1] if not isinstance(widths[i + 1], tuple) else widths[i + 1][0]
        wr2 = widths[i + 1] if not isinstance(widths[i + 1], tuple) else widths[i + 1][1]
        w = np.where(side >= 0, wl + (wl2 - wl) * t, wr + (wr2 - wr) * t) + 1e-6
        closer = d < best
        best = np.where(closer, d, best)
        U = np.where(closer, (acc + t * L) / total, U)
        V = np.where(closer, side * np.sqrt(d) / w, V)
        acc += L
    V = np.clip(V, -1, 1)
    # enine silindir normali: omurganın soluna/sağına doğru
    nx = np.zeros(CX.shape); ny = np.zeros(CX.shape)
    acc = 0.0
    for i in range(n - 1):
        (ax, ay), (bx, by) = spine[i], spine[i + 1]
        sx, sy = bx - ax, by - ay; L = math.hypot(sx, sy) + 1e-9
        lo, hi = acc / total, (acc + L) / total
        sel = (U >= lo - 1e-6) & (U <= hi + 1e-6)
        nx = np.where(sel, -sy / L * V, nx); ny = np.where(sel, sx / L * V, ny)
        acc += L
    nz = np.sqrt(np.clip(1 - V * V, 0, 1))
    return Part(mask, nx, ny, nz, U, V, material, group, far, pattern)


def polygon(pts, material, group, far=False, pattern=None, normal=(0.0, .25)):
    mask = coverage(inside_polygon(GX, GY, pts))
    nx = np.full(CX.shape, normal[0]); ny = np.full(CX.shape, normal[1])
    nz = np.sqrt(np.clip(1 - nx * nx - ny * ny, 0, 1))
    xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
    U = (CX - min(xs)) / (max(xs) - min(xs) + 1e-6); V = (CY - min(ys)) / (max(ys) - min(ys) + 1e-6)
    return Part(mask, nx, ny, nz, U, V, material, group, far, pattern)


# ----------------------------------------------------------------------------------------
# Desenler: ton kaydırmaları (lamel sıraları, zincir örgü, sarık katları, pelerin kıvrımları)
# ----------------------------------------------------------------------------------------

def pat_lamellar(part, tone):
    rows = (np.floor(CY + 200) % 3 == 0)
    tone = np.where(rows, tone - 1, tone)
    return tone


def pat_mail(part, tone):
    checker = ((np.floor(CX + 200) + np.floor(CY + 200)) % 2 == 0)
    return np.where(checker, tone - 1, tone)


def pat_wrap(part, tone):
    stripe = (np.floor((CX * .55 + CY) + 200) % 3 == 0)
    return np.where(stripe, tone - 1, tone)


def pat_folds(part, tone):
    fold = np.sin(part.v * 5.5 + part.u * 2.0) > .55
    return np.where(fold, tone - 1, tone)


def pat_greave(part, tone):
    band_ = (np.abs(part.u - .55) < .08)
    return np.where(band_, tone + 1, tone)


# ----------------------------------------------------------------------------------------
# İskelet
# ----------------------------------------------------------------------------------------

def v_add(a, b, k=1.0):
    return (a[0] + b[0] * k, a[1] + b[1] * k)


def rot(v, deg):
    a = math.radians(deg); c, s = math.cos(a), math.sin(a)
    return (v[0] * c - v[1] * s, v[0] * s + v[1] * c)


def ik(start, end, l1, l2, bend):
    dx, dy = end[0] - start[0], end[1] - start[1]
    d = math.hypot(dx, dy)
    ax, ay = (dx / d, dy / d) if d > 1e-6 else (0.0, -1.0)
    dd = min(max(d, abs(l1 - l2) + 1e-3), l1 + l2 - 1e-3)
    along = (l1 * l1 - l2 * l2 + dd * dd) / (2 * dd)
    across = math.sqrt(max(0.0, l1 * l1 - along * along))
    joint = (start[0] + ax * along - ay * across * bend, start[1] + ay * along + ax * across * bend)
    return joint, (start[0] + ax * dd, start[1] + ay * dd)


PROPORTIONS = dict(thigh=15.0, shin=14.0, torso=18.5, upper=10.0, fore=9.5, ankle=3.0)


def pose_defaults():
    return dict(hip=(0.0, 30.0), lean=6.0, head=0.0,
                footN=(7.0, 3.0), footF=(-8.0, 3.0), toeN=0.0, toeF=0.0,
                hN=(8.0, -7.0), hF=(-2.0, -10.0), sword=38.0, swordOn=True, twoHand=False,
                cape=.2, capeWave=0.0, hood=.2, rot=0.0, rotC=(0.0, 30.0),
                item=None, itemAngle=0.0, shieldUp=0.0, bow=None, flash=0.0, dead=0.0)


def P(**kw):
    p = pose_defaults(); p.update(kw); return p


def reach(p, variant, handN, handF):
    """Elleri mutlak hedeflere (dayanağa göre piksel) yerleştirir: omuzlar pozdan hesaplanır."""
    k = 1.09 if variant == 'captain' else (.98 if variant != 'hero' else 1.0)
    sk = skeleton(p, k)
    q = dict(p)
    q['hN'] = ((handN[0] * k - sk['shoulderN'][0]) / k, (handN[1] * k - sk['shoulderN'][1]) / k)
    q['hF'] = ((handF[0] * k - sk['shoulderF'][0]) / k, (handF[1] * k - sk['shoulderF'][1]) / k)
    return q


def skeleton(p, k):
    L = {n: v * k for n, v in PROPORTIONS.items()}
    hip = (p['hip'][0] * k, p['hip'][1] * k)
    up = rot((0.0, 1.0), -p['lean'])
    fwd = (up[1], -up[0])
    chest = v_add(hip, up, L['torso'])
    sk = dict(hip=hip, up=up, fwd=fwd, chest=chest, L=L)
    sk['shoulderN'] = v_add(v_add(chest, fwd, .8 * k), up, -3.2 * k)
    sk['shoulderF'] = v_add(v_add(chest, fwd, -2.2 * k), up, -2.6 * k)
    hup = rot(up, -p['head'])
    hfw = (hup[1], -hup[0])
    sk['hup'], sk['hfw'] = hup, hfw
    sk['neck'] = v_add(chest, up, .6 * k)
    sk['head'] = v_add(v_add(sk['neck'], hup, 5.0 * k), hfw, 1.4 * k)
    sk['hipN'] = v_add(hip, fwd, 1.6 * k)
    sk['hipF'] = v_add(v_add(hip, fwd, -1.6 * k), up, .6 * k)
    for side in 'NF':
        foot = (p['foot' + side][0] * k, p['foot' + side][1] * k)
        knee, ankle = ik(sk['hip' + side], foot, L['thigh'], L['shin'], 1)
        sk['knee' + side], sk['ankle' + side] = knee, ankle
        hand = v_add(sk['shoulder' + side], (p['h' + side][0] * k, p['h' + side][1] * k))
        elbow, wrist = ik(sk['shoulder' + side], hand, L['upper'], L['fore'], -1)
        sk['elbow' + side], sk['wrist' + side] = elbow, wrist
    return sk


# ----------------------------------------------------------------------------------------
# Karakter çizimi
# ----------------------------------------------------------------------------------------

def boot(ankle, toe_angle, k, group, far):
    """Kıvrık burunlu deri çizme: topuk, taban, yukarı kalkan burun."""
    a = ankle
    pts = [(-2.6, 2.4), (-2.9, -1.2), (-2.2, -2.9), (3.0, -2.9), (5.6, -2.4), (7.2, -1.2), (7.0, -.4), (5.2, -.9), (2.4, .2), (1.9, 2.6)]
    pts = [rot((x * k, y * k), toe_angle) for x, y in pts]
    pts = [(a[0] + x, a[1] + y) for x, y in pts]
    parts = [polygon(pts, 'leather', group, far, normal=(.0, .35))]
    cuff = limb(v_add(a, (-2.3 * k, 2.0 * k)), v_add(a, (2.0 * k, 2.2 * k)), 1.0 * k, 1.0 * k, 'bronze', group, far)
    parts.append(cuff)
    return parts


def kilij(hand, angle, k, group, length=21.0, far=False):
    """Kılıç: kabzası elde, ağzı hafifçe yukarı kıvrılan, ucu genişleyen (yelman) bir kılıç.
    Kök ve uç noktaları vuruş hesabı için döner."""
    d = rot((1.0, 0.0), angle); n = (-d[1], d[0])
    guard = v_add(hand, d, 2.0 * k)
    parts = []
    parts.append(limb(v_add(hand, d, -3.2 * k), v_add(hand, d, 1.2 * k), 1.05 * k, 1.05 * k, 'red', group, far))      # kabza
    parts.append(blob(v_add(hand, d, -3.8 * k), 1.3 * k, 1.3 * k, 'bronze', group, far=far))                        # topuz
    parts.append(limb(v_add(guard, n, -2.4 * k), v_add(guard, n, 2.4 * k), .9 * k, .9 * k, 'bronze', group, far))   # balçak
    Lb = length * k
    spine, widths = [], []
    for i in range(8):
        s = i / 7
        pt = v_add(v_add(guard, d, s * Lb), n, 2.6 * k * s * s)
        spine.append(pt)
        w = 1.05 if s < .62 else (1.45 if s < .86 else .9 * (1 - s) / .14 + .15)
        widths.append((w * k * .9, w * k * 1.05))
    parts.append(band(spine, widths, 'blade', group, far, pattern=pat_blade))
    tip = spine[-1]
    return parts, guard, tip


def pat_blade(part, tone):
    edge = part.v < -.25
    back = part.v > .45
    t = np.where(edge, 4, np.where(back, 2, 3))
    return t


def draw_head_hero(sk, p, k, parts):
    hc, hup, hfw = sk['head'], sk['hup'], sk['hfw']
    G = 6
    parts.append(limb(sk['neck'], v_add(hc, hup, -2.0 * k), 2.6 * k, 2.4 * k, 'mail', G, pattern=pat_mail))
    parts.append(blob(hc, 4.1 * k, 4.6 * k, 'skin', G, angle=-p['lean'] - p['head']))
    parts.append(blob(v_add(v_add(hc, hfw, 4.0 * k), hup, -.6 * k), 1.1 * k, 1.0 * k, 'skin', G))          # burun
    parts.append(blob(v_add(v_add(hc, hfw, 1.2 * k), hup, -2.9 * k), 3.6 * k, 2.6 * k, 'beard', G, angle=-p['lean'] - p['head']))
    parts.append(blob(v_add(v_add(hc, hfw, 2.9 * k), hup, -1.2 * k), 1.5 * k, .8 * k, 'beard', G))          # bıyık
    # sarık: başın üstünü ve arkasını saran sargı, arkadan sarkan kızıl uç
    tail_dir = rot(hup, 180 - 25 - 40 * p['cape'])
    tail = [v_add(v_add(hc, hfw, -3.5 * k), hup, 1.0 * k)]
    for i in range(1, 4):
        tail.append(v_add(tail[-1], rot(tail_dir, -12 * i * (.5 + p['cape']) + p['capeWave'] * 8), 3.0 * k))
    parts.append(band(tail, [(1.4 * k, 1.4 * k), (1.6 * k, 1.5 * k), (1.4 * k, 1.3 * k), (.6 * k, .6 * k)], 'red', G, pattern=pat_folds))
    parts.append(blob(v_add(v_add(hc, hup, 2.0 * k), hfw, -.5 * k), 5.3 * k, 3.4 * k, 'turban', G, angle=-p['lean'] - p['head'] + 8, pattern=pat_wrap))
    parts.append(blob(v_add(v_add(hc, hup, 2.6 * k), hfw, 3.2 * k), 1.1 * k, 1.1 * k, 'bronze', G))         # madalyon


def draw_head_guard(sk, p, k, parts, variant):
    hc, hup, hfw = sk['head'], sk['hup'], sk['hfw']
    G = 6
    # başlığın arkadan omuzlara dökülen kumaşı
    drape = [v_add(v_add(hc, hfw, -2.5 * k), hup, 2.0 * k), v_add(v_add(hc, hfw, -4.5 * k), hup, -2.5 * k),
             v_add(v_add(sk['neck'], sk['fwd'], -4.5 * k), sk['up'], -4.0 * k)]
    parts.append(band(drape, [(2.6 * k, 2.6 * k), (3.4 * k, 3.0 * k), (3.0 * k, 2.4 * k)], 'cloth', G, pattern=pat_folds))
    parts.append(limb(sk['neck'], v_add(hc, hup, -2.0 * k), 2.6 * k, 2.4 * k, 'mail', G, pattern=pat_mail))
    parts.append(blob(hc, 4.0 * k, 4.6 * k, 'skin', G, angle=-p['lean'] - p['head']))
    parts.append(blob(v_add(v_add(hc, hfw, 3.9 * k), hup, -.6 * k), 1.1 * k, 1.0 * k, 'skin', G))
    if variant != 'ghoul':
        parts.append(blob(v_add(v_add(hc, hfw, 2.8 * k), hup, -1.4 * k), 1.8 * k, .8 * k, 'beard', G))      # pala bıyık
    parts.append(blob(v_add(v_add(hc, hup, 1.9 * k), hfw, -.9 * k), 5.0 * k, 3.6 * k, 'cloth', G, angle=-p['lean'] - p['head'] + 4, pattern=pat_folds))
    parts.append(limb(v_add(v_add(hc, hup, 1.0 * k), hfw, -4.0 * k), v_add(v_add(hc, hup, 1.6 * k), hfw, 3.6 * k), 1.0 * k, .9 * k, 'red', G))
    if variant == 'captain':   # tepelik ve tüy
        parts.append(limb(v_add(hc, hup, 4.6 * k), v_add(v_add(hc, hup, 10 * k), hfw, -3 * k), 1.0 * k, .4 * k, 'red', G))
        parts.append(blob(v_add(hc, hup, 4.8 * k), 1.3 * k, 1.3 * k, 'bronze', G))


def draw_character(p, variant):
    k = 1.09 if variant == 'captain' else (.98 if variant != 'hero' else 1.0)
    sk = skeleton(p, k)
    up, fwd = sk['up'], sk['fwd']
    parts = []
    meta = {}
    hero = variant == 'hero'

    # 0: pelerin (en arkada) — yalnız kahraman ve yüzbaşı
    if hero or variant == 'captain':
        root_ = v_add(v_add(sk['chest'], fwd, -2.6 * k), up, -1.0 * k)
        ang = -90 - 10 - 70 * p['cape']
        spine = [root_]
        for i in range(1, 6):
            wave = math.sin(i * 1.3 + p['capeWave'] * 6.283) * 6 * (.3 + p['cape'])
            spine.append(v_add(spine[-1], rot((1.0, 0.0), ang + (i - 1) * 4 * p['cape'] + wave * .6), 5.4 * k))
        widths = [(2.0 * k, 2.6 * k), (3.0 * k, 3.6 * k), (3.6 * k, 4.4 * k), (4.0 * k, 5.0 * k), (4.3 * k, 5.4 * k), (4.4 * k, 5.6 * k)]
        hem_end = spine[-1]

        def tattered(px, py, end=hem_end, d=rot((1.0, 0.0), ang)):
            along = (px - end[0]) * d[0] + (py - end[1]) * d[1]
            across = (px - end[0]) * -d[1] + (py - end[1]) * d[0]
            return along < -1.5 * k + 2.4 * k * (np.sin(across * 1.7) > .2) * (np.sin(across * .9 + 1) > -.4)
        parts.append(band(spine, widths, 'red', 0, pattern=pat_folds, cut=tattered))

    # 1: arka kol, 2: arka bacak (bir ton koyu)
    parts.append(limb(sk['shoulderF'], sk['elbowF'], 2.7 * k, 2.4 * k, 'red', 1, far=True, pattern=pat_folds))
    parts.append(limb(sk['elbowF'], sk['wristF'], 2.4 * k, 2.0 * k, 'steel', 1, far=True))
    handF = v_add(sk['wristF'], (sk['wristF'][0] - sk['elbowF'][0], sk['wristF'][1] - sk['elbowF'][1]), .17)
    parts.append(blob(handF, 1.8 * k, 1.8 * k, 'leather', 1, far=True))
    parts.append(limb(sk['hipF'], sk['kneeF'], 4.0 * k, 3.3 * k, 'red', 2, far=True, pattern=pat_folds))
    parts.append(limb(sk['kneeF'], sk['ankleF'], 2.9 * k, 2.3 * k, 'steel', 2, far=True, pattern=pat_greave))
    parts.extend(boot(sk['ankleF'], p['toeF'], k, 2, True))
    parts.append(blob(sk['kneeF'], 2.1 * k, 2.1 * k, 'bronze', 2, far=True))

    # 3: gövde — lamel zırh, bel kuşağı, kemer
    torso_spine = [v_add(sk['hip'], up, -1.0 * k), v_add(sk['hip'], up, 5 * k), v_add(sk['hip'], up, 11 * k), v_add(sk['hip'], up, 16.5 * k), v_add(sk['chest'], up, .5 * k)]
    torso_w = [(5.2 * k, 5.4 * k), (4.6 * k, 4.8 * k), (5.6 * k, 5.4 * k), (6.4 * k, 5.6 * k), (4.8 * k, 4.6 * k)]
    parts.append(band(torso_spine, torso_w, 'steel', 3, pattern=pat_lamellar))
    parts.append(band([v_add(sk['hip'], up, 3.2 * k), v_add(sk['hip'], up, 6.4 * k)], [(5.3 * k, 5.3 * k), (5.0 * k, 5.0 * k)], 'red', 3, pattern=pat_folds))
    parts.append(band([v_add(sk['hip'], up, 1.6 * k), v_add(sk['hip'], up, 2.8 * k)], [(5.5 * k, 5.5 * k), (5.5 * k, 5.5 * k)], 'bronze', 3))
    parts.append(blob(v_add(v_add(sk['hip'], up, 2.2 * k), fwd, 3.6 * k), 1.4 * k, 1.4 * k, 'bronze', 3))
    parts.append(blob(v_add(v_add(sk['chest'], up, -6.5 * k), fwd, 3.4 * k), 1.3 * k, 1.3 * k, 'bronze', 3))

    # 4: ön bacak
    parts.append(limb(sk['hipN'], sk['kneeN'], 4.1 * k, 3.4 * k, 'red', 4, pattern=pat_folds))
    parts.append(limb(sk['kneeN'], sk['ankleN'], 3.0 * k, 2.4 * k, 'steel', 4, pattern=pat_greave))
    parts.extend(boot(sk['ankleN'], p['toeN'], k, 4, False))
    parts.append(blob(sk['kneeN'], 2.2 * k, 2.2 * k, 'bronze', 4))

    # 5: etek (lamel paneller ve altından taşan kızıl kumaş)
    waist = v_add(sk['hip'], up, 1.0 * k)
    skirt_down = rot(up, 180 - 4 * math.copysign(1, p['lean']))
    skirt_spine = [waist, v_add(waist, skirt_down, 6 * k), v_add(waist, skirt_down, 11.5 * k)]
    parts.append(band(skirt_spine, [(5.6 * k, 5.4 * k), (6.4 * k, 6.2 * k), (6.8 * k, 6.6 * k)], 'red', 5, pattern=pat_folds,
                      cut=lambda px, py: np.sin(px * 1.9) * 1.2 * k + (py - (waist[1] - 10.8 * k)) > 0))
    parts.append(band(skirt_spine[:2] + [v_add(waist, skirt_down, 8.5 * k)], [(5.8 * k, 5.6 * k), (6.2 * k, 6.0 * k), (6.3 * k, 6.1 * k)], 'steel', 5, pattern=pat_lamellar))

    # 6: baş
    if hero:
        draw_head_hero(sk, p, k, parts)
    else:
        draw_head_guard(sk, p, k, parts, variant)

    # kalkan: arka kolda, gövdenin önünde
    if variant in ('shield', 'captain') and p.get('shield', True):
        sc = v_add(handF, fwd, 2.0 * k)
        parts.append(blob(sc, 4.2 * k + 3.2 * k * (1 - abs(p['shieldUp'] - .5) * 0), 7.4 * k, 'red', 7, angle=-p['lean'] - 8 + 20 * p['shieldUp'], pattern=pat_shield))
        parts.append(blob(sc, 1.4 * k, 1.4 * k, 'bronze', 7))

    # 8: ön kol, omuzluk, el ve silah
    parts.append(limb(sk['shoulderN'], sk['elbowN'], 2.8 * k, 2.5 * k, 'red', 8, pattern=pat_folds))
    parts.append(blob(sk['shoulderN'], 3.6 * k, 3.0 * k, 'bronze' if hero else 'steel', 8, angle=-p['lean']))
    parts.append(limb(sk['elbowN'], sk['wristN'], 2.6 * k, 2.1 * k, 'steel', 8))
    wdir = (sk['wristN'][0] - sk['elbowN'][0], sk['wristN'][1] - sk['elbowN'][1])
    wl = math.hypot(*wdir) + 1e-6
    handN = v_add(sk['wristN'], (wdir[0] / wl, wdir[1] / wl), 1.6 * k)
    meta['handN'] = handN
    meta['handF'] = handF

    weapon_parts = []
    if p['item'] == 'pistol':
        d = rot((1.0, 0.0), p['itemAngle'])
        weapon_parts.append(limb(v_add(handN, d, -1.0 * k), v_add(handN, d, 7.5 * k), 1.0 * k, .8 * k, 'steel', 9))
        weapon_parts.append(limb(v_add(handN, (0, -.5)), v_add(v_add(handN, d, -3.5 * k), (0, -2.6 * k)), 1.5 * k, 1.2 * k, 'wood', 9))
        meta['bladeRoot'] = handN; meta['bladeTip'] = v_add(handN, d, 8 * k)
    elif p['item'] == 'potion':
        weapon_parts.append(blob(v_add(handF, (0, 1.6 * k)), 1.6 * k, 2.2 * k, 'glass', 9, angle=p['itemAngle']))
        weapon_parts.append(limb(v_add(handF, (0, 3.4 * k)), v_add(handF, (0, 4.8 * k)), .7 * k, .7 * k, 'wood', 9))
        meta['bladeRoot'] = handN; meta['bladeTip'] = v_add(handN, (6 * k, 0))
    elif variant == 'archer':
        bow_parts, string_hand = bow(handF, handN, p, k)
        weapon_parts.extend(bow_parts)
        meta['bladeRoot'] = handF; meta['bladeTip'] = v_add(handF, fwd, 4 * k)
    elif p['swordOn']:
        sw, guard, tip = kilij(handN, p['sword'], k, 9, length=21.0 if hero else 19.0)
        weapon_parts.extend(sw)
        meta['bladeRoot'] = handN; meta['bladeTip'] = tip
    else:
        meta['bladeRoot'] = handN; meta['bladeTip'] = v_add(handN, (4 * k, 0))

    near_hand = blob(handN, 1.9 * k, 1.9 * k, 'leather', 8)
    if p['twoHand']:
        parts.append(blob(v_add(handN, rot((1.0, 0.0), p['sword']), -2.6 * k), 1.8 * k, 1.8 * k, 'leather', 8))
    parts.extend(weapon_parts)
    parts.append(near_hand)

    # dönüş (takla, yuvarlanma, ölüm): tüm geometri pozla birlikte döndürülür
    return parts, sk, meta


def pat_shield(part, tone):
    r = np.sqrt(part.u ** 2 + part.v ** 2)
    rim = r > .82
    crescent = (np.sqrt((part.u - .05) ** 2 + (part.v) ** 2) < .45) & (np.sqrt((part.u + .12) ** 2 + (part.v - .05) ** 2) > .38)
    return np.where(rim | crescent, 10 + tone, tone)   # 10+ : bronz malzemeye geçiş işareti


def bow(handF, handN, p, k):
    draw = p['bow'] or 0.0
    top = v_add(handF, (-1.5 * k, 11 * k)); bot = v_add(handF, (-1.5 * k, -11 * k))
    mid = v_add(handF, (1.8 * k, 0))
    parts = [band([top, v_add(handF, (1.0 * k, 6 * k)), mid, v_add(handF, (1.0 * k, -6 * k)), bot],
                  [.7 * k, 1.0 * k, 1.1 * k, 1.0 * k, .7 * k], 'wood', 9)]
    nock = v_add(handF, (-3 * k - 8 * k * draw, 0))
    parts.append(limb(top, nock, .35 * k, .35 * k, 'cloth', 9))
    parts.append(limb(bot, nock, .35 * k, .35 * k, 'cloth', 9))
    if draw > .05:
        parts.append(limb(nock, v_add(handF, (5 * k, 0)), .45 * k, .45 * k, 'wood', 9))
    return parts, nock


# ----------------------------------------------------------------------------------------
# Tarama: parçaları birleştir, gölgele, çizgileri çek
# ----------------------------------------------------------------------------------------

NO_LINE = {(3, 5), (5, 3), (6, 3), (3, 6), (0, 3), (8, 9), (9, 8), (4, 5), (5, 4), (1, 2), (2, 1), (6, 0), (0, 6)}


def rasterize(parts, ramps, rotation=0.0, center=(0.0, 30.0)):
    names = list(ramps.keys())
    mat = np.full((H, W), -1, int)
    tone = np.zeros((H, W), int)
    group = np.full((H, W), -1, int)
    layer = np.full((H, W), -1, int)
    for li, part in enumerate(parts):
        m = part.mask
        if not m.any():
            continue
        lam = part.nx * LIGHT[0] + part.ny * LIGHT[1] + part.nz * LIGHT[2]
        t = 1 + (lam > -.12).astype(int) + (lam > .5).astype(int) + (lam > .86).astype(int)
        if part.pattern is not None:
            t = part.pattern(part, t)
        material = np.full((H, W), names.index(part.material))
        switch = t >= 10
        if switch.any():
            material = np.where(switch, names.index('bronze'), material); t = np.where(switch, t - 10, t)
        if part.far:
            t = t - 1
        t = np.clip(t, 1, 4)
        mat[m] = material[m]; tone[m] = t[m]; group[m] = part.group; layer[m] = li
    # iç ayrım çizgileri: öndeki parçanın arkadaki başka bir gruba değen kenarı koyulaşır
    line = np.zeros((H, W), bool)
    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
        g2 = np.roll(group, (dy, dx), axis=(0, 1)); l2 = np.roll(layer, (dy, dx), axis=(0, 1))
        cand = (group >= 0) & (g2 >= 0) & (g2 != group) & (l2 < layer)
        if cand.any():
            pairs = np.zeros((H, W), bool)
            for a, b in NO_LINE:
                pairs |= (group == a) & (g2 == b)
            line |= cand & ~pairs
    tone = np.where(line, 0, tone)
    # dış çizgi: siluetin hemen dışına komşu malzemenin en koyu tonu
    out_mat = np.full((H, W), -1, int)
    empty = mat < 0
    for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
        m2 = np.roll(mat, (dy, dx), axis=(0, 1))
        take = empty & (m2 >= 0) & (out_mat < 0)
        out_mat[take] = m2[take]
    rgba = np.zeros((H, W, 4), np.uint8)
    table = np.array([ramps[n] for n in names], np.uint8)  # (materials, 5, 3)
    solid = mat >= 0
    rgba[solid, :3] = table[mat[solid], tone[solid]]
    rgba[solid, 3] = 255
    edge = out_mat >= 0
    rgba[edge, :3] = table[out_mat[edge], 0]
    rgba[edge, 3] = 255
    return rgba


def rotate_parts_pose(p, sk_fn):
    return p


def render_pose(p, variant, ramps):
    parts, sk, meta = draw_character(p, variant)
    if p['rot']:
        # Dönen pozlar: geometriyi değil örnekleme ızgarasını ters döndürmek yerine,
        # kareyi döndürülmüş koordinatlarda yeniden tarıyoruz.
        parts, meta = draw_rotated(p, variant)
    rgba = rasterize(parts, ramps)
    return rgba, meta, sk


def draw_rotated(p, variant):
    """Takla / yuvarlanma: tarama ızgarasını dönme merkezinin etrafında ters çevirip yeniden çizer."""
    global GX, GY, CX, CY
    a = math.radians(-p['rot']); c, s = math.cos(a), math.sin(a)
    k = 1.09 if variant == 'captain' else (.98 if variant != 'hero' else 1.0)
    cx, cy = p['rotC'][0] * k, p['rotC'][1] * k
    saved = (GX, GY, CX, CY)
    try:
        def turn(X, Y):
            x, y = X - cx, Y - cy
            return x * c - y * s + cx, x * s + y * c + cy
        GX, GY = turn(saved[0], saved[1]); CX, CY = turn(saved[2], saved[3])
        q = dict(p); q['rot'] = 0.0
        parts, sk, meta = draw_character(q, variant)
    finally:
        GX, GY, CX, CY = saved
    inv = math.radians(p['rot']); ci, si = math.cos(inv), math.sin(inv)
    for key in ('bladeRoot', 'bladeTip', 'handN', 'handF'):
        if key in meta:
            x, y = meta[key][0] - cx, meta[key][1] - cy
            meta[key] = (x * ci - y * si + cx, x * si + y * ci + cy)
    return parts, meta


# ----------------------------------------------------------------------------------------
# Animasyonlar
# ----------------------------------------------------------------------------------------

def lerp(a, b, t):
    if isinstance(a, tuple):
        return tuple(lerp(x, y, t) for x, y in zip(a, b))
    if isinstance(a, (int, float)) and isinstance(b, (int, float)):
        return a + (b - a) * t
    return b if t >= .5 else a


def mix(p, q, t):
    return {key: lerp(p[key], q[key], t) for key in p}


def smooth(t):
    t = max(0.0, min(1.0, t)); return t * t * (3 - 2 * t)


def gait(phase, stride, stance, lift, foot_y=3.0, spread=0.0):
    """Ayak yolu: basma evresinde ayak gövdeye göre tam olarak gövdenin ilerlediği kadar geri kayar."""
    phase %= 1.0
    half = stride * stance / 2
    if phase < stance:
        x = half - stride * phase
        return (x + spread, foot_y), 0.0, True
    s = (phase - stance) / (1 - stance)
    x = -half + 2 * half * smooth(s)
    y = foot_y + lift * math.sin(math.pi * s)
    return (x + spread, y), 18 * math.sin(math.pi * s) - 6 * s, False


def locomotion(variant, run):
    frames = []
    n = 8
    stride = (50.0 if run else 34.0) if variant == 'hero' else 30.0
    stance = .4 if run else .62
    lift = 8.0 if run else 4.5
    for i in range(n):
        ph = i / n
        fN, tN, pN = gait(ph, stride, stance, lift)
        fF, tF, pF = gait(ph + .5, stride, stance, lift)
        bob = math.cos(ph * 4 * math.pi)
        swing = math.sin(ph * 2 * math.pi)
        p = P(hip=(1.5 if run else .5, (28.6 if run else 29.6) + (1.3 if run else .7) * bob),
              lean=(13 if run else 5) + bob * (1.2 if run else .4), head=-4 if run else -1,
              footN=fN, footF=fF, toeN=tN, toeF=tF,
              hN=(7.5 + 2.5 * swing, -8.0 + 1.5 * abs(swing)), hF=(-1.0 - 5.5 * swing, -10.5 + 2 * abs(swing)),
              sword=(18 if run else 30) - 8 * swing, cape=(.75 if run else .35) + .08 * bob, capeWave=ph)
        frames.append((p, {'planted': [fN[0] if pN else None, fF[0] if pF else None]}))
    return frames, stride


def make_animations(variant):
    hero = variant == 'hero'
    A = {}
    stance = P()
    # Bekleme: hafif nefes, pelerin dalgası
    A['idle'] = [(mix(stance, P(hip=(0, 29.4), lean=7.5, hN=(8.3, -7.6), sword=36, cape=.24), smooth(abs(math.sin(i / 6 * math.pi)))) | {'capeWave': i / 6}, {}) for i in range(6)]
    run, run_stride = locomotion(variant, True)
    walk, walk_stride = locomotion(variant, False)
    A['run'] = run; A['walk'] = walk
    A['skid'] = [(P(hip=(-2, 27.5), lean=-10, footN=(11, 3), footF=(-6, 3), toeN=14, hN=(5, -6), hF=(-6, -6), sword=10, cape=.55), {}),
                 (P(hip=(-1.5, 28), lean=-7, footN=(10, 3), footF=(-6, 3), toeN=10, hN=(6, -7), hF=(-5, -8), sword=18, cape=.45), {})]
    A['rise'] = [(P(hip=(0, 31), lean=4, footN=(5, 6), footF=(-5, 1), toeF=-30, hN=(7, -2), hF=(-4, -4), sword=55, cape=.1), {}),
                 (P(hip=(0, 31), lean=2, footN=(6, 12), footF=(-4, 9), toeN=20, toeF=-10, hN=(8, -3), hF=(-5, -2), sword=50, cape=.05), {})]
    A['apex'] = [(P(hip=(0, 31), lean=6, footN=(7, 13), footF=(-3, 11), toeN=25, hN=(9, -5), hF=(-6, -3), sword=40, cape=.35), {})]
    A['fall'] = [(P(hip=(0, 31), lean=4, footN=(6, 8), footF=(-5, 5), toeN=10, hN=(9, -1), hF=(-7, 1), sword=48, cape=.6), {}),
                 (P(hip=(0, 31), lean=2, footN=(5, 4), footF=(-6, 2), hN=(9, 2), hF=(-8, 3), sword=55, cape=.75), {})]
    A['land'] = [(P(hip=(0, 23.5), lean=16, footN=(8, 3), footF=(-8, 3), hN=(9, -9), hF=(-4, -12), sword=15, cape=.5), {}),
                 (P(hip=(0, 27.5), lean=10, footN=(7, 3), footF=(-8, 3), hN=(8.5, -8), hF=(-3, -11), sword=28, cape=.3), {})]
    tuck = P(hip=(0, 30), lean=40, footN=(6, 22), footF=(1, 20), toeN=40, toeF=40, hN=(8, -8), hF=(5, -12), sword=-30, cape=.4)
    A['flip'] = [(tuck | {'rot': -60.0 * i, 'rotC': (0.0, 30.0)}, {}) for i in range(6)]
    # Saldırılar: kare sınırları saldırı ilerlemesine göre (GameWorld.Active pencereleriyle uyumlu)
    ready = P(hip=(0, 29), lean=7, hN=(8, -7), sword=36, cape=.25)
    l1 = [ready,
          P(hip=(-1, 29.5), lean=1, hN=(-3, 7), hF=(-5, -8), sword=150, cape=.3),
          P(hip=(1, 28.5), lean=10, hN=(7, 6), sword=80, cape=.4, footN=(9, 3)),
          P(hip=(2, 27.5), lean=16, hN=(12, -1), sword=4, cape=.55, footN=(11, 3)),
          P(hip=(2, 27), lean=18, hN=(10, -8), sword=-38, cape=.6, footN=(11, 3)),
          P(hip=(1.5, 27.5), lean=14, hN=(9, -8), sword=-12, cape=.45, footN=(10, 3)),
          P(hip=(.5, 28.5), lean=9, hN=(8, -7), sword=22, cape=.3, footN=(8, 3))]
    l2 = [ready,
          P(hip=(0, 28), lean=12, hN=(5, -11), hF=(-4, -7), sword=-70, cape=.35, footN=(9, 3)),
          P(hip=(1.5, 28), lean=14, hN=(11, -5), sword=-15, cape=.5, footN=(10, 3)),
          P(hip=(2, 28.5), lean=8, hN=(11, 4), sword=40, cape=.6, footN=(10, 3)),
          P(hip=(1.5, 29), lean=2, hN=(6, 10), sword=95, cape=.5, footN=(9, 3)),
          P(hip=(1, 29), lean=4, hN=(7, 3), sword=70, cape=.35, footN=(9, 3)),
          P(hip=(.5, 29), lean=7, hN=(8, -5), sword=42, cape=.28, footN=(8, 3))]
    l3 = [ready,
          P(hip=(-2, 28.5), lean=-2, hN=(-4, -2), hF=(-6, -7), sword=2, cape=.3, footN=(6, 3), footF=(-10, 3)),
          P(hip=(4, 26.5), lean=18, hN=(14, -1), hF=(-7, -9), sword=0, cape=.65, footN=(16, 3), footF=(-9, 3)),
          P(hip=(5, 26), lean=20, hN=(15, -1.5), hF=(-8, -9), sword=-2, cape=.7, footN=(17, 3), footF=(-9, 3)),
          P(hip=(4, 26.5), lean=17, hN=(13, -2), hF=(-7, -9), sword=0, cape=.6, footN=(16, 3), footF=(-9, 3)),
          P(hip=(2, 28), lean=11, hN=(10, -5), sword=18, cape=.4, footN=(11, 3)),
          P(hip=(.5, 28.8), lean=8, hN=(8, -7), sword=30, cape=.28, footN=(8, 3))]
    bounds_light = [0, .12, .27, .38, .52, .66, .82, 1.0]
    A['light1'] = [(f, {}) for f in l1]; A['light2'] = [(f, {}) for f in l2]; A['light3'] = [(f, {}) for f in l3]
    heavy = [ready,
             P(hip=(-1, 29.5), lean=-2, hN=(1, 8), hF=(0, 6), sword=110, twoHand=True, cape=.3),
             P(hip=(-2, 30), lean=-8, hN=(-4, 11), hF=(-3, 9), sword=155, twoHand=True, cape=.25, footN=(8, 3), footF=(-9, 3)),
             P(hip=(-2, 30), lean=-9, hN=(-5, 11), hF=(-4, 9), sword=162, twoHand=True, cape=.25, footN=(8, 3), footF=(-9, 3)),
             P(hip=(2, 27), lean=12, hN=(9, 8), hF=(7, 6), sword=85, twoHand=True, cape=.55, footN=(12, 3), footF=(-10, 3)),
             P(hip=(4, 24), lean=24, hN=(13, -2), hF=(11, -2), sword=8, twoHand=True, cape=.75, footN=(14, 3), footF=(-11, 3)),
             P(hip=(4, 22.5), lean=28, hN=(11, -11), hF=(9, -10), sword=-42, twoHand=True, cape=.8, footN=(14, 3), footF=(-11, 3)),
             P(hip=(3, 24.5), lean=20, hN=(10, -10), hF=(8, -10), sword=-30, twoHand=True, cape=.6, footN=(13, 3), footF=(-10, 3)),
             P(hip=(1, 27.5), lean=11, hN=(9, -8), sword=10, cape=.4, footN=(10, 3)),
             P(hip=(.5, 28.8), lean=8, hN=(8, -7), sword=30, cape=.28, footN=(8, 3))]
    A['heavy'] = [(f, {}) for f in heavy]
    bounds_heavy = [0, .08, .2, .36, .44, .54, .64, .74, .87, 1.0]
    A['dive'] = [(P(hip=(0, 31), lean=10 + 2 * i, footN=(5, 19 - i), footF=(-3, 17 - i), toeN=30, toeF=30, hN=(4, -14), hF=(2, -13), sword=-88, twoHand=True, cape=.05 + .05 * i), {}) for i in range(3)]
    A['spirit'] = [(P(hip=(0, 26), lean=18, footN=(9, 3), footF=(-11, 3), hN=(5, -6), hF=(-6, -8), sword=8, cape=.6), {}),
                   (P(hip=(4, 22), lean=34, footN=(16, 3), footF=(-14, 5), toeF=-20, hN=(16, 0), hF=(-9, -6), sword=0, cape=.95), {}),
                   (P(hip=(4, 22), lean=36, footN=(17, 3), footF=(-15, 6), toeF=-25, hN=(16, -1), hF=(-10, -5), sword=-2, cape=1.0), {}),
                   (P(hip=(2, 26), lean=18, footN=(12, 3), footF=(-10, 3), hN=(11, -5), hF=(-6, -9), sword=10, cape=.6), {})]
    A['block'] = [(P(hip=(-1, 28), lean=2, footN=(8, 3), footF=(-9, 3), hN=(6, 2), hF=(4, -1), sword=78, cape=.2, shieldUp=1.0), {}),
                  (P(hip=(-1, 27.6), lean=3, footN=(8, 3), footF=(-9, 3), hN=(6.5, 2.5), hF=(4.5, -.5), sword=80, cape=.22, shieldUp=1.0), {})]
    A['parry'] = [(P(hip=(-1, 28), lean=0, footN=(8, 3), footF=(-9, 3), hN=(7, 6), hF=(3, -2), sword=108, cape=.3, shieldUp=1.0), {}),
                  (P(hip=(-1.5, 28.5), lean=-3, footN=(8, 3), footF=(-9, 3), hN=(5, 9), hF=(2, -1), sword=125, cape=.35, shieldUp=1.0), {})]
    ball = P(hip=(0, 18), lean=70, footN=(7, 12), footF=(3, 10), toeN=60, toeF=60, hN=(9, -11), hF=(6, -12), sword=-60, cape=.3)
    A['roll'] = [(P(hip=(1, 24), lean=30, footN=(9, 3), footF=(-6, 3), hN=(9, -10), hF=(4, -12), sword=-20, cape=.5), {})] + \
                [(ball | {'rot': -72.0 * i, 'rotC': (1.0, 14.0)}, {}) for i in range(1, 5)] + \
                [(P(hip=(1, 25), lean=24, footN=(8, 3), footF=(-7, 3), hN=(9, -9), hF=(3, -11), sword=10, cape=.6), {})]
    A['backdash'] = [(P(hip=(-2, 26), lean=-4, footN=(5, 5), footF=(-10, 3), toeN=-10, hN=(9, -4), hF=(-7, -7), sword=22, cape=.05), {}),
                     (P(hip=(-1, 25), lean=-2, footN=(7, 3), footF=(-11, 3), hN=(9, -5), hF=(-8, -7), sword=20, cape=.1), {}),
                     (P(hip=(0, 27.5), lean=6, footN=(7, 3), footF=(-9, 3), hN=(8.5, -6), hF=(-4, -9), sword=30, cape=.2), {})]
    A['hurt'] = [(P(hip=(-2, 28), lean=-16, head=-14, footN=(8, 3), footF=(-9, 3), hN=(3, 2), hF=(-8, 0), sword=95, cape=.15), {}),
                 (P(hip=(-1.5, 28), lean=-12, head=-9, footN=(8, 3), footF=(-9, 3), hN=(5, -2), hF=(-7, -3), sword=70, cape=.2), {}),
                 (P(hip=(-.5, 28.5), lean=-2, head=-3, footN=(8, 3), footF=(-9, 3), hN=(7, -5), hF=(-4, -8), sword=45, cape=.25), {})]
    A['dead'] = [(P(hip=(-1, 25), lean=-14, head=-20, footN=(8, 3), footF=(-8, 3), hN=(4, -9), hF=(-7, -6), sword=-60, cape=.1), {}),
                 (P(hip=(-3, 20), lean=-30, head=-25, footN=(9, 3), footF=(-6, 3), hN=(2, -10), hF=(-8, -5), sword=-80, cape=.15, rot=20, rotC=(0, 3)), {}),
                 (P(hip=(-5, 16), lean=-35, head=-20, footN=(10, 3), footF=(-4, 3), hN=(-2, -8), hF=(-9, -4), sword=-100, cape=.2, rot=50, rotC=(0, 3)), {}),
                 (P(hip=(-2, 30), lean=0, head=-8, footN=(5, 3), footF=(-4, 3), hN=(-3, -12), hF=(-6, -10), sword=-110, cape=.05, rot=88, rotC=(0, 3)), {}),
                 (P(hip=(-2, 30), lean=2, head=-10, footN=(5, 3), footF=(-3, 3), hN=(-1, -13), hF=(-6, -11), sword=-100, cape=.0, rot=90, rotC=(0, 2.5)), {})]
    A['heal'] = [(P(hip=(0, 28.5), lean=6, hN=(7, -8), hF=(1, -12), sword=30, cape=.25, item='potion'), {}),
                 (P(hip=(0, 29), lean=3, hN=(7, -8), hF=(3, -3), sword=30, cape=.25, item='potion'), {}),
                 (P(hip=(0, 29.5), lean=-4, head=-18, hN=(6.5, -8), hF=(3, 4), sword=32, cape=.25, item='potion', itemAngle=-60), {}),
                 (P(hip=(0, 29.5), lean=-5, head=-22, hN=(6.5, -8), hF=(3, 5), sword=32, cape=.25, item='potion', itemAngle=-80), {}),
                 (P(hip=(0, 29), lean=2, head=-6, hN=(7, -8), hF=(1, -4), sword=30, cape=.25, item='potion'), {}),
                 (P(hip=(0, 29), lean=6, hN=(8, -7), hF=(-2, -10), sword=36, cape=.25), {})]
    if hero:
        A['shoot'] = [(P(hip=(0, 29), lean=4, hN=(9, -3), hF=(-3, -9), swordOn=False, item='pistol', itemAngle=-20, cape=.25), {}),
                      (P(hip=(-.5, 29), lean=-1, hN=(13, 1), hF=(-3, -9), swordOn=False, item='pistol', itemAngle=0, cape=.25), {}),
                      (P(hip=(-1, 29), lean=-4, hN=(12, 4), hF=(-3, -9), swordOn=False, item='pistol', itemAngle=16, cape=.3), {}),
                      (P(hip=(-.5, 29), lean=-1, hN=(12.5, 2), hF=(-3, -9), swordOn=False, item='pistol', itemAngle=6, cape=.28), {}),
                      (P(hip=(0, 29), lean=4, hN=(9, -4), hF=(-3, -9), swordOn=False, item='pistol', itemAngle=-25, cape=.25), {})]
    else:
        A['shoot'] = [(P(hip=(0, 29), lean=2, hN=(3, -6), hF=(9, 2), swordOn=False, bow=0.0, cape=.2), {}),
                      (P(hip=(0, 29), lean=0, hN=(2, 1), hF=(11, 2), swordOn=False, bow=.3, cape=.2), {}),
                      (P(hip=(-.5, 29), lean=-2, hN=(-3, 2), hF=(12, 2), swordOn=False, bow=.75, cape=.2), {}),
                      (P(hip=(-.5, 29), lean=-2, hN=(-4, 2), hF=(12, 2), swordOn=False, bow=.85, cape=.2), {}),
                      (P(hip=(0, 29), lean=1, hN=(-6, 3), hF=(12, 2.5), swordOn=False, bow=0.0, cape=.2), {}),
                      (P(hip=(0, 29), lean=2, hN=(2, -5), hF=(9, 1), swordOn=False, bow=0.0, cape=.2), {})]
    # Asılma ve tırmanma: eller kenarın köşesinde (dayanaktan +7.5, +44.5 piksel). Pozun elleri mutlak
    # hedeflere yerleştirilir; GameWorld.Climb'ın dayanak yolu burada aynen hesaplanır.
    def hang_pose(sway):
        q = P(hip=(.5 + sway, 9.5), lean=-4 + sway * 2, head=-10, footN=(2 + sway, -19), footF=(-2 + sway, -18), toeN=-35, toeF=-40, sword=-95, cape=.0)
        return reach(q, variant, (6.0, 43.6), (4.0, 43.4))
    A['hang'] = [(hang_pose(0.0), {}), (hang_pose(.8), {})]
    climb = []
    for p_ in (.08, .25, .42, .58, .75, .92):
        rise = smooth(min(1, p_ / .7)); over = smooth(max(0, min(1, (p_ - .3) / .6)))
        lx, ly = (.3 - .58 * over) * UNIT, 1.78 * (1 - rise) * UNIT
        if p_ <= .45:
            u = smooth(p_ / .45)
            q = P(hip=(lx - 6, ly - 34 + 17 * u), lean=-4 + 16 * u, head=-8, footN=(lx - 3, ly - 54 + 22 * u), footF=(lx - 8, ly - 53 + 20 * u),
                  toeN=-30, toeF=-30, sword=-95, cape=.1)
            q = reach(q, variant, (lx - 1.5, ly - 1), (lx - 3.5, ly - 1.2))
        elif p_ <= .7:
            v = smooth((p_ - .45) / .25)
            q = P(hip=(lx - 4 + 4 * v, ly - 12 + 8 * v), lean=30 - 4 * v, head=-4, footN=(lx + 3, ly + 3), footF=(lx - 8, ly - 12), toeF=-25, sword=-70, cape=.25)
            q = reach(q, variant, (lx + 3, ly + 1), (lx + 1, ly + 1))
        else:
            t = smooth((p_ - .7) / .3)
            q = P(hip=(lx * (1 - t) - 1 * t, (ly - 4) * (1 - t) + 29 * t), lean=24 - 17 * t, head=-4,
                  footN=(lx + 3 + (7 - lx - 3) * t, ly + 3 + (3 - ly - 3) * t), footF=(lx - 4 + (-8 - lx + 4) * t, ly + 3 + (3 - ly - 3) * t),
                  hN=(6 + 2 * t, -12 + 5 * t), hF=(2 - 4 * t, -12 + 2 * t), sword=-50 + 86 * t, cape=.2)
        climb.append((q, {}))
    A['climb'] = climb
    timing = {'light1': bounds_light, 'light2': bounds_light, 'light3': bounds_light, 'heavy': bounds_heavy}
    strides = {'run': run_stride, 'walk': walk_stride}
    return A, timing, strides


# ----------------------------------------------------------------------------------------
# Çıktı
# ----------------------------------------------------------------------------------------

VARIANTS = ['hero', 'duelist', 'shield', 'archer', 'captain', 'ghoul']


def build(variant, preview=True):
    ramps = ramps_for(variant)
    anims, timing, strides = make_animations(variant)
    if variant != 'hero':
        for name in ('flip', 'roll', 'backdash', 'spirit', 'dive', 'heal', 'skid', 'hang', 'climb', 'light2', 'light3', 'parry'):
            anims.pop(name, None)
        if variant != 'captain':
            anims.pop('heavy', None)
    frames, records, clips = [], [], []
    for name, seq in anims.items():
        start = len(frames)
        for pose, extra in seq:
            rgba, meta, sk = render_pose(pose, variant, ramps)
            frames.append(rgba)
            rec = {'bladeRoot': [round(meta['bladeRoot'][0], 2), round(meta['bladeRoot'][1], 2)],
                   'bladeTip': [round(meta['bladeTip'][0], 2), round(meta['bladeTip'][1], 2)],
                   'plantedN': -999.0 if extra.get('planted', [None, None])[0] is None else round(extra['planted'][0] * (1.0 if variant == 'hero' else (1.09 if variant == 'captain' else .98)), 3),
                   'plantedF': -999.0 if extra.get('planted', [None, None])[1] is None else round(extra['planted'][1] * (1.0 if variant == 'hero' else (1.09 if variant == 'captain' else .98)), 3)}
            records.append(rec)
        clip = {'name': name, 'start': start, 'count': len(seq), 'bounds': timing.get(name, []), 'stride': 0.0}
        if name in strides:
            k = 1.09 if variant == 'captain' else (.98 if variant != 'hero' else 1.0)
            clip['stride'] = strides[name] * k
        clips.append(clip)
    rows = (len(frames) + COLUMNS - 1) // COLUMNS
    sheet = np.zeros((rows * H, COLUMNS * W, 4), np.uint8)
    for i, f in enumerate(frames):
        r, c = divmod(i, COLUMNS)
        sheet[r * H:(r + 1) * H, c * W:(c + 1) * W] = f
    os.makedirs(OUT, exist_ok=True)
    Image.fromarray(sheet, 'RGBA').save(os.path.join(OUT, variant + '.png'))
    data = {'frameWidth': W, 'frameHeight': H, 'pivotX': PX, 'pivotY': PY, 'columns': COLUMNS, 'pixelsPerUnit': UNIT,
            'frames': records, 'clips': clips}
    with open(os.path.join(OUT, variant + '.json'), 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False)
    if preview:
        os.makedirs(REVIEW, exist_ok=True)
        bg = np.zeros_like(sheet); bg[..., :3] = (58, 64, 78); bg[..., 3] = 255
        a = sheet[..., 3:4] / 255.0
        comp = (sheet[..., :3] * a + bg[..., :3] * (1 - a)).astype(np.uint8)
        img = Image.fromarray(comp, 'RGB')
        img = img.resize((img.width * 2, img.height * 2), Image.NEAREST)
        img.save(os.path.join(REVIEW, variant + '-sheet.png'))
    return len(frames)


if __name__ == '__main__':
    only = sys.argv[1:] or VARIANTS
    for v in only:
        n = build(v)
        print(v, n, 'frames')
