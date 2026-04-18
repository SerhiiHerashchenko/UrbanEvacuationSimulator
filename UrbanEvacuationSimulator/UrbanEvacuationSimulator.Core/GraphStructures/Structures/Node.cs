using UrbanEvacuationSimulator.Core.Constants;

namespace UrbanEvacuationSimulator.Core.GraphStructures.Structures;

public class Node
{
    public readonly int Id;
    public double Lat { get; set; }
    public double Lon { get; set; }

    public Node(int id, double lat, double lon)
    {
        Id = id;
        Lat = lat;
        Lon = lon;
    }

    public double GetDistance(Node target)
    {
        var t = Math.PI / 180.0;
        var d1 = Lat * t;
        var num1 = Lon * t;
        var d2 = target.Lat * t;
        var num2 = target.Lon * t - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        
        return GraphConstants.EARTH_RADIUS_METRES * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }

    public override int GetHashCode() => HashCode.Combine(Lat, Lon);
    public override bool Equals(object? obj) => obj is Node n && n.Lat == Lat && n.Lon == Lon;
}