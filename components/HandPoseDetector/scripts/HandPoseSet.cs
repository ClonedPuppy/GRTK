using Godot;
using System;
using System.Linq;

[Tool]
[GlobalClass]
public partial class HandPoseSet : Resource
{
    /// <summary>
    /// Hand Pose Set Resource
    ///
    /// This resource defines a set of hand poses. The hand pose detector takes
    /// a pose set and searches for poses within the set.
    /// </summary>

    private Godot.Collections.Array<HandPose> _poses = new();

    /// <summary>
    /// Array of hand poses
    /// </summary>
    [Export]
    public Godot.Collections.Array<HandPose> Poses
    {
        get => _poses;
        set => _poses = value;
    }

    /// <summary>
    /// Returns the best pose for the specified hand.
    /// </summary>
    /// <param name="hand">The hand pose data to evaluate</param>
    /// <returns>The best matching HandPose, or null if no pose matches</returns>
    public HandPose FindPose(HandPoseData hand)
    {
        return _poses
            .Select(p => new { Pose = p, Fitness = p.GetFitness(hand) })
            .OrderByDescending(x => x.Fitness)
            .FirstOrDefault(x => x.Fitness > 0)?.Pose;
    }
}