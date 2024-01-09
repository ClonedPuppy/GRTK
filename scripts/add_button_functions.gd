extends GridMap

@onready var ui_panel = $"."
@onready var ui_meshes = ui_panel.mesh_library

func _ready():
	if ui_meshes:
		var used_cells = get_used_cells()
		for cell in used_cells:
			var item_id = get_cell_item(cell)
			if item_id != -1:  # Check if cell is not empty
				var item_name = mesh_library.get_item_name(item_id)
				if item_name == "Button":
					print("Cell: ", cell, " has mesh: ", item_name)
					create_area_for_cell(cell)

func create_area_for_cell(cell: Vector3):
	var area = Area3D.new()
	add_child(area)

	# Calculate the global transform for the cell
	var cell_transform = Transform3D()
	cell_transform.origin = cell * cell_size + Vector3(0.01, 0.0025, 0.01)
	area.global_transform = global_transform * cell_transform

	var collision_shape = CollisionShape3D.new()
	area.add_child(collision_shape)

	# Assuming button size is 0.01 x 0.0025 x 0.01
	var shape = BoxShape3D.new()
	shape.extents = Vector3(0.01, 0.0025, 0.01)  # Half extents
	collision_shape.shape = shape

	# Connect signals using Callable
	area.connect("body_entered", Callable(self, "_on_Area3D_body_entered"))

func _on_Area3D_body_entered(body: Node):
	# Handle the signal when a body enters the Area3D
	pass
