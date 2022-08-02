local dashBlock = {}

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

return dashBlock