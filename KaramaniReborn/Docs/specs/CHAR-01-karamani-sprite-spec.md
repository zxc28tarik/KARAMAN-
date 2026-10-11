# CHAR-01: Karamani sprite quality spec (Gate A)

Author: art-director, 2026-10-11. Status: for integrator approval. Scope: hero sprite construction and key poses only. Pipeline, palettes, framing and environment stay as they are. Frame format (128×128, pivot 64,40) is unchanged. **Locks touched: none.**

**Evidence opened as images:** `scratchpad/hero-6x.png`, `scratchpad/ingame-crop.png`, `Review/characters/ingame-lineup-idle.png`, `Review/chapter-1.png`, and my emulator output `scratchpad/char01-current-quantised-3x.png`. **Read:** baseline review findings 4–7, `pixel_characters.py`, `PixelPipeline.cs`, `PixelPost.shader`, `PaintedSprite.shader`.

**Measurements:** scratchpad scripts `char01_metrics.py`, `char01_quantframe.py`, `char01_ramp_solver.py`, `char01_eval.py` emulate the `PixelPost` grade and OKLab nearest match (chroma 1.1, accent bias), with Bayer ±0.0075 and vignette 0.96. The emulator's zone-0 output matches `ingame-crop.png` (violet hero, purple cape).

## 1. Diagnosis: why it reads "clean mobile/indie"
Values are for idle frame 0 unless noted.

| # | What you see | Cause in `pixel_characters.py` | Measured |
| --- | --- | --- | --- |
| 1 | Tube limbs, pillow-shaded ellipses | Each primitive is shaded on its own (`t = 1+(λ>-.12)+(λ>.5)+(λ>.86)`). There is no depth, AO or cast shadow, so forms never shade each other | The same 4-band pattern on every limb |
| 2 | Flat torso with stripes | `pat_lamellar` darkens every 3rd *world* row and ignores curvature, lean and plates | Rows stay world-horizontal at any lean (18° in light1) |
| 3 | Cartoon outline | Every exterior pixel is ramp 0, and every front/back group contact is tone 0 | 100 % of outline px are tone 0; 60–72 % of key-side edge px have lum < .1 |
| 4 | Face blob | An 8×9 skin ellipse sits under the turban (11×7) and beard (7×5) ellipses; there is no eye | 7–10 skin px |
| 5 | Lumpy turban | One ellipse plus diagonal `pat_wrap` stripes; red appears only on the tail | No crown, brim or pin structure |
| 6 | Flat cape | One `band`; folds are `sin(…)>.55` stripes and the hem is a sine cut. The same pattern is on the skirt, sleeves and thighs | Folds never change the silhouette |
| 7 | Noise | `pat_mail` is a 1-px checker | Orphans 15–19 %; px in clusters ≤ 2: 30–34 %; 39–42 colours per frame |
| 8 | Identity lost in game | Ramps are authored in natural colour, but the grade multiplies chroma by `grade.y` (.48–.55) before matching | **Zone 0:** crimson t3–4 → main6/8 (violet) and all skin → main ramp. **Zones 2–3:** crimson t3–4 and bronze → skin slots (brown cape). 26 % of quantised px land in pure-black slots |
| 9 | Short legs | Skirt hem at y ≈ 18 | Visible leg ≈ 19–20 px |
| 10 | Straight sword | The blade bend `2.6·s²` gives 0.65 px of sagitta against the chord | Reads as straight |

Verdict: a sound skeleton renderer with a cartoon shader. Replace the shading, head and ramps; keep the skeleton and animation data.

## 2. Target rendering model
### 2.1 References (studied, never traced)
| Reference | How the look is made (observable) | Take | Don't take |
| --- | --- | --- | --- |
| The Last Faith | Low-key figures: most pixels in each material's 2 darkest tones; light only on key-facing planes and thin rims; 1–3-px metal glints; outlines break on lit edges | Value distribution (≤ 15 % of the figure in tones 3–4), lit-edge breaks, few glints | Designs, palettes, frames |
| Blasphemous | Frame-by-frame hand pixel art; hue-shifted ramps; ornament as 2–4-px clusters; faces from a few key-lit pixels; large 1-frame smears | Cluster discipline, face economy, smear frames | Iconography, poses |
| Dead Cells | 3D-modelled and animated characters rendered small without anti-aliasing, plus a normal pass for in-game light: identical volumes in every frame | **The method.** Our generator is already a small 3D renderer (skeleton → primitives with normals). Make it a G-buffer renderer, then add hand-pixel cleanup passes | Cel-cartoon colour, low-detail faces |

