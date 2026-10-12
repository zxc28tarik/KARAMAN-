# Karamani's opening chapter: enemy and boss design

**Date:** 2026-10-12. **Status:** user-approved design; written spec awaiting user review.
**Scope:** the 6 regular enemies and 4 bosses of Karamani's opening chapter (zones Kızıl Surlar, Yanmış Mahalle, Kurt Geçidi, Karanlık Kale). The other characters' stories get their own enemies later.

## 1. Purpose and decisions (user, 2026-10-12)
- **Realism:** human enemies first, supernatural later. Humans fight with believable 16th-century weapons and tactics. This does not mean historical accuracy, and real history is not pushed into the story.
- **Combat feel:** like Blasphemous / The Last Faith.
  - Few but dangerous enemies.
  - Every attack has a readable wind-up.
  - Timing, blocking/parrying and evasion matter.
- **Roster:** 5 humans plus 1 supernatural (the sorcerer, a half-boss). The ghoul is removed from the game.
- **Bosses:** 4 human janissary aghas. Each has a distinctive silhouette, weapon and second phase, and each tests the skill taught in its zone.
- **Success criteria:**
  - Every enemy forces a different behaviour on the player (parry, flank, close the distance, climb, dodge).
  - A careful player can win every fight without taking a hit.
  - Every attack is read from its wind-up.

## 2. Rules common to all enemies
1. **Wind-up and glint.** Every attack has a wind-up pose and a glint on the weapon.
   - ⚪ **White glint:** blockable and parryable.
   - 🔴 **Red glint:** unblockable. Blocking it still deals full damage; only a dodge (i-frames) or a jump avoids it.
2. **Guard (poise).** Uses the existing `guard` / `maxGuard` system. When guard reaches zero, the enemy staggers (`Hurt`, about 0.9 s) and is open to an execution. Parrying a ⚪ attack also staggers.
3. **Attack order.** At most 2 enemies attack at once; the others hold their range and wait. This generalises the existing `attackDirector`. A single enemy runs at most one combo at a time.
4. **Preferred range.** Every type holds its own range. Melee types close in and step back when needed; ranged types retreat when the player approaches.
5. **Punishing openings.** A nearby enemy attacks immediately when the player drinks a potion, whiffs a heavy attack or misses a parry.
6. **Minimum frames.** Every enemy has: base pose, idle, walk, its own attacks, hurt (stagger uses the hurt frames with a longer hold), death.

## 3. Regular enemies
All values are starting points; they are tuned in playtesting. The player's light hit deals 20, the third combo hit 29, the heavy 44.

