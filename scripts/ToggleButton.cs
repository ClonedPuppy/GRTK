using Godot;
using System;

public partial class ToggleButton : Area3D
{
    private int buttonNumber;
    private MeshInstance3D lever;
    private AudioStreamPlayer clickSound;

    public override void _Ready()
    {
        clickSound = new AudioStreamPlayer();
        clickSound.Stream = GD.Load<AudioStream>("res://assets/General_Button_2_User_Interface_Tap_FX_Sound.ogg");
        AddChild(clickSound);
    }

    public void Initialize(int cellNo, int cellOrientation)
    {
        buttonNumber = cellNo;
        Name = $"ToggleButton_{buttonNumber}";
        var leverMesh = GD.Load<MeshLibrary>("res://assets/levers.tres");

        var collisionShape = new CollisionShape3D();
        collisionShape.Shape = new CylinderShape3D { Height = 0.01f, Radius = 0.01f };
        AddChild(collisionShape);

        lever = new MeshInstance3D();
        lever.Mesh = leverMesh.GetItemMesh(1);
        lever.Name = $"Lever_{buttonNumber}";
        AddChild(lever);

        ApplyOrientation(cellOrientation);

        // Connect signals
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void ApplyOrientation(int cellOrientation)
    {
        switch (cellOrientation)
        {
            case 16:
                lever.RotationDegrees = new Vector3(0, 90, 0);
                break;
            case 10:
                lever.RotationDegrees = new Vector3(0, 180, 0);
                break;
            case 22:
                lever.RotationDegrees = new Vector3(0, -90, 0);
                break;
            default:
                lever.RotationDegrees = Vector3.Zero;
                break;
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        // Handle body entered logic
        GD.Print($"Body entered ToggleButton {buttonNumber}");
    }

    private void OnBodyExited(Node3D body)
    {
        // Handle body exited logic
        GD.Print($"Body exited ToggleButton {buttonNumber}");
    }

    // Add other necessary methods for button functionality
}

// using Godot;
// using System;

// public partial class ToggleButton : Area3D
// {
//     private Area3D area;
//     private int buttonNumber;
//     private Node3D trackedBody = null;
//     private bool active = false;
//     private Node3D lever;
//     private float initialYPosition = -0.0025f;
//     private float lastYPosition = 0.0f;
//     private const float MinMovementThreshold = 0.0005f;
//     private const float MaxMovementThreshold = 0.5f;
//     private const float FingerCollisionOffset = 0.0025f;
//     private AudioStreamPlayer clickSound;

//     public void Init(Area3D areaNode, int cellNo, Mesh currentMesh, int cellOrientation)
//     {
//         area = areaNode;
//         buttonNumber = cellNo;

//         BodyEntered += OnArea3DBodyEntered;
//         BodyExited += OnArea3DBodyExited;

//         var collisionShape = new CollisionShape3D();
//         collisionShape.Name = "Collision_" + cellNo;
//         area.AddChild(collisionShape);

//         var shape = new CylinderShape3D();
//         shape.Height = 0.01f;
//         shape.Radius = 0.01f;
//         collisionShape.Shape = shape;

//         var meshInstance = new MeshInstance3D();
//         var meshLibrary = GD.Load<MeshLibrary>("res://assets/levers.tres");
//         var mesh = meshLibrary.GetItemMesh(1);
//         if (mesh != null)
//         {
//             meshInstance.Mesh = mesh.Duplicate() as Mesh;
//         }
//         else
//         {
//             GD.Print("Failed to load lever mesh from:", mesh);
//             return;
//         }

//         var correctedRotation = new Basis(new Quaternion(Vector3.Right, 0));
//         meshInstance.Transform = new Transform3D(correctedRotation, new Vector3(0, initialYPosition, 0));
//         meshInstance.Name = "Lever_" + cellNo;

//         var labelPosition = new Transform3D(Basis.FromEuler(new Vector3(Mathf.DegToRad(-90), 0, 0)), new Vector3(0.0f, 0.0055f, 0.012f));
//         if (cellOrientation != -1)
//         {
//             switch (cellOrientation)
//             {
//                 case 16:
//                     meshInstance.RotationDegrees = new Vector3(0, 90, 0);
//                     labelPosition = new Transform3D(Basis.FromEuler(new Vector3(Mathf.DegToRad(-90), 0, 0)), new Vector3(0.0f, 0.0055f, 0.019f));
//                     break;
//                 case 10:
//                     meshInstance.RotationDegrees = new Vector3(0, 180, 0);
//                     labelPosition = new Transform3D(Basis.FromEuler(new Vector3(Mathf.DegToRad(-90), 0, 0)), new Vector3(0.0f, 0.0055f, 0.012f));
//                     break;
//                 case 22:
//                     meshInstance.RotationDegrees = new Vector3(0, -90, 0);
//                     labelPosition = new Transform3D(Basis.FromEuler(new Vector3(Mathf.DegToRad(-90), 0, 0)), new Vector3(0.0f, 0.0055f, 0.019f));
//                     break;
//                 default:
//                     meshInstance.RotationDegrees = Vector3.Zero;
//                     break;
//             }
//         }

//         area.AddChild(meshInstance);
//         lever = meshInstance;

//         var label3D = new Label3D();
//         label3D.Text = "Button: " + buttonNumber;
//         label3D.Name = "Label_" + buttonNumber;
//         label3D.Transform = labelPosition;
//         label3D.PixelSize = 0.0001f;
//         label3D.FontSize = 40;
//         label3D.OutlineSize = 0;
//         label3D.Modulate = Colors.Black;
//         AddChild(label3D);

//         SetProcess(true);

//         clickSound = new AudioStreamPlayer();
//         clickSound.Name = "AudioStreamPlayer_" + buttonNumber;
//         clickSound.Stream = GD.Load<AudioStream>("res://assets/General_Button_2_User_Interface_Tap_FX_Sound.ogg");
//         AddChild(clickSound);

//         var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
//         buttonStatesAutoload.UpdateButtonState(buttonNumber, Variant.CreateFrom(false));
//     }

//     public override void _Process(double delta)
//     {
//         if (trackedBody != null && !active)
//         {
//             var globalPosition = trackedBody.GlobalTransform.Origin;
//             var localPosition = area.ToLocal(globalPosition);

//             var movementDistance = lastYPosition - localPosition.Y;

//             var buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
//             var buttonState = buttonStatesAutoload.GetValue(buttonNumber);

//             if (localPosition.Y >= 0 && lastYPosition >= 0 && movementDistance >= MinMovementThreshold)
//             {
//                 UpdateButtonPlatePosition(localPosition.Y - FingerCollisionOffset);

//                 if (localPosition.Y < 0.003 && !active)
//                 {
//                     active = true;
//                     clickSound.Play();

//                     // Toggle the button state
//                     bool newState = !buttonState.As<bool>();
//                     buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(newState));
//                 }
//             }

//             lastYPosition = localPosition.Y;
//         }
//     }

//     private void ResetButtonPlate()
//     {
//         var buttonPlateTransform = lever.Transform;
//         buttonPlateTransform.Origin = new Vector3(buttonPlateTransform.Origin.X, initialYPosition, buttonPlateTransform.Origin.Z);
//         lever.Transform = buttonPlateTransform;
//     }

//     private void UpdateButtonPlatePosition(float yPosition)
//     {
//         var buttonPlateTransform = lever.Transform;
//         buttonPlateTransform.Origin = new Vector3(buttonPlateTransform.Origin.X, initialYPosition + yPosition - 0.007f, buttonPlateTransform.Origin.Z);
//         lever.Transform = buttonPlateTransform;
//     }

//     private void OnArea3DBodyEntered(Node3D body)
//     {
//         trackedBody = body;
//     }

//     private void OnArea3DBodyExited(Node3D body)
//     {
//         if (trackedBody == body)
//         {
//             trackedBody = null;
//             active = false;
//             ResetButtonPlate();
//         }
//     }
// }