namespace UrbanEvacuationSimulator.Core.Enums;

public enum MetricType
{
    //General metrics
    RunId,
    TotalAgents,
    TotalSimulationTimeSpentMilliseconds,
    TotalTicks,

    //Count metrics
    EvacuatedCount,
    DeadVehicleCount,
    PathfindingFailureCount,
    SurvivalRate,

    //Average metrics
    AverageEvacuatedDistance,
    AverageDeadVehicleDistance,
    AverageEvacuatedNodesPassed,
    AverageDeadVehicleNodesPassed,
    AverageEvacuatedEdgeLength,
    AverageDeadVehicleEdgeLength
}