### 2.2 Pipeline (`rasterize` and helpers)
- **R1 G-buffer per 1× pixel:** material, normal, depth z (+ toward camera), form id, flags (keep, glint, smear). Composite by z, not list order. Depths: cape −8, far limbs −6, torso 0, near limbs +4, sword arm +6.
- **R2 Joints:** blend normals of connected forms within 1.5 px of the joint (smooth-min, k = 1.5). No tube ends at elbows, knees or neck.
- **R3 Diffuse:** d = N·L, L = (.38, .62, .68); wrap d′ = (d + .25) / 1.25.
- **R4 Tones:** tone = 1 + (d′ > .20) + (d′ > .55), +1 where d′ > .85 **and** metal **and** (N·H)¹⁶ > .5. Cloth, skin and beard stop at tone 3, except cloth sheen (tone 4) on key-facing fold crests, ≤ 4 % of cloth px.
- **R5 AO:** −1 tone (min 1) within 1 px of a nearer form (Δz ≥ 3); 2 px under the turban brim, beard, pauldron, mantle hem and sash.
- **R6 Cast shadow:** shift the turban, beard, pauldron, near-arm and sash masks 1 px left and 2 px down; where they land on a farther form, −1 tone.
- **R7 Cool back-rim:** R = norm(−.75, .45, −.48). Exterior-edge px with N·R > .35 on the turban back, shoulders, upper back, cape top edge and thigh backs take the rim colour (§5.1). Runs of 2–5 px, ≤ 20 % of the back-edge length, never on face or hands.
- **R8 Cleanup** (after shading, before outline): (a) a pixel with no same-colour 4-neighbour takes its 8-neighbourhood majority unless flagged *keep*; (b) remove nubs and L-corners from tone boundaries; (c) anti-pillow: two tone boundaries parallel 1 px apart for > 3 px → merge the middle band into the darker side.
- **R9 Sidecar:** `Review/characters/hero-materials.json` maps hex → (material, tone) and lists per-frame landmarks, guard, tip and smear px, so criteria are measured, not guessed.

### 2.3 Outline and interior separation
- 1 px, outside the silhouette, 4-connected. Edges facing away from the key (outward normal · (.52, .85) ≤ .25: back, underside) take tone 0 of the adjacent material; key-facing edges take tone 1.
- **Lit-edge breaks:** where the inner pixel is tone ≥ 3 and faces the key, omit the outline for 2–6-px runs (turban top, pauldron top, plate tops, blade edge), ≤ 15 % of the key-side edge.
- Interior separation is the AO band (R5) on the farther form at tone 1, never tone 0. Tone 0 inside the silhouette only in crevices: armpit, between the legs, under the cape hem, beard/collar junction, eye.

### 2.4 Clusters and dithering
- Every same-colour 4-connected cluster is ≥ 2 px.
- Intentional single pixels are flagged *keep*: the eye, pin glint, blade tip, and 1 glint per brass object. At most 8 per frame.
- Tone-4 clusters are 2–6 px and cover ≤ 10 % of their material.
- Long lines (blade, cape edge, sash) use monotonic run lengths (3-3-2-2-1, never 3-1-3).
- The sprite carries no dither. The post pass adds world-fixed Bayer ±0.0075, and every source colour must keep its slot under that jitter (C12).
  - Only exception: a 2-px crimson1/crimson2 checker on cape tone transitions longer than 6 px.

## 3. Proportions (idle; y = rows above the pivot, outline included)
| Landmark | y |
| --- | --- |
| Sole outline / turban-top outline | −1 / 59 (61 rows, locked) |
| Ankle / knee / crotch / hip joint | 3 / 17 / 27 / 30 (skeleton unchanged) |
| Sash / chest line / shoulder joint / trapezius top | 33–35 / 41 / 45 / 47 |
| Chin / eye / turban lower edge | 49 / 53 / 55 |

