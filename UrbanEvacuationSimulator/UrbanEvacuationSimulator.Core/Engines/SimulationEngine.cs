using UrbanEvacuationSimulator.Core.AgentStructure;
using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.GraphStructures;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;
using UrbanEvacuationSimulator.Core.Interfaces;


namespace UrbanEvacuationSimulator.Core.Engines;

public class SimulationEngine : ISimulationEngine
{
    private readonly Graph _graph;
    private readonly IReadOnlyList<Agent> _agents;
    private readonly IPathFinder _pathFinder;
    private readonly Func<Node, Node, double> _heuristic;
    private readonly List<Agent> _activeAgents;
    
    public int CurrentTick { get; private set; }

    public SimulationEngine(Graph graph, IReadOnlyList<Agent> agents, IPathFinder pathFinder)
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

        foreach (var edge in _graph.Edges)
        {
            edge.UpdateWeight();
        }

        for (int i = _activeAgents.Count - 1; i >= 0; i--)
        {
            var agent = _activeAgents[i];
            bool pathJustCalculated = false;

            if ((agent.State == AgentState.Idle || agent.State == AgentState.Moving) && agent.CurrentPath.Count == 0)
            {
                var path = _pathFinder.FindPath(_graph, agent.CurrentNode, agent.TargetNode, _heuristic);
                if (path.Count > 0)
                {
                    agent.CurrentPath = new Queue<Edge>(path);
                    agent.State = AgentState.Moving;
                    agent.PathCalculationsCount++;
                    pathJustCalculated = true;
                }
                else
                {
                    agent.State = AgentState.PathNotFound; 
                    continue; 
                }
            }

            if (agent.State == AgentState.Moving)
            {
                agent.Fuel -= 0.5; 

                if (agent.CurrentEdge == null && agent.CurrentPath.Count > 0)
                {
                    var nextEdge = agent.CurrentPath.Peek();

                    if (!pathJustCalculated && nextEdge.CurrentWeight > nextEdge.Length * 3.0)
                    {
                        agent.CurrentPath.Clear();
                        continue; 
                    }

                    agent.CurrentEdge = agent.CurrentPath.Dequeue();
                    agent.CurrentEdge.ActiveAgentsCount++; 
                    agent.TotalNodesPassed++;
                    agent.DistanceOnCurrentEdge = 0;
                }

                if (agent.CurrentEdge != null)
                {
                    double speedFactor = agent.CurrentEdge.GetSpeedFactor();
                    double actualSpeed = agent.Speed * speedFactor;

                    if (speedFactor < 0.5) 
                    {
                        agent.TicksInCongestion++;
                    }

                    agent.DistanceOnCurrentEdge += actualSpeed;
                    agent.TotalPassedDistance += actualSpeed;
                    
                    agent.Fuel -= actualSpeed * agent.FuelConsumptionRate; 

                    if (agent.Fuel <= 0)
                    {
                        agent.Fuel = 0;
                        agent.State = AgentState.DeadVehicle;
                        
                        agent.CurrentEdge.ActiveAgentsCount--; 
                        agent.CurrentEdge.DeadVehiclesCount++; 
                        continue;
                    }

                    if (agent.DistanceOnCurrentEdge >= agent.CurrentEdge.Length)
                    {
                        agent.CurrentEdge.ActiveAgentsCount--; 
                        agent.CurrentEdge.TotalAgentsPassed++; // АНАЛИТИКА: Агент успешно покинул ребро
                        agent.CurrentNode = agent.CurrentEdge.Target;
                        agent.CurrentEdge = null;

                        if (agent.CurrentNode.Id == agent.TargetNode.Id)
                        {
                            agent.State = AgentState.Evacuated;
                            agent.CurrentPath.Clear();
                        }
                    }
                }
                else if (agent.Fuel <= 0)
                {
                    agent.Fuel = 0;
                    agent.State = AgentState.DeadVehicle;
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