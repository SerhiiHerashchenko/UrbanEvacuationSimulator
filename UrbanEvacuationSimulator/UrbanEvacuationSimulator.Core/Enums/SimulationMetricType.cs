namespace UrbanEvacuationSimulator.Core.Enums;

public enum SimulationMetricType
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
    T99ClearanceTime,
}
