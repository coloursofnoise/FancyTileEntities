module FancyTileEntitiesFancyCoverupWall

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyCoverupWall" FancyCoverupWall(x::Integer, y::Integer,
	width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
	tileData::String="", blendIn::Bool=true)

const placements = Ahorn.PlacementDict(
	"Coverup Wall (FancyTileEntities)" => Ahorn.EntityPlacement(
     	FancyCoverupWall,
     	"rectangle",
		Dict{String, Any}()
   ),
)

Ahorn.minimumSize(entity::FancyCoverupWall) = 8, 8
Ahorn.resizable(entity::FancyCoverupWall) = true, true

Ahorn.selection(entity::FancyCoverupWall) = Ahorn.getEntityRectangle(entity)

Ahorn.propertyOptions(entity::FancyCoverupWall, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyCoverupWall, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