**Head count:** the skull runs from the crown (y 57, inside the turban) to the chin (y 49), 9 px. That gives 61 / 9 = **6.8 heads**, with the turban adding 2 rows above the crown.

**Profile widths (px, without outline):** head back-to-nose 9; turban 10 + 1-px brim; neck 5 (hidden by beard and aventail); chest depth 11 at y 41; waist 9 at y 34; pelvis 10; tasset flare 12 at y 25. Upper arm 10 long, 5 → 4 thick; forearm 9, 4 → 3; fist 3×3 + 1-px thumb. Thigh 15, 6 → 5; knee 4 with a 4×4 bronze cop; shin 14, calf 5 → ankle 3; boot 8 × 4, toe up 1 px. Current diameters shrink: thigh 8.2 → 6.8, shin 6 → 4.8, upper arm 5.6 → 5.

**Silhouette rules (40s veteran):**
- V-taper of only 2 px: solid, not a superhero wedge. Head 1–2 px forward of the shoulder line; shoulders rolled 1 px forward.
- Knees bent ≥ 8° in every grounded frame; feet 14–16 px apart in idle.
- Tasset hem y ≥ 23 front, ≥ 21 back; the kaftan skirt shows only behind the far leg (hem y ≥ 16), so the near leg reads 24 rows.
- Idle negative space: ≥ 2 px between sword arm and torso, a leg gap ≥ 6 px, no tangents.
- **Fill test:** in solid black, the turban dome and tail, beard point, curved blade, cape and bent knees all read.

## 4. Head and face (1×, idle, facing right)
```
       c: 0123456789ABCD
r0  y59   ......OrRo....
r1  y58   ....O1rrRR3o..
r2  y57   ..O122333443o.
r3  y56   .O11112233bBo.
r4  y55   .O11122233bbo.
r5  y54   ..OmmsssssSo..
r6  y53   ..OmmSsSeKKo..
r7  y52   ..OmmSSSKKKKo.
r8  y51   ..OmmmSSsmmsO.
r9  y50   ..OmmmmSmMMmmO
r10 y49   ..OmmmmmmMmmO.
r11 y48   ...OnmmmMmmO..
r12 y47   ...OnNnmmmO...
```
**Legend:**

| Symbol | Meaning |
| --- | --- |
| `.` | empty |
| `O` | outline, tone 0 |
| `o` | key-side outline, tone 1 |
| `1`–`4` | ivory tones 1–4 |
| `r` / `R` | crimson tone 1 / tone 3 |
| `b` / `B` | bronze tone 2 / tone 4 |
| `s` / `S` / `K` | skin tones 1 / 2 / 3 |
| `e` | eye: beard tone 0, flagged *keep* |
| `m` / `M` | beard tone 1 / tone 3 |
| `n` / `N` | mail tone 1 / tone 3 |

- **Turban**, 5 rows (y 55–59): red crown 6 px (4 × 2; crimson1 back half, crimson3 front); two wrap bands rising 1 px toward the front, the groove (ivory t1) on the back half only, ending 2 px before the lit area; 2×2 brass pin in front with 1 glint; brim overhangs the brow by 1 px. Tail: crimson, 2 px wide, 6–8 px long, leaving the back at y 55–56, lagging the head by 1 frame.
- **Face:** a skin1 brim-AO row across the forehead; the eye is 1 px at tone 0 directly under it, 2 px behind the face plane; the key-lit cheek–nose plane is a skin3 cluster ≥ 6 px; the nose projects 1 px, with a skin1 nostril underside; ear 3 px.
- **Hair:** moustache 2 rows, drooping 1 px past the lip; beard 3 rows below the chin, ending in a 2-px point; two grey streaks (beard t3, 2–3 px) for the 40s salt-and-pepper.
- **Counts:** 23 skin px in the mock (minimum 20), 3 skin tones; turban top to chin 11 rows. The head block moves as one unit and the face pattern never re-shuffles between frames (C14).

