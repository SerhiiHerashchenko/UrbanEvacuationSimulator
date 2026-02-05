namespace UrbanEvacuationSimulator.GraphStructure
{
    public class Graph<T> where T : notnull
    {
        private Dictionary<T, HashSet<T>> adjacencyList;

        public Graph() => adjacencyList = new Dictionary<T, HashSet<T>>();

        public void AddVertex(T vertex)
        {
            if (!adjacencyList.ContainsKey(vertex))
            {
                adjacencyList[vertex] = new HashSet<T>();
            }
        }
        
        public void AddEdge(T source, T destination, bool bidirectional = false)
        {
            AddVertex(source);
            AddVertex(destination);
            
            adjacencyList[source].Add(destination);

            if (bidirectional)
            {
                adjacencyList[destination].Add(source);
            }
        }
        
        public List<T> FindPathAStar(
            T start, 
            T destination, 
            Func<T, T, double> heuristic, 
            Func<T, T, double> costFunction)
        {
            if (!adjacencyList.ContainsKey(start) ||
                !adjacencyList.ContainsKey(destination))
            {
                return null;
            }

            var openSet = new PriorityQueue<T, double>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<T, T>();
            var gScore = new Dictionary<T, double>();
            gScore[start] = 0;
            
            var fScore = new Dictionary<T, double>();
            fScore[start] = heuristic(start, destination);

            var openSetHash = new HashSet<T> { start };

            while (openSet.Count > 0)
            {
                T current = openSet.Dequeue();
                openSetHash.Remove(current);

                if (EqualityComparer<T>.Default.Equals(current, destination))
                {
                    return ReconstructPath(cameFrom, current);
                }
                
                if (adjacencyList.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        double weight = costFunction(current, neighbor);
                        
                        double tentativeGScore = gScore[current] + weight;

                        if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                        {
                            cameFrom[neighbor] = current;
                            gScore[neighbor] = tentativeGScore;
                            double f = tentativeGScore + heuristic(neighbor, destination);
                            fScore[neighbor] = f;

                            if (!openSetHash.Contains(neighbor))
                            {
                                openSet.Enqueue(neighbor, f);
                                openSetHash.Add(neighbor);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private List<T> ReconstructPath(Dictionary<T, T> cameFrom, T current)
        {
            var totalPath = new List<T>();
            totalPath.Add(current);
            
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            
            totalPath.Reverse();
            return totalPath;
        }
    }
}