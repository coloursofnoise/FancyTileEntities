module FancyTileEntitiesFancySolidTiles

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancySolidTiles" FancySolidTiles(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	blendEdges::Bool=false, randomSeed::Int=0, tileData::String="")

const placements = Ahorn.PlacementDict(
   "Solid Block (FancyTileEntities)" => Ahorn.EntityPlacement(
      FancySolidTiles,
      "rectangle",
	  Dict{String, Any}()
   ),
)


Ahorn.minimumSize(entity::FancySolidTiles) = 8, 8
Ahorn.resizable(entity::FancySolidTiles) = true, true

Ahorn.selection(entity::FancySolidTiles) = Ahorn.getEntityRectangle(entity)

Ahorn.propertyOptions(entity::FancySolidTiles, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancySolidTiles, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
