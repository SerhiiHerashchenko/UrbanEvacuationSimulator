using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.Interfaces;

namespace UrbanEvacuationSimulator.Core.Metrics;

public class MetricsCollector: IMetricsCollector
{
    private readonly Dictionary<MetricType, double> _metrics = new();

    public void Collect(MetricType metricType, double value)
    {
        _metrics[metricType] = value;
    }

    public double GetMetric(MetricType metricType)
    {
        return _metrics.TryGetValue(metricType, out var value) ? value : 0;
    }

    public void PrintMetricsToConsole()
    {
        Console.WriteLine("----- Simulation Metrics -----");
        foreach (var metric in _metrics)
        {
            Console.WriteLine($"{metric.Key}: {metric.Value}");
        }
        Console.WriteLine("------------------------------");
    }
}