## 5. Materials
### 5.1 Export ramps (verified in all 4 zones)
The sheet stores these pre-compensated hexes; the zone grade turns them into the listed slots. They replace the hero's ramps via `ramps_for('hero')` (other variants out of scope). Every hex is unique so the sidecar can decode it. On grey they look oversaturated: judge only the zone previews. Do not change `_CharacterVivid` or any palette.

| Material | t0 | t1 | t2 | t3 | t4 | Slots |
| --- | --- | --- | --- | --- | --- | --- |
| Crimson cloth | 360f0f | 5d1824 | 962439 | c9393f | dc8751 (sheen) | crimson0–3, skin3. t0 may jitter to main0 |
| Skin | 512118 | 7e3924 | b46039 | db8751 | – | skin0–3 (t0 may jitter to main2). No source hits skin4 in all zones, so it is not used |
| Bronze / brass | 2a1f10 | 7b5700 | a58106 | d5a803 | f9e47b | main0–1, brass1–4 |
| Ivory turban | 3a332c | 6e6556 | 9a907c | c4b89c | ece2c8 | z0: m3, m7, m9, skin4, fire5 · z1: m3, 6, 9, 10, 11 · z2: m2, 6, 8, 10, 11 · z3: m2, 6, 8, 9, 11 |
| Steel | 101218 | 272b33 | 3f4550 | 646b78 | 98a0ad | m0, 2, 4, 6–7, 8–10 |
| Mail | 0e0f13 | 1e2127 | 343840 | 4a4f59 | 646a75 | m0, 1, 3, 4–5, 6–7 |
| Beard / hair | 0c0a0a | 1a1615 | 2a2321 | 3e3532 | 5a504b | m0, 0, 1, 3, 4–5 |
| Leather | 1a1411 | 2e231d | 46362c | 624c3d | b16c33 | m0, 1, 3, skin1 or m4–5, skin2. Wood slots are unreachable in zone 1 |
| Blade | 1c2026 | 4a515c | 858e9b | c3cad3 | f4f6f8 | m1, 4–5, 8–9, 9–11, fire5 or m11 |
| Rim | 7d8499 | | | | | m8 (z0–1), m7 (z2–3) |

### 5.2 Construction (% = share of idle figure px)
**Bronze lamellar cuirass** (per the lock), 14–18 %
- 5 rows from sash to chest, each 3 px tall: a 2-px plate face plus a 1-px crimson1 lacing row.
- Seams every 3 px at tone −1, staggered 1–2 px per row, only where tone ≥ 2. Each row's top line is +1 tone on the key half (bevel).
- Rows tilt with the lean and bow 1 px down toward the front. At most 3 two-pixel glints.

**Mail** (aventail, upper-arm sleeves, under the tassets), 8–12 %: 2-px rows alternating t / t−1; light rows broken into 2-px dashes, offset 1 px every other row; t3 only on the key half; no t4, no checker.

**Steel** (bracers, greaves), 8–10 %: a 2-px t3 band on the key side, one 2–3-px t4 dash, a 1-px t1 rolled edge.

**Crimson cloth** (3-px sash, kaftan skirt behind the far leg, 2-px sleeve cuffs): folds are normal ridges radiating from the belt knot and hip, each a 1–2-px t3 crest and a 2-px t1 valley; t0 hem underside on the shadow side; the hem zig-zags 1 px at each fold.

**Cape: the identity block** (crimson ≥ 30 % of the figure in total)
- Two layers: a shoulder mantle 6–8 px deep whose hem casts AO onto the main cape; the main cape runs from the shoulder blades to y ≈ 14, 12–16 px wide at the hem in idle.
- 4 fold ridges in idle, 3–5 in motion. Hem: ≤ 3 tears, 1–2 px deep, no sine saw; ≥ 6 px of lining (t0–1) where the hem turns.
- Motion: idle sway ±1 px. Run: 55–75° from vertical, 2–3 travelling 2-px waves, hem lags the hips by 2 frames. Attacks: lags 1–2 frames, flares on the frame after impact, settles over 3.

**Leather** (gloves, boots, straps), 6–9 %
- A 1-px t3 rim on the boot top and toe; a t0 sole; a t3 knuckle row on the key side.

