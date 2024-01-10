extends GridMap

@onready var ui_meshes = self.mesh_library
var button_number = 0

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
					create_area_for_cell(cell, item_name + str(button_number))

func create_area_for_cell(cell: Vector3, cell_name: String):
	var area = Area3D.new()
	area.name = "Area3D_" + str(cell)
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

	area.set_meta("cell", cell)
	area.set_meta("name", cell_name)
	
	# Connect signals using Callable
	area.connect("body_entered", Callable(self, "_on_Area3D_body_entered"))
	
func _on_Area3D_body_entered(body: Node):
	if body.has_meta("cell"):
		var cell = body.get_meta("cell")
		print("Body entered at cell: ", cell)
		# Handle the signal
