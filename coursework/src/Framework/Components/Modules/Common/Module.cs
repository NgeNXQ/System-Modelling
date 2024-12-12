using System;
using Coursework.Framework.Components.Common;
using Coursework.Framework.Components.Tasks.Common;

namespace Coursework.Framework.Components.Modules.Common;

internal abstract class Module : Element
{
    private protected Module(string identifier) : base(identifier)
    {
        this.TimeNext = Single.MaxValue;
    }

    internal float TimeNext { get; private protected set; }
    internal float TimeCurrent { get; private protected set; }

    internal abstract void UpdateTimeline(float currentTime);
    private protected abstract void MoveTimeline(float deltaTime);

    internal abstract void CompleteTask();
    internal abstract void AcceptTask(DummyTask task);
}
