module FancyTileEntitiesFancyExitBlock

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyExitBlock" FancyExitBlock(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	playTransitionReveal::Bool=false, tileData::String="")

const placements = Ahorn.PlacementDict(
   "Exit Block (FancyTileEntities)" => Ahorn.EntityPlacement(
      FancyExitBlock,
      "rectangle",
		Dict{String, Any}()
   ),
)

Ahorn.minimumSize(entity::FancyExitBlock) = 8, 8
Ahorn.resizable(entity::FancyExitBlock) = true, true

Ahorn.selection(entity::FancyExitBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.propertyOptions(entity::FancyExitBlock, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyExitBlock, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
