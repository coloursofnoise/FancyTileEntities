module FancyTileEntitiesFancyIntroCrusher

using ..Ahorn, Maple
using Ahorn.AhornTileEntity

#tiletype kept so I don't have to deal with it in the class
@mapdef Entity "FancyTileEntities/FancyIntroCrusher" FancyIntroCrusher(x::Int, y::Int,
	width::Int=Maple.defaultBlockWidth, height::Int=Maple.defaultBlockHeight,
	manualTrigger::Bool=false, delay::Float64=1.2, speed::Float64=2.0,
	tiletype::String="3", flags::String="1,0b", tileData::String="")

const placements = Ahorn.PlacementDict(
   "Intro Crusher (FancyTileEntities)" => Ahorn.EntityPlacement(
      	FancyIntroCrusher,
      	"rectangle",
	  	Dict{String, Any}(),
		function(entity)
         entity.data["nodes"] = [(Int(entity.data["x"]) + 40, Int(entity.data["y"]))]
      end
   ),
)

Ahorn.nodeLimits(entity::FancyIntroCrusher) = 0, 1

Ahorn.minimumSize(entity::FancyIntroCrusher) = 8, 8
Ahorn.resizable(entity::FancyIntroCrusher) = true, true

function Ahorn.selection(entity::FancyIntroCrusher)
	x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
end

function Ahorn.propertyOptions(entity::FancyIntroCrusher, ignores::Array{String, 1}=String[])
	push!(ignores, "tiletype")
	Ahorn.AhornTileEntity.tileEntityConfigOptions(entity, ignores)
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyIntroCrusher, room::Maple.Room) = renderTileEntity(ctx, entity, room)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FancyIntroCrusher, room::Maple.Room)
	x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])
        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

		renderTileEntity(ctx, entity, room, nx-x, ny-y)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end
