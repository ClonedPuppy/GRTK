using Godot;
using System;

public partial class OneEuroFilter : Node
{
    private float minCutoff;
    private float beta;
    private float dCutoff;
    private LowPassFilter xFilter;
    private LowPassFilter dxFilter;

    public OneEuroFilter(Godot.Collections.Dictionary args)
    {
        minCutoff = args["cutoff"].As<float>();
        beta = args["beta"].As<float>();
        dCutoff = args["cutoff"].As<float>();
        xFilter = new LowPassFilter();
        dxFilter = new LowPassFilter();
    }

    private float Alpha(float rate, float cutoff)
    {
        float tau = 1.0f / (2.0f * Mathf.Pi * cutoff);
        float te = 1.0f / rate;
        return 1.0f / (1.0f + tau / te);
    }

    public float Filter(float value, float delta)
    {
        float rate = 1.0f / delta;
        float dx = (value - xFilter.LastValue) * rate;

        float edx = dxFilter.Filter(dx, Alpha(rate, dCutoff));
        float cutoff = minCutoff + beta * Mathf.Abs(edx);
        return xFilter.Filter(value, Alpha(rate, cutoff));
    }

    private class LowPassFilter
    {
        public float LastValue { get; private set; }

        public LowPassFilter()
        {
            LastValue = 0;
        }

        public float Filter(float value, float alpha)
        {
            float result = alpha * value + (1 - alpha) * LastValue;
            LastValue = result;
            return result;
        }
    }
}