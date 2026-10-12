# Boss Aghas (4 zone bosses) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the placeholder `Captain` boss with the four human agha bosses of spec §4 (Bayraktar Arslan, Kör Demir, Deli Kurtbaş, Kara Murad), each with its own moves, second phase and tests, built on the merged enemy system.

**Architecture:**
- Bosses are new `EnemyRole`s whose data lives in `EnemyProfiles.cs` like every other enemy.
- New boss mechanics are added to the shared brain (`EnemyBrain.cs`) as generic `AttackKind`s and fields so later bosses (goblins) can reuse them:
  - phase-gated attacks
  - a phase-change attack
  - uneven hit rhythm
  - summons
  - lingering fire
  - arcing grenades
  - hook pull
  - leaps
  - frenzy
- `GameWorld.cs` keeps damage and projectiles, and gains fire patches, grenades and a deferred spawn queue.

**Tech Stack:** Unity 6000.6.0f1, C#; editor batch tests through `Tools/unity_batch.ps1 -Job Enemies` / `-Job Verify`; offline compile through `Tools/compile_check.ps1`.

**Spec:** `KaramaniReborn/Docs/specs/2026-10-12-enemy-roster-design.md` §2 (common rules) and §4 (bosses). Earlier plan and ledger: `Docs/plans/2026-10-12-enemy-roster-plan.md`, `Docs/plans/2026-10-12-enemy-roster-ledger.md`.

## Global Constraints
- **Code comments in Turkish;** match the surrounding file. In-game text is Turkish.
- **Unity:** one instance per project, so one batch job at a time. Never kill a Unity process you did not start. License exit 198 (wrapper exit 4): the user must sign in; never sign in for them.
- **Source paintings:** never edit the paintings in `Assets/Resources/Art/`.
- **Git:** work on a local branch; commit locally per task. Push or merge to `main` only when the user asks (CLAUDE.md).
- **Batch side effects:** test runs regenerate `Assets/Scenes/Karamani.unity` and `Review/*`. Revert the scene before committing and never stage `Review/*` in task commits.
- **Commands (from `KaramaniReborn/`):**
  - `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`, expect `COMPILE_CHECK_SUCCESS`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`, expect `UNITY_BATCH_SUCCESS`
  - `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Verify`, expect `UNITY_BATCH_SUCCESS`
- **Spec rules:**
  - user-approved sizes relative to Karamani: Arslan 1.15, Kör Demir 1.12, Kurtbaş 1.12, Kara Murad 1.6 (bulk 2)
  - a second phase starts at 50% health
  - each boss uses the boss bar with its name
  - ⚪ attacks are blockable and parryable; 🔴 attacks are avoided only by dodge invulnerability or a jump
- **Player's invulnerability:** 0.4 s after a hit, so consecutive hits of one attack must be ≥ 0.42 s apart.
- **Placeholder art:** all four bosses use the `captain` pixel sheet until their HD sheets arrive. Prompts already exist in `Docs/references/enemy-sheets.md`.
- **Boss names (bar and messages):**
  - zone 0: "Bayraktar Arslan"
  - zone 1: "Humbaracıbaşı Kör Demir"
  - zone 2: "Deli Kurtbaş"
  - zone 3: "Yeniçeri Ağası Kara Murad"

## Review Focus
1. **Summoned Nefers over a pit.** Summoned Nefers must only appear on solid ground. Test in Task 2.
2. **Leap landing over a pit or outside the level.** Kurtbaş's leap-over must land on solid ground or fall back to landing in front of the player. Test in Task 4.
3. **Fire patch and a jumping player.** A fire patch must not burn a player jumping over it. Test in Task 3.
4. **Grab while the player attacks or rolls.** Kara Murad must never start a grab while the player is attacking or invulnerable. Test in Task 5.
5. **Boss dies while its summons are alive.** The gate still opens, because `cleared` depends on the boss only. Test in Task 2.

## File Structure
| File | Responsibility |
|---|---|
| `Assets/Scripts/GameTypes.cs` | new roles; `Actor.home`; boss names |
| `Assets/Scripts/EnemyProfiles.cs` | new `AttackKind`s and fields; profiles for the 4 bosses |
| `Assets/Scripts/EnemyBrain.cs` | phase gating, phase change, uneven rhythm, summon, pull, leaps, grenade and arena-fire release, frenzy movement |
| `Assets/Scripts/GameWorld.cs` | spawn queue, boss spawn from profile, fire patches, grenades, frenzy damage |
| `Assets/Editor/EnemyTests.cs` | `Bosses` (common), `Arslan`, `KorDemir`, `Kurtbas`, `KaraMurad`; updated `Layouts` |
| `Assets/Scripts/RuntimeVerification.cs` | unchanged counts; boss role check |
| `Docs/TASK_BOARD.md`, `Docs/DESIGN_CONSTRAINTS.md` | evidence and decisions |

---

### Task 1: Boss foundations (roles, profiles, phase gating, phase change, rhythm, spawn queue, layout)

**Files:**
- Modify: `Assets/Scripts/GameTypes.cs` (enum, `Actor.home`, boss names)
- Modify: `Assets/Scripts/EnemyProfiles.cs` (kinds, fields, 4 boss profiles)
- Modify: `Assets/Scripts/EnemyBrain.cs` (`Allowed`, `StartAttack`, `Choose`, phase change, `InStrike` rhythm, speed, chain facing, summon / rage release)
- Modify: `Assets/Scripts/GameWorld.cs` (`Spawn`, spawn queue, boss layout, frenzy damage)
- Modify: `Assets/Editor/EnemyTests.cs` (`Bosses`, `Layouts`)

**Interfaces:**
- Produces:
  - roles `EnemyRole.Arslan, KorDemir, Kurtbas, KaraMurad`
  - `AttackKind.Summon, Rage, Grenade, ArenaFire, Pull, LeapOver, LeapSlam`
  - `EnemyAttack` fields:
    - `int phase` (0 = both phases)
    - `bool grab`
    - `float[] hitTimes`
    - `EnemyRole summon`, `int summonCount`
    - `float patchWidth, patchLife, patchDamage = 6, radius, hitReach`
  - `EnemyProfile` fields: `string phaseShift`, `float phase2Speed = 1, phase2Taken = 1`, `bool phase2Unstoppable`
  - `Actor.home`
  - `GameWorld.Allowed(Actor, EnemyAttack)` (static)
  - `GameWorld.spawnQueue`
  - `static readonly EnemyRole[] GameWorld.ZoneBosses`

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Bosses(root);` from `Verify` after `Layouts(root);`.

