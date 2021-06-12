module FancyTileEntitiesBetterIntroCrusher

using ..Ahorn, Maple

@mapdef Entity "FancyTileEntities/BetterIntroCrusher" BetterIntroCrusher(x::Int, y::Int,
    width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
    manualTrigger::Bool=false, delay::Float64=1.2, speed::Float64=2.0,
    tiletype::String="3", flags::String="1,0b")

const placements = Ahorn.PlacementDict(
    "Better Intro Crusher (FancyTileEntities)" => Ahorn.EntityPlacement(
        BetterIntroCrusher,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
            Ahorn.tileEntityFinalizer(entity)
        end,
    ),
)

Ahorn.editingOptions(entity::BetterIntroCrusher) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.nodeLimits(entity::BetterIntroCrusher) = 1, 1

# The Intro Crusher functionality only works if the entity is >= 22 wide
Ahorn.minimumSize(entity::BetterIntroCrusher) = 24, 8
Ahorn.resizable(entity::BetterIntroCrusher) = true, true

function Ahorn.selection(entity::BetterIntroCrusher)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BetterIntroCrusher, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, blendIn=false)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BetterIntroCrusher, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    
    if !isempty(nodes)
        nx, ny = Int.(nodes[1])
        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

        material = get(entity.data, "tiletype", "3")[1] 

        fakeTiles = Ahorn.createFakeTiles(room, nx, ny, width, height, material, blendIn=false)
        Ahorn.drawFakeTiles(ctx, room, fakeTiles, room.objTiles, true, nx, ny, clipEdges=true)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end