# VS-01 proposal: "Kızıl Surlar duel" (first visual vertical slice)

Status: **proposed**, waiting for the user's approval. Not started. Task IDs are assigned in TASK_BOARD on approval.
Input: the baseline review `Review/reviews/2026-10-11-baseline-visual-reviewer.md` (Gate E baseline: FAIL; findings F1–F10 below refer to it) and the T-001 test report.

## Why this slice
The game is technically sound: 4:3 framing, a 24–30 colour palette per zone, and 20/20 editor PASS lines (re-run 2026-10-11). Visually it is not credible yet:
- **F1 (Critical):** the in-game painted-rig hero renders as a **near-black cut-out** in every zone. Interior median luminance is 0.053 (O5b fails), 28–35% of its pixels are isolated speckle, and the turban, cape and materials don't read.
- **F4:** the player and the guards look almost the same.
- The new frame-by-frame **pixel hero** (`Characters/hero.png`, 115 frames, 25 clips) is clearly the better basis. It isn't in the game yet, and its face, proportions (≈5.5 heads), outline and attack poses need work (F5–F7).

The fastest route to a commercially credible look is to make **one short moment** right end to end, rather than touching all four zones.

## Scope (deliberately small)
- **Shot area:** Kızıl Surlar (zone 0), from the first checkpoint (x≈2) to the first guard encounter (x≈12), in the sunset palette.
- **Actors:** Karamani as a pixel sprite and one Janissary duelist guard as a pixel sprite.
- **The 10-second moment:** idle → run → 3-hit combo on the guard → guard attack → timed parry → riposte execution → guard death. Hit FX, hit-stop and brazier light included.
- **Out of scope (backlog):** chapter 2 value key (F2), per-zone prop dressing (F8), other zones' lanterns (F9 outside zone 0), archer and shield enemies, captains, HUD redesign, story, the 100-Janissary opening.

## Phases, owners and gates
| Phase | Work (owner) | Files (non-overlapping) | Gate |
| --- | --- | --- | --- |
| 0a | Split `pixel_characters.py` into a construction module and an animation module, with byte-identical output (integrator) | `Tools/pixel_*.py` | - |
| 0b | **Root-cause why actors render at a 0.05 median luminance** (F1b), reported with numbers. Suspect: actor pass through fog / darkening and the main-ramp darks in `PixelPipeline` / `LightField`. Diagnosis only, no fix yet (**vfx-specialist**) | read-only + a scratch script | - |
| 1 | VS-01 visual spec (**art-director**): references; shot list; hero/guard identity blocks (F4); face, turban and proportion targets (F5, F6: ≈6.5–7 heads at 61–62 px); attack key poses and smear (F7); kılıç curve; brazier light; zone-0 value plan; whether to lower the floor line (F10 touches SotN framing, so the user decides); calibrated O5a/O5b thresholds | `Docs/specs/VS-01-visual-spec.md` | **A** |
| 2a | Hero face, turban, outline, mail, crimson and proportions; duelist guard sheet with distinct identity; **preview quantised to the zone-0 palette** (**character-artist**) | construction module, `Assets/Resources/Characters/*` | B |
| 2b | Body commitment and smear frames for light1–3, parry/riposte, hurt and dead; pose updates for the new limb lengths; O8 and O9 numbers (**animator**) | animation module | B/C |
| 2c | ~~`PixelCharacter.cs`~~ done 2026-10-11 (single-piece frames in game, all checks pass); HUD-off captures; per-actor silhouette masks and boxes; a `vs01-duel` frame sequence (**gameplay-engineer**) | `Assets/Scripts/PixelCharacter.cs`, `GameWorld.cs`, `GameRoot.cs`, `Assets/Editor/BuildTools.cs` | C |
| 2d | Fix the actor darkening found in 0b; key-side rim; hit, parry and riposte FX; brazier warm light pool and dithered glows (F9, zone 0) (**vfx-specialist**; palette changes need the art-director's sign-off) | `PixelPipeline.cs`, `LightField.cs`, `Effects.cs`, `BladeTrail.cs`, shaders | D |
| 2e | Zone-0 action line: a foreground silhouette plane (F10), less competing contrast behind actors, a bigger brazier flame sprite (**environment-artist**) | `StageArt.cs` | D |
| 3 | Compile, Verify, Build, EXE verify, captures and the sequence (**unity-tester**) | `Review/test-reports/*` | B, C, D |
| 4 | Independent reviews (**visual-reviewer**, then **art-director**) | `Review/reviews/*` | **E** |
| 5 | Fix loop on Critical and Major findings; re-capture and re-measure | as assigned | **F** |

Phases 0a and 0b run in parallel. 2a–2e also run **in parallel**, since their owned files don't overlap. Only the unity-tester runs Unity.

## Exit criteria (VS-01 is done when)
- O1–O11 pass on the VS-01 shots, in particular **O5a and O5b for hero and guard** (separated silhouette **and** readable interior) in every frame of the `vs01-duel` sequence, measured with real masks.
- All 20 existing editor PASS lines and 10 EXE PASS lines still pass.
- The art-director and visual-reviewer have no open Critical or Major findings.
- A before/after snapshot pair is in `Review/snapshots/`, and the user has seen the result.

## Decisions needed from the user
1. Approve VS-01 as the first slice, or name a different moment.
2. ~~Pixel characters replace the painted rig~~ **Decided by the user on 2026-10-11:** single-piece characters only; done.
3. Hero size: keep **~23% of view height** (the current SotN lock; default) or try a smaller, Last Faith-like ~15–19%.
4. Floor line: may the art-director propose moving the walkable line lower in the frame (≈78–80% instead of 67%) for more sky and architecture? The 4:3 aspect and the hero size are unchanged. Default: no change unless you approve.
5. Allow `git init` in `KaramaniReborn/` with a baseline commit, so slice work can use branches and worktrees.
