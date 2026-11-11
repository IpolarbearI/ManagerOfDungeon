# Fantasy Dungeon Pixel Pack — Top-Down RPG

A complete top-down pixel-style RPG kit: animated characters, dungeon tileset, props, a ready Tile Palette, prefabs, animator controllers, a playable demo scene, and a sample C# controller. Drop it in and build.

---

## ✨ What's included
- **Characters** (Knight + Skeleton, Slime, Goblin, Orc, Dark-Knight Boss) — each with **Idle · Walk · Run · Attack · Hurt · Death**, 4 directions.
- **Dungeon Tileset** — floors, walls (+corners/edges), door, stairs, water.
- **Props** — chest, barrel, crate, torch, pot, skulls, mushroom, web, key, gold.
- **Unity Tile Palette** asset (paint instantly).
- **Prefabs** — player, props, lit torch.
- **Animator Controllers** per character (Idle/Walk/Run/Attack/Hurt/Death).
- **Demo scene** (a playable dungeon room).
- **Sample C# controller** (`TopDownCharacterController.cs`).

*(Exact counts in the store description.)*

## 🎮 Demo controls
- **WASD / Arrows** — move · **Left Shift** — run · **Space** — attack · **H** — hurt · **K** — die.

## ⚙️ Import / setup
- **Pixels-Per-Unit:** 256 (tiles = 256px = 1 unit; characters share PPU 256). Use a **Pixel Perfect Camera** for crispest pixels.
- Sprite import (already configured): **Filter Mode = Point**, **Compression = None**, **Mesh = Full Rect**, **Pivot = Bottom** (characters) / Center (tiles), no mip maps.
- **Render pipeline:** built for **URP (2D Renderer)** — lit-torch demo uses 2D Lights. The raw sprites are pipeline-neutral and also work in Built-in (default sprite materials).
- Scripts target **Unity 6+** (`linearVelocity`) and auto-support **both** the legacy Input Manager and the new Input System (no setup needed).
- Included C# systems: `TopDownCharacterController`, `Health`, `EnemyAI`, `PlayerCombat`, `SaveSystem` (JSON F5/F9). Demo-ready, drop-in, commented.

## 🤖 AI Disclosure
Per Unity Asset Store guidelines: **the 2D pixel-style sprites in this pack were created with the help of AI image generation, then curated, assembled, and integrated by hand.** Animator setup, tile palette, prefabs, demo scene, scripts and documentation are human-made.

## 📜 License
Standard Unity Asset Store EULA. You may use these assets in commercial and non-commercial games. You may **not** resell or redistribute the assets themselves.

## 💬 Support
[CONTACT / EMAIL / DISCORD]

Thanks for your purchase — happy dungeon-crawling! 🛡️
