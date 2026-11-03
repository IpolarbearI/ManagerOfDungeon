# Fantasy Dungeon — Setup Guide (Nika Studio)

Get a playable top-down RPG running in **under 5 minutes**.

## 1. Import
1. Unity 6 (URP 2D template recommended). `Assets > Import Package > Custom Package` → select the `.unitypackage` → Import All.
2. Everything lives under `Assets/NikaStudio/Fantasy_Mega_Pack/`.

## 2. Try the demos (30 seconds)
Open and press Play:
- `Scenes/DemoLevel` — full RPG loop: move, fight, loot, chest, dodge.
- `Scenes/TestArena` — cycle all characters (A/D) and trigger every animation (1–6).
- `Scenes/DemoDungeon` — full cast showcase.

**Controls:** WASD/Arrows move · Shift run · Space attack · Q / right-click dodge-roll · E interact · F5 save · F9 load.

## 3. Use a character in YOUR scene
1. Drag a prefab from `Prefabs/Characters/` (e.g. `Hero`) into your scene.
2. Hero already has: controller, combat, health, inventory-ready setup.
3. Tag the hero **"Player"** (enemies need it to find their target).
4. Drag any enemy prefab (Skeleton, Orc, Bat…) in — it chases + attacks the Player automatically (`EnemyAI`).

## 4. The C# systems (drop-in)
| System | How to use |
|---|---|
| `TopDownCharacterController` | On the Hero prefab. 8-direction movement, walk/run. |
| `Health` | Add to anything damageable. `TakeDamage(int)`, `Heal(int)`, `onDeath` event, `invulnerable` flag. |
| `PlayerCombat` | Melee on Space. Set `damage`, `attackRadius`. |
| `PlayerDodge` | Dodge-roll with i-frames on Q/right-click. |
| `EnemyAI` | Chase + attack the Player. Tune `chaseRange`, `attackRange`, `damage`. |
| `InventorySystem` | Add to player. `Add(ItemSO, count)` / `Remove(...)`. Fires `OnChanged`. |
| `ItemSO` | Create items: `Assets > Create > FantasyDungeon > Item`. Set icon from `UI/Icons` (224 included). |
| `LootTableSO` + `LootDropper` | Create table: `Assets > Create > FantasyDungeon > Loot Table`. Put `LootDropper` on an enemy, assign table → drops pickups on death. |
| `StatsSystem` | HP/MP/XP/Level. `AddXP(int)` → auto level-up. Drives bars via `HPPercent`/`MPPercent`/`XPPercent`. |
| `FloatingDamageNumbers` | Automatic — every `TakeDamage` shows a floating number. |
| `InteractionSystem` + `ChestInteractable` | On player. Press E near chests/NPCs. Make anything interactable via `IInteractable`. |
| `EnemySpawner` + `WaveConfigSO` | Wave survival: create config (`Assets > Create > FantasyDungeon > Wave Config`), assign enemy prefabs. |
| `DialogueRunner` | `DialogueRunner.Instance.Begin("Name", new[]{"Line 1","Line 2"})` — typewriter box. |
| `SaveSystem` | On player. F5/F9. For inventory persistence, fill its `itemDatabase` list with your ItemSO assets. |
| `CameraFollow` | On the camera, assign target → smooth follow. |

## 5. Build a map
- Tile sprites: `Sprites/Tiles` (128 PPU, grid-ready). Paint with Unity Tilemap (Window > 2D > Tile Palette).
- Props/decor/traps: `Sprites/Environment`.
- Walls: add `TilemapCollider2D` to the wall Tilemap.

## 6. Animations
- Each character: 6 animations × 4 directions (Animator controllers in `Animations/Character`).
- Drive via parameters: `MoveX`, `MoveY`, `Speed`, `IsRunning` + triggers `Attack`, `Hurt`, `Die`.
- Hero skins: `Sprites/HeroSkins` (swap the SpriteRenderer sprite or build variants).

## 7. UI & icons
- Bars/panels/buttons: `UI/Elements`. 224 item/spell icons: `UI/Icons` (64px, transparent).

## Support & feedback
Found a bug? Want something in the next pack? → Discord / survey links in readme.txt.

— Nika Studio · License: see License.txt · Art AI-assisted (disclosed), all code human-made.
