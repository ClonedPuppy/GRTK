extends MeshInstance3D

@onready var plane = self
@onready var camera_node = $"../XROrigin3D/XRCamera3D"
@onready var plane_size = mesh.size

func _process(delta):
	if not camera_node:
		return

	var camera_global_position = camera_node.global_transform.origin
	var camera_local_position = plane.to_local(camera_global_position)

	var uv_coordinates = Vector2(
		0.5 + camera_local_position.x / plane_size.x,
		0.5 + camera_local_position.z / plane_size.y
	)

	var shader_material = mesh.surface_get_material(0)  # Assuming the mesh is the first surface
	if shader_material and shader_material is ShaderMaterial:
		shader_material.set_shader_parameter("camera_uv", uv_coordinates)



#extends MeshInstance3D
#
#@onready var plane = self
#@onready var camera_global_position = $"../XROrigin3D/XRCamera3D".global_transform.origin
#@onready var camera_local_position = plane.to_local(camera_global_position)
#@onready var projected_position = Vector2(camera_local_position.x, camera_local_position.z)
#@onready var plane_size = mesh.size  
#
#func _process(delta):
	#camera_global_position = $"../XROrigin3D/XRCamera3D".global_transform.origin
	#camera_local_position = plane.to_local(camera_global_position)
	#projected_position = Vector2(camera_local_position.x, camera_local_position.z)
	#
	#var uv_coordinates = Vector2(
		#0.5 + camera_local_position.x / plane_size.x,
		#0.5 + camera_local_position.z / plane_size.y
	#)
#
	#var shader_material = mesh.surface_get_material(0) # Assuming the mesh is the first surface
	#if shader_material and shader_material is ShaderMaterial:
		#shader_material.set_shader_parameter("camera_uv", uv_coordinates)
