extends MeshInstance3D

@onready var cube = $"."
func _process(delta):
	if ButtonStatesAutoload.state_dict:
		var material = cube.get_active_material(0)
		if ButtonStatesAutoload.state_dict[1] == true:
			if material:
				material.albedo_color = Color(0, 1, 0)
		else:
			if material:
				material.albedo_color = Color(1, 0, 0)
