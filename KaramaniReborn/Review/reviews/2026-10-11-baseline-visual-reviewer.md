# baseline review: visual-reviewer, 2026-10-11

Task T-002 (baseline, Gate E, no slice spec). Independent review: no other agent's review of this evidence was read.

Evidence examined (frozen copies in `Review/snapshots/baseline-E-20261011-0006/`, byte-identical `cp -p` copies; SHA-256 below):
| File | Source | Captured | Opened as image |
| --- | --- | --- | --- |
| `player-chapter-1.png` (Kızıl Surlar) | EXE capture, 1440×1080, HUD + vignette | 2026-10-08 20:15 | yes |
| `player-chapter-2.png` (Yanmış Mahalle) | EXE capture | 2026-10-08 20:15 | yes |
| `player-chapter-3.png` (Kurt Geçidi) | EXE capture | 2026-10-08 20:15 | yes |
| `player-chapter-4.png` (Karanlık Kale) | EXE capture | 2026-10-08 20:15 | yes |
| `hero-sheet.png` | generator preview (`pixel_characters.py`), 4096×2048 = 2× of the 128-px-cell atlas, 115 frames | 2026-10-08 20:36 | yes (the viewer downscaled it to 2000×1000; pixel-level judgement uses my native crops below) |
| `hero-zoom.png` | generator-style preview, 2400×450, 6 frames | 2026-10-10 23:51 | yes (viewer downscaled to 2000×375) |

Crops I made from those files with nearest-neighbour scaling, all in the snapshot folder and all opened: `zoom-ch1-hero-grid.png`, `zoom-ch4-hero-grid.png` (3×, 20-px coordinate grid), `zoom-ch4-hero-mask.png` (silhouette mask overlay), `zoom-ch1-hero-4x.png`, `zoom-ch1-guard-4x.png`, `zoom-ch2-hero-3x.png`, `zoom-ch3-undercroft-2x.png`, `zoom-ch1-brazier-banner-3x.png`, `compare-ingame-vs-pixelhero-samescale.png`, `zoom-pixelhero-idle0-8x.png`, `zoom-pixelhero-head-24x.png`, `zoom-pixelhero-idle-3x.png`, `zoom-pixelhero-light1-3x.png` and `zoom-pixelhero-heavy-2x.png` (the viewer slightly downscaled the last two, by 1.05× and 1.10×). `zoom-ch2-hero-grid.png` and `zoom-ch3-hero-grid.png` were also generated but **not opened**.

Text read for provenance only (not judged): `Review/player-verification.txt` and `Review/player.log` (EXE run of 2026-10-08), file timestamps, and `Assets/Resources/Characters/hero.json` (clip and frame layout).

Freshness: the EXE captures are about 52 h old. `Build/KaramaniReborn_Data/Managed/Assembly-CSharp.dll` was built at 20:14:44, after the last runtime script change (`PixelPipeline.cs` at 20:13). `find Assets Tools -newer player-chapter-4.png` lists only `Characters/hero.*` and `Tools/*`. **So the captures still show the current in-game look.** Provenance of `hero-zoom.png` is unclear: it is 2 days newer than `hero-sheet.png` and `hero.png`, and the generator was not re-run. Its content matches the sheet.

SHA-256: player-chapter-1 `e6468747…`, -2 `32e24410…`, -3 `50c4c66c…`, -4 `e1238b27…`, hero-sheet `a1df8372…`, hero-zoom `8846f2b1…` (full hashes are in the snapshot folder via `sha256sum`).

Evidence NOT available:
- No video or in-game motion of any kind. The pixel hero's motion is judged only from its generator frame sheet, and the painted rig's motion is not judged at all (`Review/animation/*` and `Review/poses/*` were excluded).
- No HUD-free capture, so O2 has no baseline.
- No raw 360×270 buffers (`Review/pixel/*` excluded), so O3 was not measured.
- No per-actor bounding boxes or masks from the capture.
- No in-game attack or hit frame.
- The pixel hero has never been rendered in-game, quantised to a zone palette or shown over a zone backdrop. Its look under `PixelLook` is unknown.
- No previous accepted snapshot to compare against (this is the first).
- No slice visual spec. `Docs/specs/VS-01-proposal.md` exists but was deliberately not read, to keep this review independent; it is not a visual spec.

