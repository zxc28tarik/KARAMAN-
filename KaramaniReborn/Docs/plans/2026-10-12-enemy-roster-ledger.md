# SDD ledger — plan: KaramaniReborn/Docs/plans/2026-10-12-enemy-roster-plan.md
Spec: KaramaniReborn/Docs/specs/2026-10-12-enemy-roster-design.md
Branch: dusman-kadrosu (from main 9d58899); merge base c6c73a2^ = 9d58899

## Preflight rulings
- Ruling: work on local branch `dusman-kadrosu` in place, no git worktree — Unity needs the existing 412 MB Library and allows one instance per project; a worktree forces a full reimport — cost if wrong: main checkout busy on a branch until merge.
- Ruling: implementers commit locally on the branch (the user chose this workflow); nothing is pushed and nothing merges to main without the user's yes (CLAUDE.md: commit/push only when asked) — cost if wrong: commits can be squashed or reset locally.
- Ruling: implementers are general-purpose agents (the project's gameplay-engineer agent is not loaded in this session) and run Unity batch jobs themselves, strictly one at a time — normally unity-tester's role — cost if wrong: workflow deviation only.

## Preflight conflict scan
| Pair / task | Produces → consumes | Finding |
|---|---|---|
| T1→T2 | Actor.attack/strike/attackReady, EnemyProfile, Spawn(public) → Begin/StartAttack/Arena | consistent |
| T2→T3 | TickEnemy block "a.face = … else a.state = Idle" → T3 replaces exactly that block; Damage(…, red) → T3 shield/guard edits use `red` | consistent |
| T2→T4 | PerformAttack `default: return;` → T4 replaces it | consistent |
| T4→T5 | `case Reload/Backstep/Teleport: return;`, Release switch (`hand`), ShotKind/Projectile → T5 edits those | consistent |
| T1→T6 | constructor spawns → T6 Layout replaces | consistent |
| T2/T6 | VerifyCombat parry edit (T2) and T6 note on combo guard | consistent |
| T1 self | test uses Spawn/view.variant/Find — all produced in T1 | ok |
| T2 self | dash ledge guard included; parry/red/window tests match code | ok |
| T3 self | back-hit guard 75-40=35 (no stagger) matches assertion | ok |
| T4 self | reload starts at 1.55 s, asserted at 1.7 s | ok |
| T5 self | teleport options cover the pit case (17.6+5=22.6 standable) | ok |
| T6 self | layout order matches EnemyTests.Layouts expectation | ok |
| T7 self | 89 prompts / 178 fences | ok |

## Execution
- Task 1: Ruling: Sorcerer skipped in VerifyCharacters idle-height check (placeholder captain sheet is 2.88 tall at size 1) — the check is a sheet sanity check already covered by Captain on the same sheet — cost if wrong: Sorcerer sheet errors uncaught until its HD sheet arrives.
- Task 1: minor (deferred): skip wraps whole loop body (also skips single-piece/state render asserts for Sorcerer); add TODO to remove when Sorcerer gets its own sheet.
- Task 1: minor (deferred): Spawn boss branch hardcodes guard 95 / size 1.09 (duplicates Captain profile) — plan-mandated.
- Task 1: minor (deferred): "ghoul" variant still in PixelCharacter.Variants and pixel_characters.py, unused.
- Task 1: minor (deferred): Profiles test does not cover Captain or ranged flags of other roles.
- Task 1: note: Review/verification.txt summary text not updated with enemy lines (cosmetic).
- Task 1: complete (commits 807b199..66cdd63, review clean)
- Task 2: Ruling: full Verify is red at VerifyRanged (elevated archer) between Task 2 and Task 4 — plan ordering gap: old TickEnemy (which fired arrows) is deleted in T2, ranged release arrives in T4 which also rewrites that test — accept, T4 must end with full Verify green — cost if wrong: one red Verify window on a local branch.
- Task 2: minor (deferred): no test for red attack vs a well-timed parry (only held block); add a red+timed-block case.
- Task 2: minor (deferred): dodge test lacks a no-dodge control (roll also moves the player away).
- Task 2: minor (deferred): EnemyBrain gap comment "değilse kenarda dur" describes an unreachable branch (Sign(vx)==face always while approaching; retreat added in T3 makes it reachable).
- Task 2: note: shield block removed with old TickEnemy — owned by Task 3 (shield in Damage).
- Task 2: minor (deferred): dash stop at ledge may overrun slightly (decel 80 vs .34 look-ahead).
- Task 2: complete (commits 66cdd63..fee26d0, review clean)
- Task 3: Ruling: musketeer retreat test runs 2 s instead of the plan's 1.5 s (threshold > 4 kept) — plan arithmetic error: retreat .8 x 1.2 = .96 u/s reaches 3.92 at 1.5 s; spec sets no retreat speed — cost if wrong: retreat slightly slower than intended, tunable in profile.
- Task 3: Ruling: shielded guard multiplier ×2 when heavy OR from behind (not stacking to ×3), per spec §3.2 (plan said 1.5) — spec is binding — cost if wrong: shield breaks slightly faster to heavies.
- Task 3: Ruling: "misses a parry" opening (spec §2.5) = player in Block with the parry window spent (stateTime .14–.5 s) — the game has no separate failed-parry state — cost if wrong: enemies press a blocking player a bit more.
- Task 3: Ruling: execution hit (source.riposting on its riposte target) does not drain guard / re-stagger — reviewer found parry→execution→re-stagger chains until death — cost if wrong: none expected.
- Task 3: minor (deferred): approach/retreat while a spearman is mid-turn moves relative to old facing (cosmetic).
- Task 3: minor (deferred): `front` treats source==null as a back hit (hazards on shielded enemies).
- Task 3: fix round 1/5 (6 addressed, 0 open — shield ×2, cap proof via mutation, missed-parry opening, execution no re-stagger, punish-only during intent, turnTimer reset; commits c3e01aa..6112d1b)
- Task 3: minor (deferred): parry knockback (2 u) pushes the staggered enemy out of execution reach — player must step in; game-feel check for playtest.
- Task 3: minor (deferred): no dedicated tests for punish-only-during-intent (finding 5).
- Task 3: complete (commits fee26d0..6112d1b, review clean)
- Task 4: Ruling: volley ground mark must stay until the arrows land (spec "mark 0.8 s before landing"): mark life .8 → ~1.15 s; and platform-edge test must exercise the backstep (intentTime 0 + assert backstep fired) — both carried into Task 5's dispatch as small extra items (same files) — cost if wrong: none.
- Task 4: minor (deferred): volley knockback always +x (falling arrows have vx 0).
- Task 4: minor (deferred): volley arrows spawn at y+5 with no visible lob from the archer.
- Task 4: minor (deferred): BuildTools RANGED_PASS text still says "held-weapon origin"; enemy shots originate from position offset.
- Task 4: note: Verify wrapper exit not seen by implementer — Task 6 reruns full Verify.
- Task 4: complete (commits 6112d1b..faee1b3, review clean)
- Task 5: minor (deferred): commit trailer says Sonnet 5.5 instead of the session's attribution (cosmetic; fix on squash/merge if wanted).
- Task 5: Ruling: sorcerer slam fires at random when the player is within 1.5 u (teleport has priority while off cooldown) — accepted as "cornered" approximation; spec gives no stricter trigger — cost if wrong: slam slightly more frequent than intended.
- Task 5: Ruling: pit-guard teleport test must also assert the teleport happened — carried into Task 6 dispatch (test-only).
- Task 5: minor (deferred): Standable ignores height (all non-one-way surfaces are floor-level today; revisit when levels gain raised solid ground).
- Task 5: minor (deferred): teleport with no standable spot still spends cooldown/i-frames; no ResetContacts after teleport.
- Task 5: minor (deferred): flame waves travel straight at the caster's y (ignore terrain); 2nd wave origin fixed at release.
- Task 5: minor (deferred): ToUpperInvariant on Turkish title in phase notify; sorcerer shows both boss bar and small widget — check in captures.
- Task 5: complete (commits faee1b3..893a655, review clean)
- Task 6: Ruling: batch Review/chapter-*.png were zoomed-in mosaics — cause: controller's earlier LedgeShot (BuildTools.VerifyLedge, commit 9d58899 era) sets gameCamera.orthographicSize = 2.2 and never restores it; CaptureChapters does not set it — fix in Task 6 (restore ortho in LedgeShot + set ortho in CaptureChapters) — cost if wrong: none.
- Task 6: Ruling: spec §5 "stronger Sorcerer" in Karanlık Kale is missing (plan-mandated gap) — spec is binding: fold into the final fix wave as health ×1.3, guard ×1.2 for a half-boss spawned in zone 3 (+ assertion in Layouts) — cost if wrong: one profile multiplier to tune.
- Task 6: minor (deferred): CaptureAnimation pose gallery sets orthographicSize 1.65 / photoMode without restoring (harmless today, last capture).
- Task 6: minor (deferred): Layouts test checks roles/count only, not that each spawn stands on a surface.
- Task 6: minor (deferred): TASK_BOARD T-009 cites the pre-fix Verify log; post-fix is Verify-20261012-060254.log.
- Task 6: complete (commits 893a655..e3b5ac0, review clean)
- Task 7: Ruling: commit trailers name the model that did the work (Sonnet/Haiku) instead of Opus — accepted, attribution is still Claude; can be normalised on squash — cost if wrong: cosmetic.
- Task 7: Ruling: fix round adds Kurtbaş walk 8, Kara Murad phase-2 art (pala-throw transition 5, bare-handed idle 4 + walk 8), a 6-frame boss death description, neutral smear wording for red attacks, "Kör Demir" spelling — expected counts become 93 prompts / 186 fences — cost if wrong: a few extra sheets for the user.
- Task 7: fix round 1/5 (5 addressed, 0 open; commits 9e933ad..26e7bb6)
- Task 7: minor (deferred → final fix wave): Kara Murad phase-2 bare-hand prompts inherit "giant two-handed pala" in the description while the action says "NO sword" — contradictory for ChatGPT.
- Task 7: minor (deferred): Nefer combo prompt still says "white motion smear" (fine: combo is white).
- Task 7: complete (commits e3b5ac0..26e7bb6, review clean after 1 fix round)

## Final review (opus, 9d58899..26e7bb6): Ready to merge — With fixes
- Final: Ruling: fix #1 (parry leaves guard 0 → parry/execution/hit/second execution kills half-boss) by routing parry through Stagger (guard reset, knockback 0) — also resolves T3 "knockback pushes out of execution reach" — cost if wrong: executions need the player to stay close (none expected).
- Final: Ruling: #2 add profile field parryStagger (default .9, Sorcerer 1.4) per spec §3.6.
- Final: Ruling: #3 Spearman bash: push to 1.5 u and stamina drain, applied on block too, per spec §3.2.
- Final: Ruling: #4 musketeer stays "empty" after firing even if the reload is interrupted (aim ready time = shot + recovery + reload) per spec §3.4.
- Final: Ruling: #5 tests: dodge-through (roll toward) sweep, red vs timed parry, data-driven every-melee/slam attack window test.
- Final: Ruling: #6 poise — spec §2 wants "few but dangerous" and §3.3/§3.6 rely on wind-up/recovery windows: add profile flag `poise` (Halberdier, Spearman, Sorcerer): hits during their own attack deal damage and drain guard but do not interrupt; only a guard break staggers. Nefer/Musketeer/Archer/Captain stay interruptible — cost if wrong: one flag per profile to flip after playtest; user may revisit.
- Final: Ruling: #7 prompts follow the user's phenotype rule (fair skin, almond eyes, chestnut/kumral hair, moustache and beard; no black/dark hair) for janissaries and all four aghas; drop the "long cloth flap hanging down the back" wording.
- Final: Ruling: #8 stronger Sorcerer in Karanlık Kale via Spawn zone multiplier (health ×1.3, guard ×1.2) + Layouts assertion; Kara Murad phase-2 prompts use a separate pala-less description; add Arslan phase-change prompt (setting the standard alight) + burning-standard idle.
- Final: Ruling: also fix now: minimap markers per enemy count (boss marker missing in 7-enemy zones), ranged enemies never jump off/approach past a platform edge, slam push/direction toward the player's side, ResetContacts after teleport, remove dead enemy TryBlade branch, a normal hit never shortens an active longer stagger, Halberdier shove recovery .8 (spec §3.3), Spearman shows its block clip briefly on a shield hit, VerifyCharacters Sorcerer-skip TODO, RANGED_PASS text, TASK_BOARD T-009 with final log paths and status.
- Final: Ruling: hitGap .45 for 2-hit combos (spec says .22) kept — player's .4 s i-frames would swallow the second hit at .22 — cost if wrong: combos a bit slower than the spec text; note added to spec.
- Final: deferred (can wait, by reviewer triage): projectile speeds/teleport offsets/awake ranges into the profile table; boss bar phase indicator; pistol guard-break balance; Captain shield; Standable height; teleport with no spot; flame waves ignore terrain; volley lob visual; ghoul variant; boss hardcodes; Profiles test coverage; dash ledge overrun; turn-facing movement; null-source front.
- Final: fix wave (commits 26e7bb6..e1aca51): 23 items + sorcerer phenotype; all checks green (Enemies-20261012-121433, Verify-20261012-121801, Build-20261012-121927, VERIFY_WINDOWS 10/10).
- Final: re-review (opus) — all 23 items + sorcerer prompt ADDRESSED; ready to merge.
- Final: parked — bash push lands 1.44 u instead of 1.5 (pushTime decremented before first apply) — Ruling: real, tiny; tune with playtest — cost: 4% shorter push.
- Final: parked — push overrides dodge movement for up to 0.2 s — Ruling: real, minor feel; revisit in playtest.
- Final: parked — item-6 test lacks a health assertion; slam-behind and bash-on-parry untested — Ruling: covered by RED evidence; add later.
- Final: parked — spearman airborne Block can drop him into a pit (rare, same as Idle) — Ruling: deferred.
- Final: parked — parrying a dead archer's arrow re-staggers the corpse (pre-existing) — Ruling: add !source.Dead guard later.
