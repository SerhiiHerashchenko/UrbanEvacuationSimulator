using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UrbanEvacuationSimulator.Core.AgentStructure;
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
        var agents = GenerateAgents(graph, count: 3000);
        metricsCollector.CollectSimulationMetric(SimulationMetricType.TotalAgents, agents.Count);

        var pathFinder = new AStarPathFinder();
        var engine = new SimulationEngine(graph, agents, pathFinder);

        Console.WriteLine($"\nInitialized engine with {agents.Count} agents. Starting simulation loop...");

        bool isRunning = true;

        var telemetry = new TelemetryExporter("..\\..\\artifacts\\datasets\\simulation_trace.csv");
        while (isRunning)
        {
            engine.Tick();
            telemetry.ExportTick(engine.CurrentTick, agents);

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

        var runId = 0;;
        metricsCollector.CollectSimulationMetric(SimulationMetricType.RunId,
            runId.GetHashCode());
        metricsCollector.CollectSimulationMetric(SimulationMetricType.TotalSimulationTimeSpentMilliseconds,
            ts.TotalMilliseconds);
        metricsCollector.CollectSimulationMetric(SimulationMetricType.TotalTicks,
            engine.CurrentTick);

        metricsCollector.CollectSimulationMetric(SimulationMetricType.EvacuatedCount,
            evacuatedAgentsList.Count);
        metricsCollector.CollectSimulationMetric(SimulationMetricType.DeadVehicleCount,
            deadVehicleAgentsList.Count);
        metricsCollector.CollectSimulationMetric(SimulationMetricType.PathfindingFailureCount,
            agents.Count(a => a.State == AgentState.PathNotFound));

        metricsCollector.CollectSimulationMetric(SimulationMetricType.SurvivalRate,
            (double)evacuatedAgentsList.Count / agents.Count * 100);

        if (evacuatedAgentsList.Any())
        {
            var sortedEvacuated = evacuatedAgentsList.OrderBy(a => a.EvacuationTick).ToList();
            int t99Index = (int)Math.Ceiling(sortedEvacuated.Count * 0.99) - 1;
            t99Index = Math.Max(0, t99Index);
            
            int t99Tick = sortedEvacuated[t99Index].EvacuationTick ?? engine.CurrentTick;
            metricsCollector.CollectSimulationMetric(SimulationMetricType.T99ClearanceTime, t99Tick);
        }
            
        telemetry.ExportDatasets(agents, graph);
        metricsCollector.ExportMetricsToCsv("..\\..\\artifacts\\datasets\\simulation_metrics.csv");
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
            double fuel = random.Next(2000, 30000); 
            double speed = random.Next(25, 50);

            agents.Add(new Agent(i, startNode, targetNode, fuel, speed));
        }

        return agents.AsReadOnly();
    }
}