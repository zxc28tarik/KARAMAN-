# Enemy Roster (regular enemies) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the single generic enemy AI with data-driven enemy profiles and behaviour for the 6 regular enemies of Karamani's opening chapter, using placeholder pixel art until the HD sheets arrive. The 6 enemies are Sword Nefer, Shield Spearman, Halberdier, Musketeer, Archer and Sorcerer (half-boss).

**Architecture:**
- **`EnemyProfiles.cs`:** holds one data table (`EnemyProfile`, `EnemyAttack`) per enemy role. Every tuning value lives there.
- **`EnemyBrain.cs`:** a `partial` of `GameWorld`. It owns enemy behaviour: range keeping, attack choice, the 2-attacker limit, punishing openings, executing attacks, projectiles and spells.
- **`GameWorld.cs`:** keeps physics, the player and damage resolution. `Damage` gains red (unblockable) hits, the shield and guard-break stagger.
- **Bosses:** this plan does not cover the 4 bosses. They keep the existing `Captain` placeholder through a `Captain` profile and get their own plan later.

**Tech Stack:** Unity 6000.6.0f1 (built-in RP), C# (no external packages). Editor batch tests through `Tools/unity_batch.ps1`; offline Roslyn compile through `Tools/compile_check.ps1`; Python 3 for the prompt generator.

**Spec:** `KaramaniReborn/Docs/specs/2026-10-12-enemy-roster-design.md`

## Global Constraints
- **Code comments in Turkish;** match the surrounding file. In-game text is Turkish.
- **Unity:** one instance per project, so only one batch job at a time. Never kill a Unity process you did not start. License exit 198 (wrapper exit 4): the user must sign in to Unity Hub; never sign in for them.
- **Source paintings:** never edit, re-save or resample the paintings in `KaramaniReborn/Assets/Resources/Art/`.
- **Git:** commit or push only when the user asks (CLAUDE.md). The "Checkpoint" steps below list the commit to make once the user allows it.
- **Run commands from `KaramaniReborn/`:**
  - compile: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`, expect `COMPILE_CHECK_SUCCESS`
  - enemy tests: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`, expect `UNITY_BATCH_SUCCESS` (added in Task 1)
  - full verification: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Verify`, expect `UNITY_BATCH_SUCCESS`
- **Values:** exactly as in the spec §3. The player's light hit deals 20, the third combo hit 29, the heavy 44.
- **Hit types:**
  - Red (unblockable) attacks ignore block and parry but **not** dodge invulnerability.
  - White attacks are blockable and parryable.
- **Attack order:** at most 2 enemies attacking at once (`GameWorld.MaxAttackers = 2`).
- **Placeholder pixel sheets:** Nefer and Halberdier → `duelist`, Spearman → `shield`, Musketeer and Archer → `archer`, Sorcerer and Captain → `captain`.

## Review Focus
1. **Ranged enemy retreats off a platform edge.** An Archer or Musketeer standing on a platform must stop at the edge, not fall. Test in Task 4.
2. **Sorcerer teleports into a pit.** If the spot behind the player is a pit, he must pick another standable spot. Test in Task 5.
3. **Melee across platform heights.** An enemy on the floor must not start a melee attack at a player standing on a platform above (height difference > 0.8). Test in Task 3.
4. **Interrupted chain.** A Musketeer hit during his aim must not go into the reload chain; he returns to idle after the stagger. Test in Task 4.
5. **Dead player.** When the player is dead, no enemy starts a new attack. Test in Task 3.

## File Structure
| File | Responsibility |
|---|---|
| `Assets/Scripts/EnemyProfiles.cs` (new) | `AttackKind`, `EnemyAttack`, `EnemyProfile` data table for all roles |
| `Assets/Scripts/EnemyBrain.cs` (new) | `partial class GameWorld`: `TickEnemy`, `Choose`, `Begin`, `StartAttack`, `PerformAttack`, `Release`, `Teleport`, `AddShot`, `Attackers`, `BarTarget` |
| `Assets/Scripts/GameWorld.cs` | becomes `partial`; spawn layout; `Damage` (red, shield, stagger); projectiles (`ShotKind`, `TickProjectiles`); player pistol |
| `Assets/Scripts/GameTypes.cs` | new `EnemyRole` values; enemy attack fields on `Actor` |
| `Assets/Scripts/PixelCharacter.cs` | placeholder sheet per profile; enemy attack clip with a fallback clip |
| `Assets/Scripts/Effects.cs` | red glint variant |
| `Assets/Scripts/GameHUD.cs` | ranged check through the profile; boss bar also for the half-boss |
| `Assets/Scripts/RuntimeVerification.cs` | per-zone enemy counts |
| `Assets/Editor/EnemyTests.cs` (new) | all enemy tests |
| `Assets/Editor/BuildTools.cs` | `VerifyEnemiesOnly` entry; `Verify` calls the enemy tests; updated combat/ranged tests |
| `Assets/Editor/ArtValidation.cs` | explicit role list for the character review |
| `Tools/unity_batch.ps1` | `Enemies` job |
| `Tools/enemy_prompts.py` (new) | writes `Docs/references/enemy-sheets.md` (ChatGPT prompts) |

---

### Task 1: Enemy profile table, new roles, fast test job

**Files:**
- Create: `Assets/Scripts/EnemyProfiles.cs`
- Create: `Assets/Editor/EnemyTests.cs`
- Modify: `Assets/Scripts/GameTypes.cs:7`, `:31-58` (enum, Actor fields, `Enter`)
- Modify: `Assets/Scripts/GameWorld.cs:40-54` (spawns, `Spawn`), `:226`, `:231`, `:310`
- Modify: `Assets/Scripts/PixelCharacter.cs:94`
- Modify: `Assets/Scripts/GameHUD.cs:160`
- Modify: `Assets/Editor/ArtValidation.cs:79-84`
- Modify: `Assets/Editor/BuildTools.cs:52-63` (+ new `VerifyEnemiesOnly`), `:145-147`
- Modify: `Tools/unity_batch.ps1:13`, `:21-26`

**Interfaces:**
- Produces:
  - `enum EnemyRole { Nefer, Spearman, Halberdier, Musketeer, Archer, Sorcerer, Captain }`
  - `enum AttackKind { Melee, Slam, Bullet, Arrow, Volley, FlameWave, Orbs, Reload, Backstep, Teleport }`
  - `class EnemyAttack { string name, clip; AttackKind kind; float windup, active, recovery, minRange, reach, damage, dash, push, cooldown, hitGap; int hits; bool red, heavy, punish, first; string then; float Duration; bool Ranged }`
  - `class EnemyProfile { EnemyRole role; string title, sheet; float health, guard, speed, keepMin, keepMax, size, phaseAt, turnDelay; bool halfBoss, shielded, ranged; EnemyAttack[] attacks; EnemyAttack Find(string); static EnemyProfile For(EnemyRole) }`
  - `Actor`: `EnemyAttack attack; int strike, phase; float turnTimer; Dictionary<string,float> attackReady`
  - `public Actor GameWorld.Spawn(float x, float y, EnemyRole role, bool boss = false)`
  - `EnemyTests.Verify(GameRoot)`, `EnemyTests.Arena(...)`, `EnemyTests.Run(...)`

- [ ] **Step 1: Write the failing test**

Create `Assets/Editor/EnemyTests.cs`:

```csharp
using System;
using UnityEngine;

namespace KaramaniReborn.Editor
{
    // Düşman tasarımının (Docs/specs/2026-10-12-enemy-roster-design.md) otomatik denetimi
    static class EnemyTests
    {
        public static void Verify(GameRoot root)
        {
            Profiles(root);
            Debug.Log("KARAMANI_ENEMY_PROFILES_PASS · six regular enemies with spec values and placeholder sheets");
        }

        static void Assert(bool ok, string message) => ArtValidation.Assert(ok, message);

        // Deneme alanı: 0. bölüm, oyuncu x=4'te sağa bakar; bölümün kendi düşmanları uzağa alınır,
        // denenen düşman oyuncunun gap birim önüne konur. intentTime=99: kendiliğinden saldırmaz.
        static GameWorld Arena(GameRoot root, EnemyRole role, float gap, out Actor e)
        {
            root.LoadZone(0, true); var w = root.world;
            foreach (var other in w.enemies) other.position.x = 58;
            w.player.position = new Vector2(4, w.zone.floor); w.player.face = 1; w.player.view.ResetContacts();
            e = w.Spawn(4 + gap, w.zone.floor, role); e.face = gap > 0 ? -1 : 1; e.intentTime = 99; e.view.ResetContacts();
            return w;
        }

        // 120 Hz simülasyon; input kare sırasını alır
        static void Run(GameWorld w, float seconds, Func<int, InputFrame> input = null)
        {
            int steps = Mathf.RoundToInt(seconds * 120);
            for (int i = 0; i < steps; i++) w.Simulate(1f / 120, input?.Invoke(i) ?? new InputFrame(), true);
        }

        static void Profiles(GameRoot root)
        {
            var expected = new (EnemyRole role, float health, float guard, string sheet)[]
            {
                (EnemyRole.Nefer, 70, 35, "duelist"), (EnemyRole.Spearman, 95, 75, "shield"), (EnemyRole.Halberdier, 110, 90, "duelist"),
                (EnemyRole.Musketeer, 55, 30, "archer"), (EnemyRole.Archer, 50, 25, "archer"), (EnemyRole.Sorcerer, 220, 60, "captain")
            };
            foreach (var x in expected)
            {
                Arena(root, x.role, 3, out var e);
                Assert(e.maxHealth == x.health && e.maxGuard == x.guard, x.role + " health/guard " + e.maxHealth + "/" + e.maxGuard);
                Assert(e.view.variant == x.sheet, x.role + " placeholder sheet " + e.view.variant);
                foreach (var k in EnemyProfile.For(x.role).attacks)
                    Assert(!string.IsNullOrEmpty(k.clip) && k.Duration > 0, x.role + "." + k.name + " has no clip or duration");
            }
            Assert(EnemyProfile.For(EnemyRole.Spearman).shielded && EnemyProfile.For(EnemyRole.Sorcerer).halfBoss, "Shield / half-boss flags");
            var aim = EnemyProfile.For(EnemyRole.Musketeer).Find("aim");
            Assert(aim.red && aim.then == "reload", "Musket shot must be red and chain into reload");
        }
    }
}
```

Add the fast entry to `Assets/Editor/BuildTools.cs`, right after `Verify()`:

```csharp
        // Yalnız düşman doğrulaması: düşman işinde tüm Verify'ı (2-3 dk) beklememek için
        public static void VerifyEnemiesOnly()
        {
            Configure();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Enemy verification").AddComponent<GameRoot>(); root.Initialize(true);
            root.menu = false; root.hud.SetMenu(false);
            EnemyTests.Verify(root);
            Debug.Log("KARAMANI_ENEMIES_SUCCESS");
        }
```

In `Verify()`, append `EnemyTests.Verify(root);` after `VerifyRanged(root);` (same line, before `VerifyJourney(root);`).

In `Tools/unity_batch.ps1`:
- Extend the `ValidateSet` on line 13 with `'Enemies'`.
- Add to `$jobs`:

```powershell
    Enemies     = @{ Method = 'KaramaniReborn.Editor.BuildTools.VerifyEnemiesOnly'; Marker = 'KARAMANI_ENEMIES_SUCCESS'; Graphics = $true }
```

- [ ] **Step 2: Run compile to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: FAIL, with errors `EnemyProfile` / `EnemyRole.Nefer` not found.

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/GameTypes.cs`:
- Line 7 becomes `public enum EnemyRole { Nefer, Spearman, Halberdier, Musketeer, Archer, Sorcerer, Captain }`.
- In `Actor`, after `public PixelCharacter view;`, add:

```csharp
        // Düşman saldırısı: yürüyen saldırının tanımı, kaçıncı vuruşta olduğu, evre ve türlere özel sayaçlar
        public EnemyAttack attack;
        public int strike = -1, phase = 1;
        public float turnTimer;
        public readonly System.Collections.Generic.Dictionary<string, float> attackReady = new();
```

- In `Enter`, append `attack = null; strike = -1;` before the closing `}`.

