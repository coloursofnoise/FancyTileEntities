module FancyTileEntitiesFancyFallingBlock

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyFallingBlock" FancyFallingBlock(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	climbFall::Bool=true, behind::Bool=false, tileData::String="", tileDataHighlight::String="",
	manualTrigger::Bool=false,
)

const placements = Ahorn.PlacementDict(
	 "Falling Block (FancyTileEntities)" => Ahorn.EntityPlacement(
		  FancyFallingBlock,
		  "rectangle",
		Dict{String, Any}(
			"finalBoss" => false
		)
    ),
    "Badeline Boss Falling Block (FancyTileEntities)" => Ahorn.EntityPlacement(
        FancyFallingBlock,
        "rectangle",
		  Dict{String, Any}(
  			"finalBoss" => true
  		)
    ),
)

Ahorn.minimumSize(entity::FancyFallingBlock) = 8, 8
Ahorn.resizable(entity::FancyFallingBlock) = true, true

Ahorn.selection(entity::FancyFallingBlock) = Ahorn.getEntityRectangle(entity)

function Ahorn.propertyOptions(entity::FancyFallingBlock, ignores::Array{String, 1}=String[])
	push!(ignores, "finalBoss")
	if Bool(get(entity.data, "finalBoss", false))
		push!(ignores, "climbFall", "behind")
	else
		push!(ignores, "tileDataHighlight")
	end
	Ahorn.AhornTileEntity.tileEntityConfigOptions(entity, ignores)
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyFallingBlock, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
