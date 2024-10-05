using Godot;
using System;

[Tool]
public partial class AddButtonFunctions : GridMap
{
    private MeshLibrary uiMeshes;
    private int buttonNumber = 0;
    private ShaderMaterial sliderMaterial;

    public override void _Ready()
    {
        uiMeshes = MeshLibrary;
        if (uiMeshes != null)
        {
            var usedCells = GetUsedCells();
            foreach (Vector3I cell in usedCells)
            {
                int itemId = GetCellItem(cell);
                int cellOrientation = GetCellItemOrientation(cell);
                if (itemId != -1)  // Check if cell is not empty
                {
                    string itemName = MeshLibrary.GetItemName(itemId);
                    if (itemName == "ToggleButton")
                    {
                        //Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        buttonNumber++;
                        SetupToggleButton(cell, cellOrientation);
                    }
                    if (itemName == "MomentaryButton")
                    {
                        //Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        buttonNumber++;
                        SetupMomentaryButton(cell);
                    }
                    if (itemName == "RectButton")
                    {
                        //Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        buttonNumber++;
                        SetupRectButton(cell, cellOrientation);
                    }
                    if (itemName == "Slider")
                    {
                        Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        var shader = GD.Load<Shader>("res://components/buttonPanel/assets/shaders/hBar.gdshader");
                        sliderMaterial = new ShaderMaterial
                        {
                            Shader = shader
                        };
                        sliderMaterial.SetShaderParameter("fill_amount", 0.0f);

                        
                        SetupSlider(cell, cellOrientation, sliderMaterial);
                        buttonNumber++;
                    }
                }
            }
        }
    }

    private void SetupToggleButton(Vector3I cell, int cellOrientation)
    {
        var toggleButton = new ToggleButton();
        AddChild(toggleButton);

        // Set the button's position
        Vector3 position = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);
        toggleButton.Position = position;

        // Initialize the button
        toggleButton.Initialize(buttonNumber, cellOrientation);
    }

    private void SetupMomentaryButton(Vector3I cell)
    {
        var momentaryButton = new MomentaryButton();
        AddChild(momentaryButton);

        // Set the button's position
        Vector3 position = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);
        momentaryButton.Position = position;

        // Initialize the button
        momentaryButton.Initialize(buttonNumber);
    }

    private void SetupRectButton(Vector3I cell, int cellOrientation)
    {
        var rectButton = new RectButton();
        AddChild(rectButton);

        // Set the button's position
        Vector3 position = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);
        rectButton.Position = position;

        // Initialize the button
        rectButton.Initialize(buttonNumber, cellOrientation);
    }

    private void SetupSlider(Vector3I cell, int cellOrientation, ShaderMaterial sliderMaterial)
    {
        var slider = new Slider();
        AddChild(slider);

        // Set the button's position
        Vector3 position = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);
        slider.Position = position;

        // Initialize the button
        slider.Initialize(buttonNumber, cellOrientation, sliderMaterial);
    }
}