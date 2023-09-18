local ridgeGate = {}
local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

ridgeGate.name = "FancyTileEntities/FancyRidgeGate"
ridgeGate.depth = 0
ridgeGate.nodeLineRenderType = "line"
ridgeGate.nodeLimits = {1, 1}
ridgeGate.placements = {
    name = "ridge_gate",
    data = {
        tileData = "",
        flag = "",
        width = 8,
        height = 8
    }
}

ridgeGate.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

ridgeGate.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})


return ridgeGate