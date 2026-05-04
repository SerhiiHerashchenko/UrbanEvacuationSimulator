import pandas as pd
import folium
from folium.plugins import TimestampedGeoJson
from datetime import datetime, timedelta
import random


def create_animation(csv_file, output_html):
    print(f"Загрузка данных из {csv_file}...")

    # Читаем данные и сразу удаляем строки с битыми/пустыми координатами
    df = pd.read_csv(csv_file)
    df = df.dropna(subset=['Lat', 'Lon', 'Tick', 'AgentId'])

    print(f"Успешно загружено строк: {len(df)}")

    if df.empty:
        print("Ошибка: Файл CSV пуст или не содержит нужных колонок.")
        return

    # ---------------------------------------------------------
    # 1. БЕЗОПАСНАЯ ФИЛЬТРАЦИЯ ДАННЫХ
    # ---------------------------------------------------------
    # Прореживание по времени: берем каждый 10-й уникальный тик, который есть в файле
    unique_ticks = sorted(df['Tick'].unique())
    print(f"Всего уникальных тиков в файле: {len(unique_ticks)} (от {unique_ticks[0]} до {unique_ticks[-1]})")

    # Берем каждый 10-й кадр (если тиков мало, берем каждый 2-й или 1-й)
    step = max(1, len(unique_ticks) // 550)  # Стараемся оставить около 50 кадров для плавной анимации
    ticks_to_keep = unique_ticks[::step]

    df = df[df['Tick'].isin(ticks_to_keep)]
    print(f"После фильтрации тиков (оставлено {len(ticks_to_keep)} кадров) осталось строк: {len(df)}")

    # Прореживание по агентам: оставляем максимум 1000 случайных машин
    unique_agents = df['AgentId'].unique()
    if len(unique_agents) > 1000:
        agents_to_keep = random.sample(list(unique_agents), 1000)
        df = df[df['AgentId'].isin(agents_to_keep)]

    print(f"После фильтрации агентов (оставлено 1000) итоговых строк для рендера: {len(df)}")

    # Финальная проверка перед рендером
    if df.empty:
        print("Критическая ошибка: После фильтрации не осталось данных! Снизьте агрессивность фильтра.")
        return

    # ---------------------------------------------------------
    # 2. ПОДГОТОВКА ВРЕМЕНИ И ЦВЕТОВ
    # ---------------------------------------------------------
    start_time = datetime(2026, 4, 24, 8, 0, 0)

    # Folium требует точного формата времени ISO 8601
    df['time'] = df['Tick'].apply(lambda x: (start_time + timedelta(seconds=int(x))).isoformat())

    color_map = {
        'Idle': '#808080',  # Серый (ожидание)
        'Moving': '#3388ff',  # Синий (в движении)
        'DeadVehicle': '#ff0000',  # Красный (застрял/нет топлива)
        'Evacuated': '#00ff00',  # Зеленый (успешно эвакуирован)
        'PathNotFound': '#ffa500'  # Оранжевый (тупик)
    }

    # ---------------------------------------------------------
    # 3. ФОРМИРОВАНИЕ GEOJSON ДЛЯ FOLIUM
    # ---------------------------------------------------------
    print("Генерация GeoJSON кадров (это может занять около минуты)...")
    features = []

    for _, row in df.iterrows():
        state = str(row['State']).strip()
        color = color_map.get(state, '#ffffff')  # Белый по умолчанию, если статус не распознан

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
                    'fillColor': color,
                    'fillOpacity': 0.8,
                    'stroke': 'false',
                    'radius': 4
                },
                'popup': f"Agent: {row['AgentId']}<br>State: {state}"
            }
        }
        features.append(feature)

    # ---------------------------------------------------------
    # 4. СБОРКА И СОХРАНЕНИЕ КАРТЫ
    # ---------------------------------------------------------
    print("Сборка HTML карты...")
    center_lat = df['Lat'].mean()
    center_lon = df['Lon'].mean()

    m = folium.Map(location=[center_lat, center_lon], zoom_start=14, tiles='CartoDB dark_matter')

    # Формируем строку времени ISO 8601 (например, 'PT10S' для 10 секунд)
    # Переменная step у нас уже вычислена выше в коде при фильтрации
    time_step = f'PT{step}S'

    TimestampedGeoJson(
        {'type': 'FeatureCollection', 'features': features},
        period=time_step,  # Шаг ползунка времени
        duration=time_step,  # ВАЖНО: Время жизни точки на экране. Очищает старые кадры!
        add_last_point=False,
        auto_play=True,
        loop=True,
        max_speed=5,
        loop_button=True,
        time_slider_drag_update=True,
        transition_time=50  # ВАЖНО: Скорость анимации. 50 мс = 20 FPS (очень плавно). Было 200.
    ).add_to(m)

    m.save(output_html)
    print(f"Успех! Файл сохранен как: {output_html}")
    print("Откройте этот файл двойным кликом в любом браузере (Chrome, Edge, Safari).")

if __name__ == "__main__":
    create_animation("simulation_trace.csv", "evacuation_map.html")