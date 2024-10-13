using Godot;
using System.Collections.Generic;

[Tool]
public partial class GridMapHandController : GridMap
{
    private Node3D _handTracker;
    private bool _isGripping = false;
    private Vector3 _grabPosition = Vector3.Zero;
    private Vector3 _initialHandPosition = Vector3.Zero;
    private List<int> _grabbableTiles = new List<int> { 1, 2, 3 }; // Example tile IDs that are grabbable

    private MeshLibrary _uiMeshes;
    private int _buttonNumber = 0;

    public override void _Ready()
    {
        _handTracker = GetNode<Node3D>("HandTracker");

        _uiMeshes = MeshLibrary;
        if (_uiMeshes != null)
        {
            SetupButtons();
        }
    }

    public override void _Process(double delta)
    {
        HandleHandTracking();
    }

    private void HandleHandTracking()
    {
        Vector3 handPosition = _handTracker.GlobalTransform.Origin;
        Vector3I gridPos = LocalToMap(ToLocal(handPosition));
        int tileId = GetCellItem(gridPos);

        if (_grabbableTiles.Contains(tileId))
        {
            if (CheckGripPose())
            {
                if (!_isGripping)
                {
                    // Start of grip
                    _isGripping = true;
                    _grabPosition = ToLocal(handPosition);
                    _initialHandPosition = handPosition;
                }
                else
                {
                    // Continuing grip
                    Vector3 movement = handPosition - _initialHandPosition;
                    GlobalTransform = GlobalTransform.Translated(movement);
                    _initialHandPosition = handPosition;
                }
            }
            else
            {
                _isGripping = false;
            }
        }
        else
        {
            _isGripping = false;
        }
    }

    private bool CheckGripPose()
    {
        // Implement your grip pose detection logic here
        // This is a placeholder implementation
        // return _handTracker.IsFistClosed();
        return Input.IsActionPressed("grip"); // Example using input action
    }

    private void SetupButtons()
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
                    _buttonNumber++;
                    SetupToggleButton(cell, cellOrientation);
                }
                else if (itemName == "MomentaryButton")
                {
                    _buttonNumber++;
                    SetupMomentaryButton(cell);
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
        toggleButton.Initialize(_buttonNumber, cellOrientation);
    }

    private void SetupMomentaryButton(Vector3I cell)
    {
        var momentaryButton = new MomentaryButton();
        AddChild(momentaryButton);

        // Set the button's position
        Vector3 position = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);
        momentaryButton.Position = position;

        // Initialize the button
        momentaryButton.Initialize(_buttonNumber);
    }
}