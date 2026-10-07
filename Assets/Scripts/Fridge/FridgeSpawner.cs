using System.Collections.Generic;
using UnityEngine;

public class FridgeSpawner : MonoBehaviour
{
    [Header("Слоты для предметов (позиции в холодильнике)")]
    [Tooltip("Создай пустые объекты внутри ItemsContainer и перетащи их сюда")]
    [SerializeField] private Transform[] slots;

    [Header("Настройки")]
    [Tooltip("Сколько максимум предметов показывать одновременно")]
    [SerializeField] private int maxItemsToShow = 6;

    [Tooltip("Оставлять ли слоты пустыми (если предметов меньше, чем слотов)")]
    [SerializeField] private bool allowEmptySlots = true;

    private List<GameObject> spawnedItems = new List<GameObject>();

    /// <summary>
    /// Очистить и заново раскидать предметы по слотам.
    /// Вызывается при открытии холодильника.
    /// </summary>
    public void RespawnItems()
    {
        ClearItems();

        if (ItemSystem.Instance == null)
        {
            Debug.LogWarning("[FridgeSpawner] ItemSystem не найден!");
            return;
        }

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("[FridgeSpawner] Не назначены слоты в инспекторе!");
            return;
        }

        // 1. Получаем все напитки
        var drinks = ItemSystem.Instance.GetAllDrinks();
        if (drinks.Count == 0)
        {
            Debug.Log("[FridgeSpawner] В ItemSystem нет ни одного напитка.");
            return;
        }

        // 2. Перемешиваем напитки
        Shuffle(drinks);

        // 3. Перемешиваем слоты (индексы)
        List<int> slotIndexes = new List<int>();
        for (int i = 0; i < slots.Length; i++) slotIndexes.Add(i);
        Shuffle(slotIndexes);

        // 4. Спавним столько, сколько можем
        int count = Mathf.Min(drinks.Count, slots.Length);
        if (maxItemsToShow > 0) count = Mathf.Min(count, maxItemsToShow);

        for (int i = 0; i < count; i++)
        {
            var item = drinks[i];
            if (item.fridgePrefab == null)
            {
                Debug.LogWarning($"[FridgeSpawner] У '{item.name}' нет fridgePrefab!");
                continue;
            }

            Transform slot = slots[slotIndexes[i]];

            GameObject go = Instantiate(item.fridgePrefab, slot);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            float size = item.fridgeScale > 0 ? item.fridgeScale : 1f;
            go.transform.localScale = Vector3.one * size;

            go.name = "Drink_" + item.name;

            // Коллайдер
            if (go.GetComponent<Collider>() == null)
                go.AddComponent<BoxCollider>();

            // Компонент, по которому FridgeClicker поймёт, что это
            var fridgeItem = go.GetComponent<FridgeItem>();
            if (fridgeItem == null) fridgeItem = go.AddComponent<FridgeItem>();
            fridgeItem.itemName = item.name;

            // Слой для raycast
            go.layer = LayerMask.NameToLayer("Interactable"); // или любой слой, по которому кликаешь

            spawnedItems.Add(go);
        }

        Debug.Log($"[FridgeSpawner] Спавн: {count} напитков в холодильнике.");
    }

    public void ClearItems()
    {
        foreach (var go in spawnedItems)
            if (go != null) Destroy(go);
        spawnedItems.Clear();

        // На всякий случай чистим всё, что осталось
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    /// <summary>
    /// Перемешивание списка (Фишер-Йетс).
    /// </summary>
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}