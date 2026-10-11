# Agent workflow: Karamani production

The main Claude session is the **integrator**: production coordinator, final decision-maker below the user, and the only party that merges work across owners, marks gates and talks to the user. Specialist subagents (defined in `../../.claude/agents/`) do bounded work and return evidence.

Required reading for every agent: this file, `DESIGN_CONSTRAINTS.md`, `ASSET_CONTRACTS.md`. Visual roles also read `ART_DIRECTION.md` (Part 1) and `VISUAL_ACCEPTANCE.md`. Anyone who runs tools reads `BUILD_AND_CAPTURE.md`.

## 1. Roster, models and file ownership
| Agent | Model | Owns: may edit | Never edits |
| --- | --- | --- | --- |
| **integrator** (main session) | Opus | `CLAUDE.md`, `Docs/AGENT_WORKFLOW.md`, `Docs/ASSET_CONTRACTS.md`, `Docs/DESIGN_CONSTRAINTS.md` (user decisions only), `Docs/TASK_BOARD.md`, `Review/gates/*`, `README.md`, `Docs/QA.md`, `Tools/unity_batch.ps1`, `Tools/compile_check.ps1`, `Tools/frame_metrics.py` | - |
| **art-director** | Opus | `Docs/ART_DIRECTION.md` Part 1, `Docs/specs/*-visual-spec.md`, slice criteria appended to `Docs/VISUAL_ACCEPTANCE.md` §1, `Review/reviews/*-art-director.md` | code, assets, ART_DIRECTION Part 2 |
| **character-artist** | Sonnet | `Tools/pixel_characters.py`: construction (ramps, `draw_*`, materials, patterns, variants); `Assets/Resources/Characters/*`; `Review/characters/*` | animation poses and timing in the generator (animator), C# |
| **animator** | Sonnet | `Tools/pixel_characters.py`: animation (`pose_defaults`, `gait`, `locomotion`, `make_animations`, timing / strides); `Review/animation-review/*` | gameplay timing constants, state machine |
| **environment-artist** | Sonnet | `Assets/Scripts/StageArt.cs`; environment loading in `ArtLibrary.cs`; `Assets/Resources/Environment/*`; `Tools/env_*.py` | `ZoneDefinition.surfaces` (collision belongs to gameplay; send a request), the source paintings |
| **vfx-specialist** | Sonnet | `Assets/Scripts/Effects.cs`, `BladeTrail.cs`, `LightField.cs`, `PixelPipeline.cs` (palette values need art-director sign-off), `Assets/Resources/Shaders/*`, `Assets/Resources/FX/*` | gameplay rules, actor code |
| **gameplay-engineer** | Sonnet | `GameRoot.cs`, `GameWorld.cs`, `GameTypes.cs`, `GameHUD.cs`, `RuntimeVerification.cs`, `PixelCharacter.cs` (single-piece frame renderer), new runtime integration components, `Assets/Editor/BuildTools.cs`, `Assets/Editor/ArtValidation.cs`, `VERIFY_WINDOWS.ps1`, `BUILD_WINDOWS.ps1` | `GameView.cs` constants (locked), art files |
| **unity-tester** | Sonnet | `Review/test-reports/*` only. Runs `unity_batch.ps1`, `VERIFY_WINDOWS.ps1`, `compile_check.ps1` | any source, asset or doc |
| **visual-reviewer** | Opus | `Review/reviews/*-visual-reviewer.md`, `Review/snapshots/*` | any source, asset or doc |

Shared files: `pixel_characters.py` (character-artist + animator) and `ArtLibrary.cs` (environment + character loading) have **one editor at a time**. The lock is recorded in TASK_BOARD. A planned early task is to split `pixel_characters.py` into construction and animation modules, which removes the shared lock.

Unowned or new paths: the integrator assigns an owner in TASK_BOARD before anyone edits.

## 2. Routing: who gets called (not everyone, every time)
| Task type | Agents | Typical order |
| --- | --- | --- |
| New visual slice / look change | art-director (spec) → specialists → unity-tester → visual-reviewer + art-director | Gates A→F |
| Character art fix (e.g. face readability) | character-artist → unity-tester (only if integrated) → visual-reviewer | B, D, E |
| Animation polish | animator → unity-tester → visual-reviewer | C, D, E |
| Gameplay bug or new mechanic hook | gameplay-engineer → unity-tester | C |
| Lighting / post / FX | vfx-specialist → unity-tester → visual-reviewer | D, E |
| "Does it still build / pass?" | unity-tester alone | - |
| "How does this look?" | visual-reviewer alone (on existing evidence) | E |

Model escalation: Sonnet specialists do bounded implementation. The integrator re-routes a task to an Opus-level pass (itself or the art-director) when it needs a cross-system architecture decision, a contract change, or a third failed attempt at the same acceptance criterion.

