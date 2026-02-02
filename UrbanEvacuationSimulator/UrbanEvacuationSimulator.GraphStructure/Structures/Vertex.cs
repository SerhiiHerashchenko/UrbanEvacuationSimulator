namespace UrbanEvacuationSimulator.GraphStructure.Structures
{
    public readonly record struct Vertex
    {
        public double Lon { get; }
        public double Lat { get; }

        public Vertex(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }
    }
}