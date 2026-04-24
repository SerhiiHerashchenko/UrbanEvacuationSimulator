using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;

namespace UrbanEvacuationSimulator.Core.Agent;

public class Agent
{
    public int Id { get; set; }
    public Node CurrentNode { get; set; }
    public Node TargetNode { get; set; }
    
    public double Fuel { get; set; }
    public double Speed { get; set; }
    public double FuelConsumptionRate { get; set; } = 1.0;

    public AgentState State { get; set; } = AgentState.Idle;
    public Queue<Edge> CurrentPath { get; set; } = new();
    public Edge? CurrentEdge { get; set; }
    public double DistanceOnCurrentEdge { get; set; }

    public double TotalPassedDistance { get; set; }
    public int TotalNodesPassed { get; set; }

    public Agent(int id, Node startNode, Node targetNode, double fuel, double speed)
    {
        Id = id;
        CurrentNode = startNode;
        TargetNode = targetNode;
        Fuel = fuel;
        Speed = speed;
        TotalPassedDistance = 0;
        TotalNodesPassed = 0;
    }
}