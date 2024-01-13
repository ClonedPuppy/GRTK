extends GridMap

@onready var lever = $"../lever"
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
					print("Cell: ", cell, " has mesh: ", item_name + str(button_number))
					button_number += 1
					create_manipulator_for_cell(cell)

func create_manipulator_for_cell(cell: Vector3):
	var area = Area3D.new()
	current_area = area
	area.name = "Button_" + str(button_number)
	
	# Calculate the global transform for the cell
	var cell_transform = Transform3D()
	cell_transform.origin = cell * cell_size + Vector3(0.01, 0.0025, 0.01)
	area.global_transform = global_transform * cell_transform
	
	add_child(area)

	# Add script
	var script_path = "res://scripts/btn.gd"
	var script = load(script_path)
	if script:
		area.script = script
		if area.has_method("init"):
			area.call("init", area, button_number)
	else:
		print("Failed to load script:", script_path)
