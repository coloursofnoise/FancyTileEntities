module FancyTileEntitiesEntityTrigger

using ..Ahorn, Maple

@mapdef Trigger "everest/entityTrigger" EntityTrigger(x::Integer, y::Integer, 
	width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
	manualTrigger::Bool=false, persistent::Bool=false)
	
const placements = Ahorn.PlacementDict(
	"Entity Trigger (Everest)" => Ahorn.EntityPlacement(
		EntityTrigger,
		"rectangle",
		Dict{String, Any}(),
		function(trigger)
			trigger.data["nodes"] = [
				 (Int(trigger.data["x"]) + Int(trigger.data["width"]) + 8, Int(trigger.data["y"])),
				 (Int(trigger.data["x"]) + Int(trigger.data["width"]) + 16, Int(trigger.data["y"]))
			]
		end
	)
)

Ahorn.nodeLimits(trigger::EntityTrigger) = 2, 2

end 