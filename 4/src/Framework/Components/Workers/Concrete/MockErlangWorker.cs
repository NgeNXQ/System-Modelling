using System;
using LabWork4.Framework.Components.Workers.Common;

namespace LabWork4.Framework.Components.Workers.Concrete;

internal sealed class MockErlangWorker : IMockWorker
{
    private readonly Random random;

    private readonly int k;
    private readonly float delayMean;

    internal MockErlangWorker(float delayMean, int k)
    {
        this.k = k;
        this.delayMean = delayMean;
        this.random = new Random();
    }

    public float DelayPayload
    {
        get
        {
            float erlangMultiplier = 1.0f;

            for (int i = 0; i < this.k; ++i)
                erlangMultiplier *= 1 - this.random.NextSingle();

            return -MathF.Log(erlangMultiplier) / (this.k * this.delayMean);
        }
    }
}