Create `Assets/Scripts/EnemyProfiles.cs`:

```csharp
using System.Collections.Generic;

namespace KaramaniReborn
{
    public enum AttackKind { Melee, Slam, Bullet, Arrow, Volley, FlameWave, Orbs, Reload, Backstep, Teleport }

    // Bir düşman saldırısı. Süreler saniye, mesafeler dünya birimi; oyun dengesi yalnız buradan ayarlanır.
    // Kırmızı (red) saldırı blok ve savuşturmayla durmaz: yalnız kaçışın dokunulmazlığı ya da zıplama kurtarır.
    public sealed class EnemyAttack
    {
        public string name, clip;
        public AttackKind kind = AttackKind.Melee;
        public float windup, active, recovery, minRange, reach, damage, dash, push, cooldown = .8f, hitGap;
        public int hits = 1;
        public bool red, heavy, punish, first;
        public string then;   // bitince hemen başlayan saldırı (tüfekte doldurma)
        public float Duration => windup + active + recovery;
        public bool Ranged => kind is AttackKind.Bullet or AttackKind.Arrow or AttackKind.Volley or AttackKind.FlameWave or AttackKind.Orbs;
    }

    // Düşman türü: değerler, tuttuğu mesafe ve saldırı listesi (Docs/specs/2026-10-12-enemy-roster-design.md §3)
    public sealed class EnemyProfile
    {
        public EnemyRole role;
        public string title, sheet;
        public float health, guard, speed, keepMin, keepMax, size = .98f, phaseAt, turnDelay;
        public bool halfBoss, shielded, ranged;
        public EnemyAttack[] attacks;

        public EnemyAttack Find(string name) { foreach (var a in attacks) if (a.name == name) return a; return null; }
        public static EnemyProfile For(EnemyRole role) => Table[role];

        static EnemyProfile() { foreach (var p in Table.Values) foreach (var a in p.attacks) a.clip ??= a.name; }

        static readonly Dictionary<EnemyRole, EnemyProfile> Table = new()
        {
            [EnemyRole.Nefer] = new EnemyProfile
            {
                role = EnemyRole.Nefer, title = "Kılıç Neferi", sheet = "duelist", health = 70, guard = 35, speed = 1.35f, keepMin = .9f, keepMax = 1.5f,
                attacks = new[]
                {
                    // İkinci vuruş oyuncunun .4 sn'lik dokunulmazlığından sonra iner
                    new EnemyAttack { name = "combo", windup = .35f, active = .6f, hitGap = .45f, hits = 2, recovery = .35f, reach = 1.5f, damage = 14, punish = true },
                    new EnemyAttack { name = "lunge", windup = .3f, active = .2f, recovery = .4f, minRange = 1.8f, reach = 2.6f, dash = 1.2f, damage = 16, cooldown = 2.5f },
                }
            },
            [EnemyRole.Spearman] = new EnemyProfile
            {
                role = EnemyRole.Spearman, title = "Kalkanlı Mızrakçı", sheet = "shield", health = 95, guard = 75, speed = 1.1f, keepMin = 1.4f, keepMax = 2.2f,
                shielded = true, turnDelay = .6f,
                attacks = new[]
                {
                    new EnemyAttack { name = "thrust", windup = .4f, active = .15f, recovery = .45f, reach = 2.4f, damage = 16 },
                    new EnemyAttack { name = "bash", windup = .25f, active = .12f, recovery = .4f, reach = 1.2f, damage = 8, push = 5, punish = true, cooldown = 1.5f },
                }
            },
            [EnemyRole.Halberdier] = new EnemyProfile
            {
                role = EnemyRole.Halberdier, title = "Teberli", sheet = "duelist", health = 110, guard = 90, speed = 1f, keepMin = 1.8f, keepMax = 2.6f,
                attacks = new[]
                {
                    new EnemyAttack { name = "shove", windup = .2f, active = .12f, recovery = .35f, reach = 1f, damage = 6, push = 6, punish = true, first = true, cooldown = 2 },
                    new EnemyAttack { name = "chop", windup = .55f, active = .15f, recovery = .8f, reach = 2.4f, damage = 20, heavy = true },
                    new EnemyAttack { name = "sweep", windup = .6f, active = .2f, recovery = .8f, reach = 2.8f, damage = 22, red = true, cooldown = 1.6f },
                }
            },
            [EnemyRole.Musketeer] = new EnemyProfile
            {
                role = EnemyRole.Musketeer, title = "Tüfekli", sheet = "archer", health = 55, guard = 30, speed = 1.2f, keepMin = 5, keepMax = 9, ranged = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "butt", windup = .25f, active = .12f, recovery = .4f, reach = 1.3f, damage = 12, push = 4, punish = true },
                    new EnemyAttack { name = "aim", kind = AttackKind.Bullet, windup = 1.2f, active = .05f, recovery = .3f, minRange = 2, reach = 12, damage = 35, red = true, then = "reload", cooldown = 0 },
                    new EnemyAttack { name = "reload", kind = AttackKind.Reload, active = 2 },
                }
            },
            [EnemyRole.Archer] = new EnemyProfile
            {
                role = EnemyRole.Archer, title = "Okçu", sheet = "archer", health = 50, guard = 25, speed = 1.3f, keepMin = 6, keepMax = 10, ranged = true,
                attacks = new[]
                {
                    new EnemyAttack { name = "backstep", kind = AttackKind.Backstep, active = .3f, recovery = .1f, reach = 2.5f, dash = -2, first = true, cooldown = 2 },
                    new EnemyAttack { name = "dagger", windup = .25f, active = .12f, recovery = .35f, reach = 1.2f, damage = 9 },
                    new EnemyAttack { name = "arrow", clip = "shoot", kind = AttackKind.Arrow, windup = .45f, active = .05f, recovery = .4f, minRange = 2, reach = 11, damage = 14, cooldown = 1.2f },
                    new EnemyAttack { name = "volley", kind = AttackKind.Volley, windup = .6f, active = .05f, recovery = .5f, minRange = 4, reach = 11, damage = 18, red = true, cooldown = 3 },
                }
            },
            [EnemyRole.Sorcerer] = new EnemyProfile
            {
                role = EnemyRole.Sorcerer, title = "Paşanın Büyücüsü", sheet = "captain", health = 220, guard = 60, speed = 1f, keepMin = 4, keepMax = 8, size = 1,
                halfBoss = true, ranged = true, phaseAt = .5f,
                attacks = new[]
                {
                    new EnemyAttack { name = "teleport", kind = AttackKind.Teleport, windup = .35f, active = .05f, recovery = .35f, reach = 1.8f, first = true, cooldown = 4 },
                    new EnemyAttack { name = "staff", windup = .3f, active = .6f, hitGap = .45f, hits = 2, recovery = .4f, reach = 1.6f, damage = 15, punish = true },
                    new EnemyAttack { name = "slam", kind = AttackKind.Slam, windup = .5f, active = .12f, recovery = .6f, reach = 1.5f, damage = 20, red = true, push = 6, cooldown = 3 },
                    new EnemyAttack { name = "flame", kind = AttackKind.FlameWave, windup = .9f, active = .05f, recovery = .5f, minRange = 2, reach = 12, damage = 22, red = true, cooldown = 3.5f },
                    new EnemyAttack { name = "orbs", kind = AttackKind.Orbs, windup = 1f, active = .05f, recovery = .5f, minRange = 2, reach = 12, damage = 16, red = true, cooldown = 6 },
                }
            },
            // Boss'lar kendi planlarına kadar eski komutan çizimi ve bu basit kılıç / ağır vuruş setiyle
            [EnemyRole.Captain] = new EnemyProfile
            {
                role = EnemyRole.Captain, title = "Komutan", sheet = "captain", health = 210, guard = 95, speed = 1.7f, keepMin = .9f, keepMax = 1.56f, size = 1.09f,
                attacks = new[]
                {
                    new EnemyAttack { name = "slash", clip = "light1", windup = .3f, active = .3f, recovery = .35f, reach = 1.7f, damage = 19, punish = true },
                    new EnemyAttack { name = "heavy", windup = .5f, active = .25f, recovery = .5f, reach = 1.9f, damage = 25, heavy = true, cooldown = 2 },
                }
            },
        };
    }
}
```

In `Assets/Scripts/GameWorld.cs`:
- Replace the six `Spawn(...)` calls in the constructor (lines 40–45) with:

```csharp
            Spawn(10.0f, zone.floor, EnemyRole.Nefer);
            Spawn(18.1f, zone.floor, EnemyRole.Spearman);
            Spawn(25.5f, -.75f, EnemyRole.Archer);
            Spawn(35.1f, zone.floor, EnemyRole.Nefer);
            Spawn(42.3f, zone.floor, EnemyRole.Spearman);
            Spawn(50.4f, zone.floor, EnemyRole.Captain, true);
```

- Replace `Spawn` (lines 48–54) with:

```csharp
        public Actor Spawn(float x, float y, EnemyRole role, bool boss = false)
        {
            var p = EnemyProfile.For(role);
            var a = new Actor { id = enemies.Count + 1, role = role, boss = boss, position = new Vector2(x, y), face = -1,
                health = boss ? 210 + zone.index * 20 : p.health, maxGuard = boss ? 95 : p.guard, size = boss ? 1.09f : p.size, intentTime = .3f + enemies.Count * .11f };
            a.maxHealth = a.health; a.guard = a.maxGuard; new PixelCharacter(a, actorsRoot); enemies.Add(a);
            return a;
        }
```

- Line 226: `a.role == EnemyRole.Ghoul ? 1.9f : a.boss ? 1.7f : 1.35f` becomes `EnemyProfile.For(a.role).speed`.
- Line 231: `bool shield = a.role is EnemyRole.Shield or EnemyRole.Captain;` becomes `bool shield = EnemyProfile.For(a.role).shielded || a.role == EnemyRole.Captain;`.
- Line 219: `a.role == EnemyRole.Archer` becomes `EnemyProfile.For(a.role).ranged`.
- Line 310: `e.role != EnemyRole.Archer` becomes `!EnemyProfile.For(e.role).ranged`.

Other files:
- `Assets/Scripts/PixelCharacter.cs:94`: `variant = actor.player ? "hero" : EnemyProfile.For(actor.role).sheet;`
- `Assets/Scripts/GameHUD.cs:160`: `a.role == EnemyRole.Archer` becomes `EnemyProfile.For(a.role).ranged`.
- `Assets/Editor/ArtValidation.cs:79-84`: replace the role computation with an explicit list:

```csharp
            var actors = new Actor[6];
            EnemyRole[] roles = { EnemyRole.Nefer, EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Archer, EnemyRole.Captain, EnemyRole.Sorcerer };
            for (int i = 0; i < actors.Length; i++)
            {
                var role = roles[i];
                actors[i] = new Actor { player = i == 0, role = role, boss = role == EnemyRole.Captain, position = new Vector2(.4f + i * 1.6f, -2.25f), size = i == 0 ? 1 : EnemyProfile.For(role).size };
                new PixelCharacter(actors[i], parent);
            }
```

- `Assets/Editor/BuildTools.cs:145`: `role == EnemyRole.Duelist` becomes `role == EnemyRole.Nefer`.
- `Assets/Editor/BuildTools.cs:147`: `size = player ? 1 : role == EnemyRole.Captain ? 1.09f : .98f` becomes `size = player ? 1 : EnemyProfile.For(role).size`.

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: `COMPILE_CHECK_SUCCESS` (the editor count now includes `EnemyTests.cs`).
Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_SUCCESS`, and the log contains `KARAMANI_ENEMY_PROFILES_PASS`.

- [ ] **Step 5: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Assets/Scripts/EnemyProfiles.cs KaramaniReborn/Assets/Editor/EnemyTests.cs KaramaniReborn/Assets/Scripts/GameTypes.cs KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/PixelCharacter.cs KaramaniReborn/Assets/Scripts/GameHUD.cs KaramaniReborn/Assets/Editor/ArtValidation.cs KaramaniReborn/Assets/Editor/BuildTools.cs KaramaniReborn/Tools/unity_batch.ps1
git commit -m "Düşman profilleri: yeni türler, veri tablosu, hızlı düşman testi"
```

