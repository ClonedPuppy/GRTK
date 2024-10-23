using Godot;
using System;

public partial class Grabbable : MeshInstance3D
{
    [Export] public NodePath HandPoseDetectorPath { get; set; }
    [Export] public NodePath PinchPointPath { get; set; }
    [Export] public GridMap TargetGrid { get; set; }
    [Export] public bool ShowDebug { get; set; } = true;  // Toggle for debug visualization

    private HandPoseDetector handPoseDetector;
    private Node3D pinchPoint;
    private bool isGrabbed;
    private Transform3D grabOffset;
    private Godot.Collections.Array<Aabb> boundingBoxes;
    private Node3D debugNode; // Parent node for debug visualization

    public override void _Ready()
    {
        handPoseDetector = GetNode<HandPoseDetector>(HandPoseDetectorPath);
        pinchPoint = GetNode<Node3D>(PinchPointPath);
        
        // Initialize bounding boxes from GridMap
        UpdateBoundingBoxes();

        handPoseDetector.PoseStarted += OnPoseStarted;
        handPoseDetector.PoseEnded += OnPoseEnded;

        if (ShowDebug)
        {
            CreateDebugVisualization();
        }
    }

    private void UpdateBoundingBoxes()
    {
        boundingBoxes = new Godot.Collections.Array<Aabb>();
        
        if (TargetGrid == null) return;

        foreach (Vector3I cell in TargetGrid.GetUsedCells())
        {
            int itemId = TargetGrid.GetCellItem(cell);
            string meshName = TargetGrid.MeshLibrary.GetItemName(itemId);
            
            if (meshName != "Grip") continue;

            Vector3 worldPos = TargetGrid.MapToLocal(cell);
            Vector3 size = new Vector3(1, 1, 1);  // Adjust size as needed
            boundingBoxes.Add(new Aabb(worldPos - size/2, size));
        }
    }

    private void CreateDebugVisualization()
    {
        // Remove old debug visualization if it exists
        if (debugNode != null)
        {
            debugNode.QueueFree();
        }

        // Create new debug node
        debugNode = new Node3D();
        AddChild(debugNode);

        // Create visualization for each bounding box
        foreach (var box in boundingBoxes)
        {
            var meshInstance = new MeshInstance3D();
            var boxMesh = new BoxMesh
            {
                Size = box.Size
            };

            var material = new StandardMaterial3D
            {
                AlbedoColor = new Color(1, 1, 0, 0.3f), // Semi-transparent yellow
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha
            };

            meshInstance.Mesh = boxMesh;
            meshInstance.MaterialOverride = material;
            meshInstance.Position = box.Position + box.Size / 2; // Center the mesh in the box
            debugNode.AddChild(meshInstance);
        }
    }

    public override void _Process(double delta)
    {
        if (isGrabbed)
        {
            GlobalTransform = pinchPoint.GlobalTransform * grabOffset;
        }
    }

    private void OnPoseStarted(string poseName)
    {
        if (poseName == "Pinch" && !isGrabbed && pinchPoint != null)
        {
            var fingerPos = pinchPoint.GlobalPosition;
            
            foreach (var box in boundingBoxes)
            {
                var localFingerPos = fingerPos - box.Position;
                if (box.HasPoint(localFingerPos))
                {
                    isGrabbed = true;
                    grabOffset = pinchPoint.GlobalTransform.Inverse() * GlobalTransform;
                    break;
                }
            }
        }
    }

    private void OnPoseEnded(string poseName)
    {
        if (poseName == "Pinch")
        {
            isGrabbed = false;
        }
    }
}