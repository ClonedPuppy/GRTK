using Godot;

[Tool]
public partial class StepSlider : Area3D
{
    private Node3D trackedBody = null;
    private MeshInstance3D sliderPlane;
    private ShaderMaterial sliderMaterial;
    private Label3D label3D;

    private bool isRuntime;
    private int buttonNumber;
    private const float sliderMin = -0.062f;
    private const float sliderMax = 0.028f;
    private int direction = 0;
    private double lastUpdateTime = 0;
    private const double UpdateInterval = 0.05; // 50ms interval for updates

    // Constants for slider dimensions
    private const float SliderLength = 0.072f;
    private const float SliderWidth = 0.02f;
    private const float SliderHeight = 0.01f;
    private const int MAX_STEPS = 10;
    private int currentStep = 0;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        isRuntime = !Engine.IsEditorHint();
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

        var buttonPanel = GetParent<GridMap>()?.GetParent<AddButtonFunctions>();
        if (buttonPanel != null && buttonPanel.showLabels)  // Make showLabels field public instead of property
        {
            SetupLabel();
        }

        if (isRuntime)
        {
            GetTree().CallGroup("UIListeners", "OnSliderValueChanged", buttonNumber, 0f);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!isRuntime || trackedBody == null) return;

        var localPosition = ToLocal(trackedBody.GlobalTransform.Origin);
        double currentTime = Time.GetTicksMsec() / 1000.0;

        if (currentTime - lastUpdateTime >= UpdateInterval)
        {
            UpdateSliderPosition(localPosition);
            lastUpdateTime = currentTime;
        }
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

    private void SetupLabel()
    {
        var label3D = new Label3D
        {
            Text = $"Slider: {buttonNumber}",
            Name = $"Label_{buttonNumber}",
            Transform = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.02f)),
            PixelSize = 0.0001f,
            FontSize = 40,
            OutlineSize = 0,
            Modulate = Colors.Black
        };
        AddChild(label3D);
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

    private void UpdateSliderPosition(Vector3 localPosition)
    {
        float rawPosition = 0f;
        switch (direction)
        {
            case 0:
                rawPosition = Mathf.Lerp(1f, 0f, (localPosition.Z - sliderMin) / (sliderMax - sliderMin));
                break;
            case 1:
                rawPosition = Mathf.Lerp(1f, 0f, (-localPosition.Z - sliderMin) / (sliderMax - sliderMin));
                break;
            case 2:
                rawPosition = Mathf.Lerp(1f, 0f, (localPosition.X - sliderMin) / (sliderMax - sliderMin));
                break;
            case 3:
                rawPosition = Mathf.Lerp(1f, 0f, (-localPosition.X - sliderMin) / (sliderMax - sliderMin));
                break;
        }

        // Convert raw position to steps
        rawPosition = Mathf.Clamp(rawPosition, 0f, 1f);
        int newStep = Mathf.RoundToInt(rawPosition * MAX_STEPS);

        if (newStep != currentStep) // Only update if step changed
        {
            currentStep = newStep;
            currentStep = Mathf.RoundToInt(Mathf.Clamp(rawPosition, 0f, 1f) * MAX_STEPS);
            float fillAmount = (float)currentStep / MAX_STEPS;
            sliderMaterial.SetShaderParameter("fill_amount", fillAmount);
            GetTree().CallGroup("UIListeners", "OnSliderValueChanged", buttonNumber, fillAmount);
        }
    }

    private void OnBodyEntered(Node3D body) => trackedBody = body;

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody != body) return;
        trackedBody = null;
    }
}