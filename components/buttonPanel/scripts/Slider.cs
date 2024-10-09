using Godot;
using System;

[Tool]
public partial class Slider : Area3D
{
    private int buttonNumber;
    private Node3D trackedBody = null;
    private bool active = false;
    private MeshInstance3D sliderPlane;
    private ShaderMaterial sliderMaterial;
    private const float sliderMin = -0.062f;
    private const float sliderMax = 0.028f;
    private float lastFillAmount = 0f;
    private ButtonStatesAutoload buttonStatesAutoload;
    private bool isRuntime;
    private Label3D label3D;
    private int direction = 0;

    // Constants for slider dimensions
    private const float SliderLength = 0.072f;
    private const float SliderWidth = 0.02f;
    private const float SliderHeight = 0.01f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        isRuntime = !Engine.IsEditorHint();
        if (isRuntime)
        {
            buttonStatesAutoload = GetNode<ButtonStatesAutoload>("/root/ButtonStatesAutoload");
        }
    }

    public void Initialize(int cellNo, int cellOrientation)
    {
        buttonNumber = cellNo;
        Name = $"Slider_{buttonNumber}";
        var shader = GD.Load<Shader>("res://components/buttonPanel/assets/shaders/hBar.gdshader");
        sliderMaterial = new ShaderMaterial
        {
            Shader = shader
        };
        sliderMaterial.SetShaderParameter("fill_amount", 0.0f);
        SetupCollision(cellOrientation);
        SetupSliderPlane(cellOrientation);
        var parent = GetParent();
        if (parent.Get("showLabels").AsBool())
        {
            SetupLabel(cellOrientation);
        }

        if (isRuntime)
        {
            buttonStatesAutoload.UpdateButtonState(buttonNumber, 0f);
        }
    }

    public override void _Process(double delta)
    {
        if (!isRuntime || trackedBody == null || active) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        UpdateSliderPosition(localPosition);
    }

    private void SetupCollision(int cellOrientation)
    {
        var collisionShape = new CollisionShape3D
        {
            Name = $"Collision_{buttonNumber}",
            Shape = new BoxShape3D { Size = new Vector3(SliderWidth, SliderHeight, SliderLength) },
        };

        ApplyOrientation(collisionShape, cellOrientation, true);
        AddChild(collisionShape);
    }

    private void SetupSliderPlane(int cellOrientation)
    {
        sliderPlane = new MeshInstance3D { Name = $"SliderPlane_{buttonNumber}" };
        var leverMeshLib = GD.Load<MeshLibrary>("res://components/buttonPanel/assets/resources/levers.tres");
        var mesh = leverMeshLib.GetItemMesh(3);
        if (mesh == null)
        {
            GD.PrintErr($"Failed to load slider mesh for slider {buttonNumber}");
            return;
        }
        sliderPlane.Mesh = mesh.Duplicate() as Mesh;

        ApplyOrientation(sliderPlane, cellOrientation, false);
        sliderPlane.Mesh.SurfaceSetMaterial(0, sliderMaterial);
        AddChild(sliderPlane);
    }

    private void ApplyOrientation(Node3D node, int cellOrientation, bool isColShape)
    {
        switch (cellOrientation)
        {
            case 16: // 90 degrees
                direction = 2;
                node.RotateY(Mathf.DegToRad(90));
                if (isColShape)
                {
                    node.Position = new Vector3(-0.02f, 0, 0);
                }
                else
                {
                    node.Position = new Vector3(0, -SliderHeight / 2, 0);
                }
                break;
            case 10: // 180 degrees
                direction = 1;
                node.RotateY(Mathf.DegToRad(180));
                if (isColShape)
                {
                    node.Position = new Vector3(0, 0, 0.02f);
                }
                else
                {
                    node.Position = new Vector3(0, -SliderHeight / 2, 0);
                }
                break;
            case 22: // -90 degrees
                direction = 3;
                node.RotateY(Mathf.DegToRad(-90));
                if (isColShape)
                {
                    node.Position = new Vector3(0.02f, 0, 0);
                }
                else
                {
                    node.Position = new Vector3(0, -SliderHeight / 2, 0);
                }

                break;
            default:
                direction = 0;
                if (isColShape)
                {
                    node.Position = new Vector3(0, 0, -0.02f);
                }
                else
                {
                    node.Position = new Vector3(0, -SliderHeight / 2, 0);
                    //GD.Print("Last Pos: " + node.Position.ToString());
                }
                break;
        }
    }

    private void SetupLabel(int cellOrientation)
    {
        var label3D = new Label3D
        {
            Text = $"Slider: {buttonNumber}",
            Name = $"Label_{buttonNumber}",
            PixelSize = 0.0001f,
            FontSize = 40,
            OutlineSize = 0,
            Modulate = Colors.Black
        };

        // Position and rotate the label based on orientation
        Vector3 labelOffset;
        Vector3 labelRotation;

        switch (cellOrientation)
        {
            case 16: // 90 degrees (facing positive X)
                //GD.Print("90");
                labelOffset = new Vector3(SliderLength / 2 + 0.005f, 0.0055f, 0f);
                labelRotation = new Vector3(-90, -90, 0);
                break;
            case 10: // 180 degrees (facing negative Z)
                labelOffset = new Vector3(0f, 0.0055f, -SliderLength / 2 - 0.005f);
                labelRotation = new Vector3(-90, 180, 0);
                break;
            case 22: // -90 degrees (facing negative X)
                //GD.Print("-90");
                labelOffset = new Vector3(-SliderLength / 2 - 0.005f, 0.0055f, 0f);
                labelRotation = new Vector3(-90, 90, 0);
                break;
            default: // 0 degrees (facing positive Z)
                labelOffset = new Vector3(0f, 0.0055f, SliderLength / 2 + 0.005f);
                labelRotation = new Vector3(-90, 0, 0);
                break;
        }

        //Vector3 baseOffset = GetOrientationOffset(cellOrientation);
        label3D.Position = labelOffset;
        label3D.RotationDegrees = labelRotation;

        AddChild(label3D);
    }

    private void UpdateSliderPosition(Vector3 localPosition)
    {
        float fillAmount = 0f;
        switch (direction)
        {
            case 0:
                fillAmount = Mathf.Lerp(1f, 0f, (localPosition.Z - sliderMin) / (sliderMax - sliderMin));
                break;

            case 1:
                fillAmount = Mathf.Lerp(1f, 0f, (-localPosition.Z - sliderMin) / (sliderMax - sliderMin));
                break;

            case 2:
                fillAmount = Mathf.Lerp(1f, 0f, (localPosition.X - sliderMin) / (sliderMax - sliderMin));
                break;

            case 3:
                fillAmount = Mathf.Lerp(1f, 0f, (-localPosition.X - sliderMin) / (sliderMax - sliderMin));
                break;
        }
        fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);

        lastFillAmount = fillAmount;
        sliderMaterial.SetShaderParameter("fill_amount", fillAmount);

        if (isRuntime)
        {
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(fillAmount));
        }
    }

    public void SetFillAmount(float amount)
    {
        lastFillAmount = Mathf.Clamp(amount, 0f, 1f);
        sliderMaterial.SetShaderParameter("fill_amount", lastFillAmount);

        if (isRuntime)
        {
            buttonStatesAutoload.SetValue(buttonNumber, Variant.CreateFrom(lastFillAmount));
        }
    }

    private void OnBodyEntered(Node3D body) => trackedBody = body;

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody != body) return;

        trackedBody = null;
        active = false;
        sliderMaterial.SetShaderParameter("fill_amount", lastFillAmount);
    }
}