module FancyTileEntitiesFancyFloatySpaceBlock

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyFloatySpaceBlock" FancyFloatySpaceBlock(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	disableSpawnOffset::Bool=false, connectsTo::String="3", tileData::String="")

const placements = Ahorn.PlacementDict(
    "Floaty Space Block (FancyTileEntities)" => Ahorn.EntityPlacement(
        FancyFloatySpaceBlock,
        "rectangle",
		Dict{String, Any}()
    ),
)

Ahorn.minimumSize(entity::FancyFloatySpaceBlock) = 8, 8
Ahorn.resizable(entity::FancyFloatySpaceBlock) = true, true

Ahorn.selection(entity::FancyFloatySpaceBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.editingOptions(entity::FancyFloatySpaceBlock) = Dict{String, Any}(
    "connectsTo" => Ahorn.tiletypeEditingOptions()
)

Ahorn.propertyOptions(entity::FancyFloatySpaceBlock, ignores::Array{String, 1}=String[]) = tileEntityConfigOptions(entity, ignores)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyFloatySpaceBlock, room::Maple.Room) = renderTileEntity(ctx, entity, room)

end
