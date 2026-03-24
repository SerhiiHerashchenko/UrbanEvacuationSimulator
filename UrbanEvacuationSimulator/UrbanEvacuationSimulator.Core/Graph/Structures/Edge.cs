namespace UrbanEvacuationSimulator.Core.Graph.Structures;

public class Edge
{
    public readonly int Id;
    
    public Node Source { get; set; }
    public Node Target { get; set; }

    public double Length { get; set; }
    public double Capacity { get; set; }
    public double CurrentWeight { get; set; }
}