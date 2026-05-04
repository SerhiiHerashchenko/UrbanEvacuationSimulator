# Urban Evacuation Simulator (UES-Sim)

**Urban Evacuation Simulator** is an agent-based model designed to simulate mass population evacuation processes in urban environments. The project utilizes real topological data from OpenStreetMap (OSM) and implements A* pathfinding algorithms, taking into account fuel consumption, movement speed, and dynamic congestion (graph backpressure).

⚠️ **Project Status: Work in Progress (Active Development)** ⚠️  
*The base engine and visualization pipeline are successfully functioning. The project is currently in the architectural design phase for the analytics subsystem to enable multi-dimensional statistical analysis.*

---

## 🎯 Current Development Stage: Metrics & Statistical Analysis

At this current stage, our primary objective is to determine **what metrics to collect and how to collect them** to perform deep, multi-dimensional statistical analysis of the simulation.

We plan to subject the collected data (agent telemetry and graph states) to advanced statistical methods, including Correlation Analysis, Principal Component Analysis (PCA), and Clustering.

**Open Research Questions:**
* How can we dynamically collect the "congestion" metric for each node/edge to evaluate traffic jams over time?
* Which individual agent metrics (remaining fuel, distance traveled, number of route recalculations) have the highest correlation with the final outcome (`Evacuated` vs. `DeadVehicle`)?
* How can we aggregate data at the tick level (Time-series analysis) to identify critical "bottlenecks" in the real urban infrastructure?

---

## 🏗 Project Architecture

The project is divided into a high-performance mathematical core and a set of utilities for data preparation and visualization.

1. **Map Loader (`map_loader.py`)** — A Python utility for downloading the real road graph of a selected region via Nominatim and the Overpass API. It saves the connected topology into `map.json`.
2. **Simulation Core (C# / .NET)** — The multi-threaded simulation engine. 
   * Parses OSM data into a mathematical system of nodes and edges.
   * Initializes agents and calculates their routes using the A* algorithm.
   * Generates a telemetry CSV file (`simulation_trace.csv`) containing frame-by-frame state changes.
3. **Visualizer (`vizualizator.py`)** — A Python script based on the `folium` library. It reads the exported CSV telemetry, performs smart data downsampling for browser performance, and generates an interactive HTML animation of the evacuation on a real map.

---

## 🚀 How to Run the Simulation

### Step 1. Fetch the Map
Ensure you have Python installed along with the required libraries (`requests`).
```bash
pip install requests
python map_loader.py
```
Output: A map.json file will be generated in the root directory.

### Step 2. Run the Simulation Core (C#)
Ensure you have the .NET SDK installed. Run the C# application, passing the path to the downloaded map.

```Bash
cd UrbanEvacuationSimulator.Runner
dotnet run -- ../map.json
```
Output: The engine will load the graph, run the simulation for the specified number of agents, and create the simulation_trace.csv telemetry file.

### Step 3. Visualize the Results
To generate the HTML map, you will need the pandas and folium libraries.

```Bash
pip install pandas folium
python vizualizator.py
```
Output: The script will generate an evacuation_map.html file. Open this file in any modern web browser to view the real-time agent movement animation.

### 📊 Collected Metrics (Current Implementation)
The MetricsCollector class currently implements basic collection for general simulation indicators (SimulationMetricType):

- TotalSimulationTimeSpentMilliseconds — Real-world computation time.

- TotalTicks (T_max) — Clearance time (the time taken until full evacuation or complete traffic gridlock).

- SurvivalRate — The percentage of agents that successfully evacuated.

- Averages (AverageEvacuatedDistance, AverageDeadVehicleDistance, etc.).

- The development of an extended list of AgentMetricType (behavioral and localized metrics) is planned for the upcoming iterations to feed our statistical models.

### 🛠 Tech Stack
- Core Engine: C#, .NET 8+

- Data Fetching: Python 3, Requests, Overpass API

- Visualization: Python 3, Pandas, Folium, Leaflet.js