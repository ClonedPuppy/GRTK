using Godot;
using System;

public partial class HBar : Area3D
{
    private int buttonNumber;
    private Node3D trackedBody = null;
    private bool active = false;
    private ShaderMaterial sliderMaterial;
private const float MIN_Z = -0.062f;
private const float MAX_Z = 0.028f;
    private float lastFillAmount = 0f;

    public override void _Ready()
    {
        // Signals emitted at entry and exit
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void Initialize(int cellNo, int cellOrientation, ShaderMaterial _sliderMaterial)
    {
        buttonNumber = cellNo;
        Name = $"Slider_{buttonNumber}";
        sliderMaterial = _sliderMaterial;

        // Add a collision node
        var collisionShape = new CollisionShape3D();
        collisionShape.Name = "Collision_" + cellNo.ToString();
        AddChild(collisionShape);

        collisionShape.Shape = new BoxShape3D { Size = new Vector3(0.02f, 0.01f, 0.08f) };
        collisionShape.Position = new Vector3(0, 0, -0.02f);

        //sliderMaterial.SetShaderParameter("fill_amount", 0.0f); // Start with no fill

        var labelPosition = new Transform3D(new Basis(new Quaternion(Vector3.Right, Mathf.DegToRad(-90))), new Vector3(0.0f, 0.0055f, 0.013f));

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

    }

public override void _Process(double delta)
{
    if (trackedBody != null && !active)
    {
        var globalPosition = trackedBody.GlobalTransform.Origin;
        var localPosition = ToLocal(globalPosition);
        
        float fillAmount = Mathf.Lerp(1f, 0f, (localPosition.Z - MIN_Z) / (MAX_Z - MIN_Z));
        fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);

        lastFillAmount = fillAmount;

        //GD.Print($"HBar {buttonNumber}: Fill Amount = {fillAmount}");

        sliderMaterial.SetShaderParameter("fill_amount", fillAmount);
    }
}

    public void SetFillAmount(float amount)
{
    lastFillAmount = Mathf.Clamp(amount, 0f, 1f);
    sliderMaterial.SetShaderParameter("fill_amount", lastFillAmount);
}

    private void OnBodyEntered(Node3D body)
    {
        trackedBody = body;
    }

    private void OnBodyExited(Node3D body)
    {
        if (trackedBody == body)
        {
            trackedBody = null;
            active = false;
            sliderMaterial.SetShaderParameter("fill_amount", lastFillAmount);
        }
    }
}