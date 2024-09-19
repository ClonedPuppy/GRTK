using Godot;
using System;

public partial class SmoothOpenXRHand : Node
{
    [Export] public XROrigin3D XrOrigin { get; set; }

    [ExportGroup("Skeletons")]
    [Export] public Skeleton3D SourceSkeleton { get; set; }
    [Export] public Skeleton3D DestinationSkeleton { get; set; }

    [ExportGroup("Nodes")]
    [Export] public Node3D SourceNode { get; set; }
    [Export] public Node3D DestinationNode { get; set; }

    [ExportGroup("Filter Parameters")]
    [Export] public float AllowedJitter { get; set; } = 1; // fcmin (cutoff), decrease to reduce jitter
    [Export] public float LagReduction { get; set; } = 5; // beta, increase to reduce lag

    private OneEuroFilter xFilter;
    private OneEuroFilter yFilter;
    private OneEuroFilter zFilter;

    public override void _Ready()
    {
        var args = new Godot.Collections.Dictionary
        {
            { "cutoff", Variant.CreateFrom(AllowedJitter) },
            { "beta", Variant.CreateFrom(LagReduction) }
        };

        xFilter = new OneEuroFilter(args);
        yFilter = new OneEuroFilter(args);
        zFilter = new OneEuroFilter(args);
    }

    public override void _Process(double delta)
    {
        if (SourceSkeleton != null && DestinationSkeleton != null)
        {
            Vector3 origin = SourceSkeleton.GlobalTransform.Origin - XrOrigin.GlobalTransform.Origin;
            float x = xFilter.Filter(origin.X, (float)delta);
            float y = yFilter.Filter(origin.Y, (float)delta);
            float z = zFilter.Filter(origin.Z, (float)delta);

            DestinationSkeleton.GlobalTransform = new Transform3D(
                SourceSkeleton.GlobalTransform.Basis,
                XrOrigin.GlobalTransform.Origin + new Vector3(x, y, z)
            );

            for (int boneId = 0; boneId < SourceSkeleton.GetBoneCount(); boneId++)
            {
                DestinationSkeleton.SetBonePosePosition(boneId, SourceSkeleton.GetBonePosePosition(boneId));
                DestinationSkeleton.SetBonePoseRotation(boneId, SourceSkeleton.GetBonePoseRotation(boneId));
            }
        }

        if (SourceNode != null && DestinationNode != null)
        {
            Vector3 origin = SourceNode.GlobalTransform.Origin - XrOrigin.GlobalTransform.Origin;
            float x = xFilter.Filter(origin.X, (float)delta);
            float y = yFilter.Filter(origin.Y, (float)delta);
            float z = zFilter.Filter(origin.Z, (float)delta);

            DestinationNode.GlobalTransform = new Transform3D(
                SourceNode.GlobalTransform.Basis,
                XrOrigin.GlobalTransform.Origin + new Vector3(x, y, z)
            );
        }
    }
}