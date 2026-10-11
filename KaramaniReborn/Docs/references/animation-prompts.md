# Prompts for animation sheets (for the image model)

Goal: produce the animation frames for the characters in `characters-lineup-ref.webp` in the same style and the same costume, but **in strict side profile**. Each prompt yields **one sheet = one animation**. `Tools/sprite_from_art.py --grid name:N` slices it automatically and converts it to the game's 256 px scale.

## Why side profile
In a side-scrolling game the character moves along a single line. The body, hips and feet must point in the direction of travel, as in Castlevania SotN, Blasphemous, The Last Faith and Hollow Knight. The reference uses a three-quarter view turned toward the camera, so a walk drawn in that view looks like "walking toward the camera" (user feedback, 2026-10-11). Warping cannot turn an image into profile. **Every gameplay animation must be drawn in profile.**

## Step 0: one profile base pose per character
Before any animation, get one **strict side-profile standing pose** image of each character. After that, attach that profile image (not the three-quarter reference) to every animation request, so the frames stay consistent with each other.
```
Pixel art of the SAME character as the attached reference image: [DESCRIPTION].
Strict side profile view facing right (like a side-scrolling action game sprite: Castlevania Symphony
of the Night / Blasphemous style), standing idle, full body, the body, hips and feet pointing right.
Same costume, colors and proportions as the reference. Flat solid background #47304C, no shadow, no text.
```

## Rules for every sheet (keep them in every prompt)
- **Attach the profile base pose image** from Step 0.
- **Strict side profile facing right.** The body, hips and feet point in the direction of travel; this is not a three-quarter view.
- Frames in **a single horizontal row**, equally spaced, all at the same scale; the feet on the same ground line in every frame (except jumps).
- The whole body is visible in every frame, and frames don't overlap.
- **Flat background in the single colour #47304C.** No ground shadow, text, frame lines or grid.
- The same colours, outfit details and proportions as the reference; no new items.

## Prompt template (in English; models follow it more reliably)
```
Pixel art sprite sheet of the SAME character as the attached reference image: [DESCRIPTION].
[N] frames of a [ACTION] animation, arranged in ONE horizontal row, evenly spaced, identical scale,
STRICT SIDE PROFILE view facing right (side-scrolling game sprite, body/hips/feet pointing right), full body visible in every frame,
feet on the same ground line, frames must not overlap. Flat solid background color #47304C,
no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.
```

## Character descriptions (fill in [DESCRIPTION])
- **Karamani:** Ottoman akıncı commander in his 40s; fair skin, almond eyes, chestnut beard and moustache; pointed steel helmet with gold band and red plume, mail aventail; red-and-gold kaftan over mail armour, wide red sash, red cape; curved kilij sword in right hand.
- **Ludwig:** young German knight, 21; blond wavy hair, clean-shaven; black full plate armour, blue tabard with a red emblem, blue cape with gold embroidery; long straight sword.
- **Álmos:** elegant pale vampire nobleman, looks 30; black hair in a ponytail, red eyes; long black coat with gold trim, white jabot, red sash, black trousers and boots.
- **Kutluca:** 12–13 year old Turkic noble boy; fair skin, almond eyes, chestnut hair; blue cap, blue tunic with gold trim, red scarf, satchel; wooden staff with a spiral top. (A child: brave, determined, never sexualised.)
- **Kısmet:** clever goblin; green skin, long pointed ears, earring; brass helmet, red scarf, cream shirt, brown leather vest with straps, dark trousers, boots; short curved dagger.

## Animation list (order = importance; frame counts match ASSET_CONTRACTS §5)
| Animation | Frames | [ACTION] text |
| --- | --- | --- |
| run | 8 | running cycle, loopable, strong forward lean |
| walk | 8 | calm walking cycle, loopable |
| light attack 1 | 7 | horizontal sword slash: anticipation, swing with motion smear, follow-through, recover |
| light attack 2 | 7 | rising diagonal slash, then recover |
| light attack 3 | 7 | forward thrust finisher, then recover |
| heavy attack | 10 | two-handed overhead heavy strike: big wind-up, powerful downward slash, recover |
| block / parry | 2 + 2 | holding a defensive guard / deflecting a blow |
| roll | 6 | forward combat roll |
| jump | 6 | takeoff, rising, apex, falling, landing crouch |
| hurt | 3 | recoiling from a hit |
| death | 5 | collapsing to the ground |
| (Karamani) shoot | 5 | aiming and firing a flintlock pistol |

Order: Karamani's **profile base pose (Step 0)**, then **walk**, **run** and **light attack 1**. Once the profile pose exists, I'll redo the idle (breathing, cape sway) from that image too. The current idle and walk were made from the three-quarter pose, so they are only previews.

## Conversion
```
python -I Tools/sprite_from_art.py <sheet.png> --grid karamani-run:8 --height 256 --colors 96 --no-outline --grade-only --scale 1
```
Note: with `--grid`, the scale comes from the **tallest frame** in the sheet. That frame is taken as the character's standing height (Karamani 258, Ludwig 244, Álmos 248, Kutluca 200, Kısmet 220 px), so the size stays the same across sheets. Give `--height` that character's value.
