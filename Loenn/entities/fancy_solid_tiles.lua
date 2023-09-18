local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

local solidTiles = {}

solidTiles.name = "FancyTileEntities/FancySolidTiles"
solidTiles.placements = {
    name = "solid_tiles",
    data = {
        tileData = "0",
        randomSeed = 0,
        blendEdges = true,
        width = 8,
        height = 8
    }
}

solidTiles.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

solidTiles.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return solidTiles