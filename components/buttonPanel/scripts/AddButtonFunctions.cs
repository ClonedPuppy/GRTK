using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class AddButtonFunctions : Node3D
{
    [Export]
    public bool showLabels = false;
    [Export] public NodePath HandPoseDetectorPath { get; set; }
    [Export] public NodePath PinchPointPath { get; set; }
    [Export] public bool ShowDebug { get; set; } = true;  // Toggle for debug visualization
    private HandPoseDetector handPoseDetector;
    private Node3D pinchPoint;
    private bool isGrabbed;
    private Transform3D grabOffset;
    private Godot.Collections.Array<Aabb> boundingBoxes;
    private Node3D debugNode; // Parent node for debug visualization

    private Godot.Collections.Array<Aabb> currentBounds;
    //public Dictionary<int, string> labelNames;
    private GridMap[] layoutInstances;
    private int currentLayoutIndex = -1;
    private MeshLibrary uiMeshes;
    private Vector3 centerTile = new Vector3(0.02f, 0.005f, 0.02f);
    private double lastSwitchTime = 0;
    private const double SwitchDebounceTime = 0.5f; // 500ms debounce time
    public override void _Ready()
    {
        handPoseDetector = GetNode<HandPoseDetector>(HandPoseDetectorPath);
        pinchPoint = GetNode<Node3D>(PinchPointPath);

        // Initialize collections
        boundingBoxes = new Godot.Collections.Array<Aabb>();

        // Initialize layout instances
        layoutInstances = GetChildren().OfType<GridMap>().ToArray();
        if (layoutInstances.Length > 0)
        {
            uiMeshes = layoutInstances[0].MeshLibrary;
            UpdateBoundingBoxes();

            if (ShowDebug)
            {
                CreateDebugVisualization();
            }
        }

        handPoseDetector.PoseStarted += OnPoseStarted;
        handPoseDetector.PoseEnded += OnPoseEnded;

        uiMeshes = layoutInstances[0].MeshLibrary;
        var gmIndex = 0;
        var btnNumber = 0;

        // Initialize bounding boxes from GridMap
        UpdateBoundingBoxes();

        foreach (var gridMapInst in layoutInstances)
        {
            if (uiMeshes != null)
            {
                var usedCells = gridMapInst.GetUsedCells();
                foreach (Vector3I cell in usedCells)
                {
                    int itemId = gridMapInst.GetCellItem(cell);
                    int cellOrientation = gridMapInst.GetCellItemOrientation(cell);
                    if (itemId != -1)
                    {
                        string itemName = uiMeshes.GetItemName(itemId);
                        if (itemName == "ToggleButton")
                        {
                            btnNumber++;
                            SetupToggleButton(cell, cellOrientation, gmIndex, btnNumber);
                        }
                        else if (itemName == "MomentaryButton")
                        {
                            btnNumber++;
                            SetupMomentaryButton(cell, gmIndex, btnNumber);
                        }
                        else if (itemName == "TabButton")
                        {
                            btnNumber++;
                            SetupTabButton(cell, gmIndex, btnNumber);
                        }
                        else if (itemName == "RectButton")
                        {
                            btnNumber++;
                            SetupRectButton(cell, cellOrientation, gmIndex, btnNumber);
                        }
                        else if (itemName == "Slider")
                        {
                            btnNumber++;
                            SetupSlider(cell, cellOrientation, gmIndex, btnNumber);
                        }
                        else if (itemName == "StepSlider")
                        {
                            btnNumber++;
                            SetupStepSlider(cell, cellOrientation, gmIndex, btnNumber);
                        }
                    }
                }
                gmIndex++;
            }
        }

        // Hide all layouts except the first one (if any exist)
        layoutInstances[0].Visible = true;
        for (int i = 1; i < layoutInstances.Length; i++)
        {
            layoutInstances[i].Visible = false;
        }

        // Set the current layout to the first one if it exists
        if (layoutInstances.Length > 0)
        {
            currentLayoutIndex = 0;
        }
        InitializeActiveLayout();

        // Setup bounds
        currentBounds = GetGripBounds(layoutInstances[0], 1);
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint())
        {
            if (pinchPoint != null)
            {
                // Debug print finger position relative to bounds
                var fingerPos = ToLocal(pinchPoint.GlobalPosition);
                GD.Print($"Finger pos: {fingerPos}");

                if (isGrabbed)
                {
                    GlobalTransform = pinchPoint.GlobalTransform * grabOffset;
                }
            }
        }
    }

    private void SetupToggleButton(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var toggleButton = new ToggleButton();
        layoutInstances[index].AddChild(toggleButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        toggleButton.Position = position;
        toggleButton.Initialize(btnNumber, cellOrientation);
    }

    private void SetupMomentaryButton(Vector3I cell, int index, int btnNumber)
    {
        var momentaryButton = new MomentaryButton();
        layoutInstances[index].AddChild(momentaryButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        momentaryButton.Position = position;
        momentaryButton.Initialize(btnNumber);
    }

    private void SetupTabButton(Vector3I cell, int index, int btnNumber)
    {
        var tabButton = new TabButton();
        layoutInstances[index].AddChild(tabButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        tabButton.Position = position;
        tabButton.Initialize(btnNumber);
    }

    private void SetupRectButton(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var rectButton = new RectButton();
        layoutInstances[index].AddChild(rectButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        rectButton.Position = position;
        rectButton.Initialize(btnNumber, cellOrientation);
    }

    private void SetupSlider(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var slider = new Slider();
        layoutInstances[index].AddChild(slider);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        slider.Position = position;
        slider.Initialize(btnNumber, cellOrientation);
    }

    private void SetupStepSlider(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var stepSlider = new StepSlider();
        layoutInstances[index].AddChild(stepSlider);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        stepSlider.Position = position;
        stepSlider.Initialize(btnNumber, cellOrientation);
    }

    private void SetGridMapProcessing(GridMap gridMap, bool enable)
    {
        foreach (Node child in gridMap.GetChildren())
        {
            if (child is Area3D area)
            {
                area.SetPhysicsProcess(enable);
                area.SetProcessInput(enable);
                area.Monitoring = enable;
                area.Monitorable = enable;
            }
        }
    }

    public void SwitchToLayout(int index)
    {
        if (index >= 1 && index < layoutInstances.Length)  // Start from index 1
        {
            if (currentLayoutIndex >= 1)  // Only hide if it's not gridmap[0]
            {
                layoutInstances[currentLayoutIndex].Visible = false;
                SetGridMapProcessing(layoutInstances[currentLayoutIndex], false);
            }

            layoutInstances[index].Visible = true;
            SetGridMapProcessing(layoutInstances[index], true);
            currentLayoutIndex = index;
        }
    }

    public void SwitchToSpecificLayout(int buttonNumber)
    {
        // Since button numbers start at 1 and correlate directly to gridmap indices
        // Check if the buttonNumber corresponds to a valid gridmap (excluding gridmap[0])
        if (buttonNumber < layoutInstances.Length)
        {
            SwitchToLayout(buttonNumber);
        }
        // If button number is higher than available gridmaps, do nothing
    }

    public void InitializeActiveLayout()
    {
        if (layoutInstances.Length > 0)
        {
            // Keep gridmap[0] always active
            SetGridMapProcessing(layoutInstances[0], true);
            layoutInstances[0].Visible = true;

            // Start from index 1, set initial state
            if (layoutInstances.Length > 1)
            {
                currentLayoutIndex = 1;
                layoutInstances[1].Visible = true;
                SetGridMapProcessing(layoutInstances[1], true);

                // Hide and disable all other gridmaps
                for (int i = 2; i < layoutInstances.Length; i++)
                {
                    layoutInstances[i].Visible = false;
                    SetGridMapProcessing(layoutInstances[i], false);
                }
            }
        }
    }

    public void SwitchToNextLayout()
    {
        double currentTime = Time.GetTicksMsec() / 1000.0;
        if (currentTime - lastSwitchTime < SwitchDebounceTime)
        {
            return;
        }

        // Only switch between gridmaps 1 and above
        int nextIndex = currentLayoutIndex + 1;
        if (nextIndex >= layoutInstances.Length || nextIndex < 1)
        {
            nextIndex = 1;  // Reset to gridmap[1]
        }

        SwitchToLayout(nextIndex);
        lastSwitchTime = currentTime;
    }

    public int GetLayoutCount()
    {
        return layoutInstances.Length;
    }

private void UpdateBoundingBoxes()
{
    boundingBoxes = new Godot.Collections.Array<Aabb>();

    if (layoutInstances[0] == null) return;

    foreach (Vector3I cell in layoutInstances[0].GetUsedCells())
    {
        int itemId = layoutInstances[0].GetCellItem(cell);
        string meshName = layoutInstances[0].MeshLibrary.GetItemName(itemId);

        if (meshName != "Grip") continue;

        Vector3 worldPos = layoutInstances[0].MapToLocal(cell);
        // Set your desired size here
        Vector3 size = new Vector3(0.04f, 0.04f, 0.04f);  // Example: 4cm cube
        boundingBoxes.Add(new Aabb(worldPos - size/2, size));
    }
}

    private void CreateDebugVisualization()
    {
        // Check if we have any bounding boxes to visualize
        if (boundingBoxes == null || boundingBoxes.Count == 0) return;

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

    private void OnPoseStarted(string poseName)
    {
        if (poseName == "Pinch" && !isGrabbed && pinchPoint != null)
        {
            var fingerPos = pinchPoint.GlobalPosition;

            // Convert finger position to local space of this node
            var localFingerPos = ToLocal(fingerPos);

            foreach (var box in boundingBoxes)
            {
                if (box.HasPoint(localFingerPos))
                {
                    GD.Print("Grabbed!"); // Debug print
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

    public Godot.Collections.Array<Aabb> GetGripBounds(GridMap gridMap, int regionSize = 4)
    {
        var regions = new Godot.Collections.Dictionary<Vector3I, Godot.Collections.Array<Vector3>>();

        // Get all cells and filter for only "Grip" cells
        foreach (Vector3I cell in gridMap.GetUsedCells())
        {
            // Get the mesh library item id at this cell
            int itemId = gridMap.GetCellItem(cell);

            // Get the name of the mesh at this cell
            string meshName = gridMap.MeshLibrary.GetItemName(itemId);

            // Only process cells named "Grip"
            if (meshName != "Grip") continue;

            var region = new Vector3I(
                Mathf.FloorToInt(cell.X / regionSize),
                Mathf.FloorToInt(cell.Y / regionSize),
                Mathf.FloorToInt(cell.Z / regionSize)
            );

            if (!regions.ContainsKey(region))
                regions[region] = new Godot.Collections.Array<Vector3>();

            regions[region].Add(gridMap.MapToLocal(cell));
        }

        var bounds = new Godot.Collections.Array<Aabb>();
        foreach (var region in regions.Values)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var pos in region)
            {
                min = min.Min(pos);
                max = max.Max(pos);
            }

            bounds.Add(new Aabb(min, max - min));
        }

        return bounds;
    }
}
