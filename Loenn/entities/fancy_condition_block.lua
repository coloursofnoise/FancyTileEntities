local enums = require("consts.celeste_enums")

local conditionBlock = {}

conditionBlock.name = "FancyTileEntities/FancyConditionBlock=LoadConditionBlock"
conditionBlock.depth = -13000
conditionBlock.placements = {
    name = "condition_block",
    data = {
        tileData = "",
        condition = "Key",
        conditionID = "1:1",
        width = 8,
        height = 8
    }
}

conditionBlock.fieldInformation = {
    condition = {
        options = enums.condition_block_conditions
    }
}

return conditionBlock