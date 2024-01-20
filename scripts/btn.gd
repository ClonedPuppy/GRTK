extends Area3D

var area
var button_number
var tracked_body = null
var pressed = false
var lever
var initial_y_position = -0.005  # Store the initial y-position of the button

func init(area_node: Area3D, cell_no: int):
	area = area_node
	button_number = cell_no
	area_node.connect("body_entered", Callable(self, "_on_Area3D_body_entered"))
	area_node.connect("body_exited", Callable(self, "_on_Area3D_body_exited"))

	# Add a collision node
	var collision_shape = CollisionShape3D.new()
	collision_shape.name = "Collision_" + str(cell_no)
	area.add_child(collision_shape)

	# Add a shape to the collision node, assuming button size is 0.005 x 0.005
	var shape = CylinderShape3D.new()
	shape.height = 0.01
	shape.radius = 0.01
	collision_shape.shape = shape
	
	# Add a MeshInstance3D with the leverage mesh at the same location
	var mesh_instance = MeshInstance3D.new()
	var mesh_library = load("res://assets/levers.tres")
	var mesh = mesh_library.get_item_mesh(0)
	if mesh:
		mesh_instance.mesh = mesh
	else:
		print("Failed to load mesh from:", mesh)
		return

	var corrected_rotation = Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))
	mesh_instance.transform = Transform3D(corrected_rotation, Vector3(0, initial_y_position, 0))
	mesh_instance.name = "Lever_" + str(cell_no)

	area.add_child(mesh_instance)
	set_process(true)
	lever = mesh_instance


func _process(delta):
	if tracked_body and !pressed:
		var _global_position = tracked_body.global_transform.origin
		var _local_position = area.to_local(_global_position)
		update_button_plate_position(_local_position.y)
		if _local_position.y < 0.0015:
			pressed = true
			print("pressed")
			reset_button_plate()
			ButtonStatesAutoload.update_button_state(button_number, pressed)


func reset_button_plate():
	var button_plate_transform = lever.transform
	button_plate_transform.origin.y = initial_y_position  # Reset to original y position
	lever.transform = button_plate_transform


func update_button_plate_position(y_position):
	var button_plate_transform = lever.transform
	button_plate_transform.origin.y = initial_y_position + y_position - 0.007
	lever.transform = button_plate_transform


func _on_Area3D_body_entered(body: Node):
	tracked_body = body


func _on_Area3D_body_exited(body: Node):
	if tracked_body == body:
		tracked_body = null
		pressed = false
		reset_button_plate()
