extends GridMap

@onready var ui_panel = $"."
@onready var ui_meshes = ui_panel.mesh_library


# Assuming this script is attached to a GridMap node
func _ready():
	if ui_meshes:
		# Iterate over all cells in the GridMap
		var used_cells = get_used_cells()  # This gets an array of all used cell positions
		for cell in used_cells:
			var item_id = get_cell_item(cell)
			if item_id != -1:  # -1 means no item
				var item_name = mesh_library.get_item_name(item_id)
				print("Cell: ", cell, " has mesh: ", item_name)
	else:
		print("No MeshLibrary found in the GridMap")
		
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