**Brass trim** (pin, pauldron rim, buckle, knee cops, guard, pommel), 2–4 %
- Uses the bronze ramp. At most 2 glints per object, placed on convex key-facing points.

## 6. Kılıç
- **Length:** guard-to-tip chord 24 px (now 23.1 from the hand). **Grip:** 4 px of crimson cord, alternating t1/t2 wraps. **Pommel:** 2-px brass cap angled 30° toward the edge side.
- **Guard (balçak):** 5 px across, 1 px thick, brass, plus a 2-px langet along the blade.
- **Curve:** sagitta 3 ± 0.5 px from the chord, peaking at 60–70 % of the length; cutting edge on the convex side.
- **Width:** 2 px for the first 17 px; the yelman is the last 7 px at 3 px, with a 1-px step on the back edge; tip clipped to 1 px.
- **Shading:** edge row t3 with a 1-px t4 highlight line in ≥ 3-px dashes on the key-facing half; spine t1–2. Pixel-perfect lines, no L-corners.

## 7. Key poses (animator; ASSET_CONTRACTS §5 timing unchanged)
| Clip | Requirement |
| --- | --- |
| idle (6) | 60 % of the weight on the back leg. Shoulders counter-rotated. Sword low and forward at 35–40°, edge down. Chest +1 px on frames 2–4; the head follows 1 frame later |
| run (8) | Contact f0/f4, down f1/f5 (lowest, −2 px), passing f2/f6, up f3/f7 (highest, both feet off the ground). The current cycle is highest at contact and samples no flight frame; shift the bob and stance phases. Lean 12–15°. Sword trails low behind. Feet stride-locked (O8) |
| light1–3 | Anticipation 1–2 frames: blade cocked, torso back 8–10°. **Active frame = smear:** a crescent from the previous blade angle, 3–4 px wide, t3 body with a t4 leading edge in 2–4-px dashes, tapering to 1 px; the real blade is drawn at the arc's end. Versus `ready`: front foot ≥ +5 px, hip ≥ +3 px forward and ≥ 2 px lower, back heel lifted (toe angle ≥ 15°); lean +10°. 1-frame impact hold. light2: rising backhand with the opposite twist. light3: lunge, step ≥ 6 px, back leg straight |
| heavy (10) | 3 wind-up frames, two-handed, tip behind the back, lean −8 to −10°. Smear of 150°, 5–6 px wide. At impact the hip is ≥ 7 px below `ready`, the front foot ≥ +8 px and the front knee ≤ 100°; the cape flares. 2-frame hold |
| parry (2) | Blade diagonal in front of the face, edge out, guard at chin height, ≥ 2 px gap between blade and face. 70 % of the weight back. Frame 2: blade knocked 15° back |

Every attack key frame has one continuous line of action from the back foot through the spine to the head. The spine is never straight-vertical in an active frame.

## 8. Objective acceptance criteria
Measured on the sheet plus the sidecar with a new `Tools/char_metrics.py` (the integrator assigns its owner; the scratchpad scripts are reference implementations).
- Unless stated, all frames are measured.
- **Zone preview** means the emulated grade plus OKLab nearest-colour match, without vignette.

