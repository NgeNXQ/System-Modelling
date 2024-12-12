using System;
using Coursework.Framework.Common;

namespace Coursework.Framework.Components.Common;

internal abstract class Element : IStatisticsPrinter, IStatisticsAggregator
{
    private protected Element(string identifier)
    {
        if (String.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException($"Invalid value of the {nameof(identifier)}.");

        this.Identifier = identifier;
    }

    internal string Identifier { get; private init; }

    public abstract void PrintStatistics();
    public abstract void ResetStatistics();
    public abstract void UpdateStatistics(float deltaTime);
}
