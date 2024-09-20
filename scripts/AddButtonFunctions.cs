using Godot;
using System;

public partial class AddButtonFunctions : GridMap
{
    private MeshLibrary uiMeshes;
    private int buttonNumber = 0;

    public override void _Ready()
    {
        uiMeshes = MeshLibrary;
        if (uiMeshes != null)
        {
            var usedCells = GetUsedCells();
            foreach (Vector3I cell in usedCells)
            {
                int itemId = GetCellItem(cell);
                int cellOrientation = GetCellItemOrientation(cell);
                if (itemId != -1)  // Check if cell is not empty
                {
                    string itemName = MeshLibrary.GetItemName(itemId);
                    if (itemName == "ToggleButton")
                    {
                        Mesh mesh = MeshLibrary.GetItemMesh(itemId);
                        buttonNumber++;
                        SetupToggleButton(cell, mesh, cellOrientation);
                    }
                    // Handle other button types if needed
                }
            }
        }
    }

    private void SetupToggleButton(Vector3I cell, Mesh mesh, int cellOrientation)
    {
        var toggleButton = new ToggleButton();
        AddChild(toggleButton);

        // Set the button's position
        Vector3 position = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);
        toggleButton.Position = position;

        // Initialize the button
        toggleButton.Initialize(buttonNumber, cellOrientation);

        GD.Print($"ToggleButton {buttonNumber} set up at position {position}");
    }
}

// using Godot;
// using System;

// public partial class AddButtonFunctions : GridMap
// {
//     private MeshLibrary uiMeshes;
//     private int buttonNumber = 0;
//     private Area3D currentArea;

//     public override void _Ready()
//     {
//         uiMeshes = MeshLibrary;
//         if (uiMeshes != null)
//         {
//             var usedCells = GetUsedCells();
//             foreach (Vector3I cell in usedCells)
//             {
//                 int itemId = GetCellItem(cell);
//                 int cellOrientation = GetCellItemOrientation(cell);
//                 if (itemId != -1)  // Check if cell is not empty
//                 {
//                     string itemName = MeshLibrary.GetItemName(itemId);
//                     if (itemName == "MomentaryButton")
//                     {
//                         Mesh mesh = MeshLibrary.GetItemMesh(itemId);
//                         buttonNumber++;
//                         SetupMomentaryButton(cell, mesh);
//                     }
//                     if (itemName == "ToggleButton")
//                     {
//                         Mesh mesh = MeshLibrary.GetItemMesh(itemId);
//                         buttonNumber++;
//                         SetupToggleButton(cell, mesh, cellOrientation);
//                     }
//                 }
//             }
//         }
//     }

//     private void SetupMomentaryButton(Vector3I cell, Mesh mesh)
//     {
//         var area = new Area3D();

//         currentArea = area;
//         area.Name = "MomentaryButton_" + buttonNumber;

//         // Calculate the local transform for the cell
//         Transform3D cellLocalTransform = Transform3D.Identity;
//         cellLocalTransform.Origin = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);

//         // Set the area's local transform relative to the GridMap
//         area.Transform = cellLocalTransform;

//         // Add the area as a child of the GridMap
//         AddChild(area);

//         // Load and assign script to the Area3D node
//         string scriptPath = "res://scripts/MomentaryBtn.cs";
//         Script script = GD.Load<Script>(scriptPath);
//         if (script != null)
//         {
//             area.SetScript(script);
//             if (area.HasMethod("Init"))
//             {
//                 area.Call("Init", area, buttonNumber, mesh);
//             }
//         }
//         else
//         {
//             GD.Print("Failed to attach script:", scriptPath);
//         }
//     }

//     private void SetupToggleButton(Vector3I cell, Mesh mesh, int cellOrientation)
//     {
//         try
//         {
//             GD.Print("Creating Area3D");
//             var area = new Area3D();
//             GD.Print($"Area3D created. IsInstanceValid: {IsInstanceValid(area)}");

//             currentArea = area;
//             area.Name = "ToggleButton_" + buttonNumber;
//             GD.Print($"Area3D named: {area.Name}. IsInstanceValid: {IsInstanceValid(area)}");

//             // Calculate the local transform for the cell's button
//             Transform3D cellLocalTransform = Transform3D.Identity;
//             cellLocalTransform.Origin = cell * CellSize + new Vector3(0.02f, 0.005f, 0.02f);

//             // Set the area's local transform relative to the GridMap
//             area.Transform = cellLocalTransform;
//             GD.Print($"Transform set. IsInstanceValid: {IsInstanceValid(area)}");

//             // Add the area as a child of the GridMap
//             AddChild(area);
//             GD.Print($"Area3D added as child. IsInstanceValid: {IsInstanceValid(area)}");

//             // Load and assign script to the Area3D node
//             string scriptPath = "res://scripts/ToggleButton.cs";
//             GD.Print($"Loading script from: {scriptPath}");
//             Script script = GD.Load<Script>(scriptPath);
//             if (script != null)
//             {
//                 GD.Print("Script loaded successfully");
//                 if (IsInstanceValid(area))
//                 {
//                     area.SetScript(script);
//                     GD.Print($"Script set on Area3D. IsInstanceValid: {IsInstanceValid(area)}");
//                     GD.Print($"Area3D name after setting script: {area.Name}");

//                     if (area.HasMethod("Init"))
//                     {
//                         GD.Print("Calling Init method");
//                         area.Call("Init", area, buttonNumber, mesh, cellOrientation);
//                         GD.Print("Init method called successfully");
//                     }
//                     else
//                     {
//                         GD.PrintErr("Area3D doesn't have Init method");
//                     }
//                 }
//                 else
//                 {
//                     GD.PrintErr("Area3D is no longer valid before setting script");
//                 }
//             }
//             else
//             {
//                 GD.PrintErr($"Failed to load script: {scriptPath}");
//             }
//         }
//         catch (ObjectDisposedException ode)
//         {
//             GD.PrintErr($"ObjectDisposedException: {ode.Message}");
//             GD.PrintErr($"Object Name: {ode.ObjectName}");
//             GD.PrintErr($"Stack Trace: {ode.StackTrace}");
//         }
//         catch (Exception e)
//         {
//             GD.PrintErr($"An error occurred in SetupToggleButton: {e.Message}");
//             GD.PrintErr($"Stack Trace: {e.StackTrace}");
//         }
//     }
// }