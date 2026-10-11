# Visual acceptance criteria

Used at Gates D–F. Criteria are split into **objective** (measured with a tool; pass/fail is not debatable) and **judged** (art-director / visual-reviewer opinion, always labelled as such). A review that mixes the two without labels is rejected.

Measurement tool: `python -I Tools/frame_metrics.py <png…> [--box x,y,w,h] [--json]` (see BUILD_AND_CAPTURE.md).

## 1. Objective criteria
| ID | Criterion | Threshold | How to measure | Baseline 2026-10-08 build |
| --- | --- | --- | --- | --- |
| O1 | Presentation aspect | content aspect = 1.3333 ± 0.002; pure-black side margins on non-4:3 windows | frame_metrics `contentAspect`, `letterbox` | PASS (all captures) |
| O2 | Pixel grid integrity (HUD-free region) | final 1440×1080 frame: game pixels are 4×4 flat blocks. Flat-block share must not drop more than 0.02 below the last accepted capture of the same shot | frame_metrics `flatBlockShare` | 0.758–0.877 with HUD and vignette on; HUD-free baseline still to capture |
| O3 | Zone palette | raw game buffer (`Review/pixel/chapter-N.png`) ≤ 37 colours | frame_metrics `gameColours` on raw buffer | PASS: 30 / 24 / 26 / 26 |
| O4 | Scene value key | mean luminance of the content area 0.10–0.26 (The Last Faith screenshots measured at ~.10–.26) | frame_metrics `meanLuminance` | ch1 0.161 PASS, **ch2 0.089 FAIL (low)**, ch3 0.119 PASS, ch4 0.239 PASS |
| O5a | Character silhouette separation | \|silhouetteWeber\| ≥ 0.30 against an 8-px ring | frame_metrics `--mask <actor mask>`. Masks come from the capture harness (`Review/chapter-N.<actor>.mask.png`; VS-01 task). Without a mask, `--box` is only a rough fallback: it mixes background in and under-reports contrast (box baseline: Weber 0.036 / 0.311 / 0.078 / −0.183) | PASS as a dark cut-out: −0.67 to −0.76 (hand-built mask, 2026-10-11) |
| O5b | Character interior legibility (not a black cut-out) | figure median luminance ≥ 0.12 **and** P95−P5 spread ≥ 0.30 inside the mask | frame_metrics `--mask`: `figureMedianLuminance`, `figureSpreadP95_P5` | **FAIL in all 4 zones:** median 0.053–0.061, spread ≈ 0.21. Reference: the ungraded pixel hero sheet has a median of 0.25–0.29 and a spread of 0.70 |
| O6 | Character scale | idle hero 20–26 % of view height | `BuildTools.Verify` line "idle hero occupies 20-26 percent" | PASS |
| O7 | No source-art distortion | fixed sprite scales across poses; imported textures = source dimensions | `Review/verification.txt` | PASS |
| O8 | Foot lock | planted foot drift at frame changes < 1 game px (0.04 u) | verification.txt line "stride-locked planted feet" (KARAMANI_WALK_PASS slip value) | PASS: 0.000001 u (2026-10-11, single-piece characters) |
| O9 | Hit-window sync | the clip's active (blade-extended) frames overlap the gameplay hit window by ≥ 80 % | gameplay state durations vs. clip `bounds` (tester computes) | not yet measured |
| O10 | No runtime errors | 0 exceptions / errors in `verify.log`, `Review/player.log` during capture | grep `Exception\|error` (excluding known Unity noise listed in BUILD_AND_CAPTURE.md) | PASS |
| O11 | Regression lock | every PASS line in `Review/verification.txt` and `player-verification.txt` still PASS | diff against the previous run | PASS |

O5a/O5b thresholds are initial values set on 2026-10-11, after the baseline review (`Review/reviews/2026-10-11-baseline-visual-reviewer.md`) showed the box metric could pass a frame where the hero is a black cut-out. The art-director calibrates them at VS-01 Gate A.

When a slice changes a shot, the art-director may add slice-specific objective criteria in `Docs/specs/<slice>-visual-spec.md`, for example "bloom-free", "≥3 parallax planes visible", or "enemy telegraph frame luminance > 0.5".

## 2. Judged criteria (checklist; reviewer must cite the frame and the region)
- **J1 Silhouette:** the player, every enemy and every interactive object reads from its silhouette alone (fill test: imagine it solid black). The weapon's line is readable in attack frames.
- **J2 Focal hierarchy:** the eye lands on the player first, then threats, then the route. Background detail does not compete with actors, and there are no high-contrast clusters behind the action line.
- **J3 Depth:** at least 3 distinct value planes (vista haze, playfield, foreground silhouettes). Atmospheric perspective makes far planes lower in contrast and closer to the fog colour.
- **J4 Light logic:** key direction and warm/cool split are consistent within the shot. Light sources (braziers, lanterns, fire) visibly light their surroundings.
- **J5 Material reading:** steel, mail, cloth, leather, stone and wood read as different materials, through value ramp and cluster shape, not through noise.
- **J6 Pixel craft:** clean clusters, no single-pixel noise in large areas, no banding staircases, no unintended jaggies on long lines, consistent outline policy, no mixels.
- **J7 Animation:** anticipation → action → follow-through on attacks; contact frames have weight (hit-stop and impact FX); no frame pops; arcs on swings; secondary motion (cape, turban tail) lags the body. Idle "breathes".
- **J8 Style fit:** The Last Faith / SotN gothic, 90s–2000s dark anime mood. Not generic mobile fantasy, not flat vector, not over-saturated.
- **J9 Composition:** 4:3 frame balance, horizon placement, and HUD that never covers the action line.

## 3. Severity scale
| Severity | Meaning | Gate effect |
| --- | --- | --- |
| **Critical** | Breaks readability or play (an O-criterion fails, the character is lost in the background, wrong scale, broken animation, errors) | blocks the gate |
| **Major** | Clearly visible quality defect a player would notice in the first minute | must be fixed or explicitly waived by the integrator, with the reason recorded |
| **Minor** | Visible on inspection | goes to the backlog |
| **Polish** | Taste or nice-to-have | backlog, optional |

## 4. Review report format (`Review/reviews/<YYYY-MM-DD>-<slice>-<agent>.md`)
```
# <slice> review: <agent>, <date>
Evidence examined: <exact file paths; say which ones were actually opened as images>
Evidence NOT available: <e.g. no video, no capture of X>
## Objective results
| ID | Value | Threshold | PASS/FAIL | Command |
## Findings (most severe first)
1. [Critical|Major|Minor|Polish] [Objective|Judged] <frame path + region> - <defect> - <concrete correction, owner agent>
## Verdict
Gate <X>: PASS / FAIL (blocking: …)
```
