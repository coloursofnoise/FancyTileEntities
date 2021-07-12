# Fancy Tile Entities - Celeste Mod
A code mod for the [Everest](https://everestapi.github.io/) mod loader for [Celeste](http://www.celestegame.com/).

Adds placements for vanilla tile-entities with arbitrary shapes and multiple tile types.

Comes with a custom Ahorn editing menu for Fancy placements.  
:warning: When editing a tile entity, make sure to press `Update` in the tile editing window, then `Update` in the regular properties window to save your changes.

## Other Features:
- **Entity Trigger** - Ahorn placement for the Everest [`Entity Trigger`](https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Entities/EntityTrigger.cs) (integrated with some fancy tile entities).
- **Better Intro Crusher** - non-fancy Intro Crusher with additional features: `manualTrigger` mode for use with `Entity Triggers`, `delay` and `speed` to configure activation delay and move speed, respectively.
- **Tile Seed Controller** - WiP controller entity that can be used to apply consistent RNG to foreground and/or background tiles in a room.


---

To include this mod in your map, add the following to the `Dependencies` section of your [everest.yaml](https://github.com/EverestAPI/Resources/wiki/Mod-Structure#using-helper-mods):
```yaml
    - Name: FancyTileEntities
      Version: 1.5.0
```

Report bugs to `@coloursofnoise` on the [Celeste Discord](discord.gg/celeste) or by opening an issue on [Github](https://github.com/coloursofnoise/FancyTileEntities/issues).