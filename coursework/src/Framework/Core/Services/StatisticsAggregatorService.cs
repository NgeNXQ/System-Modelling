using System.Collections.Generic;

namespace CourseWork.Framework.Core.Services;

internal sealed class StatisticsAggregatorService
{
    private static readonly StatisticsAggregatorService instance;

    private readonly IDictionary<string, Dictionary<string, float>> statistics;

    static StatisticsAggregatorService()
    {
        StatisticsAggregatorService.instance = new StatisticsAggregatorService();
    }

    private StatisticsAggregatorService()
    {
        this.statistics = new Dictionary<string, Dictionary<string, float>>();
    }

    internal static StatisticsAggregatorService Instance => StatisticsAggregatorService.instance;

    internal float GetValue(string moduleIdentifier, string valueIdentifier)
    {
        return 0.0f;
    }

    internal void SetaValue(string moduleIdentifier, string valueIdentifier, float value)
    {

    }
}
