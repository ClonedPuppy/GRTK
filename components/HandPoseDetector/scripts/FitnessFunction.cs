using Godot;
using System;

[Tool]
[GlobalClass]
public partial class FitnessFunction : Resource
{
    /// <summary>
    /// Fitness Function Resource
    /// 
    /// This resource defines a fitness function which returns values in the range
    /// 0..1 in response to an input measurement.
    /// </summary>

    // Enum to define the types of fitness functions available
    public enum Type
    {
        Smoothstep, // Smoothstep response: Smooth transition between two values
        Range       // Range response: More complex response with min, max, and transition ranges
    }

    // Property to get or set the function type
    public Type FunctionType
    {
        get => _type;
        set => _SetType(value);
    }
    private Type _type = Type.Smoothstep; // Default to Smoothstep type

    // Properties for the Range type function
    public float Min { get; set; } // Minimum value of the range
    public float From { get; set; } // Start of the "perfect" range
    public float To { get; set; } // End of the "perfect" range
    public float Max { get; set; } // Maximum value of the range

    // Override the _Set method to handle setting the FunctionType property
    public override bool _Set(StringName property, Variant value)
    {
        if (property == "FunctionType")
        {
            _SetType((Type)value.As<int>()); // Convert the Variant to int, then to Type enum
            return true; // Indicate that we handled this property
        }
        return base._Set(property, value); // For other properties, use the base implementation
    }

    // Override _GetPropertyList to dynamically define properties based on the function type
    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
    {
        var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        // Add the FunctionType property
        properties.Add(new Godot.Collections.Dictionary
        {
            { "name", "FunctionType" },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.Enum },
            { "hint_string", "Smoothstep,Range" }
        });

        // Add From and To properties for both function types
        properties.Add(CreateFloatProperty("From"));
        properties.Add(CreateFloatProperty("To"));

        // Add Min and Max properties only for the Range function type
        if (_type == Type.Range)
        {
            properties.Add(CreateFloatProperty("Min"));
            properties.Add(CreateFloatProperty("Max"));
        }

        return properties;
    }

    // Helper method to create a property dictionary for float properties
    private Godot.Collections.Dictionary CreateFloatProperty(string name)
    {
        return new Godot.Collections.Dictionary
        {
            { "name", name },
            { "type", (int)Variant.Type.Float },
            { "usage", (int)PropertyUsageFlags.Default }
        };
    }

    // Calculate the fitness value based on the input and function type
    public float Calculate(float input)
    {
        return _type switch
        {
            Type.Smoothstep => Mathf.SmoothStep(From, To, input),
            Type.Range => CalculateRange(input),
            _ => 0.0f // Default case, should never happen
        };
    }

    // Get warnings about potentially invalid configurations
    public Godot.Collections.Array<string> GetWarnings()
    {
        var warnings = new Godot.Collections.Array<string>();

        switch (_type)
        {
            case Type.Smoothstep:
                if (From == To)
                    warnings.Add("Smoothstep Function: from == to");
                break;
            case Type.Range:
                if (Min >= From)
                    warnings.Add("Range Function: min >= from");
                if (From >= To)
                    warnings.Add("Range Function: from >= to");
                if (To >= Max)
                    warnings.Add("Range Function: to >= max");
                break;
            default:
                warnings.Add("Unknown Function Type");
                break;
        }

        return warnings;
    }

    // Calculate the fitness value for the Range function type
    private float CalculateRange(float input)
    {
        if (input < Min)
            return 0.0f; // Below Min, fitness is 0
        if (input < From)
            return Mathf.SmoothStep(Min, From, input); // Transition from Min to From
        if (input < To)
            return 1.0f; // Between From and To, fitness is perfect (1)
        if (input < Max)
            return Mathf.SmoothStep(Max, To, input); // Transition from To to Max
        return 0.0f; // Above Max, fitness is 0
    }

    // Set the function type and notify that the property list has changed
    private void _SetType(Type pType)
    {
        _type = pType;
        NotifyPropertyListChanged(); // This will cause the editor to refresh the property list
    }
}