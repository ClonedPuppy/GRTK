using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class AddButtonFunctions : Node3D
{
    [Export] public bool showLabels = false;
    [Export] public NodePath LeftHandPoseDetectorPath { get; set; }
    [Export] public NodePath RightHandPoseDetectorPath { get; set; }
    [Export] public bool ShowDebug { get; set; } = true;

    private HandPoseDetector leftHandPoseDetector;
    private HandPoseDetector rightHandPoseDetector;
    private GripHandler gripHandler;
    private Node3D debugNode;
    private GridMap[] layoutInstances;
    private MeshLibrary uiMeshes;
    private readonly Vector3 centerTile = new(0.02f, 0.005f, 0.02f);
    private int currentLayoutIndex = -1;
    private static readonly Vector3 LEFT_PINCH_OFFSET = new Vector3(-0.021f, 0.093f, 0.079f);
    private static readonly Vector3 RIGHT_PINCH_OFFSET = new Vector3(0.021f, 0.093f, 0.079f);
    private MeshInstance3D leftDebugSphere;
    private MeshInstance3D rightDebugSphere;
    private BoneAttachment3D leftWristAttachment;
    private BoneAttachment3D rightWristAttachment;

    public override void _Ready()
    {
        bool isRuntime = !Engine.IsEditorHint();
        layoutInstances = GetChildren().OfType<GridMap>().ToArray();

        if (layoutInstances.Length > 0)
        {
            uiMeshes = layoutInstances[0].MeshLibrary;

            if (isRuntime)
            {
                InitializeRuntimeComponents();
            }
        }

        SetupAllButtons();
        InitializeLayouts();
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint() && gripHandler != null)
        {
            if (gripHandler.IsGrabbedByLeftHand && leftWristAttachment != null)
            {
                var grabPosition = leftWristAttachment.GlobalPosition +
                    leftWristAttachment.GlobalTransform.Basis * LEFT_PINCH_OFFSET;
                var processTransform = new Transform3D(
                    leftWristAttachment.GlobalTransform.Basis,
                    grabPosition
                );
                gripHandler.ProcessMovement(processTransform, delta);
            }
            else if (gripHandler.IsGrabbedByRightHand && rightWristAttachment != null)
            {
                var grabPosition = rightWristAttachment.GlobalPosition +
                    rightWristAttachment.GlobalTransform.Basis * RIGHT_PINCH_OFFSET;
                var processTransform = new Transform3D(
                    rightWristAttachment.GlobalTransform.Basis,
                    grabPosition
                );
                gripHandler.ProcessMovement(processTransform, delta);
            }
        }
    }

    private void InitializeRuntimeComponents()
    {
        if (string.IsNullOrEmpty(LeftHandPoseDetectorPath.ToString()) ||
            string.IsNullOrEmpty(RightHandPoseDetectorPath.ToString()))
        {
            GD.PrintErr("Hand pose detector paths not set");
            return;
        }

        try
        {
            leftHandPoseDetector = GetNode<HandPoseDetector>(LeftHandPoseDetectorPath);
            rightHandPoseDetector = GetNode<HandPoseDetector>(RightHandPoseDetectorPath);

            if (leftHandPoseDetector == null || rightHandPoseDetector == null)
            {
                GD.PrintErr("Failed to get hand pose detectors");
                return;
            }

            leftHandPoseDetector.PoseStarted += OnLeftPoseStarted;
            leftHandPoseDetector.PoseEnded += OnLeftPoseEnded;
            rightHandPoseDetector.PoseStarted += OnRightPoseStarted;
            rightHandPoseDetector.PoseEnded += OnRightPoseEnded;

            gripHandler = new GripHandler(this);
            gripHandler.UpdateGripBounds(layoutInstances[0]);

            SetupWristAttachments();

            if (ShowDebug)
            {
                CreateDebugVisualization();
                CreateDebugSpheres();
            }
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Error initializing runtime components: {e.Message}");
        }
    }

    private void SetupWristAttachments()
    {
        // Left hand setup
        var leftSkeleton = FindFirstNodeOfType<Skeleton3D>(leftHandPoseDetector.GetParent());
        if (leftSkeleton != null)
        {
            leftWristAttachment = new BoneAttachment3D();
            leftWristAttachment.BoneName = "LeftHand";
            leftSkeleton.AddChild(leftWristAttachment);
        }

        // Right hand setup
        var rightSkeleton = FindFirstNodeOfType<Skeleton3D>(rightHandPoseDetector.GetParent());
        if (rightSkeleton != null)
        {
            rightWristAttachment = new BoneAttachment3D();
            rightWristAttachment.BoneName = "RightHand";
            rightSkeleton.AddChild(rightWristAttachment);
        }
    }

    private T FindFirstNodeOfType<T>(Node startNode) where T : Node
    {
        if (startNode == null) return null;

        // Check if any direct child is of type T
        foreach (Node child in startNode.GetChildren())
        {
            if (child is T typedNode) return typedNode;

            // Recursively check this child's children
            var result = FindFirstNodeOfType<T>(child);
            if (result != null) return result;
        }

        return null;
    }

    private void CreateDebugSpheres()
    {
        if (leftWristAttachment != null)
        {
            leftDebugSphere = CreateDebugSphere(Colors.Red);
            leftWristAttachment.AddChild(leftDebugSphere);
            leftDebugSphere.Position = LEFT_PINCH_OFFSET;
        }

        if (rightWristAttachment != null)
        {
            rightDebugSphere = CreateDebugSphere(Colors.Blue);
            rightWristAttachment.AddChild(rightDebugSphere);
            rightDebugSphere.Position = RIGHT_PINCH_OFFSET;
        }
    }

    private MeshInstance3D CreateDebugSphere(Color color)
    {
        return new MeshInstance3D
        {
            Mesh = new SphereMesh
            {
                Radius = 0.005f,
                Height = 0.01f
            },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = color,
                Emission = color,
                EmissionEnabled = true
            }
        };
    }

    private void SetupAllButtons()
    {
        var btnNumber = 0;

        for (int gmIndex = 0; gmIndex < layoutInstances.Length; gmIndex++)
        {
            if (uiMeshes == null) continue;

            var usedCells = layoutInstances[gmIndex].GetUsedCells();
            foreach (Vector3I cell in usedCells)
            {
                int itemId = layoutInstances[gmIndex].GetCellItem(cell);
                if (itemId == -1) continue;

                int cellOrientation = layoutInstances[gmIndex].GetCellItemOrientation(cell);
                string itemName = uiMeshes.GetItemName(itemId);

                if (itemName is "ToggleButton" or "MomentaryButton" or "TabButton"
                    or "RectButton" or "Slider" or "StepSlider")
                {
                    btnNumber++;
                    SetupButton(cell, cellOrientation, gmIndex, btnNumber, itemName);
                }
            }
        }
    }

    private void SetupButton(Vector3I cell, int cellOrientation, int index, int btnNumber, string buttonType)
    {
        Area3D button = buttonType switch
        {
            "ToggleButton" => new ToggleButton(),
            "MomentaryButton" => new MomentaryButton(),
            "TabButton" => new TabButton(),
            "RectButton" => new RectButton(),
            "Slider" => new Slider(),
            "StepSlider" => new StepSlider(),
            _ => null
        };

        if (button == null) return;

        layoutInstances[index].AddChild(button);
        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        button.Position = position;

        switch (button)
        {
            case ToggleButton tb:
                tb.Initialize(btnNumber, cellOrientation);
                break;
            case MomentaryButton mb:
                mb.Initialize(btnNumber);
                break;
            case TabButton tab:
                tab.Initialize(btnNumber);
                break;
            case RectButton rb:
                rb.Initialize(btnNumber, cellOrientation);
                break;
            case Slider s:
                s.Initialize(btnNumber, cellOrientation);
                break;
            case StepSlider ss:
                ss.Initialize(btnNumber, cellOrientation);
                break;
        }
    }

    private void InitializeLayouts()
    {
        if (layoutInstances.Length == 0) return;

        // Only process what's needed initially
        layoutInstances[0].Visible = true;
        SetGridMapProcessing(layoutInstances[0], true);

        if (layoutInstances.Length > 1)
        {
            // Process layout 1 and disable others in one pass
            for (int i = 1; i < layoutInstances.Length; i++)
            {
                layoutInstances[i].Visible = i == 1;
                SetGridMapProcessing(layoutInstances[i], i == 1);
            }
            currentLayoutIndex = 1;
        }
    }

    private void SetGridMapProcessing(GridMap gridMap, bool enable)
    {
        foreach (Area3D area in gridMap.GetChildren().OfType<Area3D>())
        {
            area.SetPhysicsProcess(enable);
            area.SetProcessInput(enable);
            area.Monitoring = enable;
            area.Monitorable = enable;
        }
    }

    public void SwitchToLayout(int index)
    {
        if (index < 1 || index >= layoutInstances.Length) return;

        if (currentLayoutIndex >= 1)
        {
            layoutInstances[currentLayoutIndex].Visible = false;
            SetGridMapProcessing(layoutInstances[currentLayoutIndex], false);
        }

        layoutInstances[index].Visible = true;
        SetGridMapProcessing(layoutInstances[index], true);
        currentLayoutIndex = index;
    }

    public void SwitchToSpecificLayout(int buttonNumber)
    {
        if (buttonNumber < layoutInstances.Length)
        {
            SwitchToLayout(buttonNumber);
        }
    }

    private void CreateDebugVisualization()
    {
        if (layoutInstances[0] == null || gripHandler == null) return;

        debugNode?.QueueFree();

        debugNode = new Node3D();
        AddChild(debugNode);

        gripHandler.UpdateGripBounds(layoutInstances[0]);
        foreach (var box in gripHandler.GripBounds)
        {
            var meshInstance = new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = box.Size },
                MaterialOverride = new StandardMaterial3D
                {
                    AlbedoColor = new Color(1, 1, 0, 0.3f),
                    Transparency = BaseMaterial3D.TransparencyEnum.Alpha
                },
                Position = box.Position + box.Size / 2
            };
            debugNode.AddChild(meshInstance);
        }
    }

    private void OnLeftPoseStarted(string poseName)
    {
        if (poseName == "Pinch" && leftWristAttachment != null)
        {
            var grabPosition = leftWristAttachment.GlobalPosition +
                leftWristAttachment.GlobalTransform.Basis * LEFT_PINCH_OFFSET;
            var grabTransform = new Transform3D(
                leftWristAttachment.GlobalTransform.Basis,
                grabPosition
            );
            gripHandler.TryGrab(grabPosition, grabTransform, true);
        }
    }

    private void OnRightPoseStarted(string poseName)
    {
        if (poseName == "Pinch" && rightWristAttachment != null)
        {
            var grabPosition = rightWristAttachment.GlobalPosition +
                rightWristAttachment.GlobalTransform.Basis * RIGHT_PINCH_OFFSET;
            var grabTransform = new Transform3D(
                rightWristAttachment.GlobalTransform.Basis,
                grabPosition
            );
            gripHandler.TryGrab(grabPosition, grabTransform, false);
        }
    }

    private void OnLeftPoseEnded(string poseName)
    {
        if (poseName == "Pinch")
        {
            gripHandler.Release();
        }
    }

    private void OnRightPoseEnded(string poseName)
    {
        if (poseName == "Pinch")
        {
            gripHandler.Release();
        }
    }
}