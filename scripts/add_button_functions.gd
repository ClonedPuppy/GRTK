extends GridMap

@onready var ui_meshes = self.mesh_library
var button_number = 0
var current_area

func _ready():
	if ui_meshes:
		var used_cells = get_used_cells()
		for cell in used_cells:
			var item_id = get_cell_item(cell)
			if item_id != -1:  # Check if cell is not empty
				var item_name = mesh_library.get_item_name(item_id)
				if item_name == "Button":
					var mesh = mesh_library.get_item_mesh(item_id)
					button_number += 1
					setup_cell(cell, mesh)


func setup_cell(cell: Vector3, mesh: Mesh):
	var area = Area3D.new()

	current_area = area
	area.name = "Button_" + str(button_number)

	# Calculate the local transform for the cell
	var cell_local_transform = Transform3D()
	cell_local_transform.origin = cell * cell_size + Vector3(0.02, 0.005, 0.02)

	# Set the area's local transform relative to the GridMap
	area.transform = cell_local_transform

	# Add the area as a child of the GridMap
	add_child(area)

	# Load and assign script to the Area3D node
	var script_path = "res://scripts/momentary_btn.gd"
	var script = load(script_path)
	if script:
		area.script = script
		if area.has_method("init"):
			area.call("init", area, button_number, mesh)
	else:
		print("Failed to attach script:", script_path)
