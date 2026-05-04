using UrbanEvacuationSimulator.Core.Enums;

namespace UrbanEvacuationSimulator.Core.Interfaces;

public interface IMetricsCollector
{
    void CollectSimulationMetric(SimulationMetricType metricType, double value);
    void CollectAgentMetric(AgentMetricType metricType, int tickNumber, double value);
}