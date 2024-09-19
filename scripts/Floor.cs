using Godot;
using System;

public partial class Floor : MeshInstance3D
{
    private MeshInstance3D plane;
    //private Camera3D cameraNode;
    private Vector2 planeSize;

    public override void _Ready()
    {
        plane = this;
        //cameraNode = GetNode<Camera3D>("../XROrigin3D/XRCamera3D");
        planeSize = (Mesh as PlaneMesh).Size;
    }

    public override void _Process(double delta)
    {
        // Uncomment and adjust the following code if you want to use it
        /*
        if (cameraNode == null)
            return;

        Vector3 cameraGlobalPosition = cameraNode.GlobalTransform.Origin;
        Vector3 cameraLocalPosition = plane.ToLocal(cameraGlobalPosition);

        Vector2 uvCoordinates = new Vector2(
            0.5f + cameraLocalPosition.X / planeSize.X,
            0.5f + cameraLocalPosition.Z / planeSize.Y
        );

        ShaderMaterial shaderMaterial = Mesh.SurfaceGetMaterial(0) as ShaderMaterial;
        if (shaderMaterial != null)
        {
            shaderMaterial.SetShaderParameter("camera_uv", uvCoordinates);
        }
        */
    }
}