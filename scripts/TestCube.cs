using Godot;
using System;

public partial class TestCube : MeshInstance3D
{
    private int associatedButtonNumber;
    [Export] public int Button { get; set; } = 1;

    public override void _Ready()
    {
        associatedButtonNumber = Button;
        AddToGroup("UIListeners");
    }

    // This method will be called when a button state changes
    public void OnButtonStateChanged(int buttonNumber, bool isPressed)
    {
        if (buttonNumber == associatedButtonNumber)
        {
            UpdateCubeColor(isPressed ? 1.0f : 0.0f);
        }
    }

    // This method will be called when a slider value changes
    public void OnSliderValueChanged(int sliderNumber, float value)
    {
        if (sliderNumber == associatedButtonNumber)
        {
            UpdateCubeColor(value);
        }
    }

    private void UpdateCubeColor(float value)
    {
        var material = GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
        {
            // For buttons: 0.0f will be red, 1.0f will be green
            // For sliders: value will determine the mix between red and green
            material.AlbedoColor = new Color(1.0f - value, value, 0);
        }
    }
}