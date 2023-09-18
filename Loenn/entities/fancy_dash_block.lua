local dashBlock = {}

local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

dashBlock.name = "FancyTileEntities/FancyDashBlock"
dashBlock.depth = 0
dashBlock.placements = {
    name = "dash_block",
    data = {
        tileData = "",
        blendin = true,
        canDash = true,
        permanent = true,
        width = 8,
        height = 8
    }
}

dashBlock.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

dashBlock.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return dashBlock