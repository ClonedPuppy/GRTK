extends Node3D

@onready var interact_volume = $InteractVolume as Area3D
@onready var button_plate = %ButtonPlate as MeshInstance3D
@onready var gltf_document_save := GLTFDocument.new()
#@onready var gltf_scene_root_node := $"."
@onready var gltf_state_save := GLTFState.new()

var tracked_body = null
var initial_y_position = 0.0  # Store the initial y-position of the button_plate

func _ready():
	initial_y_position = button_plate.transform.origin.y
	print(initial_y_position)
	interact_volume.connect("body_entered", Callable(self, "_on_interact_volume_body_entered"))
	interact_volume.connect("body_exited", Callable(self, "_on_interact_volume_body_exited"))
	
func _input(event):
	if event.is_action_pressed("saveScene"):
		save_scene()
		
func _on_interact_volume_body_entered(body):
	print(body.name)
	tracked_body = body


func _on_interact_volume_body_exited(body):
	if tracked_body == body:
		tracked_body = null
		reset_button_plate()
		print("Exited")

func _process(delta):
	if tracked_body:
		var _global_position = tracked_body.global_transform.origin
		var _local_position = interact_volume.to_local(_global_position)
		print(_local_position.y)
		update_button_plate_position(_local_position.y)

func update_button_plate_position(y_position):
	var button_plate_transform = button_plate.transform
	button_plate_transform.origin.y = initial_y_position + y_position - 0.05
	button_plate.transform = button_plate_transform

func reset_button_plate():
	var button_plate_transform = button_plate.transform
	button_plate_transform.origin.y = initial_y_position  # Reset to original y position
	button_plate.transform = button_plate_transform
	
func save_scene():
	gltf_document_save.append_from_scene($".", gltf_state_save)
	gltf_document_save.write_to_filesystem(gltf_state_save, "res://savedSnapshot.glb")