```csharp
        static void Bosses(GameRoot root)
        {
            // Her bölümün sonunda kendi ağası; değerler profilden, boy kullanıcı kararı (Karamani'ye göre), ad çubukta
            EnemyRole[] expect = { EnemyRole.Arslan, EnemyRole.KorDemir, EnemyRole.Kurtbas, EnemyRole.KaraMurad };
            string[] names = { "Bayraktar Arslan", "Humbaracıbaşı Kör Demir", "Deli Kurtbaş", "Yeniçeri Ağası Kara Murad" };
            for (int zone = 0; zone < 4; zone++)
            {
                root.LoadZone(zone, true); var w = root.world; var boss = w.enemies[^1]; var p = EnemyProfile.For(boss.role);
                Assert(boss.boss && boss.role == expect[zone], "Zone " + zone + " boss is " + boss.role);
                Assert(boss.maxHealth == p.health && boss.maxGuard == p.guard && boss.size >= 1.12f && boss.size <= 1.25f, boss.role + " values from profile");
                Assert(w.zone.bossName == names[zone], "Zone " + zone + " boss name " + w.zone.bossName);
                Assert(p.phaseAt == .5f && p.poise && p.phaseShift != null && p.Find(p.phaseShift) != null, boss.role + " needs a phase change at 50% with a transition attack");
            }

            // Evre kapısı: yalnız ikinci evre saldırısı birinci evrede başlamaz; geçişte bekleme süreleri sıfırlanır ve geçiş saldırısı oynar
            var w2 = Arena(root, EnemyRole.KaraMurad, 1.4f, out var e);
            Assert(!w2.StartAttack(e, "fists") && w2.StartAttack(e, "combo"), "Phase 1 must allow only phase-1 attacks");
            e.Enter(ActionState.Idle); e.attackReady["combo"] = 999;
            e.health = e.maxHealth * .45f; Run(w2, .05f);
            Assert(e.phase == 2 && e.attack != null && e.attack.name == EnemyProfile.For(e.role).phaseShift && e.invincible > 0, "Phase change must start the transition attack, invulnerable");
            Assert(!e.attackReady.ContainsKey("combo"), "Phase change must reset cooldowns");
            Assert(w2.StartAttack(e, "fists") && !w2.StartAttack(e, "combo"), "Phase 2 must allow only phase-2 attacks");

            // Düzensiz ritim: Kara Murad'ın 3'lü palası yavaş-hızlı-yavaş (vuruş anları 0, .45, 1.1)
            w2 = Arena(root, EnemyRole.KaraMurad, 1.4f, out e); var pl = w2.player;
            var combo = EnemyProfile.For(EnemyRole.KaraMurad).Find("combo");
            w2.StartAttack(e, "combo");
            Run(w2, combo.windup + .05f); Assert(pl.health == pl.maxHealth - combo.damage, "First pala hit at 0 s, health " + pl.health);
            Run(w2, .35f); Assert(pl.health == pl.maxHealth - combo.damage, "No second hit before .45 s, health " + pl.health);
            Run(w2, .15f); Assert(pl.health == pl.maxHealth - 2 * combo.damage, "Second hit at .45 s, health " + pl.health);
            Run(w2, .45f); Assert(pl.health == pl.maxHealth - 2 * combo.damage, "No third hit before 1.1 s, health " + pl.health);
            Run(w2, .25f); Assert(pl.health == pl.maxHealth - 3 * combo.damage, "Third hit at 1.1 s, health " + pl.health);
            Debug.Log("KARAMANI_BOSS_FOUNDATION_PASS · four aghas per zone, profile values, phase gating + transition, uneven rhythm");
        }
```

In `Layouts`, the last expected role of each zone becomes that zone's boss. Replace `EnemyRole.Captain` in the four `expect` rows with `EnemyRole.Arslan`, `EnemyRole.KorDemir`, `EnemyRole.Kurtbas` and `EnemyRole.KaraMurad` respectively.

- [ ] **Step 2: Run compile to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: FAIL, with error `EnemyRole.Arslan` not found.

- [ ] **Step 3: Write the implementation**

`GameTypes.cs`:
- The enum becomes `public enum EnemyRole { Nefer, Spearman, Halberdier, Musketeer, Archer, Sorcerer, Captain, Arslan, KorDemir, Kurtbas, KaraMurad }`.
- In `Actor`, next to `turnTimer`, add `public float home;   // doğduğu x (boss arenasının merkezi)`.
- The `bosses` array becomes `{ "Bayraktar Arslan", "Humbaracıbaşı Kör Demir", "Deli Kurtbaş", "Yeniçeri Ağası Kara Murad" }`.

`EnemyProfiles.cs`:
- `AttackKind` becomes `{ Melee, Slam, Bullet, Arrow, Volley, FlameWave, Orbs, Reload, Backstep, Teleport, Summon, Rage, Grenade, ArenaFire, Pull, LeapOver, LeapSlam }`.
- In `EnemyAttack`, after `staminaDrain`, add:

```csharp
        // Boss alanları: yalnız bu evrede (0 = iki evre); yakalama (oyuncu saldırmıyor ve dokunulmaz değilken);
        // eşit aralık yerine vuruş anları; çağrı (tür, sayı); arkasında bıraktığı ateş (genişlik, süre, vuruş başı hasar);
        // sıçrayıp çakılmanın etki yarıçapı
        public int phase;
        public bool grab;
        public float[] hitTimes;
        public EnemyRole summon; public int summonCount;
        public float patchWidth, patchLife, patchDamage = 6, radius;
        // Vuruş mesafesi: seçim mesafesinden (reach) farklıysa (uzaktan başlayan hücum yalnız temas edince vurur)
        public float hitReach;
```

- In `EnemyProfile`, after `parryStagger`, add:

```csharp
        // Evre geçişi: canı phaseAt'e inince oynayan saldırı (dokunulmaz); ikinci evrede hız ve alınan hasar çarpanı,
        // sendelemezlik (çılgınlık)
        public string phaseShift;
        public float phase2Speed = 1, phase2Taken = 1;
        public bool phase2Unstoppable;
```

- Add the four boss profiles to `Table` before the closing `};`:

