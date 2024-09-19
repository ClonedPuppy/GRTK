using Godot;
using System;

public partial class TestCube : MeshInstance3D
{
    private MeshInstance3D cube;
    [Export] public int Button { get; set; } = 1;

    public override void _Ready()
    {
        cube = this;
    }

    public override void _Process(double delta)
    {
        var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
        if (buttonStatesAutoload.StateDict.Count > 0)
        {
            var material = cube.GetActiveMaterial(0) as StandardMaterial3D;
            if (material != null)
            {
                if (buttonStatesAutoload.StateDict.TryGetValue(Button, out var state) && state.As<bool>())
                {
                    material.AlbedoColor = new Color(0, 1, 0);
                }
                else
                {
                    material.AlbedoColor = new Color(1, 0, 0);
                }
            }
        }
    }
}