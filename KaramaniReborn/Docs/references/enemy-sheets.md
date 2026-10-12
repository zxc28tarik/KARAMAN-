# Düşmanlar ve boss'lar: kare sayfası istemleri (ChatGPT)

Tasarım: `Docs/specs/2026-10-12-enemy-roster-design.md`. Bu dosyayı `python -I Tools/enemy_prompts.py` üretir.

**Nasıl kullanılır:**
1. **Önce her karakterin 0 numaralı "ana poz" istemi.** ChatGPT'ye `Docs/references/karamani-profile.webp` dosyasını **stil örneği** olarak ekle. Beğendiğin görsel o karakterin ana pozu olur.
2. **Sonra o karakterin hareketleri.** Her hareket için yeni bir sohbet aç, **o karakterin ana poz görselini** ekle (Karamani'yi değil), istemi yapıştır.
3. Görselleri `İndirilenler`e at ya da buraya gönder; adsız da olur.

**Sıra:** Kılıç Neferi, Kalkanlı Mızrakçı, Tüfekli, Okçu, Teberli, Büyücü; boss'lar en son. Her karakterde ana poz, bekleme, yürüme, ilk saldırı, darbe alma ve ölüm yeterli; diğerleri sonra gelebilir.

---

## Kılıç Neferi
En sık görülen düşman: önce bu.

### nefer 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand. The character is about 7% shorter than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### nefer 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### nefer 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### nefer 3. kombo: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand.
8 frames of a 2-HIT YATAGAN COMBO: 1-2 wind-up, 3 first diagonal slash with a white motion smear, 4 follow-through, 5 second slash the other way with a smear, 6 impact hold, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### nefer 4. atılış: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand.
6 frames of a FAST LUNGE: 1 crouching, 2 springing forward, 3 long forward lunge with the sword thrust out, 4 impact hold, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### nefer 5. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### nefer 6. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a curved yatagan sword in his right hand.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Kalkanlı Mızrakçı
Kalkanıyla önden gelen hafif vuruşları keser.

### spearman 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand. The character is about 7% shorter than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### spearman 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### spearman 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### spearman 3. dürtüş: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
6 frames of a LONG SPEAR THRUST from behind the raised shield: 1-2 pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### spearman 4. kalkanla itme: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
5 frames of a SHIELD BASH: 1 bracing, 2 stepping in, 3 slamming the shield forward, 4 hold, 5 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### spearman 5. blok: 2 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
2 frames of a SHIELD BLOCK: 1 raising the shield in front of the body, 2 braced behind the shield with knees bent, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### spearman 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### spearman 7. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Teberli
Yavaş ama geniş ve ağır vuruşlu.

### halberdier 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands. The character is about 7% shorter than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### halberdier 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### halberdier 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### halberdier 3. yatay savuruş: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
8 frames of a WIDE HORIZONTAL HALBERD SWEEP: 1-3 long wind-up turning the body back, 4 huge horizontal sweep with a wide motion smear, 5 follow-through, 6-8 slow recovery, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### halberdier 4. yukarıdan iniş: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
8 frames of a HEAVY OVERHEAD HALBERD CHOP: 1-3 raising the halberd high, 4 chopping down hard, 5 blade hitting the ground, 6-8 slow recovery, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### halberdier 5. sapla itme: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
5 frames of a SHORT HAFT SHOVE: 1 bracing, 2-3 shoving the haft forward to push the enemy away, 4 hold, 5 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### halberdier 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### halberdier 7. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Tüfekli
Uzaktan ölümcül tek atış, sonra uzun doldurma.

### musketeer 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt. The character is about 7% shorter than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### musketeer 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### musketeer 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### musketeer 3. nişan ve atış: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
6 frames of AIMING AND FIRING THE MUSKET: 1 raising the musket to the shoulder, 2-3 aiming steadily, the match glowing, 4 firing with a bright muzzle flash and smoke, 5 recoil, 6 lowering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### musketeer 4. doldurma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
6 frames of RELOADING THE MUSKET: 1 musket upright, 2 pouring powder from the horn, 3 ramming with the rod, 4 ramming again, 5 putting the rod back, 6 ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### musketeer 5. dipçik: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
5 frames of a MUSKET BUTT STRIKE: 1 turning the musket around, 2-3 smashing forward with the stock, 4 hold, 5 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### musketeer 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### musketeer 7. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Okçu
Yüksek platformlardan hızlı oklar atar.

### archer 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash. The character is about 7% shorter than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### archer 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 3. ok atma: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
5 frames of SHOOTING THE BOW STRAIGHT: 1 nocking an arrow, 2 raising and drawing, 3 full draw aiming straight forward, 4 release, 5 lowering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 4. yukarı atış: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
5 frames of SHOOTING THE BOW HIGH INTO THE SKY: 1 nocking, 2 raising the bow steeply upward, 3 full draw aiming 60 degrees up, 4 release, 5 lowering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 5. geri sıçrama: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
3 frames of a QUICK BACKWARD HOP while facing right: 1 pushing off, 2 in the air hopping back, 3 landing ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 6. hançer: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
5 frames of a QUICK DAGGER SLASH: 1 drawing the dagger, 2 wind-up, 3 fast slash, 4 hold, 5 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 7. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### archer 8. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a small brass ornament at the front; fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and a short chestnut beard (never black or dark hair, not swarthy), stern grim face; dark crimson long coat tucked up at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark trousers; brown leather boots; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Paşanın Büyücüsü (yarı-boss)
Mesafe tutar, büyü yapar, yakına girince asayla dövüşür. Dinî bir figür değil.

### sorcerer 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple. The character is the same height as the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### sorcerer 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 3. asa kombosu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
8 frames of a 2-HIT STAFF COMBO: 1-2 wind-up, 3 first staff swing with a smear, 4 follow-through, 5 second swing the other way, 6 hold, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 4. alev büyüsü: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
6 frames of casting a GROUND FLAME SPELL: 1-2 raising the staff, dark flames gathering in the crystal, 3 slamming the staff end to the ground, 4 black-purple fire bursting forward along the ground, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 5. küre büyüsü: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
6 frames of casting CURSE ORBS: 1-2 raising the free hand, 3 three glowing purple orbs forming above the hand, 4 throwing the hand forward, orbs flying, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 6. ışınlanma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
6 frames of TELEPORTING: 1 wrapping himself in his robe, 2 dissolving into black smoke, 3 only smoke left, 4 smoke gathering, 5 re-forming, 6 standing ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 7. yere vurma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
6 frames of a STAFF GROUND SLAM: 1-2 raising the staff high with both hands, 3 slamming it into the ground, 4 a purple shock ring bursting around him, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 8. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### sorcerer 9. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale fair skin, sunken almond eyes and a thin grey-streaked chestnut beard (never black or dark hair, not swarthy), layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple.
6 frames of a DEATH: 1 struck, 2 dropping the staff, 3 falling to his knees, 4 body crumbling into black smoke, 5 mostly smoke, 6 only a pile of robe and amulets left, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Büyü efektleri (büyücü)
Ana poz gerekmez; Karamani profilini stil örneği olarak ekle.

### efekt 1. alev dalgası efekti: 4 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 4 frames of a BLACK-PURPLE GROUND FLAME WAVE travelling to the right, a low wall of dark fire about knee high, loopable, pixel art effect, transparent-looking edges. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

### efekt 2. lanet küresi efekti: 4 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 4 frames of a GLOWING PURPLE CURSE ORB with a small dark core and wisps, pulsing, loopable, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

---

# Boss'lar

---

## Bayraktar Arslan (Kızıl Surlar)
Dev sancaktar; ikinci evrede sancağı ateşe verir.

### arslan 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash. The character is about 15% TALLER and much broader than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### arslan 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 3. direkle dürtüş: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
6 frames of a LONG THRUST with the standard pole: 1-2 pulling back, 3 thrusting far forward, 4 hold, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 4. direkle savuruş: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
8 frames of a WIDE ARC SWEEP with the standard pole: 1-3 big wind-up, 4 huge sweeping arc with a smear, 5 follow-through, 6-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 5. sancağı dikme: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
6 frames of PLANTING THE STANDARD and roaring a command: 1 lifting the pole, 2 stabbing it into the ground, 3-4 roaring with an arm raised, 5-6 pulling it out, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 6. yanan sancakla savuruş: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
8 frames of a SWEEP WITH THE BURNING STANDARD (the banner is on fire): 1-3 wind-up, 4 sweeping arc trailing flames, 5 follow-through, 6-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 7. sancağı tutuşturma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
6 frames of SETTING THE STANDARD ALIGHT (phase change): 1 tearing the standard out of the ground, 2 holding it up, 3 touching the banner to a brazier flame, 4 the banner catching fire, 5 roaring, 6 ready with the burning standard, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 8. yanan sancakla bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
4 frames of a subtle IDLE loop holding the BURNING standard, flames flickering (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 9. yanan sancakla yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
8 frames of a WALKING cycle holding the BURNING standard ready, flames trailing from the banner, loopable, evenly spaced phases, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 10. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 11. yanan sancakla darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
3 frames of a HURT reaction while holding the BURNING standard: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, flames swaying, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### arslan 12. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash.
6 frames of a DEATH (he always dies in phase 2, with the BURNING standard): 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the burning standard falling from his hands, 5 collapsing forward, 6 lying still face down, the fallen banner still smouldering beside him. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Humbaracıbaşı Kör Demir (Yanmış Mahalle)
Mahalleyi yakan adam: humbara, kanca, yağ küpü.

### kordemir 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe. The character is about 12% TALLER and broader than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### kordemir 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 3. humbara atma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
6 frames of THROWING A CLAY FIRE-GRENADE in a high arc: 1 lighting the fuse, 2 winding up, 3 throwing, 4 follow-through, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 4. kancayla çekme: 7 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
7 frames of a HOOK PULL: 1-2 swinging the hooked axe forward, 3 hooking, 4-5 yanking it back hard toward himself, 6-7 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 5. yağ küpü: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
6 frames of THROWING AN OIL POT: 1 lifting a clay pot over his head, 2-3 winding up, 4 hurling it forward, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 6. balta inişi: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
8 frames of a HEAVY AXE CHOP: 1-3 raising the axe high, 4 chopping down hard, 5 axe in the ground, 6-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 7. arenayı ateşe verme: 7 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
7 frames of SETTING THE GROUND ABLAZE (phase change): 1 tearing two clay grenades from his belt, 2 lighting both fuses, 3-4 hurling them down hard on both sides, 5 a burst of flame at his feet spreading along the ground, 6 laughing with arms spread among the flames, 7 ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 8. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kordemir 9. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Chief Grenadier Kör Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, fair skin, almond eyes, chestnut (light brown) hair on the unburned side, a chestnut moustache and beard (never black or dark hair, not swarthy), shaved head with a scorched cap, leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe.
6 frames of a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the weapon falling from the hands, 5 collapsing forward, 6 lying still face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Deli Kurtbaş (Kurt Geçidi)
Kurt postlu vahşi delibaş; doğaüstü değil.

### kurtbas 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand. The character is about 12% TALLER than the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### kurtbas 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 3. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
8 frames of a FAST RUNNING cycle, loopable, low and aggressive, axes held out, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 4. balta fırtınası: 10 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
10 frames of a 4-HIT AXE STORM: 1 wind-up, 2-3 first slash, 4-5 second slash, 6-7 third slash, 8-9 fourth slash, each with a smear, 10 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 5. üstünden atlama: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
6 frames of a FLIP OVER THE ENEMY: 1 crouch, 2 leaping up, 3 upside-down flip in the air, 4 coming down, 5 landing facing back, 6 ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 6. sıçrayıp çakılma: 7 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
7 frames of a LEAPING SLAM: 1 crouch, 2 leaping high, 3 axes raised at the apex, 4 diving down, 5 both axes slamming into the ground, 6-7 rising, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 7. uluma: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
5 frames of a HOWL into frenzy: 1 hunching, 2-3 throwing the head back howling, 4 shaking with rage, 5 ready, eyes wild, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 8. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kurtbas 9. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, fair skin, almond eyes, wild chestnut hair and beard (never black or dark hair, not swarthy), leather and fur armor, bare scarred arms, an axe in each hand.
6 frames of a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the weapon falling from the hands, 5 collapsing forward, 6 lying still face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Yeniçeri Ağası Kara Murad (Karanlık Kale)
Dev, kaslı dövüşçü; ikinci evrede çıplak elle.

### karamurad 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword. The character is about 60% TALLER and twice as broad as the man in the reference.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### karamurad 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
4 frames of a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
8 frames of a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 3. pala kombosu: 10 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
10 frames of a 3-HIT HEAVY PALA COMBO with a slow-fast-slow rhythm: 1-2 wind-up, 3 first heavy slash, 4-5 quick second slash, 6-7 wind-up, 8 huge third slash with a wide smear, 9-10 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 4. omuz hücumu: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
6 frames of a SHOULDER CHARGE: 1 lowering the shoulder, 2-4 charging forward fast, 5 impact, 6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 5. yakalayıp fırlatma: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
8 frames of a GRAB AND THROW: 1 reaching forward, 2 grabbing, 3 lifting an invisible enemy overhead, 4-5 slamming down hard, 6 impact, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 6. yere çakma: 7 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
7 frames of a GROUND SLAM with the pala: 1-3 raising it high with both hands, 4 slamming into the ground, 5 a shock wave bursting forward, 6-7 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 7. yumruk kombosu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
8 frames of a BARE-HANDED FIST COMBO (no sword): 1 guard up, 2-3 heavy straight punch, 4-5 hook, 6-7 uppercut, 8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 8. palayı fırlatma: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); he holds his giant two-handed pala only in the first frames of this sheet and throws it away, ending bare-handed.
5 frames of THROWING THE PALA AWAY in rage (phase change): 1 holding the pala, 2 raising it, 3 hurling it aside, 4 roaring with fists clenched, 5 bare-handed fighting stance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 9. çıplak el bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
4 frames of a subtle bare-handed IDLE loop in a boxer-wrestler stance, fists raised, NO sword: 1 base, 2 chest rises, 3 shift of weight, 4 almost back (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 10. çıplak el yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
8 frames of a bare-handed WALKING cycle, fists raised, NO sword, loopable, evenly spaced phases, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 11. çıplak el omuz hücumu: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
6 frames of a bare-handed SHOULDER CHARGE (no sword): 1 lowering the shoulder, fists clenched, 2-4 charging forward fast, 5 impact, 6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 12. çıplak el yakalayıp fırlatma: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
8 frames of a bare-handed GRAB AND THROW (no sword): 1 reaching forward with both hands, 2 grabbing, 3 lifting an invisible enemy overhead, 4-5 slamming down hard, 6 impact, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 13. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy), a giant two-handed pala sword.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 14. çıplak el darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karamurad 15. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, twice as broad as a normal man, bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, fair skin, almond eyes, chestnut (light brown) hair, a thick chestnut moustache and beard (never black or dark hair, not swarthy); bare-handed, his pala thrown away: NO sword or weapon anywhere, empty hands.
6 frames of a DEATH (he always dies bare-handed, in phase 2): 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 one fist still planted on the ground, 5 collapsing forward, 6 lying still face down on the ground. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

# 2. Kısım: Çatlak Kaya goblinleri

Tasarım: `Docs/specs/2026-10-12-goblin-part2-design.md`. Goblinler ciddi, vahşi, korkutucu yağmacılardır: komik ya da sevimli değil, makine ve barut yok. Ten kül-gri ve soluk yeşilimsi (Kısmet'in canlı yeşili DEĞİL), kırmızı-siyah savaş dövmeleri, kemik küpe, deri-kürk-kemik zırh. Sıradan goblin Karamani'nin yaklaşık %68'i boyundadır. Kullanım yukarıdakiyle aynı (önce ana poz, sonra hareketler).

**Sıra:** Bıçakçı, Okçu, Pusucu, Kalkanlı, Çömlekçi, Kaba, Kırçıl (+ efektleri), Çatal (binekli, yaya, domuz), Kara Diş en son (+ efektleri). Çatal'ın üç bölümünün her birinin kendi ana pozu var.

---

## Bıçakçı
Sürünün temeli; 2-3'lü gruplar hâlinde gelir. Cılız, sırım gibi, sinsi.

### bicakci 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest. The character is about 68% of the height of the man in the reference (about a third shorter), wiry.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### bicakci 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
4 frames of a subtle menacing IDLE loop in a low wary stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### bicakci 2. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
8 frames of a FAST RUNNING cycle, loopable, low and predatory, evenly spaced phases of the stride, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### bicakci 3. saplama: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
6 frames of a LEAPING DOUBLE DAGGER STAB: 1 coiling low, 2 springing forward, 3 first stab in mid-air, 4 second stab in mid-air, 5 landing, 6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### bicakci 4. yuvarlanıp dolanma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
6 frames of a SIDE-STEP ROLL past the enemy: 1 ducking low, 2 starting to roll forward, 3 tucked in mid-roll, 4 coming out of the roll, 5 rising into a crouch, 6 a quick dagger stab, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### bicakci 5. geri sıçrama: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
3 frames of a QUICK BACKWARD HOP while facing right: 1 pushing off, 2 in the air hopping back, 3 landing in a crouch, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### bicakci 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### bicakci 7. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; lean and stringy, a broad rusty dagger (kama) in the right hand, a leather rag wrap on the other arm, a red tattoo across the face, a necklace of bone and teeth on the bare chest.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground, limp. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Zehirli Okçu
Yüksek yerleri sever, mesafe tutar. Oklar siyah uçlu.

### okcu 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid. The character is about 68% of the height of the man in the reference (about a third shorter), wiry.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### okcu 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
4 frames of a subtle menacing IDLE loop in a low wary stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### okcu 2. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
8 frames of a FAST RUNNING cycle, loopable, low and predatory, evenly spaced phases of the stride, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### okcu 3. ok atma: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
5 frames of SHOOTING THE BOW STRAIGHT with a black-tipped arrow: 1 nocking an arrow, 2 raising and drawing, 3 full draw aiming straight forward, 4 release, 5 lowering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### okcu 4. yukarı atış: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
5 frames of SHOOTING THE BOW HIGH INTO THE SKY (arrow volley): 1 nocking, 2 raising the bow steeply upward, 3 full draw aiming 60 degrees up, 4 release, 5 lowering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### okcu 5. geri sıçrama: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
3 frames of a QUICK BACKWARD HOP while facing right, bow in hand: 1 pushing off, 2 in the air hopping back, 3 landing in a crouch, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### okcu 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### okcu 7. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; short, a short horn-and-sinew bow in the left hand, a fur quiver at the hip full of arrows with black-tipped heads, the lower half of the face covered with a dirty cloth, hair in a single braid.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground, limp. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Pusucu (üstüne atlayan goblin)
Arka plandaki pusu noktasında (dal, çıkıntı, kiriş) bekler, gözü parlayınca atlar. Pusuda beklemenin 4. karesi göz parlaması uyarısıdır. Düz arka planda dal/kaya ÇİZDİRME, yalnız goblin.

### pusucu 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared. The character is about 68% of the height of the man in the reference (about a third shorter), wiry.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### pusucu 1. pusuda bekleme (4. kare: göz parlaması uyarısı): 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
4 frames of an AMBUSH WAIT, crouched on a high branch or ledge in the dark, waiting motionless (draw ONLY the goblin, in a deep squat as if perched on a thin branch, NO branch, NO rock, NO tree in the image): 1 deep squat, long arms hanging between the knees, head low and watching, 2 barely breathing, cloak stirring slightly, 3 head turning slowly to follow prey below, 4 the WARNING FRAME: eyes flare bright glowing yellow-red in the dark, teeth bared, body tensed and coiled ready to spring. Only frame 4 has glowing eyes, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 2. atlayış: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
6 frames of a POUNCING LEAP from above onto the enemy below: 1 eyes glowing bright yellow-red, coiling on the perch, 2 launching himself off, 3 flying through the air, daggers drawn back, 4 diving, 5 both daggers thrust down-forward, 6 striking with both daggers, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 3. yere çakılma: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
4 frames of CRASHING TO THE GROUND after a missed pounce, left vulnerable for a moment: 1 hitting the ground hard, 2 sprawled and stunned, 3 shaking the head, 4 starting to rise, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 4. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
4 frames of a subtle menacing IDLE loop on the ground in a predatory crouch, a dagger in each hand: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 5. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
8 frames of a FAST RUNNING cycle, loopable, low and predatory, evenly spaced phases of the stride, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 6. hançer kombosu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
8 frames of a 3-HIT DAGGER COMBO with two curved daggers: 1 wind-up, 2 first slash with a white motion smear, 3 follow-through, 4 second slash with the other dagger and a smear, 5 follow-through, 6 third crossing slash with both daggers and a smear, 7-8 quick recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 7. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### pusucu 8. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a dark cloak of fur and moss, the face painted with ash and black, a short curved dagger in EACH hand, a very predatory build: long arms, hunched, teeth bared.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground, limp. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Kalkanlı Mızrakçı (goblin)
İkili sıra kurar; kalkanıyla önden gelen hafif vuruşları keser.

### kalkanli 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap. The character is about 68% of the height of the man in the reference (about a third shorter), wiry.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### kalkanli 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
4 frames of a subtle menacing IDLE loop in a low wary stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kalkanli 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
8 frames of a stalking WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, hunched and wary, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kalkanli 3. dürtüş: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
6 frames of a LONG SPEAR THRUST from behind the raised shield: 1-2 pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kalkanli 4. kalkanla itme: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
5 frames of a SHIELD BASH: 1 bracing, 2 stepping in, 3 slamming the shield forward, 4 hold, 5 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kalkanli 5. blok: 2 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
2 frames of a SHIELD BLOCK: 1 raising the shield in front of the body, 2 braced behind the shield with knees bent, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kalkanli 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kalkanli 7. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; thick-skinned and stocky, a big round shield of bone and stretched animal hide on the left arm, a long spear with a bone-inlaid tip in the right hand, fur over the shoulders, a horned leather cap.
5 frames of a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground, limp. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Ateş Çömlekçisi
Sırtında yağ çömlekleri; ölünce çömlekler kırılıp tutuşur.

### comlekci 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks. The character is about 68% of the height of the man in the reference (about a third shorter), wiry.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### comlekci 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks.
4 frames of a subtle menacing IDLE loop in a low wary stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### comlekci 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks.
8 frames of a stalking WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready, hunched and wary, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### comlekci 3. çömlek fırlatma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks.
6 frames of THROWING A BURNING OIL POT in a high arc: 1 pulling a clay pot from the back, 2 lighting the rag in its mouth, 3 winding up, 4 hurling it forward, 5 follow-through, 6 recovering. Small flame on the rag only, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### comlekci 4. yakın vuruş: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks.
5 frames of a CLOSE POT SMASH: 1 raising a clay pot, 2 winding up, 3 smashing the pot forward, 4 shards flying and a brief small flame, 5 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### comlekci 5. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### comlekci 6. ölüm (çömleklerin kırılıp tutuşması): 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; clay pots full of flammable oil strapped to his back in a rope net, rags tied at the waist, hands and arms covered in old burn scars, a hard soot-blackened face; no gunpowder, only oil pots with cloth wicks.
6 frames of a DEATH where his own pots burst: 1 struck and staggering, 2 dropping to the knees, 3 the clay pots on his back cracking, 4 the pots breaking and bursting into flames, 5 falling forward, 6 lying face down with a fire burning around the broken pots. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Kaba (dev goblin)
Yavaş ama durmadan ilerler; hafif vuruşla sendelemez.

### kaba 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck. The character is about the same height as the man in the reference but TWICE as broad, hunched forward, heavy and frightening.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### kaba 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
4 frames of a subtle menacing IDLE loop in a low wary stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kaba 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
8 frames of a heavy, lumbering WALKING cycle, loopable, hunched, the chained cleaver dragging, evenly spaced phases of the step, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kaba 3. ağır iniş: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
8 frames of a HEAVY CLEAVER CHOP: 1-3 raising the huge cleaver high with both hands, 4 chopping down with all his weight, 5 cleaver slamming into the ground, 6-8 slow recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kaba 4. zincir savuruşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
8 frames of a WIDE CHAINED CLEAVER SWING: 1-3 winding up, the cleaver swinging on its chain, 4-5 huge wide horizontal arc with a motion smear, 6 follow-through, 7-8 slow recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kaba 5. yakalayıp çarpma: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
8 frames of a GRAB AND SLAM: 1 a lumbering step forward, 2 both arms reaching, 3 grabbing, 4 lifting an invisible enemy overhead, 5-6 slamming down hard, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kaba 6. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kaba 7. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hulking hunched brute, twice as broad as an ordinary goblin, back covered in scars and tattoos, a huge cleaver chained to his forearm, chains looted from burned villages hanging around his neck.
6 frames of a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the cleaver sliding from the hand, 5 toppling forward, 6 lying still face down on the ground. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Kan Şamanı Kırçıl (yarı-boss)
Mesafe tutar, büyü yapar; kan bağıyla yakındaki goblinleri iyileştirir. Efektler bu bölümün sonunda.

### kircil 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines. The character is about 90% of the height of the man in the reference, tall, thin and old.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### kircil 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
4 frames of a subtle IDLE loop leaning on the staff: 1 base pose, 2 chest rises slightly, 3 skull staff swaying a little, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 2. yürüme: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
8 frames of a slow stalking WALKING cycle, loopable, the staff planted with each step, evenly spaced phases, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 3. asa kombosu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
8 frames of a 2-HIT STAFF COMBO: 1-2 wind-up, 3 first staff swing with a smear, 4 follow-through, 5 second swing the other way with a smear, 6 hold, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 4. lanet büyüsü: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
6 frames of casting a BLACK CURSE: 1-2 raising the skull staff, 3 slamming the staff end to the ground, dark energy gathering, 4 holding, mask eyes glowing faint red, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 5. kurt çağırma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
6 frames of SUMMONING SPIRIT WOLVES: 1 raising both arms, 2 throwing the head back, 3 dark wisps swirling around him, 4 pointing the staff forward, 5-6 recovering. Draw ONLY the shaman, no wolves, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 6. iyileştirme: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
6 frames of a BLOOD-BOND HEALING RITUAL: 1 raising the staff, 2 lifting the free hand, 3-4 holding both hands out, faint red light pulsing around the hands, 5 lowering, 6 ready. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 7. ışınlanma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
6 frames of TELEPORTING in a bone-white mist: 1 wrapping himself in the wolf pelt, 2 dissolving into pale mist, 3 only mist left, 4 mist gathering, 5 re-forming, 6 standing ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 8. kan ritüeli (evre geçişi): 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
6 frames of a PHASE-CHANGE BLOOD RITUAL: 1 raising a short bone knife, 2 drawing the knife across his own forearm (only a thin dark red line, a few drops, NO gore), 3 raising the arm, 4 a red halo glow forming around him, 5 roaring, 6 ready, glowing red. Not gory, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 9. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### kircil 10. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the blood shaman Kircil of the Cracked Rock goblins (dark fantasy, serious and frightening, NOT comic): old, tall and gaunt, ash-grey skin with a pale greenish tint (NOT bright green), pointed ears with bone ear rings, a carved bone mask on the face, a grey wolf pelt on the back, strings of bones and charms around the neck, a twisted staff topped with an animal skull in the hand, arms covered in old ritual scars, red-black tattoos, looted cloth scraps; no guns, no machines.
6 frames of a DEATH: 1 struck, 2 dropping the skull staff, 3 falling to his knees, 4 the bone mask cracking, 5 collapsing forward, 6 lying still, the mask and staff beside him. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Kırçıl efektleri
Ana poz gerekmez; Karamani profilini stil örneği olarak ekle.

### efekt 1. diken halkası efekti: 4 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 4 frames of BLACK THORNS ERUPTING FROM THE GROUND in a line (the cross-section of a curse ring, seen from the side): 1 cracks and dark glow on the ground, 2 sharp black thorns shooting up, 3 full height row of tall black thorns, 4 thorns crumbling away, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

### efekt 2. ruh kurdu efekti: 6 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 6 frames of a SEMI-TRANSPARENT SPIRIT WOLF, black and smoky with pale glowing eyes, in a fast RUNNING cycle in strict side profile facing right, loopable, wisps trailing behind, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

---

## Domuz Binicisi Çatal: binekli (birinci evre)
Çatal kocaman mağara domuzunun üstünde. Ana poz, binekli hâli gösterir. İkinci evre için sonraki iki bölüme bak.

### catal 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane. The character is about 15% TALLER than the man in the reference in total (the huge boar and the goblin rider together), the boar long and massive.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### catal 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane.
4 frames of a subtle IDLE loop of the mounted rider: 1 base pose, 2 the boar breathes heavily and the rider sits steady, 3 the boar paws the ground once, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal 2. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane.
8 frames of a GALLOPING cycle of the boar with the rider, loopable, low and fast, the spear held forward, evenly spaced phases of the gallop, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal 3. hücum: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane.
6 frames of a CHARGE: 1 the boar lowering its head and tusks, the rider levelling the spear, 2 pawing and launching, 3-4 full charge with the head down and a trail of dust, 5 full speed, 6 skidding to a stop, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal 4. mızrak: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane.
6 frames of a LONG SPEAR THRUST from the saddle: 1-2 the rider pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal 5. şahlanma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, riding a HUGE tusked cave boar armoured with bone plates (no iron), the boar has great curved tusks and a bristled mane.
6 frames of the boar REARING UP and slamming down: 1-2 the boar coiling and lifting, 3 rearing high on its hind legs with the rider hanging on, 4 slamming the front hooves down, 5 a dusty shock ring bursting around the hooves, 6 settling, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Domuz Binicisi Çatal: yaya (ikinci evre)
Çatal canı yarıya inince domuzdan atlar; mızrakla hızlı bir yaya dövüşçü. Domuz da ayrıca saldırmaya devam eder. Kendi ana pozu var.

### catal_yaya 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot. The character is about 68% of the height of the man in the reference (about a third shorter), wiry.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### catal_yaya 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot.
4 frames of a subtle menacing IDLE loop in a low stance with the forked spear held across the body: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_yaya 2. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot.
8 frames of a FAST RUNNING cycle, loopable, low and predatory, evenly spaced phases of the stride, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_yaya 3. mızrak kombosu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot.
8 frames of a FAST 3-HIT SPEAR COMBO: 1 wind-up, 2 first quick thrust, 3 pulling back, 4 diagonal slash with the spear shaft and a smear, 5 follow-through, 6 powerful final thrust, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_yaya 4. binekten atlama: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot.
5 frames of LEAPING OFF THE BOAR on purpose to fight on foot (draw ONLY the goblin, NO boar, the space under him empty): 1 crouching in the saddle, 2 springing up and off, spear held high, 3 airborne, 4 landing in a crouch, 5 rising into a ready stance with the spear, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_yaya 5. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_yaya 6. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a savage goblin raider of the Cracked Rock clan (dark fantasy, 1566 period, serious and frightening, NOT comic, NOT cute, no goofy expression): ash-grey skin with a pale greenish tint (clearly NOT bright green), pointed ears with bone ear rings, hollow cheeks, yellow or reddish eyes, sharp teeth, a cruel hostile face, red-black war tattoos on the face and arms, crude leather, fur and bone armour, scraps of looted cloth worn as trophies; no guns, no machines, only period weapons; a hard-faced warrior with ONE EAR CUT OFF, light bone-plate armour (no iron), a long forked spear (two-pronged tip) in the hands, the clan's horsetail banner on his back, on foot.
6 frames of a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the spear falling from the hands, 5 collapsing forward, 6 lying still face down on the ground, the horsetail banner beside him. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Domuz Binicisi Çatal: domuz tek başına
Çatal atlayınca binicisiz domuz da saldırır; öldürülebilir, kolay bir düşman. Kendi ana pozu var.

### catal_domuz 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines. The character is about 70% of the height of the man at its shoulder hump and much longer than he is tall (it is huge); riderless, an empty bone saddle on its back.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### catal_domuz 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines.
4 frames of a subtle IDLE loop of the riderless boar: 1 base pose, 2 snorting, head lifting slightly, 3 pawing the ground once, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_domuz 2. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines.
8 frames of a GALLOPING cycle of the riderless boar, loopable, low and fast, evenly spaced phases of the gallop, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_domuz 3. hücum: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines.
6 frames of a CHARGE: 1 lowering the head and tusks, 2 pawing and launching, 3-4 full charge with the head down and a trail of dust, 5 full speed, 6 skidding to a stop, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_domuz 4. diş savuruşu: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines.
6 frames of a TUSK GORE: 1 lowering the head, 2 stepping in, 3 hooking the tusks upward hard with a white smear, 4 hold, 5-6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_domuz 5. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines.
3 frames of a HURT reaction: 1 flinching from a hit to the flank, 2 stumbling sideways a step, 3 regaining footing, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### catal_domuz 6. ölüm: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: a HUGE tusked cave boar (dark fantasy), armoured with bone plates instead of iron, great curved tusks, bristled mane, small red-rimmed eyes, scarred hide; NO rider, no machines.
5 frames of a DEATH: 1 struck hard, 2 staggering, 3 front legs buckling, 4 rolling onto its side, 5 lying still. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Oymak Beyi Kara Diş (boss)
Yaşlı, yapılı, çift palalı düellocu. En son. Efektler bu bölümün sonunda.

### karadis 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)
```
Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), but draw a DIFFERENT character: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines. The character is about 10% TALLER than the man in the reference, broad-shouldered.
Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. Flat solid background color #47304C, no shadow, no text.
```

### karadis 1. bekleme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
4 frames of a subtle menacing IDLE loop with a pala in each hand held low: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 2. koşu: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
8 frames of a FAST RUNNING cycle, loopable, low and predatory, both palas trailing behind, evenly spaced phases of the stride, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 3. çift pala kombosu: 12 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
12 frames of a 4-HIT DUAL PALA COMBO: 1-2 wind-up, 3 first diagonal slash with the right pala and a white smear, 4 follow-through, 5 second slash crossing with the left pala and a smear, 6 follow-through, 7 third slash with the right pala and a smear, 8 follow-through, 9 fourth slash with both palas and a big smear, 10 impact hold, 11-12 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 4. yatay dönüş kesiği: 8 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
8 frames of a HORIZONTAL SPINNING SLASH that keeps enemies away: 1-2 a short crouch as the warning, 3 both palas swung wide to the sides, 4-5 spinning a full turn with a wide circular motion smear, 6 still spinning, 7-8 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 5. X atılışı: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of an X-SLASH DASH: 1 crossing both palas over the chest, 2 coiling, 3 lunging forward in a blur, 4 slashing through with both palas in an X shape and a smear, 5 skidding to a stop, 6 standing ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 6. yükselen kesik: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of a RISING SLASH against an airborne enemy: 1 crouching, 2 leaping upward, 3 both palas slashing upward with a smear, 4 apex of the jump, 5 landing, 6 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 7. üçgen hava saldırısı: 12 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
12 frames of a TRIANGLE AIR ATTACK drawing a triangle in the air: 1 crouching, 2 leaping up and backward, 3 apex, 4 hanging in the air with both palas glowing, 5 diving diagonally and slashing, 6 bouncing off the ground, 7 springing high again, 8 apex of the second corner, hovering with palas glowing, 9 diving straight down, 10 slamming into the ground with a small shock ring, 11-12 staying low and recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 8. geri ışınlanma: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of a SHORT BACKWARD TELEPORT in red ember dust: 1 crouching, 2 dissolving into red embers, 3 only glowing embers left, 4 embers gathering, 5 re-forming, 6 standing ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 9. kaya sekmesi: 5 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
5 frames of a LEDGE REBOUND (draw ONLY the chieftain, NO rock, NO beam): 1 crouched with feet gripping an invisible ledge, 2 pushing off, 3 airborne with the palas drawn back, 4 diving forward with the palas forward, 5 landing in a crouch, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 10. çekme büyüsü: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of casting a PULL SPELL: 1 raising one empty hand, 2 arm extended, palm open, eyes and chest tattoo glowing red, 3 holding for a moment, 4 clenching the hand and pulling it back, 5 the other pala drawn back ready to slash, 6 ready. Draw no chain, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 11. gök ateşi büyüsü: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of casting a SKY FIRE SPELL: 1 lowering into a stance, 2 raising both palas to the sky, 3 eyes and chest tattoo glowing red, 4 hold, 5 hold with the glow pulsing, 6 lowering the palas. Draw no fireballs, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 12. pala fırtınası: 12 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
12 frames of a 6-HIT PALA STORM, slashes getting faster: 1-2 wind-up, 3 slash one, 4 slash two, 5 slash three, 6 slash four, 7 slash five, each with a smear, 8-9 raising both palas overhead, 10 a heavy final downward chop with both palas, 11-12 recovering, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 13. evre geçişi (kükreme): 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of a PHASE-CHANGE ROAR: 1 faltering, 2 dropping to one knee, 3 head bowed, the chest tattoo igniting, 4 throwing the head back and roaring, eyes and tattoo blazing red, 5 rising, 6 ready, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 14. nefeslenme: 4 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
4 frames of a TIRED BREATHER, both palas planted point-down in the ground, leaning on them: 1 chest heaving, 2 head low, 3 chest heaving, 4 almost back to the start (loopable), in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 15. darbe alma: 3 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
3 frames of a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

### karadis 16. ölüm: 6 kare (ana pozu ekle)
```
Pixel art sprite sheet of the SAME character as the attached reference image: the goblin chieftain Kara Dis of the Cracked Rock clan (dark fantasy boss, serious and frightening, NOT comic): old but sinewy and broad-shouldered, ash-grey skin with a pale greenish tint (NOT bright green), long dirty grey hair and braided beard, pointed ears with bone ear rings, a hollow sharp face, yellow-red eyes, sharp teeth with the FRONT TEETH CAPPED IN BLACK IRON, bare tattooed arms, a red ritual tattoo on the chest, a wolf pelt over the waist and shoulders, bone charms on the belt, a heavy curved black-edged pala sword in EACH hand; no guns, no machines.
6 frames of a DEATH: 1 struck hard, 2 staggering back, 3 dropping to one knee, 4 the weapon falling from the hands, 5 collapsing forward, 6 lying still face down on the ground. Not gory, no blood, in ONE horizontal row.
STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

---

## Kara Diş efektleri
Ana poz gerekmez; Karamani profilini stil örneği olarak ekle.

### efekt 1. ateş topu efekti (düşerken): 4 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 4 frames of a FALLING FIREBALL dropping from the sky (it falls straight down, the tail of flame trailing UP), pulsing, loopable, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

### efekt 2. ateş topu yere çarpma: 5 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 5 frames of a FIREBALL IMPACT on the ground: 1 touching down, 2 bursting, 3 largest fireball flash, 4 flames spreading low and fading, 5 embers and smoke, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

### efekt 3. yer halkası efekti: 3 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 3 frames of a thin flat RED RING of glowing embers on the ground, seen edge-on as a flat glowing ellipse, pulsing brighter, a warning marker, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

### efekt 4. kara zincir efekti (çekme): 4 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 4 frames of a BLACK CHAIN OF DARK ENERGY with a faint red glow, stretched horizontally and pulling toward the left, links shifting, loopable, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

### efekt 5. kor tozu efekti (ışınlanma): 4 kare
```
Use the attached image ONLY as a STYLE reference (same pixel-art style). 4 frames of a PUFF OF RED EMBER DUST (teleport smoke): 1 sparks bursting, 2 cloud of red embers, 3 embers drifting up, 4 fading away, pixel art effect. Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.
```

