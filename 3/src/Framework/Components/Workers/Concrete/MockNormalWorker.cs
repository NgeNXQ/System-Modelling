using System;
using LabWork3.Framework.Components.Workers.Common;

namespace LabWork3.Framework.Components.Workers.Concrete;

internal sealed class MockNormalWorker : IMockWorker
{
    private readonly Random random;
    private readonly float timeMean;
    private readonly float timeDeviation;

    internal MockNormalWorker(float timeMean, float timeDeviation)
    {
        this.timeMean = timeMean;
        this.random = new Random();
        this.timeDeviation = timeDeviation;
    }

    public float DelayPayload
    {
        get => this.timeMean + this.timeDeviation * this.GaussianNumber;
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
