using Godot;
using System;

[Tool]
public partial class RectButton : Area3D
{
    private int buttonNumber;
    private Node3D trackedBody = null;
    private bool active = false;
    private MeshInstance3D lever;
    private float initialYposition = -0.0025f;
    private float lastYposition = 0.0f;
    private const float minMoveThreshold = 0.0005f;
    private const float maxMoveThreshold = 0.5f;
    private const float fingerCollisionOffset = 0.0025f;
    private AudioStreamPlayer clickSound;

    public void Initialize(int cellNo, int cellOrientation) // Start by setting up the button and it's functions
    {
        buttonNumber = cellNo;
        Name = $"RectButton_{buttonNumber}";

        // Signals emitted at entry and exit
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Add a collision node
        var collisionShape = new CollisionShape3D();
        collisionShape.Name = "Collision_" + cellNo.ToString();
        AddChild(collisionShape);

        // Add a shape to the collision node, assuming button size is 0.01 x 0.01
        collisionShape.Shape = new BoxShape3D { Size = new Vector3(0.02f, 0.01f, 0.02f) };

        // Add a MeshInstance3D with the leverage mesh at the same location
        lever = new MeshInstance3D();
        var leverMeshLib = GD.Load<MeshLibrary>("res://components/buttonPanel/assets/resources/levers.tres");

        var mesh = leverMeshLib.GetItemMesh(2);
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
        var correctedRotation = new Quaternion(Vector3.Right, Mathf.DegToRad(0)); // not sure if lever correctly oriented in blender
        lever.Transform = new Transform3D(new Basis(correctedRotation), new Vector3(0, initialYposition, 0));
        lever.Name = "Lever_" + cellNo.ToString();

        var labelPosition = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.014f));
        if (cellOrientation != -1)
        {
            switch (cellOrientation)
            {
                case 16:
                    lever.RotationDegrees = new Vector3(lever.RotationDegrees.X, 90, lever.RotationDegrees.Z);
                    labelPosition = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.014f));
                    break;
                case 10:
                    lever.RotationDegrees = new Vector3(lever.RotationDegrees.X, 180, lever.RotationDegrees.Z);
                    labelPosition = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.014f));
                    break;
                case 22:
                    lever.RotationDegrees = new Vector3(lever.RotationDegrees.X, -90, lever.RotationDegrees.Z);
                    labelPosition = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.014f));
                    break;
                default:
                    lever.RotationDegrees = new Vector3(lever.RotationDegrees.X, 0, lever.RotationDegrees.Z);
                    break;
            }
        }
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
        clickSound.Stream = GD.Load<AudioStream>("res://components/buttonPanel/assets/audio/General_Button_2_User_Interface_Tap_FX_Sound.ogg");
        AddChild(clickSound);

        // Setup the button in the global Dict
        var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
        buttonStatesAutoload.UpdateButtonState(buttonNumber, false);
    }

    public override void _Process(double delta)
    {
        if (trackedBody != null && !active)
        {
            Vector3 globalPosition = trackedBody.GlobalTransform.Origin;
            Vector3 localPosition = ToLocal(globalPosition);

            float movementDistance = lastYposition - localPosition.Y;

            // Get the current state of the button
            var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
            bool buttonState = buttonStatesAutoload.GetValue(buttonNumber).AsBool();

            // Make sure we are pressing the button from a top - down direction
            if (localPosition.Y >= 0 && lastYposition >= 0 && movementDistance >= minMoveThreshold)
            {
                // Update the lever position as the "finger tip" is moving in the Area3D
                UpdateButtonPlatePosition(localPosition.Y - fingerCollisionOffset);

                // Check if the "finger tip" is far enough down the Area3D volume to trigger the button as pressed 
                if (localPosition.Y < 0.003 && !active)
                {
                    active = true;
                    clickSound.Play();

                    // Toggle the button state
                    buttonState = !buttonState;

                    // Change the resting height based on the button state
                    //initialYposition = buttonState ? -0.005f : -0.0025f;
                    buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(buttonState));
                    // ResetButtonPlate();
                }
            }
            // Update the last y position
            lastYposition = localPosition.Y;
        }
    }

    private void ResetButtonPlate() // Resets the lever mesh to its original height
    {
        Transform3D buttonPlateTransform = lever.Transform;
        buttonPlateTransform.Origin = new Vector3(
            buttonPlateTransform.Origin.X,
            initialYposition,
            buttonPlateTransform.Origin.Z
        );
        lever.Transform = buttonPlateTransform;
    }

    private void UpdateButtonPlatePosition(float yPosition) // Transforms the lever's height in accordance with where the finger tip is in the Area3D
    {
        Transform3D buttonPlateTransform = lever.Transform;
        buttonPlateTransform.Origin = new Vector3(
            buttonPlateTransform.Origin.X,
            initialYposition + yPosition - 0.007f,
            buttonPlateTransform.Origin.Z
        );
        lever.Transform = buttonPlateTransform;
    }

    private void OnBodyEntered(Node3D body)
    {
        // Handle body entered logic
        trackedBody = body;
    }

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody == body)
        {
            trackedBody = null;
            active = false;
            ResetButtonPlate();
        }
    }
}