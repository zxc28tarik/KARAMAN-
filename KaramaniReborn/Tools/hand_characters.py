"""Beş oynanabilir karakterin elle çizilmiş örnek kareleri (bekleme duruşu, sağa bakış).

Işık sağ üst önden; arka (sol) kenarda soğuk ışık; dış çizgi seçici. Ölçü: 1 piksel = 1 oyun pikseli
(Karamani ~61 satır, oyundaki gerçek boy). Karamani CHAR-01 şartnamesine göre çizilir
(Docs/specs/CHAR-01-karamani-sprite-spec.md). Çalıştır: python -I Tools/hand_characters.py [ad ...]
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from hand_sprites import Sprite, preview, REVIEW  # noqa: E402

HERO = {
    'a': ('h_red', 0), 'b': ('h_red', 1), 'c': ('h_red', 2), 'd': ('h_red', 3), 'D': ('h_red', 4),
    '5': ('h_skin', 0), '6': ('h_skin', 1), '7': ('h_skin', 2), '8': ('h_skin', 3),
    'h': ('h_beard', 0), 'm': ('h_beard', 1), 'n': ('h_beard', 2), 'M': ('h_beard', 3), 'N': ('h_beard', 4),
    'w': ('h_ivory', 0), '1': ('h_ivory', 1), '2': ('h_ivory', 2), '3': ('h_ivory', 3), '4': ('h_ivory', 4),
    'o': ('h_bronze', 0), 'p': ('h_bronze', 1), 'q': ('h_bronze', 2), 'Q': ('h_bronze', 3), 'B': ('h_bronze', 4),
    's': ('h_steel', 0), 't': ('h_steel', 1), 'u': ('h_steel', 2), 'U': ('h_steel', 3), 'V': ('h_steel', 4),
    'i': ('h_mail', 0), 'j': ('h_mail', 1), 'k': ('h_mail', 2), 'K': ('h_mail', 3),
    'l': ('h_leather', 0), 'L': ('h_leather', 1), 'r': ('h_leather', 2), 'R': ('h_leather', 3), 'T': ('h_leather', 4),
    'X': ('h_blade', 0), 'x': ('h_blade', 1), 'y': ('h_blade', 2), 'z': ('h_blade', 3), 'Z': ('h_blade', 4),
    'E': ('eye', 0), 'F': ('rim', 2),
}


def curved_blade(s, guard, angle_deg, length, sagitta, mat, yelman=7):
    """Kıvrık kılıç ağzı: kiriş boyunca, kesen ağız dış bükey yanda. Sırt 1-2 tonu, ağız 3, ışık çizgisi 4."""
    a = math.radians(angle_deg)
    d = (math.cos(a), -math.sin(a))          # ekran: y aşağı
    n_edge = (-d[1], d[0])                   # ağız tarafı (aşağı-ön)
    if n_edge[1] < 0:
        n_edge = (-n_edge[0], -n_edge[1])
    n_spine = (-n_edge[0], -n_edge[1])
    steps = int(length * 4)
    edge, spine, extra = [], [], []
    for i in range(steps + 1):
        t = i / steps
        bow = sagitta * math.sin(math.pi * min(1.0, t / 1.3)) if t < .65 else sagitta * (1 - (t - .65) / .35 * .6)
        cx = guard[0] + d[0] * length * t + n_edge[0] * bow
        cy = guard[1] + d[1] * length * t + n_edge[1] * bow
        e = (round(cx), round(cy))
        sp = (round(cx + n_spine[0]), round(cy + n_spine[1]))
        if e not in edge:
            edge.append(e)
        if t < .97 and sp not in spine and sp not in edge:
            spine.append(sp)
        if length * (1 - t) < yelman and .1 < length * (1 - t) < yelman - 1:
            ex = (round(cx + 2 * n_spine[0]), round(cy + 2 * n_spine[1]))
            if ex not in extra:
                extra.append(ex)
    for p in extra:
        s.px([p], mat, 1)
    for i, p in enumerate(spine):
        s.px([p], mat, 2 if i % 7 < 5 else 1)
    for i, p in enumerate(edge):
        s.px([p], mat, 4 if (i // 3) % 2 == 0 and i > 2 else 3)
    return edge[-1]


def karamani():
    s = Sprite(60, 64)
    R, BR, ST, ML, LE = 'h_red', 'h_bronze', 'h_steel', 'h_mail', 'h_leather'

    # 1) ana pelerin (en arkada): çoğu gölgede, dört kıvrım sırtı, yırtık etek, dış kenarda soğuk ışık
    cape = {18: (14, 17), 19: (13, 17), 20: (12, 17), 21: (12, 17), 22: (11, 17), 23: (11, 17), 24: (10, 17), 25: (10, 17),
            26: (9, 17), 27: (9, 17), 28: (8, 17), 29: (8, 17), 30: (8, 16), 31: (8, 16), 32: (7, 16), 33: (7, 16),
            34: (6, 16), 35: (6, 16), 36: (6, 15), 37: (6, 15), 38: (5, 15), 39: (5, 15), 40: (5, 15), 41: (5, 15),
            42: (4, 15), 43: (4, 14), 44: (4, 14)}
    for y, (x0, x1) in cape.items():
        s.span(y, x0, x1, R, 1, back=0, front=2, back_w=1, front_w=2)
    for (x0, y0), (x1, y1) in (((16, 21), (14, 44)), ((14, 22), (11, 44)), ((12, 25), (8, 44)), ((10, 29), (6, 43))):
        steps = y1 - y0
        for i in range(steps + 1):
            y = y0 + i; x = round(x0 + (x1 - x0) * i / steps)
            s.px([(x, y)], R, 3 if i < 6 else 2)              # kıvrım sırtı (üstte ışık alır)
            s.px([(x - 1, y)], R, 0 if i > 3 else 1)          # sırtın gölge yanı
    s.px([(4, 44), (5, 44), (6, 44), (7, 44)], R, 0)          # astar (dönen etek)
    s.erase([(9, 44), (10, 44)])
    s.px([(5, 45), (6, 45), (11, 45), (12, 45), (13, 45), (12, 46)], R, 1)
    s.px([(11, 23), (11, 22), (10, 24), (10, 25)], 'rim', 2)
    s.px([(15, 22), (16, 22), (17, 22), (14, 23)], R, 0)      # manto eteğinin gölgesi

    # 2) kaftanın arka eteği (arka bacağın arkasında)
    for y, (x0, x1) in {35: (15, 21), 36: (15, 21), 37: (15, 21), 38: (15, 21), 39: (14, 21), 40: (14, 20), 41: (14, 20), 42: (14, 19), 43: (14, 19), 44: (15, 18)}.items():
        s.span(y, x0, x1, R, 1, back=0, front=2)
    s.px([(16, 40), (16, 41), (17, 43)], R, 0)

    # 3) arka bacak (bir ton koyu)
    for y, (x0, x1) in {37: (17, 21), 38: (17, 21), 39: (17, 21), 40: (17, 21), 41: (16, 20), 42: (16, 19)}.items():
        s.span(y, x0, x1, R, 1, back=0, front=2)
    for y, (x0, x1) in {45: (14, 18), 46: (14, 18), 47: (14, 18), 48: (14, 17), 49: (14, 17), 50: (14, 17), 51: (14, 17),
                        52: (14, 16), 53: (14, 16), 54: (14, 16), 55: (14, 16), 56: (14, 16), 57: (14, 16)}.items():
        s.span(y, x0, x1, ST, 1, back=0, front=2)
    s.ascii({42: [(16, "pq")], 43: [(15, "pqqp")], 44: [(15, "oppp")], 45: [(16, "oo")]}, HERO)
    for y, (x0, x1) in {57: (13, 16), 58: (12, 17), 59: (12, 18), 60: (12, 19), 61: (12, 19)}.items():
        s.span(y, x0, x1, LE, 1, back=0, front=2)
    s.span(57, 13, 16, LE, 2, front=3); s.span(61, 12, 19, LE, 0)

    # 4) arka kol (gövdenin arkasında; görünen ön kol ve el)
    for y in range(17, 23):
        s.span(y, 16, 18, ML, 1, back=0)
    for y in range(23, 29):
        s.span(y, 16, 18, ST, 1, back=0, front=2)
    s.ascii({29: [(16, "LrL")], 30: [(16, "Lrr")], 31: [(17, "LL")]}, HERO)

    # 5) gövde: göğüste bronz lamel zırh (2 piksel plaka + 1 piksel kızıl bağ); bele doğru daralır
    torso = {15: (21, 24), 16: (19, 26), 17: (18, 27), 18: (18, 28), 19: (18, 28), 20: (18, 28), 21: (18, 28),
             22: (19, 28), 23: (19, 27), 24: (19, 27)}
    lacing = {18, 21}
    tops = {16, 19, 22}
    seams = {16: (21, 24), 17: (21, 24), 19: (22, 25), 20: (22, 25), 22: (21, 24), 23: (21, 24), 24: (22, 25)}
    for y, (x0, x1) in torso.items():
        if y in lacing:
            s.span(y, x0, x1, BR, 1, back=0, back_w=3)
            s.px([(23, y), (26, y)], R, 2)
        elif y in tops:
            s.span(y, x0, x1, BR, 2, back=1, back_w=3, front=3, front_w=3)
        else:
            s.span(y, x0, x1, BR, 2, back=1, back_w=3, front=2)
        for x in seams.get(y, ()):
            if s.tone[y, x] >= 2:
                s.px([(x, y)], BR, s.tone[y, x] - 1)
    s.px([(27, 16), (28, 19)], BR, 4)
    s.span(24, 19, 27, BR, 1, back=0)                                     # zırhın alt kenarı gölgede

    # 6) omuz mantosu (sırtın üstünde kızıl kısa pelerin)
    mantle = {14: (18, 21), 15: (17, 22), 16: (16, 22), 17: (16, 22), 18: (16, 21), 19: (16, 21), 20: (16, 20), 21: (17, 19)}
    for y, (x0, x1) in mantle.items():
        s.span(y, x0, x1, R, 2, back=1, back_w=2, front=3 if y < 17 else 2)
    s.span(21, 17, 19, R, 1, back=0)
    s.px([(19, 16), (19, 17), (18, 18), (18, 19)], R, 1)                 # kıvrım
    s.px([(16, 16), (16, 17), (17, 15)], 'rim', 2)

    # 7) ön bacak: kızıl şalvar, 4×4 bronz dizlik, baldırı belli incik zırhı, kıvrık burunlu çizme
    for y, (x0, x1) in {33: (23, 28), 34: (23, 28), 35: (23, 29), 36: (23, 29), 37: (24, 29), 38: (24, 29), 39: (24, 29),
                        40: (25, 29), 41: (25, 29)}.items():
        s.span(y, x0, x1, R, 2, back=1, back_w=2, front=3)
    s.px([(26, 36), (26, 37), (27, 39), (27, 40)], R, 1); s.px([(28, 37), (29, 38)], R, 4)   # şalvar kıvrımı, ışıklı sırt
    for y, (x0, x1) in {46: (25, 29), 47: (25, 29), 48: (25, 29), 49: (25, 29), 50: (25, 28), 51: (25, 28), 52: (26, 28),
                        53: (26, 28), 54: (26, 28), 55: (26, 28), 56: (26, 28), 57: (26, 28)}.items():
        s.span(y, x0, x1, ST, 3, back=1, front=3, front_w=2 if x1 - x0 >= 3 else 1)
        s.px([(x0 + 1, y)], ST, 2)
    s.px([(29, 47), (29, 48), (28, 53), (28, 54)], ST, 4); s.span(51, 25, 28, ST, 1); s.px([(28, 51)], ST, 2)   # ışık çizgisi, incik bandı
    s.ascii({42: [(26, "pQQp")], 43: [(26, "qQBq")], 44: [(26, "pqqq")], 45: [(26, "oppp")]}, HERO)
    for y, (x0, x1) in {57: (25, 29), 58: (25, 30), 59: (25, 31), 60: (25, 32), 61: (25, 32)}.items():
        s.span(y, x0, x1, LE, 2, back=1, front=3)
    s.span(57, 25, 29, LE, 3, back=2); s.span(61, 25, 32, LE, 0); s.px([(32, 59)], LE, 3); s.px([(32, 60)], LE, 2)

    # 8) kuşak ve iki ayrı etek plakası (arada zincir): önde kısa, arkada uzun
    for y, (x0, x1) in {25: (19, 27), 26: (19, 28), 27: (19, 28)}.items():
        s.span(y, x0, x1, R, 2, back=1, back_w=2, front=3)
    s.px([(21, 26), (24, 26)], R, 1)
    for y, (x0, x1) in {28: (19, 27), 29: (19, 27), 30: (18, 27), 31: (18, 27), 32: (18, 22)}.items():
        s.span(y, x0, x1, ML, 1, back=0)                                  # plakaların altındaki zincir
    s.px([(20, 29), (22, 29), (24, 29), (26, 29), (21, 31), (23, 31), (25, 31)], ML, 2)
    front_tasset = {28: (23, 28), 29: (23, 28), 30: (23, 29), 31: (24, 29), 32: (24, 29), 33: (24, 29), 34: (25, 29)}
    back_tasset = {28: (17, 21), 29: (17, 21), 30: (16, 21), 31: (16, 21), 32: (16, 21), 33: (16, 21), 34: (16, 21), 35: (16, 21), 36: (17, 20)}
    for plate in (back_tasset, front_tasset):
        ys = sorted(plate)
        for y in ys:
            x0, x1 = plate[y]
            lace = (y - ys[0]) % 3 == 2
            if lace:
                s.span(y, x0, x1, BR, 1, back=0)
                s.px([((x0 + x1) // 2, y)], R, 2)
            elif (y - ys[0]) % 3 == 0:
                s.span(y, x0, x1, BR, 2, back=1, front=3, front_w=2)
            else:
                s.span(y, x0, x1, BR, 2, back=1, front=2)
        s.span(ys[-1], plate[ys[-1]][0], plate[ys[-1]][1], BR, 1, back=0)
    s.px([(28, 27), (29, 27), (29, 28)], R, 3); s.px([(29, 29), (29, 30)], R, 2); s.px([(30, 30)], R, 1)   # kuşak düğümü, sarkan uç

    # 9) baş: sarık kuyruğu, zincir boyunluk, yüz, sakal, sarık (şartnamedeki piksel taslağı)
    s.ascii({5: [(18, "c")], 6: [(17, "dc")], 7: [(17, "cb")], 8: [(16, "cb")], 9: [(16, "cb")], 10: [(15, "cb")],
             11: [(15, "ba")], 12: [(14, "b")]}, HERO)
    s.ascii({13: [(20, "jj")], 14: [(19, "jkj")], 15: [(20, "jkjkj")]}, HERO)
    s.ascii({
        2: [(24, "bd")],
        3: [(22, "1bbdd3")],
        4: [(20, "122333443")],
        5: [(19, "11112233qB")],
        6: [(19, "11122233qq")],
        7: [(20, "mm666667")],
        8: [(20, "mm767E88")],
        9: [(20, "mm7778888")],
        10: [(20, "mmm776mm6")],
        11: [(20, "mmmm7mMMmm")],
        12: [(20, "mmmmmmMmm")],
        13: [(21, "jmmmMmm")],
        14: [(21, "jKjmmm")],
        15: [(24, "mm")],
    }, HERO)

    # 10) ön kol: iki katlı omuzluk, zincir yen, çelik kolluk (gövdeyle arasında hava boşluğu)
    s.ascii({14: [(24, "QQBQ")], 15: [(23, "pqqqQQq")], 16: [(23, "ppppppp")],
             17: [(24, "qqqQQQq")], 18: [(24, "pqqqqQq")], 19: [(25, "ppppp")]}, HERO)
    for y, (x0, x1) in {20: (26, 29), 21: (26, 29), 22: (27, 30), 23: (27, 30)}.items():
        s.span(y, x0, x1, ML, 2 if y % 2 == 0 else 1, back=1 if y % 2 == 0 else 0, front=3 if y % 2 == 0 else 2)
    for y, (x0, x1) in {20: (32, 33), 21: (31, 33), 22: (30, 33), 23: (29, 32), 24: (28, 31), 25: (28, 30)}.items():
        s.span(y, x0, x1, ST, 2, back=1, front=3)
    s.px([(32, 20), (33, 20), (31, 21)], ST, 4); s.px([(28, 25), (29, 25)], ST, 1); s.px([(30, 25)], ST, 0)

    # 11) kılıç: balçak, kıvrık ağız ve yelman, kabza ipi ve topuz
    tip = curved_blade(s, (37.5, 19.0), 33, 24, 3.0, 'h_blade')
    s.ascii({16: [(36, "Q")], 17: [(36, "q")], 18: [(37, "q")], 19: [(37, "Q")], 20: [(38, "p")]}, HERO)
    s.ascii({22: [(34, "c")], 23: [(33, "bc")], 24: [(32, "QB")]}, HERO)

    # 12) yumruk (kabzayı kavrar): parmak boğumları ışıkta, başparmak üstte
    s.ascii({18: [(34, "RR")], 19: [(33, "rRRT")], 20: [(34, "Lrrr")], 21: [(34, "LLr")]}, HERO)
    return s.outline()


LUD = {
    '5': ('fair', 0), '6': ('fair', 1), '7': ('fair', 2), '8': ('fair', 3), '9': ('fair', 4),
    'a': ('blond', 0), 'b': ('blond', 1), 'c': ('blond', 2), 'd': ('blond', 3), 'e': ('blond', 4),
    's': ('plate', 0), 't': ('plate', 1), 'u': ('plate', 2), 'v': ('plate', 3), 'V': ('plate', 4),
    'g': ('navy', 0), 'h': ('navy', 1), 'i': ('navy', 2), 'j': ('navy', 3), 'J': ('navy', 4),
    'w': ('red', 0), 'q': ('red', 1), 'r': ('red', 2), 'R': ('red', 3), 'Q': ('red', 4),
    'k': ('wood', 0), 'l': ('wood', 1), 'L': ('wood', 2), 'm': ('wood', 3),
    'O': ('bronze', 0), 'o': ('bronze', 1), 'p': ('bronze', 2), 'P': ('bronze', 3), 'B': ('bronze', 4),
    'X': ('blade', 0), 'x': ('blade', 1), 'y': ('blade', 2), 'z': ('blade', 3), 'Z': ('blade', 4),
    'E': ('eye', 0), 'F': ('rim', 2),
}


def lames(s, rows, mat, every, start, mid=2, back=1, back_w=2, front=3, front_w=2, line=1):
    """Plaka dizisi: her `every` satırda bir koyu ek yeri; her plakanın üst satırının ön kenarı ışıkta."""
    for y, (x0, x1) in rows.items():
        k = (y - start) % every
        if k == every - 1:
            s.span(y, x0, x1, mat, line, back=max(0, line - 1))
        elif k == 0:
            s.span(y, x0, x1, mat, mid, back=back, back_w=back_w, front=front, front_w=front_w)
        else:
            s.span(y, x0, x1, mat, mid, back=back, back_w=back_w, front=mid + (1 if front_w > 2 else 0))


def ludwig():
    """Ludwig von Rosenstein: genç Alman şövalye, tank. Cilalı plaka omuzluk ve bacaklar, lacivert savaş
    cübbesi, göğüste kızıl gül arması, sarı saç; kılıcı önünde yere saplı, iki eli kabzada (ağır, kararlı)."""
    s = Sprite(56, 64)
    PL, NV, BW = 'plate', 'navy', 'brown'
    # arka bacak
    for y, (x0, x1) in {36: (18, 23), 37: (18, 23), 38: (17, 22), 39: (17, 22), 40: (16, 21), 41: (16, 21), 42: (16, 21)}.items():
        s.span(y, x0, x1, PL, 1, back=0, front=2)
    s.ascii({43: [(15, "tuut")], 44: [(14, "tuvut")], 45: [(15, "sttt")], 46: [(16, "ss")]}, LUD)
    for y in range(47, 57):
        s.span(y, 15, 19 if y < 51 else 18, PL, 1, back=0, front=2)
    for y, (x0, x1) in {57: (14, 19), 58: (13, 20), 59: (13, 21), 60: (13, 21), 61: (13, 21)}.items():
        s.span(y, x0, x1, PL, 1, back=0, front=2)
    s.span(58, 14, 19, PL, 0); s.span(61, 13, 21, PL, 0)
    # ön bacak: bacak plakası, yelpazeli dizlik, incik, lamlı demir ayakkabı
    for y, (x0, x1) in {36: (25, 30), 37: (25, 30), 38: (25, 30), 39: (25, 30), 40: (26, 30), 41: (26, 31), 42: (26, 31)}.items():
        s.span(y, x0, x1, PL, 2, back=1, front=3, front_w=2)
    s.px([(29, 39), (29, 40), (30, 41)], PL, 4)
    s.ascii({43: [(25, "suvvu")], 44: [(24, "stuvVvu")], 45: [(25, "tuuuv")], 46: [(26, "sss")]}, LUD)
    for y in range(47, 57):
        x1 = 30 if y < 51 else 29
        s.span(y, 26, x1, PL, 2, back=1, front=3, front_w=2)
    s.px([(29, 48), (29, 49), (29, 50)], PL, 4); s.span(53, 26, 29, PL, 1)
    for y, (x0, x1) in {57: (26, 30), 58: (25, 32), 59: (25, 33), 60: (25, 34), 61: (25, 34)}.items():
        s.span(y, x0, x1, PL, 2, back=1, front=3)
    s.span(58, 26, 31, PL, 1); s.span(61, 25, 34, PL, 0); s.px([(33, 59), (34, 60)], PL, 4)
    # tabanca kabzası (arka kalçada; iki tabancadan biri)
    s.ascii({27: [(15, "LL")], 28: [(14, "lLm")], 29: [(14, "Pl")], 30: [(14, "P")]}, LUD)
    # arka omuzluk (gövdenin arkasında, koyu)
    for y, (x0, x1) in {12: (17, 21), 13: (15, 22), 14: (14, 22), 15: (14, 22), 16: (14, 22), 17: (14, 21), 18: (15, 21), 19: (16, 20)}.items():
        s.span(y, x0, x1, PL, 1, back=0, front=2)
    s.span(15, 14, 21, PL, 0); s.span(18, 15, 20, PL, 0)
    # lacivert savaş cübbesi: gövde, ortadan yırtmaçlı etek
    tab = {14: (20, 27), 15: (19, 28), 16: (18, 29), 17: (18, 30), 18: (18, 30), 19: (18, 30), 20: (18, 30), 21: (18, 30),
           22: (18, 30), 23: (18, 30), 24: (19, 29), 25: (19, 29)}
    for y, (x0, x1) in tab.items():
        s.span(y, x0, x1, NV, 2, back=1, back_w=3, front=3, front_w=2)
    s.px([(18, 17), (18, 18), (18, 19)], 'rim', 2)
    skirt = {28: (18, 30), 29: (18, 30), 30: (17, 31), 31: (17, 31), 32: (17, 31), 33: (16, 31), 34: (16, 32), 35: (16, 32),
             36: (16, 32), 37: (17, 31), 38: (17, 30)}
    for y, (x0, x1) in skirt.items():
        s.span(y, x0, x1, NV, 2, back=1, back_w=2, front=3)
    for y in range(30, 39):
        s.px([(24, y)], NV, 0); s.px([(25, y)], NV, 1)                     # ön yırtmaç
    for x, y0 in ((20, 31), (28, 31)):
        s.line(x, y0, x - 1, 38, NV, 1); s.px([(x + 1, y0)], NV, 3)
    s.px([(18, 38), (22, 38), (27, 38), (30, 38)], NV, 1)
    # kızıl gül arması (göğüste)
    s.ascii({17: [(23, "qRq")], 18: [(22, "qRQRq")], 19: [(22, "rRrRr")], 20: [(23, "qrq")]}, LUD)
    s.px([(24, 21)], 'green', 2); s.px([(25, 22)], 'green', 1)
    # kemer ve toka
    for y in (26, 27):
        s.span(y, 18, 30, BW, 2 if y == 26 else 1, back=1 if y == 26 else 0, front=3 if y == 26 else 2)
    s.ascii({26: [(27, "PB")], 27: [(27, "op")]}, LUD)
    # boyunluk
    for y, (x0, x1) in {11: (23, 26), 12: (22, 27), 13: (22, 27)}.items():
        s.span(y, x0, x1, PL, 2, back=1, front=3)
    s.span(12, 22, 27, PL, 0)
    # baş: kısa sarı saç, genç temiz traşlı yüz (alın, kaş çıkıntısı, geride göz, burun ucu, dudak, çene)
    s.ascii({
        1: [(22, "bccd")],
        2: [(21, "bccdde")],
        3: [(20, "bbccdddd")],
        4: [(20, "bbbccdde7")],
        5: [(20, "bbbcc6778")],
        6: [(20, "abb67778 8")],
        7: [(20, "ab6667E78")],
        8: [(21, "b6767778")],
        9: [(21, "b66777789")],
        10: [(22, "6677768")],
        11: [(23, "667767")],
        12: [(24, "6677")],
    }, LUD)
    s.px([(28, 6)], 'fair', 3); s.px([(26, 6), (27, 6)], 'fair', 1)        # kaş çıkıntısı ve altındaki gölge
    s.px([(23, 7), (23, 8)], 'fair', 1); s.px([(24, 8)], 'fair', 2)           # kulak
    s.px([(27, 5)], 'blond', 3)                                            # alına düşen perçem
    # ön omuzluk: üç lam, aralarında koyu ayrım, ön kenarlarda parıltı
    paul = {11: (25, 30), 12: (24, 32), 13: (23, 33), 14: (23, 33), 15: (23, 33), 16: (24, 33), 17: (24, 33), 18: (25, 32), 19: (26, 32), 20: (27, 31)}
    for y, (x0, x1) in paul.items():
        s.span(y, x0, x1, PL, 2, back=1, back_w=2, front=3, front_w=3)
    for y, (x0, x1) in {14: (24, 32), 17: (25, 32), 20: (27, 31)}.items():
        s.span(y, x0, x1, PL, 0)
    s.px([(29, 11), (30, 12), (32, 13), (32, 15), (32, 16)], PL, 4)
    # kollar: ikisi de önde kabzaya uzanır (arka kol önce, koyu)
    for y, (x0, x1) in {28: (27, 31), 29: (28, 32), 30: (29, 32)}.items():
        s.span(y, x0, x1, PL, 1, back=0, front=2)
    for y, (x0, x1) in {21: (28, 31), 22: (28, 31), 23: (28, 31), 24: (28, 31)}.items():
        s.span(y, x0, x1, PL, 2, back=1, front=3)
    s.ascii({25: [(28, "tuvP")], 26: [(28, "tuvv")]}, LUD)                   # dirsek kapağı ve perçin
    for y, (x0, x1) in {26: (31, 32), 27: (30, 33), 28: (31, 33)}.items():
        s.span(y, x0, x1, PL, 2, back=1, front=3)
    # kılıç: yere saplı dikey ağız, balçak; topuz eldivenlerin üstünde
    for y in range(32, 60):
        s.px([(34, y)], 'blade', 3 if y % 6 else 4); s.px([(35, y)], 'blade', 2)
    s.px([(34, 60)], 'blade', 3)
    s.ascii({31: [(30, "stuvvvvuu")]}, LUD)
    s.ascii({24: [(34, "PB")], 25: [(34, "op")]}, LUD)
    # eldivenler kabzayı kavrar: boğumlar ışıkta
    s.ascii({26: [(33, "uvvV")], 27: [(32, "tuvvvv")], 28: [(32, "tuuvvu")], 29: [(32, "ttuuu")], 30: [(33, "ttt")]}, LUD)
    return s.outline()


ALM = {
    '5': ('pale', 0), '6': ('pale', 1), '7': ('pale', 2), '8': ('pale', 3), '9': ('pale', 4),
    'a': ('black', 0), 'b': ('black', 1), 'c': ('black', 2), 'd': ('black', 3), 'e': ('black', 4),
    'k': ('coat', 0), 'l': ('coat', 1), 'm': ('coat', 2), 'n': ('coat', 3), 'N': ('coat', 4),
    'O': ('bronze', 0), 'o': ('bronze', 1), 'p': ('bronze', 2), 'P': ('bronze', 3), 'B': ('bronze', 4),
    'w': ('blood', 0), 'q': ('blood', 1), 'r': ('blood', 2), 'R': ('blood', 3), 'Q': ('blood', 4),
    's': ('leather', 0), 't': ('leather', 1), 'u': ('leather', 2), 'v': ('leather', 3), 'V': ('leather', 4),
    'x': ('blade', 1), 'y': ('blade', 2), 'z': ('blade', 3), 'Z': ('blade', 4),
    'E': ('blood', 3), 'F': ('rim', 2),
}


def almos():
    """Álmos de genere Kán: vampir hekim, otuzunda görünen yedi yüz yaşında Macar soylusu. Arkaya taranmış
    uzun siyah saç, solgun ve yakışıklı ince yüz, ince bıyık, gözde tek kırmızı pırıltı; yüksek yakalı,
    dize kadar koyu yeşil mente, göğüste altın kordonlar, kızıl kuşak; elinde kan kırmızısı hekim şişesi,
    öbür elinde deri çanta, belinde kılıç, uzun binici çizmeleri."""
    s = Sprite(52, 64)
    CT, BK = 'coat', 'black'
    # palto eteğinin arkada uçuşan ucu
    for y, (x0, x1) in {33: (13, 17), 34: (12, 17), 35: (12, 17), 36: (11, 17), 37: (11, 17), 38: (11, 16), 39: (11, 16),
                        40: (12, 16), 41: (12, 15), 42: (13, 15)}.items():
        s.span(y, x0, x1, CT, 1, back=0, front=2)
    s.line(14, 34, 13, 41, CT, 0); s.px([(11, 37), (11, 38)], 'rim', 2)
    # kılıç kını (belden arkaya doğru iner)
    s.line(20, 29, 12, 46, 'leather', 1); s.line(21, 29, 13, 46, 'leather', 2); s.px([(12, 47)], 'bronze', 2)
    # arka çizme
    for y in range(44, 57):
        s.span(y, 16, 19, BK, 1, back=0, front=2)
    for y, (x0, x1) in {57: (15, 20), 58: (14, 20), 59: (13, 20), 60: (13, 21), 61: (13, 21)}.items():
        s.span(y, x0, x1, BK, 1, back=0, front=2)
    s.span(61, 13, 21, BK, 0); s.span(44, 16, 19, BK, 2, back=1)
    # ön çizme: diz kapağının üstüne çıkan parlak binici çizmesi
    for y in range(44, 57):
        s.span(y, 24, 27, BK, 2, back=1, front=3)
    for y, (x0, x1) in {57: (23, 28), 58: (23, 29), 59: (23, 30), 60: (23, 31), 61: (23, 31)}.items():
        s.span(y, x0, x1, BK, 2, back=1, front=3)
    s.span(44, 24, 27, BK, 3, back=2); s.span(61, 23, 31, BK, 0)
    s.px([(27, 47), (27, 48), (27, 49), (27, 53), (30, 59)], BK, 4)
    # palto: gövde
    coat = {13: (20, 25), 14: (19, 26), 15: (19, 27), 16: (19, 27), 17: (19, 27), 18: (19, 28), 19: (19, 28), 20: (19, 28),
            21: (19, 28), 22: (19, 28), 23: (19, 27), 24: (19, 27), 25: (19, 27)}
    for y, (x0, x1) in coat.items():
        s.span(y, x0, x1, CT, 2, back=1, back_w=3, front=3, front_w=2)
    s.px([(19, 15), (19, 16), (19, 17), (19, 18)], 'rim', 2)
    # dize kadar etek: hafif açılır, ön kenarı altın şeritli, iki kıvrım
    skirt = {28: (18, 28), 29: (18, 28), 30: (17, 28), 31: (17, 29), 32: (17, 29), 33: (16, 29), 34: (16, 29), 35: (16, 29),
             36: (16, 30), 37: (15, 30), 38: (15, 30), 39: (15, 30), 40: (15, 30), 41: (15, 31), 42: (15, 31), 43: (16, 31)}
    for y, (x0, x1) in skirt.items():
        s.span(y, x0, x1, CT, 2, back=1, back_w=2, front=3)
        s.px([(x1 - 1, y)], 'bronze', 2 if y % 4 else 3)
    s.span(43, 16, 31, 'bronze', 2, back=1, front=3)
    for (xa, ya), (xb, yb) in (((19, 30), (17, 42)), ((23, 30), (22, 42))):
        s.line(xa, ya, xb, yb, CT, 0); s.line(xa + 1, ya, xb + 1, yb, CT, 3)
    # göğüste altın kordonlar (Macar tarzı)
    for y in (15, 17, 19, 21, 23):
        s.line(22, y, 26, y, 'bronze', 2); s.px([(27, y)], 'bronze', 3); s.px([(21, y)], 'bronze', 1)
        s.px([(24, y)], 'bronze', 4 if y in (15, 19) else 3)
    # kızıl kuşak; kılıç kabzası belde önde
    for y, (x0, x1) in {26: (19, 28), 27: (19, 28)}.items():
        s.span(y, x0, x1, 'blood', 2, back=1, front=3)
    s.px([(28, 28), (28, 29)], 'blood', 2)
    s.ascii({25: [(28, "PB")], 26: [(29, "p")], 27: [(29, "o")]}, ALM)
    # arka kol: deri hekim çantası taşır
    for y in range(15, 27):
        s.span(y, 17, 19, CT, 1, back=0)
    s.ascii({27: [(17, "66")], 28: [(16, "t6")]}, ALM)
    for y, (x0, x1) in {30: (12, 18), 31: (11, 18), 32: (11, 18), 33: (11, 18), 34: (11, 18), 35: (12, 17)}.items():
        s.span(y, x0, x1, 'leather', 2, back=1, front=3)
    s.span(30, 12, 18, 'leather', 3, back=2); s.span(35, 12, 17, 'leather', 1, back=0)
    s.ascii({29: [(13, "s   s")], 32: [(14, "PB")]}, ALM)
    # yüksek yaka (altın kenarlı) — yüzün arkasında
    s.ascii({11: [(20, "lmm")], 12: [(19, "lmmnp")], 13: [(19, "lmmmnp")], 14: [(20, "ooppP")]}, ALM)
    # baş: arkaya taranmış uzun siyah saç omuzlara iner; solgun yakışıklı profil, ince bıyık
    s.ascii({
        1: [(21, "bccd")],
        2: [(20, "bccdde")],
        3: [(19, "abbccd78")],
        4: [(18, "aabbcc678")],
        5: [(18, "aabbb67788")],
        6: [(18, "abbbb66E78")],
        7: [(18, "abbbb67778")],
        8: [(18, "abbb5677789")],
        9: [(18, "abbb56767b")],
        10: [(18, "abbb66776")],
        11: [(18, "aabb66677")],
        12: [(18, "aab"), (23, "66")],
        13: [(18, "aab")],
        14: [(18, "ab")],
        15: [(19, "a")],
    }, ALM)
    s.px([(21, 3), (22, 4)], BK, 3)                                         # saçta ışık
    s.px([(26, 9), (27, 9)], BK, 1)                                         # ince bıyık
    # ön kol: öne kalkar, solgun el kan kırmızısı şişeyi tutar
    for y, (x0, x1) in {15: (24, 27), 16: (24, 27), 17: (25, 28), 18: (25, 28), 19: (26, 29), 20: (27, 30), 21: (28, 31)}.items():
        s.span(y, x0, x1, CT, 2, back=1, front=3)
    s.ascii({21: [(31, "p")], 20: [(31, "pp")]}, ALM)                       # altın kol ağzı
    s.ascii({19: [(31, "678")], 20: [(33, "8")], 18: [(32, "67")]}, ALM)    # el
    s.ascii({13: [(33, "x")], 14: [(32, "zZ")], 15: [(32, "rQ")], 16: [(32, "rR")], 17: [(32, "qr")]}, ALM)   # şişe
    return s.outline()


KUT = {
    '5': ('skin', 0), '6': ('skin', 1), '7': ('skin', 2), '8': ('skin', 3), '9': ('skin', 4),
    'a': ('brown', 0), 'b': ('brown', 1), 'c': ('brown', 2), 'd': ('brown', 3), 'e': ('brown', 4),
    'g': ('teal', 0), 'h': ('teal', 1), 'i': ('teal', 2), 'j': ('teal', 3), 'J': ('teal', 4),
    '1': ('ivory', 1), '2': ('ivory', 2), '3': ('ivory', 3), '4': ('ivory', 4),
    'q': ('red', 1), 'r': ('red', 2), 'R': ('red', 3),
    'o': ('bronze', 1), 'p': ('bronze', 2), 'P': ('bronze', 3), 'B': ('bronze', 4),
    'k': ('wood', 0), 'l': ('wood', 1), 'L': ('wood', 2), 'm': ('wood', 3), 'M': ('wood', 4),
    'G': ('navy', 0), 'n': ('navy', 1), 'N': ('navy', 2),
    'x': ('mustard', 0), 'y': ('mustard', 1), 'Y': ('mustard', 2), 'z': ('mustard', 3), 'Z': ('mustard', 4),
    'u': ('amber', 2), 'U': ('amber', 3), 'W': ('amber', 4),
    'E': ('eye', 0),
}


def kutluca():
    """Kutluca: 12-13 yaşında, kadim Türk soylu çocuk. Kürk kenarlı başlık, camgöbeği entari, kızıl kuşakta
    babasının hançeri; elinde babaannesinin bastonu, öbür elinde annesinin tesbihi. Gözde belli belirsiz
    kehribar pırıltı (uyanan kurt kanının ipucu; güç icat edilmez)."""
    s = Sprite(40, 64)
    TL = 'teal'
    # arka kol ve tesbih (gövdenin arkasında)
    for y in range(29, 36):
        s.span(y, 16, 18, TL, 1, back=0)
    s.ascii({36: [(16, "667")], 37: [(16, "66")]}, KUT)
    s.ascii({38: [(16, "U")], 39: [(16, "u")], 40: [(15, "U")], 41: [(15, "u")], 42: [(15, "U")], 43: [(16, "u")],
             44: [(17, "U")], 45: [(18, "q")], 46: [(18, "r")], 47: [(18, "q")]}, KUT)
    s.px([(16, 38), (15, 42)], 'amber', 4)
    # arka bacak (şalvar) ve çedik — bacaklar arasında boşluk
    for y in range(47, 56):
        s.span(y, 16, 18, 'navy', 1, back=0, front=2)
    for y, (x0, x1) in {55: (16, 18), 56: (15, 19), 57: (15, 19), 58: (14, 19), 59: (14, 20), 60: (14, 20), 61: (14, 20)}.items():
        s.span(y, x0, x1, 'mustard', 1, back=0, front=2)
    s.span(61, 14, 20, 'mustard', 0); s.px([(20, 58)], 'mustard', 2)
    # ön bacak ve kıvrık burunlu çedik
    for y in range(47, 56):
        s.span(y, 22, 24, 'navy', 2, back=1, front=3)
    for y, (x0, x1) in {55: (22, 24), 56: (22, 25), 57: (22, 26), 58: (22, 27), 59: (22, 28), 60: (22, 28), 61: (22, 28)}.items():
        s.span(y, x0, x1, 'mustard', 2, back=1, front=3)
    s.span(61, 22, 28, 'mustard', 0); s.px([(28, 58), (27, 57)], 'mustard', 3); s.px([(25, 56)], 'mustard', 4)
    # entari: dar omuzlar, dize kadar açılan etek, altın kenar
    body = {27: (19, 24), 28: (18, 25), 29: (18, 25), 30: (17, 26), 31: (17, 26), 32: (17, 26), 33: (17, 26), 34: (17, 26)}
    for y, (x0, x1) in body.items():
        s.span(y, x0, x1, TL, 2, back=1, back_w=2, front=3)
    s.ascii({27: [(22, "23")], 28: [(23, "23")], 29: [(23, "2")], 30: [(24, "3")]}, KUT)     # yakadaki gömlek
    robe = {37: (16, 26), 38: (16, 26), 39: (16, 27), 40: (15, 27), 41: (15, 27), 42: (15, 27), 43: (15, 28), 44: (15, 28),
            45: (15, 28), 46: (15, 28), 47: (16, 27)}
    for y, (x0, x1) in robe.items():
        s.span(y, x0, x1, TL, 2, back=1, back_w=2, front=3)
        s.px([(x1, y)], 'bronze', 2 if y % 3 else 3)
    s.span(47, 16, 27, 'bronze', 2, back=1, front=3)
    s.line(19, 38, 18, 46, TL, 0); s.line(23, 38, 23, 46, TL, 1); s.px([(20, 39), (24, 40)], TL, 3)
    # kızıl kuşak ve babasının hançeri (kabza kuşakta, kın önde)
    for y in (35, 36):
        s.span(y, 17, 26, 'red', 2 if y == 35 else 1, back=1 if y == 35 else 0, front=3 if y == 35 else 2)
    s.ascii({32: [(25, "P")], 33: [(25, "l")], 34: [(25, "L")], 35: [(24, "BP")]}, KUT)
    s.ascii({37: [(26, "lm")], 38: [(27, "lm")], 39: [(28, "lm")], 40: [(29, "P")]}, KUT)
    # baş: kürk kenarlı başlık, kısa koyu saç, yuvarlak çocuk yüzü, iri göz
    s.ascii({
        15: [(19, "hiij")],
        16: [(18, "hiijjJ")],
        17: [(17, "ghiiijj")],
        18: [(16, "abccddcd")],
        19: [(16, "abcddcdde")],
        20: [(16, "abb67778")],
        21: [(16, "ab67E788")],
        22: [(16, "ab6676788")],
        23: [(17, "b6777789")],
        24: [(17, "b677768")],
        25: [(18, "667778")],
        26: [(19, "6677")],
    }, KUT)
    s.px([(21, 22)], 'amber', 3)                                            # gözde kehribar pırıltı
    s.px([(19, 22), (19, 23)], 'skin', 1); s.px([(20, 23)], 'skin', 2)       # kulak
    s.px([(23, 24)], 'skin', 3); s.px([(24, 25)], 'skin', 1)                 # yanak ve ağız
    # ön kol ve babaannesinin bastonu (çoban kancası gibi kıvrık sap); kolun gövdeye düşen gölgesi
    for y in range(29, 61):
        s.px([(29, y)], 'wood', 2 if y % 5 else 3); s.px([(30, y)], 'wood', 1)
    s.ascii({25: [(30, "lmm")], 26: [(29, "lM  m")], 27: [(29, "L   l")], 28: [(29, "L")], 61: [(29, "kk")]}, KUT)
    for x, y in ((21, 28), (21, 29), (22, 30), (22, 31), (23, 32), (24, 33)):
        s.px([(x, y)], TL, 0)
    for y, (x0, x1) in {28: (22, 24), 29: (22, 25), 30: (23, 25), 31: (23, 26), 32: (24, 27), 33: (25, 27)}.items():
        s.span(y, x0, x1, TL, 2, back=1, front=3)
    s.px([(26, 33), (27, 33)], 'bronze', 3); s.px([(25, 33)], 'bronze', 2)
    s.ascii({33: [(28, "7")], 34: [(27, "6789")], 35: [(27, "6778")], 36: [(28, "6")]}, KUT)       # el bastonu kavrar
    return s.outline()


KIS = {
    '5': ('green', 0), '6': ('green', 1), '7': ('green', 2), '8': ('green', 3), '9': ('green', 4),
    'x': ('mustard', 0), 'y': ('mustard', 1), 'Y': ('mustard', 2), 'z': ('mustard', 3), 'Z': ('mustard', 4),
    'a': ('brown', 0), 'b': ('brown', 1), 'c': ('brown', 2), 'd': ('brown', 3), 'e': ('brown', 4),
    '1': ('ivory', 1), '2': ('ivory', 2), '3': ('ivory', 3), '4': ('ivory', 4),
    'q': ('red', 1), 'r': ('red', 2), 'R': ('red', 3),
    'k': ('wood', 0), 'l': ('wood', 1), 'L': ('wood', 2), 'm': ('wood', 3),
    'o': ('bronze', 1), 'p': ('bronze', 2), 'P': ('bronze', 3), 'B': ('bronze', 4),
    'v': ('blade', 1), 'w': ('blade', 2), 'W': ('blade', 3), 'V': ('blade', 4),
    'E': ('eye', 0),
}


def kismet():
    """Kısmet: sakar ama çok şanslı goblin müfettiş. Koca baş, kanca burun, uzun sivri kulaklar, iri sarı göz,
    sırıtış; sarkık keçe külah, yamalı yelek, göğüste dart kayışı; elinde geniş ağızlı kama, öbür elinde
    atılmaya hazır dart; çıplak, iri yeşil ayaklar. Çömelmiş, muzip duruş."""
    s = Sprite(46, 64)
    GR = 'green'
    # arka kol: havada dart tutar
    s.ascii({33: [(13, "67")], 34: [(12, "678")], 35: [(13, "66")], 36: [(14, "6")], 37: [(15, "66")], 38: [(16, "6")]}, KIS)
    s.ascii({29: [(16, "R")], 30: [(15, "rL")], 31: [(14, "Lm")], 32: [(13, "L")]}, KIS)
    # arka bacak (bol çizgili şalvar) ve iri çıplak ayak
    for y, (x0, x1) in {47: (16, 21), 48: (16, 21), 49: (15, 21), 50: (15, 20), 51: (15, 20), 52: (15, 20), 53: (16, 19)}.items():
        s.span(y, x0, x1, 'mustard', 1, back=0, front=2)
    s.px([(17, 48), (18, 50), (17, 52)], 'brown', 1)
    for y, (x0, x1) in {54: (16, 19), 55: (16, 19), 56: (16, 20), 57: (15, 20), 58: (14, 21), 59: (13, 22), 60: (13, 22), 61: (13, 22)}.items():
        s.span(y, x0, x1, GR, 1, back=0, front=2)
    s.span(61, 13, 22, GR, 0); s.px([(20, 60), (22, 60)], GR, 0)
    # ön bacak ve iri ayak (uzun parmaklar)
    for y, (x0, x1) in {47: (22, 28), 48: (22, 28), 49: (23, 29), 50: (23, 29), 51: (24, 29), 52: (24, 28), 53: (24, 27)}.items():
        s.span(y, x0, x1, 'mustard', 2, back=1, front=3)
    s.px([(25, 48), (26, 50), (27, 52), (24, 50)], 'brown', 1)
    for y, (x0, x1) in {54: (24, 27), 55: (24, 27), 56: (24, 28), 57: (23, 29), 58: (23, 31), 59: (22, 33), 60: (22, 34), 61: (22, 34)}.items():
        s.span(y, x0, x1, GR, 2, back=1, front=3)
    s.span(61, 22, 34, GR, 0); s.px([(29, 60), (31, 60), (33, 60)], GR, 0); s.px([(30, 59), (32, 59)], GR, 4)
    # gövde: kambur, kirli gömlek üstünde yamalı yelek
    for y, (x0, x1) in {38: (19, 25), 39: (18, 26), 40: (17, 27), 41: (17, 27), 42: (17, 27), 43: (17, 27), 44: (17, 28),
                        45: (17, 28), 46: (17, 28)}.items():
        s.span(y, x0, x1, 'ivory', 2, back=1, front=3)
    for y, (x0, x1) in {39: (18, 23), 40: (17, 24), 41: (17, 24), 42: (17, 24), 43: (17, 25), 44: (17, 25), 45: (17, 25), 46: (17, 25)}.items():
        s.span(y, x0, x1, 'brown', 2, back=1, back_w=2, front=3)
    s.ascii({42: [(19, "yY")], 43: [(19, "Yz")], 45: [(22, "rr")]}, KIS)          # yamalar
    s.px([(21, 41), (21, 44)], 'brown', 4)
    # dart kayışı ve üç dart
    s.line(18, 39, 27, 46, 'brown', 1); s.line(19, 39, 28, 46, 'brown', 3)
    s.ascii({34: [(20, "R")], 35: [(20, "q"), (23, "R")], 36: [(20, "L"), (23, "q"), (26, "R")], 37: [(20, "l"), (23, "L"), (26, "q")],
             38: [(20, "l"), (23, "l"), (26, "L")], 39: [(23, "l"), (26, "l")]}, KIS)
    # kuşak
    s.span(47, 17, 28, 'red', 2, back=1, front=3)
    # baş: yuvarlak kubbe kafa, iri sarı göz, kalın kaş, ince kanca burun, kafanın içinde sırıtış
    s.ascii({
        24: [(18, "67777")],
        25: [(17, "6777778")],
        26: [(16, "567777788")],
        27: [(16, "5677777788")],
        28: [(15, "5667777aab88")],
        29: [(15, "5667777zZz88")],
        30: [(15, "5667777zEZ7888")],
        31: [(15, "56667777y7778888")],
        32: [(16, "5667777777776889")],
        33: [(16, "566777555577"), (30, "886")],
        34: [(17, "56674343466"), (31, "6")],
        35: [(17, "5566755556")],
        36: [(18, "5566666")],
        37: [(19, "55555")],
    }, KIS)
    s.px([(19, 31), (20, 32)], 'green', 3)                                   # yanak
    # yatay uzanan sivri kulak
    s.ascii({28: [(10, "56")], 29: [(8, "5667777")], 30: [(9, "56777")], 31: [(12, "5667")], 32: [(14, "56")]}, KIS)
    s.px([(10, 29), (11, 29), (11, 30)], 'green', 1)                         # kulak içi
    # sarkık keçe külah ve kızıl püskül
    s.ascii({
        21: [(15, "yyY")],
        22: [(13, "yyY"), (18, "yYYz")],
        23: [(11, "yy"), (16, "yYYYzzZ")],
        24: [(15, "xyYYYzzzZ")],
        25: [(15, "xyyYYYzzz")],
        26: [(15, "xxyyyyyyyx")],
    }, KIS)
    s.ascii({23: [(9, "rR")], 24: [(9, "q")]}, KIS)
    # ön kol ve geniş ağızlı kama
    s.ascii({40: [(24, "667")], 41: [(25, "6778")], 42: [(27, "678")], 43: [(29, "789")], 44: [(30, "78")]}, KIS)
    s.ascii({41: [(31, "p")], 42: [(31, "Pp")], 43: [(32, "P")]}, KIS)
    s.ascii({36: [(39, "V")], 37: [(37, "wWV")], 38: [(35, "wWWV")], 39: [(34, "vwWW")], 40: [(33, "vwW")], 41: [(33, "v")]}, KIS)
    return s.outline()


CHARACTERS = {'karamani': karamani, 'ludwig': ludwig, 'almos': almos, 'kutluca': kutluca, 'kismet': kismet}


def main():
    names = sys.argv[1:] or list(CHARACTERS)
    sprites = [CHARACTERS[n]() for n in names]
    path = preview(sprites, os.path.join(REVIEW, '-'.join(names) + '.png'), scale=8)
    print(path)


if __name__ == '__main__':
    main()
