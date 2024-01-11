extends Area3D

var area

func init(area_node):
	print("run..")
	area = area_node
	area_node.connect("body_entered", Callable(self, "_on_Area3D_body_entered"))


func _on_Area3D_body_entered(body: Node):
	print("Body that entered: ", body.name)
	print("Button that sent signal: ", area.name)

