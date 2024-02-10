extends Node

# Dictionary to store all the values and states
var state_dict = {}
var sdf_atlas = preload("res://assets/sdf_font.png")
var sdf_material = preload("res://materials/sdf_label_material.tres")
var json_data = preload("res://assets/sdf_font.json")
var font_data = {}
var atlas_width = sdf_atlas.get_width()
var atlas_height = sdf_atlas.get_height()

func _ready():
	load_font_data(json_data)

# Function to set a value in the dictionary
func set_value(key, value):
	state_dict[key] = value

# Function to get a value from the dictionary
func get_value(key: int):
	return state_dict.get(key)

func update_button_state(button_id: int, new_state):
	state_dict[button_id] = new_state

func load_font_data(path : String):
	var file = FileAccess.open(path, FileAccess.READ)
	if file.get_error() == OK:
		var content = file.get_as_text()
		font_data = JSON.parse_string(content)
		file.close()
	else:
		print("Failed to load font data.")
