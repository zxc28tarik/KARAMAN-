# Karamani: instructions for Claude

## What is where
- **Active project:** `KaramaniReborn/`, Unity 6000.6.0f1, built-in RP, a 2D dark-fantasy action-platformer. All game work happens here.
- Inactive (don't touch without asking): the HTML prototype (`index.html`, `js/`, `assets/`, `karamani_tek_dosya.html`), `KaramaniUnity/` (superseded slice), `_yedek/` (backups), the `*.zip` packages, `AKTARIM.md` (design handoff; read it for design context).
- Production docs (read before working): `KaramaniReborn/Docs/AGENT_WORKFLOW.md`, `DESIGN_CONSTRAINTS.md`, `ART_DIRECTION.md`, `ASSET_CONTRACTS.md`, `VISUAL_ACCEPTANCE.md`, `BUILD_AND_CAPTURE.md`, `TASK_BOARD.md`.

## User and language
- The user writes Turkish; answer in Turkish. In-game text is Turkish. Code comments in this repo are Turkish; match the file you edit.
- Call the first character **Karamani**, never "Tarık". Don't steer the story with real history.
- Locked design and visual decisions are in `Docs/DESIGN_CONSTRAINTS.md`. Only the user changes them.

## Multi-agent production
- The main session is the **integrator**: it plans, writes task briefs, delegates, merges, keeps `Docs/TASK_BOARD.md` and `Review/gates/*` up to date, and reports to the user.
- Specialists are in `.claude/agents/`: art-director and visual-reviewer (Opus); character-artist, animator, environment-artist, vfx-specialist, gameplay-engineer and unity-tester (Sonnet). Ownership, routing, brief and report formats, and Gates A–F are in AGENT_WORKFLOW.md.
- Delegate only bounded work, in parallel only when owned files don't overlap. Don't call every specialist for every task.
- A gate is marked only with evidence (command + exit code + file paths). Visual claims need captures that were actually opened.

## Commands (from `KaramaniReborn/`)
```
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1          # offline C# compile, ~25 s
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Verify # Unity verify + captures, 1-2 min
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job CaptureLook|Build
powershell -NoProfile -ExecutionPolicy Bypass -File VERIFY_WINDOWS.ps1               # EXE self-test (after Build)
python -I Tools/frame_metrics.py Review/player-chapter-1.png --box x,y,w,h            # objective frame metrics
python -I Tools/pixel_characters.py hero                                              # regenerate pixel sprite sheet
```
- Unity runs one instance per project: one batch job at a time, and never kill a Unity process you didn't start (the user may have the Editor open).
- License exit 198 (wrapper exit 4): the user must sign in to Unity Hub. Never sign in for them.
- Never edit, re-save or resample the source paintings in `KaramaniReborn/Assets/Resources/Art/`.
- `KaramaniReborn` is not a git repository yet. Ask before `git init`, commits or pushes.
