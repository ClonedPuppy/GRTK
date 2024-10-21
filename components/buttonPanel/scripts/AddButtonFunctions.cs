using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
public partial class AddButtonFunctions : Node3D
{
    [Export]
    public Dictionary<int, string> labelNames;
    private int buttonNumber = 0;
    private GridMap[] layoutInstances;
    private int currentLayoutIndex = -1;
    private MeshLibrary uiMeshes;
    private Vector3 centerTile = new Vector3(0.02f, 0.005f, 0.02f);
    private double lastSwitchTime = 0;
    private const double SwitchDebounceTime = 1f; // 500ms debounce time
    public override void _Ready()
    {
        labelNames = new Dictionary<int, string>();

        // Find all GridMap children and store them
        layoutInstances = GetChildren().OfType<GridMap>().ToArray();
        uiMeshes = layoutInstances[0].MeshLibrary;
        var gmIndex = 0;

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
                            buttonNumber++;
                            SetupToggleButton(cell, cellOrientation, gmIndex);
                        }
                        if (itemName == "MomentaryButton")
                        {
                            buttonNumber++;
                            SetupMomentaryButton(cell, gmIndex);
                        }
                        if (itemName == "TabButton")
                        {
                            buttonNumber++;
                            SetupTabButton(cell, gmIndex);
                        }
                        if (itemName == "RectButton")
                        {
                            buttonNumber++;
                            SetupRectButton(cell, cellOrientation, gmIndex);
                        }
                        if (itemName == "Slider")
                        {
                            buttonNumber++;
                            SetupSlider(cell, cellOrientation, gmIndex);
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

    private void SetupToggleButton(Vector3I cell, int cellOrientation, int index)
    {
        var toggleButton = new ToggleButton();
        layoutInstances[index].AddChild(toggleButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        toggleButton.Position = position;

        toggleButton.Initialize(buttonNumber, cellOrientation);
    }

    private void SetupMomentaryButton(Vector3I cell, int index)
    {
        var momentaryButton = new MomentaryButton();
        layoutInstances[index].AddChild(momentaryButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        momentaryButton.Position = position;

        momentaryButton.Initialize(buttonNumber);
    }

    private void SetupTabButton(Vector3I cell, int index)
    {
        var tabButton = new TabButton();
        layoutInstances[index].AddChild(tabButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        tabButton.Position = position;

        tabButton.Initialize(buttonNumber);
    }

    private void SetupRectButton(Vector3I cell, int cellOrientation, int index)
    {
        var rectButton = new RectButton();
        layoutInstances[index].AddChild(rectButton);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        rectButton.Position = position;

        rectButton.Initialize(buttonNumber, cellOrientation);
    }

    private void SetupSlider(Vector3I cell, int cellOrientation, int index)
    {
        var slider = new Slider();
        layoutInstances[index].AddChild(slider);

        Vector3 position = cell * layoutInstances[index].CellSize + centerTile;
        slider.Position = position;

        slider.Initialize(buttonNumber, cellOrientation);
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
        if (index >= 0 && index < layoutInstances.Length)
        {
            if (currentLayoutIndex >= 0)
            {
                layoutInstances[currentLayoutIndex].Visible = false;
                SetGridMapProcessing(layoutInstances[currentLayoutIndex], false);
            }

            layoutInstances[index].Visible = true;
            SetGridMapProcessing(layoutInstances[index], true);
            currentLayoutIndex = index;
        }
    }

    public void InitializeActiveLayout()
    {
        if (layoutInstances.Length > 0)
        {
            SetGridMapProcessing(layoutInstances[0], true);
            for (int i = 1; i < layoutInstances.Length; i++)
            {
                SetGridMapProcessing(layoutInstances[i], false);
            }
        }
    }

    public void SwitchToNextLayout()
    {
        double currentTime = Time.GetTicksMsec() / 1000.0;
        if (currentTime - lastSwitchTime < SwitchDebounceTime)
        {
            return; // Ignore rapid successive calls
        }

        int nextIndex = (currentLayoutIndex + 1) % layoutInstances.Length;
        SwitchToLayout(nextIndex);
        lastSwitchTime = currentTime;
    }

    public int GetLayoutCount()
    {
        return layoutInstances.Length;
    }
}
