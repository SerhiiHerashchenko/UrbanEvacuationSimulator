import json
import requests

def fetch_roads():
    area_name = "Малиновский, Одесса"
    print(f"Ищем район: {area_name}...")

    nom_url = "https://nominatim.openstreetmap.org/search"
    nom_params = {
        "q": area_name,
        "format": "json",
        "limit": 1
    }

    headers = {"User-Agent": "UrbanEvacuationDataFetcher/1.0"}

    nom_response = requests.get(nom_url, params=nom_params, headers=headers).json()

    if not nom_response:
        print("Area not found.")
        return

    area_data = nom_response[0]
    osm_id = int(area_data["osm_id"])
    osm_type = area_data["osm_type"]

    if osm_type == "relation":
        overpass_area_id = osm_id + 3600000000
    elif osm_type == "way":
        overpass_area_id = osm_id + 2400000000
    else:
        print("Unsupported object type for area.")
        return

    overpass_url = "https://overpass-api.de/api/interpreter" 
    overpass_query = f"""
    [out:json];
    area({overpass_area_id})->.searchArea;
    way["highway"](area.searchArea);
    (._;>;);
    out body;
    """

    print("Map Topology downloading...(it can take about a minute)")

    response = requests.post(overpass_url, data={"data": overpass_query}, headers=headers)

    if response.status_code != 200:
        print(f"Server Error (Overpass): {response.status_code}")
        print("Server Response:", response.text)
        return

    data = response.json()
    elements = data.get("elements", [])

    nodes_count = sum(1 for e in elements if e["type"] == "node")
    ways_count = sum(1 for e in elements if e["type"] == "way")

    print(f"Found objects: {len(elements)} (Nodes: {nodes_count}, Roads: {ways_count}).")

    output_filename = "map.json"

    with open(output_filename, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"Done! The complete connected graph has been successfully saved to {output_filename}")


if __name__ == "__main__":
    fetch_roads()