# Stardew Armors

A Stardew Valley 1.6 mod that adds stat-granting armor pieces, crafted from the ores used for vanilla weapons and worn in a new Armor equipment slot.

## Project structure

- `StardewArmors.slnx`: the Visual Studio solution.
- `[CP] Stardew Armors/`: the Content Patcher content pack (items, recipes and shops data).
- `StardewArmors/`: the C# SMAPI mod (Armor slot via SpaceCore and stat buff logic).
- `docs/`: design notes; not shipped.
- `art-source/`: source art files; not shipped.
- `release/`: holds the release zips.

For release, both mod folders are zipped together inside a single `Stardew Armors/` parent folder.
