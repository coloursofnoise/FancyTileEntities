local crumbleWall = {}

local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

crumbleWall.name = "FancyTileEntities/FancyCrumbleWallOnRumble"
crumbleWall.placements = {
    name = "crumble_wall",
    data = {
        tileData = "0",
        blendin = true,
        persistent = false,
        width = 8,
        height = 8
    }
}

function crumbleWall.depth(room, entity)
    return entity.blendin and -10501 or -12999
end

crumbleWall.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

crumbleWall.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return crumbleWall