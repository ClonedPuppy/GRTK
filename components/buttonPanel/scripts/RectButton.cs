using Godot;
using System;

[Tool]
public partial class RectButton : Area3D
{
    private Node3D trackedBody = null;
    private MeshInstance3D lever;
    private AudioStreamPlayer clickSound;
    private Label3D label3D;

    private bool isRuntime;
    private int buttonNumber;
    private float initialYPosition = -0.0025f;
    private float pressedYPosition = -0.005f;
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
        Name = $"RectButton_{buttonNumber}";

        SetupCollision();
        SetupLever(cellOrientation);
        SetupLabel();
        SetupAudio();

        if (isRuntime)
        {
            UpdateButtonRestingPosition(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!isRuntime || trackedBody == null) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        var movementDistance = lastYPosition - localPosition.Y;

        if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
        {
            UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

            bool shouldBePressed = localPosition.Y < ActivationThreshold;
            double currentTime = Time.GetTicksMsec() / 1000.0;

            if (shouldBePressed != lastReportedState && (currentTime - lastStateChangeTime) > DebounceTime)
            {
                lastReportedState = shouldBePressed;
                lastStateChangeTime = currentTime;
                if (shouldBePressed)
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
            Shape = new BoxShape3D { Size = new Vector3(0.02f, 0.01f, 0.02f) }
        };
        AddChild(collisionShape);
    }

    private void SetupLever(int cellOrientation)
    {
        lever = new MeshInstance3D { Name = $"Lever_{buttonNumber}" };
        var leverMeshLib = GD.Load<MeshLibrary>("res://components/buttonPanel/assets/resources/levers.tres");
        var mesh = leverMeshLib.GetItemMesh(2);
        if (mesh == null)
        {
            GD.PrintErr($"Failed to load lever mesh for rect button {buttonNumber}");
            return;
        }
        lever.Mesh = mesh.Duplicate() as Mesh;
        lever.Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, 0)), new Vector3(0, initialYPosition, 0));

        ApplyCellOrientation(cellOrientation);
        AddChild(lever);
    }

    private void ApplyCellOrientation(int cellOrientation)
    {
        switch (cellOrientation)
        {
            case 16:
                lever.RotationDegrees = lever.RotationDegrees with { Y = 90 };
                break;
            case 10:
                lever.RotationDegrees = lever.RotationDegrees with { Y = 180 };
                break;
            case 22:
                lever.RotationDegrees = lever.RotationDegrees with { Y = -90 };
                break;
        }
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

    private void ToggleButtonState()
    {
        clickSound.Play();
        currentState = !currentState;
        UpdateButtonRestingPosition(currentState);
        GetTree().CallGroup("UIListeners", "OnButtonStateChanged", buttonNumber, currentState);
    }

    private void UpdateButtonRestingPosition(bool isPressed)
    {
        float targetY = isPressed ? pressedYPosition : initialYPosition;
        lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, targetY, lever.Transform.Origin.Z) };
    }

    private void UpdateButtonPlatePosition(float yPosition)
    {
        float baseY = currentState ? pressedYPosition : initialYPosition;
        lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, baseY + yPosition - 0.007f, lever.Transform.Origin.Z) };
    }

    private void ResetButtonPlate()
    {
        UpdateButtonRestingPosition(currentState);
    }

    private void OnBodyEntered(Node3D body) => trackedBody = body;

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody != body) return;

        trackedBody = null;
        lastReportedState = false;
        ResetButtonPlate();
    }

    public void SetInitialState(bool state)
    {
        currentState = state;
        UpdateButtonRestingPosition(state);
    }
}
