module FancyTileEntitiesTileSeedController

using ..Ahorn, Maple

@mapdef Entity "FancyTileEntities/TileSeedController" Controller(x::Int, y::Int, fg::Bool=true, bg::Bool=true, randomSeed::Int=42, localPosition::Bool=true)

const placements = Ahorn.PlacementDict(
   "Tile Seed Controller (FancyTileEntities)" => Ahorn.EntityPlacement(
      Controller
   )
)

end 