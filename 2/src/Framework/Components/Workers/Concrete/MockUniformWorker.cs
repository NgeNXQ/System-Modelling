using System;
using LabWork2.Framework.Components.Workers.Common;

namespace LabWork2.Framework.Components.Workers.Concrete;

internal sealed class MockUniformWorker : IMockWorker
{
    private readonly Random random;
    private readonly float timeMin;
    private readonly float timeMax;

    internal MockUniformWorker(float timeMin, float timeMax)
    {
        this.timeMin = timeMin;
        this.timeMax = timeMax;
        this.random = new Random();
    }

    public float DelayPayload
    {
        get
        {
            float a = 0;

            while (a == 0)
                a = this.random.NextSingle();

            return this.timeMin + a * (this.timeMax - this.timeMin);
        }
    }
}
