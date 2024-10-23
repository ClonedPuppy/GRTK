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
    public override void _Ready()
    {
        _handTracker = GetNode<Node3D>("HandTracker");
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
}