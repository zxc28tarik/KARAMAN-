# Multi-agent setup: Phase 1 discovery (2026-10-08)

## Setup result (2026-10-11): all phases done
- **Agents:** 8 definitions in `karamani/.claude/agents/`, frontmatter validated (name = filename, model as assigned, tools from Read/Grep/Glob/Write/Edit/Bash). They load at the **next session start**; this session had already loaded its agent list (`Agent type 'unity-tester' not found`).
- **Delegation tests:** proxy runs, where a general-purpose agent loaded the definition file as its role, with the assigned model:
  - T-001 unity-tester (sonnet): PASS. compile_check exit 0; `unity_batch.ps1 -Job Verify` exit 0 in 143 s; 20/20 PASS lines fresh; 0 real log errors. Report: `Review/test-reports/2026-10-11-T-001-baseline.md`. Claims spot-checked by the integrator: mtime, marker, log.
  - T-002 visual-reviewer (opus): DONE, baseline Gate E FAIL (2 Critical, 5 Major, 3 Minor). Report: `Review/reviews/2026-10-11-baseline-visual-reviewer.md`. Its O5 numbers were reproduced exactly by the integrator; snapshot SHA-256 verified. It found a flaw in the O5 box metric, which led to the new `--mask` mode and the O5a/O5b split.
  - Both ran in parallel without file conflicts.
- **Docs:** CLAUDE.md, AGENT_WORKFLOW, DESIGN_CONSTRAINTS, ART_DIRECTION (Part 1 added), ASSET_CONTRACTS, VISUAL_ACCEPTANCE, BUILD_AND_CAPTURE, TASK_BOARD, specs/VS-01-proposal.
- **Tools:** compile_check.ps1 (positive and negative tests passed), unity_batch.ps1 (used by T-001), frame_metrics.py (box, mask and raw-buffer modes).
- **Not verifiable here:** native loading of the agents (needs a new session); the standalone CLI is not logged in.

---
Phase 1 discovery notes (2026-10-08) follow.

## Project facts
- Active project: `KaramaniReborn/`, Unity **6000.6.0f1** (only editor installed: `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe`).
- Built-in render pipeline (no URP/HDRP). Packages: ugui 2.6.0, audio, imgui, ui, jsonserialize, imageconversion, screencapture, particlesystem. No Input System, no Animator/Timeline usage, no Test Framework package.
- One scene: `Assets/Scenes/Karamani.unity`. The world is built at runtime by `GameRoot`.
- Scripts (~3.5k lines): GameRoot, GameWorld, GameTypes (ActionState enum, Actor, ZoneDefinition, KinematicMotor), GameView (single 4:3 view profile: ortho 5.4, 1440x1080 render), PaintedRig (rigid-part painted rig, 2-link IK, world-locked feet), PixelPipeline + PixelLook (360x270 pixel world, per-zone 37-color palettes), StageArt, ArtLibrary, Effects, BladeTrail, LightField, GameHUD, RuntimeVerification.
- Shaders: BladeTrail, PaintedSprite, PixelPost, PixelScreen (Assets/Resources/Shaders).
- Art: `Assets/Resources/Art` has 10 AI-painted PNGs (hero/guard 16-part atlases, 4 backgrounds, props, equipment, stone, rampart). Imported uncompressed, NPOT none. SHA-256 provenance is in Docs/QA.md.
- Existing docs (Turkish): `README.md`, `Docs/ART_DIRECTION.md` (asset generation prompts and provenance; do NOT overwrite), `Docs/VIEW_SCALE.md`, `Docs/QA.md`.
- No CLAUDE.md exists anywhere. No `.claude/agents/`. `karamani/.claude/launch.json` serves the old HTML game only.
- Not a git repo (a `.gitignore` exists). Backups in `karamani/_yedek/`. Old projects: `KaramaniUnity/` (superseded) and the HTML game at the root (inactive).

## Existing automation (works, last run 2026-10-08 20:13–20:15, all PASS)
- `BuildTools.Verify` (batch `-executeMethod KaramaniReborn.Editor.BuildTools.Verify`) runs numeric checks and writes `Review/verification.txt` plus Unity camera renders (`Review/chapter-*.png`, `menu.png`, `Review/pixel`, `poses`, `animation`).
- `BUILD_WINDOWS.ps1` builds `Build/KaramaniReborn.exe` (build.log shows success, about 32 s).
- `VERIFY_WINDOWS.ps1` runs the EXE with `--karamani-verify` and writes `Review/player-*.png` and `player-verification.txt`.
- `Tools/export-motion.cjs` (node) builds `Review/motion-preview.gif`; `Tools/package.ps1` makes zips.
- Menu items: Karamani/Projeyi hazırla, Sahneyi önizle, Windows sürümünü derle, Hareket ve savaş doğrulaması, Görünüm yakala, Animation/Review Painted Rigs.

## Capabilities
- Available: Unity batchmode CLI (needs a valid Hub license; exit 198 means sign in again), Unity-camera PNG capture (Claude can Read PNGs = real visual review), git 2.55, python 3.14, node 24, dotnet, gh, Bundled Roslyn offline compile check (see memory unity-tooling).
- Missing: Blender, ffmpeg/ffprobe, ImageMagick, Aseprite, `claude` CLI on PATH, Unity Test Framework package, Git repo. GitHub MCP failed to connect this session.
- Video: no direct video inspection. Use PNG frame sequences from Unity (already produced in `Review/animation`).

## Decisions for Phase 2+
- Put agents in `karamani/.claude/agents/` (session root) and a root `karamani/CLAUDE.md` (it is loaded from KaramaniReborn sessions too, as a parent).
- New production docs go in `KaramaniReborn/Docs/`. Production art direction: extend, don't replace, the existing ART_DIRECTION.md (add a current pixel-pipeline section above the provenance content), or create `Docs/ART_SPEC.md` and link both.
- Recommend `git init` in KaramaniReborn before multi-agent work (ask the user first), so worktrees and branches are possible.
- Preserve: 4:3 SotN framing, PixelPipeline 360x270 look, rigid painted-part rig, existing combat (parry/block, combo, wolf dash, pistol rules), character design in memory.