```csharp
            [EnemyRole.Arslan] = new EnemyProfile
            {
                role = EnemyRole.Arslan, title = "Bayraktar Arslan", sheet = "captain", health = 260, guard = 110, speed = 1.4f, keepMin = 1.6f, keepMax = 2.6f, size = 1.15f,
                poise = true, phaseAt = .5f, phaseShift = "ignite",
                attacks = new[]
                {
                    new EnemyAttack { name = "thrust", windup = .45f, active = .15f, recovery = .5f, reach = 2.7f, damage = 18, punish = true },
                    new EnemyAttack { name = "sweep", windup = .6f, active = .25f, recovery = .8f, reach = 3f, damage = 22, red = true, cooldown = 2, phase = 1 },
                    new EnemyAttack { name = "burnsweep", windup = .6f, active = .25f, recovery = .8f, reach = 3f, damage = 24, red = true, cooldown = 2, phase = 2, patchWidth = 2.6f, patchLife = 1.5f },
                    new EnemyAttack { name = "plant", kind = AttackKind.Summon, windup = .8f, active = .05f, recovery = .6f, reach = 7, summon = EnemyRole.Nefer, summonCount = 2, cooldown = 999 },
                    new EnemyAttack { name = "ignite", kind = AttackKind.Rage, windup = 1f, active = .05f, recovery = .3f },
                }
            },
            [EnemyRole.KorDemir] = new EnemyProfile
            {
                role = EnemyRole.KorDemir, title = "Humbaracıbaşı Kör Demir", sheet = "captain", health = 280, guard = 100, speed = 1.2f, keepMin = 3, keepMax = 6.5f, size = 1.12f,
                poise = true, phaseAt = .5f, phaseShift = "ignite",
                attacks = new[]
                {
                    new EnemyAttack { name = "axe", windup = .45f, active = .15f, recovery = .6f, reach = 1.8f, damage = 20, heavy = true, punish = true },
                    new EnemyAttack { name = "hook", kind = AttackKind.Pull, windup = .4f, active = .15f, recovery = .6f, minRange = 2, reach = 4.5f, damage = 8, cooldown = 3 },
                    new EnemyAttack { name = "grenade", kind = AttackKind.Grenade, windup = .5f, active = .05f, recovery = .5f, minRange = 2, reach = 9, damage = 18, red = true, patchWidth = 1.6f, patchLife = 4, cooldown = 2.5f },
                    new EnemyAttack { name = "oil", kind = AttackKind.Grenade, windup = .7f, active = .05f, recovery = .6f, minRange = 3, reach = 9, damage = 10, red = true, patchWidth = 4.5f, patchLife = 5, cooldown = 6 },
                    new EnemyAttack { name = "ignite", kind = AttackKind.ArenaFire, windup = 1f, active = .05f, recovery = .3f },
                }
            },
            [EnemyRole.Kurtbas] = new EnemyProfile
            {
                role = EnemyRole.Kurtbas, title = "Deli Kurtbaş", sheet = "captain", health = 300, guard = 90, speed = 2.6f, keepMin = 1, keepMax = 1.8f, size = 1.12f,
                poise = true, phaseAt = .5f, phaseShift = "howl", phase2Taken = 1.25f, phase2Unstoppable = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "storm", windup = .3f, active = 1.4f, hits = 4, hitGap = .42f, recovery = .5f, reach = 1.6f, damage = 10, punish = true },
                    new EnemyAttack { name = "leapover", kind = AttackKind.LeapOver, active = .7f, recovery = .1f, reach = 2, cooldown = 3 },
                    new EnemyAttack { name = "leapslam", kind = AttackKind.LeapSlam, windup = .4f, active = 1f, recovery = .6f, minRange = 3.5f, reach = 9, radius = 1.6f, damage = 24, red = true, cooldown = 4 },
                    new EnemyAttack { name = "howl", kind = AttackKind.Rage, windup = 1f, active = .05f, recovery = .3f },
                }
            },
            [EnemyRole.KaraMurad] = new EnemyProfile
            {
                role = EnemyRole.KaraMurad, title = "Yeniçeri Ağası Kara Murad", sheet = "captain", health = 380, guard = 140, speed = 1.5f, keepMin = 1, keepMax = 1.8f, size = 1.25f,
                poise = true, phaseAt = .5f, phaseShift = "throwpala", phase2Speed = 1.3f,
                attacks = new[]
                {
                    new EnemyAttack { name = "combo", windup = .45f, active = 1.25f, hitTimes = new[] { 0f, .45f, 1.1f }, recovery = .6f, reach = 1.9f, damage = 18, punish = true, phase = 1 },
                    new EnemyAttack { name = "charge", windup = .5f, active = .9f, recovery = .8f, minRange = 3, reach = 12, hitReach = 1.2f, dash = 8, damage = 26, red = true, push = 6, cooldown = 5, phase = 1 },
                    new EnemyAttack { name = "grab", windup = .35f, active = .15f, recovery = .7f, reach = 1.2f, damage = 30, red = true, heavy = true, grab = true, cooldown = 3, phase = 1 },
                    new EnemyAttack { name = "quake", kind = AttackKind.FlameWave, windup = .7f, active = .05f, recovery = .6f, minRange = 2.5f, reach = 10, damage = 20, red = true, cooldown = 4, phase = 1 },
                    new EnemyAttack { name = "throwpala", kind = AttackKind.Rage, windup = 1f, active = .05f, recovery = .2f },
                    new EnemyAttack { name = "fists", windup = .25f, active = 1f, hitTimes = new[] { 0f, .42f, .84f }, recovery = .4f, reach = 1.3f, damage = 14, punish = true, phase = 2 },
                    new EnemyAttack { name = "charge2", clip = "charge", windup = .4f, active = .9f, recovery = .3f, minRange = 3, reach = 12, hitReach = 1.2f, dash = 8, damage = 26, red = true, push = 6, cooldown = 4, then = "recharge", phase = 2 },
                    new EnemyAttack { name = "recharge", clip = "charge", windup = .35f, active = .9f, recovery = .8f, hitReach = 1.2f, dash = 8, damage = 26, red = true, push = 6, phase = 2 },
                    new EnemyAttack { name = "grab2", clip = "grab", windup = .3f, active = .15f, recovery = .6f, reach = 1.2f, damage = 30, red = true, heavy = true, grab = true, cooldown = 1.5f, phase = 2 },
                }
            },
```

Note: `recharge` has `reach = 0`, so `Choose` never picks it; it runs only as `charge2`'s chain.

`EnemyBrain.cs`:
- Add:

```csharp
        // Evre kapısı: phase 0 iki evrede, 1 / 2 yalnız o evrede
        public static bool Allowed(Actor a, EnemyAttack k) => k.phase == 0 || k.phase == a.phase;
```

- `StartAttack`: `if (k == null) return false;` becomes `if (k == null || !Allowed(a, k)) return false;`.
- `Choose`, at the top of the loop body: `if (!Allowed(a, k)) continue;` and `if (k.grab && (player.Attacking || player.invincible > 0)) continue;   // yakalama yalnız boşta duran oyuncuya`.
- `TickEnemy`, replace the phase line with:

```csharp
            if (p.phaseAt > 0 && a.phase == 1 && a.health <= a.maxHealth * p.phaseAt)
            {
                a.phase = 2; a.attackReady.Clear();   // evreye özel saldırılar (çağrı gibi) bu evrede yeniden açılır
                Notify((a.boss ? zone.bossName : p.title).ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " · İKİNCİ EVRE");
                if (p.phaseShift != null && StartAttack(a, p.phaseShift)) a.invincible = Mathf.Max(a.invincible, a.duration);
            }
```

- In the end-of-attack chain, before `StartAttack(a, next)`, face the player: replace `if (next != null) StartAttack(a, next);` with `if (next != null) { a.face = player.position.x > a.position.x ? 1 : -1; StartAttack(a, next); }`.
- The two movement uses of `p.speed` become `speed` with `float speed = p.speed * (a.phase > 1 ? p.phase2Speed : 1);` declared before `if (!a.Locked && awake)`.
- `MeleeReach`: replace its body's last line with
  `float r = k.hitReach > 0 ? k.hitReach : k.reach; return k.kind == AttackKind.Slam ? Mathf.Abs(d) <= r : d >= -.2f && d <= r + .23f;`
- The slot exemption becomes `choice.kind is AttackKind.Backstep or AttackKind.Teleport or AttackKind.LeapOver || Attackers < MaxAttackers`.
- Replace `InStrike` with:

