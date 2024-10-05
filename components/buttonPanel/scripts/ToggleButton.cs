using Godot;
using System;

[Tool]
public partial class ToggleButton : Area3D
{
    private int buttonNumber;
    private Node3D trackedBody = null;
    private bool isPressed = false;
    private MeshInstance3D lever;
    private float initialYPosition = -0.0025f;
    private float lastYPosition = 0.0f;
    private const float MinMovementThreshold = 0.0005f;
    private const float ActivationThreshold = 0.003f;
    private const float FingerCollisionOffset = 0.0025f;
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
        Name = $"ValueAdjusterButton_{buttonNumber}";

        SetupCollision();
        SetupLever(cellOrientation);
        // Check if the parent has showLabels property and set label visibility
        var parent = GetParent();
        if (parent is AddButtonFunctions addButtonFunctions)
        {
            if (label3D != null)
            {
                SetupLabel(cellOrientation);
            }
        }
        SetupAudio();

        if (isRuntime)
        {
            buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
            buttonStatesAutoload.UpdateButtonState(buttonNumber, false);
        }
    }

    public override void _Process(double delta)
    {
        if (!isRuntime || trackedBody == null) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        var movementDistance = lastYPosition - localPosition.Y;

        if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
        {
            UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

            if (localPosition.Y < ActivationThreshold && !isPressed)
            {
                PressButton();
            }
            else if (localPosition.Y >= ActivationThreshold && isPressed)
            {
                ReleaseButton();
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
        var leverMeshLib = GD.Load<MeshLibrary>("res://components/buttonPanel/assets/resources/levers.tres");
        var mesh = leverMeshLib.GetItemMesh(1);
        if (mesh == null)
        {
            GD.PrintErr($"Failed to load lever mesh for momentary switch {buttonNumber}");
            return;
        }
        lever.Mesh = mesh.Duplicate() as Mesh;
        lever.Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, 0)), new Vector3(0, initialYPosition, 0));

        ApplyCellOrientation(lever, cellOrientation);
        AddChild(lever);
    }

    private void SetupLabel(int cellOrientation)
    {
        var label3D = new Label3D
        {
            Text = $"Switch: {buttonNumber}",
            Name = $"Label_{buttonNumber}",
            PixelSize = 0.0001f,
            FontSize = 40,
            OutlineSize = 0,
            Modulate = Colors.Black
        };

        // Base position and rotation
        Vector3 labelPosition = new Vector3(0.0f, 0.0055f, 0.015f);
        Vector3 labelRotation = new Vector3(-90, 0, 0);

        switch (cellOrientation)
        {
            case 16: // 90 degrees (facing positive X)
                labelPosition = new Vector3(0.015f, 0.0055f, 0f);
                labelRotation = new Vector3(-95, -90, 0);
                break;
            case 10: // 180 degrees (facing negative Z)
                labelPosition = new Vector3(0f, 0.0055f, 0.015f);
                labelRotation = new Vector3(-90, 180, 180); // Changed to keep text upright
                break;
            case 22: // -90 degrees (facing negative X)
                labelPosition = new Vector3(-0.015f, 0.0055f, 0f);
                labelRotation = new Vector3(-90, 90, 0);
                break;
                // Default case (0) uses the base position and rotation
        }

        label3D.Position = labelPosition;
        label3D.RotationDegrees = labelRotation;

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

    private void PressButton()
    {
        isPressed = true;
        clickSound.Play();

        if (isRuntime)
        {
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(true));
        }
    }

    private void ReleaseButton()
    {
        isPressed = false;

        if (isRuntime)
        {
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(false));
        }
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
        isPressed = false;
        if (isRuntime)
        {
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(false));
        }
        ResetButtonPlate();
    }
}