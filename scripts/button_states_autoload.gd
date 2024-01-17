extends Node

# Dictionary to store all the values and states
var state_dict = {}

func _ready():
	pass # Initialize anything if needed

# Function to set a value in the dictionary
func set_value(key, value):
	state_dict[key] = value

# Function to get a value from the dictionary
func get_value(key):
	return state_dict.get(key)

func update_button_state(button_id, new_state):
	state_dict[button_id] = new_state
