using Godot;
using System;

public partial class TestCube : MeshInstance3D
{
    private MeshInstance3D cube;
    private int associatedButtonNumber;
    [Export] public int Button { get; set; } = 1;

    public override void _Ready()
    {
        associatedButtonNumber = Button;
        AddToGroup("UIListeners");
        //cube = this;
    }

    public override void _Process(double delta)
    {
        // var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
        // if (buttonStatesAutoload.StateDict.Count > 0)
        // {
        //     var material = cube.GetActiveMaterial(0) as StandardMaterial3D;
        //     if (material != null)
        //     {
        //         if (buttonStatesAutoload.StateDict.TryGetValue(Button, out var state))
        //         {
        //             if (state.VariantType == Variant.Type.Bool)
        //             {
        //                 material.AlbedoColor = state.AsBool() ? new Color(0, 1, 0) : new Color(1, 0, 0);
        //             }
        //             else if (state.VariantType == Variant.Type.Float)
        //             {
        //                 float value = (float)state;
        //                 material.AlbedoColor = new Color(0, value, 0);
        //             }
        //             else
        //             {
        //                 // Handle unexpected type or set a default color
        //                 material.AlbedoColor = new Color(1, 0, 0);
        //             }
        //         }
        //         else
        //         {
        //             // Handle case where the key doesn't exist in the dictionary
        //             material.AlbedoColor = new Color(1, 0, 0);
        //         }
        //     }
        // }
    }

    // This method will be called when the group signal is emitted
    public void OnButtonStateChanged(int buttonNumber, bool isPressed)
    {
        GD.Print("Group Hit: " + buttonNumber + " :" + isPressed);
        if (buttonNumber == associatedButtonNumber)
        {
            UpdateCubeState(isPressed);
        }
    }

    private void UpdateCubeState(bool isPressed)
    {
        var material = GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
        {
            if (isPressed)
            {
                // if (state.VariantType == Variant.Type.Bool)
                // {
                material.AlbedoColor = new Color(0, 1, 0);
                // }
                // else if (state.VariantType == Variant.Type.Float)
                // {
                //     float value = (float)state;
                //     material.AlbedoColor = new Color(0, value, 0);
                // }
                // else
                // {
                //     // Handle unexpected type or set a default color
                //     material.AlbedoColor = new Color(1, 0, 0);
                // }
            }
            else
            {
                // Handle case where the key doesn't exist in the dictionary
                material.AlbedoColor = new Color(1, 0, 0);
            }
        }
    }

}