using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UrbanEvacuationSimulator.Core.DTOs;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;

namespace UrbanEvacuationSimulator.Core.GraphStructures;

public class Graph
{
    public IReadOnlyList<Node> Nodes { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public List<List<Edge>> AdjacencyList { get; }

    public Graph(IReadOnlyList<Node> nodes, IReadOnlyList<Edge> edges, List<List<Edge>> adjacencyList)
    {
        Nodes = nodes;
        Edges = edges;
        AdjacencyList = adjacencyList;
    }

    public static Graph CreateGraph(OverpassResponseDto data)
    {
        var nodeMap = new Dictionary<long, Node>();
        var edges = new List<Edge>();
        var adjacencyList = new List<List<Edge>>();

        int internalNodeId = 0;
        int edgeIdCounter = 0;

        foreach (var el in data.Elements.Where(e => e.Type == "node"))
        {
            if (el.Lat.HasValue && el.Lon.HasValue)
            {
                var node = new Node(internalNodeId++, el.Lat.Value, el.Lon.Value);
                nodeMap[el.Id] = node;
                adjacencyList.Add(new List<Edge>());
            }
        }

        foreach (var way in data.Elements.Where(e => e.Type == "way"))
        {
            if (way.Nodes == null || way.Nodes.Count < 2) continue;

            double capacity = 1.0;
            if (way.Tags != null && way.Tags.TryGetValue("lanes", out var lanesStr))
            {
                double.TryParse(lanesStr, NumberStyles.Any, CultureInfo.InvariantCulture, out capacity);
            }

            bool isOneWay = way.Tags != null && 
                            way.Tags.TryGetValue("oneway", out var onewayValue) && 
                            (onewayValue == "yes" || onewayValue == "true" || onewayValue == "1");

            for (int i = 0; i < way.Nodes.Count - 1; i++)
            {
                long osmSourceId = way.Nodes[i];
                long osmTargetId = way.Nodes[i + 1];

                if (!nodeMap.TryGetValue(osmSourceId, out var sourceNode) || 
                    !nodeMap.TryGetValue(osmTargetId, out var targetNode))
                {
                    continue;
                }

                double length = sourceNode.GetDistance(targetNode);

                var forwardEdge = new Edge(edgeIdCounter++, sourceNode, targetNode, length, capacity);
                edges.Add(forwardEdge);
                adjacencyList[sourceNode.Id].Add(forwardEdge);

                if (!isOneWay)
                {
                    var reverseEdge = new Edge(edgeIdCounter++, targetNode, sourceNode, length, capacity);
                    edges.Add(reverseEdge);
                    adjacencyList[targetNode.Id].Add(reverseEdge);
                }
            }
        }

        var connectedNodes = nodeMap.Values.Where(n => adjacencyList[n.Id].Count > 0).ToList();

        return new Graph(connectedNodes, edges, adjacencyList);
    }
}