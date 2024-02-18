extends Area3D

# Vars
var area
var button_number
var tracked_body = null
var active = false
var lever
var initial_y_position = -0.0025  # Store the initial y-position of the button
var last_y_position: float = 0.0  # Variable to store the last y position
var min_movement_threshold = 0.0005 # Minimum movement required to consider a press
var max_movement_threshold = 0.5 # Maximum movement allowed for a valid press
var finger_collision_offset = 0.0025
var click_sound: AudioStreamPlayer


# Start by setting up the button and it's functions
func init(area_node: Area3D, cell_no: int, current_mesh: Mesh, cell_orientation):
	area = area_node
	button_number = cell_no
	
	# Signals emitted at entry and exit
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
	var mesh = mesh_library.get_item_mesh(1) # Since I beforehand know this particular lever is in the 0 slot
	
	if mesh:
		mesh_instance.mesh = mesh.duplicate()
	else:
		print("Failed to load lever mesh from:", mesh)
		return

	var corrected_rotation = Quaternion(Vector3(1, 0, 0), deg_to_rad(0)) # not sued if lever correctly oriented in blender
	mesh_instance.transform = Transform3D(corrected_rotation, Vector3(0, initial_y_position, 0))
	mesh_instance.name = "Lever_" + str(cell_no)
	
	# Rotate the button accordingly
	var label_position = Transform3D(Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))).translated(Vector3(0.0, 0.0055, 0.012))
	print(cell_orientation)
	if cell_orientation != -1:
		if cell_orientation == 16:
			mesh_instance.rotation_degrees.y = 90
			label_position = Transform3D(Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))).translated(Vector3(0.0, 0.0055, 0.019))
		elif cell_orientation == 10:
			mesh_instance.rotation_degrees.y = 180
			label_position = Transform3D(Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))).translated(Vector3(0.0, 0.0055, 0.012))
		elif cell_orientation == 22:
			mesh_instance.rotation_degrees.y = -90
			label_position = Transform3D(Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))).translated(Vector3(0.0, 0.0055, 0.019))
		else:
			mesh_instance.rotation_degrees.y = 0
		
	area.add_child(mesh_instance)
	
	lever = mesh_instance
	
	# Create a label
	var label_3d = Label3D.new()
	label_3d.text = "Button: " + str(button_number)
	label_3d.transform = label_position
	label_3d.pixel_size = 0.0001
	label_3d.font_size = 40
	label_3d.outline_size = 0
	label_3d.modulate = Color.BLACK
	add_child(label_3d)
	
	# Manually turn on the _process function as it's disabled since attaching the script via code
	set_process(true) 

	# Initialize the AudioStreamPlayer
	click_sound = AudioStreamPlayer.new()
	click_sound.stream = preload("res://assets/General_Button_2_User_Interface_Tap_FX_Sound.ogg")
	add_child(click_sound)
	
	# Setup the button in the global Dict
	ButtonStatesAutoload.update_button_state(button_number, false)


# frame by frame process
func _process(delta):
	if tracked_body and !active:
		var _global_position = tracked_body.global_transform.origin
		var _local_position = area.to_local(_global_position)
		
		var movement_distance = last_y_position - _local_position.y
		
		# Get the current state of the button
		var _button_state = ButtonStatesAutoload.get_value(button_number) 
		
		# Make sure we are pressing the button from a top - down direction
		if _local_position.y >= 0 and last_y_position >= 0 and movement_distance >= min_movement_threshold:
			
			# Update the lever position as the "finger tip" is moving in the Area3D
			update_button_plate_position(_local_position.y - finger_collision_offset)
			
			# Check if the "finger tip" is far enough down the Are3D volume to trigger the button as pressed 
			if _local_position.y < 0.003 and not active:
				active = true
				click_sound.play()

				## Toggle the button state
				_button_state = not _button_state
#
				## Change the resting height based on the button state
				#initial_y_position = -0.005 if _button_state else -0.0025
				ButtonStatesAutoload.set_value(button_number, true)
				#reset_button_plate()

		# Update the last y position
		last_y_position = _local_position.y  


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
		active = false
		# Set the button state to false when the finger is no longer pressing
		ButtonStatesAutoload.set_value(button_number, false)
		reset_button_plate()
