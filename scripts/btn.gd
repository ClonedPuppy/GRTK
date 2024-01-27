extends Area3D

var area
var button_number
var tracked_body = null
var pressed = false
var lever
var initial_y_position = -0.0025  # Store the initial y-position of the button
var last_y_position: float = 0.0  # Variable to store the last y position
var finger_collision_offset = 0.0025
var click_sound: AudioStreamPlayer

# Start by setting up the button and it's functions
func init(area_node: Area3D, cell_no: int):
	area = area_node
	button_number = cell_no
	area_node.connect("body_entered", Callable(self, "_on_Area3D_body_entered"))
	area_node.connect("body_exited", Callable(self, "_on_Area3D_body_exited"))

	# Add a collision node
	var collision_shape = CollisionShape3D.new()
	collision_shape.name = "Collision_" + str(cell_no)
	area.add_child(collision_shape)

	# Add a shape to the collision node, assuming button size is 0.01 x 0.01
	var shape = CylinderShape3D.new()
	shape.height = 0.01
	shape.radius = 0.01
	collision_shape.shape = shape
	
	# Add a MeshInstance3D with the leverage mesh at the same location
	var mesh_instance = MeshInstance3D.new()
	var mesh_library = load("res://assets/levers.tres")
	var mesh = mesh_library.get_item_mesh(0) # Since I beforehand know this particular lever is in the 0 slot
	if mesh:
		mesh_instance.mesh = mesh
	else:
		print("Failed to load lever mesh from:", mesh)
		return

	var corrected_rotation = Quaternion(Vector3(1, 0, 0), deg_to_rad(0)) # not sued if lever correctly oriented in blender
	mesh_instance.transform = Transform3D(corrected_rotation, Vector3(0, initial_y_position, 0))
	mesh_instance.name = "Lever_" + str(cell_no)

	area.add_child(mesh_instance)
	set_process(true) #manually turn on the _process function as it's disabled since attaching the script via code
	lever = mesh_instance
	
	# Initialize the AudioStreamPlayer
	click_sound = AudioStreamPlayer.new()
	click_sound.stream = preload("res://assets/General_Button_2_User_Interface_Tap_FX_Sound.ogg")
	add_child(click_sound)


func _process(delta):
	if tracked_body and !pressed:
		var _global_position = tracked_body.global_transform.origin
		var _local_position = area.to_local(_global_position)
		if _local_position.y >= 0 and last_y_position >= 0 and _local_position.y < last_y_position:
			update_button_plate_position(_local_position.y - finger_collision_offset)
			print("Current: %s  Last: %s" % [_local_position.y, last_y_position])

			if _local_position.y < 0.0025:
				if ButtonStatesAutoload.get(button_number) != null:
					ButtonStatesAutoload.update_button_state(button_number, true)
				elif ButtonStatesAutoload.get(button_number) == true:
					initial_y_position = 0
					reset_button_plate()
				else:
					ButtonStatesAutoload.update_button_state(button_number, false)
					initial_y_position = -0.0025
					reset_button_plate()

				pressed = true
				click_sound.play()
				print("pressed")

		last_y_position = _local_position.y  # Update the last y position


# Resets the lever mesh to it's original height
func reset_button_plate():
	var button_plate_transform = lever.transform
	button_plate_transform.origin.y = initial_y_position  # Reset to original y position
	lever.transform = button_plate_transform


# Transforms the levers height in accordance to where the finger tip is in the Area3D
func update_button_plate_position(y_position):
	var button_plate_transform = lever.transform
	button_plate_transform.origin.y = initial_y_position + y_position - 0.007
	lever.transform = button_plate_transform


# Set the tracked_body to follow whatever has entered the Area3D
func _on_Area3D_body_entered(body: Node):
	tracked_body = body


# Resets the button functions and sets it in a waiting state
func _on_Area3D_body_exited(body: Node):
	if tracked_body == body:
		tracked_body = null
		pressed = false
		reset_button_plate()
