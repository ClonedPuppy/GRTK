using Godot;
using System;

[Tool]
public partial class RectButton : Area3D
{
    private int buttonNumber;
    private Node3D trackedBody = null;
    private bool active = false;
    private MeshInstance3D lever;
    private float initialYPosition = -0.0025f;
    private float pressedYPosition = -0.005f;
    private float lastYPosition = 0.0f;
    private const float MinMovementThreshold = 0.0005f;
    private const float MaxMovementThreshold = 0.5f;
    private const float FingerCollisionOffset = 0.0025f;
    private const float ActivationThreshold = 0.003f;
    private AudioStreamPlayer clickSound;
    private ButtonStatesAutoload buttonStatesAutoload;
    private bool isRuntime;
    private Label3D label3D;

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
        var parent = GetParent();
        if (parent.Get("showLabels").AsBool())
        {
            SetupLabel(cellOrientation);
        }
        SetupAudio();

        if (isRuntime)
        {
            buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
            buttonStatesAutoload.UpdateButtonState(buttonNumber, false);
            UpdateButtonRestingPosition(false);
        }
    }

    public override void _Process(double delta)
    {
        if (!isRuntime || trackedBody == null || active) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        var movementDistance = lastYPosition - localPosition.Y;

        if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
        {
            UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

            if (localPosition.Y < ActivationThreshold)
            {
                ToggleButtonState();
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

    private void SetupLabel(int cellOrientation)
    {
        var labelPosition = new Vector3(0.0f, 0.0055f, 0.014f);
        var label3D = new Label3D
        {
            Text = $"Button: {buttonNumber}",
            Name = $"Label_{buttonNumber}",
            Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), labelPosition),
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
        active = true;
        clickSound.Play();

        if (isRuntime)
        {
            bool currentState = buttonStatesAutoload.GetValue(buttonNumber).AsBool();
            bool newState = !currentState;
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(newState));
            UpdateButtonRestingPosition(newState);
        }
    }

    private void UpdateButtonRestingPosition(bool isPressed)
    {
        float targetY = isPressed ? pressedYPosition : initialYPosition;
        lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, targetY, lever.Transform.Origin.Z) };
    }

    private void UpdateButtonPlatePosition(float yPosition)
    {
        bool isPressed = isRuntime && buttonStatesAutoload.GetValue(buttonNumber).AsBool();
        float baseY = isPressed ? pressedYPosition : initialYPosition;
        lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, baseY + yPosition - 0.007f, lever.Transform.Origin.Z) };
    }

    private void ResetButtonPlate()
    {
        if (isRuntime)
        {
            bool isPressed = buttonStatesAutoload.GetValue(buttonNumber).AsBool();
            UpdateButtonRestingPosition(isPressed);
        }
        else
        {
            lever.Transform = lever.Transform with { Origin = new Vector3(lever.Transform.Origin.X, initialYPosition, lever.Transform.Origin.Z) };
        }
    }

    private void OnBodyEntered(Node3D body) => trackedBody = body;

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody != body) return;

        trackedBody = null;
        active = false;
        ResetButtonPlate();
    }
}