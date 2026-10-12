"""Düşman ve boss kare sayfaları için ChatGPT istemleri: Docs/references/enemy-sheets.md üretir.

Kaynak: Docs/specs/2026-10-12-enemy-roster-design.md (§3 düşmanlar, §4 boss'lar). Her karakter için önce
"ana poz" istemi (Karamani profili yalnız stil örneği), sonra her hareket için ana pozun eklendiği istem.
Kullanım: python -I Tools/enemy_prompts.py
"""
import io
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Docs', 'references', 'enemy-sheets.md')

# Kullanıcının fenotip kuralı (Türkler / Osmanlılar): açık ten, badem göz, kumral saç, bıyık ve sakal; asla siyah / koyu saç, esmer değil
LOOK = "fair skin, almond eyes, chestnut (light brown) hair"
NOT_DARK = "never black or dark hair, not swarthy"
JAN = ("an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; "
       f"{LOOK}, a thick chestnut moustache and a short chestnut beard ({NOT_DARK}), stern grim face; dark crimson long coat tucked up "
       "at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark "
       "trousers; brown leather boots")
TAIL = ("STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, "
        "feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background "
        "color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.")
HURT = ('darbe alma', 3, "a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance")
DEATH = ('ölüm', 5, "a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory")
IDLE = ('bekleme', 4, "a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)")
WALK = ('yürüme', 8, "a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready")
BOSS_DEATH = ('ölüm', 6, "a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the weapon falling from the hands, 5 collapsing forward, 6 lying still face down on the ground. Not gory")

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
        ('yatay savuruş', 8, "a WIDE HORIZONTAL HALBERD SWEEP: 1-3 long wind-up turning the body back, 4 huge horizontal sweep with a wide motion smear, 5 follow-through, 6-8 slow recovery"),
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
     "the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, "
     f"pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard ({NOT_DARK}), "
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
# Kara Murad: silahsız gövde tanımı; ikinci evre istemleri palasız tanımı kullanır (ChatGPT'ye çelişkili "pala" + "kılıçsız" gitmesin)
KARA_MURAD = ("Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, "
              "bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, "
              f"{LOOK}, a thick chestnut moustache and beard ({NOT_DARK})")