---

### Task 2: Attack runner (windows, red/white, parry) and basic brain

**Files:**
- Create: `Assets/Scripts/EnemyBrain.cs`
- Modify: `Assets/Scripts/GameWorld.cs`:
  - line 7 → `partial`
  - `:64-70` (Simulate loop)
  - `:206-242` (delete the old `TickEnemy`)
  - `:366-370` (`Damage` signature and block condition)
- Modify: `Assets/Scripts/Effects.cs:101-110` (`Glint`)
- Modify: `Assets/Scripts/PixelCharacter.cs` (`Pick`, `Resolve`, new field `fallback`)
- Modify: `Assets/Editor/EnemyTests.cs` (add `Attacks`)
- Modify: `Assets/Editor/BuildTools.cs:286-290` (`VerifyCombat` parry setup)

**Interfaces:**
- Consumes:
  - `EnemyProfile.For`, `EnemyAttack` (Task 1)
  - `Actor.attack`, `Actor.strike`, `Actor.attackReady` (Task 1)
- Produces:
  - `public bool GameWorld.StartAttack(Actor a, string name)`
  - `void Begin(Actor, EnemyAttack)`
  - `EnemyAttack Choose(Actor, EnemyProfile, float dist, float height)`
  - `void PerformAttack(Actor)`, `static bool InStrike(Actor, out int strike)`
  - `Damage(Actor target, float damage, int direction, bool heavy, bool unblockable, Actor source = null, float stop = -1, bool red = false)`
  - `Effects.Glint(Vector2 point, bool strong, bool red = false)`

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Attacks(root);` from `Verify` after `Profiles(root);`.

```csharp
        static void Attacks(GameRoot root)
        {
            // Kombo: iki vuruş da yalnız etkin pencerede iner (2 x 14)
            var w = Arena(root, EnemyRole.Nefer, 1.2f, out var e); var pl = w.player;
            var combo = EnemyProfile.For(EnemyRole.Nefer).Find("combo");
            Assert(w.StartAttack(e, "combo"), "StartAttack(combo) failed");
            Run(w, combo.windup - .05f);
            Assert(pl.health == pl.maxHealth, "Combo hit before its wind-up ended");
            Run(w, combo.Duration);
            Assert(pl.health == pl.maxHealth - 28, "Nefer combo should land 2 x 14, health " + pl.health);

            // Kırmızı savuruş: blok işe yaramaz
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            w.StartAttack(e, "sweep");
            Run(w, 1.7f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 22, "Red sweep must ignore block, health " + pl.health);

            // Kırmızı savuruş: zamanında yuvarlanma kurtarır
            w = Arena(root, EnemyRole.Halberdier, 2.2f, out e); pl = w.player;
            var sweep = EnemyProfile.For(EnemyRole.Halberdier).Find("sweep");
            w.StartAttack(e, "sweep");
            int press = Mathf.RoundToInt((sweep.windup - .03f) * 120);
            Run(w, 1.7f, i => new InputFrame { move = i >= press && i < press + 2 ? -1 : 0, dodge = i == press });
            Assert(pl.health == pl.maxHealth, "Dodge i-frames should avoid the red sweep, health " + pl.health);

            // Beyaz vuruş: zamanında blok savuşturur, saldıran sendeler, infaz açılır
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); pl = w.player;
            w.StartAttack(e, "combo");
            press = Mathf.RoundToInt((combo.windup - .06f) * 120);
            Run(w, combo.windup + .1f, i => new InputFrame { block = i >= press });
            Assert(pl.health == pl.maxHealth && e.state == ActionState.Hurt && pl.riposte > 0, "Parry of a white attack should stagger and open a riposte");
            Debug.Log("KARAMANI_ENEMY_ATTACKS_PASS · active windows, red ignores block, dodge avoids red, white parry staggers");
        }
```

- [ ] **Step 2: Run compile to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: FAIL, with error `GameWorld.StartAttack` not found.

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/GameWorld.cs`:
- Line 7 becomes `public sealed partial class GameWorld`.
- In `Simulate`, the enemy loop line `if (enemyAI) ResolveBlade(a);` becomes `if (enemyAI) PerformAttack(a);`.
- Delete the old `TickEnemy` method (lines 206–242) completely. Its replacement lives in `EnemyBrain.cs`.
- `Damage`: the signature becomes `public void Damage(Actor target, float damage, int direction, bool heavy, bool unblockable, Actor source = null, float stop = -1, bool red = false)`.
- `Damage`: the block condition `if (!unblockable && frontal && target.state == ActionState.Block)` becomes `if (!unblockable && !red && frontal && target.state == ActionState.Block)`.

`Assets/Scripts/Effects.cs`, replace `Glint` with:

```csharp
        // Saldırı uyarısı: beyaz savuşturulabilir, kırmızı yalnız kaçışla atlatılır
        public void Glint(Vector2 point, bool strong, bool red = false)
        {
            float s = strong ? 1.3f : 1;
            Color core = red ? new Color(1, .22f, .16f, .95f) : new Color(1, .93f, .76f, .95f), ray = red ? new Color(1, .35f, .25f, .95f) : new Color(1, .96f, .86f, .95f);
            Spawn(ArtLibrary.Glow, point, new Vector2(.42f, .42f) * s, core, .22f, 181);
            for (int i = 0; i < 2; i++)
            {
                var p = Spawn(ArtLibrary.White, point, new Vector2(.62f * s, .012f), ray, .2f, 182);
                p.renderer.transform.rotation = Quaternion.Euler(0, 0, i * 90 + 15);
            }
        }
```

`Assets/Scripts/PixelCharacter.cs`:
- Add the field `string fallback = "idle";` next to `string clip = "";`.
- At the very top of `Pick` (before `timing = Timing.Progress; value = a.Progress;`), add:

```csharp
            // Düşman saldırıları: tanımdaki klip, saldırının ilerleyişiyle; yer tutucu sayfada yoksa genel saldırı / atış karesi
            fallback = "idle";
            if (!a.player && a.attack != null && (a.state is ActionState.Light or ActionState.Shoot or ActionState.Dodge))
            {
                timing = Timing.Progress; value = a.Progress;
                fallback = a.state == ActionState.Light ? "light1" : a.state == ActionState.Shoot ? "shoot" : "hurt";
                return a.attack.clip;
            }
```

- In `Resolve`, the last line `return "idle";` becomes `return pixel.clips.ContainsKey(fallback) ? fallback : "idle";`.

Create `Assets/Scripts/EnemyBrain.cs`:

```csharp
using UnityEngine;

namespace KaramaniReborn
{
    // Düşman davranışı: mesafe, saldırı seçimi ve saldırının yürütülmesi. Değerler EnemyProfiles.cs'te.
    public sealed partial class GameWorld
    {
        // Adıyla saldırı başlatır (testler ve zincirleme saldırılar için)
        public bool StartAttack(Actor a, string name)
        {
            var k = EnemyProfile.For(a.role).Find(name);
            if (k == null) return false;
            Begin(a, k); return true;
        }

        void Begin(Actor a, EnemyAttack k)
        {
            var state = k.kind switch { AttackKind.Reload => ActionState.Shoot, AttackKind.Backstep or AttackKind.Teleport => ActionState.Dodge, _ => ActionState.Light };
            a.Enter(state, k.Duration); a.attack = k; a.combo = 1;
            a.attackReady[k.name] = clock + k.cooldown * (a.phase > 1 ? .6f : 1);
            if (k.kind == AttackKind.Teleport) a.invincible = k.Duration;
            if (state == ActionState.Light && !k.Ranged) sound?.Invoke(k.heavy || k.red ? "heavy" : "swing");
        }

        void TickEnemy(Actor a, float dt)
        {
            if (a.Dead) { SettleCorpse(a, dt); return; }
            var p = EnemyProfile.For(a.role);
            a.intentTime -= dt;
            var k = a.attack;
            // Hazırlığın başında silahta parıltı: beyaz savuşturulabilir, kırmızı yalnız kaçışla atlatılır
            if (k != null && a.state == ActionState.Light && !a.telegraphed && a.stateTime >= k.windup * .25f)
            { a.telegraphed = true; effects.Glint(a.view.BladeTip, a.boss || p.halfBoss, k.red); }
            if (a.Locked && a.stateTime >= a.duration)
            {
                string next = a.state == ActionState.Hurt ? null : k?.then;
                a.Enter(ActionState.Idle);
                if (next != null) StartAttack(a, next); else a.cooldown = a.boss ? .46f : .7f;
            }
            float dx = player.position.x - a.position.x, dist = Mathf.Abs(dx), height = Mathf.Abs(player.position.y - a.position.y);
            bool awake = !player.Dead && dist < (a.boss || p.halfBoss ? 10 : 8.6f) + (p.ranged ? 3 : 0);
            float vx = 0;
            if (!a.Locked && awake)
            {
                a.face = dx > 0 ? 1 : -1;
                var choice = a.intentTime <= 0 ? Choose(a, p, dist, height) : null;
                if (choice != null && a.cooldown <= 0 && attackDirector <= 0) { Begin(a, choice); attackDirector = .26f; a.cooldown = .8f; }
                else if (dist > p.keepMax) { vx = a.face * p.speed; a.state = ActionState.Move; }
                else a.state = ActionState.Idle;
                if (vx != 0 && a.grounded && !GroundAhead(a, Mathf.Sign(vx) * .34f))
                {
                    // Önde boşluk: oyuncuya doğruysa atla, değilse kenarda dur
                    if (Mathf.Sign(vx) == a.face && dist > 2) { a.velocity.y = 8.4f; a.grounded = false; a.view.ResetContacts(); }
                    else { vx = 0; a.state = ActionState.Idle; }
                }
            }
            k = a.attack;
            if (k != null && (a.state is ActionState.Light or ActionState.Dodge) && k.dash != 0 && a.stateTime >= k.windup && a.stateTime <= k.windup + k.active)
            {
                vx = a.face * k.dash / Mathf.Max(.05f, k.active);
                if (a.grounded && !GroundAhead(a, Mathf.Sign(vx) * .34f)) vx = 0;   // atılış / geri sıçrama kenardan taşmaz
            }
            if (a.state == ActionState.Hurt) vx = a.knockback * (1 - a.Progress);
            a.velocity.x = Mathf.MoveTowards(a.velocity.x, vx, dt * (k != null && k.dash != 0 ? 80 : 24)); KinematicMotor.Step(a, zone, dt);
        }

        // Mesafeye ve yüksekliğe uyan, beklemesi dolmuş saldırılardan biri; 'first' işaretli olan önceliklidir
        EnemyAttack Choose(Actor a, EnemyProfile p, float dist, float height)
        {
            EnemyAttack pick = null; int seen = 0;
            foreach (var k in p.attacks)
            {
                if (k.reach <= 0 || dist < k.minRange || dist > k.reach) continue;
                if (height > (k.Ranged ? 3 : .8f)) continue;
                if (a.attackReady.TryGetValue(k.name, out float ready) && clock < ready) continue;
                if (k.first) return k;
                if (rng.Next(++seen) == 0) pick = k;
            }
            return pick;
        }

        // Etkin pencere: çok vuruşlu saldırıda her vuruşun ilk .12 sn'si
        static bool InStrike(Actor a, out int strike)
        {
            var k = a.attack; float t = a.stateTime - k.windup;
            strike = k.hits > 1 ? Mathf.Clamp(Mathf.FloorToInt(t / k.hitGap), 0, k.hits - 1) : 0;
            if (t < 0 || t > k.active) return false;
            return k.hits <= 1 || t - strike * k.hitGap < Mathf.Min(.12f, k.hitGap);
        }

        void PerformAttack(Actor a)
        {
            var k = a.attack;
            if (k == null || a.Dead || a.state == ActionState.Hurt) return;
            switch (k.kind)
            {
                case AttackKind.Melee: case AttackKind.Slam:
                    if (a.state != ActionState.Light || !InStrike(a, out int strike)) return;
                    if (strike != a.strike) { a.strike = strike; a.hitTargets.Clear(); }
                    if (a.hitTargets.Contains(player.id) || !MeleeReach(a, k)) return;
                    a.hitTargets.Add(player.id);
                    Damage(player, k.damage, a.face, k.heavy, false, a, -1, k.red);
                    if (k.push > 0 && player.state == ActionState.Hurt) player.knockback = a.face * k.push;
                    return;
                default: return;
            }
        }

        bool MeleeReach(Actor a, EnemyAttack k)
        {
            if (Mathf.Abs(player.position.y - a.position.y) > 1.2f) return false;
            float d = (player.position.x - a.position.x) * a.face;
            return k.kind == AttackKind.Slam ? Mathf.Abs(d) <= k.reach : d >= -.2f && d <= k.reach + .23f;
        }
    }
}
```

