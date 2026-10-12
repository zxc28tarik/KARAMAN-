# Goblins, Part 2: Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: use superpowers:subagent-driven-development. Each task is described as behaviour plus exact values plus required tests. The implementer designs the code by following the patterns already in `EnemyProfiles.cs`, `EnemyBrain.cs` and `GameWorld.cs`.

**Goal:** Implement Karamani's part 2: three new zones (4, 5, 6), the six regular goblins, two half-bosses and the boss Oymak Beyi Kara Diş. The source is spec `Docs/specs/2026-10-12-goblin-part2-design.md`, and each task names the section it implements.

**Architecture:**
- Goblins are new `EnemyRole`s whose data lives in `EnemyProfiles.cs`.
- New mechanics are generic `AttackKind`s or profile fields in `EnemyBrain.cs` and `GameWorld.cs`, reusing the part-1 systems: fire patches, grenade arcs, volley marks, teleport, pull, leaps, summon queue and phases.
- Goblin tests go in a new file, `Assets/Editor/GoblinTests.cs`. It is called from the same `Enemies` batch job and must log `KARAMANI_GOBLIN_*_PASS` markers.

**Spec:** `KaramaniReborn/Docs/specs/2026-10-12-goblin-part2-design.md`. The common combat rules are in `Docs/specs/2026-10-12-enemy-roster-design.md` §2.

