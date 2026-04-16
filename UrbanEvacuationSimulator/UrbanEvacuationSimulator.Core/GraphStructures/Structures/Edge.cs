namespace UrbanEvacuationSimulator.Core.GraphStructures.Structures;

public class Edge
{
    public readonly int Id;
    public Node Source { get; set; }
    public Node Target { get; set; }

    public double Length { get; set; }
    public double Capacity { get; set; }
    public double CurrentWeight { get; set; }

    public Edge(int id, Node source, Node target, double length = 0, double capacity = 0)
    {
        Id = id;
        Source = source;
        Target = target;
        Length = length;
        Capacity = capacity;
        CurrentWeight = length;
    }
}