## Objective results
Snapshot dir = `Review/snapshots/baseline-E-20261011-0006`. Official numbers are in `metrics.json`. Supplementary numbers (my own method, not gate criteria) are in `metrics-silhouette-supplementary.json` and `metrics-pixelhero.json`.

| ID | Value | Threshold | PASS/FAIL | Command |
| --- | --- | --- | --- | --- |
| O1 | contentAspect 1.3333 in all 4; no letterbox (native 4:3 render) | 1.3333 ± 0.002 | PASS (black side margins on non-4:3 windows can't be tested from these captures) | `python -I Tools/frame_metrics.py <4 captures> --box 452,481,82,245 --json` → exit 0 |
| O2 | flatBlockShare, full frame: .8071 / .8765 / .8453 / .7580. HUD-free band y 200–920: exact-flat .873 / .914 / .889 / .822, and **100 % of 4×4 blocks are flat within ±2 levels**: every non-flat block has a max channel range ≤ 2, which is the vignette, not content. Grid phase (0,0) | ≤ 0.02 drop vs. last accepted | Not gradable (no accepted HUD-free baseline). Structurally: **no mixels** | frame_metrics + inline numpy block-range scan (method in this report) |
| O3 | not measured | ≤ 37 colours | NOT MEASURED | raw buffers excluded by task |
| O4 | mean luminance .1606 / **.0886** / .1186 / .2385 | 0.10–0.26 | ch1 PASS, **ch2 FAIL**, ch3 PASS, ch4 PASS | frame_metrics |
| O5 | box `452,481,82,245`: ch1 Weber .036 / rgbDist .0373; ch2 .311 / .0672; ch3 .078 / .061; ch4 −.183 / .1173 | \|Weber\| ≥ .30 or rgbDist ≥ .12 | **ch1 FAIL, ch2 PASS, ch3 FAIL, ch4 FAIL** (rgbDist misses by .0027). Doc baseline box `450,480,90,240` on ch1 reproduces .037 / .0348 | frame_metrics `--box` |
| O5-supp | silhouette mask vs. 8-px ring (floor excluded): Weber −.639 / −.554 / −.578 / −.709; rgbDist .375 / .225 / .280 / .505 | same | would PASS in all 4. **Supplementary, not the gate metric** (see finding 3) | scratchpad `silhouette.py` → `metrics-silhouette-supplementary.json` |
| O6 | hero box height 245 px = **22.7 %** of view (≈ 61 game px); pixel hero idle = 61 game px | 20–26 % | PASS (supplementary; the official `BuildTools.Verify` line was not opened) | box derivation below |
| O7, O8, O9, O11 | - | - | NOT VERIFIED (outside this evidence set) | - |
| O10 | `Review/player.log` (EXE run 2026-10-08): 0 lines match `exception\|error` | 0 | PASS for that run | `grep -ciE 'exception\|error' Review/player.log` → 0 |

**How the O5 box was derived.** I inspected 3× nearest-neighbour crops with a 20-px grid (`zoom-ch1-hero-grid.png`, `zoom-ch4-hero-grid.png`):
- Top of the turban: y ≈ 481. Sole: y ≈ 725 (the floor line starts at y ≈ 727).
- Back of the cape: x ≈ 452. Front of the chest and front knee: x ≈ 533.
- Excluded: the sword arm and blade beyond x 533. The barrel starts at x ≈ 523 below y ≈ 615 and overlaps the front shin.

A dark-pixel extent scan (lum < 0.55 × the local background median) gave rows 480–723/725 and columns 452–539/543; it is wider because it catches the forearm and hilt. The hero sits on identical screen pixels in all four captures (the camera is clamped at the zone start), so one box serves all four.

## Findings (most severe first)
1. **[Critical] [Objective + Judged]** `player-chapter-1/3/4.png`, hero box `452,481,82,245` (all four chapters judged): **O5 FAILs in 3 of 4 zones, and the in-game painted-rig hero renders as a near-black cut-out.**
   - **Measured** (`metrics-pixelhero.json`, `metrics-silhouette-supplementary.json`):
     - Figure median luminance is **0.053 in every zone**.
     - Only 14–21 colours on the figure.
     - **28–35 % of figure pixels are isolated single pixels.**
   - **Judged** (`zoom-ch1-hero-4x.png`):
     - The turban, which is ivory and red in the HUD portrait painted from the same character, renders dark. The head reads as a narrow hood or helmet.
     - There is no crimson cape block.
     - Lamellar, mail, cloth and leather are indistinguishable: a black mass with pink and tan speckles. This is the "AI texture soup" that ART_DIRECTION 1.1 forbids.
     - The key-side rim is a broken 1-px chain, not a rim.
     - He reads as "a dark man with a sword", not as Karamani.
   - **Correction:**
     - (a) Don't spend effort polishing the rig. Make pixel-hero integration the path (fix findings 5–7 first).
     - (b) First find out why actors come out at a 0.05 median, so the pixel hero doesn't inherit it. **Likely cause (unverified):** the actor pass goes through the fog / darkening multipliers and the zone main-ramp darks in `PixelPipeline` / `LightField`.
     - Targets for any actor: interior median luminance ≥ 0.15; turban pixels ≥ 0.45; a continuous 1-game-px rim on the key (right/top) side; isolated-pixel share ≤ 0.15.
   - **Owners:** vfx-specialist (actor treatment in `PixelPipeline`; palette changes need art-director sign-off), character-artist (sprite).
2. **[Critical] [Objective]** `player-chapter-2.png`, full frame: **O4 FAIL, mean luminance 0.0886** (p50 0.074; threshold ≥ 0.10).
   - **Judged:** Yanmış Mahalle reads as uniform murk. The fire accents (windows at x 90–130, y 410–470 and x 500–560, y 300–330) are too small to anchor the key.
   - **Correction:** lift the zone mean by ≥ +0.03 (target 0.12–0.14). Raise the fog band behind the playfield (y ≈ 380–650) by about 2 ramp steps and the vista mid-tones by 1 step. Add 2–3 larger distant fire glows on the warm accent ramp. Re-measure O4 and keep ch2 ≤ 0.26.
   - **Owners:** environment-artist; vfx-specialist for the `PixelLook` ramp (art-director sign-off).
3. **[Major] [Objective]** All four captures: **the O5 metric doesn't track readability.**
   - Chapter 2 is visually the darkest scene, yet it is the only one that passes.
   - The rectangle is about 50 % background, and the ring (pad 122 px) includes the dark floor, the barrel and the brazier. A dark figure on a mid background therefore averages out.
   - With a silhouette mask, all four zones separate strongly in value (Weber −0.55 to −0.71).
   - So O5 currently can't be trusted in either direction. Finding 1 stands on the judged identity loss, not on the O5 number alone.
   - **Correction:** gameplay-engineer writes per-actor screen rects plus an alpha mask at capture time (`Review/chapter-N.boxes.json`, BUILD_AND_CAPTURE §5.3). The integrator adds a `--mask` mode to `frame_metrics.py` (figure pixels vs. an 8-px dilated ring with floor rows excluded) and re-baselines the O5 thresholds.
   - **Owners:** gameplay-engineer, integrator.
4. **[Major] [Judged]** `player-chapter-1..4.png`, hero (x 452–533) vs. guard (x ≈ 1270–1365, y 481–726), `zoom-ch1-guard-4x.png`: **the player and the guard are nearly indistinguishable.**
   - Both get the same render treatment, the same dark-maroon speckle and the same value range.
   - Objective support: in ch1 the guard's mean saturation is 0.145 vs. the hero's 0.123, so the threat is more colourful than the player.
   - Neither carries a readable identity colour block (ART_DIRECTION 1.4).
   - **Correction:** the player owns a unique crimson cape plus an ivory turban block (≥ 6×10 game px of crimson visible in idle). Guards get an ivory hood and desaturated mail, with their burgundy kept ≥ 1 ramp step darker than the player's crimson.
   - **Owner:** character-artist.
5. **[Major] [Judged]** `zoom-pixelhero-head-24x.png` (hero-sheet frame 0, rows 28–40): **the known face issue (ART_DIRECTION 1.4) is still present** in this sheet.
   - The face is a grey-brown block of about 6×5 px with a 2-px skin nose tip. There is no eye, brow, cheek or beard.
   - The turban is a lumpy beige mass. Its red appears only as the back flap.
   - **Correction:**
     - Skin-ramp profile ≥ 4×6 px (forehead, nose, cheek), with a 1-px dark eye/brow line under the turban edge.
     - Beard as a separate 2-tone dark-brown cluster under the jaw, ≥ 3 px tall.
     - Turban wraps as 2-px diagonal bands on ivory ramp 2–4, with a visible red crown of ≥ 3×3 px and the brass pin kept.
   - **Owner:** character-artist.
6. **[Major] [Judged + Objective]** `zoom-pixelhero-idle0-8x.png`, `compare-ingame-vs-pixelhero-samescale.png`: **the pixel hero's style fit and proportions don't meet the target.**
   - **Objective** (scan of frames 0, 37, 40 and 62):
     - Key-side (right) silhouette pixels are near-black (lum < 0.12) in 66–76 % of rows, vs. 92–95 % on the shadow side, so the outline is only weakly selective.
     - 40–41 colours per frame before zone quantisation.
   - **Estimated from the crop:**
     - Head ≈ 11 of 61 px, so about 5.5 heads tall (target about 7).
     - Legs from crotch to sole ≈ 20 px, about 33 % of height.
   - **Judged:** the flat mid-grey, plate-like limbs, fully saturated crimson and near-continuous black outline read as a clean indie/mobile sprite, not The Last Faith gothic. The mail doesn't read as mail.
   - **Correction:**
     - On key-facing edges, replace the outline with ramp 1–2 of the local material (target ≤ 35 % near-black on the key side).
     - Lower the steel mid-tone by one ramp step and add a 2-px offset-row mail cluster on the arms and legs.
     - Desaturate the crimson by about 15–20 %.
     - Lengthen the legs by about 4 px and trim the turban mass by about 2 px, toward 6.5–7 heads, while keeping idle height at 61–62 px (the O6 lock).
   - **Owners:** character-artist, with the animator for the limb-length pose impact.
7. **[Major] [Judged; motion from PNG frame sheet, not video]** `zoom-pixelhero-light1-3x.png` (frames 37–43) and `zoom-pixelhero-heavy-2x.png` (frames 58–67): **the attacks lack body commitment.**
   - light1 has an arm arc (back → overhead → forward → down → recover), but the lower body is the same in all 7 frames: no step-in, no hip or torso rotation.
   - heavy has a good lunge (frames 63–64).
   - Neither attack has the smear/active frame that ART_DIRECTION 1.5 asks for.
   - The blade reads as a near-straight 1-px line, not a curved kılıç (DESIGN_CONSTRAINTS: a real kılıç, not a pala).
   - **Correction:**
     - light1 active frame: front foot +3–4 px, hips down 1–2 px, torso rotated.
     - Add a 1-frame smear arc (2–3 px wide, steel ramp 3–4) on each attack's active frame.
     - Blade: a visible curve (sagitta ≥ 2 px) with a flared tip (yelman).
   - O9 hit-window sync was not measured.
   - **Owners:** animator (poses and timing), character-artist (blade shape).
8. **[Major] [Judged]** All four captures: **the zones share one identical playfield set.**
   - The banners (x ≈ 125–230 and 770–875), crates, barrels, braziers, timber walkway and rampart arches sit at the same pixel positions in every zone. Only the vista and the palette change.
   - The Ottoman crescent banners appear even in Karanlık Kale. Kurt Geçidi has no wolf statues and Karanlık Kale has no ground snow (ART_DIRECTION 1.6).
   - **Correction:** per-zone prop dressing, at least for each zone's opening screen.
   - **Caveat:** all four captures are at the zone start. The rest of each zone was not seen.
   - **Owner:** environment-artist.
9. **[Minor] [Judged]** `zoom-ch1-brazier-banner-3x.png`, `zoom-ch3-undercroft-2x.png`; all chapters at x ≈ 360–460 / 770–880 / 1110–1220, y ≈ 930–1030: **the light sources don't light anything, and the glows band.**
   - The brazier flame is about 2×4 game px and casts no warm light on the floor, the banner or the hero.
   - The undercroft lantern glows are 4–5 concentric stepped rings around a 1-px dot, with no lantern sprite and no light on the arch stone. Next to the item slots they read as targets or HUD widgets (J4, J6 banding).
   - **Correction:**
     - Brazier: flame ≥ 4×8 game px on a 3-tone fire ramp, plus a dithered warm pool (radius about 20 game px) on the floor and nearby actors.
     - Lanterns: show the lantern sprite, replace the rings with an ordered dither of ≤ 2 bands, and light the intrados.
   - **Owners:** vfx-specialist, plus environment-artist for the lantern sprite.
10. **[Minor] [Judged]** All four captures, composition and depth (J9, J3, J6):
    - The walkable line sits at y ≈ 727, 67 % of frame height. The bottom third is dark rampart facade plus HUD.
    - No foreground-silhouette plane is visible.
    - The vista's large rectangular mosaic clusters and checker-dither patches read as down-sampling artefacts (ch4 clouds x 300–1100, y 20–330; ch1 haze). This part is Polish.
    - **Correction:**
      - Propose moving the floor line to about 78–80 % of frame height: camera y ≈ +0.7…+0.9 instead of −0.4, putting the floor at about 840–860 px. This touches SotN framing, so it goes to the art-director and integrator, not a unilateral change. The 4:3 aspect and the about 23 % hero height are unaffected.
      - Add one foreground silhouette per zone at sorting 205, kept off the action line.
      - Soften the vista cluster edges.
    - **Owners:** gameplay-engineer (camera), environment-artist.

Positives, for the record:
- No mixels in the world layer (O2 scan).
- The HUD never covers the action line (J9).
- Chapter 1 has three clear value planes with good atmospheric perspective (J3).
- The kılıç reads clearly against the smoke in ch2.

## Painted rig vs. pixel hero (is the pixel hero the better basis?)
**Yes, clearly.** At the same game-pixel scale (`compare-ingame-vs-pixelhero-samescale.png`), the pixel hero is ahead on every axis. The rig problems in finding 1 are rooted in pixelating a painting, and the pixel hero avoids them by construction.

| | Painted rig, in-game | Pixel hero, generator preview |
| --- | --- | --- |
| Identity | lost (dark turban, no cape colour) | ivory turban, crimson cape, bronze and steel all read |
| Material separation | none | steel, cloth, brass and leather separate by ramp |
| Single-pixel noise | 28–35 % isolated pixels | 15–19 % |
| Idle height | 62 game px | 61 game px (contract ≈ 62) |
| Contact | - | feet sit on pivot row 88 in every sampled frame |
| Clip set | - | 25 clips, matching the ASSET_CONTRACTS §5 names |

Caveat: the pixel hero has only been seen on a neutral #3A404E background, unlit and unquantised.

Must fix before integration:
1. Findings 5, 6 and 7.
2. Find and fix the actor-darkening cause from finding 1 first, or the pixel hero will also render near-black.
3. Generator preview of the hero **quantised to each of the 4 zone palettes over a zone backdrop crop**, proving that the crimson, ivory and brass survive `PixelLook` (41 source colours vs. a 37-colour zone palette).
4. After integration: O5 measured with real masks (finding 3), O8 foot lock (< 1 game px) and O9 hit-window sync.

## Requested new evidence (for the unity-tester / owners; I did not run any jobs)
1. HUD-free EXE captures of the same 4 shots, to set the O2 baseline. gameplay-engineer adds the HUD-off variant (BUILD_AND_CAPTURE §5.2); unity-tester runs `Tools/unity_batch.ps1 -Job Build` then `VERIFY_WINDOWS.ps1`.
2. `Review/chapter-N.boxes.json` plus actor masks (finding 3). Owner: gameplay-engineer.
3. Per-zone quantised pixel-hero preview: `python -I Tools/pixel_characters.py hero` with a zone-preview option. Owner: character-artist.
4. Once the job that is currently rewriting them finishes: the raw buffers `Review/pixel/chapter-1..4.png` (for O3) and an attack sequence `Review/sequences/light1/frame-NNN.png`.

## Verdict
Gate E (baseline): **FAIL**. Blocking:
- O4 in chapter 2.
- O5 in chapters 1, 3 and 4. This is the official metric, which finding 3 shows to be unreliable; the underlying identity loss in finding 1 is real regardless.

Majors 3–8 must be fixed or waived. As a baseline, the main output is the ranked issue list above. The single highest-leverage move is: fix the pixel hero's head and style, prove it survives zone quantisation, then integrate it in place of the painted rig.
