using Godot;
using System;

[Tool]
[GlobalClass]
public partial class HandPose : Resource
{
    [Export]
    public string PoseName { get; set; }

    [Export(PropertyHint.Range, "0.0,1.0")]
    public float Threshold { get; set; } = 0.5f;

    [Export(PropertyHint.Range, "0.01,1.0")]
    public float HoldTime { get; set; } = 0.2f;

    [Export(PropertyHint.Range, "0.01,1.0")]
    public float ReleaseTime { get; set; } = 0.2f;

    [ExportGroup("Flexion")]
    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction FlexionThumb { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction FlexionIndex { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction FlexionMiddle { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction FlexionRing { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction FlexionPinky { get; set; }

    [ExportGroup("Curl")]
    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction CurlThumb { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction CurlIndex { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction CurlMiddle { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction CurlRing { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction CurlPinky { get; set; }

    [ExportGroup("Abduction")]
    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction AbductionThumbIndex { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction AbductionIndexMiddle { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction AbductionMiddleRing { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction AbductionRingPinky { get; set; }

    [ExportGroup("Tip-Distance")]
    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction DistanceThumbIndex { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction DistanceThumbMiddle { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction DistanceThumbRing { get; set; }

    [Export(PropertyHint.ResourceType, "FitnessFunction")]
    public FitnessFunction DistanceThumbPinky { get; set; }

    public float GetFitness(HandPoseData hand)
    {
        float fitness = 1.0f;

        fitness *= CalculateFitness(FlexionThumb, hand.FlxThumb);
        fitness *= CalculateFitness(FlexionIndex, hand.FlxIndex);
        fitness *= CalculateFitness(FlexionMiddle, hand.FlxMiddle);
        fitness *= CalculateFitness(FlexionRing, hand.FlxRing);
        fitness *= CalculateFitness(FlexionPinky, hand.FlxPinky);

        fitness *= CalculateFitness(CurlThumb, hand.CrlThumb);
        fitness *= CalculateFitness(CurlIndex, hand.CrlIndex);
        fitness *= CalculateFitness(CurlMiddle, hand.CrlMiddle);
        fitness *= CalculateFitness(CurlRing, hand.CrlRing);
        fitness *= CalculateFitness(CurlPinky, hand.CrlPinky);

        fitness *= CalculateFitness(AbductionThumbIndex, hand.AbdThumb);
        fitness *= CalculateFitness(AbductionIndexMiddle, hand.AbdIndex);
        fitness *= CalculateFitness(AbductionMiddleRing, hand.AbdMiddle);
        fitness *= CalculateFitness(AbductionRingPinky, hand.AbdRing);

        fitness *= CalculateFitness(DistanceThumbIndex, hand.DstIndex);
        fitness *= CalculateFitness(DistanceThumbMiddle, hand.DstMiddle);
        fitness *= CalculateFitness(DistanceThumbRing, hand.DstRing);
        fitness *= CalculateFitness(DistanceThumbPinky, hand.DstPinky);

        return fitness < Threshold ? 0.0f : fitness;
    }

    private float CalculateFitness(FitnessFunction function, float value)
    {
        return function != null ? function.Calculate(value) : 1.0f;
    }
}