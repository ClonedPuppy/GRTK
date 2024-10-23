using Godot;
using System;

[Tool]
public partial class HandPoseDetector : Node
{
    /// <summary>
    /// Hand Pose Detector Script
    ///
    /// This script checks for hand poses and reports them as events.
    /// </summary>

    [Signal]
    public delegate void PoseStartedEventHandler(string pName);

    [Signal]
    public delegate void PoseEndedEventHandler(string pName);

    [ExportGroup("Hand")]
    [Export(PropertyHint.EnumSuggestion, "/user/hand_tracker/left,/user/hand_tracker/right")]
    public string HandTrackerName { get; set; } = "/user/hand_tracker/left";

    [Export]
    public HandPoseSet HandPoseSet { get; set; }

    public XRHandTracker HandTracker { get; private set; }

    private HandPoseData _currentData = new HandPoseData();
    private HandPose _currentPose;
    private float _currentHold = 0.0f;
    private HandPose _newPose;
    private float _newHold = 0.0f;

    public override void _Ready()
    {
        XRServer.TrackerAdded += OnTrackerChanged;
        XRServer.TrackerUpdated += OnTrackerChanged;
        XRServer.TrackerRemoved += OnTrackerChanged;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        if (HandTracker == null || HandPoseSet == null)
            return;

        var flags = HandTracker.GetHandJointFlags(XRHandTracker.HandJoint.Palm);
        if (!flags.HasFlag(XRHandTracker.HandJointFlags.PositionTracked))
            return;
        if (!flags.HasFlag(XRHandTracker.HandJointFlags.OrientationTracked))
            return;

        var activePos = _currentPose;

        _currentData.Update(HandTracker);
        var pose = HandPoseSet.FindPose(_currentData);

        if (_currentPose != null)
        {
            if (pose == _currentPose)
            {
                _currentHold = 1.0f;
            }
            else
            {
                _currentHold -= (float)delta / _currentPose.ReleaseTime;
                if (_currentHold <= 0.0f)
                {
                    _currentHold = 0.0f;
                    _currentPose = null;
                }
            }
        }

        if (pose != _newPose)
        {
            _newPose = pose;
            _newHold = 0.0f;
        }
        else if (_newPose != null)
        {
            _newHold += (float)delta / _newPose.HoldTime;
            if (_newHold >= 1.0f)
            {
                _newHold = 1.0f;
                if (_currentPose == null)
                {
                    _currentPose = _newPose;
                    _currentHold = 1.0f;
                }
            }
        }

        if (_currentPose != activePos)
        {
            if (activePos != null)
                EmitSignal(SignalName.PoseEnded, activePos.PoseName);

            activePos = _currentPose;
            if (activePos != null)
                EmitSignal(SignalName.PoseStarted, activePos.PoseName);
        }
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (string.IsNullOrEmpty(HandTrackerName))
            warnings.Add("Hand tracker name not set");

        return warnings.ToArray();
    }

    private void OnTrackerChanged(StringName pName, long type)
    {
        if (pName == HandTrackerName)
            HandTracker = XRServer.GetTracker(HandTrackerName) as XRHandTracker;
    }
}