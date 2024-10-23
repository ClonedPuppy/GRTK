using Godot;
using System;

[Tool]
[GlobalClass]
public partial class HandPoseAction : Resource
{
    [Export]
    public HandPose Pose { get; set; }

    [Export]
    public ActionType Type { get; set; }

    [Export]
    public string ActionName { get; set; }

    public enum ActionType
    {
        Bool,
        Float
    }
}