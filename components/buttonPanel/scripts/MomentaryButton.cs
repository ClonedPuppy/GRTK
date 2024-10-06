using Godot;
using System;

[Tool]
public partial class MomentaryButton : Area3D
{
    private int buttonNumber;
    private Node3D trackedBody = null;
    private bool active = false;
    private MeshInstance3D lever;
    private float initialYPosition = -0.0025f;
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

    public void Initialize(int cellNo)
    {
        buttonNumber = cellNo;
        Name = $"MomentaryButton_{buttonNumber}";

        SetupCollision();
        SetupLever();
        var parent = GetParent();
        if (parent.Get("showLabels").AsBool())
        {
            SetupLabel();
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
        if (!isRuntime || trackedBody == null || active) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        var movementDistance = lastYPosition - localPosition.Y;

        if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
        {
            UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

            if (localPosition.Y < ActivationThreshold)
            {
                ActivateButton();
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

    private void SetupLever()
    {
        lever = new MeshInstance3D { Name = $"Lever_{buttonNumber}" };
        var leverMeshLib = GD.Load<MeshLibrary>("res://components/buttonPanel/assets/resources/levers.tres");
        var mesh = leverMeshLib.GetItemMesh(0);
        if (mesh == null)
        {
            GD.PrintErr($"Failed to load lever mesh for button {buttonNumber}");
            return;
        }
        lever.Mesh = mesh.Duplicate() as Mesh;
        lever.Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, 0)), new Vector3(0, initialYPosition, 0));
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

    private void ActivateButton()
    {
        active = true;
        clickSound.Play();
        buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(true));
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
        active = false;
        if (isRuntime)
        {
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(false));
        }
        ResetButtonPlate();
    }
}