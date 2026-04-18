using UrbanEvacuationSimulator.Core.GraphStructures;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;

namespace UrbanEvacuationSimulator.Core.Interfaces;

public interface IPathFinder
{
    IReadOnlyList<Edge> FindPath(
        Graph graph,
        Node start,
        Node target,
        Func<Node, Node, double> heuristic);
}