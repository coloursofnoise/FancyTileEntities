module FancyTileEntitiesFancyDashBlock

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyDashBlock" FancyDashBlock(x::Int, y::Int,
   width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
   blendin::Bool=true, canDash::Bool=true,
   permanent::Bool=true, tileData::String="")

const placements = Ahorn.PlacementDict(
   "Dash Block (FancyTileEntities)" => Ahorn.EntityPlacement(
      FancyDashBlock,
      "rectangle",
      Dict{String, Any}()
   ),
)

Ahorn.minimumSize(entity::FancyDashBlock) = 8, 8
Ahorn.resizable(entity::FancyDashBlock) = true, true

Ahorn.selection(entity::FancyDashBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.propertyOptions(entity::FancyDashBlock, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyDashBlock, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
