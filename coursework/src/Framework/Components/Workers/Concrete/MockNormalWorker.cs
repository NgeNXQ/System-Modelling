using System;
using CourseWork.Framework.Components.Workers.Common;

namespace CourseWork.Framework.Components.Workers.Concrete;

internal sealed class MockNormalWorker : IWorker
{
    private readonly Random random;
    private readonly float delayMean;
    private readonly float delayDeviation;

    internal MockNormalWorker(float delayMean, float delayDeviation)
    {
        this.delayMean = delayMean;
        this.random = new Random();
        this.delayDeviation = delayDeviation;
    }

    public float Delay
    {
        get => this.delayMean + this.delayDeviation * this.GaussianNumber;
    }

    private float GaussianNumber
    {
        get
        {
            float u1 = 1.0f - this.random.NextSingle();
            float u2 = 1.0f - this.random.NextSingle();
            return MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2);
        }
    }
}