`Assets/Editor/BuildTools.cs` `VerifyCombat`: the parry block (lines 286–290) currently enters `Light` without an attack definition. Replace:

```csharp
            target.Enter(ActionState.Light, .7f); target.stateTime = .345f;
            w.Simulate(1f / 120, new InputFrame { block = true }, true);
```

with:

```csharp
            target.intentTime = 99; w.StartAttack(target, "combo");
            target.stateTime = EnemyProfile.For(target.role).Find("combo").windup - .01f;
            w.Simulate(1f / 120, new InputFrame { block = true }, true);
            w.Simulate(1f / 120, new InputFrame { block = true }, true);
```

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: `COMPILE_CHECK_SUCCESS`.
Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_SUCCESS`; the log contains `KARAMANI_ENEMY_PROFILES_PASS` and `KARAMANI_ENEMY_ATTACKS_PASS`.

- [ ] **Step 5: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/Effects.cs KaramaniReborn/Assets/Scripts/PixelCharacter.cs KaramaniReborn/Assets/Editor/EnemyTests.cs KaramaniReborn/Assets/Editor/BuildTools.cs
git commit -m "Düşman saldırı yürütücüsü: etkin pencere, kırmızı/beyaz, savuşturma"
```

---

### Task 3: Behaviour (retreat, 2-attacker limit, punish, shield, turn delay, guard stagger)

**Files:**
- Modify: `Assets/Scripts/EnemyBrain.cs` (`TickEnemy` choice block; new `Face`, `Attackers`, `MaxAttackers`)
- Modify: `Assets/Scripts/GameWorld.cs` `Damage` (shield, guard stagger) and new `Stagger`
- Modify: `Assets/Editor/EnemyTests.cs` (add `Behaviour`)

**Interfaces:**
- Consumes: `Begin`, `Choose`, `StartAttack` (Task 2)
- Produces: `public const int GameWorld.MaxAttackers = 2`, `public int GameWorld.Attackers`, `void Stagger(Actor target, Actor source)`

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Behaviour(root);` from `Verify` after `Attacks(root);`.

```csharp
        static void Behaviour(GameRoot root)
        {
            // Mesafe: tüfekli yakındaki oyuncudan uzaklaşır, nefer uzaktaki oyuncuya yaklaşır
            var w = Arena(root, EnemyRole.Musketeer, 2.5f, out var e);
            Run(w, 1.5f);
            Assert(e.position.x - w.player.position.x > 4f, "Musketeer did not retreat: gap " + (e.position.x - w.player.position.x));
            w = Arena(root, EnemyRole.Nefer, 6, out e);
            Run(w, 4);
            Assert(e.position.x - w.player.position.x < 1.6f, "Nefer did not close in: gap " + (e.position.x - w.player.position.x));

            // Sıra: aynı anda en fazla iki düşman saldırır
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.intentTime = 0;
            foreach (float gap in new[] { -1.2f, 1.4f, -1.4f }) w.Spawn(4 + gap, w.zone.floor, EnemyRole.Nefer).intentTime = 0;
            int most = 0;
            for (int i = 0; i < 360; i++) { w.Simulate(1f / 120, new InputFrame { block = true }, true); most = Mathf.Max(most, w.Attackers); }
            Assert(most >= 1 && most <= GameWorld.MaxAttackers, "Simultaneous attackers " + most);

            // Açık cezalandırma: iksir içen oyuncuya bekleyen nefer hemen saldırır
            w = Arena(root, EnemyRole.Nefer, 1.3f, out e); e.intentTime = 0; e.cooldown = 5; w.player.health = 50;
            Run(w, .3f, i => new InputFrame { heal = i == 0 });
            Assert(e.state == ActionState.Light, "Nefer did not punish the potion");

            // Yükseklik: platformdaki oyuncuya yerdeki nefer yakın saldırı başlatmaz
            w = Arena(root, EnemyRole.Nefer, 1.0f, out e); e.intentTime = 0;
            w.player.position = new Vector2(8.5f, -.9f); e.position.x = 8.5f + 1f;
            Run(w, 1f);
            Assert(e.state != ActionState.Light, "Nefer attacked a player on a platform above");

            // Ölü oyuncu: yeni saldırı başlamaz
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.intentTime = 0;
            w.Damage(w.player, 1000, 1, true, true);
            Run(w, 1f);
            Assert(w.Attackers == 0, "Enemy attacked a dead player");

            // Kalkan: önden hafif vuruş can götürmez, ağır vuruş götürür, arkadan vuruş dengeyi iki kat düşürür
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e);
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.health == e.maxHealth && e.guard < e.maxGuard, "Shield did not stop a frontal light hit");
            e.Enter(ActionState.Idle); e.invincible = 0; float hp = e.health;
            w.Damage(e, 44, 1, true, false, w.player);
            Assert(e.health < hp, "Heavy hit should pass the shield");
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e); e.face = 1;   // sırtı oyuncuya dönük
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.health == e.maxHealth - 20 && Mathf.Approximately(e.guard, e.maxGuard - 40), "Back hit should hurt and drain guard twice: guard " + e.guard);

            // Denge kırılınca uzun sendeleme ve infaz fırsatı
            w = Arena(root, EnemyRole.Nefer, 1.2f, out e); e.guard = 10;
            w.Damage(e, 20, 1, false, false, w.player);
            Assert(e.state == ActionState.Hurt && e.duration > .8f && w.player.riposte > 0, "Guard break should stagger for 0.9 s and open a riposte");

            // Dönüş gecikmesi: oyuncu arkasına geçince kalkanlı geç döner
            w = Arena(root, EnemyRole.Spearman, 1.3f, out e); e.face = -1;
            w.player.position.x = e.position.x + 1.5f;
            Run(w, .3f); Assert(e.face == -1, "Spearman turned too fast");
            Run(w, .5f); Assert(e.face == 1, "Spearman never turned");
            Debug.Log("KARAMANI_ENEMY_BEHAVIOUR_PASS · range, two attackers, punish, height, dead player, shield, guard break, turn delay");
        }
```

- [ ] **Step 2: Run compile to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: FAIL, with errors `GameWorld.Attackers` / `MaxAttackers` not found.

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/EnemyBrain.cs`. Add these members:

```csharp
        public const int MaxAttackers = 2;

        // Şu anda saldırı yürüten (hazırlık dahil) düşman sayısı
        public int Attackers { get { int n = 0; foreach (var e in enemies) if (!e.Dead && e.attack != null && e.state == ActionState.Light) n++; return n; } }

        // Oyuncuya dönüş; kalkanlı yavaş döner (arkasına geçmek fırsattır)
        static void Face(Actor a, EnemyProfile p, float dx, float dt)
        {
            int want = dx > 0 ? 1 : -1;
            if (want == a.face) { a.turnTimer = 0; return; }
            if (p.turnDelay > 0 && (a.turnTimer += dt) < p.turnDelay) return;
            a.face = want; a.turnTimer = 0;
        }
```

In `TickEnemy`, replace the block from `a.face = dx > 0 ? 1 : -1;` through `else a.state = ActionState.Idle;` with:

```csharp
                Face(a, p, dx, dt);
                // Açık: iksir içen ya da ağır vuruşunu ıskalayıp toparlanan oyuncu
                bool opening = player.state == ActionState.Heal || player.state == ActionState.Heavy && player.Progress > .73f;
                bool facing = dx * a.face > 0;
                var choice = facing && (a.intentTime <= 0 || opening) ? Choose(a, p, dist, height) : null;
                bool ready = a.cooldown <= 0 || opening && choice != null && choice.punish;
                bool slot = choice != null && (choice.kind is AttackKind.Backstep or AttackKind.Teleport || Attackers < MaxAttackers);
                if (choice != null && ready && slot && attackDirector <= 0) { Begin(a, choice); attackDirector = .26f; a.cooldown = .8f; }
                else if (dist > p.keepMax) { vx = a.face * p.speed; a.state = ActionState.Move; }
                else if (dist < p.keepMin) { vx = -a.face * p.speed * .8f; a.state = ActionState.Move; }   // yüzü oyuncuda, geri çekilir
                else a.state = ActionState.Idle;
```

`Assets/Scripts/GameWorld.cs` `Damage`. Right after the first line (`if (target.Dead || target.invincible > 0 && !unblockable) return;`), insert:

```csharp
            // Kalkanlı: önden gelen hafif vuruşu kalkan karşılar; ağır vuruş geçer, arkadan vuruş dengeyi iki kat düşürür
            var profile = target.player ? null : EnemyProfile.For(target.role);
            bool front = source != null && (source.position.x - target.position.x) * target.face > 0;
            if (profile != null && profile.shielded && front && !heavy && !red && !unblockable && target.state != ActionState.Hurt)
            {
                target.guard -= damage * 1.3f;
                effects.Impact(target.position + Vector2.up * 1.35f, direction, true, false, float.NaN, .045f); sound?.Invoke("block");
                if (target.guard > 0) return;
                Stagger(target, source); Notify("KALKAN KIRILDI"); return;
            }
```

Right after the line `target.knockback = direction * (heavy ? 3.1f : 1.5f); target.Enter(target.health <= 0 ? ActionState.Dead : ActionState.Hurt, heavy ? .6f : .28f);`, insert:

```csharp
            // Denge: her vuruş düşürür (kalkanlıya arkadan iki kat); sıfırlanınca uzun sendeleme ve infaz fırsatı
            if (profile != null && !target.Dead)
            {
                target.guard -= damage * (heavy ? 1.5f : 1) * (profile.shielded && !front ? 2 : 1);
                if (target.guard <= 0) Stagger(target, source);
            }
```

Add the method next to `Launch`:

```csharp
        void Stagger(Actor target, Actor source)
        {
            target.Enter(ActionState.Hurt, .9f); target.guard = target.maxGuard; target.knockback = 0;
            if (source != null && source.player) { source.riposte = .8f; source.riposteTarget = target.id; }
        }
```

Note on the back-hit test: `e.guard` is 75 − 40 = 35 > 0, so no stagger happens; the test expects exactly `maxGuard − 40`.

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: `COMPILE_CHECK_SUCCESS`.
Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_SUCCESS`; the log contains `KARAMANI_ENEMY_BEHAVIOUR_PASS`.

- [ ] **Step 5: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Editor/EnemyTests.cs
git commit -m "Düşman davranışı: geri çekilme, iki saldırgan sınırı, açık cezalandırma, kalkan, denge kırılması"
```

---

### Task 4: Projectiles (musket, arrow, volley), reload chain, archer backstep

**Files:**
- Modify: `Assets/Scripts/GameWorld.cs`:
  - `:27` (`Projectile` + `ShotKind`)
  - `:195-196` (player pistol call)
  - `:402-428` (`Shoot`, `TickProjectiles` + new `HitsPlayer`, `RemoveShot`)
- Modify: `Assets/Scripts/EnemyBrain.cs` (`PerformAttack` default branch, new `Release`, `AddShot`)
- Modify: `Assets/Editor/EnemyTests.cs` (add `Ranged`)
- Modify: `Assets/Editor/BuildTools.cs:345-348` (`VerifyRanged` archer setup)

