# SDD ledger — plan: KaramaniReborn/Docs/plans/2026-10-12-boss-aghas-plan.md
Spec: KaramaniReborn/Docs/specs/2026-10-12-enemy-roster-design.md (§2, §4)
Branch: agalar (from main 7d03ef7); plan commit 576ef1c

## Preflight rulings
- Ruling: local branch `agalar` in place, no worktree (Unity Library / single instance) — cost if wrong: main checkout busy until merge.
- Ruling: implementers commit locally per task; merge to main + push only at the end (the user asked to start and earlier to finish everything) — cost if wrong: local commits are reversible.
- Ruling: implementers are general-purpose agents and run Unity batch jobs themselves, one at a time — cost if wrong: workflow deviation only.

## Preflight conflict scan
| Pair / task | Produces → consumes | Finding |
|---|---|---|
| T1→T2 | Arslan profile, Allowed, phase change (attackReady reset), spawnQueue, Summon/Rage release → Arslan tests | consistent |
| T1→T3 | KorDemir profile, Actor.home → arena fire uses home | consistent; ArenaFire/Grenade release absent between T1 and T3 (no-op) — acceptable |
| T2→T3 | ShotKind.Fire/Grenade, Projectile.width/tick/target/attack, SpawnFire → Grenade/Explode/ArenaFire | consistent |
| T1→T4 | Kurtbas profile, phase2Taken/Unstoppable in Damage → frenzy test | consistent |
| T1→T5 | hitReach, hitTimes, grab gating, chain facing, phase2Speed → Kara Murad tests | consistent |
| T1 self | Layouts expects bosses; Spawn values from profile | ok |
| T2 self | plant pit case 15.9 → summons 13.7 (rejected) / 18.1 | ok |
| T3 self | arena fire spans vs test points home+3 / home-2 | ok |
| T4 self | storm from gap 1.0 keeps 4th hit in reach | ok |
| T5 self | charge hitReach 1.2 so it hits only on contact | ok |
- Task 1: BLOCKED — Unity license exit 198 at 15:01/15:02 (Hub signed out). Implementation uncommitted in working tree (5 files), compile passes; Enemies/Verify not run. Waiting for the user to sign in to Unity Hub; then resume implementer to run jobs + commit.
- Task 1: Ruling: Kara Murad size 1.25 → 1.22 (spec says both "bosses 12–25% taller than a janissary (.98)" and "about 25% taller than Karamani"; 1.22 = +24.5% vs janissary, +22% vs Karamani) and the Bosses test asserts the ratio to the janissary size — cost if wrong: 3% shorter giant.
- Task 1: Ruling: Kurtbaş phase 2 is still staggered by a successful parry — spec says hits don't stagger him; a parry is not a hit and keeps the player's counter tool — cost if wrong: one condition in the parry branch.
- Task 1: Ruling: test gaps — frenzy damage/unstoppable is covered by Task 4's test, summon spawn path by Task 2's; add now only a Choose-gating test (phase 2 AI never starts a phase-1 attack) and the size ratio.
- Task 1: carry → Task 3: Grenade/Pull/LeapSlam use the .8 melee height gate in Choose — grenades should use the ranged height gate (3).
- Task 1: carry → Task 2: summoned Nefers get no minimap markers (rebuild when enemy count changes).
- Task 1: minor (deferred): non-blade kinds (Summon/Rage/ArenaFire/Grenade/LeapOver) play swing sounds and glint; chain facing bypasses turnDelay; non-boss Captain zone-scaled health.
- Task 1: RULING REVERSED by the user (2026-10-12): boss sizes were decided earlier with the user — Kara Murad keeps 1.25 (≈25% taller than Karamani). The "12–25%" rule is read against Karamani (size 1), not the .98 janissary. Never change approved character sizes without asking.
- USER DECISION (2026-10-12): Kara Murad is 60% taller than Karamani and twice as bulky → size 1.6 and a new profile `bulk` (hurtbox width multiplier) = 2.0. Ruling: his melee reaches scale ~×1.25 with his body (combo 1.9→2.4, fists 1.3→1.65, grab/grab2 1.2→1.5, charge/charge2/recharge hitReach 1.2→1.5) — cost if wrong: tune reach later. The placeholder captain sheet cannot show 1.6× height: VerifyCharacters skips KaraMurad's idle-height check (TODO: remove when his HD sheet exists); his HD sheet must be built at 1.6× Karamani's height.
- Ruling (user: 'geç ama çok yavaşladık'): speed over per-task gates — Task 1 fix round goes straight to the final review (no scoped re-review); Tasks 2–5 run as ONE implementer dispatch (sequential, a commit per task) with ONE combined review; then Task 6 and the final whole-branch review — cost if wrong: a defect found later spans more code.
- Task 1: complete (commits 576ef1c..5bb282a + spec note; fix round 1 per user decisions; final review will see it)
- Tasks 2–5: complete (commits 3318f8d..2a233a5; Verify-20261012-154602 KARAMANI_VERIFY_SUCCESS + 5 BOSS markers). Deviations: Kör Demir jump-over-fire test uses a full held jump (short hop landed at 6.45 inside the 6.5 burn edge).
- Ruling (speed): the combined Task 2–5 review and the final whole-branch review are merged into ONE opus review of 7d03ef7..HEAD, run in parallel with Task 6 (verification/build/docs; read-only reviewer, no file overlap) — cost if wrong: one review pass covers more code.

## Final review (opus, 7d03ef7..2a233a5): Ready to merge — With fixes
- Final: Ruling: #1 fires sourced by a boss are extinguished when that boss dies, and arena-fire spans never reach the exit trigger (clamp to zone.length-3.5) — cost: none.
- Final: Ruling: #2 grenade/oil target y snaps to the ground below the player (highest surface top ≤ player y at that x, else zone.floor) — cost: none.
- Final: Ruling: #3 leap-slam target clamped to a standable x (else no leap: slam in place); horizontal velocity zeroed on the first grounded frame of any leap; leap-over `front` fallback also Standable-checked (else stay) — cost: none.
- Final: Ruling: #4 Kör Demir (any enemy) never walks/retreats into an active fire patch: treat fire spans like edges in TickEnemy movement — spec "narrow space" confines both — cost if wrong: flag later.
- Final: Ruling: #5 add the deterministic positive grab test (+ mutation `if (k.grab) continue;` must fail it).
- Final: Ruling: fix now (minors): arena-fire test asserts fire geometry directly; jump-over test asserts the player crossed (x > 6.6); hook pull and push apply only if the hit actually damaged (health dropped); fire ticks burn without hitstun (health, flash, number; death if 0) — player expectation for DoT; stale "12–25%" comment.
- Final: deferred: tuning constants in brain/world → profile; BodyWidth helper; Captain dead in play; leap-over windup/glint/sound polish; frenzy parry test; Sorcerer cooldown reset on phase change (accepted).
- Final: fix wave (commits 9d58bb5..487e161): 10 items; all checks green. Deviation: item-4 test uses player at home-0.5 (home-2 could not fail).
Final re-review (9d58bb5..487e161): all 10 items addressed; ready to merge: yes. Minor findings parked by ruling:
- Extinguished fire may burn once on the boss's death frame (or once when a delayed patch activates): parked, single tick at most, the fight is already over.
- Burn grants no post-hit i-frames, so overlapping patches stack: parked, intended (fire zones should hurt; dodge i-frames still protect).
- Burn has no sound, hit-stop or death launch: parked for the audio/VFX pass.
- LeapOver plays in place when no spot is standable: parked, harmless.
- Stale "boy %12-25" snippet in the plan doc: fixed.
