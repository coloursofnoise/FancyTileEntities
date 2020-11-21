module FancyTileEntitiesFancyConditionBlock

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyConditionBlock=LoadConditionBlock" FancyConditionBlock(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	condition::String="Key", conditionID::String="1:1", tileData::String="")

const placements = Ahorn.PlacementDict(
   "Condition Block (FancyTileEntities)" => Ahorn.EntityPlacement(
      FancyConditionBlock,
      "rectangle",
	  Dict{String, Any}()
   ),
)


Ahorn.minimumSize(entity::FancyConditionBlock) = 8, 8
Ahorn.resizable(entity::FancyConditionBlock) = true, true

Ahorn.selection(entity::FancyConditionBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.editingOptions(entity::Maple.ConditionBlock) = Dict{String, Any}(
    "condition" => Maple.condition_block_conditions
)
Ahorn.propertyOptions(entity::FancyConditionBlock, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyConditionBlock, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
