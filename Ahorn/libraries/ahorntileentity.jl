module AhornTileEntity

using ..Ahorn, Maple
using Gtk, Cairo

export renderTileEntity, tileEntityConfigOptions, TileData

struct TileData
	width
	height
end

function renderTileEntity(ctx, entity, tileData, room, xOffset=0, yOffset=0)
	width = Int(get(entity.data, "width", 32))
	height = Int(get(entity.data, "height", 32))

	if tileData == ""
		x = Int(get(entity.data, "x", 0)) + xOffset
		y = Int(get(entity.data, "y", 0)) + yOffset
		Ahorn.drawRectangle(ctx, x, y, width, height, (0.6, 0.2, 0.6, 0.8), (0.6, 0.2, 0.6, 0.6))
	else
		tileRows = strip.(split(tileData, ","; keepempty=false))
		tiles = [only.(split(row, "")) for row in tileRows]
		
		rows = length(tiles)
		columns = maximum(length.(tiles))
		
		tileMap = fill('0', (rows+2, columns+2))
		
		for y in eachindex(tiles), x in eachindex(tiles[y])
		  if tiles[y][x] in Ahorn.validTileEntityTiles()
			tileMap[y+1,x+1] = tiles[y][x]
		  end
		end

		x = Int(get(entity.data, "x", 0)) + xOffset
		y = Int(get(entity.data, "y", 0)) + yOffset

		objTiles = Ahorn.getObjectTiles(room, x, y, width, height)

		Ahorn.drawFakeTiles(ctx, room, tileMap, objTiles, true, x, y, alpha=0.7, clipEdges=true)
	end
end
renderTileEntity(ctx, entity, room, xOffset=0, yOffset=0) = renderTileEntity(ctx, entity, String(get(entity.data, "tileData", "")), room, xOffset, yOffset)

mutable struct ButtonOption <: Ahorn.Form.Option
	name::String
	dataName::String

	button::GtkButton

	value::String
	size::Tuple{Number, Number}

	function ButtonOption(name, dataName=name, tooltip="", value="", size=(200,300))
		button = GtkButton(name, tooltip_text=tooltip)
		return new(name, dataName, button, value, size)
	end
end

Base.size(option::ButtonOption) = (1, 1)
Ahorn.Form.getValue(option::ButtonOption) = option.value
Ahorn.Form.setValue!(option::ButtonOption, value::String) = option.value = value
Ahorn.Form.getGroup(option::ButtonOption) = 2
Ahorn.Form.setGtkProperty!(option::ButtonOption, field::Symbol, value::Any) = set_gtk_property!(option.button, field, value)
Ahorn.Form.getGtkProperty(option::ButtonOption, field::Symbol, typ::DataType) = get_gtk_property!(option.button, field, typ)

function Ahorn.Form.addToGrid!(grid, option::ButtonOption, col=0, row=0)
    grid[col, row] = option.button

	 @guarded signal_connect(option.button, :clicked) do args...
		spawnEditingWindow("Edit Tile Entity", option.size...,
			function(data::String)
				Ahorn.Form.setValue!(option, data)
			end,
			option.value
		)
	 end
end

#=
function Ahorn.Form.suggestOption(displayName::String, value::String; tooltip::String="", dataName::String=displayName, choices::TileData, editable::Bool=false)
	if (isa(value, String))
		return ButtonOption("Edit Tiles " * displayName, attr, tooltip, value, (choices.width, choices.height))
	end
end
=#