**Interfaces:**
- Consumes: `Begin`, `StartAttack`, `PerformAttack` (Task 2); `Damage(..., red)` (Task 2)
- Produces:
  - `public enum GameWorld.ShotKind { Pistol, Arrow, Bullet, VolleyMark, VolleyArrow, Wave, Orb }`
  - `GameWorld.Projectile { ShotKind kind; Vector2 position, velocity; bool friendly, red; float life, delay, damage; SpriteRenderer renderer; Actor source }`
  - `Projectile AddShot(ShotKind, Vector2 position, Vector2 velocity, float life, EnemyAttack k, Actor source, float delay = 0)`
  - `void Release(Actor, EnemyAttack)`

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Ranged(root);` from `Verify` after `Behaviour(root);`.

```csharp
        static void Ranged(GameRoot root)
        {
            // Tüfek: kırmızı mermi bloğu deler (35), ardından 2 sn doldurma
            var w = Arena(root, EnemyRole.Musketeer, 7, out var e); var pl = w.player;
            w.StartAttack(e, "aim");
            Run(w, 1.7f, i => new InputFrame { block = true });
            Assert(pl.health == pl.maxHealth - 35, "Musket shot should pierce the block for 35, health " + pl.health);
            Assert(e.state == ActionState.Shoot && e.attack?.name == "reload", "Musketeer did not reload after firing");

            // Mermiye doğru zamanında yuvarlanma kurtarır
            w = Arena(root, EnemyRole.Musketeer, 7, out e); pl = w.player;
            w.StartAttack(e, "aim");
            int press = Mathf.RoundToInt(1.36f * 120);
            Run(w, 1.8f, i => new InputFrame { move = i >= press && i < press + 2 ? 1 : 0, dodge = i == press });
            Assert(pl.health == pl.maxHealth, "Dodging into the bullet should avoid it, health " + pl.health);

            // Nişan sırasında vurulan tüfekli doldurmaya geçmez
            w = Arena(root, EnemyRole.Musketeer, 7, out e);
            w.StartAttack(e, "aim"); Run(w, .3f);
            w.Damage(e, 10, 1, false, false, w.player);
            Run(w, .6f);
            Assert(e.attack == null || e.attack.name != "reload", "Interrupted aim must not chain into reload");

            // Ok yağmuru: işaretli yerde duran vurulur
            w = Arena(root, EnemyRole.Archer, 7, out e); pl = w.player;
            w.StartAttack(e, "volley");
            Run(w, 2f);
            Assert(pl.health == pl.maxHealth - 18, "Volley should hit a player standing on the mark, health " + pl.health);

            // Ok yağmuru: işaretten çıkan kurtulur
            w = Arena(root, EnemyRole.Archer, 7, out e); pl = w.player;
            var volley = EnemyProfile.For(EnemyRole.Archer).Find("volley");
            int leave = Mathf.RoundToInt((volley.windup + .1f) * 120);
            Run(w, 2f, i => new InputFrame { move = i >= leave && i < leave + 60 ? -1 : 0 });
            Assert(pl.health == pl.maxHealth, "Leaving the volley mark should avoid the arrows, health " + pl.health);

            // Okçu yakındaki oyuncudan geri sıçrar
            w = Arena(root, EnemyRole.Archer, 1f, out e); e.intentTime = 0;
            Run(w, .6f);
            Assert(e.position.x - w.player.position.x > 2.3f, "Archer did not backstep, gap " + (e.position.x - w.player.position.x));

            // Platformdaki okçu geri çekilirken kenardan düşmez (platform 23.5 - 26.6, y = -0.75)
            w = Arena(root, EnemyRole.Archer, 1f, out e);
            e.position = new Vector2(26.2f, -.75f); w.player.position = new Vector2(24.8f, -.75f);
            Run(w, 2f);
            Assert(e.grounded && Mathf.Abs(e.position.y + .75f) < .01f, "Archer fell off the platform while retreating");
            Debug.Log("KARAMANI_ENEMY_RANGED_PASS · musket pierce + reload + dodge, interrupted chain, volley mark, backstep, platform edge");
        }
```

- [ ] **Step 2: Run compile to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: compile passes (the test only uses existing API).
Then run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_FAILED`; the log shows `Musket shot should pierce the block for 35` (no bullet is released yet).

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/GameWorld.cs`, replace line 27 (`public sealed class Projectile ...`) with:

```csharp
        public enum ShotKind { Pistol, Arrow, Bullet, VolleyMark, VolleyArrow, Wave, Orb }
        public sealed class Projectile { public ShotKind kind; public Vector2 position, velocity; public bool friendly, red; public float life, delay, damage; public SpriteRenderer renderer; public Actor source; }
```

In `TickPlayer`, `{ a.attackConnected = true; Shoot(a, true); sound?.Invoke("shot"); }` becomes `{ a.attackConnected = true; Shoot(a); sound?.Invoke("shot"); }`.

Replace `Shoot` and `TickProjectiles` (lines 402–428) with:

```csharp
        // Karamani'nin tabancası
        void Shoot(Actor a)
        {
            Vector2 position = a.view.BladeRoot + new Vector2(a.face * .46f, .06f);
            var sr = ArtLibrary.Draw("Pistol shot", ArtLibrary.White, actorsRoot, position, 170, new Color(1, .88f, .63f));
            sr.transform.localScale = new Vector3(.23f, .025f, 1);
            sr.transform.rotation = Quaternion.Euler(0, 0, a.face > 0 ? 0 : 180);
            projectiles.Add(new Projectile { kind = ShotKind.Pistol, position = position, velocity = Vector2.right * a.face * 19, life = 1.4f, friendly = true, damage = 38, renderer = sr, source = a });
            effects.GlowBurst(position, new Color(1, .62f, .25f, .65f));
        }

        void TickProjectiles(float dt)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i];
                if (p.life <= 0) { RemoveShot(i); continue; }
                p.renderer.enabled = p.delay <= 0;
                if (p.delay > 0) { p.delay -= dt; continue; }
                // Lanet küresi oyuncuyu yavaşça izler
                if (p.kind == ShotKind.Orb) p.velocity = Vector2.MoveTowards(p.velocity, (player.position + Vector2.up * 1.2f - p.position).normalized * 2.2f, dt * 6);
                Vector2 old = p.position; p.position += p.velocity * dt; p.life -= dt;
                if (p.friendly)
                {
                    foreach (var a in enemies)
                        if (!a.Dead && SegmentHits(old, p.position, new Rect(a.position.x - .3f, a.position.y + .2f, .6f, 2.1f)))
                        { Damage(a, p.damage, p.velocity.x > 0 ? 1 : -1, true, false, p.source, .08f); p.life = 0; break; }
                }
                else if (p.kind != ShotKind.VolleyMark && !player.Dead && HitsPlayer(p, old))
                { Damage(player, p.damage, p.velocity.x >= 0 ? 1 : -1, false, false, p.source, -1, p.red); p.life = 0; }
                p.renderer.transform.position = p.kind == ShotKind.Wave ? p.position + Vector2.up * .45f : p.position;
                if (p.life <= 0) RemoveShot(i);
            }
        }

        bool HitsPlayer(Projectile p, Vector2 old)
        {
            var box = new Rect(player.position.x - .23f, player.position.y + .2f, .46f, 1.95f);
            if (p.kind == ShotKind.Wave) return new Rect(p.position.x - .3f, p.position.y, .6f, .9f).Overlaps(box);
            if (p.kind == ShotKind.Orb) return SegmentHits(old, p.position, new Rect(box.x - .2f, box.y - .2f, box.width + .4f, box.height + .4f));
            return SegmentHits(old, p.position, box);
        }

        void RemoveShot(int i)
        {
            var p = projectiles[i];
            if (Application.isPlaying) UnityEngine.Object.Destroy(p.renderer.gameObject); else UnityEngine.Object.DestroyImmediate(p.renderer.gameObject);
            projectiles.RemoveAt(i);
        }
```

`Assets/Scripts/EnemyBrain.cs`:
- In `PerformAttack`, replace `default: return;` with:

```csharp
                case AttackKind.Reload: case AttackKind.Backstep: case AttackKind.Teleport:
                    return;
                default:   // mermi ve büyüler: hazırlık bittiği an bir kez
                    if (a.state == ActionState.Light && a.stateTime >= k.windup && !a.attackConnected) { a.attackConnected = true; Release(a, k); }
                    return;
```

- Add the members:

```csharp
        void Release(Actor a, EnemyAttack k)
        {
            Vector2 hand = a.position + new Vector2(a.face * .5f, 1.35f * a.size);
            Vector2 chest = player.position + Vector2.up * 1.2f;
            switch (k.kind)
            {
                case AttackKind.Bullet: case AttackKind.Arrow:
                {
                    float speed = k.kind == AttackKind.Bullet ? 22 : 13;
                    Vector2 aim = chest - hand, velocity = aim.x * a.face > 0 ? aim.normalized * speed : Vector2.right * a.face * speed;
                    AddShot(k.kind == AttackKind.Bullet ? ShotKind.Bullet : ShotKind.Arrow, hand, velocity, 1.4f, k, a);
                    sound?.Invoke("shot"); effects.GlowBurst(hand, new Color(1, .62f, .25f, .65f));
                    break;
                }
                case AttackKind.Volley:
                {
                    // Düşeceği yer önceden kırmızı işaretle belli olur; oklar .8 sn sonra yukarıdan iner
                    float x = player.position.x, y = player.position.y;
                    AddShot(ShotKind.VolleyMark, new Vector2(x, y + .05f), Vector2.zero, .8f, k, a);
                    for (int i = -1; i <= 1; i++) AddShot(ShotKind.VolleyArrow, new Vector2(x + i * .8f, y + 5), new Vector2(0, -16), 5f / 16 + .05f, k, a, .8f);
                    break;
                }
            }
        }

        Projectile AddShot(ShotKind kind, Vector2 position, Vector2 velocity, float life, EnemyAttack k, Actor source, float delay = 0)
        {
            var (sprite, color, scale) = kind switch
            {
                ShotKind.Bullet => (ArtLibrary.White, new Color(1, .86f, .55f), new Vector3(.32f, .035f, 1)),
                ShotKind.Arrow or ShotKind.VolleyArrow => (ArtLibrary.White, new Color(.7f, .59f, .43f), new Vector3(.36f, .03f, 1)),
                ShotKind.VolleyMark => (ArtLibrary.Glow, new Color(1, .15f, .1f, .55f), new Vector3(2.2f, .35f, 1)),
                ShotKind.Wave => (ArtLibrary.Glow, new Color(.3f, .02f, .08f, .9f), new Vector3(.7f, 1.1f, 1)),
                _ => (ArtLibrary.Glow, new Color(.55f, .1f, .8f, .9f), new Vector3(.45f, .45f, 1)),
            };
            var sr = ArtLibrary.Draw(kind.ToString(), sprite, actorsRoot, position, 170, color);
            sr.transform.localScale = scale;
            if (kind is ShotKind.Bullet or ShotKind.Arrow or ShotKind.VolleyArrow)
                sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            sr.enabled = delay <= 0;
            var shot = new Projectile { kind = kind, position = position, velocity = velocity, life = life, delay = delay, damage = k.damage, red = k.red, renderer = sr, source = source };
            projectiles.Add(shot); return shot;
        }
```

`Assets/Editor/BuildTools.cs` `VerifyRanged`, replace lines 345–348:

```csharp
            var archer = w.enemies[2]; archer.position = new Vector2(6, -.75f); archer.face = -1;
            archer.cooldown = archer.intentTime = 3; archer.Enter(ActionState.Shoot, .94f); archer.stateTime = .61f;
            archer.view.ResetContacts(); archer.view.Render(archer, .016f, 1);
            for (int i = 0; i < 100; i++) w.Simulate(1f / 120, new InputFrame(), true);
```

with:

```csharp
            var archer = w.enemies[2]; archer.position = new Vector2(6, -.75f); archer.face = -1; archer.intentTime = 99;
            w.StartAttack(archer, "arrow"); archer.view.ResetContacts();
            for (int i = 0; i < 160; i++) w.Simulate(1f / 120, new InputFrame(), true);
```

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: `COMPILE_CHECK_SUCCESS`.
Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_SUCCESS`; the log contains `KARAMANI_ENEMY_RANGED_PASS`.

