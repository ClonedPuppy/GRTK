extends MeshInstance3D

@onready var cube = $"."
@export var button = 1
func _process(delta):
	if ButtonStatesAutoload.state_dict:
		var material = cube.get_active_material(0)
		if ButtonStatesAutoload.state_dict[button] == true:
			if material:
				material.albedo_color = Color(0, 1, 0)
		else:
			if material:
				material.albedo_color = Color(1, 0, 0)
