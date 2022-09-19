local fakeTilesHelper = require("helpers.fake_tiles")

local floatySpaceBlock = {}

floatySpaceBlock.name = "floatySpaceBlock"
floatySpaceBlock.depth = -9000
floatySpaceBlock.placements = {
    name = "floaty_space_block",
    data = {
        connectsTo = "3",
        tileData = "",
        disableSpawnOffset = false,
        randomSeed = 0,
        width = 8,
        height = 8
    }
}

floatySpaceBlock.fieldInformation = fakeTilesHelper.getFieldInformation("connectsTo")

return floatySpaceBlock