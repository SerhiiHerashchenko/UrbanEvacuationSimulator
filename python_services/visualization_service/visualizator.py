import random
from datetime import datetime, timedelta

import pandas as pd
import folium
from folium.plugins import TimestampedGeoJson


def create_animation(csv_file, output_html):
    print(f"Loading data from {csv_file}...")

    df = pd.read_csv(csv_file)
    df = df.dropna(subset=['Lat', 'Lon', 'Tick', 'AgentId'])

    if df.empty:
        print("Error: CSV file is empty or missing required columns.")
        return

    step = 2

    unique_ticks = sorted(df['Tick'].unique())
    step = max(1, step)
    df = df[df['Tick'].isin(unique_ticks[::step])]

    unique_agents = df['AgentId'].unique()
    if len(unique_agents) > 1000:
        agents_to_keep = random.sample(list(unique_agents), 1000)
        df = df[df['AgentId'].isin(agents_to_keep)]

    if df.empty:
        print("Error: No data left after filtering.")
        return

    start_time = datetime(2026, 4, 24, 8, 0, 0)
    df['time'] = df['Tick'].apply(
        lambda x: (start_time + timedelta(seconds=int(x))).isoformat()
    )

    color_map = {
        'Idle': '#808080',
        'Moving': '#3388ff',
        'DeadVehicle': '#ff0000',
        'Evacuated': '#00ff00',
        'PathNotFound': '#ffa500'
    }

    print("Generating GeoJSON frames...")
    features = []

    for _, row in df.iterrows():
        state = str(row['State']).strip()
        
        feature = {
            'type': 'Feature',
            'geometry': {
                'type': 'Point',
                'coordinates': [float(row['Lon']), float(row['Lat'])]
            },
            'properties': {
                'time': row['time'],
                'icon': 'circle',
                'iconstyle': {
                    'fillColor': color_map.get(state, '#ffffff'),
                    'fillOpacity': 0.8,
                    'stroke': 'false',
                    'radius': 4
                },
                'popup': f"Agent: {row['AgentId']}<br>State: {state}"
            }
        }
        features.append(feature)

    print("Generating HTML map...")
    center_lat = df['Lat'].mean()
    center_lon = df['Lon'].mean()

    m = folium.Map(
        location=[center_lat, center_lon], 
        zoom_start=14, 
        tiles='CartoDB dark_matter'
    )

    TimestampedGeoJson(
        {'type': 'FeatureCollection', 'features': features},
        period=f'PT{step}S',
        duration=f'PT{step}S',
        add_last_point=False,
        auto_play=True,
        loop=True,
        max_speed=5,
        loop_button=True,
        time_slider_drag_update=True,
        transition_time=50
    ).add_to(m)

    m.save(output_html)
    print(f"Success! Map saved as: {output_html}")


if __name__ == "__main__":
    create_animation("..\\artifacts\\datasets\\simulation_trace.csv", "..\\artifacts\\datasets\\evacuation_map.html")