- [ ] **Step 5: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Editor/EnemyTests.cs KaramaniReborn/Assets/Editor/BuildTools.cs
git commit -m "Uzaktan saldıranlar: tüfek, ok, ok yağmuru, doldurma zinciri, geri sıçrama"
```

---

### Task 5: Sorcerer (flame wave, orbs, teleport, phase 2) and half-boss bar

**Files:**
- Modify: `Assets/Scripts/EnemyBrain.cs` (`Release` cases, `PerformAttack` teleport, phase switch in `TickEnemy`, `Teleport`, `Standable`, `BarTarget`)
- Modify: `Assets/Scripts/GameWorld.cs` `ResolveBlade` (snuff orbs)
- Modify: `Assets/Scripts/GameHUD.cs:165-167`
- Modify: `Assets/Editor/EnemyTests.cs` (add `Sorcerer`)

**Interfaces:**
- Consumes: `Release`, `AddShot`, `ShotKind` (Task 4); `Begin` (Task 2)
- Produces: `public Actor GameWorld.BarTarget`, `void Teleport(Actor)`, `bool Standable(float x, out float y)`

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Sorcerer(root);` from `Verify` after `Ranged(root);`.

```csharp
        static void Sorcerer(GameRoot root)
        {
            var flame = EnemyProfile.For(EnemyRole.Sorcerer).Find("flame");
            // Alev dalgası: yerde duran yanar
            var w = Arena(root, EnemyRole.Sorcerer, 6, out var e); var pl = w.player;
            w.StartAttack(e, "flame"); Run(w, 2.4f);
            Assert(pl.health == pl.maxHealth - 22, "Flame wave should hit a grounded player, health " + pl.health);

            // Alev dalgası: zamanında zıplayan kurtulur
            w = Arena(root, EnemyRole.Sorcerer, 6, out e); pl = w.player;
            w.StartAttack(e, "flame");
            float arrive = flame.windup + (6 - .6f - .53f) / 6;   // dalganın ön kenarı oyuncu kutusuna varır
            int jump = Mathf.RoundToInt((arrive - .25f) * 120);
            Run(w, 2.4f, i => new InputFrame { jump = i == jump, jumpHeld = i >= jump && i < jump + 40 });
            Assert(pl.health == pl.maxHealth, "Jumping should clear the flame wave, health " + pl.health);

            // Lanet küreleri: üç küre çıkar, kılıçla söndürülür
            w = Arena(root, EnemyRole.Sorcerer, 6, out e);
            var orbs = EnemyProfile.For(EnemyRole.Sorcerer).Find("orbs");
            w.StartAttack(e, "orbs"); Run(w, orbs.windup + .05f);
            Assert(w.projectiles.FindAll(s => s.kind == GameWorld.ShotKind.Orb).Count == 3, "Sorcerer should release three orbs");
            Run(w, .02f, i => new InputFrame { light = i == 0 });
            Run(w, .12f);   // hafif vuruşun etkin penceresine
            var orb = w.projectiles.Find(s => s.kind == GameWorld.ShotKind.Orb);
            orb.position = Vector2.Lerp(w.player.view.StrikeRoot, w.player.view.BladeTip, .7f); orb.velocity = Vector2.zero;
            Run(w, 2f / 120);
            Assert(!w.projectiles.Contains(orb), "A sword swing should snuff out a curse orb");

            // Işınlanma: yaklaşan oyuncudan kaçar ve sağlam zemine iner
            w = Arena(root, EnemyRole.Sorcerer, 1.2f, out e); e.intentTime = 0; float before = e.position.x;
            Run(w, .9f);
            Assert(Mathf.Abs(e.position.x - before) > 1.5f && e.grounded, "Sorcerer did not teleport away: " + before + " -> " + e.position.x);

            // Işınlanma: oyuncunun arkası çukursa başka yere iner (çukur 13.5 - 15.7)
            w = Arena(root, EnemyRole.Sorcerer, 1.2f, out e); e.intentTime = 0;
            w.player.position = new Vector2(16.4f, w.zone.floor); w.player.face = 1; e.position.x = 17.6f;
            Run(w, .9f);
            Assert(e.grounded && Mathf.Abs(e.position.y - w.zone.floor) < .01f && !(e.position.x > 13.5f && e.position.x < 15.7f), "Sorcerer teleported into a pit: x " + e.position.x);

            // İkinci evre: canı yarının altına inince alev dalgaları ikişer gelir
            w = Arena(root, EnemyRole.Sorcerer, 6, out e);
            e.health = e.maxHealth * .45f; Run(w, .05f);
            Assert(e.phase == 2, "Sorcerer did not enter phase 2");
            w.StartAttack(e, "flame"); Run(w, flame.windup + .6f);
            Assert(w.projectiles.FindAll(s => s.kind == GameWorld.ShotKind.Wave).Count == 2, "Phase 2 flame should release two waves");

            // Yarı-boss can çubuğu
            Assert(w.BarTarget == e, "Half-boss bar should target the sorcerer");
            Debug.Log("KARAMANI_ENEMY_SORCERER_PASS · flame wave + jump, orbs + snuff, teleport + pit guard, phase 2, half-boss bar");
        }
```

- [ ] **Step 2: Run compile to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: FAIL, with error `GameWorld.BarTarget` not found.

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/EnemyBrain.cs`:
- In `TickEnemy`, right after `a.intentTime -= dt;`, add:

```csharp
            if (p.phaseAt > 0 && a.phase == 1 && a.health <= a.maxHealth * p.phaseAt) { a.phase = 2; Notify(p.title.ToUpperInvariant() + " · İKİNCİ EVRE"); }
```

- In `PerformAttack`, replace `case AttackKind.Reload: case AttackKind.Backstep: case AttackKind.Teleport: return;` with:

```csharp
                case AttackKind.Teleport:
                    if (a.stateTime >= k.windup && !a.attackConnected) { a.attackConnected = true; Teleport(a); }
                    return;
                case AttackKind.Reload: case AttackKind.Backstep:
                    return;
```

- In `Release`, add before the closing `}` of the switch:

```csharp
                case AttackKind.FlameWave:
                {
                    // Yerde ilerleyen kara alev; ikinci evrede yarım saniye arayla iki dalga
                    Vector2 start = new Vector2(a.position.x + a.face * .6f, a.position.y);
                    int waves = a.phase > 1 ? 2 : 1;
                    for (int i = 0; i < waves; i++) AddShot(ShotKind.Wave, start, new Vector2(a.face * 6, 0), 2, k, a, i * .5f);
                    break;
                }
                case AttackKind.Orbs:
                    for (int i = 0; i < 3; i++) AddShot(ShotKind.Orb, hand + new Vector2(0, .4f * (i - 1)), new Vector2(a.face * (1 + i * .4f), .6f * (i - 1)), 4, k, a);
                    break;
```

- Add the members:

```csharp
        // Duman olup oyuncunun arkasına, olmazsa uzağa geçer; yalnız sağlam (tek yönlü olmayan) zemine iner
        void Teleport(Actor a)
        {
            var smoke = new Color(.35f, .1f, .45f);
            effects.GlowBurst(a.position + Vector2.up, smoke);
            float away = -Mathf.Sign(player.position.x - a.position.x);
            float[] options = { player.position.x - player.face * 2.2f, a.position.x + away * 5, player.position.x + player.face * 3 };
            foreach (float x in options)
                if (Standable(x, out float y)) { a.position = new Vector2(x, y); a.velocity = Vector2.zero; break; }
            a.face = player.position.x > a.position.x ? 1 : -1;
            effects.GlowBurst(a.position + Vector2.up, smoke);
        }

        bool Standable(float x, out float y)
        {
            foreach (var s in zone.surfaces) if (!s.oneWay && x >= s.left + .3f && x <= s.right - .3f) { y = s.y; return true; }
            y = 0; return false;
        }

        // Arayüzün can çubuğu: yakındaki boss ya da yarı-boss
        public Actor BarTarget
        {
            get
            {
                Actor best = null; float near = 9;
                foreach (var e in enemies)
                {
                    if (e.Dead || !(e.boss || EnemyProfile.For(e.role).halfBoss)) continue;
                    float d = Mathf.Abs(e.position.x - player.position.x); if (d < near) { near = d; best = e; }
                }
                return best;
            }
        }
```

`Assets/Scripts/GameWorld.cs` `ResolveBlade`, replace `if (source.player) { foreach (var target in enemies) TryBlade(source, target); }` with:

```csharp
            if (source.player)
            {
                // Lanet küreleri kılıçla söndürülür
                foreach (var shot in projectiles)
                    if (shot.kind == ShotKind.Orb && shot.life > 0 && shot.delay <= 0 && SegmentHits(source.view.StrikeRoot, source.view.BladeTip, new Rect(shot.position.x - .3f, shot.position.y - .3f, .6f, .6f)))
                    { shot.life = 0; effects.GlowBurst(shot.position, new Color(.55f, .1f, .8f)); }
                foreach (var target in enemies) TryBlade(source, target);
            }
```

`Assets/Scripts/GameHUD.cs`, replace lines 165–167:

```csharp
            var boss = w.BarTarget;
            bossGroup.SetActive(boss != null);
            if (boss != null) { bossName.text = boss.boss ? w.zone.bossName : EnemyProfile.For(boss.role).title; bossBar.fillAmount = boss.health / boss.maxHealth; }
```

- [ ] **Step 4: Run compile and the enemy job to verify they pass**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1`
Expected: `COMPILE_CHECK_SUCCESS`.
Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_SUCCESS`; the log contains `KARAMANI_ENEMY_SORCERER_PASS`.

- [ ] **Step 5: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Assets/Scripts/EnemyBrain.cs KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/GameHUD.cs KaramaniReborn/Assets/Editor/EnemyTests.cs
git commit -m "Büyücü: alev dalgası, lanet küreleri, ışınlanma, ikinci evre, yarı-boss çubuğu"
```

---

### Task 6: Zone layouts, full verification, EXE, docs

**Files:**
- Modify: `Assets/Scripts/GameWorld.cs` (constructor spawns → `Layout`)
- Modify: `Assets/Scripts/RuntimeVerification.cs:97` (enemy count per zone)
- Modify: `Assets/Editor/EnemyTests.cs` (add `Layouts`)
- Modify: `Docs/TASK_BOARD.md`, `Docs/DESIGN_CONSTRAINTS.md`

**Interfaces:**
- Consumes: `Spawn` (Task 1)
- Produces: `(float x, float y, EnemyRole role)[] Layout(int zone)` (private)

- [ ] **Step 1: Write the failing test**

Add to `EnemyTests`. Call `Layouts(root);` from `Verify` after `Sorcerer(root);`.

```csharp
        static void Layouts(GameRoot root)
        {
            // Bölge dizilişi (spec §5): insanlar önce, büyücü Kurt Geçidi'nde ilk kez; her bölümde bir boss
            var expect = new[]
            {
                new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Archer, EnemyRole.Nefer, EnemyRole.Nefer, EnemyRole.Captain },
                new[] { EnemyRole.Nefer, EnemyRole.Musketeer, EnemyRole.Halberdier, EnemyRole.Musketeer, EnemyRole.Nefer, EnemyRole.Captain },
                new[] { EnemyRole.Spearman, EnemyRole.Musketeer, EnemyRole.Archer, EnemyRole.Halberdier, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.Captain },
                new[] { EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Musketeer, EnemyRole.Halberdier, EnemyRole.Archer, EnemyRole.Sorcerer, EnemyRole.Captain },
            };
            for (int zone = 0; zone < 4; zone++)
            {
                root.LoadZone(zone, true); var w = root.world;
                Assert(w.enemies.Count == expect[zone].Length, "Zone " + zone + " enemy count " + w.enemies.Count);
                for (int i = 0; i < expect[zone].Length; i++) Assert(w.enemies[i].role == expect[zone][i], "Zone " + zone + " enemy " + i + " is " + w.enemies[i].role);
                Assert(w.enemies.FindAll(a => a.boss).Count == 1 && w.enemies[^1].boss, "Zone " + zone + " must end with one boss");
                Assert(!w.enemies.Exists(a => EnemyProfile.For(a.role).halfBoss) || zone >= 2, "Sorcerer before Kurt Geçidi");
            }
            Debug.Log("KARAMANI_ENEMY_LAYOUT_PASS · four zones per spec §5");
        }
```

