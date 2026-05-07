import json
import requests

def fetch_roads():
    area_name = "Малиновский, Одесса"
    print(f"Ищем район: {area_name}...")

    # 1. Получаем ID района через Nominatim API
    nom_url = "https://nominatim.openstreetmap.org/search"
    nom_params = {
        "q": area_name,
        "format": "json",
        "limit": 1
    }

    # Обязательный заголовок для серверов OSM
    headers = {"User-Agent": "UrbanEvacuationDataFetcher/1.0"}

    nom_response = requests.get(nom_url, params=nom_params, headers=headers).json()

    if not nom_response:
        print("Район не найден.")
        return

    area_data = nom_response[0]
    osm_id = int(area_data["osm_id"])
    osm_type = area_data["osm_type"]

    # Конвертируем OSM ID в Area ID для Overpass API
    if osm_type == "relation":
        overpass_area_id = osm_id + 3600000000
    elif osm_type == "way":
        overpass_area_id = osm_id + 2400000000
    else:
        print("Неподдерживаемый тип объекта для площади.")
        return

    # 2. Запрос к Overpass API
    overpass_url = "https://overpass-api.de/api/interpreter"  # Изменил на https
    overpass_query = f"""
    [out:json];
    area({overpass_area_id})->.searchArea;
    way["highway"](area.searchArea);
    (._;>;);
    out body;
    """

    print("Загружаем топологию дорог (это может занять несколько секунд)...")

    # ИСПРАВЛЕНИЕ ЗДЕСЬ: Добавлен параметр headers=headers
    response = requests.post(overpass_url, data={"data": overpass_query}, headers=headers)

    if response.status_code != 200:
        print(f"Ошибка сервера Overpass: {response.status_code}")
        print("Ответ сервера:", response.text)  # Поможет, если вдруг ошибка повторится
        return

    data = response.json()
    elements = data.get("elements", [])

    nodes_count = sum(1 for e in elements if e["type"] == "node")
    ways_count = sum(1 for e in elements if e["type"] == "way")

    print(f"Найдено объектов: {len(elements)} (Узлов: {nodes_count}, Дорог: {ways_count}).")

    # 3. Сохранение результата
    output_filename = "map.json"

    with open(output_filename, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"Готово! Полный связный граф успешно сохранен в {output_filename}")


if __name__ == "__main__":
    fetch_roads()