module FancyTileEntitiesFancyRidgeGate

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

@mapdef Entity "FancyTileEntities/FancyRidgeGate" FancyRidgeGate(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	tileData::String="", flag::String="")

const placements = Ahorn.PlacementDict(
   "Ridge Gate (FancyTileEntities)" => Ahorn.EntityPlacement(
		FancyRidgeGate,
		"rectangle",
	  	Dict{String, Any}(),
		function(entity)
         entity.data["nodes"] = [(Int(entity.data["x"]) + 40, Int(entity.data["y"]))]
		end
   ),
)

Ahorn.nodeLimits(entity::FancyRidgeGate) = 1, 1

Ahorn.minimumSize(entity::FancyRidgeGate) = 8, 8
Ahorn.resizable(entity::FancyRidgeGate) = true, true

function Ahorn.selection(entity::FancyRidgeGate)
	x, y = Ahorn.position(entity)
   nx, ny = Int.(entity.data["nodes"][1])

   width = Int(get(entity.data, "width", 8))
   height = Int(get(entity.data, "height", 8))

   return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
end

function Ahorn.propertyOptions(entity::FancyRidgeGate, ignores::Array{String, 1}=String[])
	Ahorn.AhornTileEntity.tileEntityConfigOptions(entity, ignores)
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyRidgeGate, room::Maple.Room) = renderTileEntity(ctx, entity, room)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyRidgeGate, room::Maple.Room)
	x, y = Ahorn.position(entity)
   nodes = get(entity.data, "nodes", ())

   width = Int(get(entity.data, "width", 8))
   height = Int(get(entity.data, "height", 8))

   if !isempty(nodes)
      nx, ny = Int.(nodes[1])
      cox, coy = floor(Int, width / 2), floor(Int, height / 2)

	  renderTileEntity(ctx, entity, room, nx-x, ny-y)
      Ahorn.drawArrow(ctx, nx + cox, ny + coy, x + cox, y + coy, Ahorn.colors.selection_selected_fc, headLength=6)
   end
end

end
