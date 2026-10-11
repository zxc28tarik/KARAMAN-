"""Beş karakterin kahramanca oranlı, havalı duruşlu elle çizilmiş örnekleri (ikinci tur).

Kahramanca oran: ~8 baş boy, geniş omuz, dar bel (V gövde), uzun bacak; kontrapposto duruş, göğüs
dışarıda, rüzgârda pelerin / saç. Işık sağ üst önden, arka kenarda soğuk ışık, seçici dış çizgi.
Kutluca çocuktur: havalı ve kararlı çizilir, asla cinsel çekicilik üzerinden çizilmez.
Çalıştır: python -I Tools/hand_heroes.py [ad ...]
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from hand_sprites import Sprite, preview, REVIEW  # noqa: E402
from hand_characters import HERO, LUD, ALM, KUT, KIS, curved_blade  # noqa: E402

W_, H_ = 60, 66        # tuval; zemin (taban) satırı 63


def wave_cape(s, mat, rows, folds, hem_cut=(), rim=()):
    """Rüzgârda dalgalanan pelerin: çoğu gölgede, kıvrım sırtları üstte ışık alır, astar etekte görünür."""
    for y, (x0, x1) in rows.items():
        s.span(y, x0, x1, mat, 1, back=0, front=2, back_w=1, front_w=2)
    for (x0, y0), (x1, y1) in folds:
        steps = max(1, y1 - y0)
        for i in range(steps + 1):
            y = y0 + i; x = round(x0 + (x1 - x0) * i / steps)
            if s.mat[y, x] < 0:
                continue
            s.px([(x, y)], mat, 3 if i < steps * .35 else 2)
            if s.mat[y, x - 1] >= 0:
                s.px([(x - 1, y)], mat, 0 if i > 2 else 1)
    s.erase(list(hem_cut))
    for x, y in rim:
        s.px([(x, y)], 'rim', 2)


def karamani():
    """Karamani: sert, deneyimli akıncı çavuşu. Geniş göğüs, bronz lamel zırh, rüzgârda kızıl pelerin ve sarık
    ucu; kılıcı önde aşağı indirilmiş, arka eli kalçada; ağırlık arka bacakta, göğüs dışarıda."""
    s = Sprite(W_, H_)
    R, BR, ML, LE, ST = 'h_red', 'h_bronze', 'h_mail', 'h_leather', 'h_steel'
    # rüzgârda pelerin
    cape = {12: (14, 20), 13: (12, 20), 14: (10, 19), 15: (8, 18), 16: (7, 18), 17: (6, 17), 18: (5, 17), 19: (4, 17),
            20: (4, 17), 21: (3, 16), 22: (3, 16), 23: (3, 16), 24: (3, 16), 25: (3, 16), 26: (4, 16), 27: (4, 16),
            28: (4, 16), 29: (5, 16), 30: (5, 16), 31: (5, 16), 32: (6, 16), 33: (6, 16), 34: (7, 16), 35: (7, 15),
            36: (8, 15), 37: (8, 15), 38: (9, 15), 39: (9, 15), 40: (10, 15), 41: (11, 15), 42: (12, 15)}
    wave_cape(s, R, cape, [((17, 14), (13, 41)), ((14, 15), (9, 38)), ((11, 16), (6, 33)), ((8, 18), (4, 27))],
              hem_cut=[(9, 39), (10, 41), (11, 42), (5, 31), (4, 28), (6, 33)], rim=[(4, 19), (4, 20), (3, 21), (3, 22), (5, 17)])
    s.px([(4, 26), (4, 27), (5, 30)], R, 0)
    # arka bacak (ağırlık taşır, düz): şalvar, dizlik, incik, çizme
    for y, (x0, x1) in {33: (18, 22), 34: (18, 22), 35: (17, 22), 36: (17, 21), 37: (17, 21), 38: (17, 21), 39: (17, 21),
                        40: (16, 20), 41: (16, 20), 42: (16, 20), 43: (16, 20)}.items():
        s.span(y, x0, x1, R, 1, back=0, front=2)
    s.ascii({44: [(16, "pqqp")], 45: [(15, "pqQqp")], 46: [(16, "oppo")]}, HERO)
    for y in range(47, 59):
        s.span(y, 16, 19, ST, 1, back=0, front=2)
    for y, (x0, x1) in {59: (15, 19), 60: (15, 20), 61: (14, 21), 62: (14, 21), 63: (14, 21)}.items():
        s.span(y, x0, x1, LE, 1, back=0, front=2)
    s.span(59, 15, 19, LE, 2); s.span(63, 14, 21, LE, 0)
    # arka kol: dirsek dışarıda, el kalçada (gövdeyle arasında üçgen boşluk)
    for y, (x0, x1) in {14: (15, 18), 15: (14, 18), 16: (13, 17), 17: (12, 16), 18: (12, 15), 19: (12, 15), 20: (12, 15)}.items():
        s.span(y, x0, x1, ML, 1, back=0)
    for y, (x0, x1) in {21: (12, 15), 22: (13, 16), 23: (14, 17), 24: (15, 18), 25: (16, 19)}.items():
        s.span(y, x0, x1, LE, 1, back=0, front=2)
    s.ascii({25: [(18, "LL")], 26: [(17, "LrL")], 27: [(18, "LL")]}, HERO)
    # göğüs zırhı: geniş, öne çıkan göğüs, bele doğru V
    torso = {13: (17, 26), 14: (17, 26), 15: (16, 27), 16: (16, 28), 17: (16, 29), 18: (16, 30), 19: (17, 30), 20: (17, 30),
             21: (18, 29), 22: (18, 29), 23: (19, 28), 24: (19, 28), 25: (20, 28)}
    for y, (x0, x1) in torso.items():
        k = (y - 13) % 3
        if k == 2:
            s.span(y, x0, x1, BR, 1, back=0, back_w=3)
        elif k == 0:
            s.span(y, x0, x1, BR, 2, back=1, back_w=5, front=3, front_w=3)
        else:
            s.span(y, x0, x1, BR, 1, back=0, back_w=2, front=2, front_w=4)
        for x in ((x0 + 5, x0 + 9) if k == 1 else (x0 + 6, x0 + 10)):
            if x <= x1 and k == 0 and s.tone[y, x] >= 2:
                s.px([(x, y)], BR, s.tone[y, x] - 1)
    s.px([(29, 16), (30, 19), (29, 22)], BR, 4)
    for y in range(15, 26):
        s.px([(26 if y < 21 else 25, y)], BR, 0)                           # göğüs ortasındaki ek yeri
    # kuşak ve sarkan ucu
    for y, (x0, x1) in {26: (20, 28), 27: (20, 29), 28: (20, 29)}.items():
        s.span(y, x0, x1, R, 2, back=1, back_w=2, front=3)
    s.px([(29, 27), (30, 28), (30, 29), (30, 30)], R, 3); s.px([(31, 30), (29, 30)], R, 1)
    # ön bacak (öne atılmış, hafif bükük diz): ince uyluk, açık incik, küçük çizme
    for y, (x0, x1) in {32: (24, 29), 33: (24, 29), 34: (25, 29), 35: (25, 29), 36: (25, 30), 37: (25, 30), 38: (26, 30),
                        39: (26, 30), 40: (26, 30), 41: (26, 30), 42: (27, 30), 43: (27, 30)}.items():
        s.span(y, x0, x1, R, 2, back=1, back_w=2, front=3)
    s.px([(27, 35), (27, 36), (28, 38), (28, 39)], R, 1); s.px([(29, 35), (29, 36), (30, 38)], R, 4)
    s.ascii({44: [(27, "pQQp")], 45: [(26, "qQBQq")], 46: [(27, "oppo")]}, HERO)
    for y in range(47, 59):
        x0, x1 = (27, 30) if y < 53 else (28, 30)
        s.span(y, x0, x1, ST, 3, back=1, front=3)
    s.px([(30, 48), (30, 49), (30, 50), (29, 56)], ST, 4); s.span(52, 27, 30, ST, 1)
    for y, (x0, x1) in {59: (27, 31), 60: (27, 32), 61: (26, 33), 62: (26, 34), 63: (26, 34)}.items():
        s.span(y, x0, x1, LE, 2, back=1, front=3)
    s.span(59, 27, 31, LE, 3, back=2); s.span(63, 26, 34, LE, 0); s.px([(34, 61), (33, 60)], LE, 4)
    # iki ayrı etek plakası, aralarında zincir
    for y, (x0, x1) in {29: (19, 28), 30: (19, 28), 31: (18, 23)}.items():
        s.span(y, x0, x1, ML, 1, back=0)
    for plate in ({29: (17, 22), 30: (17, 22), 31: (17, 22), 32: (17, 22), 33: (17, 22), 34: (18, 21)},
                  {29: (24, 29), 30: (24, 29), 31: (24, 30), 32: (25, 30)}):
        ys = sorted(plate)
        for y in ys:
            x0, x1 = plate[y]
            k = (y - ys[0]) % 3
            if k == 2:
                s.span(y, x0, x1, BR, 1, back=0); s.px([((x0 + x1) // 2, y)], R, 2)
            else:
                s.span(y, x0, x1, BR, 2, back=1, front=3 if k == 0 else 2, front_w=2)
        s.span(ys[-1], plate[ys[-1]][0], plate[ys[-1]][1], BR, 1, back=0)
    # boyun (kalın) ve zincir boyunluk
    s.ascii({12: [(19, "jkj")], 13: [(19, "jkjk")], 14: [(19, "kjkjk")]}, HERO)
    # ön omuzluk: iri, katmanlı
    s.ascii({11: [(27, "QQBQ")], 12: [(26, "qQQQQq")], 13: [(25, "pqqQQQQq")], 14: [(25, "oppppppp")],
             15: [(26, "qqQQQQq")], 16: [(26, "pqqqQQq")], 17: [(27, "ooppp")]}, HERO)
    # baş (Türk): uzun kızıl külahın etrafına sarılmış tombul beyaz Osmanlı sarığı, pirinç sorguç ve geriye
    # kıvrılan beyaz telek; açık buğday ten, kalın kaş, elmacık kemiği, gür pala bıyık, kısa kesilmiş sakal
    TK = dict(HERO, **{'5': ('oguz', 0), '6': ('oguz', 1), '7': ('oguz', 2), '8': ('oguz', 3), '9': ('oguz', 4),
                       'h': ('kumral', 0), 'm': ('kumral', 1), 'n': ('kumral', 2), 'M': ('kumral', 3), 'N': ('kumral', 4)})
    s.ellipse(20.6, 2.7, 2.7, 2.3, R, 2)                                       # külah
    s.px([(19, 1), (18, 2), (18, 3)], R, 1); s.px([(21, 1), (22, 2)], R, 3); s.px([(21, 2)], R, 4)
    s.ellipse(21.6, 5.4, 5.7, 2.6, 'h_ivory', 2)                                # sarık
    for x0 in (17, 20, 23):                                                     # çapraz sargı katları
        s.line(x0, 7, x0 + 3, 3, 'h_ivory', 1, clip='h_ivory'); s.line(x0 + 1, 7, x0 + 4, 3, 'h_ivory', 3, clip='h_ivory')
    s.px([(24, 4), (25, 5), (26, 5), (25, 4)], 'h_ivory', 4)
    s.span(7, 17, 25, 'h_ivory', 1, back=0); s.px([(16, 5), (16, 6)], 'h_ivory', 0)
    s.ascii({3: [(25, "QB")], 4: [(25, "qQ")]}, TK)                            # sorguç
    s.ascii({0: [(22, "23344")], 1: [(26, "43")], 2: [(26, "3")]}, TK)          # telek
    s.ascii({
        7: [(26, "8")],
        8: [(19, "n6677mm8")],
        9: [(19, "n67767mE8")],
        10: [(19, "n677788789")],
        11: [(20, "6777hmmmh")],
        12: [(20, "6676m77nm"), (29, "m")],
        13: [(21, "66mnnnnm"), (29, "h")],
        14: [(23, "nMnn")],
    }, TK)
    s.px([(19, 7), (20, 7)], 'kumral', 2); s.px([(21, 7), (22, 7), (23, 7), (24, 7), (25, 7)], 'oguz', 1)   # sarık altı gölge
    s.px([(21, 9), (21, 10)], 'oguz', 1); s.px([(22, 9)], 'oguz', 3)            # kulak
    s.px([(26, 11), (27, 11)], 'kumral', 3); s.px([(25, 13)], 'kumral', 3)      # bıyık ve sakalda ışık
    # ön kol: zincir yen, güçlü önkol (deri kolluk), kılıcı önde aşağı tutar
    for y, (x0, x1) in {18: (28, 31), 19: (28, 31), 20: (29, 32), 21: (29, 32)}.items():
        s.span(y, x0, x1, ML, 2 if y % 2 == 0 else 1, back=1 if y % 2 == 0 else 0, front=3 if y % 2 == 0 else 2)
    for y, (x0, x1) in {22: (30, 33), 23: (31, 34), 24: (31, 34), 25: (32, 35), 26: (33, 35)}.items():
        s.span(y, x0, x1, LE, 2, back=1, front=3)
    s.px([(33, 23), (34, 24)], LE, 4); s.span(24, 31, 34, 'h_bronze', 2, back=1, front=3)   # kolluk bandı
    curved_blade(s, (37.5, 29.0), -35, 21, 2.5, 'h_blade')
    s.ascii({26: [(36, "Q")], 27: [(36, "q")], 28: [(37, "Q")], 29: [(37, "q")], 30: [(37, "p")]}, HERO)   # balçak
    s.ascii({26: [(33, "rRR")], 27: [(33, "rRRT")], 28: [(34, "Lrr")], 29: [(34, "LL")]}, HERO)          # yumruk
    return s.outline()


def compose(base, top, dx, dy):
    """top sprite'ının dolu piksellerini base'in üstüne (dx, dy) kaydırarak yerleştirir."""
    ys, xs = (top.mat >= 0).nonzero()
    for y, x in zip(ys, xs):
        if 0 <= y + dy < base.h and 0 <= x + dx < base.w:
            base.mat[y + dy, x + dx] = top.mat[y, x]; base.tone[y + dy, x + dx] = top.tone[y, x]
    return base


def almos():
    """Álmos: çekici, karanlık bir soylu. Açık mente rüzgârda arkaya dalgalanır, gömleğin yakası açık, uzun
    siyah saç geriye savrulur; keskin çene, kırmızı göz pırıltısı; ince kılıcı önde aşağıda, kuşakta kızıl şişe."""
    s = Sprite(W_, H_)
    CT, BK = 'coat', 'black'
    # kan kırmızısı astarlı uzun siyah pelerin: rüzgârda açılır, dönen kenarında astar görünür
    cloak = {9: (16, 20), 10: (14, 20), 11: (12, 20), 12: (11, 20), 13: (10, 20), 14: (9, 20), 15: (8, 20), 16: (7, 20),
             17: (7, 19), 18: (6, 19), 19: (6, 19), 20: (5, 19), 21: (5, 19), 22: (5, 19), 23: (5, 19), 24: (5, 19),
             25: (5, 19), 26: (5, 19), 27: (5, 19), 28: (5, 19), 29: (5, 19), 30: (5, 19), 31: (5, 19), 32: (5, 19),
             33: (5, 19), 34: (5, 19), 35: (5, 19), 36: (5, 19), 37: (6, 19), 38: (6, 19), 39: (6, 19), 40: (6, 19),
             41: (7, 19), 42: (7, 19), 43: (7, 19), 44: (8, 19), 45: (8, 19), 46: (9, 19), 47: (10, 19), 48: (11, 19), 49: (13, 19)}
    wave_cape(s, BK, cloak, [((18, 12), (15, 47)), ((15, 13), (11, 45)), ((12, 15), (8, 40)), ((9, 18), (6, 33))],
              rim=[(7, 16), (6, 18), (6, 19), (5, 20)])
    for y, (x0, x1) in cloak.items():
        if y >= 22:
            s.px([(x0, y)], 'blood', 2 if y % 4 else 3); s.px([(x0 + 1, y)], 'blood', 1)
    for y in (47, 48, 49):
        x0, x1 = cloak[y]
        s.span(y, x0, x1, 'blood', 1, back=0, front=2)
    s.px([(9, 46), (8, 45)], 'blood', 2)
    # arka bacak ve çizme
    for y in range(28, 46):
        s.span(y, 18, 21 if y < 40 else 20, BK, 1, back=0, front=2)
    for y in range(46, 59):
        s.span(y, 17, 20, BK, 1, back=0, front=2)
    for y, (x0, x1) in {59: (16, 20), 60: (15, 21), 61: (14, 21), 62: (14, 21), 63: (14, 21)}.items():
        s.span(y, x0, x1, BK, 1, back=0, front=2)
    s.span(63, 14, 21, BK, 0); s.span(46, 17, 20, BK, 2, back=1)
    # ön bacak (öne atılmış), parlak binici çizmesi
    for y, (x0, x1) in {28: (22, 27), 29: (22, 27), 30: (23, 27), 31: (23, 28), 32: (23, 28), 33: (24, 28), 34: (24, 28),
                        35: (24, 28), 36: (24, 28), 37: (25, 28), 38: (25, 28), 39: (25, 29), 40: (25, 29), 41: (25, 29),
                        42: (26, 29), 43: (26, 29)}.items():
        s.span(y, x0, x1, BK, 2, back=1, front=3)
    for y in range(44, 59):
        s.span(y, 26, 29, BK, 2, back=1, front=3)
    for y, (x0, x1) in {59: (25, 30), 60: (25, 31), 61: (25, 32), 62: (25, 33), 63: (25, 33)}.items():
        s.span(y, x0, x1, BK, 2, back=1, front=3)
    s.span(44, 26, 29, BK, 3, back=2); s.span(63, 25, 33, BK, 0)
    s.px([(29, 46), (29, 47), (29, 48), (29, 53), (32, 61), (28, 32), (28, 33)], BK, 4)
    # arka kol (sarkık)
    for y, (x0, x1) in {14: (15, 18), 15: (15, 18), 16: (15, 18), 17: (14, 17), 18: (14, 17), 19: (14, 17), 20: (14, 17),
                        21: (14, 17), 22: (14, 17), 23: (14, 17), 24: (14, 17), 25: (14, 17), 26: (14, 17)}.items():
        s.span(y, x0, x1, CT, 1, back=0)
    s.ascii({26: [(14, "pp")], 27: [(14, "667")], 28: [(15, "67")]}, ALM)
    # gövde: açık yakalı gömlek, solgun göğüs; menteninin arka ve ön panelleri; geniş omuzlar
    for y, (x0, x1) in {12: (19, 27), 13: (18, 28), 14: (18, 28), 15: (18, 28), 16: (18, 28), 17: (18, 28), 18: (19, 28),
                        19: (19, 27), 20: (19, 27), 21: (19, 27), 22: (20, 27), 23: (20, 27)}.items():
        s.span(y, x0, x1, 'ivory', 1, back=0, back_w=2, front=2)
    for y, (x0, x1) in {12: (23, 25), 13: (23, 26), 14: (23, 26), 15: (24, 26), 16: (24, 26), 17: (24, 26), 18: (25, 26), 19: (25, 26)}.items():
        s.span(y, x0, x1, 'pale', 2, back=1, front=3)                       # açık yaka: göğüs
        s.px([(x0 - 1, y)], 'ivory', 3)                                      # yakanın ışıklı kenarı
    s.px([(25, 13), (25, 14), (26, 15)], 'pale', 4); s.px([(24, 18), (25, 20)], 'pale', 1)
    for y, (x0, x1) in {12: (16, 20), 13: (15, 20), 14: (15, 20), 15: (15, 20), 16: (15, 20), 17: (15, 20), 18: (15, 20),
                        19: (16, 20), 20: (16, 20), 21: (16, 20), 22: (16, 20), 23: (17, 20)}.items():
        s.span(y, x0, x1, CT, 1, back=0, front=2)                            # menteninin arka paneli
    for y, (x0, x1) in {13: (27, 29), 14: (27, 30), 15: (27, 30), 16: (27, 30), 17: (27, 29), 18: (27, 29), 19: (27, 29),
                        20: (27, 29), 21: (27, 29), 22: (27, 29), 23: (27, 29)}.items():
        s.span(y, x0, x1, CT, 2, back=1, front=3)                            # ön panel (açık)
    for y in (15, 17, 19, 21):
        s.px([(27, y), (28, y)], 'bronze', 3); s.px([(29, y)], 'bronze', 2)  # altın kordonlar
    s.px([(16, 13), (16, 14), (15, 15)], 'rim', 2)
    # kızıl kuşak ve asılı kızıl şişe
    for y, (x0, x1) in {24: (19, 28), 25: (19, 28), 26: (20, 28)}.items():
        s.span(y, x0, x1, 'blood', 2, back=1, front=3)
    s.ascii({27: [(28, "x")], 28: [(28, "rQ")], 29: [(28, "qR")], 30: [(28, "q")]}, ALM)
    # pelerinin başın arkasında yükselen yakası (iç yüzü kızıl) ve boyun
    s.ascii({3: [(16, "a")], 4: [(15, "ab")], 5: [(15, "abq")], 6: [(15, "abqr")], 7: [(15, "abqr")], 8: [(15, "abqr")],
             9: [(15, "abqr")], 10: [(16, "abq")], 11: [(18, "66")]}, ALM)
    # baş: geriye taranmış, omuzlara inen uzun siyah saç; sivri saç çizgisi; belirgin elmacık, keskin çene
    s.ascii({
        0: [(20, "bccd")],
        1: [(18, "bbccdd")],
        2: [(17, "abbccdd7")],
        3: [(17, "abbbccd78")],
        4: [(17, "abbbb67888")],
        5: [(17, "abbbb6qE78")],
        6: [(17, "abbbb6777789")],
        7: [(17, "abbb5678677")],
        8: [(17, "abbb5677q8")],
        9: [(17, "abbb56778")],
        10: [(17, "abb"), (21, "56667")],
        11: [(17, "abb")],
        12: [(17, "abb")],
        13: [(16, "aab")],
        14: [(15, "aab")],
        15: [(14, "ab")],
    }, ALM)
    s.px([(19, 2), (20, 3), (19, 4), (19, 6)], BK, 3)                     # saçta ışık
    s.px([(24, 7)], 'pale', 4); s.px([(25, 9)], 'pale', 3)                  # elmacık ve çene ışığı
    # ön kol: güçlü omuz, kol öne-aşağı uzanır, ince kılıç (hafif kıvrık) aşağıda
    for y, (x0, x1) in {13: (27, 30), 14: (27, 31), 15: (27, 31), 16: (28, 31), 17: (28, 31), 18: (28, 31), 19: (29, 31),
                        20: (29, 32), 21: (29, 32), 22: (30, 32), 23: (30, 33), 24: (31, 33)}.items():
        s.span(y, x0, x1, CT, 2, back=1, front=3)
    s.span(24, 31, 33, 'bronze', 2, front=3)                                 # altın kol ağzı
    s.ascii({25: [(31, "678")], 26: [(31, "6778")], 27: [(32, "67")]}, ALM)
    curved_blade(s, (34.0, 28.0), -58, 20, 1.5, 'blade', yelman=4)
    s.ascii({26: [(34, "P")], 27: [(35, "p")], 28: [(33, "o")]}, ALM)
    return s.outline()


def ludwig():
    """Ludwig: genç, yakışıklı şövalye; ilk turdaki tam plaka tasarım, arkasında rüzgârda lacivert pelerin."""
    from hand_characters import ludwig as ludwig_v1
    s = Sprite(W_, H_)
    cape = {12: (14, 20), 13: (12, 20), 14: (10, 19), 15: (9, 19), 16: (8, 18), 17: (7, 18), 18: (6, 18), 19: (5, 17),
            20: (5, 17), 21: (4, 17), 22: (4, 17), 23: (4, 17), 24: (4, 17), 25: (4, 17), 26: (5, 17), 27: (5, 17),
            28: (5, 17), 29: (6, 17), 30: (6, 17), 31: (6, 17), 32: (7, 17), 33: (7, 17), 34: (8, 17), 35: (8, 16),
            36: (9, 16), 37: (9, 16), 38: (10, 16), 39: (11, 16), 40: (12, 16)}
    wave_cape(s, 'navy', cape, [((17, 14), (14, 39)), ((14, 15), (10, 36)), ((11, 17), (6, 30))],
              hem_cut=[(12, 40), (10, 38), (6, 31)], rim=[(5, 19), (5, 20), (4, 21), (4, 22)])
    for y, (x0, x1) in cape.items():
        if y >= 33:
            s.px([(x0, y)], 'bronze', 2)
    return compose(s, ludwig_v1(), 0, 2).outline()


KT2 = {
    '5': ('oguz', 0), '6': ('oguz', 1), '7': ('oguz', 2), '8': ('oguz', 3), '9': ('oguz', 4),
    'h': ('kumral', 0), 'm': ('kumral', 1), 'n': ('kumral', 2), 'M': ('kumral', 3),
    'w': ('ivory', 0), '1': ('ivory', 1), '2': ('ivory', 2), '3': ('ivory', 3), '4': ('ivory', 4),
    'a': ('brown', 0), 'b': ('brown', 1), 'c': ('brown', 2), 'd': ('brown', 3), 'e': ('brown', 4),
    'g': ('teal', 0), 'i': ('teal', 1), 'j': ('teal', 2), 'J': ('teal', 3), 'K': ('teal', 4),
    'q': ('red', 1), 'r': ('red', 2), 'R': ('red', 3), 'D': ('red', 4),
    'o': ('bronze', 1), 'p': ('bronze', 2), 'P': ('bronze', 3), 'B': ('bronze', 4),
    'k': ('wood', 0), 'l': ('wood', 1), 'L': ('wood', 2), 'x': ('wood', 3), 'X': ('wood', 4),
    'u': ('amber', 2), 'U': ('amber', 3), 'W': ('amber', 4),
    'E': ('eye', 0),
}


def kutluca():
    """Kutluca: kadim Türk soylu çocuk (12-13). Beyaz ten, badem göz, kumral saç ve arkaya sarkan örgü;
    kürk kenarlı beyaz keçe börk; altın kenarlı, çapraz kapanan camgöbeği kaftan; kızıl kuşakta babasının
    hançeri; babaannesinin bastonunu asa gibi önüne çapraz dikmiş, öbür elinde annesinin tesbihi;
    boynunda rüzgârda kızıl atkı. Cesur ve kararlı (çocuk: asla cinsel çekicilik)."""
    s = Sprite(W_, H_)
    TL = 'teal'
    # rüzgârda atkı (arkada)
    for y, (x0, x1) in {29: (14, 23), 30: (10, 22), 31: (7, 17), 32: (5, 12), 33: (4, 8)}.items():
        s.span(y, x0, x1, 'red', 2, back=1, front=3)
    s.px([(6, 32), (9, 31), (13, 30), (17, 30)], 'red', 1); s.px([(12, 30), (16, 29), (19, 29)], 'red', 4)
    # örgü (arkaya sarkar)
    s.ascii({27: [(20, "n")], 28: [(20, "m")], 29: [(20, "n")], 30: [(19, "m")], 31: [(19, "n")], 32: [(19, "q")]}, KT2)
    # arka kol ve tesbih
    for y in range(31, 40):
        s.span(y, 20, 22, TL, 1, back=0)
    s.ascii({40: [(20, "667")], 41: [(20, "66")]}, KT2)
    s.ascii({42: [(20, "U")], 43: [(20, "u")], 44: [(19, "U")], 45: [(19, "u")], 46: [(19, "U")], 47: [(20, "u")],
             48: [(21, "q")], 49: [(21, "r")], 50: [(21, "q")]}, KT2)
    s.px([(20, 42), (19, 46)], 'amber', 4)
    # bacaklar ve çizmeler (ayrık duruş)
    for y in range(48, 57):
        s.span(y, 20, 22, 'navy', 1, back=0, front=2)
        s.span(y, 25, 27, 'navy', 2, back=1, front=3)
    for y, (x0, x1) in {56: (20, 22), 57: (19, 23), 58: (19, 23), 59: (18, 23), 60: (18, 24), 61: (18, 24), 62: (18, 24), 63: (18, 24)}.items():
        s.span(y, x0, x1, 'brown', 1, back=0, front=2)
    for y, (x0, x1) in {56: (25, 27), 57: (25, 28), 58: (25, 28), 59: (25, 29), 60: (25, 30), 61: (25, 31), 62: (25, 31), 63: (25, 31)}.items():
        s.span(y, x0, x1, 'brown', 2, back=1, front=3)
    s.span(63, 18, 24, 'brown', 0); s.span(63, 25, 31, 'brown', 0); s.span(56, 25, 27, 'brown', 3); s.px([(31, 60)], 'brown', 4)
    # kaftan: dar omuzlar, bele kadar çapraz kapanış, dize kadar açılan etek, altın kenar
    for y, (x0, x1) in {30: (22, 27), 31: (21, 28), 32: (21, 28), 33: (21, 28), 34: (21, 28), 35: (21, 28), 36: (21, 28), 37: (21, 28)}.items():
        s.span(y, x0, x1, TL, 2, back=1, back_w=2, front=3)
    for i, y in enumerate(range(30, 38)):
        s.px([(27 - i // 2, y)], 'bronze', 3 if i % 3 == 0 else 2)          # çapraz kapanışın altın kenarı
    for y, (x0, x1) in {40: (20, 29), 41: (20, 29), 42: (19, 30), 43: (19, 30), 44: (19, 30), 45: (18, 30), 46: (18, 31), 47: (18, 31)}.items():
        s.span(y, x0, x1, TL, 2, back=1, back_w=2, front=3)
        s.px([(x1, y)], 'bronze', 2 if y % 3 else 3)
    s.span(47, 18, 31, 'bronze', 2, back=1, front=3)
    s.line(22, 41, 21, 46, TL, 0); s.line(25, 41, 25, 46, TL, 1); s.px([(26, 42), (27, 44)], TL, 4)
    # kızıl kuşak ve babasının hançeri
    s.span(38, 20, 29, 'red', 2, back=1, front=3); s.span(39, 20, 29, 'red', 1, back=0, front=2)
    s.ascii({35: [(28, "P")], 36: [(28, "l")], 37: [(28, "L")], 38: [(27, "BP")]}, KT2)
    # boyun, atkının düğümü
    s.ascii({29: [(23, "667")], 30: [(23, "RRDr")], 31: [(24, "rq")]}, KT2)
    # baş: kürk kenarlı beyaz keçe börk, kızıl püskül; beyaz ten, badem göz, kumral saç
    s.ascii({
        18: [(23, "234")],
        19: [(22, "12334")],
        20: [(21, "112333")],
        21: [(21, "1122334")],
        22: [(20, "abccddcd")],
        23: [(20, "abcddcdde")],
        24: [(20, "mm67788")],
        25: [(20, "mn67nE78")],
        26: [(20, "mn6777789")],
        27: [(21, "m677788")],
        28: [(21, "m67768")],
        29: [(22, "6677")],
    }, KT2)
    s.ascii({19: [(26, "q")], 18: [(26, "R")]}, KT2)                         # börkte kızıl püskül
    s.px([(22, 25), (22, 26)], 'oguz', 1); s.px([(23, 26)], 'oguz', 2)     # kulak
    s.px([(27, 27)], 'oguz', 3); s.px([(28, 28)], 'oguz', 1)                # yanak, ağız
    # ön kol ve asa gibi çapraz dikilmiş baston (çoban kancası gibi kıvrık sap)
    for y in range(25, 64):
        x = round(37 - 8 * (63 - y) / 39)
        s.px([(x, y)], 'wood', 2 if y % 5 else 3); s.px([(x + 1, y)], 'wood', 1)
    s.ascii({22: [(29, "lxx")], 23: [(28, "lX  x")], 24: [(28, "L   l")]}, KT2)
    for y, (x0, x1) in {31: (26, 28), 32: (26, 29), 33: (27, 29), 34: (27, 30), 35: (28, 30), 36: (29, 31)}.items():
        s.span(y, x0, x1, TL, 2, back=1, front=3)
    s.px([(30, 37), (31, 37)], 'bronze', 3)
    s.ascii({37: [(32, "78")], 38: [(31, "6789")], 39: [(32, "67")]}, KT2)  # el bastonu kavrar
    return s.outline()


KS2 = {
    '5': ('green', 0), '6': ('green', 1), '7': ('green', 2), '8': ('green', 3), '9': ('green', 4),
    'a': ('brown', 0), 'b': ('brown', 1), 'c': ('brown', 2), 'd': ('brown', 3), 'e': ('brown', 4),
    'y': ('mustard', 1), 'Y': ('mustard', 2), 'z': ('mustard', 3), 'Z': ('mustard', 4),
    '1': ('ivory', 1), '2': ('ivory', 2), '3': ('ivory', 3), '4': ('ivory', 4),
    'q': ('red', 1), 'r': ('red', 2), 'R': ('red', 3),
    'k': ('black', 0), 'K': ('black', 1), 'L': ('black', 2),
    'l': ('wood', 1), 'm': ('wood', 2),
    'o': ('bronze', 1), 'p': ('bronze', 2), 'P': ('bronze', 3), 'B': ('bronze', 4),
    'v': ('blade', 1), 'w': ('blade', 2), 'W': ('blade', 3), 'V': ('blade', 4),
    'E': ('eye', 0),
}


def kismet():
    """Kısmet: kurnaz goblin haydut. Yırtık kahverengi kukuletanın yanlarından uzun kulaklar çıkar; gölgede
    parlayan iri sarı göz, kanca burun, sivri dişli sırıtış; göğüste dart kayışı; kamayı ters tutar;
    kısa, bükük bacaklarla çömelmiş; çıplak pençeli yeşil ayaklar."""
    BW = 'brown'
    up = Sprite(W_, H_)                       # üst gövde (sonra 6 satır aşağı yerleşir)
    # omuzlardan sarkan dar, yırtık pelerin
    cape = {31: (19, 24), 32: (17, 24), 33: (16, 24), 34: (15, 24), 35: (14, 24), 36: (14, 24), 37: (13, 23), 38: (13, 23),
            39: (12, 23), 40: (12, 23), 41: (12, 22), 42: (12, 22), 43: (13, 22), 44: (13, 21)}
    wave_cape(up, BW, cape, [((22, 32), (19, 44)), ((19, 33), (15, 43)), ((16, 35), (13, 41))],
              hem_cut=[(12, 42), (14, 44), (16, 44), (18, 44), (13, 43), (20, 44)])
    # gövde: koyu yelek, dart kayışı, kemer ve kese
    for y, (x0, x1) in {33: (24, 31), 34: (23, 32), 35: (23, 32), 36: (23, 33), 37: (23, 33), 38: (23, 33), 39: (24, 33),
                        40: (24, 33), 41: (24, 32), 42: (24, 32), 43: (24, 32)}.items():
        up.span(y, x0, x1, 'black', 2, back=1, back_w=2, front=3)
    up.line(24, 34, 32, 41, BW, 1); up.line(25, 34, 33, 41, BW, 3)
    up.ascii({30: [(26, "R")], 31: [(26, "q"), (29, "R")], 32: [(26, "l"), (29, "q"), (32, "R")], 33: [(26, "l"), (29, "l"), (32, "q")],
              34: [(29, "l"), (32, "l")], 35: [(32, "l")]}, KS2)
    up.span(44, 23, 33, BW, 2, back=1, front=3); up.ascii({44: [(30, "PB")], 45: [(24, "bcc")], 46: [(24, "bb")]}, KS2)
    # baş: kukuleta, gölgede yüz, parlayan iri göz, kanca burun, sivri dişli sırıtış; kulak yandan çıkar
    up.ascii({
        22: [(24, "abc")],
        23: [(22, "abccd")],
        24: [(21, "abbccdd")],
        25: [(20, "abbcccdd")],
        26: [(20, "abbbcccdd")],
        27: [(20, "abbb55666d")],
        28: [(20, "abb5566zZ6")],
        29: [(20, "abb5566zE67")],
        30: [(20, "abb556666778")],
        31: [(20, "abbb5666667889")],
        32: [(21, "abb556666")],
        33: [(21, "abb55"), (27, "4343")],
        34: [(22, "ab"), (25, "56555")],
        35: [(23, "abc")],
    }, KS2)
    up.px([(17, 23), (18, 23), (19, 24)], BW, 2)                                 # kukuletanın sarkan ucu
    up.ascii({28: [(13, "5"), (14, "567")], 29: [(11, "56777")], 30: [(12, "5677")], 31: [(15, "56")]}, KS2)   # uzun kulak
    up.px([(29, 28)], 'mustard', 4)
    # ön kol: öne uzanır, kamayı ters tutar
    for y, (x0, x1) in {35: (31, 33), 36: (32, 34), 37: (33, 35), 38: (34, 36)}.items():
        up.span(y, x0, x1, 'black', 2, back=1, front=3)
    up.ascii({37: [(36, "78")], 38: [(36, "6789")], 39: [(36, "677")]}, KS2)
    up.ascii({38: [(35, "P")], 39: [(35, "p")], 40: [(34, "vW")], 41: [(34, "vWV")], 42: [(34, "vwW")], 43: [(35, "wW")], 44: [(35, "V")]}, KS2)

    s = Sprite(W_, H_)
    # kısa, bükük bacaklar: uyluk koyu şalvar, dizden aşağısı çıplak yeşil, pençeli ayaklar
    for y, (x0, x1) in {50: (23, 27), 51: (22, 26), 52: (21, 25), 53: (20, 24), 54: (20, 23)}.items():
        s.span(y, x0, x1, 'black', 1, back=0, front=2)                            # arka uyluk geriye
    for y, (x0, x1) in {55: (20, 23), 56: (21, 23), 57: (21, 24), 58: (22, 24), 59: (22, 24)}.items():
        s.span(y, x0, x1, 'green', 1, back=0, front=2)                            # arka incik öne
    for y, (x0, x1) in {60: (20, 25), 61: (19, 26), 62: (19, 27), 63: (19, 27)}.items():
        s.span(y, x0, x1, 'green', 1, back=0, front=2)
    s.span(63, 19, 27, 'green', 0); s.px([(25, 62), (27, 62)], 'green', 0); s.px([(27, 63), (26, 63)], 'ivory', 2)
    for y, (x0, x1) in {50: (28, 32), 51: (29, 33), 52: (30, 34), 53: (31, 35), 54: (32, 35)}.items():
        s.span(y, x0, x1, 'black', 2, back=1, front=3)                            # ön uyluk öne
    s.px([(34, 52), (35, 53)], 'black', 4)
    for y, (x0, x1) in {55: (32, 35), 56: (32, 35), 57: (31, 34), 58: (31, 34), 59: (31, 34)}.items():
        s.span(y, x0, x1, 'green', 2, back=1, front=3)                            # ön incik geriye
    for y, (x0, x1) in {60: (30, 36), 61: (30, 37), 62: (30, 38), 63: (30, 38)}.items():
        s.span(y, x0, x1, 'green', 2, back=1, front=3)
    s.span(63, 30, 38, 'green', 0); s.px([(35, 62), (37, 62)], 'green', 0); s.px([(38, 63), (37, 63)], 'ivory', 3)
    compose(s, up, 0, 6)
    return s.outline()


HEROES = {'karamani': karamani, 'ludwig': ludwig, 'almos': almos, 'kutluca': kutluca, 'kismet': kismet}


def main():
    names = sys.argv[1:] or list(HEROES)
    sprites = [HEROES[n]() for n in names]
    print(preview(sprites, os.path.join(REVIEW, 'hero-' + '-'.join(names) + '.png'), scale=8))


if __name__ == '__main__':
    main()
