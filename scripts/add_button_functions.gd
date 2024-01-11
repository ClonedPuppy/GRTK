extends GridMap

@onready var lever = $"../lever"
@onready var ui_meshes = self.mesh_library
var button_number = 0
var current_sender

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
					create_manipulator_for_cell(cell, item_name + str(button_number))

func create_manipulator_for_cell(cell: Vector3, cell_name: String):
	var area = Area3D.new()
	current_sender = area
	area.name = "Button_" + str(button_number)
	add_child(area)

	# Calculate the global transform for the cell
	var cell_transform = Transform3D()
	cell_transform.origin = cell * cell_size + Vector3(0.01, 0.0025, 0.01)
	area.global_transform = global_transform * cell_transform

	# Add a collision node
	var collision_shape = CollisionShape3D.new()
	collision_shape.name = "Collision_" + str(button_number)
	area.add_child(collision_shape)

	# Add a shape to the collision node, assuming button size is 0.005 x 0.005
	var shape = CylinderShape3D.new()
	shape.height = 0.005
	shape.radius = 0.005
	collision_shape.shape = shape

	area.set_meta("cell", cell)
	area.set_meta("name", cell_name)
	
	# Add a MeshInstance3D with the leverage mesh at the same location
	var mesh_instance = MeshInstance3D.new()
	var mesh = lever.mesh
	if mesh:
		mesh_instance.mesh = mesh
	else:
		print("Failed to load mesh from:", lever)
		return

	var corrected_rotation = Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))
	mesh_instance.transform = Transform3D(corrected_rotation, Vector3(0,-0.0025,0))
	mesh_instance.name = "Lever_" + str(button_number)

	area.add_child(mesh_instance)
	
	# Add script
	var script_path = "res://scripts/btn.gd"
	var script = load(script_path)
	if script:
		area.script = script
		if area.has_method("init"):
			area.call("init", area)
	else:
		print("Failed to load script:", script_path)
