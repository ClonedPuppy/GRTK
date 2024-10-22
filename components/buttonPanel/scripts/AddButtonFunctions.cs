using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class AddButtonFunctions : Node3D
{
    [Export]
    public Dictionary<int, string> labelNames;
    private GridMap[] layoutInstances;
    private int currentLayoutIndex = -1;
    private MeshLibrary uiMeshes;
    private Vector3 centerTile = new Vector3(0.02f, 0.005f, 0.02f);
    private double lastSwitchTime = 0;
    private const double SwitchDebounceTime = 0.5f; // 500ms debounce time
    public override void _Ready()
    {
        labelNames = new Dictionary<int, string>();

        layoutInstances = GetChildren().OfType<GridMap>().ToArray();
        uiMeshes = layoutInstances[0].MeshLibrary;
        var gmIndex = 0;
        var btnNumber = 0;

        foreach (var gridMapInst in layoutInstances)
        {
            if (uiMeshes != null)
            {
                var usedCells = gridMapInst.GetUsedCells();
                foreach (Vector3I cell in usedCells)
                {
                    int itemId = gridMapInst.GetCellItem(cell);
                    int cellOrientation = gridMapInst.GetCellItemOrientation(cell);
                    if (itemId != -1)
                    {
                        string itemName = uiMeshes.GetItemName(itemId);
                        if (itemName == "ToggleButton")
                        {
                            btnNumber++;
                            SetupToggleButton(cell, cellOrientation, gmIndex, btnNumber);
                        }
                        else if (itemName == "MomentaryButton")
                        {
                            btnNumber++;
                            SetupMomentaryButton(cell, gmIndex, btnNumber);
                        }
                        else if (itemName == "TabButton")
                        {
                            btnNumber++;
                            SetupTabButton(cell, gmIndex, btnNumber);
                        }
                        else if (itemName == "RectButton")
                        {
                            btnNumber++;
                            SetupRectButton(cell, cellOrientation, gmIndex, btnNumber);
                        }
                        else if (itemName == "Slider")
                        {
                            btnNumber++;
                            SetupSlider(cell, cellOrientation, gmIndex, btnNumber);
                        }
                    }
                }
                gmIndex++;
            }
        }

        // Hide all layouts except the first one (if any exist)
        layoutInstances[0].Visible = true;
        for (int i = 1; i < layoutInstances.Length; i++)
        {
            layoutInstances[i].Visible = false;
        }

        // Set the current layout to the first one if it exists
        if (layoutInstances.Length > 0)
        {
            currentLayoutIndex = 0;
        }
        InitializeActiveLayout();
    }

    private void SetupToggleButton(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var toggleButton = new ToggleButton();
        layoutInstances[index].AddChild(toggleButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        toggleButton.Position = position;
        toggleButton.Initialize(btnNumber, cellOrientation);
    }

    private void SetupMomentaryButton(Vector3I cell, int index, int btnNumber)
    {
        var momentaryButton = new MomentaryButton();
        layoutInstances[index].AddChild(momentaryButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        momentaryButton.Position = position;
        momentaryButton.Initialize(btnNumber);
    }

    private void SetupTabButton(Vector3I cell, int index, int btnNumber)
    {
        var tabButton = new TabButton();
        layoutInstances[index].AddChild(tabButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        tabButton.Position = position;
        tabButton.Initialize(btnNumber);
    }

    private void SetupRectButton(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var rectButton = new RectButton();
        layoutInstances[index].AddChild(rectButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        rectButton.Position = position;
        rectButton.Initialize(btnNumber, cellOrientation);
    }

    private void SetupSlider(Vector3I cell, int cellOrientation, int index, int btnNumber)
    {
        var slider = new Slider();
        layoutInstances[index].AddChild(slider);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        slider.Position = position;
        slider.Initialize(btnNumber, cellOrientation);
    }

    private void SetGridMapProcessing(GridMap gridMap, bool enable)
    {
        foreach (Node child in gridMap.GetChildren())
        {
            if (child is Area3D area)
            {
                area.SetPhysicsProcess(enable);
                area.SetProcessInput(enable);
                area.Monitoring = enable;
                area.Monitorable = enable;
            }
        }
    }

    public void SwitchToLayout(int index)
    {
        if (index >= 1 && index < layoutInstances.Length)  // Start from index 1
        {
            if (currentLayoutIndex >= 1)  // Only hide if it's not gridmap[0]
            {
                layoutInstances[currentLayoutIndex].Visible = false;
                SetGridMapProcessing(layoutInstances[currentLayoutIndex], false);
            }

            layoutInstances[index].Visible = true;
            SetGridMapProcessing(layoutInstances[index], true);
            currentLayoutIndex = index;
        }
    }

    public void SwitchToSpecificLayout(int buttonNumber)
    {
        // Since button numbers start at 1 and correlate directly to gridmap indices
        // Check if the buttonNumber corresponds to a valid gridmap (excluding gridmap[0])
        if (buttonNumber < layoutInstances.Length)
        {
            SwitchToLayout(buttonNumber);
        }
        // If button number is higher than available gridmaps, do nothing
    }

    public void InitializeActiveLayout()
    {
        if (layoutInstances.Length > 0)
        {
            // Keep gridmap[0] always active
            SetGridMapProcessing(layoutInstances[0], true);
            layoutInstances[0].Visible = true;

            // Start from index 1, set initial state
            if (layoutInstances.Length > 1)
            {
                currentLayoutIndex = 1;
                layoutInstances[1].Visible = true;
                SetGridMapProcessing(layoutInstances[1], true);

                // Hide and disable all other gridmaps
                for (int i = 2; i < layoutInstances.Length; i++)
                {
                    layoutInstances[i].Visible = false;
                    SetGridMapProcessing(layoutInstances[i], false);
                }
            }
        }
    }

    public void SwitchToNextLayout()
    {
        double currentTime = Time.GetTicksMsec() / 1000.0;
        if (currentTime - lastSwitchTime < SwitchDebounceTime)
        {
            return;
        }

        // Only switch between gridmaps 1 and above
        int nextIndex = currentLayoutIndex + 1;
        if (nextIndex >= layoutInstances.Length || nextIndex < 1)
        {
            nextIndex = 1;  // Reset to gridmap[1]
        }

        SwitchToLayout(nextIndex);
        lastSwitchTime = currentTime;
    }

    public int GetLayoutCount()
    {
        return layoutInstances.Length;
    }
}
