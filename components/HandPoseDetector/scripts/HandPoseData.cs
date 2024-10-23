using Godot;
using System;

[Tool]
public partial class HandPoseData : RefCounted
{
    /// <summary>
    /// Hand Pose Data Object
    ///
    /// This object contains hand pose data converted from a raw XRHandTracker
    /// into a form more suitable for hand-pose analysis.
    /// </summary>

    public enum Finger
    {
        THUMB = 0,   // Thumb
        INDEX = 1,   // Index Finger
        MIDDLE = 2,  // Middle Finger
        RING = 3,    // Ring Finger
        PINKY = 4    // Pinky Finger

    }

    // Hand pose measurements
    public float FlxThumb { get; private set; } = 0.0f;  // Flexion of thumb
    public float FlxIndex { get; private set; } = 0.0f;  // Flexion of index finger
    public float FlxMiddle { get; private set; } = 0.0f; // Flexion of middle finger
    public float FlxRing { get; private set; } = 0.0f;   // Flexion of ring finger
    public float FlxPinky { get; private set; } = 0.0f;  // Flexion of pinky finger
    public float CrlThumb { get; private set; } = 0.0f;  // Curl of thumb
    public float CrlIndex { get; private set; } = 0.0f;  // Curl of index finger
    public float CrlMiddle { get; private set; } = 0.0f; // Curl of middle finger
    public float CrlRing { get; private set; } = 0.0f;   // Curl of ring finger
    public float CrlPinky { get; private set; } = 0.0f;  // Curl of pinky finger
    public float AbdThumb { get; private set; } = 0.0f;  // Abduction from thumb to index finger
    public float AbdIndex { get; private set; } = 0.0f;  // Abduction from index to middle fingers
    public float AbdMiddle { get; private set; } = 0.0f; // Abduction from middle to ring fingers
    public float AbdRing { get; private set; } = 0.0f;   // Abduction from ring to pinky fingers
    public float DstIndex { get; private set; } = 0.0f;  // Distance from thumb to index finger tips
    public float DstMiddle { get; private set; } = 0.0f; // Distance from thumb to middle finger tips
    public float DstRing { get; private set; } = 0.0f;   // Distance from thumb to ring finger tips
    public float DstPinky { get; private set; } = 0.0f;  // Distance from thumb to pinky finger tips

    /// <summary>
    /// Update the hand pose data from an XRHandTracker
    /// </summary>
    public void Update(XRHandTracker hand)
    {
        FlxThumb = Flexion(hand, Finger.THUMB);
        FlxIndex = Flexion(hand, Finger.INDEX);
        FlxMiddle = Flexion(hand, Finger.MIDDLE);
        FlxRing = Flexion(hand, Finger.RING);
        FlxPinky = Flexion(hand, Finger.PINKY);
        CrlThumb = Curl(hand, Finger.THUMB);
        CrlIndex = Curl(hand, Finger.INDEX);
        CrlMiddle = Curl(hand, Finger.MIDDLE);
        CrlRing = Curl(hand, Finger.RING);
        CrlPinky = Curl(hand, Finger.PINKY);
        AbdThumb = Abduction(hand, Finger.THUMB, Finger.INDEX);
        AbdIndex = Abduction(hand, Finger.INDEX, Finger.MIDDLE);
        AbdMiddle = Abduction(hand, Finger.MIDDLE, Finger.RING);
        AbdRing = Abduction(hand, Finger.RING, Finger.PINKY);
        DstIndex = TipDistance(hand, Finger.THUMB, Finger.INDEX);
        DstMiddle = TipDistance(hand, Finger.THUMB, Finger.MIDDLE);
        DstRing = TipDistance(hand, Finger.THUMB, Finger.RING);
        DstPinky = TipDistance(hand, Finger.THUMB, Finger.PINKY);
    }

    /// <summary>
    /// Returns the flexion in degrees of the specified finger on the hand tracker.
    /// </summary>
    private static float Flexion(XRHandTracker hand, Finger finger)
    {
        var palm = hand.GetHandJointTransform(XRHandTracker.HandJoint.Palm);
        var proximal = GetProximal(hand, finger);

        if (finger == Finger.THUMB)
        {
            switch (hand.Hand)
            {
                case XRPositionalTracker.TrackerHand.Left:
                    return AngleTo(proximal.Basis.Y, -palm.Basis.X, -palm.Basis.Y);
                case XRPositionalTracker.TrackerHand.Right:
                    return AngleTo(proximal.Basis.Y, palm.Basis.X, palm.Basis.Y);
                default:
                    return 0.0f;
            }
        }

        return AngleTo(proximal.Basis.Y, palm.Basis.Y, -palm.Basis.X);
    }

