using UrbanEvacuationSimulator.Core.GraphStructures;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;
using UrbanEvacuationSimulator.Core.Interfaces;

namespace UrbanEvacuationSimulator.Core.PathFinder;

public class AStarPathFinder : IPathFinder
{
    public IReadOnlyList<Edge> FindPath(Graph graph, Node start, Node target, Func<Node, Node, double> heuristic)
    {
        var openSet = new PriorityQueue<Node, double>();
        var cameFrom = new Dictionary<int, Edge>();
        var gScore = new Dictionary<int, double>();
        
        foreach (var node in graph.Nodes)
        {
            gScore[node.Id] = double.PositiveInfinity;
        }

        gScore[start.Id] = 0;
        openSet.Enqueue(start, heuristic(start, target));

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.Id == target.Id)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            var neighbors = graph.AdjacencyList[current.Id];
            if (neighbors == null)
            {
                continue;
            }

            foreach (var edge in neighbors)
            {
                var neighbor = edge.Target;
                var tentativeGScore = gScore[current.Id] + edge.CurrentWeight;

                if (tentativeGScore < gScore[neighbor.Id])
                {
                    cameFrom[neighbor.Id] = edge;
                    gScore[neighbor.Id] = tentativeGScore;
                    
                    double fScore = tentativeGScore + heuristic(neighbor, target);
                    
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return Array.Empty<Edge>();
    }

    private static IReadOnlyList<Edge> ReconstructPath(Dictionary<int, Edge> cameFrom, Node current)
    {
        var path = new List<Edge>();
        var currentNodeId = current.Id;

        while (cameFrom.TryGetValue(currentNodeId, out var edge))
        {
            path.Add(edge);
            currentNodeId = edge.Source.Id;
        }

        path.Reverse();
        return path.AsReadOnly();
    }
}