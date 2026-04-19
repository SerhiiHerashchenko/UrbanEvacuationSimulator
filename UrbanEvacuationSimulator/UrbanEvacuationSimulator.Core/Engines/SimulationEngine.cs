using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.GraphStructures;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;
using UrbanEvacuationSimulator.Core.Interfaces;


namespace UrbanEvacuationSimulator.Core.Engines;

public class SimulationEngine : ISimulationEngine
{
    private readonly Graph _graph;
    private readonly IReadOnlyList<Agent.Agent> _agents;
    private readonly IPathFinder _pathFinder;
    private readonly Func<Node, Node, double> _heuristic;
    private readonly List<Agent.Agent> _activeAgents;
    
    public int CurrentTick { get; private set; }

    public SimulationEngine(Graph graph, IReadOnlyList<Agent.Agent> agents, IPathFinder pathFinder)
    {
        _graph = graph;
        _agents = agents;
        _pathFinder = pathFinder;
        _heuristic = (start, target) => start.GetDistance(target);
        _activeAgents = agents.ToList();
        CurrentTick = 0;
    }

    public void Tick()
    {
        CurrentTick++;

        for (int i = _activeAgents.Count - 1; i >= 0; i--)
        {
            var agent = _activeAgents[i];

            if (agent.State == AgentState.Idle && agent.CurrentPath.Count == 0)
            {
                var path = _pathFinder.FindPath(_graph, agent.CurrentNode, agent.TargetNode, _heuristic);
                if (path.Count > 0)
                {
                    agent.CurrentPath = new Queue<Edge>(path);
                    agent.State = AgentState.Moving;
                }
                else
                {
                    agent.State = AgentState.PathNotFound; 
                    continue; 
                }
            }

            if (agent.State == AgentState.Moving)
            {
                if (agent.CurrentEdge == null && agent.CurrentPath.Count > 0)
                {
                    agent.CurrentEdge = agent.CurrentPath.Dequeue();
                    agent.DistanceOnCurrentEdge = 0;
                }

                if (agent.CurrentEdge != null)
                {
                    double moveDistance = agent.Speed;
                    agent.DistanceOnCurrentEdge += moveDistance;
                    agent.Fuel -= moveDistance * agent.FuelConsumptionRate;

                    if (agent.Fuel <= 0)
                    {
                        agent.Fuel = 0;
                        agent.State = AgentState.DeadVehicle;
                        
                        agent.CurrentEdge.CurrentWeight += agent.CurrentEdge.Length * 10;
                        continue;
                    }

                    if (agent.DistanceOnCurrentEdge >= agent.CurrentEdge.Length)
                    {
                        agent.CurrentNode = agent.CurrentEdge.Target;
                        agent.CurrentEdge = null;

                        if (agent.CurrentNode.Id == agent.TargetNode.Id)
                        {
                            agent.State = AgentState.Evacuated;
                            agent.CurrentPath.Clear();
                        }
                    }
                }
            }

            if (agent.State == AgentState.Evacuated || 
                agent.State == AgentState.DeadVehicle || 
                agent.State == AgentState.PathNotFound)
            {
                _activeAgents.RemoveAt(i);
            }
        }
    }
}