    /// <summary>
    /// Returns the curl in degrees of the specified finger on the hand tracker.
    /// </summary>
    private static float Curl(XRHandTracker hand, Finger finger)
    {
        var proximal = GetProximal(hand, finger);
        var distal = GetDistal(hand, finger);

        var returned = AngleTo(proximal.Basis.Y, distal.Basis.Y, proximal.Basis.X);
        //GD.Print(returned);
        return returned;
    }

    /// <summary>
    /// Returns the abduction in degrees between finger1 and finger2 on the hand tracker.
    /// </summary>
    private static float Abduction(XRHandTracker hand, Finger finger1, Finger finger2)
    {
        var proximalA = GetProximal(hand, (Finger)Mathf.Min((int)finger1, (int)finger2));
        var proximalB = GetProximal(hand, (Finger)Mathf.Max((int)finger1, (int)finger2));

        switch (hand.Hand)
        {
            case XRPositionalTracker.TrackerHand.Left:
                return AngleTo(proximalA.Basis.Y, proximalB.Basis.Y, -proximalA.Basis.Z - proximalB.Basis.Z);
            case XRPositionalTracker.TrackerHand.Right:
                return AngleTo(proximalA.Basis.Y, proximalB.Basis.Y, proximalA.Basis.Z + proximalB.Basis.Z);
            default:
                return 0.0f;
        }
    }

    /// <summary>
    /// Returns the distance in millimeters between finger1 tip and finger2 tip on the hand tracker.
    /// </summary>
    private static float TipDistance(XRHandTracker hand, Finger finger1, Finger finger2)
    {
        var tip1 = GetTip(hand, finger1);
        var tip2 = GetTip(hand, finger2);

        return tip1.Origin.DistanceTo(tip2.Origin) * 1000;
    }

    private static Transform3D GetProximal(XRHandTracker hand, Finger finger)
    {
        XRHandTracker.HandJoint joint = finger switch
        {
            Finger.THUMB => XRHandTracker.HandJoint.ThumbPhalanxProximal,
            Finger.INDEX => XRHandTracker.HandJoint.IndexFingerPhalanxProximal,
            Finger.MIDDLE => XRHandTracker.HandJoint.MiddleFingerPhalanxProximal,
            Finger.RING => XRHandTracker.HandJoint.RingFingerPhalanxProximal,
            Finger.PINKY => XRHandTracker.HandJoint.PinkyFingerPhalanxProximal,
            _ => throw new ArgumentOutOfRangeException(nameof(finger))
        };

        return hand.GetHandJointTransform(joint);
    }

    private static Transform3D GetDistal(XRHandTracker hand, Finger finger)
    {
        XRHandTracker.HandJoint joint = finger switch
        {
            Finger.THUMB => XRHandTracker.HandJoint.ThumbPhalanxDistal,
            Finger.INDEX => XRHandTracker.HandJoint.IndexFingerPhalanxDistal,
            Finger.MIDDLE => XRHandTracker.HandJoint.MiddleFingerPhalanxDistal,
            Finger.RING => XRHandTracker.HandJoint.RingFingerPhalanxDistal,
            Finger.PINKY => XRHandTracker.HandJoint.PinkyFingerPhalanxDistal,
            _ => throw new ArgumentOutOfRangeException(nameof(finger))
        };

        return hand.GetHandJointTransform(joint);
    }

    private static Transform3D GetTip(XRHandTracker hand, Finger finger)
    {
        XRHandTracker.HandJoint joint = finger switch
        {
            Finger.THUMB => XRHandTracker.HandJoint.ThumbTip,
            Finger.INDEX => XRHandTracker.HandJoint.IndexFingerTip,
            Finger.MIDDLE => XRHandTracker.HandJoint.MiddleFingerTip,
            Finger.RING => XRHandTracker.HandJoint.RingFingerTip,
            Finger.PINKY => XRHandTracker.HandJoint.PinkyFingerTip,
            _ => throw new ArgumentOutOfRangeException(nameof(finger))
        };

        return hand.GetHandJointTransform(joint);
    }

    /// <summary>
    /// Returns the signed angle between from and to in degrees as observed from the axis vector.
    /// </summary>
    private static float AngleTo(Vector3 from, Vector3 to, Vector3 axis)
    {
        axis = axis.Normalized();
        var from2 = from.Slide(axis).Normalized();
        var to2 = to.Slide(axis).Normalized();

        return Mathf.RadToDeg(from2.SignedAngleTo(to2, axis));
    }
}