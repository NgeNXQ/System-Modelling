namespace Coursework.Framework.Common;

internal interface IStatisticsAggregator
{
    public void ResetStatistics();
    public void UpdateStatistics(float deltaTime);
}
