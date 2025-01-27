using Godot;

[Tool]
public partial class ToggleButton : Area3D
{
    private Node3D trackedBody = null;
    private MeshInstance3D lever;
    private AudioStreamPlayer clickSound;
    private Label3D label3D;

    private bool isRuntime;
    private int buttonNumber;
    private float initialYPosition = -0.0025f;
    private float lastYPosition = 0.0f;
    private const float MinMovementThreshold = 0.0005f;
    private const float FingerCollisionOffset = 0.0025f;
    private const float ActivationThreshold = 0.003f;
    private bool lastReportedState = false;
    private double lastStateChangeTime = 0;
    private const double DebounceTime = 0.05;
    private bool currentState = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        isRuntime = !Engine.IsEditorHint();
    }

    public void Initialize(int cellNo, int cellOrientation)
    {
        buttonNumber = cellNo;
        Name = $"ToggleButton_{buttonNumber}";

        SetupCollision();
        SetupLever(cellOrientation);

        var buttonPanel = GetParent<GridMap>()?.GetParent<AddButtonFunctions>();
        if (buttonPanel != null && buttonPanel.showLabels)
        {
            SetupLabel();
        }
        SetupAudio();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!isRuntime || trackedBody == null) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        var movementDistance = lastYPosition - localPosition.Y;

        if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
        {
            UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

            bool currentState = localPosition.Y < ActivationThreshold;
            double currentTime = Time.GetTicksMsec() / 1000.0;

            if (currentState != lastReportedState && (currentTime - lastStateChangeTime) > DebounceTime)
            {
                lastReportedState = currentState;
                lastStateChangeTime = currentTime;
                if (currentState)
                {
                    ToggleButtonState();
                }
            }
        }

        lastYPosition = localPosition.Y;
    }

    private void SetupCollision()
    {
        var collisionShape = new CollisionShape3D
        {
            Name = $"Collision_{buttonNumber}",
            Shape = new CylinderShape3D { Height = 0.01f, Radius = 0.01f }
        };
        AddChild(collisionShape);
    }

    private void SetupLever(int cellOrientation)
    {
        lever = new MeshInstance3D { Name = $"Lever_{buttonNumber}" };
        var leverMeshLib = GD.Load<MeshLibrary>("res://components/buttonPanel/assets/resources/levers.res");
        var mesh = leverMeshLib.GetItemMesh(1);
        if (mesh == null)
        {
            GD.PrintErr($"Failed to load lever mesh for toggle switch {buttonNumber}");
            return;
        }
        lever.Mesh = mesh.Duplicate() as Mesh;
        lever.Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, 0)), new Vector3(0, initialYPosition, 0));

        ApplyCellOrientation(lever, cellOrientation);
        AddChild(lever);
    }

    private void SetupLabel()
    {
        var label3D = new Label3D
        {
            Text = $"Button: {buttonNumber}",
            Name = $"Label_{buttonNumber}",
            Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.013f)),
            PixelSize = 0.0001f,
            FontSize = 40,
            OutlineSize = 0,
            Modulate = Colors.Black
        };
        AddChild(label3D);
    }

    private void SetupAudio()
    {
        clickSound = new AudioStreamPlayer
        {
            Name = $"AudioStreamPlayer_{buttonNumber}",
            Stream = GD.Load<AudioStream>("res://components/buttonPanel/assets/audio/General_Button_2_User_Interface_Tap_FX_Sound.ogg")
        };
        AddChild(clickSound);
    }

    private void ApplyCellOrientation(Node3D node, int cellOrientation)
    {
        switch (cellOrientation)
        {
            case 16:
                node.RotateY(Mathf.DegToRad(90));
                break;
            case 10:
                node.RotateY(Mathf.DegToRad(180));
                break;
            case 22:
                node.RotateY(Mathf.DegToRad(-90));
                break;
        }
    }

    private void ToggleButtonState()
    {
        clickSound.Play();
        currentState = !currentState;
        GetTree().CallGroup("UIListeners", "OnButtonStateChanged", buttonNumber, currentState);
    }

    private void UpdateButtonPlatePosition(float yPosition)
    {
        lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, initialYPosition + yPosition - 0.007f, lever.Transform.Origin.Z) };
    }

    private void ResetButtonPlate()
    {
        lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, initialYPosition, lever.Transform.Origin.Z) };
    }

    private void OnBodyEntered(Node3D body) => trackedBody = body;

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody != body) return;

        trackedBody = null;
        lastReportedState = false;
        ResetButtonPlate();
    }
}
