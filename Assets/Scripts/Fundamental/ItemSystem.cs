using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSystem : MonoBehaviour
{
    public static ItemSystem Instance { get; private set; }

    [Serializable]
    public class ItemEntry
    {
        public string name;
        public string requestPhrase;
        public ItemCategory category;
        public GameObject handPrefab;

        [Header("Для холодильника")]
        public GameObject fridgePrefab;
        public Vector3 fridgeLocalRotation = Vector3.zero;
        public float fridgeScale = 1f;

        [Header("Для запросов")]
        public bool isDrink;
        public bool isFood;
    }

    public enum ItemCategory { Food, Drink }

    [Header("Все предметы")]
    [SerializeField] private List<ItemEntry> items = new List<ItemEntry>();

    [Header("Точка в руке (на подносе или вместо подноса)")]
    [SerializeField] private Transform handSocket;

    [Header("Поднос")]
    [SerializeField] private GameObject trayPrefab;         // префаб подноса
    [SerializeField] private Transform traySocket;          // куда крепить поднос
    [SerializeField] private Vector3 itemOffsetOnTray = new Vector3(0f, 0.05f, 0f);  // предмет поверх подноса

    [Header("Animator игрока")]
    [SerializeField] private Animator playerAnimator;

    private ItemEntry currentItem;
    private GameObject currentTray;   // экземпляр подноса в сцене

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveItem(string itemName)
    {
        ClearItem();

        ItemEntry found = items.Find(i => i.name == itemName);
        if (found == null)
        {
            Debug.LogWarning($"Предмет '{itemName}' не найден!");
            return;
        }

        currentItem = found;

        // 1. Спавним поднос (если ещё нет)
        if (trayPrefab != null && traySocket != null && currentTray == null)
        {
            currentTray = Instantiate(trayPrefab, traySocket);
            currentTray.transform.localPosition = Vector3.zero;
            currentTray.transform.localRotation = Quaternion.identity;
        }

        // 2. Спавним сам предмет на подносе
        if (found.handPrefab != null && traySocket != null)
        {
            GameObject visual = Instantiate(found.handPrefab, traySocket);

            // Берём позицию и поворот ИЗ ПРЕФАБА
            visual.transform.localPosition = found.handPrefab.transform.localPosition;
            visual.transform.localRotation = found.handPrefab.transform.localRotation;
            visual.transform.localScale = found.handPrefab.transform.localScale;
        }

        // 3. Сообщаем аниматору
        if (playerAnimator != null)
            playerAnimator.SetBool("HasItem", true);

        Debug.Log($"В руках: {itemName} (на подносе)");
    }

    public void ClearItem()
    {
        if (traySocket != null)
        {
            for (int i = traySocket.childCount - 1; i >= 0; i--)
                Destroy(traySocket.GetChild(i).gameObject);
        }

        currentTray = null;
        currentItem = null;

        // Сообщаем аниматору
        if (playerAnimator != null)
            playerAnimator.SetBool("HasItem", false);

        Debug.Log("Руки пусты");
    }

    public bool HasItem() => currentItem != null;
    public string GetItemName() => currentItem != null ? currentItem.name : "";
    public ItemCategory? GetItemCategory() => currentItem?.category;

    public List<string> GetAllFoodNames()
    {
        List<string> r = new List<string>();
        foreach (var i in items) if (i.category == ItemCategory.Food) r.Add(i.name);
        return r;
    }

    public List<string> GetAllDrinkNames()
    {
        List<string> r = new List<string>();
        foreach (var i in items) if (i.category == ItemCategory.Drink) r.Add(i.name);
        return r;
    }

    public List<ItemEntry> GetAllFoods()
    {
        List<ItemEntry> r = new List<ItemEntry>();
        foreach (var i in items) if (i.category == ItemCategory.Food) r.Add(i);
        return r;
    }

    public List<ItemEntry> GetAllDrinks()
    {
        List<ItemEntry> r = new List<ItemEntry>();
        foreach (var i in items) if (i.category == ItemCategory.Drink) r.Add(i);
        return r;
    }

    public ItemEntry GetItem(string itemName)
    {
        return items.Find(i => i.name == itemName);
    }

    public string GetRequestPhrase(string itemName)
    {
        ItemEntry found = items.Find(i => i.name == itemName);
        if (found == null) return itemName;
        return string.IsNullOrEmpty(found.requestPhrase) ? found.name : found.requestPhrase;
    }

    public GameObject GetDishPrefab(string itemName)
    {
        ItemEntry found = items.Find(i => i.name == itemName);
        return found?.handPrefab;
    }
}