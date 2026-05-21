using System.Globalization;
using System.IO;
using UrbanEvacuationSimulator.Core.AgentStructure;
using UrbanEvacuationSimulator.Core.Enums;
using UrbanEvacuationSimulator.Core.GraphStructures;

namespace UrbanEvacuationSimulator.Core.Metrics;

public class TelemetryExporter : IDisposable
{
    private readonly StreamWriter _writer;

    public TelemetryExporter(string filePath)
    {
        _writer = new StreamWriter(filePath);
        _writer.WriteLine("Tick,AgentId,Lat,Lon,State"); 
    }

    public void ExportTick(int tick, IEnumerable<Agent> agents)
    {
        // Optimize: export every 5th tick
        //if (tick % 5 != 0) return;

        foreach (var agent in agents)
        {
            _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                "{0},{1},{2:F6},{3:F6},{4}", 
                tick, 
                agent.Id, 
                agent.CurrentNode.Lat, 
                agent.CurrentNode.Lon, 
                agent.State));
        }
    }

    public void ExportDatasets(IReadOnlyList<Agent> agents, Graph graph)
    {
        Console.WriteLine("\nExporting ML Datasets...");

        var validAgents = agents.Where(a => a.State != AgentState.PathNotFound).ToList();

        using (var writer = new StreamWriter("..\\..\\agents_dataset.csv"))
        {
            writer.WriteLine("AgentId,State,InitialFuel,InitialDistance,FuelConsumptionRate,TotalPassedDistance,TotalNodesPassed,PathCalculationsCount,TicksInCongestion,CongestionRate");
            foreach (var a in validAgents)
            {
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2:F6},{3:F6},{4:F6},{5:F6},{6},{7},{8},{9:F6}", 
                    a.Id, 
                    a.State, 
                    a.InitialFuel, 
                    a.InitialDistance, 
                    a.FuelConsumptionRate, 
                    a.TotalPassedDistance, 
                    a.TotalNodesPassed, 
                    a.PathCalculationsCount, 
                    a.TicksInCongestion,
                    a.CongestionRate));
            }
        }

        using (var writer = new StreamWriter("..\\..\\edges_dataset.csv"))
        {
            writer.WriteLine("EdgeId,SourceLat,SourceLon,TargetLat,TargetLon,Length,Capacity,MaxUtilization,CongestionDurationTicks,TotalAgentsPassed,DeadVehiclesCount");
            foreach (var e in graph.Edges)
            {
                if (e.MaxUtilization > 0 || e.DeadVehiclesCount > 0)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1:F6},{2:F6},{3:F6},{4:F6},{5:F6},{6},{7:F6},{8},{9},{10}", 
                        e.Id, 
                        e.Source.Lat, 
                        e.Source.Lon, 
                        e.Target.Lat, 
                        e.Target.Lon, 
                        e.Length, 
                        e.Capacity, 
                        e.MaxUtilization, 
                        e.CongestionDurationTicks, 
                        e.TotalAgentsPassed, 
                        e.DeadVehiclesCount));
                }
            }
        }
        Console.WriteLine("Datasets exported: agents_dataset.csv, edges_dataset.csv");
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}