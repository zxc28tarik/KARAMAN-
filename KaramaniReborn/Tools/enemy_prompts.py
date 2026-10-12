"""Düşman ve boss kare sayfaları için ChatGPT istemleri: Docs/references/enemy-sheets.md üretir.

Kaynak: Docs/specs/2026-10-12-enemy-roster-design.md (§3 düşmanlar, §4 boss'lar). Her karakter için önce
"ana poz" istemi (Karamani profili yalnız stil örneği), sonra her hareket için ana pozun eklendiği istem.
Kullanım: python -I Tools/enemy_prompts.py
"""
import io
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Docs', 'references', 'enemy-sheets.md')

JAN = ("an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a long cloth flap hanging down the back of the neck "
       "and a small brass ornament at the front; thick dark moustache, fair skin, almond eyes, stern grim face; dark crimson long coat tucked up "
       "at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark "
       "trousers; brown leather boots")
TAIL = ("STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, "
        "feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background "
        "color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.")
HURT = ('darbe alma', 3, "a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance")
DEATH = ('ölüm', 5, "a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory")
IDLE = ('bekleme', 4, "a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)")
WALK = ('yürüme', 8, "a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready")

CHARS = [
    ('nefer', 'Kılıç Neferi', 'En sık görülen düşman: önce bu.', JAN + "; a curved yatagan sword in his right hand",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('kombo', 8, "a 2-HIT YATAGAN COMBO: 1-2 wind-up, 3 first diagonal slash with a white motion smear, 4 follow-through, 5 second slash the other way with a smear, 6 impact hold, 7-8 recovering"),
        ('atılış', 6, "a FAST LUNGE: 1 crouching, 2 springing forward, 3 long forward lunge with the sword thrust out, 4 impact hold, 5-6 recovering"),
        HURT, DEATH]),
    ('spearman', 'Kalkanlı Mızrakçı', 'Kalkanıyla önden gelen hafif vuruşları keser.', JAN + "; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('dürtüş', 6, "a LONG SPEAR THRUST from behind the raised shield: 1-2 pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('kalkanla itme', 5, "a SHIELD BASH: 1 bracing, 2 stepping in, 3 slamming the shield forward, 4 hold, 5 recovering"),
        ('blok', 2, "a SHIELD BLOCK: 1 raising the shield in front of the body, 2 braced behind the shield with knees bent"),
        HURT, DEATH]),
    ('halberdier', 'Teberli', 'Yavaş ama geniş ve ağır vuruşlu.', JAN + "; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('yatay savuruş', 8, "a WIDE HORIZONTAL HALBERD SWEEP: 1-3 long wind-up turning the body back, 4 huge horizontal sweep with a wide white smear, 5 follow-through, 6-8 slow recovery"),
        ('yukarıdan iniş', 8, "a HEAVY OVERHEAD HALBERD CHOP: 1-3 raising the halberd high, 4 chopping down hard, 5 blade hitting the ground, 6-8 slow recovery"),
        ('sapla itme', 5, "a SHORT HAFT SHOVE: 1 bracing, 2-3 shoving the haft forward to push the enemy away, 4 hold, 5 recovering"),
        HURT, DEATH]),
    ('musketeer', 'Tüfekli', 'Uzaktan ölümcül tek atış, sonra uzun doldurma.', JAN + "; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('nişan ve atış', 6, "AIMING AND FIRING THE MUSKET: 1 raising the musket to the shoulder, 2-3 aiming steadily, the match glowing, 4 firing with a bright muzzle flash and smoke, 5 recoil, 6 lowering"),
        ('doldurma', 6, "RELOADING THE MUSKET: 1 musket upright, 2 pouring powder from the horn, 3 ramming with the rod, 4 ramming again, 5 putting the rod back, 6 ready"),
        ('dipçik', 5, "a MUSKET BUTT STRIKE: 1 turning the musket around, 2-3 smashing forward with the stock, 4 hold, 5 recovering"),
        HURT, DEATH]),
    ('archer', 'Okçu', 'Yüksek platformlardan hızlı oklar atar.', JAN + "; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('ok atma', 5, "SHOOTING THE BOW STRAIGHT: 1 nocking an arrow, 2 raising and drawing, 3 full draw aiming straight forward, 4 release, 5 lowering"),
        ('yukarı atış', 5, "SHOOTING THE BOW HIGH INTO THE SKY: 1 nocking, 2 raising the bow steeply upward, 3 full draw aiming 60 degrees up, 4 release, 5 lowering"),
        ('geri sıçrama', 3, "a QUICK BACKWARD HOP while facing right: 1 pushing off, 2 in the air hopping back, 3 landing ready"),
        ('hançer', 5, "a QUICK DAGGER SLASH: 1 drawing the dagger, 2 wind-up, 3 fast slash, 4 hold, 5 recovering"),
        HURT, DEATH]),
    ('sorcerer', 'Paşanın Büyücüsü (yarı-boss)', 'Mesafe tutar, büyü yapar, yakına girince asayla dövüşür. Dinî bir figür değil.',
     "the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale face with sunken eyes and a thin black beard, "
     "layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple",
     'the same height as the man in the reference', [IDLE, WALK,
        ('asa kombosu', 8, "a 2-HIT STAFF COMBO: 1-2 wind-up, 3 first staff swing with a smear, 4 follow-through, 5 second swing the other way, 6 hold, 7-8 recovering"),
        ('alev büyüsü', 6, "casting a GROUND FLAME SPELL: 1-2 raising the staff, dark flames gathering in the crystal, 3 slamming the staff end to the ground, 4 black-purple fire bursting forward along the ground, 5-6 recovering"),
        ('küre büyüsü', 6, "casting CURSE ORBS: 1-2 raising the free hand, 3 three glowing purple orbs forming above the hand, 4 throwing the hand forward, orbs flying, 5-6 recovering"),
        ('ışınlanma', 6, "TELEPORTING: 1 wrapping himself in his robe, 2 dissolving into black smoke, 3 only smoke left, 4 smoke gathering, 5 re-forming, 6 standing ready"),
        ('yere vurma', 6, "a STAFF GROUND SLAM: 1-2 raising the staff high with both hands, 3 slamming it into the ground, 4 a purple shock ring bursting around him, 5-6 recovering"),
        ('darbe alma', 3, HURT[2]),
        ('ölüm', 6, "a DEATH: 1 struck, 2 dropping the staff, 3 falling to his knees, 4 body crumbling into black smoke, 5 mostly smoke, 6 only a pile of robe and amulets left")]),
]
EFFECTS = [
    ('alev dalgası efekti', 4, "4 frames of a BLACK-PURPLE GROUND FLAME WAVE travelling to the right, a low wall of dark fire about knee high, loopable, pixel art effect, transparent-looking edges"),
    ('lanet küresi efekti', 4, "4 frames of a GLOWING PURPLE CURSE ORB with a small dark core and wisps, pulsing, loopable, pixel art effect"),
]
BOSSES = [
    ('arslan', 'Bayraktar Arslan (Kızıl Surlar)', 'Dev sancaktar; ikinci evrede sancağı ateşe verir.',
     "Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, thick black beard, "
     "battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash",
     'about 15% TALLER and much broader than the man in the reference', [IDLE, WALK,
        ('direkle dürtüş', 6, "a LONG THRUST with the standard pole: 1-2 pulling back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('direkle savuruş', 8, "a WIDE ARC SWEEP with the standard pole: 1-3 big wind-up, 4 huge sweeping arc with a smear, 5 follow-through, 6-8 recovering"),
        ('sancağı dikme', 6, "PLANTING THE STANDARD and roaring a command: 1 lifting the pole, 2 stabbing it into the ground, 3-4 roaring with an arm raised, 5-6 pulling it out"),
        ('yanan sancakla savuruş', 8, "a SWEEP WITH THE BURNING STANDARD (the banner is on fire): 1-3 wind-up, 4 sweeping arc trailing flames, 5 follow-through, 6-8 recovering"),
        HURT, ('ölüm', 6, DEATH[2])]),
    ('kordemir', 'Humbaracıbaşı Kör Demir (Yanmış Mahalle)', 'Mahalleyi yakan adam: humbara, kanca, yağ küpü.',
     "Chief Grenadier Kor Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, shaved head with a scorched cap, "
     "leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe",
     'about 12% TALLER and broader than the man in the reference', [IDLE, WALK,
        ('humbara atma', 6, "THROWING A CLAY FIRE-GRENADE in a high arc: 1 lighting the fuse, 2 winding up, 3 throwing, 4 follow-through, 5-6 recovering"),
        ('kancayla çekme', 7, "a HOOK PULL: 1-2 swinging the hooked axe forward, 3 hooking, 4-5 yanking it back hard toward himself, 6-7 recovering"),
        ('yağ küpü', 6, "THROWING AN OIL POT: 1 lifting a clay pot over his head, 2-3 winding up, 4 hurling it forward, 5-6 recovering"),
        ('balta inişi', 8, "a HEAVY AXE CHOP: 1-3 raising the axe high, 4 chopping down hard, 5 axe in the ground, 6-8 recovering"),
        HURT, ('ölüm', 6, DEATH[2])]),
    ('kurtbas', 'Deli Kurtbaş (Kurt Geçidi)', 'Kurt postlu vahşi delibaş; doğaüstü değil.',
     "Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, "
     "wild dark hair and beard, leather and fur armor, bare scarred arms, an axe in each hand",
     'about 12% TALLER than the man in the reference', [IDLE,
        ('koşu', 8, "a FAST RUNNING cycle, loopable, low and aggressive, axes held out"),
        ('balta fırtınası', 10, "a 4-HIT AXE STORM: 1 wind-up, 2-3 first slash, 4-5 second slash, 6-7 third slash, 8-9 fourth slash, each with a smear, 10 recovering"),
        ('üstünden atlama', 6, "a FLIP OVER THE ENEMY: 1 crouch, 2 leaping up, 3 upside-down flip in the air, 4 coming down, 5 landing facing back, 6 ready"),
        ('sıçrayıp çakılma', 7, "a LEAPING SLAM: 1 crouch, 2 leaping high, 3 axes raised at the apex, 4 diving down, 5 both axes slamming into the ground, 6-7 rising"),
        ('uluma', 5, "a HOWL into frenzy: 1 hunching, 2-3 throwing the head back howling, 4 shaking with rage, 5 ready, eyes wild"),
        HURT, ('ölüm', 6, DEATH[2])]),
    ('karamurad', 'Yeniçeri Ağası Kara Murad (Karanlık Kale)', 'Dev, kaslı dövüşçü; ikinci evrede çıplak elle.',
     "Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, shoulders nearly twice as wide as a normal man, "
     "bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, black beard, a giant two-handed pala sword",
     'about 25% TALLER and nearly twice as broad as the man in the reference', [IDLE, WALK,
        ('pala kombosu', 10, "a 3-HIT HEAVY PALA COMBO with a slow-fast-slow rhythm: 1-2 wind-up, 3 first heavy slash, 4-5 quick second slash, 6-7 wind-up, 8 huge third slash with a wide smear, 9-10 recovering"),
        ('omuz hücumu', 6, "a SHOULDER CHARGE: 1 lowering the shoulder, 2-4 charging forward fast, 5 impact, 6 recovering"),
        ('yakalayıp fırlatma', 8, "a GRAB AND THROW: 1 reaching forward, 2 grabbing, 3 lifting an invisible enemy overhead, 4-5 slamming down hard, 6 impact, 7-8 recovering"),
        ('yere çakma', 7, "a GROUND SLAM with the pala: 1-3 raising it high with both hands, 4 slamming into the ground, 5 a shock wave bursting forward, 6-7 recovering"),
        ('yumruk kombosu', 8, "a BARE-HANDED FIST COMBO (no sword): 1 guard up, 2-3 heavy straight punch, 4-5 hook, 6-7 uppercut, 8 recovering"),
        HURT, ('ölüm', 6, DEATH[2])]),
]


def base_prompt(desc, size):
    return ("Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), "
            f"but draw a DIFFERENT character: {desc}. The character is {size}.\n"
            "Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. "
            "Flat solid background color #47304C, no shadow, no text.")


def anim_prompt(desc, n, action):
    return f"Pixel art sprite sheet of the SAME character as the attached reference image: {desc}.\n{n} frames of {action}, in ONE horizontal row.\n{TAIL}"


def section(out, key, title, note, desc, size, moves):
    out.append(f"---\n\n## {title}\n{note}\n\n")
    out.append(f"### {key} 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)\n```\n{base_prompt(desc, size)}\n```\n\n")
    for i, (tr, n, action) in enumerate(moves, 1):
        out.append(f"### {key} {i}. {tr}: {n} kare (ana pozu ekle)\n```\n{anim_prompt(desc, n, action)}\n```\n\n")


def main():
    out = ["# Düşmanlar ve boss'lar: kare sayfası istemleri (ChatGPT)\n\n",
           "Tasarım: `Docs/specs/2026-10-12-enemy-roster-design.md`. Bu dosyayı `python -I Tools/enemy_prompts.py` üretir.\n\n",
           "**Nasıl kullanılır:**\n",
           "1. **Önce her karakterin 0 numaralı \"ana poz\" istemi.** ChatGPT'ye `Docs/references/karamani-profile.webp` dosyasını **stil örneği** olarak ekle. Beğendiğin görsel o karakterin ana pozu olur.\n",
           "2. **Sonra o karakterin hareketleri.** Her hareket için yeni bir sohbet aç, **o karakterin ana poz görselini** ekle (Karamani'yi değil), istemi yapıştır.\n",
           "3. Görselleri `İndirilenler`e at ya da buraya gönder; adsız da olur.\n\n",
           "**Sıra:** Kılıç Neferi, Kalkanlı Mızrakçı, Tüfekli, Okçu, Teberli, Büyücü; boss'lar en son. Her karakterde ana poz, bekleme, yürüme, ilk saldırı, darbe alma ve ölüm yeterli; diğerleri sonra gelebilir.\n\n"]
    for c in CHARS:
        section(out, *c)
    out.append("---\n\n## Büyü efektleri (büyücü)\nAna poz gerekmez; Karamani profilini stil örneği olarak ekle.\n\n")
    for i, (tr, n, action) in enumerate(EFFECTS, 1):
        out.append(f"### efekt {i}. {tr}: {n} kare\n```\nUse the attached image ONLY as a STYLE reference (same pixel-art style). {action}. "
                   "Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.\n```\n\n")
    out.append("---\n\n# Boss'lar\n\n")
    for b in BOSSES:
        section(out, *b)
    io.open(OUT, 'w', encoding='utf-8').write(''.join(out))
    print(OUT, sum(1 for line in out if line.startswith('### ')), 'istem')


if __name__ == '__main__':
    main()
