local fakeWall = {}

local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

fakeWall.name = "FancyTileEntities/FancyFakeWall"
fakeWall.depth = -13000
fakeWall.placements = {
    name = "fake_wall",
    data = {
        tileData = "0",
        width = 8,
        height = 8
    }
}

fakeWall.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

fakeWall.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return fakeWall