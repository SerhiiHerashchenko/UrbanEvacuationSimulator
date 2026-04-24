using UrbanEvacuationSimulator.Core.Enums;

namespace UrbanEvacuationSimulator.Core.Interfaces;

public interface IMetricsCollector
{
    void Collect(MetricType metricType, double value);
}