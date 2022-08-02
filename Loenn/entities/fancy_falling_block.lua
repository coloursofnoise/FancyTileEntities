local fallingBlock = {}

fallingBlock.name = "FancyTileEntities/FancyFallingBlock"
fallingBlock.placements = {
    {
        name = "falling_block",
        data = {
            tileData = "",
            climbFall = true,
            behind = false,
            manualTrigger = false,
            finalBoss = false,
            width = 8,
            height = 8
        }
    },
    {
        name = "boss_falling_block",
        data = {
            tileData = "",
            tileDataHighlight = "",
            behind = false,
            finalBoss = true
            width = 8,
            height = 8
        }
    }
}

fallingBlock.ignoredFields = {
    finalBoss
}

function fallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return fallingBlock