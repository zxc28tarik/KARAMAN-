# Karamani: locked design constraints

These are decisions the user has made. Agents MUST NOT change, simplify or "improve" them. If a task seems to need one changed, stop and report it to the integrator (main session); only the user can change these.

Sources: the user's decisions from 2026-10-06 to 2026-10-08 (main-session memory), `../AKTARIM.md` (full design handoff, Turkish) and the design doc in Claude Docs. If this file and AKTARIM.md disagree, the newer user decision wins; ask when unsure.

## Naming and history
- Call the first character **Karamani**, never "Tarık", in all docs, UI and chat.
- 1566 is only the setting year. Do not use real historical events to shape the story or content. The world is the user's own dark fantasy.
- The user's language is Turkish. In-game text is Turkish. Existing code comments are Turkish; match the surrounding file.

## World and structure
- Dark-fantasy world in 1566, set only in Ottoman lands and Europe and centered on the Ottoman / Holy Roman (Hungarian) border. No Central Asia.
- Races: humans, vampires, werewolves and goblins are confirmed.
- Tone is mixed: serious, epic and comedic.
- Chapters follow each other in order, each from one character's point of view. Each chapter is its own ~30-minute mission map with side quests, in the style of Warcraft 3 campaigns.
- **No backtracking.** Chapters flow one way, in the style of Prince of Persia or Dan the Man. Small key/hook loops inside a chapter are allowed only rarely.
- Character level and items carry over between chapters.
- Combat, exploration and platforming are balanced. Inspirations: Castlevania SotN, Bloodstained, Dead Cells, Prince of Persia, Dan the Man. Combat references: Odin Sphere, Muramasa, Blasphemous 2, Otherworld Legends.
- Do NOT design smith, alchemist or crafting NPCs (the user will describe them) or cutscenes (later).

## Playable roster (do not alter kits)
| Character | Role | Locked facts |
| --- | --- | --- |
| **Karamani** (Akıncı Çavuşu, 40s) | Fighter/bruiser (NOT assassin) | Master of bow, sword, pala, pistol, spear and javelin (cirit). His melee weapon is a real **kılıç (sword), not a pala**. Agile, strong, fast. NO magic: strong from the start, grows mainly through weapon quality. Many weapon slots. Horse is a summonable ability (example: 15 s every 3 min); fully mounted chapters exist. The opening mission is an escape from Istanbul against 100 Janissaries: 6–7 waves, 3–4 veteran semi-bosses, final boss an Ağa, and he sends his horse away. |
| **Ludwig von Rosenstein** (21, German noble) | Tank | Heavier armor, slower, fewer weapon slots. Sword; two pistols OR an arquebus; cavalry hammer. **Pistol rule:** pistols are NOT fast. Reload time equals the arquebus, damage and range are lower, and the only advantage is two shots before reloading. His mentor Ernst dies because of Ludwig's carelessness. |
| **Álmos de genere Kán** (vampire, looks 30) | Magic + physical hybrid | A physician who never drinks human blood, so he is weaker than other vampires and strong in bloodline abilities. His slots are ability-based, not weapon-based. |
| **Kutluca** (~12–13, ancient Turkic noble) | Mage in human form, assassin in creature form | Awakened wolf blood. The creature's exact look stays mysterious. At first he transforms involuntarily; the progression tree teaches control. Heirlooms: his grandmother's cane, his father's dagger and his mother's prayer beads (tesbih). Do NOT invent what the cane or tesbih do. |
| **Kısmet** (goblin) | Fighter-assassin, comedic | Wide short blade (kama) and darts. Stealth abilities can randomly **malfunction** (about 20% at early levels). Very lucky. |

- Plot twist: the same hidden killer destroyed Álmos's town and Kutluca's family. Do not reveal it early.
- Weapons system: each character has their own weapons and a different number of slots.

## Current production slice (KaramaniReborn)
- Playable now: Karamani on 4 zones (Kızıl Surlar, Yanmış Mahalle, Kurt Geçidi, Karanlık Kale). Each zone has 5 guards and a gate commander, checkpoints (yol taşı), a 3-hit buffered combo, heavy attack, block / timed parry / riposte execution, dodge roll, backdash, double jump with flip, ledge grab and climb, downward sword dive, wolf dash (Q), pistol (R, 8 shots), potions (H). Controls are listed in `../README.md`.
- These mechanics and their tuning are verified by `BuildTools.Verify`. Do not replace them with simpler versions. Visual work must keep every PASS line in `Review/verification.txt`.
- This slice does not yet include the HTML prototype's 100-Janissary Istanbul escape. Building it is a design task for the user and the integrator, not for specialists.

## Visual direction locks (details in ART_DIRECTION.md)
- The Last Faith-style dark gothic pixel art: world at 720×540 game pixels (game pixel = 0.04 units, ortho 10.8, 2× at 1080 lines), limited palette per zone.
- 4:3 framing. **The hero is about 12% of view height** (user, 2026-10-11: "soth gibi küçük", then "yüzde 12 yaparmısın"). The view was widened 2×; world-unit sizes, physics and combat ranges are unchanged. 16:9 is still undecided.
- **HD characters** (user, 2026-10-11, choice 1a): playable characters are drawn at screen resolution (50 px per unit at 1080 lines, Karamani ~129 px tall) on the UI layer over the chunky pixel world. Clips without HD frames fall back to the pixel sheet.
- Mood: 90s–2000s dark anime, moderately dark.
- The 10 AI-painted source PNGs in `Assets/Resources/Art` are never edited in place; SHA-256 checks are in QA.md.
- **Characters are single-piece, frame-by-frame sprites** (user, 2026-10-11: "karakterleri ayırma parçalara kol bacak vb diye tek parça karakterler inşa et"). No cut-out or part rigs in the game: no separately drawn arms, legs or torso moved as pieces. Every animation frame is one complete drawing of the whole character. Skeletons or poses may be used internally by a generator, but what ships is whole frames. `PaintedRig` (rigid painted parts) is retired from gameplay.
