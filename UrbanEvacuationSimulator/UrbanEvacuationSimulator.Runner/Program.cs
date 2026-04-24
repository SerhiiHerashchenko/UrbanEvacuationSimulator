using System;
using System.Collections.Generic;
using System.Linq;
using UrbanEvacuationSimulator.Core.Agent;
using UrbanEvacuationSimulator.Core.Engines;
using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.GraphStructures;
using UrbanEvacuationSimulator.Core.GraphStructures.Structures;
using UrbanEvacuationSimulator.Core.MapParsers;
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

        // Initialize simulation components
        var agents = GenerateAgents(graph, count: 10000);
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

        PrintMetrics(engine.CurrentTick, agents);
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

    private static void PrintMetrics(int totalTicks, IReadOnlyList<Agent> agents)
    {
        int evacuated = agents.Count(a => a.State == AgentState.Evacuated);
        int deadVehicles = agents.Count(a => a.State == AgentState.DeadVehicle);
        int pathfindingFailures = agents.Count(a => a.State == AgentState.PathNotFound);
        double survivalRate = (double)evacuated / agents.Count * 100;

        Console.WriteLine("\n--- SIMULATION RESULTS ---");
        Console.WriteLine($"T_max (Clearance Time): {totalTicks} ticks");
        Console.WriteLine($"Total Agents:         {agents.Count}");
        Console.WriteLine($"Evacuated:            {evacuated}");
        Console.WriteLine($"Dead Vehicles:        {deadVehicles}");
        Console.WriteLine($"Pathfinding Failures: {pathfindingFailures}");
        Console.WriteLine($"S_rate (Survival %):  {survivalRate:F2}%");
        Console.WriteLine("--------------------------");
    }
}