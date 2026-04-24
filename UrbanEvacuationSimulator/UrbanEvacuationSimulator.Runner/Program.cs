using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UrbanEvacuationSimulator.Core.Agent;
using UrbanEvacuationSimulator.Core.Engines;
using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.GraphStructures;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;
using UrbanEvacuationSimulator.Core.MapParsers;
using UrbanEvacuationSimulator.Core.Metrics;
using UrbanEvacuationSimulator.Core.PathFinder;

namespace UrbanEvacuationSimulator.Runner;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Urban Evacuation Simulator (UES-Sim)...");

        string mapFilePath = args.Length > 0 ? args[0] : "map.json";
        
        var mapParser = new JsonMapParser();
        if (!mapParser.TryParse(mapFilePath, out var responseDto))
        {
            Console.WriteLine($"Failed to parse map file: {mapFilePath}");
            Console.WriteLine("Ensure the JSON map file exists and is correctly formatted.");
            return;
        }

        Console.WriteLine($"Map parsed successfully. Loaded {responseDto.Elements.Count} elements.");

        var graph = Graph.CreateGraph(responseDto);
        Console.WriteLine($"Graph built: {graph.Nodes.Count} nodes, {graph.Edges.Count} edges.");

        if (graph.Nodes.Count < 2)
        {
            Console.WriteLine("Graph does not contain enough nodes for simulation.");
            return;
        }

        var metricsCollector = new MetricsCollector();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        // Initialize simulation components
        var agents = GenerateAgents(graph, count: 10000);
        metricsCollector.Collect(MetricType.TotalAgents, agents.Count);

        var pathFinder = new AStarPathFinder();
        var engine = new SimulationEngine(graph, agents, pathFinder);

        Console.WriteLine($"\nInitialized engine with {agents.Count} agents. Starting simulation loop...");

        bool isRunning = true;

        while (isRunning)
        {
            engine.Tick();

            int activeAgents = agents.Count(a => a.State == AgentState.Idle || a.State == AgentState.Moving);
            
            if (engine.CurrentTick % 50 == 0)
            {
                Console.WriteLine($"Tick {engine.CurrentTick, 5} | Active Agents: {activeAgents, 4}");
            }

            if (activeAgents == 0)
            {
                isRunning = false;
            }
        }
        
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        var evacuatedAgentsList = agents.Where(a => a.State == AgentState.Evacuated).ToList();
        var deadVehicleAgentsList = agents.Where(a => a.State == AgentState.DeadVehicle).ToList();

        var runId = Guid.NewGuid();
        metricsCollector.Collect(MetricType.RunId, runId.GetHashCode());
        metricsCollector.Collect(MetricType.TotalSimulationTimeSpentMilliseconds, ts.TotalMilliseconds);
        metricsCollector.Collect(MetricType.TotalTicks, engine.CurrentTick);

        metricsCollector.Collect(MetricType.EvacuatedCount, evacuatedAgentsList.Count);
        metricsCollector.Collect(MetricType.DeadVehicleCount, deadVehicleAgentsList.Count);
        metricsCollector.Collect(MetricType.PathfindingFailureCount, agents.Count(a => a.State == AgentState.PathNotFound));

        metricsCollector.Collect(MetricType.SurvivalRate, (double)evacuatedAgentsList.Count / agents.Count * 100);

        metricsCollector.Collect(MetricType.AverageEvacuatedDistance, evacuatedAgentsList.Average(a => a.TotalPassedDistance));
        metricsCollector.Collect(MetricType.AverageEvacuatedNodesPassed, evacuatedAgentsList.Average(a => a.TotalNodesPassed));
        metricsCollector.Collect(MetricType.AverageDeadVehicleDistance, deadVehicleAgentsList.Average(a => a.TotalPassedDistance));
        metricsCollector.Collect(MetricType.AverageDeadVehicleNodesPassed, deadVehicleAgentsList.Average(a => a.TotalNodesPassed));
        metricsCollector.Collect(MetricType.AverageEvacuatedEdgeLength, evacuatedAgentsList.Average(a => a.TotalPassedDistance / a.TotalNodesPassed));
        metricsCollector.Collect(MetricType.AverageDeadVehicleEdgeLength, deadVehicleAgentsList.Average(a => a.TotalPassedDistance / a.TotalNodesPassed));

        metricsCollector.PrintMetricsToConsole();
    }

    private static IReadOnlyList<Agent> GenerateAgents(Graph graph, int count)
    {
        var agents = new List<Agent>();
        var random = new Random(42); // Fixed seed for reproducibility

        var targetNode = graph.Nodes[random.Next(graph.Nodes.Count)];

        for (int i = 1; i <= count; i++)
        {
            var startNode = graph.Nodes[random.Next(graph.Nodes.Count)];

            while (startNode.Id == targetNode.Id)
            {
                startNode = graph.Nodes[random.Next(graph.Nodes.Count)];
            }
            
            // Randomize fuel (meters) and speed (m/tick)
            double fuel = random.Next(2000, 15000); 
            double speed = random.Next(10, 25);

            agents.Add(new Agent(i, startNode, targetNode, fuel, speed));
        }

        return agents.AsReadOnly();
    }
}