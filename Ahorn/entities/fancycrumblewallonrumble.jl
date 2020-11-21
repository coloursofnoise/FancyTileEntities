module FancyTileEntitiesFancyCrumbleWallOnRumble

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyCrumbleWallOnRumble" FancyCrumbleWallOnRumble(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	blendin::Bool=true, persistent::Bool=false, tileData::String="")

const placements = Ahorn.PlacementDict(
   "Crumble Wall On Rumble (FancyTileEntities)" => Ahorn.EntityPlacement(
      FancyCrumbleWallOnRumble,
      "rectangle",
		Dict{String, Any}()
   ),
)

Ahorn.minimumSize(entity::FancyCrumbleWallOnRumble) = 8, 8
Ahorn.resizable(entity::FancyCrumbleWallOnRumble) = true, true

Ahorn.selection(entity::FancyCrumbleWallOnRumble) = Ahorn.getEntityRectangle(entity)

Ahorn.propertyOptions(entity::FancyCrumbleWallOnRumble, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyCrumbleWallOnRumble, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