| ID | Metric | Threshold | Baseline |
| --- | --- | --- | --- |
| C1 | Idle height in rows, outline included | 61–62 | 61 |
| C2 | Idle opaque area (px) | 1150–1400 | 1223 |
| C3 | Orphan share: opaque px with no same-colour 4-neighbour, *keep* excluded | ≤ 0.04 | 0.15–0.19 |
| C4 | Share of px in same-colour clusters ≤ 2 px | ≤ 0.10 | 0.30–0.35 |
| C5 | Source colours per frame | ≤ 40 | 39–42 |
| C6 | Tone-0 share of edge px (opaque px with a transparent 4-neighbour). Key-facing = the transparent neighbour is right or above; back/bottom = all other edge px | ≤ 0.25 / ≥ 0.60 | 1.00 / 1.00 |
| C7 | Interior tone-0 px as a share of interior px | ≤ 0.04 | 0.06–0.09 (lum < .1) |
| C8 | Idle head: skin px; skin tones; eye px | ≥ 20; ≥ 3; 1–2 | 8; –; 0 |
| C9 | Crown crimson px. In every zone preview, ivory mean lum ≥ 0.45 and above every other material except the blade | ≥ 6; as stated | 0 |
| C10 | Tones present in idle | crimson, bronze, ivory, steel ≥ 4; mail, skin, leather, blade ≥ 3 | – |
| C11 | Largest connected crimson-slot region in idle, every zone preview | ≥ 120 px and bbox ≥ 8×14 | all crimson-slot px: 150 (z0, z2, z3), 234 (z1); region not measured |
| C12a | Accent-material px on their declared slot, at nominal | 100 % | z0: crimson → main |
| C12b | Same px under Bayer ±0.0075 and vignette 0.96: share that changes slot / share that leaves the allowed set | ≤ 2 % / 0 | – |
| C12c | Main-ramp materials: per zone, tone slots strictly increasing; steel t1–t4 index gaps | increasing; gaps ≥ 2 | – |
| C13 | Zone previews: figure median lum; P95−P5; share in main0 + crimson0; slots per frame | 0.14–0.26; ≥ 0.45; ≤ 0.15; 18–28 | 0.18–0.23; 0.57–0.76; 0.26; 16–24 |
| C14 | Per-material px count across the 6 idle frames; head-block diff between frames after bob alignment | within ±10 %; ≤ 4 px | – |
| C15 | Sidecar landmarks: chin; crotch; front tasset hem | 49 ± 1; 27 ± 1; ≥ 23 | hem ≈ 18 |
| C16 | Kılıç: chord; sagitta, at 55–75 % of the length; yelman width | 24 ± 1; 2.5–3.5; 3 px | 0.65 px sagitta |
| C17 | Pose params from the sidecar vs `ready`. light1 active: front foot Δx; hip Δx; hip Δy; back toe angle. heavy impact: hip Δy; front foot Δx. Smear px per attack | ≥ +5; ≥ +3; ≤ −2; ≥ 15°. ≤ −7; ≥ +8. ≥ 30 in one frame | +4; +2; −2; 0°. −6.5; +7. no smear |
| C18 | O6, O8, O9 and O11 still pass (unity-tester) | existing | PASS |

Judged at Gate E: J1 (fill test), J5 (bronze, mail, steel, cloth and leather separate at 1×), J6, J7, J8.

## 9. The 8 highest-impact changes (ranked)
1. Zone-robust export ramps (§5.1): the crimson cape, the face and the bronze survive all four zones.
2. Rebuild the head to the §4 mock.
3. Selective outline with AO-based interior separation (§2.3).
4. G-buffer shading: depth compositing, AO, cast shadow and back-rim (R1–R7).
5. Materials as geometry: lamellar rows with red lacing, 2-px mail rows, folds as ridges (§5.2).
6. Cleanup passes and cluster rules (R8, §2.4).
7. Proportions and silhouette: shorter tassets, slimmer limbs, a two-layer cape block (§3, §5.2).
8. The kılıç shape, plus attack commitment with smear frames (§6, §7).

## 10. Owners, requests and open points
- **character-artist:** items 1–7 and the blade shape. Deliverables: the sidecar and `Review/characters/hero-zone{0..3}.png` (idle, run2, light1 active, heavy impact; 6× nearest over each zone's main3). Construction half of `pixel_characters.py`, under the TASK_BOARD lock.
- **animator:** §7, after construction lands (slimmer limbs and the shorter skirt change the poses).
- **integrator:** assign the owner of `char_metrics.py`; decide whether the emulator scripts move into `Tools/`.
- **vfx-specialist** (measure only): the runtime warm rim (`_RimColor`) and cool `_MoonStrength` top rim stack on the baked light. At Gate D, report whether key-side px change slot and whether `BladeTrail` doubles the baked smear.
- **Open (integrator):** the brief says "steel lamellar"; the lock says bronze. This spec follows the lock (bronze cuirass; steel bracers, greaves, blade). The captain's gold-bronze then collides with the hero, so guards and captain need their own spec. Proposal: guards in grey mail with ivory hoods, captain in blackened steel with gold trim.