function tileEntityConfigOptions(entity, ignores=String[])
	addedNodes = false
	res = Ahorn.Form.Option[]

	options = Ahorn.editingOptions(entity)
	horizontalAllowed, verticalAllowed = Ahorn.canResizeWrapper(entity)
	nodeRange = Ahorn.nodeLimits(entity)

	key = isa(entity, Maple.Entity) ? "entities" : "triggers"
	tooltips = get(Ahorn.langdata, ["placements", key, entity.name, "tooltips"])
	names = get(Ahorn.langdata, ["placements", key, entity.name, "names"])

	# Add nothing keys for all Dict editing options
	# Merge entity data over afterwards, this makes it possible to "store" nothing values for editing later
	data = merge(
		Dict{String, Any}(
			attr => nothing for (attr, value) in options if isa(value, Dict)
		),
		entity.data
	)

	for (attr, value) in data
		if !horizontalAllowed && attr == "width" || !verticalAllowed && attr == "height"
			continue
		end

		# Always ignore these, regardless of ignore argument
		if attr == "originX" || attr == "originY"
			continue
		end

		# Special cased below
		if attr == "nodes"
			continue
		end

		if attr in ignores
			continue
		end

		if startswith(attr, "tileData")
			width = Int(get(entity.data, "width", 32))
			height = Int(get(entity.data, "height", 32))
			name = Ahorn.expandTooltipText(get(names, Symbol(attr), ""))
			displayName = isempty(name) ? length(attr) > 8 ? Ahorn.humanizeVariableName(attr[9:end]) : "" : name
			tooltip = Ahorn.expandTooltipText(get(tooltips, Symbol(attr), "Edit the tilemap of this entity."))
			push!(res, ButtonOption("Edit Tiles " * displayName, attr, tooltip, value::String, (width, height)))
			continue
		end

		if get(Ahorn.debug.config, "TOOLTIP_ENTITY_MISSING", false) && !haskey(tooltips, Symbol(attr))
			if !(attr in Ahorn.debug.defaultIgnoredTooltipAttrs)
				println("Missing tooltip for '$(entity.name)' - $attr")
			end
		end

		name = Ahorn.expandTooltipText(get(names, Symbol(attr), ""))
		displayName = isempty(name) ? Ahorn.humanizeVariableName(attr) : name
		tooltip = Ahorn.expandTooltipText(get(tooltips, Symbol(attr), ""))
		attrOptions = get(options, attr, nothing)

		push!(res, Ahorn.Form.suggestOption(displayName, value, dataName=attr, tooltip=tooltip, choices=attrOptions, editable=true))
	end

	if nodeRange[2] != 0 && !("nodes" in ignores)
	  push!(res, Ahorn.Form.ListOption("Node X;Node Y", get(entity.data, "nodes", Tuple{Int, Int}[]), dataName="nodes", minRows=nodeRange[1], maxRows=nodeRange[2]))
	end

	return res
end



# TILE ENTITY EDITING WINDOW



lastEditingWindow = nothing
lastEditingWindowDestroyed = false

brush = nothing
const DEFAULT_ZOOM = 3
zoom = DEFAULT_ZOOM;
# Represented as a [Height, Width] array
tileMap = nothing

#include("event_handler.jl")


function spawnEditingWindow(title, width, height, callback=(data) -> println(data), tileString="")
	destroyPrevious = get(Ahorn.config, "properties_destroy_previous_window", true)
   alwaysOnTop = get(Ahorn.config, "properties_always_on_top", true)

	global tileMap = stringToTileMap(tileString, width, height)

	materialListWindow = GtkScrolledWindow(vexpand=true, hscrollbar_policy=Gtk.GtkPolicyType.NEVER)
	push!(materialListWindow, getMaterialList().tree)
	setMaterials!()

	canvas = getCanvas(width, height)

	updateButton = GtkButton("Update")
	@guarded signal_connect(updateButton, :clicked) do args...
        if tileMap !== nothing
            callback(tileMapToString(tileMap))
		end
    end

	editingWindow = GtkWindow(title, width + 50, height + 50, false, icon=Ahorn.windowIcon) |> (grid = GtkGrid())
	# how does padding even work
	grid[1, 1] = push!(GtkBox(:v), push!(GtkBox(:h), canvas))
	grid[1, 2] = updateButton
	grid[2, 1:2] = materialListWindow

	set_gtk_property!(grid, :row_spacing, 1)
	set_gtk_property!(grid, :column_spacing, 1)

	@guarded signal_connect(editingWindow, :destroy) do widget
        global lastEditingWindowDestroyed = true
    end

	visible(editingWindow, false)

    winScreen = nothing
	if isa(Ahorn.lastPropertyWindow, GtkWindowLeaf) && !Ahorn.lastPropertyWindowDestroyed
		winScreen = GAccessor.screen(Ahorn.lastPropertyWindow)

      GAccessor.screen(editingWindow, winScreen)
	end

	GAccessor.transient_for(editingWindow, Ahorn.lastPropertyWindow)
	GAccessor.keep_above(editingWindow, alwaysOnTop)

	showall(editingWindow)
	visible(editingWindow, true)

	if destroyPrevious && isa(lastEditingWindow, GtkWindowLeaf)
        destroy(lastEditingWindow)
    end

	global lastEditingWindow = editingWindow
	global lastEditingWindowDestroyed = false
end

function getCanvas(width, height)
	global brush = Brush()
	global zoom = Ahorn.camera.scale
	canvas = Ahorn.Gtk.Canvas(width * zoom, height * zoom)
	assignEvents(canvas)
	@guarded draw(canvas) do widget
	    if canvas !== nothing && lastEditingWindow !== nothing && !lastEditingWindowDestroyed
			ctx = getgc(canvas)
			Ahorn.paintSurface(ctx, Ahorn.colors.background_canvas_fill)

			save(ctx)
			scale(ctx, zoom, zoom)
	      drawTileEntity(ctx)
			drawTileBrush(ctx)
			restore(ctx)
	    end
	end
	return canvas
end

