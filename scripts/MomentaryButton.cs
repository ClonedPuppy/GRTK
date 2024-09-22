using Godot;
using System;

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
    private AudioStreamPlayer clickSound;

    public override void _Ready()
    {
        // Signals emitted at entry and exit
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void Initialize(int cellNo)
    {
        buttonNumber = cellNo;
        Name = $"MomentaryButton_{buttonNumber}";

        // // Signals emitted at entry and exit
        // BodyEntered += OnBodyEntered;
        // BodyExited += OnBodyExited;


        // Add a collision node
        var collisionShape = new CollisionShape3D();
        collisionShape.Name = "Collision_" + cellNo.ToString();
        AddChild(collisionShape);

        // Add a shape to the collision node, assuming button size is 0.01 x 0.01
        collisionShape.Shape = new CylinderShape3D { Height = 0.01f, Radius = 0.01f };

        // Add a MeshInstance3D with the leverage mesh at the same location
        lever = new MeshInstance3D();
        var leverMeshLib = GD.Load<MeshLibrary>("res://assets/levers.tres");

        var mesh = leverMeshLib.GetItemMesh(0);
        if (mesh != null)
        {
            lever.Mesh = mesh.Duplicate() as Mesh;
        }
        else
        {
            GD.Print("Failed to load lever mesh from:", mesh);
            return;
        }

        lever.Name = $"Lever_{buttonNumber}";
        var correctedRotation = new Basis(new Quaternion(Vector3.Right, 0));
        lever.Transform = new Transform3D(correctedRotation, new Vector3(0, initialYPosition, 0));
        lever.Name = "Lever_" + cellNo.ToString();

        var labelPosition = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.012f));

        AddChild(lever);

        // Create a label
        var label3D = new Label3D();
        label3D.Text = "Button: " + buttonNumber.ToString();
        label3D.Name = "Label_" + buttonNumber.ToString();
        label3D.Transform = labelPosition;
        label3D.PixelSize = 0.0001f;
        label3D.FontSize = 40;
        label3D.OutlineSize = 0;
        label3D.Modulate = Colors.Black;
        AddChild(label3D);

        // Setup audio click sound
        clickSound = new AudioStreamPlayer();
        clickSound.Name = "AudioStreamPlayer_" + buttonNumber.ToString();
        clickSound.Stream = GD.Load<AudioStream>("res://assets/General_Button_2_User_Interface_Tap_FX_Sound.ogg");
        AddChild(clickSound);

        // Setup the button in the global Dict
        var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
        buttonStatesAutoload.UpdateButtonState(buttonNumber, false);
    }

    public override void _Process(double delta)
    {
        if (trackedBody != null && !active)
        {
            var globalPosition = trackedBody.GlobalTransform.Origin;
            var localPosition = ToLocal(globalPosition);

            var movementDistance = lastYPosition - localPosition.Y;

            var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
            var buttonState = buttonStatesAutoload.GetValue(buttonNumber);

            if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
            {
                UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

                if (localPosition.Y < 0.003 && !active)
                {
                    active = true;
                    clickSound.Play();
                    buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(true));
                }
            }

            lastYPosition = localPosition.Y;
        }
    }

    private void ResetButtonPlate()
    {
        var buttonPlateTransform = lever.Transform;
        buttonPlateTransform.Origin = new Vector3(buttonPlateTransform.Origin.X, initialYPosition, buttonPlateTransform.Origin.Z);
        lever.Transform = buttonPlateTransform;
    }

    private void UpdateButtonPlatePosition(float yPosition)
    {
        var buttonPlateTransform = lever.Transform;
        buttonPlateTransform.Origin = new Vector3(buttonPlateTransform.Origin.X, initialYPosition + yPosition - 0.007f, buttonPlateTransform.Origin.Z);
        lever.Transform = buttonPlateTransform;
    }

    private void OnBodyEntered(Node3D body)
    {
        //GD.Print(body.Name + " Entered");
        trackedBody = body;
    }

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody == body)
        {
            //GD.Print(body.Name + " Exited");
            trackedBody = null;
            active = false;
            var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(false));
            ResetButtonPlate();
        }
    }
}