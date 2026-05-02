using System.Globalization;
using System.IO;
using UrbanEvacuationSimulator.Core.Agent;

namespace UrbanEvacuationSimulator.Core.Metrics;

public class TelemetryExporter : IDisposable
{
    private readonly StreamWriter _writer;

    public TelemetryExporter(string filePath)
    {
        _writer = new StreamWriter(filePath);
        _writer.WriteLine("Tick,AgentId,Lat,Lon,State"); 
    }

    public void ExportTick(int tick, IEnumerable<Agent.Agent> agents)
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

    public void Dispose()
    {
        _writer.Dispose();
    }
}