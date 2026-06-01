using UrbanEvacuationSimulator.Core.Constants;

namespace UrbanEvacuationSimulator.Core.GraphStructures.Structures;

public class Edge
{
    public readonly int Id;
    public Node Source { get; set; }
    public Node Target { get; set; }

    public double Length { get; set; }
    public double Capacity { get; set; }
    public double CurrentWeight { get; set; }
    public int ActiveAgentsCount { get; set; }
    public int DeadVehiclesCount { get; set; }

    public double MaxUtilization { get; set; } = 0;
    public int CongestionDurationTicks { get; set; } = 0;
    public int TotalAgentsPassed { get; set; } = 0;

    public Edge(int id, Node source, Node target, double length = 0, double capacity = 0)
    {
        Id = id;
        Source = source;
        Target = target;
        Length = length;
        Capacity = capacity;
        CurrentWeight = length;
    }

    public void UpdateWeight()
    {
        //5.0 - average space occupied by one agent on the edge, this is an arbitrary value that can be adjusted based on the desired level of congestion sensitivity
        double effectiveCapacity = Math.Max(1.0, Length * Capacity / 5.0);
        
        double totalOccupancy = ActiveAgentsCount + DeadVehiclesCount; 
        double density = totalOccupancy / effectiveCapacity;

        if (density > MaxUtilization) MaxUtilization = density;

        if (density > SimulationConstants.CONGESTION_THRESHOLD) CongestionDurationTicks++;
        
        double deadVehiclePenalty = DeadVehiclesCount *
            (Length * SimulationConstants.DEAD_VEHICLE_PENALTY_MULTIPLIER); 

        CurrentWeight = Length * (1.0 + density * SimulationConstants.DENSITY_CONGESTION_WEIGHT_MULTIPLIER) + deadVehiclePenalty;
    }

    public double GetSpeedFactor()
    {
        double effectiveCapacity = Math.Max(1.0, Length * Capacity / SimulationConstants.DEFAULT_CAR_LENGTH_METRES);
        double totalOccupancy = ActiveAgentsCount + DeadVehiclesCount; 
        double density = totalOccupancy / effectiveCapacity;
        
        return 1.0 / (1.0 + density * 9.0); 
    }
}