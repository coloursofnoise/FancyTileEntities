local trigger = {}

trigger.name = "everest/entityTrigger"
trigger.nodeLineRenderType = "fan"
trigger.nodeLimits = {2, 2}
trigger.placements = {
    name = "entity",
    data = {
        manualTrigger = false,
        persistent = false
    }
}

return trigger