- [ ] **Step 2: Run the enemy job to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies`
Expected: `UNITY_BATCH_FAILED`; the log shows `Zone 1 enemy 1 is Spearman`.

- [ ] **Step 3: Write the implementation**

`Assets/Scripts/GameWorld.cs` constructor: replace the six `Spawn(...)` lines with:

```csharp
            foreach (var (x, y, role) in Layout(index)) Spawn(x, y, role);
            Spawn(50.4f, zone.floor, EnemyRole.Captain, true);
```

and add:

```csharp
        // Bölge dizilişi (Docs/specs/2026-10-12-enemy-roster-design.md §5): önce insanlar, büyücü Kurt Geçidi'nde ilk kez.
        // Platformlar: 23.5-26.6 (y -0.75), 37.4-40.7 (y -0.85)
        (float x, float y, EnemyRole role)[] Layout(int index)
        {
            float f = zone.floor;
            return index switch
            {
                0 => new[] { (10f, f, EnemyRole.Nefer), (18.1f, f, EnemyRole.Spearman), (25.5f, -.75f, EnemyRole.Archer), (35.1f, f, EnemyRole.Nefer), (42.3f, f, EnemyRole.Nefer) },
                1 => new[] { (10f, f, EnemyRole.Nefer), (20f, f, EnemyRole.Musketeer), (35.1f, f, EnemyRole.Halberdier), (39f, -.85f, EnemyRole.Musketeer), (44f, f, EnemyRole.Nefer) },
                2 => new[] { (10f, f, EnemyRole.Spearman), (20f, f, EnemyRole.Musketeer), (25.5f, -.75f, EnemyRole.Archer), (35.1f, f, EnemyRole.Halberdier), (39f, -.85f, EnemyRole.Archer), (44f, f, EnemyRole.Sorcerer) },
                _ => new[] { (10f, f, EnemyRole.Nefer), (18.1f, f, EnemyRole.Spearman), (25.5f, -.75f, EnemyRole.Musketeer), (35.1f, f, EnemyRole.Halberdier), (39f, -.85f, EnemyRole.Archer), (44f, f, EnemyRole.Sorcerer) },
            };
        }
```

`Assets/Scripts/RuntimeVerification.cs:97`: `game.world.enemies.Count == 6` becomes `game.world.enemies.Count == (zone >= 2 ? 7 : 6)`.

- [ ] **Step 4: Run all verification**

Run in order (one Unity job at a time):
1. `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/compile_check.ps1` → `COMPILE_CHECK_SUCCESS`
2. `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Enemies` → `UNITY_BATCH_SUCCESS`, `KARAMANI_ENEMY_LAYOUT_PASS` in the log
3. `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Verify` → `UNITY_BATCH_SUCCESS`; every existing `KARAMANI_*_PASS` line plus the five `KARAMANI_ENEMY_*_PASS` lines
4. `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity_batch.ps1 -Job Build` → `UNITY_BATCH_SUCCESS` (close any running `KaramaniReborn.exe` you started first)
5. `powershell -NoProfile -ExecutionPolicy Bypass -File VERIFY_WINDOWS.ps1` → 10 `PASS` lines, exit 0

Then open `Review/chapter-1.png` … `chapter-4.png` and `Review/player-combat.png`, and confirm the enemies render (placeholder sheets) and nothing is missing.

If a pre-existing check fails, fix the cause; do not weaken the check. One example: `VerifyCombat` "Buffered combo" can fail if a guard-break stagger changes the combo. In that case set `target.maxGuard = target.guard = 999` in that test's setup, because that test is about the player's combo, not enemy guard.

- [ ] **Step 5: Docs**

`Docs/TASK_BOARD.md`: add the row before `## Backlog from the baseline review`:

```markdown
| T-009 | Regular enemies per `Docs/specs/2026-10-12-enemy-roster-design.md`: profile table (`EnemyProfiles.cs`), behaviour (`EnemyBrain.cs`), red/white attacks, shield, guard break, ranged + sorcerer, zone layouts; placeholder pixel sheets | integrator | done | `Assets/Scripts/EnemyProfiles.cs`, `EnemyBrain.cs`, `GameWorld.cs`, `GameTypes.cs`, `PixelCharacter.cs`, `Effects.cs`, `GameHUD.cs`, `RuntimeVerification.cs`, `Assets/Editor/EnemyTests.cs`, `BuildTools.cs`, `Tools/unity_batch.ps1` | T-008 | C, D | (fill with the exact Verify / Build / VERIFY_WINDOWS log paths from Step 4) |
```

`Docs/DESIGN_CONSTRAINTS.md`, under "Visual direction locks", add:

```markdown
- **Enemies (user, 2026-10-12):** Karamani's opening chapter has 5 human enemies (Sword Nefer, Shield Spearman, Halberdier, Musketeer, Archer) plus the Sorcerer half-boss, and 4 human agha bosses (Arslan, Kör Demir, Kurtbaş, Kara Murad). Details and values: `Docs/specs/2026-10-12-enemy-roster-design.md`. Combat feel: Blasphemous / The Last Faith (white = blockable/parryable, red = dodge or jump only).
```

- [ ] **Step 6: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Assets/Scripts/GameWorld.cs KaramaniReborn/Assets/Scripts/RuntimeVerification.cs KaramaniReborn/Assets/Editor/EnemyTests.cs KaramaniReborn/Docs/TASK_BOARD.md KaramaniReborn/Docs/DESIGN_CONSTRAINTS.md
git commit -m "Bölge düşman dizilişleri, tam doğrulama ve belgeler"
```

---

### Task 7: ChatGPT prompts for the 6 enemies and 4 bosses

**Files:**
- Create: `Tools/enemy_prompts.py`
- Create (generated): `Docs/references/enemy-sheets.md`

**Interfaces:**
- Consumes: nothing from code; the frame lists come from spec §3–§4.
- Produces: `Docs/references/enemy-sheets.md`. This is the user-facing guide (Turkish header) with copy-paste English prompts: one base pose per character plus one prompt per animation.

- [ ] **Step 1: Write the generator**

Create `Tools/enemy_prompts.py`:

```python
"""Düşman ve boss kare sayfaları için ChatGPT istemleri: Docs/references/enemy-sheets.md üretir.

Kaynak: Docs/specs/2026-10-12-enemy-roster-design.md (§3 düşmanlar, §4 boss'lar). Her karakter için önce
"ana poz" istemi (Karamani profili yalnız stil örneği), sonra her hareket için ana pozun eklendiği istem.
Kullanım: python -I Tools/enemy_prompts.py
"""
import io
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Docs', 'references', 'enemy-sheets.md')

JAN = ("an Ottoman-style Janissary soldier (dark fantasy): tall off-white felt bork hat with a long cloth flap hanging down the back of the neck "
       "and a small brass ornament at the front; thick dark moustache, fair skin, almond eyes, stern grim face; dark crimson long coat tucked up "
       "at the front over a weathered steel mail shirt; brown leather lamellar belt armor with bronze rivets; wide burgundy sash; baggy dark "
       "trousers; brown leather boots")
TAIL = ("STRICT SIDE PROFILE view facing right (side-scrolling action game sprite), full body visible in every frame, evenly spaced, identical scale, "
        "feet on the same ground line (except airborne frames), frames must not overlap, every frame a DIFFERENT pose. Flat solid background "
        "color #47304C, no shadow, no text, no borders, no grid. Same colors, costume details and proportions as the reference.")
HURT = ('darbe alma', 3, "a HURT reaction: 1 recoiling backward from a hit to the chest, head snapped back, 2 staggering one step back, 3 regaining balance")
DEATH = ('ölüm', 5, "a DEATH: 1 struck and staggering, 2 dropping to the knees, 3 weapon slipping from the hand, 4 falling forward, 5 lying face down on the ground. Not gory")
IDLE = ('bekleme', 4, "a subtle IDLE loop in a ready stance with the weapon held: 1 base pose, 2 chest rises slightly, 3 slight shift of weight, 4 almost back to the start (loopable)")
WALK = ('yürüme', 8, "a WALKING cycle, loopable, evenly spaced phases of the step, weapon held ready")

