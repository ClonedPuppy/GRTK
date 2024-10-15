using Godot;
using Godot.Collections;

[Tool]
public partial class AddButtonFunctions : Node3D
{
    [Export]
    public Dictionary<int, string> labelNames;
    private int buttonNumber = 0;
    private GridMap gridMap;
    private MeshLibrary uiMeshes;
    private Vector3 centerTile = new Vector3(0.02f, 0.005f, 0.02f);
    private ShaderMaterial sliderMaterial;

    public override void _Ready()
    {
        labelNames = new Dictionary<int, string>();
        gridMap = GetNode<GridMap>("GridMap");
        uiMeshes = gridMap.MeshLibrary;
        if (uiMeshes != null)
        {
            var usedCells = gridMap.GetUsedCells();
            foreach (Vector3I cell in usedCells)
            {
                int itemId = gridMap.GetCellItem(cell);
                int cellOrientation = gridMap.GetCellItemOrientation(cell);
                if (itemId != -1)
                {
                    string itemName = uiMeshes.GetItemName(itemId);
                    if (itemName == "ToggleButton")
                    {
                        buttonNumber++;
                        SetupToggleButton(cell, cellOrientation);
                    }
                    if (itemName == "MomentaryButton")
                    {
                        buttonNumber++;
                        SetupMomentaryButton(cell);
                    }
                    if (itemName == "RectButton")
                    {
                        buttonNumber++;
                        SetupRectButton(cell, cellOrientation);
                    }
                    if (itemName == "Slider")
                    {
                        buttonNumber++;
                        SetupSlider(cell, cellOrientation);
                    }
                }
            }
        }
    }

    private void SetupToggleButton(Vector3I cell, int cellOrientation)
    {
        var toggleButton = new ToggleButton();
        gridMap.AddChild(toggleButton);

        Vector3 position = cell * gridMap.CellSize + centerTile;
        toggleButton.Position = position;

        toggleButton.Initialize(buttonNumber, cellOrientation);
    }

    private void SetupMomentaryButton(Vector3I cell)
    {
        var momentaryButton = new MomentaryButton();
        gridMap.AddChild(momentaryButton);

        Vector3 position = cell * gridMap.CellSize + centerTile;
        momentaryButton.Position = position;

        momentaryButton.Initialize(buttonNumber);
    }

    private void SetupRectButton(Vector3I cell, int cellOrientation)
    {
        var rectButton = new RectButton();
        gridMap.AddChild(rectButton);

        Vector3 position = cell * gridMap.CellSize + centerTile;
        rectButton.Position = position;

        rectButton.Initialize(buttonNumber, cellOrientation);
    }

    private void SetupSlider(Vector3I cell, int cellOrientation)
    {
        var slider = new Slider();
        gridMap.AddChild(slider);

        Vector3 position = cell * gridMap.CellSize + centerTile;
        slider.Position = position;

        slider.Initialize(buttonNumber, cellOrientation);
    }
}
