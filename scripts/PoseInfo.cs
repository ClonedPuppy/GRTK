using Godot;
using System;

[Tool]
public partial class PoseInfo : Label3D
{
    // Diagnostic Text Template
    private const string DIAGNOSTIC_TEXT = 
        "Flex (deg)\n\n" +
        "Thumb: {0}\n" +
        "Index: {1}\n" +
        "Middle: {2}\n" +
        "Ring: {3}\n" +
        "Pinky: {4}";

    // Remove the [Export] attribute
    public string TrackerName { get; set; } = "/user/hand_tracker/left";

    private XRHandTracker tracker;

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
    {
        var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        
        properties.Add(new Godot.Collections.Dictionary
        {
            { "name", "TrackerName" },
            { "type", (int)Variant.Type.String },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.EnumSuggestion },
            { "hint_string", "/user/hand_tracker/left,/user/hand_tracker/right" }
        });

        return properties;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        if (tracker == null || tracker.Name != TrackerName)
        {
            tracker = XRServer.GetTracker(TrackerName) as XRHandTracker;
            if (tracker == null)
                return;
        }

        var data = new HandPoseData();
        data.Update(tracker);

        Text = string.Format(DIAGNOSTIC_TEXT,
            (int)data.FlxThumb,
            (int)data.FlxIndex,
            (int)data.FlxMiddle,
            (int)data.FlxRing,
            (int)data.FlxPinky);
    }
}

// using Godot;
// using System;

// [Tool]
// public partial class FlexInfo : Label3D
// {
//     // Diagnostic Text Template
//     private const string DIAGNOSTIC_TEXT = 
//         "Flex (deg)\n\n" +
//         "Thumb: {0}\n" +
//         "Index: {1}\n" +
//         "Middle: {2}\n" +
//         "Ring: {3}\n" +
//         "Pinky: {4}";

//     [Export(PropertyHint.EnumSuggestion, "/user/hand_tracker/left,/user/hand_tracker/right")]
//     public string TrackerName { get; set; } = "/user/hand_tracker/left";

//     private XRHandTracker tracker;

//     public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
//     {
//         var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        
//         properties.Add(new Godot.Collections.Dictionary
//         {
//             { "name", "TrackerName" },
//             { "type", (int)Variant.Type.String },
//             { "usage", (int)PropertyUsageFlags.Default },
//             { "hint", (int)PropertyHint.EnumSuggestion },
//             { "hint_string", "/user/hand_tracker/left,/user/hand_tracker/right" }
//         });

//         return properties;
//     }

//     public override void _Process(double delta)
//     {
//         if (Engine.IsEditorHint())
//             return;

//         if (tracker == null || tracker.Name != TrackerName)
//         {
//             tracker = XRServer.GetTracker(TrackerName) as XRHandTracker;
//             if (tracker == null)
//                 return;
//         }

//         var data = new HandPoseData();
//         data.Update(tracker);

//         Text = string.Format(DIAGNOSTIC_TEXT,
//             (int)data.FlxThumb,
//             (int)data.FlxIndex,
//             (int)data.FlxMiddle,
//             (int)data.FlxRing,
//             (int)data.FlxPinky);
//     }
// }