CHARS = [
    ('nefer', 'Kılıç Neferi', 'En sık görülen düşman: önce bu.', JAN + "; a curved yatagan sword in his right hand",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('kombo', 8, "a 2-HIT YATAGAN COMBO: 1-2 wind-up, 3 first diagonal slash with a white motion smear, 4 follow-through, 5 second slash the other way with a smear, 6 impact hold, 7-8 recovering"),
        ('atılış', 6, "a FAST LUNGE: 1 crouching, 2 springing forward, 3 long forward lunge with the sword thrust out, 4 impact hold, 5-6 recovering"),
        HURT, DEATH]),
    ('spearman', 'Kalkanlı Mızrakçı', 'Kalkanıyla önden gelen hafif vuruşları keser.', JAN + "; a round wooden shield with a bronze rim and boss on his left arm, a long spear in his right hand",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('dürtüş', 6, "a LONG SPEAR THRUST from behind the raised shield: 1-2 pulling the spear back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('kalkanla itme', 5, "a SHIELD BASH: 1 bracing, 2 stepping in, 3 slamming the shield forward, 4 hold, 5 recovering"),
        ('blok', 2, "a SHIELD BLOCK: 1 raising the shield in front of the body, 2 braced behind the shield with knees bent"),
        HURT, DEATH]),
    ('halberdier', 'Teberli', 'Yavaş ama geniş ve ağır vuruşlu.', JAN + "; heavier build, a long Ottoman teber halberd (axe blade and spike on a long pole) held in both hands",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('yatay savuruş', 8, "a WIDE HORIZONTAL HALBERD SWEEP: 1-3 long wind-up turning the body back, 4 huge horizontal sweep with a wide white smear, 5 follow-through, 6-8 slow recovery"),
        ('yukarıdan iniş', 8, "a HEAVY OVERHEAD HALBERD CHOP: 1-3 raising the halberd high, 4 chopping down hard, 5 blade hitting the ground, 6-8 slow recovery"),
        ('sapla itme', 5, "a SHORT HAFT SHOVE: 1 bracing, 2-3 shoving the haft forward to push the enemy away, 4 hold, 5 recovering"),
        HURT, DEATH]),
    ('musketeer', 'Tüfekli', 'Uzaktan ölümcül tek atış, sonra uzun doldurma.', JAN + "; a long matchlock musket with a glowing slow-match cord, a powder horn and a small pouch on the belt",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('nişan ve atış', 6, "AIMING AND FIRING THE MUSKET: 1 raising the musket to the shoulder, 2-3 aiming steadily, the match glowing, 4 firing with a bright muzzle flash and smoke, 5 recoil, 6 lowering"),
        ('doldurma', 6, "RELOADING THE MUSKET: 1 musket upright, 2 pouring powder from the horn, 3 ramming with the rod, 4 ramming again, 5 putting the rod back, 6 ready"),
        ('dipçik', 5, "a MUSKET BUTT STRIKE: 1 turning the musket around, 2-3 smashing forward with the stock, 4 hold, 5 recovering"),
        HURT, DEATH]),
    ('archer', 'Okçu', 'Yüksek platformlardan hızlı oklar atar.', JAN + "; lighter gear without mail sleeves, a composite recurve bow in his left hand, a leather quiver of arrows at his hip, a short dagger in the sash",
     'about 7% shorter than the man in the reference', [IDLE, WALK,
        ('ok atma', 5, "SHOOTING THE BOW STRAIGHT: 1 nocking an arrow, 2 raising and drawing, 3 full draw aiming straight forward, 4 release, 5 lowering"),
        ('yukarı atış', 5, "SHOOTING THE BOW HIGH INTO THE SKY: 1 nocking, 2 raising the bow steeply upward, 3 full draw aiming 60 degrees up, 4 release, 5 lowering"),
        ('geri sıçrama', 3, "a QUICK BACKWARD HOP while facing right: 1 pushing off, 2 in the air hopping back, 3 landing ready"),
        ('hançer', 5, "a QUICK DAGGER SLASH: 1 drawing the dagger, 2 wind-up, 3 fast slash, 4 hold, 5 recovering"),
        HURT, DEATH]),
    ('sorcerer', 'Paşanın Büyücüsü (yarı-boss)', 'Mesafe tutar, büyü yapar, yakına girince asayla dövüşür. Dinî bir figür değil.',
     "the pasha's black sorcerer (dark fantasy, NOT a religious figure): gaunt tall man with a deep dark-purple hood, pale face with sunken eyes and a thin black beard, "
     "layered black and deep purple robes, many bone and brass amulets on chains, a long carved dark-wood staff with a cracked crystal at the top glowing faint purple",
     'the same height as the man in the reference', [IDLE, WALK,
        ('asa kombosu', 8, "a 2-HIT STAFF COMBO: 1-2 wind-up, 3 first staff swing with a smear, 4 follow-through, 5 second swing the other way, 6 hold, 7-8 recovering"),
        ('alev büyüsü', 6, "casting a GROUND FLAME SPELL: 1-2 raising the staff, dark flames gathering in the crystal, 3 slamming the staff end to the ground, 4 black-purple fire bursting forward along the ground, 5-6 recovering"),
        ('küre büyüsü', 6, "casting CURSE ORBS: 1-2 raising the free hand, 3 three glowing purple orbs forming above the hand, 4 throwing the hand forward, orbs flying, 5-6 recovering"),
        ('ışınlanma', 6, "TELEPORTING: 1 wrapping himself in his robe, 2 dissolving into black smoke, 3 only smoke left, 4 smoke gathering, 5 re-forming, 6 standing ready"),
        ('yere vurma', 6, "a STAFF GROUND SLAM: 1-2 raising the staff high with both hands, 3 slamming it into the ground, 4 a purple shock ring bursting around him, 5-6 recovering"),
        ('darbe alma', 3, HURT[2]),
        ('ölüm', 6, "a DEATH: 1 struck, 2 dropping the staff, 3 falling to his knees, 4 body crumbling into black smoke, 5 mostly smoke, 6 only a pile of robe and amulets left")]),
]
EFFECTS = [
    ('alev dalgası efekti', 4, "4 frames of a BLACK-PURPLE GROUND FLAME WAVE travelling to the right, a low wall of dark fire about knee high, loopable, pixel art effect, transparent-looking edges"),
    ('lanet küresi efekti', 4, "4 frames of a GLOWING PURPLE CURSE ORB with a small dark core and wisps, pulsing, loopable, pixel art effect"),
]
BOSSES = [
    ('arslan', 'Bayraktar Arslan (Kızıl Surlar)', 'Dev sancaktar; ikinci evrede sancağı ateşe verir.',
     "Standard-bearer Arslan, a giant janissary standard-bearer (dark fantasy boss): huge broad-shouldered man, tall off-white bork with a red band, thick black beard, "
     "battle scars, heavy mail with bronze plates, crimson coat, chained trophies on his belt, a long war-standard pole with a torn crimson banner used as a spear, a yatagan in the sash",
     'about 15% TALLER and much broader than the man in the reference', [IDLE, WALK,
        ('direkle dürtüş', 6, "a LONG THRUST with the standard pole: 1-2 pulling back, 3 thrusting far forward, 4 hold, 5-6 recovering"),
        ('direkle savuruş', 8, "a WIDE ARC SWEEP with the standard pole: 1-3 big wind-up, 4 huge sweeping arc with a smear, 5 follow-through, 6-8 recovering"),
        ('sancağı dikme', 6, "PLANTING THE STANDARD and roaring a command: 1 lifting the pole, 2 stabbing it into the ground, 3-4 roaring with an arm raised, 5-6 pulling it out"),
        ('yanan sancakla savuruş', 8, "a SWEEP WITH THE BURNING STANDARD (the banner is on fire): 1-3 wind-up, 4 sweeping arc trailing flames, 5 follow-through, 6-8 recovering"),
        HURT, ('ölüm', 6, DEATH[2])]),
    ('kordemir', 'Humbaracıbaşı Kör Demir (Yanmış Mahalle)', 'Mahalleyi yakan adam: humbara, kanca, yağ küpü.',
     "Chief Grenadier Kor Demir (dark fantasy boss): stocky brutal man, half of his face badly burned and one eye milky blind, shaved head with a scorched cap, "
     "leather apron over mail, clay fire-grenades hanging from his belt, a long hooked axe",
     'about 12% TALLER and broader than the man in the reference', [IDLE, WALK,
        ('humbara atma', 6, "THROWING A CLAY FIRE-GRENADE in a high arc: 1 lighting the fuse, 2 winding up, 3 throwing, 4 follow-through, 5-6 recovering"),
        ('kancayla çekme', 7, "a HOOK PULL: 1-2 swinging the hooked axe forward, 3 hooking, 4-5 yanking it back hard toward himself, 6-7 recovering"),
        ('yağ küpü', 6, "THROWING AN OIL POT: 1 lifting a clay pot over his head, 2-3 winding up, 4 hurling it forward, 5-6 recovering"),
        ('balta inişi', 8, "a HEAVY AXE CHOP: 1-3 raising the axe high, 4 chopping down hard, 5 axe in the ground, 6-8 recovering"),
        HURT, ('ölüm', 6, DEATH[2])]),
    ('kurtbas', 'Deli Kurtbaş (Kurt Geçidi)', 'Kurt postlu vahşi delibaş; doğaüstü değil.',
     "Deli Kurtbas, a wild delibas warrior (dark fantasy boss, human): lean muscular man, a grey wolf pelt cloak with the wolf head worn as a hood, "
     "wild dark hair and beard, leather and fur armor, bare scarred arms, an axe in each hand",
     'about 12% TALLER than the man in the reference', [IDLE,
        ('koşu', 8, "a FAST RUNNING cycle, loopable, low and aggressive, axes held out"),
        ('balta fırtınası', 10, "a 4-HIT AXE STORM: 1 wind-up, 2-3 first slash, 4-5 second slash, 6-7 third slash, 8-9 fourth slash, each with a smear, 10 recovering"),
        ('üstünden atlama', 6, "a FLIP OVER THE ENEMY: 1 crouch, 2 leaping up, 3 upside-down flip in the air, 4 coming down, 5 landing facing back, 6 ready"),
        ('sıçrayıp çakılma', 7, "a LEAPING SLAM: 1 crouch, 2 leaping high, 3 axes raised at the apex, 4 diving down, 5 both axes slamming into the ground, 6-7 rising"),
        ('uluma', 5, "a HOWL into frenzy: 1 hunching, 2-3 throwing the head back howling, 4 shaking with rage, 5 ready, eyes wild"),
        HURT, ('ölüm', 6, DEATH[2])]),
    ('karamurad', 'Yeniçeri Ağası Kara Murad (Karanlık Kale)', 'Dev, kaslı dövüşçü; ikinci evrede çıplak elle.',
     "Janissary Agha Kara Murad (dark fantasy final boss, fully human): a giant, heavily muscled, very tall fighter, shoulders nearly twice as wide as a normal man, "
     "bare huge muscular arms, an open dark vest over a mail shirt, crimson sash, tall bork with a gold band and a long plume, black beard, a giant two-handed pala sword",
     'about 25% TALLER and nearly twice as broad as the man in the reference', [IDLE, WALK,
        ('pala kombosu', 10, "a 3-HIT HEAVY PALA COMBO with a slow-fast-slow rhythm: 1-2 wind-up, 3 first heavy slash, 4-5 quick second slash, 6-7 wind-up, 8 huge third slash with a wide smear, 9-10 recovering"),
        ('omuz hücumu', 6, "a SHOULDER CHARGE: 1 lowering the shoulder, 2-4 charging forward fast, 5 impact, 6 recovering"),
        ('yakalayıp fırlatma', 8, "a GRAB AND THROW: 1 reaching forward, 2 grabbing, 3 lifting an invisible enemy overhead, 4-5 slamming down hard, 6 impact, 7-8 recovering"),
        ('yere çakma', 7, "a GROUND SLAM with the pala: 1-3 raising it high with both hands, 4 slamming into the ground, 5 a shock wave bursting forward, 6-7 recovering"),
        ('yumruk kombosu', 8, "a BARE-HANDED FIST COMBO (no sword): 1 guard up, 2-3 heavy straight punch, 4-5 hook, 6-7 uppercut, 8 recovering"),
        HURT, ('ölüm', 6, DEATH[2])]),
]


def base_prompt(desc, size):
    return ("Use the attached image ONLY as a STYLE reference (same pixel-art style, same level of detail, same lighting and color depth, same scale), "
            f"but draw a DIFFERENT character: {desc}. The character is {size}.\n"
            "Single character, strict side profile view facing right, standing idle, full body, the body, hips and feet pointing right. "
            "Flat solid background color #47304C, no shadow, no text.")


def anim_prompt(desc, n, action):
    return f"Pixel art sprite sheet of the SAME character as the attached reference image: {desc}.\n{n} frames of {action}, in ONE horizontal row.\n{TAIL}"


def section(out, key, title, note, desc, size, moves):
    out.append(f"---\n\n## {title}\n{note}\n\n")
    out.append(f"### {key} 0. Ana poz (Karamani profilini **stil örneği** olarak ekle)\n```\n{base_prompt(desc, size)}\n```\n\n")
    for i, (tr, n, action) in enumerate(moves, 1):
        out.append(f"### {key} {i}. {tr}: {n} kare (ana pozu ekle)\n```\n{anim_prompt(desc, n, action)}\n```\n\n")


def main():
    out = ["# Düşmanlar ve boss'lar: kare sayfası istemleri (ChatGPT)\n\n",
           "Tasarım: `Docs/specs/2026-10-12-enemy-roster-design.md`. Bu dosyayı `python -I Tools/enemy_prompts.py` üretir.\n\n",
           "**Nasıl kullanılır:**\n",
           "1. **Önce her karakterin 0 numaralı \"ana poz\" istemi.** ChatGPT'ye `Docs/references/karamani-profile.webp` dosyasını **stil örneği** olarak ekle. Beğendiğin görsel o karakterin ana pozu olur.\n",
           "2. **Sonra o karakterin hareketleri.** Her hareket için yeni bir sohbet aç, **o karakterin ana poz görselini** ekle (Karamani'yi değil), istemi yapıştır.\n",
           "3. Görselleri `İndirilenler`e at ya da buraya gönder; adsız da olur.\n\n",
           "**Sıra:** Kılıç Neferi, Kalkanlı Mızrakçı, Tüfekli, Okçu, Teberli, Büyücü; boss'lar en son. Her karakterde ana poz, bekleme, yürüme, ilk saldırı, darbe alma ve ölüm yeterli; diğerleri sonra gelebilir.\n\n"]
    for c in CHARS:
        section(out, *c)
    out.append("---\n\n## Büyü efektleri (büyücü)\nAna poz gerekmez; Karamani profilini stil örneği olarak ekle.\n\n")
    for i, (tr, n, action) in enumerate(EFFECTS, 1):
        out.append(f"### efekt {i}. {tr}: {n} kare\n```\nUse the attached image ONLY as a STYLE reference (same pixel-art style). {action}. "
                   "Frames in ONE horizontal row, evenly spaced, identical scale, flat solid background color #47304C, no text, no grid.\n```\n\n")
    out.append("---\n\n# Boss'lar\n\n")
    for b in BOSSES:
        section(out, *b)
    io.open(OUT, 'w', encoding='utf-8').write(''.join(out))
    print(OUT, sum(1 for line in out if line.startswith('### ')), 'istem')


if __name__ == '__main__':
    main()
```

- [ ] **Step 2: Run it and check the output**

Run: `python -I Tools/enemy_prompts.py`
Expected: prints the output path and `89 istem`. The count is 6 base poses + 44 enemy moves + 2 effects + 4 boss bases + 33 boss moves.
Then run: `grep -c '```' Docs/references/enemy-sheets.md`
Expected: 178, an even number (every prompt is a closed code block).

- [ ] **Step 3: Checkpoint** (commit only with the user's permission)

```bash
git add KaramaniReborn/Tools/enemy_prompts.py KaramaniReborn/Docs/references/enemy-sheets.md
git commit -m "Düşman ve boss istemleri (ChatGPT)"
```
