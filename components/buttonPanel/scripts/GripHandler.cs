using System.Collections.Generic;
using Godot;

public partial class GripHandler
{
    private readonly Node3D parent;
    private readonly List<Aabb> gripBounds = new();
    public IReadOnlyList<Aabb> GripBounds => gripBounds;
    private bool isGrabbed;
    private Transform3D grabOffset;
    private bool isLeftHand; // Track which hand is gripping

    // Movement parameters tuned for efficiency
    private const float SMOOTH_FACTOR = 15.0f;
    private const float MAX_MOVE_SPEED = 2.0f;
    private const float MIN_MOVE_THRESHOLD = 0.001f;
    private static readonly Vector3 DefaultSize = new(0.04f, 0.04f, 0.04f);

    public GripHandler(Node3D parent)
    {
        this.parent = parent;
    }

    public void UpdateGripBounds(GridMap gridMap)
    {
        gripBounds.Clear();

        foreach (Vector3I cell in gridMap.GetUsedCells())
        {
            if (gridMap.GetCellItem(cell) == -1 ||
                gridMap.MeshLibrary.GetItemName(gridMap.GetCellItem(cell)) != "Grip") continue;

            var worldPos = gridMap.MapToLocal(cell);
            gripBounds.Add(new Aabb(worldPos - DefaultSize / 2, DefaultSize));
        }
    }

    public bool TryGrab(Vector3 fingerPosition, Transform3D pinchTransform, bool isLeft)
    {
        if (isGrabbed) return false;

        var localFingerPos = parent.ToLocal(fingerPosition);

        foreach (var bound in gripBounds)
        {
            if (bound.HasPoint(localFingerPos))
            {
                isGrabbed = true;
                isLeftHand = isLeft;
                grabOffset = pinchTransform.Inverse() * parent.GlobalTransform;
                return true;
            }
        }
        return false;
    }

    public void ProcessMovement(Transform3D pinchTransform, double delta)
    {
        if (!isGrabbed) return;

        var targetTransform = pinchTransform * grabOffset;
        var currentTransform = parent.GlobalTransform;

        var movement = targetTransform.Origin - currentTransform.Origin;
        var movementLength = movement.Length();

        if (movementLength < MIN_MOVE_THRESHOLD) return;

        var maxMove = MAX_MOVE_SPEED * (float)delta;
        var newPosition = currentTransform.Origin;

        if (movementLength > maxMove)
        {
            movement = movement.Normalized() * maxMove;
            newPosition = currentTransform.Origin + movement;
        }
        else
        {
            newPosition = currentTransform.Origin.Lerp(targetTransform.Origin, SMOOTH_FACTOR * (float)delta);
        }

        // Handle rotation using quaternions
        var currentQuat = currentTransform.Basis.GetRotationQuaternion();
        var targetQuat = targetTransform.Basis.GetRotationQuaternion();
        var newRotation = currentQuat.Slerp(targetQuat, SMOOTH_FACTOR * (float)delta);

        // Apply final transform
        parent.GlobalTransform = new Transform3D(new Basis(newRotation), newPosition);
    }

    public void Release() => isGrabbed = false;
    public bool IsGrabbedByLeftHand => isGrabbed && isLeftHand;
    public bool IsGrabbedByRightHand => isGrabbed && !isLeftHand;
}