using Coursework.Framework.Components.Workers.Common;

namespace Coursework.Framework.Components.Workers.Concrete;

internal sealed class StubConstantWorker : IWorker
{
    internal StubConstantWorker(float delay)
    {
        this.Delay = delay;
    }

    public float Delay { get; private init; }
}
