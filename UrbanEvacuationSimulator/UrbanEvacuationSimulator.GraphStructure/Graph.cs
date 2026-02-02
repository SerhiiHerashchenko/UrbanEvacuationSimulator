namespace UrbanEvacuationSimulator.GraphStructure
{
    public class Graph<T>
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
    }
}