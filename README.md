# Urban Evacuation Simulator (UES-Sim)

## Project Overview
**Urban Evacuation Simulator** is a comprehensive micro-simulation system for urban vehicle evacuation. The project leverages real-world road network topology data (via OpenStreetMap) to simulate agent movements, accounting for factors such as traffic congestion, fuel consumption, dynamic route recalculation, and road segment capacity.

The project consists of a high-performance simulation core written in **C# (.NET)**, accompanied by a suite of **Python** microservices for map parsing, process visualization, and deep ML-driven data analytics.

---

## Architecture and Structure

The project is divided into two main parts: the simulation core and auxiliary analytics scripts.

### 1. Simulation Core (C# / .NET)
* **`UrbanEvacuationSimulator.Core`**: Contains the primary business logic and physics of the system.
    * **Agents (`Agent`)**: Vehicles with specific fuel reserves, movement speeds, and consumption rates. An agent can be in one of the following states: `Idle`, `Moving`, `Evacuated`, `DeadVehicle` (out of fuel), or `PathNotFound`.
    * **Graph (`Graph`, `Node`, `Edge`)**: The road network, taking into account segment lengths, lane capacities, and dynamic road congestion (penalties for traffic jams and abandoned vehicles).
    * **Engine (`SimulationEngine`)**: Manages the simulation ticks and agent movements.
    * **Routing (`AStarPathFinder`)**: Implementation of the A* algorithm with a dynamic route recalculation mechanic when stuck in traffic (`PathRecalculationCooldown`).
    * **Metrics (`MetricsCollector`, `TelemetryExporter`)**: Collects statistics, writes trace files, and generates datasets.
* **`UrbanEvacuationSimulator.Runner`**: The entry point of the application. It accepts a map in JSON format, generates agents, and runs the simulation loop until all units have evacuated (or stopped).

### 2. Auxiliary Services (Python)
Located in the `python_services` directory.
* **`map_loading_service/loader.py`**: Downloads the road graph of a selected area (default: "Malinovsky, Odesa") via the OpenStreetMap Overpass API and saves it to `map.json`.
* **`visualization_service/visualizator.py`**: Generates an interactive HTML map with time-based evacuation animation using telemetry data (utilizing `folium` and `pandas`).
* **`metric_analyzer_service/analyzer.py`**: Analyzes the simulation datasets. It builds correlation matrices and applies machine learning algorithms (PCA, Random Forest) to identify factors affecting evacuation success and find major road network bottlenecks.

---

## Requirements

* **.NET SDK**: Version 6.0 or newer.
* **Python**: Version 3.8 or newer.
* **Python Libraries**:
    ```bash
    pip install requests pandas folium seaborn matplotlib scikit-learn
    ```

---

## Getting Started

**Step 1. Download and Prepare the Map**
Run the road graph download script. This script will create a `map.json` file.
```bash
cd python_services/map_loading_service
python loader.py
```

**Step 2. Run the Simulation**
Place the `map.json` file in the root directory or pass it as an argument. The engine will initialize 3000 agents (by default) and start the evacuation loop.
```bash
cd UrbanEvacuationSimulator/UrbanEvacuationSimulator.Runner
dotnet run map.json
```
*During execution, statistics on active agents will be printed to the console. Upon completion, data will be saved to the `artifacts` folder.*

**Step 3. Visualize the Process**
To create an animated map of agent movements, run the visualizer:
```bash
cd python_services/visualization_service
python visualizator.py
```
*Result:* The `evacuation_map.html` file will be saved in `artifacts/datasets/`. Open it in any web browser.

**Step 4. ML Analytics and Metrics**
Run the analytics pipeline to process telemetry and generate reports:
```bash
cd python_services/metric_analyzer_service
python analyzer.py
```
*Result:* Charts and text reports will be saved in `artifacts/visualizations/` and `artifacts/reports/`.

---

## Generated Artifacts (`artifacts/`)

During execution, the simulator and Python scripts automatically generate analytical data:

* **`datasets/`**
    * `simulation_trace.csv`: Step-by-step telemetry of each agent at every tick (coordinates, state).
    * `agents_dataset.csv`: Summary table of agents (fuel consumption, time in traffic jams, nodes passed).
    * `edges_dataset.csv`: Final statistics on graph edges (peak utilization, number of abandoned vehicles).
    * `evacuation_map.html`: Interactive Folium map with a timeline.
* **`reports/`**
    * `edges_report.txt`: Top 5 most problematic road network segments (Bottlenecks).
* **`visualizations/`**
    * `correlation_heatmap.png`: Thermal matrix of evacuation feature correlation.
    * `pca_scatter.png` and `pca_eigenvectors.png`: Agent distribution using Principal Component Analysis (PCA).
    * `feature_importance.png`: Feature importance chart showing what affects evacuation chances (based on Random Forest).
    * `mortality_dynamics.png`: Dynamics of the number of evacuated and abandoned cars over time.