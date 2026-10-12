# SDD ledger — plan: KaramaniReborn/Docs/plans/2026-10-12-goblin-part2-plan.md
Branch goblinler from main b6312fc; plan commit 3e20cc7.
Preflight scan (all tasks share EnemyProfiles/EnemyBrain/GameWorld/GoblinTests, run strictly sequentially):
| Pair / task | produces → consumes | finding |
|---|---|---|
| T1→T2..T7 | placeholder goblin profiles, perches, Actor.perch, GoblinArena, faction cap | later tasks replace placeholder profiles; consistent |
| T1→T3 | Actor.perch + zone.perches → Pusucu | consistent |
| T3↔T4 | pack courage leader = Kaba or Kırçıl; Kırçıl placeholder exists from T1 | consistent |
| T1→T5/T7 | zone 6 gate = Kara Diş only; Çatal boss=false | consistent |
| T7 | journey 0..6 extends part-1 journey check | allowed: extends, not weakens |
| each task | tests listed match its requirements | self-consistent |
Ruling: Kırçıl size 0.9 — spec gives no number ("uzun sıska") — cosmetic, retunable by user.
Ruling: Çatal mounted 1.15 / on foot 0.72 — spec gives no number — cosmetic.
Ruling: zone 5 gate keeper is Kırçıl (boss=true) — spec has no boss there; a sealed gate needs a keeper — if wrong, zone 5 gate opens freely.
Ruling: placeholder backgrounds 4→2, 5→3, 6→1 — zone art is a separate job (spec §6) — visual only.
Ruling: plan written at behaviour level (no full code) at the user's speed request — implementers are sonnet with design latitude.
Task 1: dispatched (base 3e20cc7, sonnet). Side job: goblin ChatGPT prompts (enemy_prompts.py), no commit by agent; controller commits.
Side job: goblin prompts committed 8dadb6d (199 prompts). Task 1 review range must exclude it (base 8dadb6d if Task 1 commits come after).
User decision: Çatal leaves boar at 50%, boar attacks separately, easy (45 hp). Spec/plan/prompts updated in 92bd745. Task 5 brief must be regenerated from the plan.
Task 1: implementer DONE_WITH_CONCERNS, commit f576873 (review range 92bd745..f576873).
Ruling: run Task 1 review in parallel with the Task 2 implementer — user asked for speed — if Task 1 review finds load-bearing issues, they are fixed in one dispatch after Task 2 lands (both touch same files, sequential fixes avoid conflicts).
Task 1: parked — goblin pixel sheets not scaled by Actor.size (render human height; hitboxes scaled) — Ruling: pre-existing renderer behaviour (Kara Murad same), solved by HD sheets/art pass — visual only.
Task 1: parked — zones 4–6 still show Ottoman banners/braziers, perch marker reuses timber prop (gallows-like) — Ruling: zone art is a separate environment job (spec §6) — visual only.
Task 1: minor (deferred): part banner re-shows on respawn in zone 4.
Task 1: review dispatched (sonnet). Task 2: implementer dispatched (base f576873, sonnet).
Task 1: review Approved (spec ✅, 0 critical/important). Minors deferred:
Task 1: minor (deferred): BuildTools VerifyCharacters goblin skip too wide — only height check should be skipped (single-piece + clip checks must stay) — CARRY into Task 3 dispatch.
Task 1: minor (deferred): zone 5 platform y -.35 and zone 6 y -.3 are 1.90/1.95 above floor, above single-jump peak 1.89 (part 1 max 1.8) — lower to ≤ 1.8 — CARRY into Task 3 dispatch.
Task 1: minor (deferred): unused objective text zone 4; stale HUD card comment; Menu test only checks paintings; Perches/Zones overlap; ember/ash particle names in zones 4-5; AttackersOf profile lookups per tick; fourthWaited readability; banner on respawn.
Task 1: complete (commits 92bd745..f576873, review clean)
User decision (2026-10-12): part 2 = 5 chapters (open-field waves, chase to cave, cave map of rooms SotN-style for chapters 7-9). Spec §8 + plan restructured in 8c124b3: new Task 4 (room engine), Task 5 (5-chapter structure); old Tasks 4-7 renumbered 6-9.
Ruling: "3. VE 5." read as chapters 3 through 5 all in the cave map — the message says those are in the cave and one map — if wrong, region split changes only Task 5 content.
Ruling: no ability-gated doors in the cave map — the user did not ask for metroidvania abilities — adding them later is a content task.
Ruling: bait goblins in the chase retreat tactically (lure), not in panic — the user's tone rule forbids comic fleeing — behaviour only.
Ruling: zone 4 open field uses 2 waves, exit opens after the last wave — spec §8.1 — tuning only.
Carry into Task 4/5: platforms must be reachable by a single jump (≤ 1.8 above the surface below; jump peak 1.89) unless deliberately a double-jump spot (Task 1 minor). Task 1's zone 5/6 platform heights become moot after Task 5 replaces those zones.
User decision: dead enemies never respawn (rooms); more jumping/climbing in the cave. Spec §8.2 + plan Tasks 4/5 updated.
Ruling: cave abyss = 25 damage + return to last safe ground (not death) — platforming-heavy rooms would be punishing with instant death; linear zones keep pit death — if wrong, one constant.
Ruling: ropes (goblin rope ladders) and crumbling rock ledges added as platforming elements — natural, period-plausible, no machines — new mechanics, art placeholder climb clip.
Ruling: Task 5 starts with a schematic cave map (Review/cave-map.png) generated from the same data as the game, shown to the user — user doubted the map — building continues meanwhile.
User decision: crumbling ledges never return (persisted). Ruling: every crumbling crossing has an alternative route and a test proves the map is never locked with all ledges removed — otherwise a permanent fall could softlock the map.
Task 2: implementer DONE_WITH_CONCERNS, commit 508408a. Concerns noted: Bıçakçı reach 2 / hitReach 1.1 (leap start vs land), volley 12 red (brief silent), heavy 44 kills ordinary goblins so knock-down shows only on tougher ones, Pusucu/Çömlekçi placeholders carry heavyKnockdown 1.5 (carry to Task 3).
Task 2: review dispatched (f6bc03c..508408a). Task 3: implementer dispatched (base 508408a), carries Task 1 BuildTools skip-narrowing minor.
Task 2: review Approved (spec ✅, 0 critical/important).
Task 2: minor (deferred): flankstab selectable standalone (reach 1.1, no cooldown) — fix as reach 0 + hitReach 1.1 — CARRY to Task 4 dispatch (one-line).
Task 2: minor (deferred): pair line can stall if rear partner stuck at an edge / in long Hurt — add a timeout (e.g. front waits ≤ 1.5 s) — CARRY to Task 4 dispatch.
Task 2: minor (deferred): white flank stab not blockable from behind (spec: stabs from behind; ~0.7 s warning, dodge works) — Ruling: keep, matches spec §3.1 — if wrong, make it turn-blockable.
Task 2: minor (deferred): heavy knock-down invisible on ordinary goblins (44 dmg kills); volley 12 red no poison; extra Okçu dagger; poison ignores invulnerability (commented).
Task 2: complete (commits f6bc03c..508408a, review clean)
User decision (2026-10-12): the Bıçakçı flank stab becomes blockable if the player turns to face it (white rule) — CARRY to Task 4 dispatch with the other two Task 2 minors.
RESUME POINT (saved at the user's request before a usage limit):
- Done: Task 1 (f576873), Task 2 (508408a), both reviewed clean.
- In flight: Task 3 implementer (base 508408a; Pusucu, Çömlekçi, pack courage, BuildTools goblin skip narrowing). If it committed "Goblinler: Pusucu, Ateş Çömlekçisi, sürü cesareti", review it (range 508408a..that commit); if not, check git status and re-dispatch Task 3 from the brief with the report file as memory.
- Next: Task 4 room engine (carry: flankstab reach 0 + hitReach 1.1; pair-line wait timeout ≤ 1.5 s; flank stab blockable when facing), Task 5 structure (map sketch first, show user), Tasks 6-9.
Session resumed after usage limit: goblinler pushed to origin. Task 3 agent was cut mid-task (uncommitted edits in tree); resumed via SendMessage.
Task 3: BLOCKED — Unity license exit 198; work uncommitted in tree (HEAD 91972cf); RegularsB tests written, Enemies not yet green (perch-2 arena change unrun). Waiting for user to sign in to Unity Hub, then resume the same agent.
RESUME POINT (updated before moving to a new chat):
- Done + reviewed: Task 1 (f576873), Task 2 (508408a). Branch goblinler pushed to origin.
- Task 3: code written but UNCOMMITTED in the working tree (BuildTools.cs, GoblinTests.cs, Effects.cs, EnemyBrain.cs, EnemyProfiles.cs, GameHUD.cs, GameRoot.cs, GameTypes.cs, GameWorld.cs, PixelCharacter.cs). compile_check passes. Unity license dropped (exit 198) — user must sign in to Unity Hub. Then: revert Assets/Scenes/Karamani.unity, run -Job Enemies (first run failed at the pounce-landing assertion; test arena moved to perch 2, not yet run), tune until KARAMANI_GOBLIN_REGULARS_B_PASS, run -Job Verify, commit "Goblinler: Pusucu, Ateş Çömlekçisi, sürü cesareti", update task-3-report.md, then dispatch the Task 3 review (range 91972cf..<commit>; the ledger commit is docs-only).
- Task 4 dispatch must carry: flankstab reach 0 + hitReach 1.1; pair-line wait timeout ≤ 1.5 s; Bıçakçı flank stab blockable when the player turns to face it (user decision).
- Then Task 5 (map sketch first, show the user), Tasks 6-9.
