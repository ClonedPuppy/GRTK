using Godot;
using System;
using System.Linq;

[Tool]
[GlobalClass]
public partial class HandPoseActionMap : Resource
{
    /// <summary>
    /// Hand Pose Action Map Resource
    ///
    /// This resource defines a map of HandPoseAction used to associate hand poses
    /// with XR Input Actions.
    /// </summary>

    private Godot.Collections.Array<HandPoseAction> _actions = new();

    /// <summary>
    /// Array of hand pose actions
    /// </summary>
    [Export]
    public Godot.Collections.Array<HandPoseAction> Actions
    {
        get => _actions;
        set => _actions = value;
    }

    /// <summary>
    /// Returns the associated HandPoseAction
    /// </summary>
    /// <param name="poseName">The name of the pose to find</param>
    /// <returns>The HandPoseAction if found, null otherwise</returns>
    public HandPoseAction GetAction(string poseName)
    {
        return _actions.FirstOrDefault(a => a.Pose.PoseName == poseName);
    }
}