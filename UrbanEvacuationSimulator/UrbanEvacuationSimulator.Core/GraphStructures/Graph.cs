using UrbanEvacuationSimulator.Core.DTOs;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;

namespace UrbanEvacuationSimulator.Core.GraphStructures;

public class Graph
{
    public IReadOnlyList<Node> Nodes { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public Dictionary<int, List<Edge>> AdjacencyList { get; }

    private Graph(List<Node> nodes, List<Edge> edges, Dictionary<int, List<Edge>> adjacencyList)
    {
        Nodes = nodes.AsReadOnly();
        Edges = edges.AsReadOnly();
        AdjacencyList = adjacencyList;
    }
    
    public static Graph CreateGraph(IReadOnlyList<OsmEdgeDto> edgesDto)
    {
        var nodeMap = new Dictionary<(double Lat, double Lon), Node>();
        var nodes = new List<Node>();
        var edges = new List<Edge>();
        var adjacencyList = new Dictionary<int, List<Edge>>();

        int nodeIdCounter = 1;

        Node GetOrAddNode(OsmNodeDto dto)
        {
            var key = (dto.Lat, dto.Lon);
            if (!nodeMap.TryGetValue(key, out var node))
            {
                node = new Node(nodeIdCounter++, dto.Lat, dto.Lon);
                nodeMap[key] = node;
                nodes.Add(node);
                adjacencyList[node.Id] = new List<Edge>();
            }
            return node;
        }

        foreach (var edgeDto in edgesDto)
        {
            var sourceNode = GetOrAddNode(edgeDto.Start);
            var targetNode = GetOrAddNode(edgeDto.End);

            double capacity = edgeDto.Lanes ?? 1.0;
            double length = sourceNode.GetDistance(targetNode);

            var edge = new Edge(edgeDto.Id, sourceNode, targetNode, length, capacity);
            edges.Add(edge);
            
            adjacencyList[sourceNode.Id].Add(edge);
        }

        return new Graph(nodes, edges, adjacencyList);
    }
}