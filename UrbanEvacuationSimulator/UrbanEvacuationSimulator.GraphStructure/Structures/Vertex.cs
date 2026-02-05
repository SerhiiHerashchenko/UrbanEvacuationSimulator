namespace UrbanEvacuationSimulator.GraphStructure.Structures
{
    public readonly record struct Vertex
    {
        private const double EarthRadiusKm = 6371.0;
        
        public double Lon { get; }
        public double Lat { get; }

        public Vertex(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }
        
        public static double GetDistance(Vertex a, Vertex b)
        {
            var dLat = DegreesToRadians(b.Lat - a.Lat);
            var dLon = DegreesToRadians(b.Lon - a.Lon);

            var lat1 = DegreesToRadians(a.Lat);
            var lat2 = DegreesToRadians(b.Lat);

            var x = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        
            var d = 2 * Math.Asin(Math.Sqrt(x));

            return EarthRadiusKm * d;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}