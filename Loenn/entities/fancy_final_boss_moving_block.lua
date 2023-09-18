local utils = require("utils")

local movingBlock = {}
local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

movingBlock.name = "FancyTileEntities/FancyFinalBossMovingBlock"
movingBlock.depth = 0
movingBlock.nodeLineRenderType = "line"
movingBlock.nodeLimits = {1, 1}
movingBlock.fieldInformation = {
    nodeIndex = {
        fieldType = "integer",
    }
}
movingBlock.placements = {
    name = "moving_block",
    data = {
        nodeIndex = 0,
        width = 8,
        height = 8,
        tileData = "",
        tileDataHighlight = ""
    }
}

movingBlock.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    },
    tileDataHighlight = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

movingBlock.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})


return movingBlock