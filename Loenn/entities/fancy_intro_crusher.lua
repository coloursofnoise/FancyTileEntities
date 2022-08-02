local introCrusher = {}

introCrusher.name = "FancyTileEntities/FancyIntroCrusher"
introCrusher.depth = 0
introCrusher.nodeLineRenderType = "line"
introCrusher.nodeLimits = {1, 1}
introCrusher.placements = {
    name = "intro_crusher",
    data = {
        tileData = "",
        flags = "1, 0b",
        delay = 1.2,
        speed = 2.0,
        width = 8,
        height = 8
    }
}

return introCrusher