module FancyTileEntitiesFancyFinalBossMovingBlock

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@pardef FancyFinalBossMovingBlock(x1::Int, y1::Int, x2::Int=x1+16, y2::Int=y1,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight, nodeIndex::Int=0,
	tileData::String="", tileDataHighlight::String="") =
		Entity("FancyTileEntities/FancyFinalBossMovingBlock", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)],
		width=width, height=height, nodeIndex=nodeIndex, tileData=tileData, tileDataHighlight=tileDataHighlight)

const placements = Ahorn.PlacementDict(
    "Badeline Boss Moving Block (FancyTileEntities)" => Ahorn.EntityPlacement(
        FancyFinalBossMovingBlock,
        "rectangle",
		Dict{String, Any}(),
		function(entity)
    		entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    ),
)

Ahorn.nodeLimits(entity::FancyFinalBossMovingBlock) = 1, 1
Ahorn.minimumSize(entity::FancyFinalBossMovingBlock) = 8, 8
Ahorn.resizable(entity::FancyFinalBossMovingBlock) = true, true

function Ahorn.selection(entity::FancyFinalBossMovingBlock)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
end

function Ahorn.propertyOptions(entity::FancyFinalBossMovingBlock, ignores::Array{String, 1}=String[])
	Ahorn.AhornTileEntity.tileEntityConfigOptions(entity, ignores)
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyFinalBossMovingBlock, room::Maple.Room) = renderTileEntity(ctx, entity, room)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyFinalBossMovingBlock, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])
        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

		renderTileEntity(ctx, entity, String(get(entity.data, "tileDataHighlight", "")), room, nx-x, ny-y)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end
