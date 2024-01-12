extends Area3D

var area

func init(area_node: Area3D, cell_no: int):
	print("run..")
	area = area_node
	area_node.connect("body_entered", Callable(self, "_on_Area3D_body_entered"))
		# Add a collision node
	var collision_shape = CollisionShape3D.new()
	collision_shape.name = "Collision_" + str(cell_no)
	area.add_child(collision_shape)

	# Add a shape to the collision node, assuming button size is 0.005 x 0.005
	var shape = CylinderShape3D.new()
	shape.height = 0.005
	shape.radius = 0.005
	collision_shape.shape = shape
	
	# Add a MeshInstance3D with the leverage mesh at the same location
	var mesh_instance = MeshInstance3D.new()
	var lever = $"../../lever"
	var mesh = lever.mesh
	if mesh:
		mesh_instance.mesh = mesh
	else:
		print("Failed to load mesh from:", lever)
		return

	var corrected_rotation = Quaternion(Vector3(1, 0, 0), deg_to_rad(-90))
	mesh_instance.transform = Transform3D(corrected_rotation, Vector3(0,-0.0025,0))
	mesh_instance.name = "Lever_" + str(cell_no)

	area.add_child(mesh_instance)

func _on_Area3D_body_entered(body: Node):
	print("Body that entered: ", body.name)
	print("Button that sent signal: ", area.name)

