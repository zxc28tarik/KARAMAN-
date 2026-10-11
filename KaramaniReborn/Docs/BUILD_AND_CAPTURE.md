# Build, verification and capture

All commands run from `KaramaniReborn/`. PowerShell commands are shown; from Git Bash prefix them with `powershell -NoProfile -ExecutionPolicy Bypass -File`.

## 1. Tool inventory (detected 2026-10-10; re-check before relying on it)
| Capability | Status | How |
| --- | --- | --- |
| Unity Editor 6000.6.0f1 (batch mode) | **Available.** Needs a signed-in Unity Hub license (exit 198 = expired) | `Tools/unity_batch.ps1` |
| Unity GUI / Play mode driven by an agent | Not automated. The Editor GUI can be opened for the user; agents use batch jobs | - |
| Offline C# compile (no license, no project lock) | **Available** (Unity's bundled Roslyn) | `Tools/compile_check.ps1` |
| Unity camera → PNG capture | **Available** (editor: `ArtValidation.Capture`; EXE: `RuntimeVerification`) | `-Job Verify` / `-Job CaptureLook`, `VERIFY_WINDOWS.ps1` |
| Image inspection by agents | **Available.** Claude's Read tool opens PNG/JPG as images | read the PNG paths below |
| Objective frame metrics | **Available** (Python 3.14, numpy, Pillow) | `Tools/frame_metrics.py` |
| Procedural pixel sprites | **Available** | `python -I Tools/pixel_characters.py [variant]` |
| Motion preview GIF | Available (node) | `Tools/export-motion.cjs`. Agents can't watch a GIF (only the first frame shows), so review the PNG frame sequence instead |
| Video capture / video inspection | **Not available** (no ffmpeg; Claude can't view video) | review numbered PNG frame sequences |
| Unity Test Framework (EditMode/PlayMode tests) | **Not installed.** Checks live in `BuildTools.Verify` and throw on failure | adding `com.unity.test-framework` needs user approval |
| Blender | **Not installed** | the pipeline is 2D; not needed for the current direction |
| Aseprite, ImageMagick | Not installed | Pillow covers the needs |
| Git | Git 2.55 is installed, but **KaramaniReborn is not a repository yet** | see AGENT_WORKFLOW §6 |
| Desktop screenshot (OS level) | The computer-use tool exists in the main session only, with per-app user approval | prefer Unity camera captures; they are deterministic |

## 2. Commands
| Purpose | Command | Typical time | Success signal |
| --- | --- | --- | --- |
| Fast compile check | `Tools/compile_check.ps1` (`-ExtraSource draft.cs` to include a draft) | 20–30 s | exit 0, `COMPILE_CHECK_SUCCESS` |
| Full verify + captures | `Tools/unity_batch.ps1 -Job Verify` | 75–120 s | exit 0, `UNITY_BATCH_SUCCESS`, fresh `Review/verification.txt` |
| Look-only captures | `Tools/unity_batch.ps1 -Job CaptureLook` | ~60 s | exit 0, fresh `Review/chapter-*.png` + `Review/pixel/*` |
| Windows player build | `Tools/unity_batch.ps1 -Job Build` (or `BUILD_WINDOWS.ps1`) | 60–90 s | exit 0, `Build/KaramaniReborn.exe` |
| Player (EXE) verify + captures | `VERIFY_WINDOWS.ps1` (after Build) | ~60 s | `KARAMANI_PLAYER_VERIFY_SUCCESS` in `Review/player.log` |
| Pixel character sheet | `python -I Tools/pixel_characters.py hero` | ~10–60 s | `Assets/Resources/Characters/hero.png/json` + `Review/characters/hero-sheet.png` |
| Frame metrics | `python -I Tools/frame_metrics.py Review/player-chapter-1.png --box x,y,w,h` | <2 s | numbers (see VISUAL_ACCEPTANCE) |

### Concurrency rules
- **Unity allows one process per project.** Only the **unity-tester** agent, or the main session, runs `unity_batch.ps1`, one job at a time. The wrapper refuses with exit 3 if the project is open. Never kill a Unity process you didn't start: the user may have the Editor open.
- `compile_check.ps1` and `frame_metrics.py` are safe to run in parallel.
- A Verify or CaptureLook run **rewrites** `Review/chapter-*.png`, `Review/pixel/*`, `Review/poses/*`, `Review/animation/*` and `Review/menu.png`. Don't review those files while a job runs. Snapshot the evidence first (§4).

### Freshness rule
`BuildTools.Verify` writes `Review/verification.txt` as a fixed text **only after every check passes**; a failing run leaves the **old** file in place. Evidence is valid only if: the wrapper printed `UNITY_BATCH_SUCCESS` for this run, and the file's timestamp is newer than the run start (the wrapper lists the fresh files).

### Known log noise (not failures)
`[Licensing::Module] Error: Access token is unavailable; failed to update` (when the run still succeeds) · `Code 10 … LicensingClient has failed validation; ignoring` · `Native extension for WindowsStandalone/WebGL target not found` · `Unity Data Store service shutdown timeout` · `debugger-agent: Unable to listen on 3456` · `abort_threads: Failed aborting id` at shutdown. (Observed in a successful Verify on 2026-10-11, T-001.) The license is a Unity Student license valid until 2027-01-04; renew it in Unity Hub before then.

## 3. Evidence locations
| Evidence | Path | Producer |
| --- | --- | --- |
| Editor verification lines | `Review/verification.txt` | Verify |
| Chapter shots, editor, with HUD (1440×1080) | `Review/chapter-1..4.png`, `Review/menu.png` | Verify / CaptureLook |
| Raw game buffers (palette truth): 724×544 = 720×540 + 2-px margin per side (`PixelPipeline.Margin`) | `Review/pixel/chapter-1..4.png` | Verify / CaptureLook |
| Pose stills | `Review/poses/<ActionState>.png` | Verify |
| Animation frame sequence | `Review/animation/frame-000..148.png` (every 4th frame) | Verify |
| EXE shots | `Review/player-*.png`, `Review/player-verification.txt`, `Review/player.log` | VERIFY_WINDOWS.ps1 |
| Pixel sprite previews | `Review/characters/<variant>-sheet.png`, `-zoom.png` | pixel_characters.py |
| Batch logs | `Logs/batch/<Job>-<timestamp>.log` | unity_batch.ps1 |
| Reviews | `Review/reviews/<date>-<slice>-<agent>.md` | art-director / visual-reviewer |
| Gate records | `Review/gates/<slice>.md` | integrator |

## 4. Snapshot before review
Reviews cite frozen evidence: `Review/snapshots/<slice>-<gate>-<timestamp>/` (copy the PNGs being judged). That keeps a later capture run from silently changing what a review refers to and allows before/after comparison at Gate F.

## 5. Known gaps and the smallest fixes
1. **No video.** Use frame sequences. If a slice needs gameplay motion review, extend `BuildTools.CaptureAnimation` or `RuntimeVerification` to dump a named sequence (`Review/sequences/<name>/frame-NNN.png`, 30 fps, HUD off). That's a small C# change for the gameplay-engineer.
2. **No HUD-free capture.** Criterion O2 needs one. Add a HUD-off variant to `CaptureChapters` (F2 already toggles the HUD at runtime).
3. **No per-actor masks.** Criteria O5a and O5b need each actor's silhouette. During capture, render each actor alone into a mask (`Review/chapter-N.<actor>.mask.png`, same size as the frame; white = actor) plus `Review/chapter-N.boxes.json`, so readability is measured on the real silhouette instead of a guessed box.
4. **No unit-test framework.** Keep `BuildTools.Verify` as the test harness (throw-on-fail). Only propose the Unity Test Framework if the user wants it.
