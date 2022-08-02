local fakeTilesHelper = require("helpers.fake_tiles")

local ridgeGate = {}

ridgeGate.name = "FancyTileEntities/BetterRidgeGate"
ridgeGate.depth = 0
ridgeGate.nodeLineRenderType = "line"
ridgeGate.nodeLimits = {0, 1}
ridgeGate.justification = {0.0, 0.0}
ridgeGate.placements = {
    name = "ridge_gate",
    data = {
        tiletype = "3",
        flag = ""
    }
}

ridgeGate.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
ridgeGate.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return ridgeGate