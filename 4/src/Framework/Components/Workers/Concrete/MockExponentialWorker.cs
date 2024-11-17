using System;
using LabWork4.Framework.Components.Workers.Common;

namespace LabWork4.Framework.Components.Workers.Concrete;

internal sealed class MockExponentialWorker : IMockWorker
{
    private readonly Random random;
    private readonly float timeMean;

    internal MockExponentialWorker(float timeMean)
    {
        this.timeMean = timeMean;
        this.random = new Random();
    }

    public float DelayPayload
    {
        get
        {
            float number = this.random.NextSingle();
            number = (number != 0 ? number : Single.Epsilon);
            return -1 * this.timeMean * MathF.Log(number);
        }
    }
}