## 3. Delegation protocol
### 3.1 Task brief (integrator → agent). Every delegation uses this shape.
```
TASK <T-id>: <title>          Slice: <slice-id>   Gate: <A-F>
Goal: <one sentence>
Read first: <docs + specific files/line ranges>
You own (may edit): <exact paths or functions>
Read-only: everything else (especially <...>)
Deliverables: <files that must exist / change>
Acceptance: <VISUAL_ACCEPTANCE IDs + task-specific checks>
Evidence to return: <commands + paths>
Do not: run Unity batch jobs (unity-tester only), touch locks in DESIGN_CONSTRAINTS, edit source paintings
Stop and report (don't improvise) if: <contract conflict / missing input / scope creep>
```
### 3.2 Result report (agent → integrator). The agent's final message.
```
RESULT: DONE | PARTIAL | BLOCKED
Files changed: <path - what and why>
Commands run: <command - exit code - decisive output line>
Evidence: <paths>  (say which images were actually opened)
Acceptance self-check: <criterion - met/not met - how verified>
Not verified: <anything not checked, and why>
Requests to other owners: <owner - request>
```
Claims without a command, exit code or evidence path count as **unverified**. The integrator does not mark gates on unverified claims.

## 4. Parallelisation rules
1. Run agents in parallel only when their **owned file sets don't overlap**. Read-only overlap is fine.
2. Unity is single-instance per project: at most one `unity_batch.ps1` job at a time, run only by the unity-tester (or the integrator).
3. Don't review evidence that a running job is rewriting. Snapshot it first (BUILD_AND_CAPTURE §4).
4. Contracts first: when a task changes scale, coordinates, naming, animation states, asset formats or rendering settings, the change lands in `ASSET_CONTRACTS.md` **before** the parallel work starts.
5. Visual-reviewer independence: the visual-reviewer gets the evidence and criteria, **not** the art-director's or the implementer's opinion of it.

## 5. Quality gates
Each slice keeps `Review/gates/<slice-id>.md`, with one section per gate: date, evidence paths, signer, verdict. **No gate is marked complete without the evidence listed.**

| Gate | Passes when | Required evidence | Signed by |
| --- | --- | --- | --- |
| **A: Spec** | Reference and visual spec approved | `Docs/specs/<slice>-visual-spec.md`, including the reference list (what to take from each), shot list, slice-specific objective criteria and a list of the locks it touches. A lock change needs the user's yes, quoted in the gate file | art-director writes, integrator approves |
| **B: Assets valid** | Character and environment assets are structurally valid and import cleanly | generator command + exit 0; files match ASSET_CONTRACTS (names, dimensions, JSON schema); `compile_check` exit 0; Unity job SUCCESS with no import errors in the log | specialist produces, unity-tester verifies |
| **C: Integration** | Animation and gameplay integration works | fresh `UNITY_BATCH_SUCCESS` for Verify; new checks added for new behaviour; **all earlier PASS lines still present**; O9 hit-window sync | gameplay-engineer + animator, unity-tester verifies |
| **D: Rendered** | The scene renders and evidence is captured | fresh captures (editor, plus EXE when the build is affected), snapshot folder, `frame_metrics --json` output saved next to the snapshot | unity-tester |
| **E: Reviewed** | Critical issues identified | two independent reports (visual-reviewer, art-director) in `Review/reviews/`; the integrator merges findings into TASK_BOARD with owners | visual-reviewer + art-director |
| **F: Fixed** | Corrections implemented and verified | re-capture; before/after snapshots; each Critical and Major finding marked *fixed* (with evidence) or *waived* (with the integrator's reason); all O-criteria re-measured; the visual-reviewer confirms closure | integrator |

If a gate cannot be evidenced, for example motion that needs video, the gate file says so explicitly, along with the smallest workaround used (BUILD_AND_CAPTURE §5).

## 6. Version control and isolation
- Status 2026-10-10: `KaramaniReborn` is **not a git repository**. Recommended (needs the user's yes): `git init` in `KaramaniReborn/` with the existing `.gitignore`, a baseline commit, then one branch per slice (`slice/<id>`).
- Until then: strict file ownership, sequential edits of shared files, and an integrator snapshot to `../_yedek/` before any multi-file risky change.
- With git: parallel code or asset work may use `isolation: worktree`. Worktrees have no Unity `Library/`, so Unity jobs and `compile_check` (which needs `Library/ScriptAssemblies/UnityEngine.UI.dll`) run on the integrated main checkout after merge.

## 7. Task board
`Docs/TASK_BOARD.md` is the single register of tasks, owners, file locks, dependencies and gate status. The integrator updates it when delegating and when results come back.