```csharp
        // Etkin pencere: her vuruşun ilk .12 sn'si; vuruş anları (hitTimes) verilmişse eşit aralık yerine onlar
        static bool InStrike(Actor a, out int strike)
        {
            var k = a.attack; float t = a.stateTime - k.windup;
            if (k.hitTimes != null)
            {
                strike = 0; while (strike + 1 < k.hitTimes.Length && t >= k.hitTimes[strike + 1]) strike++;
                return t >= 0 && t <= k.active && t - k.hitTimes[strike] < .12f;
            }
            strike = k.hits > 1 ? Mathf.Clamp(Mathf.FloorToInt(t / k.hitGap), 0, k.hits - 1) : 0;
            if (t < 0 || t > k.active) return false;
            return k.hits <= 1 || t - strike * k.hitGap < Mathf.Min(.12f, k.hitGap);
        }
```

- In `Release`, add (the kinds `Rage` and `ArenaFire`/`Grenade`/`Summon` reach `Release` through `PerformAttack`'s default branch):

```csharp
                case AttackKind.Summon:
                    // Sancağı diker: iki yanında, yalnız sağlam zeminde birer nefer; döngü bitince doğarlar
                    for (int i = 0; i < k.summonCount; i++)
                    {
                        float x = a.position.x + (i % 2 == 0 ? -2.2f : 2.2f) * (1 + i / 2);
                        if (Standable(x, out float y)) spawnQueue.Add((new Vector2(x, y), k.summon));
                    }
                    break;
                case AttackKind.Rage:
                    effects.GlowBurst(a.position + Vector2.up * 1.4f, new Color(1, .35f, .15f));
                    break;
```

`GameWorld.cs`:
- Add the field `readonly List<(Vector2 at, EnemyRole role)> spawnQueue = new();` and `public static readonly EnemyRole[] ZoneBosses = { EnemyRole.Arslan, EnemyRole.KorDemir, EnemyRole.Kurtbas, EnemyRole.KaraMurad };`.
- Constructor: `Spawn(50.4f, zone.floor, EnemyRole.Captain, true);` becomes `Spawn(50.4f, zone.floor, ZoneBosses[index], true);`.
- `Spawn`: boss values come from the profile. The `Captain` placeholder keeps its old zone-scaled health. Replace the initializer's `health = …, maxGuard = …, size = …` with:

```csharp
                health = role == EnemyRole.Captain ? 210 + zone.index * 20 : p.health, maxGuard = p.guard, size = p.size, home = x,
```

- `Simulate`: right after the enemies `foreach` loop, add:

```csharp
            // Çağrılan düşmanlar döngü bittikten sonra doğar (liste dolaşılırken değiştirilmez)
            foreach (var (at, role) in spawnQueue) Spawn(at.x, at.y, role).intentTime = .4f;
            spawnQueue.Clear();
```

- `Damage`, right after `var profile = …`, add the second-phase damage multiplier:

```csharp
            if (profile != null && target.phase > 1) damage *= profile.phase2Taken;   // çılgınlık: daha çok hasar alır
```

- Replace the `poised` declaration with:

```csharp
            bool unstoppable = profile != null && profile.phase2Unstoppable && target.phase > 1;
            bool poised = profile != null && (unstoppable || profile.poise && target.attack != null && (target.state is ActionState.Light or ActionState.Shoot));
```

- The guard-break line `if (target.guard <= 0) Stagger(target, source);` becomes `if (target.guard <= 0) { if (unstoppable) target.guard = target.maxGuard; else Stagger(target, source); }`.

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run `compile_check` → `COMPILE_CHECK_SUCCESS`; run `-Job Enemies` → `UNITY_BATCH_SUCCESS`, with `KARAMANI_BOSS_FOUNDATION_PASS` and `KARAMANI_ENEMY_LAYOUT_PASS` in the log.
If `VerifyCharacters` (inside `-Job Verify`) rejects a boss's idle height, report it. The placeholder captain sheet is 2.88 tall, so dividing by size 1.12–1.25 gives 2.30–2.57, which is inside 2.25–2.8.

- [ ] **Step 5: Commit**

```bash
git add KaramaniReborn/Assets/Scripts/GameTypes.cs KaramaniReborn/Assets/Scripts/EnemyProfiles.cs KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Editor/EnemyTests.cs
git commit -m "Boss temeli: dört ağa, evre kapısı ve geçişi, düzensiz vuruş ritmi, çağrı kuyruğu"
```

---

### Task 2: Bayraktar Arslan (thrust, red sweep, standard summon, burning standard)

**Files:**
- Modify: `Assets/Scripts/GameWorld.cs` (fire patches: `ShotKind.Fire`, `Projectile.width/tick`, `SpawnFire`, ticking)
- Modify: `Assets/Scripts/EnemyBrain.cs` (melee trail fire)
- Modify: `Assets/Editor/EnemyTests.cs` (add `Arslan`)

**Interfaces:**
- Consumes: Arslan profile, `spawnQueue`, `Allowed`, phase change (Task 1)
- Produces:
  - `ShotKind.Fire`
  - `Projectile.width`, `Projectile.tick`
  - `Projectile SpawnFire(Vector2 ground, float width, float life, float damage, Actor source, float delay = 0)`

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Arslan(root);` from `Verify` after `Bosses(root);`.

```csharp
        static void Arslan(GameRoot root)
        {
            // Direk dürtüşü beyaz: zamanında blok savuşturur
            var w = Arena(root, EnemyRole.Arslan, 2.4f, out var e); var pl = w.player;
            var thrust = EnemyProfile.For(EnemyRole.Arslan).Find("thrust");
            w.StartAttack(e, "thrust"); int press = Mathf.RoundToInt((thrust.windup - .06f) * 120);
            Run(w, thrust.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(pl.health == pl.maxHealth && e.state == ActionState.Hurt, "Arslan's thrust must be parryable");

            // Geniş savuruş kırmızı: blok işe yaramaz
            w = Arena(root, EnemyRole.Arslan, 2.6f, out e); pl = w.player;
            w.StartAttack(e, "sweep"); Run(w, 1.8f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 22, "Red sweep must ignore block, health " + pl.health);

            // Sancak dikme: iki nefer, yalnız sağlam zeminde; bu evrede bir daha olmaz, ikinci evrede yeniden açılır
            w = Arena(root, EnemyRole.Arslan, 3, out e); int before = w.enemies.Count;
            w.StartAttack(e, "plant"); Run(w, .9f);
            var summoned = w.enemies.GetRange(before, w.enemies.Count - before);
            Assert(summoned.Count == 2 && summoned.TrueForAll(n => n.role == EnemyRole.Nefer && n.grounded), "Plant must summon two grounded Nefers, got " + summoned.Count);
            Assert(e.attackReady["plant"] > w.clock + 100, "Plant must be once per phase");
            // Çukur kenarında (13.5-15.7 çukuru): çukurun üstüne doğmaz
            w = Arena(root, EnemyRole.Arslan, 3, out e); e.position.x = 15.9f; before = w.enemies.Count;
            w.StartAttack(e, "plant"); Run(w, .9f);
            foreach (var n in w.enemies.GetRange(before, w.enemies.Count - before)) Assert(!(n.position.x > 13.5f && n.position.x < 15.7f), "Nefer summoned over the pit at " + n.position.x);

            // İkinci evre: sancak yanar; yanan savuruş ardında kısa ateş bırakır, birinci evre savuruşu bırakmaz
            w = Arena(root, EnemyRole.Arslan, 2.6f, out e);
            w.StartAttack(e, "sweep"); Run(w, .9f);
            Assert(!w.projectiles.Exists(s => s.kind == GameWorld.ShotKind.Fire), "Phase-1 sweep must not leave fire");
            e.Enter(ActionState.Idle); e.health = e.maxHealth * .45f; Run(w, 1.5f);
            Assert(e.phase == 2 && !e.attackReady.ContainsKey("plant"), "Phase 2 must reopen the standard summon");
            w.StartAttack(e, "burnsweep"); Run(w, .9f);
            Assert(w.projectiles.Exists(s => s.kind == GameWorld.ShotKind.Fire), "Burning sweep must leave a fire trail");

            // Boss ölünce çağrılan neferler yaşasa da kapı açılır
            w = Arena(root, EnemyRole.Arslan, 3, out e);
            var gateBoss = w.enemies.Find(a => a.boss); w.StartAttack(e, "plant"); Run(w, .9f);
            w.Damage(gateBoss, 5000, 1, true, true); w.Simulate(1f / 120, new InputFrame(), true);
            Assert(w.cleared, "Gate must open when the boss dies even with summons alive");
            Debug.Log("KARAMANI_BOSS_ARSLAN_PASS · parryable thrust, red sweep, summon on ground once per phase, burning standard trail, gate");
        }
```

- [ ] **Step 2: Run compile to verify it fails**

Run: `compile_check` → FAIL, with error `ShotKind.Fire` not found.

- [ ] **Step 3: Write the implementation**

`GameWorld.cs`:
- `ShotKind` becomes `{ Pistol, Arrow, Bullet, VolleyMark, VolleyArrow, Wave, Orb, Fire, Grenade }`.
- `Projectile` gains `public float width, tick; public Vector2 target; public EnemyAttack attack;`.
- Add:

```csharp
        // Yerde yanan ateş: genişlik boyunca, yerden .6 yüksekliğe kadar; her .5 sn'de bir kırmızı hasar (zıplayan yanmaz)
        Projectile SpawnFire(Vector2 ground, float width, float life, float damage, Actor source, float delay = 0)
        {
            var sr = ArtLibrary.Draw("Fire", ArtLibrary.Glow, actorsRoot, ground + Vector2.up * .35f, 172, new Color(1, .45f, .1f, .75f));
            sr.transform.localScale = new Vector3(width, .8f, 1); sr.enabled = delay <= 0;
            var fire = new Projectile { kind = ShotKind.Fire, position = ground, life = life, delay = delay, damage = damage, red = true, width = width, renderer = sr, source = source };
            projectiles.Add(fire); return fire;
        }
```

- In `TickProjectiles`, right after `Vector2 old = p.position; p.position += p.velocity * dt; p.life -= dt;`, add the fire tick and skip the normal hit for fire:

```csharp
                if (p.kind == ShotKind.Fire)
                {
                    p.tick -= dt;
                    bool inside = Mathf.Abs(player.position.x - p.position.x) < p.width * .5f + .2f && player.position.y - p.position.y < .6f && player.position.y >= p.position.y - .1f;
                    if (inside && p.tick <= 0 && !player.Dead) { Damage(player, p.damage, 0, false, false, p.source, -1, true); p.tick = .5f; }
                    if (p.life <= 0) RemoveShot(i);
                    continue;
                }
```

- Also set the fire renderer position at its fixed `+.35` offset: fire is drawn at spawn and never moves, so no change is needed in the render line.

`EnemyBrain.cs`, in `PerformAttack`'s `Melee`/`Slam` case, insert right after the `InStrike` check line and before the strike/hit checks:

```csharp
                    // Yanan sancak: savuruşun ilk etkin anında önüne (boss'un zemininde) kısa ömürlü ateş bırakır
                    if (k.patchLife > 0 && !a.attackConnected)
                    { a.attackConnected = true; SpawnFire(new Vector2(a.position.x + a.face * k.reach * .5f, a.position.y), k.patchWidth, k.patchLife, k.patchDamage, a); }
```

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run `compile_check` → `COMPILE_CHECK_SUCCESS`; run `-Job Enemies` → `UNITY_BATCH_SUCCESS` with `KARAMANI_BOSS_ARSLAN_PASS`.

- [ ] **Step 5: Commit**

```bash
git add KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Editor/EnemyTests.cs
git commit -m "Bayraktar Arslan: dürtüş, kırmızı savuruş, sancak çağrısı, yanan sancak ateşi"
```

---

### Task 3: Humbaracıbaşı Kör Demir (grenade + fire patch, hook pull, oil strip, arena fire)

**Files:**
- Modify: `Assets/Scripts/GameWorld.cs` (grenade gravity and explosion)
- Modify: `Assets/Scripts/EnemyBrain.cs` (`Release` for Grenade and ArenaFire; `PerformAttack` for Pull)
- Modify: `Assets/Editor/EnemyTests.cs` (add `KorDemir`)

**Interfaces:**
- Consumes: `SpawnFire`, `ShotKind.Fire/Grenade`, `Projectile.target/attack` (Task 2); `Actor.home` (Task 1)
- Produces: `void Explode(Projectile p)` (private)

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `KorDemir(root);` after `Arslan(root);`.

```csharp
        static void KorDemir(GameRoot root)
        {
            // Humbara: kavisle düşer, patlama 18 (kırmızı), yerinde 4 sn yanan ateş bırakır
            var w = Arena(root, EnemyRole.KorDemir, 6, out var e); var pl = w.player;
            var g = EnemyProfile.For(EnemyRole.KorDemir).Find("grenade");
            w.StartAttack(e, "grenade"); Run(w, g.windup + .9f + .1f);
            Assert(pl.health == pl.maxHealth - 18, "Grenade blast should deal 18, health " + pl.health);
            var fire = w.projectiles.Find(s => s.kind == GameWorld.ShotKind.Fire);
            Assert(fire != null && Mathf.Abs(fire.width - 1.6f) < .01f && fire.life > 3.5f, "Grenade must leave a 1.6 u fire patch for ~4 s");
            Run(w, 1f); Assert(pl.health < pl.maxHealth - 18, "Standing in the fire must burn");
            // Humbaradan kaçan kurtulur; ateşin üstünden zıplayan yanmaz
            w = Arena(root, EnemyRole.KorDemir, 6, out e); pl = w.player;
            w.StartAttack(e, "grenade");
            Run(w, 1.6f, i => new InputFrame { move = i >= 70 && i < 130 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Leaving the grenade mark should avoid it, health " + pl.health);
            w = Arena(root, EnemyRole.KorDemir, 6, out e); pl = w.player;
            w.SpawnFireForTest(new Vector2(5.5f, w.zone.floor), 1.6f, 4);
            Run(w, 1.2f, i => new InputFrame { move = 1, jump = i == 0, jumpHeld = i < 40 });
            Assert(pl.health == pl.maxHealth || pl.position.x < 4.7f, "Jumping over a fire patch should not burn, health " + pl.health);

            // Kanca: tutarsa oyuncu ayağının dibine çekilir (8 hasar); blokta çekilmez
            w = Arena(root, EnemyRole.KorDemir, 3.5f, out e); pl = w.player;
            var hook = EnemyProfile.For(EnemyRole.KorDemir).Find("hook");
            w.StartAttack(e, "hook"); Run(w, hook.windup + .5f);
            Assert(pl.health == pl.maxHealth - 8 && Mathf.Abs(e.position.x - pl.position.x) < 1.7f, "Hook should pull the player in, gap " + Mathf.Abs(e.position.x - pl.position.x));
            w = Arena(root, EnemyRole.KorDemir, 3.5f, out e); pl = w.player;
            w.StartAttack(e, "hook"); Run(w, hook.windup + .5f, i => new InputFrame { block = true });
            Assert(Mathf.Abs(e.position.x - pl.position.x) > 3f, "A blocked hook must not pull");

            // Yağ küpü: uzun yanan şerit (4.5)
            w = Arena(root, EnemyRole.KorDemir, 6, out e);
            w.StartAttack(e, "oil"); Run(w, .7f + .9f + .1f);
            Assert(w.projectiles.Exists(s => s.kind == GameWorld.ShotKind.Fire && Mathf.Abs(s.width - 4.5f) < .01f), "Oil pot must leave a 4.5 u burning strip");

            // İkinci evre: arenanın yarısı tutuşur, ortada dar bir alan kalır
            w = Arena(root, EnemyRole.KorDemir, 3, out e); pl = w.player; float home = e.home;
            e.health = e.maxHealth * .45f; Run(w, 2.2f);
            Assert(e.phase == 2, "Kör Demir did not enter phase 2");
            pl.position = new Vector2(home + 3, w.zone.floor); pl.invincible = 0; float hp = pl.health; Run(w, 1f);
            Assert(pl.health < hp, "Arena fire must burn beside the boss (home + 3)");
            pl.position = new Vector2(home - 2, w.zone.floor); pl.invincible = 0; hp = pl.health; Run(w, 1f);
            Assert(pl.health == hp || e.Attacking, "The middle strip (home - 2) must stay free of fire");
            Debug.Log("KARAMANI_BOSS_KORDEMIR_PASS · grenade blast + fire patch, dodge, jump over fire, hook pull / blocked, oil strip, arena fire");
        }
```

Add to `GameWorld` (test hook, editor tests only use it):

```csharp
        // Test için: verilen yere doğrudan ateş koyar
        public void SpawnFireForTest(Vector2 ground, float width, float life) => SpawnFire(ground, width, life, 6, null);
```

The middle-strip assertion tolerates the boss attacking the player there (melee damage) via `|| e.Attacking`. The fire must not be the cause. Keep the boss's `intentTime` high (Arena sets 99) so he does not attack.

- [ ] **Step 2: Run compile to verify it fails**

Run: `compile_check` → FAIL, with error `SpawnFireForTest` not found.

- [ ] **Step 3: Write the implementation**

`EnemyBrain.cs`, add to `Release`:

```csharp
                case AttackKind.Grenade:
                {
                    // Kavisli atış: .9 sn'de oyuncunun bulunduğu yere düşer; düşeceği yer önceden kızıl işaretli
                    const float flight = .9f;
                    Vector2 target = new Vector2(player.position.x, player.position.y);
                    var mark = AddShot(ShotKind.VolleyMark, target + Vector2.up * .05f, Vector2.zero, flight, k, a);
                    mark.renderer.transform.localScale = new Vector3(Mathf.Max(1.6f, k.patchWidth), .35f, 1);
                    var v = new Vector2((target.x - hand.x) / flight, (target.y - hand.y) / flight + 10 * flight);
                    var pot = AddShot(ShotKind.Grenade, hand, v, flight, k, a); pot.target = target; pot.attack = k;
                    break;
                }
                case AttackKind.ArenaFire:
                {
                    // Arenanın yarısı tutuşur: boss'un arkası (home+1.5 … +7) ve giriş yanı (home-8 … -5); ortada dar alan kalır.
                    // Ateş .8 sn önce kızıl işaretle belli olur
                    (float from, float to)[] spans = { (a.home + 1.5f, a.home + 7), (a.home - 8, a.home - 5) };
                    foreach (var (from, to) in spans)
                    {
                        var at = new Vector2((from + to) * .5f, zone.floor); float width = to - from;
                        var mark = AddShot(ShotKind.VolleyMark, at + Vector2.up * .05f, Vector2.zero, .8f, k, a);
                        mark.renderer.transform.localScale = new Vector3(width, .35f, 1);
                        SpawnFire(at, width, 999, k.patchDamage, a, .8f);
                    }
                    break;
                }
```

`EnemyBrain.cs`, in `PerformAttack` add before `case AttackKind.Teleport:`:

```csharp
                case AttackKind.Pull:
                {
                    if (a.state != ActionState.Light || !InStrike(a, out _) || a.hitTargets.Contains(player.id)) return;
                    float d = (player.position.x - a.position.x) * a.face;
                    if (d < -.2f || d > k.reach || Mathf.Abs(player.position.y - a.position.y) > 1.2f) return;
                    a.hitTargets.Add(player.id);
                    Damage(player, k.damage, a.face, false, false, a, -1, k.red);
                    // Kanca tuttuysa (blok, savuşturma ya da kaçış değilse) oyuncu ayağının dibine çekilir
                    if (player.state == ActionState.Hurt && !player.Dead) { player.pushTime = .25f; player.pushSpeed = -a.face * Mathf.Max(0, d - 1.1f) / .25f; }
                    return;
                }
```

`GameWorld.cs`:
- `AddShot`'s sprite switch gains `ShotKind.Grenade => (ArtLibrary.Glow, new Color(.55f, .35f, .2f, 1), new Vector3(.3f, .3f, 1)),` before the default.
- In `TickProjectiles`, before `Vector2 old = …`, add `if (p.kind == ShotKind.Grenade) p.velocity.y -= 20 * dt;   // humbara kavisle düşer`.
- In the non-friendly hit branch, grenades must not hit in flight: change `else if (p.kind != ShotKind.VolleyMark && …` to `else if (p.kind is not (ShotKind.VolleyMark or ShotKind.Grenade) && …`.
- Replace the final `if (p.life <= 0) RemoveShot(i);` with `if (p.life <= 0) { if (p.kind == ShotKind.Grenade) Explode(p); RemoveShot(i); }`. Do the same in the loop-top removal line: `if (p.life <= 0) { if (p.kind == ShotKind.Grenade) Explode(p); RemoveShot(i); continue; }`. A grenade can only expire once, because it is removed in the same branch.
- Add:

```csharp
        // Humbara / yağ küpü yere değdi: yakındakine kırmızı patlama, yerinde ateş
        void Explode(Projectile p)
        {
            var k = p.attack; effects.GlowBurst(p.target + Vector2.up * .4f, new Color(1, .5f, .15f));
            if (!player.Dead && Mathf.Abs(player.position.x - p.target.x) < 1.2f && Mathf.Abs(player.position.y - p.target.y) < 1.5f)
                Damage(player, p.damage, (int)Mathf.Sign(player.position.x - p.target.x), false, false, p.source, -1, true);
            if (k != null && k.patchLife > 0) SpawnFire(p.target, k.patchWidth, k.patchLife, k.patchDamage, p.source);
        }
```

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run `compile_check` → `COMPILE_CHECK_SUCCESS`; run `-Job Enemies` → `UNITY_BATCH_SUCCESS` with `KARAMANI_BOSS_KORDEMIR_PASS`. If a timing assertion fails because of a plan arithmetic error, change only the timing (keep the threshold) and report it with numbers.

- [ ] **Step 5: Commit**

```bash
git add KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Editor/EnemyTests.cs
git commit -m "Kör Demir: humbara ve ateş, kanca çekişi, yağ şeridi, arena ateşi"
```

---

### Task 4: Deli Kurtbaş (axe storm, leap over, leap slam, frenzy)

**Files:**
- Modify: `Assets/Scripts/EnemyBrain.cs` (`Leap`, `Begin` leap-over, leap-slam launch and landing, airborne velocity)
- Modify: `Assets/Editor/EnemyTests.cs` (add `Kurtbas`)

**Interfaces:**
- Consumes: Kurtbaş profile, phase change, frenzy damage (Task 1)
- Produces: `void Leap(Actor a, float targetX, float time)` (private)

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Kurtbas(root);` after `KorDemir(root);`.

```csharp
        static void Kurtbas(GameRoot root)
        {
            // Balta fırtınası: 4 vuruş x 10, aralar .42 (oyuncunun .4 sn dokunulmazlığından uzun)
            // Her vuruşta oyuncu ~.2 birim geri savrulur: 1.0'dan başlayınca dördüncü vuruş da menzilde (1.6 + .23)
            var w = Arena(root, EnemyRole.Kurtbas, 1f, out var e); var pl = w.player;
            var storm = EnemyProfile.For(EnemyRole.Kurtbas).Find("storm");
            w.StartAttack(e, "storm"); Run(w, storm.Duration + .05f);
            Assert(pl.health == pl.maxHealth - 40, "Axe storm should land 4 x 10, health " + pl.health);

            // Üstünden atlama: oyuncunun arkasına, sağlam zemine iner
            w = Arena(root, EnemyRole.Kurtbas, 1.5f, out e); pl = w.player;
            w.StartAttack(e, "leapover"); Run(w, 1.2f);
            Assert(e.position.x < pl.position.x && e.grounded, "Leap-over must land behind the player: boss " + e.position.x + " player " + pl.position.x);
            // Arkası çukursa önüne iner (çukur 13.5-15.7)
            w = Arena(root, EnemyRole.Kurtbas, 1.5f, out e); pl = w.player;
            pl.position = new Vector2(16.4f, w.zone.floor); e.position.x = 17.9f; e.face = -1;
            w.StartAttack(e, "leapover"); Run(w, 1.4f);
            Assert(e.grounded && Mathf.Abs(e.position.y - w.zone.floor) < .01f && !(e.position.x > 13.5f && e.position.x < 15.7f), "Leap-over landed in the pit at " + e.position.x);

            // Sıçrayıp çakılma: kırmızı, blok işe yaramaz (24); kaçan kurtulur
            w = Arena(root, EnemyRole.Kurtbas, 6, out e); pl = w.player;
            w.StartAttack(e, "leapslam"); Run(w, 1.8f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 24, "Leap slam must ignore block for 24, health " + pl.health);
            w = Arena(root, EnemyRole.Kurtbas, 6, out e); pl = w.player;
            w.StartAttack(e, "leapslam");
            Run(w, 1.8f, i => new InputFrame { move = i >= 60 && i < 120 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Running out of the slam radius should avoid it, health " + pl.health);

            // Çılgınlık: ikinci evrede vuruşla sendelemez, %25 fazla hasar alır, denge kırılsa da durmaz
            w = Arena(root, EnemyRole.Kurtbas, 1.3f, out e);
            e.health = e.maxHealth * .45f; Run(w, 1.5f);
            Assert(e.phase == 2, "Kurtbaş did not enter phase 2");
            e.Enter(ActionState.Idle); e.invincible = 0; float hp = e.health; e.guard = 5;
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(Mathf.Approximately(e.health, hp - 25) && e.state != ActionState.Hurt && e.guard > 0, "Frenzy: +25% damage, no stagger, guard refills");
            Debug.Log("KARAMANI_BOSS_KURTBAS_PASS · 4-hit storm, leap-over behind + pit fallback, red leap slam + dodge, frenzy");
        }
```

- [ ] **Step 2: Run the enemy job to verify it fails**

Run: `compile_check` → `COMPILE_CHECK_SUCCESS` (the test uses only existing API). Run `-Job Enemies` → `UNITY_BATCH_FAILED` with "Leap-over must land behind the player".

- [ ] **Step 3: Write the implementation**

`EnemyBrain.cs`:
- Add:

```csharp
        // Sıçrayış: time sn'de targetX'e aynı yüksekliğe iner (yerçekimi 20: dikey hız 10 x time)
        void Leap(Actor a, float targetX, float time)
        {
            a.velocity = new Vector2((targetX - a.position.x) / time, 10 * time); a.grounded = false; a.view.ResetContacts();
        }
```

- In `Begin`, after the teleport line, add:

```csharp
            if (k.kind == AttackKind.LeapOver)
            {
                // Oyuncunun üstünden arkasına; arkası sağlam değilse önüne iner
                float side = Mathf.Sign(player.position.x - a.position.x);
                float behind = player.position.x + side * 1.6f, front = player.position.x - side * 1.6f;
                Leap(a, Standable(behind, out _) ? behind : front, k.active);
            }
```

- In `PerformAttack`, add before `case AttackKind.Teleport:`:

```csharp
                case AttackKind.LeapSlam:
                    // Hazırlık bitince oyuncunun bulunduğu yere sıçrar; yere değdiği an çevresine kırmızı çakılma
                    if (a.state != ActionState.Light) return;
                    if (!a.attackConnected && a.stateTime >= k.windup) { a.attackConnected = true; Leap(a, player.position.x, .8f); return; }
                    if (a.attackConnected && a.strike < 0 && a.grounded && a.stateTime > k.windup + .1f)
                    {
                        a.strike = 0; effects.cameraShake = Mathf.Max(effects.cameraShake, .12f);
                        effects.GlowBurst(a.position + Vector2.up * .3f, new Color(1, .8f, .55f));
                        if (Mathf.Abs(player.position.x - a.position.x) <= k.radius && Mathf.Abs(player.position.y - a.position.y) < 1.2f)
                            Damage(player, k.damage, (int)Mathf.Sign(player.position.x - a.position.x), true, false, a, -1, k.red);
                    }
                    return;
                case AttackKind.LeapOver:
                    return;
```

- In `TickEnemy`, the last line (`a.velocity.x = Mathf.MoveTowards(…); KinematicMotor.Step(a, zone, dt);`) becomes:

```csharp
            // Sıçrayışta yatay hız korunur (havada yön değiştirmez)
            bool leaping = k != null && (k.kind is AttackKind.LeapOver or AttackKind.LeapSlam) && !a.grounded;
            if (!leaping) a.velocity.x = Mathf.MoveTowards(a.velocity.x, vx, dt * (k != null && k.dash != 0 ? 80 : 24));
            KinematicMotor.Step(a, zone, dt);
```

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run `compile_check` and `-Job Enemies` → `KARAMANI_BOSS_KURTBAS_PASS`. Timing fixes follow the same rule as Task 3.

- [ ] **Step 5: Commit**

```bash
git add KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Editor/EnemyTests.cs
git commit -m "Deli Kurtbaş: balta fırtınası, üstünden atlama, sıçrayıp çakılma, çılgınlık"
```

---

### Task 5: Yeniçeri Ağası Kara Murad (pala rhythm, charge, grab, quake, bare-handed phase)

**Files:**
- Modify: `Assets/Editor/EnemyTests.cs` (add `KaraMurad`)
- Modify only if a test exposes a gap: `Assets/Scripts/EnemyBrain.cs`

**Interfaces:**
- Consumes: Kara Murad profile, `hitTimes`, `grab` gating, `then` chain facing, `phase2Speed`, `FlameWave` release (Tasks 1 and earlier)
- Produces: none

- [ ] **Step 1: Write the test**

Add to `EnemyTests`. Call `KaraMurad(root);` after `Kurtbas(root);`.

```csharp
        static void KaraMurad(GameRoot root)
        {
            // Omuz hücumu: arenayı boydan boya, kırmızı (blok işe yaramaz) 26
            var w = Arena(root, EnemyRole.KaraMurad, 7, out var e); var pl = w.player;
            w.StartAttack(e, "charge"); Run(w, 2.3f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 26, "Shoulder charge must ignore block for 26, health " + pl.health);

            // Yakalama: yalnız boşta duran oyuncuya; saldıran ya da yuvarlanan oyuncuya hiç başlamaz
            w = Arena(root, EnemyRole.KaraMurad, 1, out e); pl = w.player; e.intentTime = 0;
            bool grabbed = false;
            for (int i = 0; i < 360; i++)
            {
                var input = new InputFrame { light = i % 30 == 0 };
                w.Simulate(1f / 120, input, true);
                if (e.attack != null && e.attack.grab && (pl.Attacking || pl.invincible > 0) && e.stateTime < 1f / 60) grabbed = true;
            }
            Assert(!grabbed, "Grab must never start while the player attacks or is invulnerable");
            w = Arena(root, EnemyRole.KaraMurad, 1, out e); pl = w.player;
            w.StartAttack(e, "grab"); Run(w, 1f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 30, "Grab must ignore block for 30, health " + pl.health);

            // Yere çakma: kırmızı yer dalgası; zıplayan kurtulur
            w = Arena(root, EnemyRole.KaraMurad, 5, out e); pl = w.player;
            w.StartAttack(e, "quake"); Run(w, 2f);
            Assert(pl.health == pl.maxHealth - 20, "Quake wave should hit a grounded player for 20, health " + pl.health);

            // İkinci evre: pala atılır; yumruk kombosu, art arda hücum, daha hızlı yürüyüş
            w = Arena(root, EnemyRole.KaraMurad, 6, out e); pl = w.player;
            e.health = e.maxHealth * .45f; Run(w, 1.4f);
            Assert(e.phase == 2 && !w.StartAttack(e, "combo"), "Phase 2 must drop the pala combo");
            e.Enter(ActionState.Idle);
            w.StartAttack(e, "charge2"); Run(w, EnemyProfile.For(EnemyRole.KaraMurad).Find("charge2").Duration + .05f);
            Assert(e.attack != null && e.attack.name == "recharge", "Phase-2 charge must chain into a second charge");
            w = Arena(root, EnemyRole.KaraMurad, 6, out e); e.health = e.maxHealth * .45f; Run(w, 1.4f);
            e.Enter(ActionState.Idle); e.intentTime = 99; float x0 = e.position.x; Run(w, 1f);
            Assert(x0 - e.position.x > 1f * EnemyProfile.For(EnemyRole.KaraMurad).speed * 1.15f, "Phase 2 must move faster (x1.3)");
            Debug.Log("KARAMANI_BOSS_KARAMURAD_PASS · red charge, grab only on idle player, quake wave, bare-handed phase 2 chain + speed");
        }
```

- [ ] **Step 2: Run the enemy job**

Run `-Job Enemies`. Everything this test needs comes from Task 1. If it passes on the first run, record that in the report as evidence (no RED is expected for a pure-data boss), and run one mutation check: set `charge2.then = null` → the chain assertion must fail; then restore it. If it fails, fix the cause in `EnemyBrain.cs` (not the test) and explain.

- [ ] **Step 3: Commit**

```bash
git add KaramaniReborn/Assets/Editor/EnemyTests.cs KaramaniReborn/Assets/Scripts/EnemyBrain.cs
git commit -m "Kara Murad: hücum, yakalama, yer dalgası, çıplak el evresi testleri"
```

---

### Task 6: Integration, full verification, EXE, docs

**Files:**
- Modify: `Docs/TASK_BOARD.md`, `Docs/DESIGN_CONSTRAINTS.md`
- Modify if needed: `Assets/Scripts/RuntimeVerification.cs`

- [ ] **Step 1: Full verification (one Unity job at a time)**

Run in order from `KaramaniReborn/`:
- `compile_check`
- `-Job Enemies`: all `KARAMANI_ENEMY_*` and `KARAMANI_BOSS_*` PASS
- `-Job Verify`: `KARAMANI_VERIFY_SUCCESS`, `KARAMANI_JOURNEY_PASS` (each zone's boss dies and the gate opens)
- `-Job Build`
- `powershell -NoProfile -ExecutionPolicy Bypass -File VERIFY_WINDOWS.ps1`: 10 PASS

Open `Review/chapter-1..4.png` and `Review/player-combat.png` and describe them. If a pre-existing check fails, fix the cause; never weaken it.

- [ ] **Step 2: Docs**

`Docs/TASK_BOARD.md`: add a T-010 row before `## Backlog from the baseline review`, with the four bosses, the files and the real log paths from Step 1.
`Docs/DESIGN_CONSTRAINTS.md`: in the Enemies line, change "4 human agha bosses (…)" to say they are implemented (`T-010`), still on the placeholder captain sheet until HD sheets arrive.

- [ ] **Step 3: Commit**

```bash
git add KaramaniReborn/Docs/TASK_BOARD.md KaramaniReborn/Docs/DESIGN_CONSTRAINTS.md KaramaniReborn/Assets/Scripts/RuntimeVerification.cs
git commit -m "Ağalar: tam doğrulama, EXE, belgeler"
```
