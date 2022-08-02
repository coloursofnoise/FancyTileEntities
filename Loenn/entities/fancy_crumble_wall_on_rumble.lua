local crumbleWall = {}

crumbleWall.name = "FancyTileEntities/FancyCrumbleWallOnRumble"
crumbleWall.placements = {
    name = "crumble_wall",
    data = {
        tileData = "",
        blendin = true,
        persistent = false,
        width = 8,
        height = 8
    }
}

function crumbleWall.depth(room, entity)
    return entity.blendin and -10501 or -12999
end

return crumbleWall