## Global Constraints
- **Code comments in Turkish;** match the surrounding file. In-game text is Turkish, and in-game uppercase uses tr-TR.
- **Unity:** one instance per project, so one batch job at a time. Never kill a Unity process you did not start. License exit 198 (wrapper exit 4): stop and report; the user must sign in.
- **Source paintings:** never edit, re-save or resample the paintings in `Assets/Resources/Art/`.
- **Git:** branch `goblinler`; commit locally per task with Turkish subjects ending in the trailer `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Never push or merge.
- **Batch side effects:** test runs regenerate `Assets/Scenes/Karamani.unity` and `Review/*`. Revert the scene (`git checkout -- Assets/Scenes/Karamani.unity`) before committing, and never stage `Review/*`.
- **Commands (from `KaramaniReborn/`):**
  - `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`, expect `COMPILE_CHECK_SUCCESS`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`, expect `UNITY_BATCH_SUCCESS`, all part-1 markers and the task's new markers
- **Part 1 is frozen:** do not change any part-1 profile value, boss size, zone 0–3 layout or existing test. Every part-1 test must keep passing. The user approves values; never re-tune approved values.
- **Tone (user rule):** goblins are serious, savage, period raiders (1566). No machines, mechanisms or gunpowder for goblins, no trapper, no comic behaviour (no laughing, no fleeing in panic).
- **Combat rules:**
  - ⚪ attacks can be blocked and parried; 🔴 attacks are avoided only by dodge invulnerability or a jump.
  - The player's post-hit invulnerability is 0.4 s, so consecutive hits of one attack must be at least 0.42 s apart.
  - The second phase starts at 50% health.
- **Sizes (spec §1, relative to Karamani = 1.0):**
  - ordinary goblin 0.68
  - Kaba 1.0 height with bulk 2
  - Kara Diş 1.1
  - Kan Şamanı Kırçıl and Domuz Binicisi Çatal: see the Task 4 and Task 5 rulings
- **Placeholder art:** goblins use existing pixel sheets until the user's HD sheets arrive. Bıçakçı, Pusucu and Çömlekçi use `duelist`; Okçu uses `archer`; Kalkanlı uses `shield`; Kaba, Kırçıl, Çatal and Kara Diş use `captain`. The size field scales them.
- **Gold attack pattern:** every new attack has its own named clip, so that HD sheets can drop in later. A missing clip falls back as today.

## Zone plan (spec §6)
| Index | Title | Subtitle | Gate keeper (boss = true) | Others |
|---|---|---|---|---|
| 4 | SİSLİ KORU | Sınırın ötesindeki orman | none: the gate is open, `cleared` is true when there is no boss | Bıçakçı ×4, Okçu ×2, Pusucu ×2 |
| 5 | DERİN MAĞARALAR | Çatlak Kaya'nın inleri | Kan Şamanı Kırçıl (half-boss used as the gate keeper; bar shows his title) | Bıçakçı ×3, Pusucu ×2, Kalkanlı ×2, Çömlekçi ×1, Kaba ×1 |
| 6 | KEMİK TAHTI | Eski maden galerileri | Oymak Beyi Kara Diş | Bıçakçı ×3, Kalkanlı ×2, Okçu ×1, Çömlekçi ×2, Kaba ×1, Domuz Binicisi Çatal (half-boss, mid-zone) |

## Task List

### Task 1: Part 2 foundations (zones 4–6, progression, roles, attackers cap, ambush perches, test harness)
**Spec:** §1, §2, §6, §7.

**Files:** `GameTypes.cs`, `GameWorld.cs`, `GameRoot.cs`, `GameHUD.cs`, `StageArt.cs`, `ArtLibrary.cs` (only index mapping), `PixelPipeline.cs` (only if `SetLook` needs an index map), `EnemyProfiles.cs`, `EnemyBrain.cs`, new `Assets/Editor/GoblinTests.cs`, `Assets/Editor/BuildTools.cs` (call GoblinTests in the Enemies job).

**Requirements:**
1. **Roles:**
   - Add `EnemyRole` entries `Bicakci, ZehirliOkcu, Pusucu, GoblinKalkanli, Comlekci, Kaba, Kircil, Catal, KaraDis`.
   - Mark goblins with a profile flag `goblin = true`.
   - Titles: "Bıçakçı", "Zehirli Okçu", "Pusucu", "Kalkanlı Mızrakçı", "Ateş Çömlekçisi", "Kaba", "Kan Şamanı Kırçıl", "Domuz Binicisi Çatal", "Oymak Beyi Kara Diş".
   - In this task each new role gets a minimal placeholder profile (health and size from the spec, one basic melee attack). Later tasks replace them.
2. **Zones 4–6:**
   - `ZoneDefinition.Create` accepts 0–6. Indices 4–6 use the titles and subtitles in the zone plan.
   - `bossName`: zone 4 "", zone 5 "Kan Şamanı Kırçıl", zone 6 "Oymak Beyi Kara Diş".
   - Each new zone has its own `surfaces` (same ground band, length 57, floor -2.25) with a different platform arrangement. Every pit is at most 2.8 wide.
   - New field `Vector2[] perches` holds the ambush points (spec §2.3): zone 4 has 3, zone 5 has 3, zone 6 has 2. They are 2.2–3.2 above the floor, placed above walkable ground and never above a pit.
   - Part-1 zones get an empty `perches` array. The ambient, fog and accent colours differ per zone: zone 4 green-grey mist, zone 5 cold dark blue-grey cave, zone 6 warm torchlit ochre.
3. **Placeholder backgrounds:**
   - Until zone art exists, map backgrounds as zone 4 → painting 2, zone 5 → painting 3, zone 6 → painting 1, through one index-mapping helper (for example `ZoneDefinition.artIndex`).
   - Never touch the paintings. StageArt's per-zone colour switches must give 4–6 their own haze, mist, shade and shaft colours.
   - StageArt draws a readable perch marker at each perch: a dark branch or ledge silhouette from the existing props, tinted. Ambush spots must be learnable.
4. **Progression:**
   - After zone 3's boss, play continues into zone 4 (`index < 6`); after zone 6, the journey completes.
   - Raise the save and continue clamps to 6. The zone-select menu lists 7 zones; adjust the HUD loops and layout so 7 cards fit at 1920×1080.
   - Add a part banner when entering zone 4: notify "İKİNCİ KISIM · ÇATLAK KAYA" (uppercase in tr-TR).
5. **Spawning:**
   - `ZoneBosses` becomes per-zone with "none" for zone 4. Zone 5's gate keeper is Kırçıl spawned with `boss = true`; zone 6's is Kara Diş.
   - `Layout(4..6)` follows the zone plan. Archers stand on platforms; Pusucu entries are bound to a perch index (see 7).
6. **Attackers cap:** at most 3 goblin attackers at once and at most 2 humans (`MaxAttackers` by faction). Teleport, backstep and leap kinds stay exempt, as today.
7. **Ambush binding (data only here):** add an `Actor.perch` field (`-1` = none). The behaviour comes in Task 3.
8. **Test harness, `GoblinTests.cs`:**
   - `GoblinArena(root, zone, role, gap, out e)` loads zone 4, moves the zone's own enemies to x 58 and the player to x 4, and spawns the role, mirroring `EnemyTests.Arena`.
   - Make `Run` and `Assert` available to both test files without duplicating them, for example by making `EnemyTests.Run` internal and reusing it.
9. **Tests (marker `KARAMANI_GOBLIN_FOUNDATION_PASS`):**
   - zones 4–6 load, with the right titles and the right enemy and boss counts
   - zone 4 is `cleared` at once and its exit works
   - zone 6 → journey complete, zone 3 → zone 4 (test `AdvanceIfRequested`)
   - perches sit over standable ground
   - 3 goblins may attack at once while a 4th waits; humans are still capped at 2

Run compile_check and `-Job Enemies`, then commit "Goblinler: 2. kısım temeli (bölge 4-6, roller, saldırı sırası 3, pusu noktaları)".

### Task 2: Regular goblins A: Bıçakçı, Zehirli Okçu, Goblin Kalkanlı, Kaba
**Spec:** §3.1, §3.2, §3.4, §3.6 and §2.4.

**Requirements:**
1. **Bıçakçı:**
   - Health 30, guard 15, speed 2.2, size 0.68. Circles at keepMin 1.2 / keepMax 2.
   - ⚪ `stab`: a leaping 2-hit dagger stab, reach 1.1, 2 × 8, hit gap 0.45.
   - ⚪ `flank`: rolls past to the player's back (lands behind, standable-checked like the leap-over), then stabs once, damage 8, cooldown 4.
   - Immediately hops back after any attack (reuse the Backstep kind as a `then` chain).
   - Two player light hits (20 + 20) kill it. A parry knocks it down: stagger at least 1.2 s.
2. **Zehirli Okçu:**
   - Health 30, guard 10, speed 1.8, size 0.68, ranged, keeps 6–10.
   - ⚪ `arrow`: damage 8 and applies **poison**, 3 damage per second for 3 s (a new generic actor poison timer; ticks never cause Hurt, just like fire `Burn`; refreshed, not stacked).
   - 🔴 `volley`: 3 arrows with ground marks (reuse Volley).
   - Backstep when the player comes within 2.5. The HUD shows the poisoned state by tinting the player's health bar green while poison is active.
3. **Goblin Kalkanlı:**
   - Health 70, guard 60, speed 1.2, size 0.68, shielded.
   - ⚪ `thrust` reach 2.2, damage 12; ⚪ `bash` (reuse the Spearman pattern).
   - The shield stops frontal light hits. A heavy hit or any hit from behind does **2×** guard damage.
   - **Pair line:** two Kalkanlı within 3 u of each other move side by side at the same speed, as one line (the rear one keeps 0.9 u behind the front one), and neither leaves the line to chase alone.
4. **Kaba:**
   - Health 130, guard 100, speed 1.0, size 1.0, bulk 2, poise (light hits do not stagger; stagger only at guard break).
   - ⚪ `cleaver`: heavy chop, damage 24; blocking it drains 35 stamina.
   - 🔴 `chainsweep`: wide arc, reach 3.2, damage 22.
   - 🔴 `grab`: reach 1.4, damage 26, gated like Kara Murad's (never while the player attacks or is invulnerable).
   - Never retreats.
5. **Heavy hit vs goblins (§2.4):** a player heavy attack knocks down ordinary goblins (Bıçakçı, Okçu, Pusucu, Çömlekçi): stagger 1.5 s. Player attacks already hit every target in reach; add a test proving one heavy hits 3 Bıçakçı at once.
6. **Tests (marker `KARAMANI_GOBLIN_REGULARS_A_PASS`):**
   - each attack's damage and colour
   - Bıçakçı dies to 2 lights and the parry knock-down holds ≥ 1.2 s
   - flank lands behind on standable ground
   - poison totals 8 + 9 and no Hurt is entered by ticks
   - volley marks
   - frontal light hit blocked by the shield while a back hit deals 2× guard
   - the pair stays within 0.9 ± 0.3
   - Kaba not staggered by lights; grab gate
   - heavy hits 3 goblins

Commit "Goblinler: Bıçakçı, Zehirli Okçu, Kalkanlı, Kaba".

### Task 3: Pusucu, Ateş Çömlekçisi, pack courage
**Spec:** §3.3, §3.5, §2.2.

**Requirements:**
1. **Pusucu:**
   - Health 40, guard 20, size 0.68. It waits at its perch (`Actor.perch`), hidden: not targetable, no gravity, rendered dark and low-alpha.
   - When the player is within 2.5 u horizontally, its **eyes glow** (two small bright pixels; a new effect at the eye position) and the sound `rustle` plays. This warning lasts 0.5 s.
   - 🔴 `pounce`: it then leaps onto the player's current x, damage 22. A miss crashes it into the ground, open for 1 s (the 1 s is real recovery during which it does not attack).
   - After landing it never returns to the perch and fights normally: ⚪ `daggers` 3-hit combo, gap 0.45, damage 7 each; quick backstep.
   - A Pusucu bound to a perch that is already gone (killed) does not respawn.
2. **Ateş Çömlekçisi:**
   - Health 35, guard 15, speed 1.6, size 0.68.
   - 🔴 `firepot`: a burning oil pot in an arc (reuse Grenade + fire patch: patch width 2, life 4 s, 6 damage per second), with the landing marked.
   - ⚪ `potsmash`: close smash, damage 10, with a short 0.8 s fire patch of width 1.
   - **On death:** the pots break and its spot ignites (patch width 2.4, life 3 s).
   - **Fire hurts goblins too:** fire patches whose source is a Çömlekçi (or his death patch) burn enemies standing in them (6 per second, through `Burn`), but never the boss (bosses ignore goblin pot fire). Part-1 boss fire keeps its current rule (does not burn enemies).
3. **Pack courage (§2.2):**
   - A **leader** is a living Kaba or Kırçıl within 8 u. Goblins near a leader get attack cooldowns ×0.75 and speed ×1.15.
   - When a leader dies, every goblin within 10 u **regroups**: it pauses 0.6 s, retreats 2–3 u from the player for 1.5 s, then returns to normal behaviour. It never flees off-screen and never laughs.
4. **Tests (marker `KARAMANI_GOBLIN_REGULARS_B_PASS`):**
   - a hidden Pusucu is not hit by player attacks
   - the eye glow comes ≥ 0.45 s before the pounce
   - a dodge during the pounce takes no damage
   - a missed pounce is open for 1 s
   - it never returns to the perch
   - the firepot patch is on the ground with the marked spot
   - the death patch burns an adjacent Bıçakçı but not the boss
   - pack buff applies within 8 u only
   - regroup retreat after the leader's death

Commit "Goblinler: Pusucu, Ateş Çömlekçisi, sürü cesareti".

### Task 4: Half-boss: Kan Şamanı Kırçıl
**Spec:** §4.1. Ruling: size 0.9 (spec says tall and thin among goblins; 0.68 × ~1.3).

**Requirements:**
1. **Stats:** health 200, guard 50, halfBoss, keeps 4–8, size 0.9. As the zone 5 gate keeper his bar shows "Kan Şamanı Kırçıl".
2. **Attacks:**
   - 🔴 `curse`: a black thorn ring at the player's position, radius 1.5, shown by a ground mark, rising 0.8 s after the mark, damage 18.
   - `wolves`: summons 2 translucent **spirit wolves**, a new role `RuhKurdu`, size 0.6. They live 6 s, are fast (speed 3), bite ⚪ for damage 8, have 1 health and dissipate on the timer. Once per phase.
   - `bloodbond`: heals every goblin within 6 u by 25% of their max health (not himself), cooldown 8. Prefer it when a nearby goblin is below 50% health.
   - `fogstep`: teleport through bone fog (reuse Teleport).
   - ⚪ `staff`: 2-hit staff combo, damage 12.
   - 🔴 `repel`: a staff slam push wave, used when cornered (player within 1.2 and his back within 2 u of a wall, edge or pit), push 6, damage 14.
3. **Phase 2 (`blood ritual`, phaseShift):**
   - a red aura (tint)
   - curses come in pairs: the second at the player's position 0.6 s later
   - wolves get speed ×1.3
   - wolves are allowed once more (once per phase)
4. **Tests (marker `KARAMANI_GOBLIN_KIRCIL_PASS`):**
   - curse delay and radius
   - wolves count, life and despawn
   - bloodbond heals others and not himself
   - repel only when cornered
   - phase-2 double curse
   - zone 5's gate opens only after he dies

Commit "Goblinler: Kan Şamanı Kırçıl".

### Task 5: Half-boss: Domuz Binicisi Çatal and his boar
**Spec:** §4.2. **User decision (2026-10-12):** at half health Çatal leaves the boar, and the boar keeps attacking as a separate, easy, killable enemy. Ruling: the mounted size is 1.15 (the boar makes him taller than Karamani); on foot he is 0.72; the riderless boar is a new role `Domuz` with size 0.8 and bulk 1.6.

**Requirements:**
1. **Çatal:** health 180, guard 80. While mounted he has poise (light hits do not stagger him).
2. **Mounted attacks (phase 1):**
   - 🔴 `charge` across the arena: dashes to 1 u past the player's position, clamped to standable ground, then turns and makes one return charge. Damage 24, push 6.
   - ⚪ `lance`: long thrust, reach 2.8, damage 16.
   - 🔴 `rear`: the boar rears and stomps, a shock with radius 2, damage 20.
3. **Dismount (phaseShift at 50% health):**
   - Çatal leaps off (`dismount` clip, 1 s, invulnerable) and lands on standable ground 1.5 u from the boar.
   - From then on he fights on foot: speed 2.4, no poise, ⚪ `spearcombo` (3 hits, gap 0.45, damage 10), backstep.
4. **The boar:**
   - The boar spawns (spawn queue) as a separate enemy at Çatal's position: role `Domuz`, health 45, guard 20, speed 2.6, goblin flag.
   - It is easy: a player heavy hit knocks it down (1.5 s), and it has no poise.
   - Attacks: 🔴 `charge` (dash 5, damage 16, push 5, cooldown 3) and ⚪ `gore` (reach 1.3, damage 10).
   - It is not a pack leader and is not healed by Kırçıl's blood bond.
   - It can be killed. The boss bar keeps showing Çatal.
5. **Placement:** he guards the middle of zone 6 (home around x 30, on ground), not the exit.
6. **Tests (marker `KARAMANI_GOBLIN_CATAL_PASS`):**
   - the charge goes and returns, and never ends in a pit
   - poise while mounted
   - at ≤ 50% health he dismounts: invulnerable for 1 s, then on foot, the spear combo works
   - exactly one boar spawns, on standable ground, and attacks (both its attacks deal their damage)
   - the boar dies in 3 player light hits or fewer (45 health vs 20 per hit) and a heavy hit knocks it down
   - zone 6's gate is unaffected by the deaths of Çatal and the boar (only Kara Diş opens it)

Commit "Goblinler: Domuz Binicisi Çatal ve domuzu".

### Task 6: Boss: Oymak Beyi Kara Diş, phase 1
**Spec:** §5.1 (attacks 1–4), §5.2, §5.3, §5.4 phase 1. The spec's user list is binding.

**Requirements:**
1. **Stats:** health 420, guard 80, speed 3.2, size 1.1, boss of zone 6.
2. **Pala attacks:**
   - ⚪ `combo`: 4 hits with hitTimes {0, .45, .9, 1.35}, damage 12; parrying the **4th** hit staggers him for 1.4 s
   - 🔴 `spin` (keep-away horizontal cut): radius 2.2, damage 20, warned by a 0.35 s crouch windup. Prefer it when the player is within 1 u or after he has taken 2 hits in a row.
   - ⚪ `xdash`: crossed palas, a dash 4 u through the player, ending behind him (standable-checked), damage 16. A parry makes him stumble (stagger 1 s).
   - ⚪ `risingcut`: used when the player is above him (player y > his y + 1.2 and |dx| < 2), damage 16
3. **Movement:**
   - `blink` (backward teleport): 3 u backwards (standable-checked), leaving an ember dust effect, cooldown 3 s. Used when the player closes in or after one of his attacks is parried. After a blink, prefer `skyfire` or `xdash`.
   - 🔴 `triangle` (aerial attack), with one path:
     - (a) jumps back and up
     - (b) hangs 0.25 s with a blade glint, then dives diagonally to the player's position: damage 18
     - (c) bounces up again
     - (d) hangs 0.25 s with a glint, then slams down at the player's new x: damage 22, shock radius 1.2
     - then he is grounded and open for 1.2 s (no attacks)
     - All landing points are standable-checked.
   - **Rock bounce:** zone 6's arena has 2 jump rocks (one-way platforms near x 44 and x 54). He may leap onto one, then do the triangle or dive from there.
4. **Spells:**
   - 🔴 `pull`: a 0.6 s windup with red eyes, then pulls the player up to 6 u toward him (reuse Pull, but with no hit needed: it is a spell). A `spin` or `combo` follows immediately. If the player dodges during the windup, the pull fails and he is open 0.8 s.
   - 🔴 `skyfire`: he raises the palas and stands still for 1 s. 3 fireballs fall at 0.5 s intervals, each aimed at the player's position **at that moment**. Each has a red ground ring shown 0.7 s before impact, damage 16. A constantly moving player avoids all three.
5. **Tests (marker `KARAMANI_GOBLIN_KARADIS_P1_PASS`):**
   - the combo's 4th-hit parry stagger
   - the spin radius and the jump/dodge avoidance
   - xdash ends behind and standable
   - risingcut only when the player is above
   - blink 3 u backwards with cooldown 3
   - the triangle's two dives with glints and the 1.2 s open window
   - pull fails on dodge with 0.8 s open; pull → follow-up
   - skyfire: 3 balls, 0.5 s apart, rings 0.7 s before, re-targeted; a moving player takes 0 damage
   - his bar shows "Oymak Beyi Kara Diş"

Commit "Goblinler: Oymak Beyi Kara Diş, birinci evre".

### Task 7: Kara Diş phase 2, integration, captures, docs
**Spec:** §5.4 phase 2, §5.1 (5).

**Requirements:**
1. **Phase transition:** phaseShift `roar` (kneel and roar, 1 s, invulnerable); chest tattoo flare (tint).
2. **Phase 2:**
   - ⚪ `storm`: 6 accelerating hits with hitTimes {0, .5, .95, 1.37, 1.79, 2.21}, damage 10. The last hit is 🔴 downward, damage 22.
   - the triangle runs two cycles back to back, then the 1.2 s open window
   - skyfire drops 5 balls
   - pull always chains to `combo`
   - blink cooldown 2 s
   - **breathing:** after every 4–5 attacks (random in that range, deterministic per seed) he rests 1.5 s, palas on the ground, without attacking
3. **Integration:** zone 4–6 chapter captures in BuildTools (`Review/chapter-5..7.png`) and a Kara Diş capture `Review/boss-7.png`. Also check the journey across zones 0–6 (each gate opens after its keeper dies; zone 4 opens at once).
4. **Full verification, in order:** compile_check → `-Job Enemies` → `-Job Verify` → `-Job Build` → `VERIFY_WINDOWS.ps1` (all PASS). Open the new captures and describe them in the report.
5. **Docs:**
   - `Docs/TASK_BOARD.md`: add a T-011 row with the evidence.
   - `Docs/DESIGN_CONSTRAINTS.md`: part 2 goblins are implemented, on placeholder sheets.
6. **Tests (marker `KARAMANI_GOBLIN_KARADIS_P2_PASS`):**
   - storm timings and the red last hit
   - double triangle
   - 5 fireballs
   - breathing after 4–5 attacks
   - blink cooldown 2

Commit "Goblinler: Kara Diş ikinci evre, tam doğrulama, belgeler".
