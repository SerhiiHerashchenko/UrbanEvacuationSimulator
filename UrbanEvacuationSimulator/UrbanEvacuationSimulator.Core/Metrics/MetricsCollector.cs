using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.Interfaces;

namespace UrbanEvacuationSimulator.Core.Metrics;

public class MetricsCollector: IMetricsCollector
{
    private readonly List<(SimulationMetricType Type, double Value)> _simulationMetrics = new();
    private readonly List<(AgentMetricType type, int tickNumber, double value)> _agentMetrics = new();
    //determine if we need to store agent metrics in a different way, maybe we can store them in a list of lists, where each list corresponds to a tick number, and each element in the list corresponds to a metric type, and the value is the metric value for that tick number and metric type
    public void CollectSimulationMetric(SimulationMetricType metricType, double value) =>
        _simulationMetrics.Add((metricType, value));

    public void CollectAgentMetric(AgentMetricType metricType, int tickNumber, double value) =>
        _agentMetrics.Add((metricType, tickNumber, value));

    public List<double> GetMetric(SimulationMetricType metricType) =>
        _simulationMetrics.Where(m => m.Type == metricType).Select(m => m.Value).ToList();

    public void PrintMetricsToConsole()
    {
        Console.WriteLine("----- Simulation Metrics -----");
        foreach (var metric in _simulationMetrics)
        {
            if (metric.Type == SimulationMetricType.SurvivalRate)
            {
                Console.WriteLine($"{metric.Type}: {metric.Value:F2}%");
            }
            else
            {
                Console.WriteLine($"{metric.Type}: {metric.Value}");
            }
        }
        Console.WriteLine("------------------------------");
    }
}