using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionMenuUI : MonoBehaviour
{
    public static InteractionMenuUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button massageButton;
    [SerializeField] private Button biteButton;
    [SerializeField] private Button drinkButton;
    [SerializeField] private Button foodButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Кулдаун на действия (секунды)")]
    [SerializeField] private float actionCooldown = 30f;

    private Dictionary<string, float> actionAvailableTime = new Dictionary<string, float>();
    private Dictionary<Button, string> originalTexts = new Dictionary<Button, string>();

    private Boyfriend currentBoyfriend;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        actionAvailableTime["Массаж"] = 0f;
        actionAvailableTime["Покусать"] = 0f;
        actionAvailableTime["Попить"] = 0f;
        actionAvailableTime["Поесть"] = 0f;
    }

    void Start()
    {
        panel.SetActive(false);

        massageButton.onClick.AddListener(() => DoAction("Массаж"));
        biteButton.onClick.AddListener(() => DoAction("Покусать"));
        drinkButton.onClick.AddListener(() => DoAction("Попить"));
        foodButton.onClick.AddListener(() => DoAction("Поесть"));

        if (closeButton != null) closeButton.onClick.AddListener(Close);

        CacheOriginalText(massageButton);
        CacheOriginalText(biteButton);
        CacheOriginalText(drinkButton);
        CacheOriginalText(foodButton);
    }

    void CacheOriginalText(Button btn)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            originalTexts[btn] = tmp.text;
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        UpdateButtonStates();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            Close();
    }

    public void Open()
    {
        if (panel.activeSelf) return;

        UpdateButtonStates();
        panel.SetActive(true);
        if (playerMovement != null) playerMovement.CanMove = false;

        if (CursorManager.Instance != null)
            CursorManager.Instance.RegisterMenuOpened();
    }

    public void OpenForBoyfriend(Boyfriend bf)
    {
        currentBoyfriend = bf;
        Open();
    }

    public void Close()
    {
        panel.SetActive(false);
        if (playerMovement != null) playerMovement.CanMove = true;

        if (currentBoyfriend != null)
        {
            currentBoyfriend.ReturnCamera();
            currentBoyfriend = null;
        }
        else
        {
            if (CameraController.Instance != null)
                CameraController.Instance.ReturnToInitial();
        }

        if (CursorManager.Instance != null)
            CursorManager.Instance.RegisterMenuClosed();
    }

    void UpdateButtonStates()
    {
        bool hasFood = false;
        bool hasDrink = false;

        if (ItemSystem.Instance != null && ItemSystem.Instance.HasItem())
        {
            var cat = ItemSystem.Instance.GetItemCategory();
            if (cat == ItemSystem.ItemCategory.Food) hasFood = true;
            if (cat == ItemSystem.ItemCategory.Drink) hasDrink = true;
        }

        UpdateButton(massageButton, "Массаж", true);
        UpdateButton(biteButton, "Покусать", true);
        UpdateButton(foodButton, "Поесть", hasFood);
        UpdateButton(drinkButton, "Попить", hasDrink);
    }

    void UpdateButton(Button btn, string actionName, bool baseReady)
    {
        if (btn == null) return;

        bool isCurrentRequest = IsActionCurrentRequest(actionName);
        bool onCooldown = !isCurrentRequest && !IsActionReady(actionName);
        bool active = baseReady && (isCurrentRequest || !onCooldown);

        btn.interactable = active;

        ColorBlock colors = btn.colors;
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        btn.colors = colors;

        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null) return;

        string original = originalTexts.ContainsKey(btn) ? originalTexts[btn] : actionName;

        if (onCooldown)
        {
            float remaining = actionAvailableTime[actionName] - Time.time;
            tmp.text = $"{original} ({Mathf.CeilToInt(remaining)}с)";
        }
        else
        {
            tmp.text = original;
        }
    }

    bool IsActionCurrentRequest(string actionName)
    {
        if (RequestSystem.Instance == null) return false;
        if (!RequestSystem.Instance.IsRequestActive()) return false;

        string req = RequestSystem.Instance.GetCurrentRequest();
        if (string.IsNullOrEmpty(req)) return false;

        if (req == actionName) return true;
        if (req.StartsWith(actionName + ":")) return true;

        return false;
    }

    bool IsActionReady(string actionName)
    {
        if (IsActionCurrentRequest(actionName)) return true;

        if (!actionAvailableTime.ContainsKey(actionName)) return true;
        return Time.time >= actionAvailableTime[actionName];
    }

    void DoAction(string actionName)
    {
        if (!IsActionReady(actionName))
        {
            Debug.Log($"Действие '{actionName}' ещё на кулдауне.");
            return;
        }

        // Проверка лимита
        var gm = GameManager.Instance;
        if (gm != null && !gm.CanDoAction())
        {
            if (NotificationUI.Instance != null)
                NotificationUI.Instance.Show("Слишком устала. Пора спать.");
            return;
        }

        Debug.Log($"Игрок выбрал: {actionName}");

        string finalAction = actionName;
        if ((actionName == "Попить" || actionName == "Поесть")
            && ItemSystem.Instance != null
            && ItemSystem.Instance.HasItem())
        {
            finalAction = actionName + ":" + ItemSystem.Instance.GetItemName();
        }

        bool wasRequest = IsActionCurrentRequest(actionName);

        if ((actionName == "Поесть" || actionName == "Попить") && ItemSystem.Instance != null)
            ItemSystem.Instance.ClearItem();

        if (RequestSystem.Instance != null)
            RequestSystem.Instance.PlayerDidAction(finalAction);

        if (gm != null) gm.RegisterAction();

        if (!wasRequest)
            actionAvailableTime[actionName] = Time.time + actionCooldown;

        Close();
    }

    public bool IsOpen() => panel != null && panel.activeSelf;
}