using Godot;
using System;

public partial class SidePanelScript : MeshInstance3D
{
    [Export]
    public bool ChangeParentColorToRed = true;

    public override void _Ready()
    {
        if (ChangeParentColorToRed && GetParent() is MeshInstance3D parent)
        {
            parent.MaterialOverride = new StandardMaterial3D { AlbedoColor = Colors.Red };
        }
    }
}