function drawTileEntity(ctx)
	#Kinda messy to stop tiles from extending towards the edges of the canvas
	(height, width) = size(tileMap)

	cleanTileMap = fill('0', (height + 2, width + 2))
	for y in 1:height, x in 1:width
		cleanTileMap[y+1, x+1] = tileMap[y, x]
	end

	fakeDr = Ahorn.DrawableRoom(
        Ahorn.loadedState.map,
        Maple.Room(
            name="Tile_Entity_Placeholder",
            fgTiles=Maple.Tiles(cleanTileMap),
        ),

        Ahorn.TileStates(),
        Ahorn.TileStates(),

        Ahorn.Layer("fgTiles"),
        Ahorn.Layer[],

        Ahorn.colors.background_room_fill
    )

	save(ctx)
	translate(ctx, -8, -8)

	Ahorn.drawTiles(ctx, fakeDr, true, alpha=Ahorn.getGlobalAlpha(), useObjectTiles=true)

	restore(ctx)
end

function drawTileBrush(ctx)
	Ahorn.drawRectangle(ctx, brush.x-1, brush.y-1, 10, 10, Ahorn.colors.brush_bc, Ahorn.colors.brush_fc)
end


# EVENT HANDLERS


mutable struct Brush{T<:Int}
	x::T
	y::T
	Brush() = new{Int}(0, 0)
end

material = '0'
materialList = nothing

function getMaterialList()
	global materialList = Ahorn.generateTreeView("Material", Tuple{String}[], sortable=false)
	Ahorn.connectChanged(materialList) do list::Ahorn.ListContainer, selected::String
		materialSelected(list, selected)
	end
	return materialList
end

function setMaterials!()
    #Ahorn.loadXMLMeta()
    validTiles = Ahorn.validTiles("fgTiles")
    tileNames = Ahorn.tileNames("fgTiles")

    Ahorn.updateTreeView!(materialList, [tileNames[mat] for mat in validTiles], row -> row[1] == get(tileNames, material::Char, nothing))
end

function materialSelected(list, selected)
    tileNames = Ahorn.tileNames("fgTiles")
    global material = tileNames[selected]::Char
end

function stringToTileMap(tileString, width, height)
	nOfTiles = (ceil(Int, height/8), ceil(Int, width/8))
	tileMap = fill('0', nOfTiles)
	if tileString != ""
		tileRows = strip.(split(tileString, ","; keepempty=false))
		tiles = [only.(split(row, "")) for row in tileRows]
		for (y, _) in zip(eachindex(tiles), 1:size(tileMap, 1)), (x, _) in zip(eachindex(tiles[y]), 1:size(tileMap, 2))
			if tiles[y][x] in Ahorn.validTileEntityTiles()
				tileMap[y, x] = tiles[y][x]
			end
		end
	end
	return tileMap
end

function tileMapToString(tileMap)
	tileString = ""
	for y in axes(tileMap, 1)
		for x in axes(tileMap, 2)
			tileString = string(tileString, tileMap[y, x])
		end
		tileString = string(tileString, ',')
	end
	return tileString
end

function mouseMotion(widget, event)
	if event.x < 0 || event.x > width(widget) || event.y < 0 || event.y > height(widget)
		return true
	end

	x = roundToMultiple(trunc(Int, event.x/zoom), 8)
	y = roundToMultiple(trunc(Int, event.y/zoom), 8)

	if brush.x !== x || brush.y !== y
		global brush.x = x
		global brush.y = y
		draw(widget)
	end
	return true
end

function roundToMultiple(num, multiple)
	if multiple == 0
		return num
	end
	remainder = num % multiple
	if remainder == 0
		return num
	end
	return num - remainder
end

function drawCurrentTile(widget, event)
	y, x = Int(brush.y/8)+1, Int(brush.x/8)+1
	if y < 0 || y > size(tileMap, 1) || x < 0 || x > size(tileMap, 2)
		return true
	end

	global tileMap[y, x] = material::Char
	draw(widget)
end

function assignEvents(widget)
	widget.mouse.motion = @guarded (widget, event) -> mouseMotion(widget, event)
	widget.mouse.button1press = @guarded (widget, event) -> drawCurrentTile(widget, event)
	widget.mouse.button1motion = @guarded (widget, event) -> drawCurrentTile(widget, event)
end

#= No longer needed, but saved just in case
	# Initalize fancytileentities modules
	externalTileEntities = Ahorn.findExternalModules("fancytileentities")
	append!(Ahorn.loadedEntities, externalTileEntities)
	Ahorn.loadModule.(externalTileEntities)
	Ahorn.loadExternalModules!(Ahorn.loadedModules, Ahorn.loadedEntities, "fancytileentities")
	Ahorn.registerPlacements!(Ahorn.entityPlacements, Ahorn.loadedEntities)
=#

end