KARA_MURAD_BARE = KARA_MURAD + "; bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands"
KARA_MURAD_THROW = KARA_MURAD + "; he holds his giant two-handed pala only in the first frames of this sheet and throws it away, ending bare-handed"
BOSSES = [
    ('arslan', 'Bayraktar Arslan (Kızıl Surlar)', 'Dev sancaktar; ikinci evrede sancağı ateşe verir.',
     "Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, "
     f"{LOOK}, a thick chestnut moustache and beard ({NOT_DARK}), "
     "battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash",
     'about 15% TALLER and much broader than the man in the reference', [IDLE, WALK,
        ('direkle dürtüş', 6, "a LONG THRUST with the standard pole: 1-2 pulling back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('direkle savuruş', 8, "a WIDE ARC SWEEP with the standard pole: 1-3 big wind-up, 4 huge sweeping arc with a smear, 5 follow-through, 6-8 recovering"),
        ('sancağı dikme', 6, "PLANTING THE STANDARD and roaring a command: 1 lifting the pole, 2 stabbing it into the ground, 3-4 roaring with an arm raised, 5-6 pulling it out"),
        ('yanan sancakla savuruş', 8, "a SWEEP WITH THE BURNING STANDARD (the banner is on fire): 1-3 wind-up, 4 sweeping arc trailing flames, 5 follow-through, 6-8 recovering"),
        ('sancağı tutuşturma', 6, "SETTING THE STANDARD ALIGHT (phase change): 1 tearing the standard out of the ground, 2 holding it up, 3 touching the banner to a brazier flame, 4 the banner catching fire, 5 roaring, 6 ready with the burning standard"),
        ('yanan sancakla bekleme', 4, "a subtle IDLE loop holding the BURNING standard, flames flickering (loopable)"),
        ('yanan sancakla yürüme', 8, "a WALKING cycle holding the BURNING standard ready, flames trailing from the banner, loopable, evenly spaced phases"),
        HURT,
        ('yanan sancakla darbe alma', 3, "a HURT reaction while holding the BURNING standard: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, flames swaying, 3 regaining balance"),
        ('ölüm', 6, "a DEATH (he always dies in phase 2, with the BURNING standard): 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the burning standard falling from his hands, 5 collapsing forward, 6 lying still face down, the fallen banner still smouldering beside him. Not gory")]),
    ('kordemir', 'Humbaracıbaşı Kör Demir (Yanmış Mahalle)', 'Mahalleyi yakan adam: humbara, kanca, yağ küpü.',
     "Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, "
     f"{LOOK} on the unburned side, a chestnut moustache and beard ({NOT_DARK}), shaved head with a scorched cap, "
     "leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe",
     'about 12% TALLER and broader than the man in the reference', [IDLE, WALK,
        ('humbara atma', 6, "THROWING A CLAY FIRE-GRENADE in a high arc: 1 lighting the fuse, 2 winding up, 3 throwing, 4 follow-through, 5-6 recovering"),
        ('kancayla çekme', 7, "a HOOK PULL: 1-2 swinging the hooked axe forward, 3 hooking, 4-5 yanking it back hard toward himself, 6-7 recovering"),
        ('yağ küpü', 6, "THROWING AN OIL POT: 1 lifting a clay pot over his head, 2-3 winding up, 4 hurling it forward, 5-6 recovering"),
        ('balta inişi', 8, "a HEAVY AXE CHOP: 1-3 raising the axe high, 4 chopping down hard, 5 axe in the ground, 6-8 recovering"),
        ('arenayı ateşe verme', 7, "SETTING THE GROUND ABLAZE (phase change): 1 tearing two clay grenades from his belt, 2 lighting both fuses, 3-4 hurling them down hard on both sides, 5 a burst of flame at his feet spreading along the ground, 6 laughing with arms spread among the flames, 7 ready"),
        HURT, BOSS_DEATH]),
    ('kurtbas', 'Deli Kurtbaş (Kurt Geçidi)', 'Kurt postlu vahşi delibaş; doğaüstü değil.',
     "Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, "
     f"fair skin, almond eyes, wild chestnut hair and beard ({NOT_DARK}), leather and fur armor, bare scarred arms, an axe in each hand",
     'about 12% TALLER than the man in the reference', [IDLE, WALK,
        ('koşu', 8, "a FAST RUNNING cycle, loopable, low and aggressive, axes held out"),
        ('balta fırtınası', 10, "a 4-HIT AXE STORM: 1 wind-up, 2-3 first slash, 4-5 second slash, 6-7 third slash, 8-9 fourth slash, each with a smear, 10 recovering"),
        ('üstünden atlama', 6, "a FLIP OVER THE ENEMY: 1 crouch, 2 leaping up, 3 upside-down flip in the air, 4 coming down, 5 landing facing back, 6 ready"),
        ('sıçrayıp çakılma', 7, "a LEAPING SLAM: 1 crouch, 2 leaping high, 3 axes raised at the apex, 4 diving down, 5 both axes slamming into the ground, 6-7 rising"),
        ('uluma', 5, "a HOWL into frenzy: 1 hunching, 2-3 throwing the head back howling, 4 shaking with rage, 5 ready, eyes wild"),
        HURT, BOSS_DEATH]),
    ('karamurad', 'Yeniçeri Ağası Kara Murad (Karanlık Kale)', 'Dev, kaslı dövüşçü; ikinci evrede çıplak elle.',
     KARA_MURAD + ", a giant two-handed pala sword",
     'about 60% TALLER and twice as broad as the man in the reference', [IDLE, WALK,
        ('pala kombosu', 10, "a 3-HIT HEAVY PALA COMBO with a slow-fast-slow rhythm: 1-2 wind-up, 3 first heavy slash, 4-5 quick second slash, 6-7 wind-up, 8 huge third slash with a wide smear, 9-10 recovering"),
        ('omuz hücumu', 6, "a SHOULDER CHARGE: 1 lowering the shoulder, 2-4 charging forward fast, 5 impact, 6 recovering"),
        ('yakalayıp fırlatma', 8, "a GRAB AND THROW: 1 reaching forward, 2 grabbing, 3 lifting an invisible enemy overhead, 4-5 slamming down hard, 6 impact, 7-8 recovering"),
        ('yere çakma', 7, "a GROUND SLAM with the pala: 1-3 raising it high with both hands, 4 slamming into the ground, 5 a shock wave bursting forward, 6-7 recovering"),
        ('yumruk kombosu', 8, "a BARE-HANDED FIST COMBO (no sword): 1 guard up, 2-3 heavy straight punch, 4-5 hook, 6-7 uppercut, 8 recovering", KARA_MURAD_BARE),
        ('palayı fırlatma', 5, "THROWING THE PALA AWAY in rage (phase change): 1 holding the pala, 2 raising it, 3 hurling it aside, 4 roaring with fists clenched, 5 bare-handed fighting stance", KARA_MURAD_THROW),
        ('çıplak el bekleme', 4, "a subtle bare-handed IDLE loop in a boxer-wrestler stance, fists raised, NO sword: 1 base, 2 chest rises, 3 shift of weight, 4 almost back (loopable)", KARA_MURAD_BARE),
        ('çıplak el yürüme', 8, "a bare-handed WALKING cycle, fists raised, NO sword, loopable, evenly spaced phases", KARA_MURAD_BARE),
        ('çıplak el omuz hücumu', 6, "a bare-handed SHOULDER CHARGE (no sword): 1 lowering the shoulder, fists clenched, 2-4 charging forward fast, 5 impact, 6 recovering", KARA_MURAD_BARE),
        ('çıplak el yakalayıp fırlatma', 8, "a bare-handed GRAB AND THROW (no sword): 1 reaching forward with both hands, 2 grabbing, 3 lifting an invisible enemy overhead, 4-5 slamming down hard, 6 impact, 7-8 recovering", KARA_MURAD_BARE),
        HURT,
        ('çıplak el darbe alma', 3, HURT[2], KARA_MURAD_BARE),
        ('ölüm', 6, "a DEATH (he always dies bare-handed, in phase 2): 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 one fist still planted on the ground, 5 collapsing forward, 6 lying still face down on the ground. Not gory", KARA_MURAD_BARE)]),
]

# ---- 2. kısım: Çatlak Kaya goblinleri (Docs/specs/2026-10-12-goblin-part2-design.md) ----
# Ton: ciddi, vahşi, korkutucu yağmacılar; komik/sevimli değil. Makine, tüfek, barut yok. Ten kül-gri + soluk yeşilimsi (Kısmet'in canlı yeşili DEĞİL).
GOB = ("a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): "
       "ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, "
       "a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; "
       "no guns, no machines, only period weapons")
GOB_SIZE = "about 68% of the height of the man in the reference (about a third shorter), wiry"
GOB_DEATH = ('ölüm', 5, "a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground, limp. Not gory, no blood")
GOB_BOSS_DEATH = ('ölüm', 6, "a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the weapon falling from the hands, 5 collapsing forward, 6 lying still face down on the ground. Not gory, no blood")
GOB_IDLE = ('bekleme', 4, "a subtle menacing IDLE loop in a low wary stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)")
GOB_WALK = ('yürüme', 8, "a stalking WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, hunched and wary")
GOB_RUN = ('koşu', 8, "a FAST RUNNING cycle, loopable, low and predatory, evenly spaced phases of the stride")

BICAKCI = GOB + "; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest"
OKCU = GOB + "; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid"
PUSUCU = GOB + "; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared"
KALKANLI = GOB + "; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap"
COMLEKCI = GOB + "; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks"
KABA = GOB + "; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck"
KABA_SIZE = "about the same height as the man in the reference but TWICE as broad, hunched forward, heavy and frightening"
KIRCIL = ("the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), "
          "pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, "
          "a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines")
KIRCIL_SIZE = "about 90% of the height of the man in the reference, tall, thin and old"
CATAL_FOOT = GOB + "; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot"
CATAL_MOUNT = (GOB + "; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, "
               "riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane")
CATAL_BOAR = ("a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, "
              "scarred hide; NO rider, no machines")
KARADIS = ("the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint "
           "(NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, "
           "bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, "
           "a heavy curved black-edged pala sword in EACH hand; no guns, no machines")
KARADIS_SIZE = "about 10% TALLER than the man in the reference, broad-shouldered"

GOBLINS = [
    ('bicakci', 'Bıçakçı', "Sürünün temeli; 2-3'lü gruplar hâlinde gelir. Cılız, sırım gibi, sinsi.", BICAKCI, GOB_SIZE, [
        GOB_IDLE, GOB_RUN,
        ('saplama', 6, "a LEAPING DOUBLE DAGGER STAB: 1 coiling low, 2 springing forward, 3 first stab in mid-air, 4 second stab in mid-air, 5 landing, 6 recovering"),
        ('yuvarlanıp dolanma', 6, "a SIDE-STEP ROLL past the enemy: 1 ducking low, 2 starting to roll forward, 3 tucked in mid-roll, 4 coming out of the roll, 5 rising into a crouch, 6 a quick dagger stab"),
        ('geri sıçrama', 3, "a QUICK BACKWARD HOP while facing right: 1 pushing off, 2 in the air hopping back, 3 landing in a crouch"),
        HURT, GOB_DEATH]),
    ('okcu', 'Zehirli Okçu', 'Yüksek yerleri sever, mesafe tutar. Oklar siyah uçlu.', OKCU, GOB_SIZE, [
        GOB_IDLE, GOB_RUN,
        ('ok atma', 5, "SHOOTING THE BOW STRAIGHT with a black-tipped arrow: 1 nocking an arrow, 2 raising and drawing, 3 full draw aiming straight forward, 4 release, 5 lowering"),
        ('yukarı atış', 5, "SHOOTING THE BOW HIGH INTO THE SKY (arrow volley): 1 nocking, 2 raising the bow steeply upward, 3 full draw aiming 60 degrees up, 4 release, 5 lowering"),
        ('geri sıçrama', 3, "a QUICK BACKWARD HOP while facing right, bow in hand: 1 pushing off, 2 in the air hopping back, 3 landing in a crouch"),
        HURT, GOB_DEATH]),
    ('pusucu', 'Pusucu (üstüne atlayan goblin)',
     'Arka plandaki pusu noktasında (dal, çıkıntı, kiriş) bekler, gözü parlayınca atlar. Pusuda beklemenin 4. karesi göz parlaması uyarısıdır. Düz arka planda dal/kaya ÇİZDİRME, yalnız goblin.',
     PUSUCU, GOB_SIZE, [
        ('pusuda bekleme (4. kare: göz parlaması uyarısı)', 4, "an AMBUSH WAIT, crouched on a high branch or ledge in the dark, waiting motionless (draw ONLY the goblin, in a deep squat as if perched on a thin branch, NO branch, NO rock, NO tree in the image): "
          "1 deep squat, long arms hanging between the knees, head low and watching, 2 barely breathing, cloak stirring slightly, 3 head turning slowly to follow prey below, "
          "4 the WARNING FRAME: eyes flare bright glowing yellow-red in the dark, teeth bared, body tensed and coiled ready to spring. Only frame 4 has glowing eyes"),
        ('atlayış', 6, "a POUNCING LEAP from above onto the enemy below: 1 eyes glowing bright yellow-red, coiling on the perch, 2 launching himself off, 3 flying through the air, daggers drawn back, 4 diving, 5 both daggers thrust down-forward, 6 striking with both daggers"),
        ('yere çakılma', 4, "CRASHING TO THE GROUND after a missed pounce, left vulnerable for a moment: 1 hitting the ground hard, 2 sprawled and stunned, 3 shaking the head, 4 starting to rise"),
        ('bekleme', 4, "a subtle menacing IDLE loop on the ground in a predatory crouch, a dagger in each hand: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)"),
        GOB_RUN,
        ('hançer kombosu', 8, "a 3-HIT DAGGER COMBO with two curved daggers: 1 wind-up, 2 first slash with a white motion smear, 3 follow-through, 4 second slash with the other dagger and a smear, 5 follow-through, 6 third crossing slash with both daggers and a smear, 7-8 quick recovering"),
        HURT, GOB_DEATH]),
    ('kalkanli', 'Kalkanlı Mızrakçı (goblin)', 'İkili sıra kurar; kalkanıyla önden gelen hafif vuruşları keser.', KALKANLI, GOB_SIZE, [
        GOB_IDLE, GOB_WALK,
        ('dürtüş', 6, "a LONG SPEAR THRUST from behind the raised shield: 1-2 pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('kalkanla itme', 5, "a SHIELD BASH: 1 bracing, 2 stepping in, 3 slamming the shield forward, 4 hold, 5 recovering"),
        ('blok', 2, "a SHIELD BLOCK: 1 raising the shield in front of the body, 2 braced behind the shield with knees bent"),
        HURT, GOB_DEATH]),
    ('comlekci', 'Ateş Çömlekçisi', 'Sırtında yağ çömlekleri; ölünce çömlekler kırılıp tutuşur.', COMLEKCI, GOB_SIZE, [
        GOB_IDLE, GOB_WALK,
        ('çömlek fırlatma', 6, "THROWING A BURNING OIL POT in a high arc: 1 pulling a clay pot from the back, 2 lighting the rag in its mouth, 3 winding up, 4 hurling it forward, 5 follow-through, 6 recovering. Small flame on the rag only"),
        ('yakın vuruş', 5, "a CLOSE POT SMASH: 1 raising a clay pot, 2 winding up, 3 smashing the pot forward, 4 shards flying and a brief small flame, 5 recovering"),
        HURT,
        ('ölüm (çömleklerin kırılıp tutuşması)', 6, "a DEATH where his own pots burst: 1 struck and staggering, 2 dropping to the knees, 3 the clay pots on his back cracking, 4 the pots breaking and bursting into flames, 5 falling forward, 6 lying face down with a fire burning around the broken pots. Not gory, no blood")]),
    ('kaba', 'Kaba (dev goblin)', 'Yavaş ama durmadan ilerler; hafif vuruşla sendelemez.', KABA, KABA_SIZE, [
        GOB_IDLE,
        ('yürüme', 8, "a heavy, lumbering WALKING cycle, loopable, hunched, the chained cleaver dragging, evenly spaced phases of the step"),
        ('ağır iniş', 8, "a HEAVY CLEAVER CHOP: 1-3 raising the huge cleaver high with both hands, 4 chopping down with all his weight, 5 cleaver slamming into the ground, 6-8 slow recovering"),
        ('zincir savuruşu', 8, "a WIDE CHAINED CLEAVER SWING: 1-3 winding up, the cleaver swinging on its chain, 4-5 huge wide horizontal arc with a motion smear, 6 follow-through, 7-8 slow recovering"),
        ('yakalayıp çarpma', 8, "a GRAB AND SLAM: 1 a lumbering step forward, 2 both arms reaching, 3 grabbing, 4 lifting an invisible enemy overhead, 5-6 slamming down hard, 7-8 recovering"),
        HURT,
        ('ölüm', 6, "a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the cleaver sliding from the hand, 5 toppling forward, 6 lying still face down on the ground. Not gory, no blood")]),
    ('kircil', 'Kan Şamanı Kırçıl (yarı-boss)', 'Mesafe tutar, büyü yapar; kan bağıyla yakındaki goblinleri iyileştirir. Efektler bu bölümün sonunda.', KIRCIL, KIRCIL_SIZE, [
        ('bekleme', 4, "a subtle IDLE loop leaning on the staff: 1 base pose, 2 chest rises slightly, 3 skull staff swaying a little, 4 almost back to the start (loopable)"),
        ('yürüme', 8, "a slow stalking WALKING cycle, loopable, the staff planted with each step, evenly spaced phases"),
        ('asa kombosu', 8, "a 2-HIT STAFF COMBO: 1-2 wind-up, 3 first staff swing with a smear, 4 follow-through, 5 second swing the other way with a smear, 6 hold, 7-8 recovering"),
        ('lanet büyüsü', 6, "casting a BLACK CURSE: 1-2 raising the skull staff, 3 slamming the staff end to the ground, dark energy gathering, 4 holding, mask eyes glowing faint red, 5-6 recovering"),
        ('kurt çağırma', 6, "SUMMONING SPIRIT WOLVES: 1 raising both arms, 2 throwing the head back, 3 dark wisps swirling around him, 4 pointing the staff forward, 5-6 recovering. Draw ONLY the shaman, no wolves"),
        ('iyileştirme', 6, "a BLOOD-BOND HEALING RITUAL: 1 raising the staff, 2 lifting the free hand, 3-4 holding both hands out, faint red light pulsing around the hands, 5 lowering, 6 ready. Not gory"),
        ('ışınlanma', 6, "TELEPORTING in a bone-white mist: 1 wrapping himself in the wolf pelt, 2 dissolving into pale mist, 3 only mist left, 4 mist gathering, 5 re-forming, 6 standing ready"),
        ('kan ritüeli (evre geçişi)', 6, "a PHASE-CHANGE BLOOD RITUAL: 1 raising a short bone knife, 2 drawing the knife across his own forearm (only a thin dark red line, a few drops, NO gore), 3 raising the arm, 4 a red halo glow forming around him, 5 roaring, 6 ready, glowing red. Not gory"),
        HURT,
        ('ölüm', 6, "a DEATH: 1 struck, 2 dropping the skull staff, 3 falling to his knees, 4 the bone mask cracking, 5 collapsing forward, 6 lying still, the mask and staff beside him. Not gory, no blood")]),
    ('catal', 'Domuz Binicisi Çatal: binekli (birinci evre)', 'Çatal kocaman mağara domuzunun üstünde. Ana poz, binekli hâli gösterir. İkinci evre için sonraki iki bölüme bak.',
     CATAL_MOUNT, 'about 15% TALLER than the man in the reference in total (the huge boar and the goblin rider together), the boar long and massive', [
        ('bekleme', 4, "a subtle IDLE loop of the mounted rider: 1 base pose, 2 the boar breathes heavily and the rider sits steady, 3 the boar paws the ground once, 4 almost back to the start (loopable)"),
        ('koşu', 8, "a GALLOPING cycle of the boar with the rider, loopable, low and fast, the spear held forward, evenly spaced phases of the gallop"),
        ('hücum', 6, "a CHARGE: 1 the boar lowering its head and tusks, the rider levelling the spear, 2 pawing and launching, 3-4 full charge with the head down and a trail of dust, 5 full speed, 6 skidding to a stop"),
        ('mızrak', 6, "a LONG SPEAR THRUST from the saddle: 1-2 the rider pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('şahlanma', 6, "the boar REARING UP and slamming down: 1-2 the boar coiling and lifting, 3 rearing high on its hind legs with the rider hanging on, 4 slamming the front hooves down, 5 a dusty shock ring bursting around the hooves, 6 settling")]),
    ('catal_yaya', 'Domuz Binicisi Çatal: yaya (ikinci evre)', 'Çatal canı yarıya inince domuzdan atlar; mızrakla hızlı bir yaya dövüşçü. Domuz da ayrıca saldırmaya devam eder. Kendi ana pozu var.',
     CATAL_FOOT, GOB_SIZE, [
        ('bekleme', 4, "a subtle menacing IDLE loop in a low stance with the forked spear held across the body: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)"),
        GOB_RUN,
        ('mızrak kombosu', 8, "a FAST 3-HIT SPEAR COMBO: 1 wind-up, 2 first quick thrust, 3 pulling back, 4 diagonal slash with the spear shaft and a smear, 5 follow-through, 6 powerful final thrust, 7-8 recovering"),
        ('binekten atlama', 5, "LEAPING OFF THE BOAR on purpose to fight on foot (draw ONLY the goblin, NO boar, the space under him empty): 1 crouching in the saddle, 2 springing up and off, spear held high, 3 airborne, 4 landing in a crouch, 5 rising into a ready stance with the spear"),
        HURT,
        ('ölüm', 6, "a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the spear falling from the hands, 5 collapsing forward, 6 lying still face down on the ground, the horsetail banner beside him. Not gory, no blood")]),
    ('catal_domuz', 'Domuz Binicisi Çatal: domuz tek başına', 'Çatal atlayınca binicisiz domuz da saldırır; öldürülebilir, kolay bir düşman. Kendi ana pozu var.',
     CATAL_BOAR, 'about 70% of the height of the man at its shoulder hump and much longer than he is tall (it is huge); riderless, an empty bone saddle on its back', [
        ('bekleme', 4, "a subtle IDLE loop of the riderless boar: 1 base pose, 2 snorting, head lifting slightly, 3 pawing the ground once, 4 almost back to the start (loopable)"),
        ('koşu', 8, "a GALLOPING cycle of the riderless boar, loopable, low and fast, evenly spaced phases of the gallop"),
        ('hücum', 6, "a CHARGE: 1 lowering the head and tusks, 2 pawing and launching, 3-4 full charge with the head down and a trail of dust, 5 full speed, 6 skidding to a stop"),
        ('diş savuruşu', 6, "a TUSK GORE: 1 lowering the head, 2 stepping in, 3 hooking the tusks upward hard with a white smear, 4 hold, 5-6 recovering"),
        ('darbe alma', 3, "a HURT reaction: 1 flinching from a hit to the flank, 2 stumbling sideways a step, 3 regaining footing"),
        ('ölüm', 5, "a DEATH: 1 struck hard, 2 staggering, 3 front legs buckling, 4 rolling onto its side, 5 lying still. Not gory, no blood")]),
]
KARADIS_MOVES = [
    ('bekleme', 4, "a subtle menacing IDLE loop with a pala in each hand held low: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)"),
    ('koşu', 8, "a FAST RUNNING cycle, loopable, low and predatory, both palas trailing behind, evenly spaced phases of the stride"),
    ('çift pala kombosu', 12, "a 4-HIT DUAL PALA COMBO: 1-2 wind-up, 3 first diagonal slash with the right pala and a white smear, 4 follow-through, 5 second slash crossing with the left pala and a smear, 6 follow-through, 7 third slash with the right pala and a smear, 8 follow-through, 9 fourth slash with both palas and a big smear, 10 impact hold, 11-12 recovering"),
    ('yatay dönüş kesiği', 8, "a HORIZONTAL SPINNING SLASH that keeps enemies away: 1-2 a short crouch as the warning, 3 both palas swung wide to the sides, 4-5 spinning a full turn with a wide circular motion smear, 6 still spinning, 7-8 recovering"),
    ('X atılışı', 6, "an X-SLASH DASH: 1 crossing both palas over the chest, 2 coiling, 3 lunging forward in a blur, 4 slashing through with both palas in an X shape and a smear, 5 skidding to a stop, 6 standing ready"),
    ('yükselen kesik', 6, "a RISING SLASH against an airborne enemy: 1 crouching, 2 leaping upward, 3 both palas slashing upward with a smear, 4 apex of the jump, 5 landing, 6 recovering"),
    ('üçgen hava saldırısı', 12, "a TRIANGLE AIR ATTACK drawing a triangle in the air: 1 crouching, 2 leaping up and backward, 3 apex, 4 hanging in the air with both palas glowing, 5 diving diagonally and slashing, 6 bouncing off the ground, 7 springing high again, 8 apex of the second corner, hovering with palas glowing, 9 diving straight down, 10 slamming into the ground with a small shock ring, 11-12 staying low and recovering"),
    ('geri ışınlanma', 6, "a SHORT BACKWARD TELEPORT in red ember dust: 1 crouching, 2 dissolving into red embers, 3 only glowing embers left, 4 embers gathering, 5 re-forming, 6 standing ready"),
    ('kaya sekmesi', 5, "a LEDGE REBOUND (draw ONLY the chieftain, NO rock, NO beam): 1 crouched with feet gripping an invisible ledge, 2 pushing off, 3 airborne with the palas drawn back, 4 diving forward with the palas forward, 5 landing in a crouch"),
    ('çekme büyüsü', 6, "casting a PULL SPELL: 1 raising one empty hand, 2 arm extended, palm open, eyes and chest tattoo glowing red, 3 holding for a moment, 4 clenching the hand and pulling it back, 5 the other pala drawn back ready to slash, 6 ready. Draw no chain"),
    ('gök ateşi büyüsü', 6, "casting a SKY FIRE SPELL: 1 lowering into a stance, 2 raising both palas to the sky, 3 eyes and chest tattoo glowing red, 4 hold, 5 hold with the glow pulsing, 6 lowering the palas. Draw no fireballs"),
    ('pala fırtınası', 12, "a 6-HIT PALA STORM, slashes getting faster: 1-2 wind-up, 3 slash one, 4 slash two, 5 slash three, 6 slash four, 7 slash five, each with a smear, 8-9 raising both palas overhead, 10 a heavy final downward chop with both palas, 11-12 recovering"),
    ('evre geçişi (kükreme)', 6, "a PHASE-CHANGE ROAR: 1 faltering, 2 dropping to one knee, 3 head bowed, the chest tattoo igniting, 4 throwing the head back and roaring, eyes and tattoo blazing red, 5 rising, 6 ready"),
    ('nefeslenme', 4, "a TIRED BREATHER, both palas planted point-down in the ground, leaning on them: 1 chest heaving, 2 head low, 3 chest heaving, 4 almost back to the start (loopable)"),
    HURT, GOB_BOSS_DEATH,
]
GOBLINS.append(('karadis', 'Oymak Beyi Kara Diş (boss)', 'Yaşlı, yapılı, çift palalı düellocu. En son. Efektler bu bölümün sonunda.', KARADIS, KARADIS_SIZE, KARADIS_MOVES))
GOBLIN_EFFECTS = {
    'kircil': ('Kırçıl efektleri', "Ana poz gerekmez; Karamani profilini stil örneği olarak ekle.", [
        ('diken halkası efekti', 4, "4 frames of BLACK THORNS ERUPTING FROM THE GROUND in a line (the cross-section of a curse ring, seen from the side): 1 cracks and dark glow on the ground, 2 sharp black thorns shooting up, 3 full height row of tall black thorns, 4 thorns crumbling away, pixel art effect"),
        ('ruh kurdu efekti', 6, "6 frames of a SEMI-TRANSPARENT SPIRIT WOLF, black and smoky with pale glowing eyes, in a fast RUNNING cycle in strict side profile facing right, loopable, wisps trailing behind, pixel art effect")]),
    'karadis': ('Kara Diş efektleri', "Ana poz gerekmez; Karamani profilini stil örneği olarak ekle.", [
        ('ateş topu efekti (düşerken)', 4, "4 frames of a FALLING FIREBALL dropping from the sky (it falls straight down, the tail of flame trailing UP), pulsing, loopable, pixel art effect"),
        ('ateş topu yere çarpma', 5, "5 frames of a FIREBALL IMPACT on the ground: 1 touching down, 2 bursting, 3 largest fireball flash, 4 flames spreading low and fading, 5 embers and smoke, pixel art effect"),
        ('yer halkası efekti', 3, "3 frames of a thin flat RED RING of glowing embers on the ground, seen edge-on as a flat glowing ellipse, pulsing brighter, a warning marker, pixel art effect"),
        ('kara zincir efekti (çekme)', 4, "4 frames of a BLACK CHAIN OF DARK ENERGY with a faint red glow, stretched horizontally and pulling toward the left, links shifting, loopable, pixel art effect"),
        ('kor tozu efekti (ışınlanma)', 4, "4 frames of a PUFF OF RED EMBER DUST (teleport smoke): 1 sparks bursting, 2 cloud of red embers, 3 embers drifting up, 4 fading away, pixel art effect")]),
}


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
    for i, move in enumerate(moves, 1):
        tr, n, action = move[:3]
        move_desc = move[3] if len(move) > 3 else desc   # hareketin kendi tanımı varsa (Kara Murad ikinci evre)
        out.append(f"### {key} {i}. {tr}: {n} kare (ana pozu ekle)\n```\n{anim_prompt(move_desc, n, action)}\n```\n\n")


def effects_block(out, title, note, effects):
    out.append(f"---\n\n## {title}\n{note}\n\n")
    for i, (tr, n, action) in enumerate(effects, 1):
        out.append(f"### efekt {i}. {tr}: {n} kare\n```\nUse the attached image ONLY as a STYLE reference (same pixel-art style). {action}. "
                   "Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.\n```\n\n")


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
    effects_block(out, "Büyü efektleri (büyücü)", "Ana poz gerekmez; Karamani profilini stil örneği olarak ekle.", EFFECTS)
    out.append("---\n\n# Boss'lar\n\n")
    for b in BOSSES:
        section(out, *b)
    out.append("---\n\n# 2. Kısım: Çatlak Kaya goblinleri\n\n"
               "Tasarım: `Docs/specs/2026-10-12-goblin-part2-design.md`. Goblinler ciddi, vahşi, korkutucu yağmacılardır: komik ya da sevimli değil, makine ve barut yok. "
               "Ten kül-gri ve soluk yeşilimsi (Kısmet'in canlı yeşili DEĞİL), kırmızı-siyah savaş dövmeleri, kemik küpe, deri-kürk-kemik zırh. "
               "Sıradan goblin Karamani'nin yaklaşık %68'i boyundadır. Kullanım yukarıdakiyle aynı (önce ana poz, sonra hareketler).\n\n"
               "**Sıra:** Bıçakçı, Okçu, Pusucu, Kalkanlı, Çömlekçi, Kaba, Kırçıl (+ efektleri), Çatal (binekli, yaya, domuz), Kara Diş en son (+ efektleri). "
               "Çatal'ın üç bölümünün her birinin kendi ana pozu var.\n\n")
    for g in GOBLINS:
        section(out, *g)
        if g[0] in GOBLIN_EFFECTS:
            effects_block(out, *GOBLIN_EFFECTS[g[0]])
    io.open(OUT, 'w', encoding='utf-8').write(''.join(out))
    print(OUT, sum(1 for line in out if line.startswith('### ')), 'istem')


if __name__ == '__main__':
    main()
