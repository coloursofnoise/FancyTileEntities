local exitBlock = {}

local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

exitBlock.name = "FancyTileEntities/FancyExitBlock"
exitBlock.depth = -13000
exitBlock.placements = {
    name = "exit_block",
    data = {
        tileData = "",
        playTransitionReveal = false,
        width = 8,
        height = 8
    }
}

exitBlock.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

exitBlock.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return exitBlock