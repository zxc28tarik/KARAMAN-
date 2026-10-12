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

