using Godot;
using System.Collections.Generic;

public partial class ButtonStatesAutoload : Node
{
    // Dictionary to store all the values and states
    public Dictionary<int, Variant> StateDict { get; private set; } = new Dictionary<int, Variant>();
    private Texture2D sdfAtlas;
    private Material sdfMaterial;
    private Godot.Collections.Dictionary fontData = new Godot.Collections.Dictionary();
    private int atlasWidth;
    private int atlasHeight;

    public override void _Ready()
    {
        // sdfAtlas = GD.Load<Texture2D>("res://components/buttonPanel/assets/sdf/sdf_font.png");
        // sdfMaterial = GD.Load<Material>("res://components/buttonPanel/assets/materials/sdf_label_material.tres");
        // atlasWidth = sdfAtlas.GetWidth();
        // atlasHeight = sdfAtlas.GetHeight();
        // LoadFontData("res://components/buttonPanel/assets/sdf/sdf_font.json");
    }

    public void SetValue(int key, Variant value)
    {
        StateDict[key] = value;
    }

    public Variant GetValue(int key)
    {
        return StateDict.TryGetValue(key, out Variant value) ? value : new Variant();
    }

    public void UpdateButtonState(int buttonId, Variant newState)
    {
        StateDict[buttonId] = newState;
    }

    // private void LoadFontData(string path)
    // {
    //     var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
    //     if (file != null)
    //     {
    //         string content = file.GetAsText();
    //         fontData = Json.ParseString(content).AsGodotDictionary();
    //         file.Close();
    //     }
    //     else
    //     {
    //         GD.Print("Failed to load font data.");
    //     }
    // }
}