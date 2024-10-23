using Godot;
using System;
using System.Linq;

[Tool]
public partial class HandPoseController : HandPoseDetector
{
    /// <summary>
    /// Hand Pose Controller Node
    ///
    /// This script creates an XRControllerTracker moved by an associated
    /// HandPoseDetector, and capable of generating XR Input Actions in response
    /// to detected hand poses.
    ///
    /// The XRControllerTracker will have a "default" pose whose position is based
    /// on the tracked hand and the selected pose_type.
    /// </summary>

    public enum PoseType
    {
        Skeleton,   // Skeleton pose (palm pose)
        Aim,        // Aim pose (aiming pose)
        Grip        // Grip pose (gripping pose)
    }

    private static readonly Transform3D[] _POSE_TRANSFORMS_LEFT = new Transform3D[]
    {
        // Skeleton-pose (identity)
        new Transform3D(Basis.Identity, Vector3.Zero),

        // Aim pose - see OpenXR specification
        new Transform3D(new Basis(new Quaternion(0.5f, -0.5f, 0.5f, 0.5f)), new Vector3(-0.05f, 0.11f, 0.035f)),

        // Grip pose - see OpenXR specification
        new Transform3D(new Basis(new Quaternion(0.6408564f, -0.2988362f, 0.6408564f, 0.2988362f)), new Vector3(0.0f, 0.0f, 0.025f))
    };

    private static readonly Transform3D[] _POSE_TRANSFORMS_RIGHT = new Transform3D[]
    {
        // Skeleton-pose (identity)
        new Transform3D(Basis.Identity, Vector3.Zero),

        // Aim pose - see OpenXR specification
        new Transform3D(new Basis(new Quaternion(0.5f, 0.5f, -0.5f, 0.5f)), new Vector3(0.05f, 0.11f, 0.035f)),

        // Grip pose - see OpenXR specification
        new Transform3D(new Basis(new Quaternion(-0.6408564f, -0.2988362f, 0.6408564f, -0.2988362f)), new Vector3(0.0f, 0.0f, 0.025f))
    };

    [ExportGroup("Controller")]
    [Export(PropertyHint.EnumSuggestion, "/user/hand_pose_controller/left,/user/hand_pose_controller/right")]
    public string ControllerTrackerName { get; set; } = "/user/hand_pose_controller/left";

    [Export]
    public PoseType ControllerPoseType { get; set; } = PoseType.Skeleton;

    [Export]
    public HandPoseActionMap ControllerActionMap { get; set; }

    public XRControllerTracker ControllerTracker { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }

    if (HandPoseSet == null && ControllerActionMap != null)
    {
        HandPoseSet = new HandPoseSet();
        HandPoseSet.Poses = new Godot.Collections.Array<HandPose>(
            ControllerActionMap.Actions.Select(action => action.Pose)
        );
    }

        PoseStarted += OnPoseStarted;
        PoseEnded += OnPoseEnded;

        ControllerTracker = new XRControllerTracker();
        ControllerTracker.Name = ControllerTrackerName;
        XRServer.AddTracker(ControllerTracker);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (HandTracker == null || ControllerTracker == null)
            return;

        var pose = HandTracker.GetPose("default");
        if (pose == null)
            return;

        var hand = HandTracker.Hand;

        var convXform = hand == XRPositionalTracker.TrackerHand.Left
            ? _POSE_TRANSFORMS_LEFT[(int)ControllerPoseType]
            : _POSE_TRANSFORMS_RIGHT[(int)ControllerPoseType];

        var poseTransform = pose.Transform * convXform;
        var poseLinear = pose.LinearVelocity * convXform.Basis;
        var poseAngular = RotateAngularVelocity(pose.AngularVelocity, convXform.Basis);

        ControllerTracker.Hand = hand;
        ControllerTracker.SetPose(
            pose.Name,
            poseTransform,
            poseLinear,
            poseAngular,
            pose.TrackingConfidence
        );
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = base._GetConfigurationWarnings().ToList();

        if (string.IsNullOrEmpty(ControllerTrackerName))
        {
            warnings.Add("Controller tracker name not set");
        }

        return warnings.ToArray();
    }

    private void OnPoseStarted(string pName)
    {
        if (ControllerTracker == null || ControllerActionMap == null)
            return;

        var action = ControllerActionMap.GetAction(pName);
        if (action == null)
            return;

        if (action.Type == HandPoseAction.ActionType.Bool)
        {
            ControllerTracker.Set(action.ActionName, true);
        }
        else
        {
            ControllerTracker.Set(action.ActionName, 1.0f);
        }
    }

    private void OnPoseEnded(string pName)
    {
        if (ControllerTracker == null || ControllerActionMap == null)
            return;

        var action = ControllerActionMap.GetAction(pName);
        if (action == null)
            return;

        if (action.Type == HandPoseAction.ActionType.Bool)
        {
            ControllerTracker.Set(action.ActionName, false);
        }
        else
        {
            ControllerTracker.Set(action.ActionName, 0.0f);
        }
    }

    private static Vector3 RotateAngularVelocity(Vector3 vel, Basis basis)
    {
        float len = vel.Length();
        if (Mathf.IsZeroApprox(len))
            return Vector3.Zero;

        vel /= len;

        var velQuat = new Quaternion(vel.X, vel.Y, vel.Z, 0).Normalized();
        velQuat *= basis.GetRotationQuaternion();
        vel = new Vector3(velQuat.X, velQuat.Y, velQuat.Z);

        return vel * len;
    }
}