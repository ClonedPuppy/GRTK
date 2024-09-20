using Godot;
using System;

public partial class AddButtonFunctions : GridMap
{
    private MeshLibrary uiMeshes;
    private int buttonNumber = 0;

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
                        Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        buttonNumber++;
                        SetupToggleButton(cell, cellOrientation);
                    }
                    if (itemName == "MomentaryButton")
                    {
                        Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        buttonNumber++;
                        SetupMomentaryButton(cell);
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
}