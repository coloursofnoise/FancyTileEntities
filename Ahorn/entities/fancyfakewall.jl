module FancyTileEntitiesFancyFakeWall

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyFakeWall" FancyFakeWall(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	playTransitionReveal::Bool=false, tileData::String="")

const placements = Ahorn.PlacementDict(
   "Fake Wall (FancyTileEntities)" => Ahorn.EntityPlacement(
      FancyFakeWall,
      "rectangle",
		Dict{String, Any}(
			"type" => "Wall"
		)
   ),
	"Fake Block (FancyTileEntities)" => Ahorn.EntityPlacement(
		FancyFakeWall,
      "rectangle",
		Dict{String, Any}(
			"type" => "Block"
		)
	),
)

Ahorn.minimumSize(entity::FancyFakeWall) = 8, 8
Ahorn.resizable(entity::FancyFakeWall) = true, true

Ahorn.selection(entity::FancyFakeWall) = Ahorn.getEntityRectangle(entity)

function Ahorn.propertyOptions(entity::FancyFakeWall, ignores::Array{String, 1}=String[])
	push!(ignores, "type")
	tileEntityConfigOptions(entity, ignores)
end


Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyFakeWall, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