### 3.1 Sword Nefer: basic melee
- **Weapon:** yatagan. **Health:** 70. **Guard:** 35. **Speed:** 1.35 u/s.
- **Attacks:**
  - ⚪ 2-hit yatagan combo, reach 1.5 u. Wind-up 0.35 s; the two hits 0.22 s apart.
    - Implementation note: 2-hit combos (this one and the Sorcerer's staff combo) use a 0.45 s hit gap, because the player's 0.4 s invulnerability after a hit would swallow a second hit only 0.22 s later.
  - ⚪ Fast lunge from 2.5 u. Wind-up 0.3 s; dashes 1.2 u forward.
- **Behaviour:** closes in, waits its turn at 1.5 u, occasionally steps back.
- **Weakness:** staggers when parried.
- **Frames:** base pose, idle 4, walk 8, combo 8, lunge 6, hurt 3, death 5.

### 3.2 Shield Spearman: closed to the front, long reach
- **Health:** 95. **Guard:** 75. **Speed:** 1.1 u/s.
- **Shield:** blocks light hits from the front. Heavy hits and hits from behind damage guard ×2.
- **Attacks:**
  - ⚪ Long spear thrust from behind the shield, reach 2.4 u, wind-up 0.4 s.
  - ⚪ Shield bash: reach 1.2 u, pushes the player back 1.5 u and drains stamina.
- **Behaviour:** advances slowly, shield first. When the player jumps over him and lands behind, he turns slowly (0.6 s), which is the opening.
- **Frames:** base pose, idle 4, walk 8, thrust 6, shield bash 5, block 2, hurt 3, death 5.

### 3.3 Halberdier: slow and wide
- **Health:** 110. **Guard:** 90. **Speed:** 1.0 u/s.
- **Attacks:**
  - 🔴 Wide horizontal halberd sweep, reach 2.8 u. Wind-up 0.6 s. Avoided by stepping back or jumping over.
  - ⚪ Heavy downward chop, reach 2.4 u. Wind-up 0.55 s; a block costs the player a lot of stamina.
  - ⚪ Haft shove when the player is close (< 1 u). Short; opens distance.
- **Behaviour:** holds 2.6 u. When the player gets inside, he steps back and shoves.
- **Weakness:** 0.8 s recovery after every attack.
- **Frames:** base pose, idle 4, walk 8, sweep 8, chop 8, haft shove 5, hurt 3, death 5.

### 3.4 Musketeer: one deadly shot from range
- **Weapon:** matchlock musket. **Health:** 55. **Guard:** 30. **Speed:** 1.2 u/s.
- **Attacks:**
  - 🔴 Aim and fire.
    - 1.2 s aim: the match glows, the barrel steadies.
    - Then a straight, fast bullet: damage 35, speed 22 u/s.
    - Avoided by dodging through it or jumping.
  - ⚪ Butt stroke when the player comes close (< 1.3 u).
- **Behaviour:** holds 5–9 u and retreats when the player approaches.
- **Weakness:** reloads for 2 s after every shot and is defenceless while doing so.
- **Frames:** base pose, idle 4, walk 8, aim+fire 6, reload 6, butt stroke 5, hurt 3, death 5.

### 3.5 Archer: fast arrows from above
- **Health:** 50. **Guard:** 25. **Speed:** 1.3 u/s.
- **Attacks:**
  - ⚪ Fast single arrow: straight, damage 14.
  - 🔴 Upward 3-arrow volley. A red mark appears on the ground 0.8 s before the arrows land where the player stands.
  - ⚪ Weak dagger slash (< 1.2 u).
- **Behaviour:** prefers high platforms and holds 6–10 u. When the player comes close, he hops back 2 u and then uses the dagger.
- **Frames:** base pose, idle 4, walk 8, shoot 5, volley 5, backstep 3, dagger 5, hurt 3, death 5.

### 3.6 Sorcerer: half-boss (supernatural)
- **Look:** the pasha's black sorcerer. Hood, amulets, carved staff. Not a religious figure.
- **Health:** 220. **Guard:** 60. Uses the boss bar at the bottom of the screen, with its own name.
- **Spells:**
  - 🔴 Black flame wave travelling along the ground: speed 6 u/s, height 0.9 u. Jump over it.
  - 🔴 3 curse orbs that slowly follow the player for 4 s. Dodge through them, or destroy them by hitting them.
  - Teleport: becomes smoke and reappears behind the player or far away. Cooldown 4 s; triggers when the player gets close.
- **Melee:**
  - ⚪ 2-hit staff combo.
  - 🔴 When cornered, slams the staff into the ground for a push wave (radius 1.5 u).
- **Phase 2 (< 50% health):** spells speed up and flame waves come in pairs.
- **Weaknesses:**
  - Defenceless for about 1 s while casting.
  - A parried staff stagger lasts 1.4 s.
- **Frames:**
  - Moves: base pose, idle 4, walk 8, staff combo 8, flame spell 6, orb spell 6, teleport 6 (vanish 3 + appear 3), ground slam 6, hurt 3, death 6.
  - Effects: flame wave 4, curse orb 4.

## 4. Bosses (human aghas)
Every boss is 12–25% taller than a regular janissary. A second phase starts at 50% health. Each uses the boss bar (`bossGroup`).

### 4.1 Kızıl Surlar: Standard-bearer Arslan
- **Who:** a giant standard-bearer. A torn crimson standard on his back, chained trophies.
- **Moves:**
  - ⚪ Long thrust with the standard pole.
  - 🔴 Wide arc sweep of the pole.
  - Plants the standard and summons 2 Sword Nefers. Once per phase.
- **Phase 2:** tears out the standard, sets the pole alight, and fights with the burning standard. Sweeps leave a short trail of fire.
- **Tests:** spacing and fighting a crowd.

### 4.2 Yanmış Mahalle: Chief Grenadier Kör Demir
- **Who:** the man who burned the quarter. Half his face is burned and one eye is blind; leather apron, clay grenades on his belt.
- **Weapon:** long hooked axe.
- **Moves:**
  - 🔴 Arcing grenade throw; leaves a fire patch on the ground for 4 s.
  - ⚪ Hook pull: pulls the player toward him.
  - 🔴 Oil-pot throw: a long burning strip.
- **Phase 2:** half the arena catches fire, so the fight happens in a narrow space.
- **Tests:** reading the ground and dodging at the right moment.

### 4.3 Kurt Geçidi: Warden of the Pass, Deli Kurtbaş
- **Who:** a wild delibaş in a wolf pelt with a wolf-head hood. Human; not supernatural.
- **Weapon:** an axe in each hand.
- **Moves:**
  - ⚪ Fast 4-hit axe storm.
  - Jumps over the player and lands behind.
  - 🔴 Leaps from the rocks and slams down.
- **Phase 2:** howls and goes into a frenzy. Hits do not stagger him, but he takes 25% more damage.
- **Tests:** parry rhythm. This is the fastest fight.

### 4.4 Karanlık Kale: Janissary Agha Kara Murad
- **Who:** a giant, muscular, tall fighter. About 25% taller than Karamani, with shoulders nearly twice as wide; bare muscular arms and an open vest over mail. Fully human: no magic, no elixir, no pistol, no summons.
- **Weapon:** a giant two-handed pala.
- **Phase 1 (pala):**
  - ⚪ 3-hit heavy pala combo with a slow, fast, slow rhythm.
  - 🔴 Shoulder charge from one end of the arena to the other.
  - 🔴 Grab and throw: if the player stands next to him without attacking, he grabs and slams them down (damage 30).
  - 🔴 Slams the pala into the ground, sending a shock wave. Jump over it.
- **Phase 2 (bare-handed):** throws away the pala. Fist combos, more frequent grabs, back-to-back charges; he is faster.
- **Tests:** everything the player has learned, plus distance discipline against grabs.

## 5. Zone distribution
| Zone | Regular enemies | Half-boss | Boss |
|---|---|---|---|
| Kızıl Surlar | 3 Nefer, 1 Shield Spearman, 1 Archer (on a platform) | – | Standard-bearer Arslan |
| Yanmış Mahalle | 2 Nefer, 2 Musketeer, 1 Halberdier | – | Chief Grenadier Kör Demir |
| Kurt Geçidi | 1 Spearman, 1 Halberdier, 2 Archer, 1 Musketeer | Sorcerer (first encounter) | Deli Kurtbaş |
| Karanlık Kale | Mixed from all, hardest set-ups | Sorcerer (stronger) | Agha Kara Murad |

This fits the current level length (about 2 screens). If the levels are extended (backlog B-05, a separate decision), the counts grow.

## 6. Technical structure
- **`EnemyRole`** becomes: `Nefer, Spearman, Halberdier, Musketeer, Archer, Sorcerer` + boss roles `Arslan, KorDemir, Kurtbas, KaraMurad`. The `Ghoul` and the shared `Captain` are removed.
- **`Assets/Scripts/EnemyProfiles.cs` (new): one data table per enemy:**
  - Fields: health, guard, speed, preferred range (min/max), phase threshold, attack list.
  - Each attack carries: name, clip, wind-up, active window, recovery, reach, damage, guard damage, ⚪/🔴, mobility (dash distance), projectile type.
  - Every value can be tuned from one place.
- **`Assets/Scripts/EnemyBrain.cs` (new): behaviour.**
  - Range keeping, retreat, attack selection by range and phase, attack order (2 at once), punishing openings.
  - Special behaviours: shield facing, turn delay, teleport, grab, summon.
  - The enemy part of `GameWorld.TickEnemy` moves here; `GameWorld` keeps physics and hit resolution.
- **Projectiles and area attacks** (generalised from the existing `Shoot`):
  - straight (bullet, arrow)
  - arcing (grenade)
  - ground-marked volley
  - ground-travelling wave (flame wave, shock wave)
  - homing orb
  - lingering fire patch
- **Unblockable attacks:** if the attack is 🔴, block and parry do not work in `Damage` / the block check.
- **Glint:** `Effects.Glint` gets a red variant.
- **Boss bar:** the existing `bossGroup` is used by the half-boss and bosses, with name and phase.
- **Frames:**
  - Each enemy goes into its own HD sheet `Resources/Characters/hd/<role>` via `Tools/hd_sheet.py` (same pipeline as Karamani).
  - Until the art arrives, the old pixel sheets act as placeholders: nefer→duelist, spearman→shield, archer and musketeer→archer, halberdier→duelist, sorcerer and bosses→captain.
  - The game logic works without waiting for art.

## 7. Tests (as `BuildTools.Verify` lines)
- **Every enemy:** every attack runs, shows the right clip at the right moment, and its hit lands only in the active window.
- **🔴 attacks:** blocking does not reduce the damage; a dodge avoids it fully.
- **Parry:** parrying a ⚪ attack causes a stagger, and an execution is possible.
- **Ranged:** the musketeer and archer hold 5–10 u; the musketeer cannot attack while reloading.
- **Shield Spearman:** a light hit from the front is blocked; a hit from behind deals guard damage.
- **Sorcerer:** teleports when approached, the orbs follow the player, phase 2 starts below 50%.
- **Bosses:** the phase changes, the special move runs (summon / fire patch / jump over / grab), the boss bar is visible.
- **Attack order:** at most 2 enemies are attacking at the same time.
- **EXE:** `RuntimeVerification` checks that every zone loads with its new enemies and that every boss starts.

## 8. Work order
1. This spec, then user approval.
2. Implementation plan (writing-plans).
3. Code: data table + behaviour + projectiles + tests, with placeholder art.
4. ChatGPT prompts for the 6 regular enemies and 4 bosses (`Docs/references/enemy-sheets.md`).
5. Integrate frames as they arrive; tune in playtesting.

## 9. Out of scope
- Level lengths and level design (B-05).
- The other 4 characters' enemies.
- Story scenes and dialogue.
- Sound design (existing sound events are reused).
