using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingUI : MonoBehaviour
{
    public static CookingUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private Button dishButtonPrefab;
    [SerializeField] private Button closeButton;

    [Header("Ссылки")]
    [SerializeField] private QTEStoveMiniGame miniGame;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        panel.SetActive(false);

        if (closeButton != null) closeButton.onClick.AddListener(CloseWithReturn);
    }

    void Update()
    {
        if (panel.activeSelf && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            CloseWithReturn();
        }
    }

    public void Open()
    {
        if (panel.activeSelf) return;

        GenerateButtons();
        panel.SetActive(true);
        if (playerMovement != null) playerMovement.CanMove = false;

        if (CursorManager.Instance != null)
            CursorManager.Instance.RegisterMenuOpened();
    }

    public void Close()
    {
        panel.SetActive(false);
        ClearButtons();
        if (playerMovement != null) playerMovement.CanMove = true;

        if (CursorManager.Instance != null)
            CursorManager.Instance.RegisterMenuClosed();
    }

    public void CloseWithReturn()
    {
        Close();

        if (CameraController.Instance != null)
            CameraController.Instance.ReturnToInitial();
    }

    void GenerateButtons()
    {
        ClearButtons();

        if (ItemSystem.Instance == null)
        {
            Debug.LogWarning("[CookingUI] ItemSystem не найден!");
            return;
        }

        if (buttonsContainer == null)
        {
            Debug.LogWarning("[CookingUI] buttonsContainer не назначен!");
            return;
        }

        if (dishButtonPrefab == null)
        {
            Debug.LogWarning("[CookingUI] dishButtonPrefab не назначен!");
            return;
        }

        List<string> foods = ItemSystem.Instance.GetAllFoodNames();
        Debug.Log($"[CookingUI] Найдено блюд: {foods.Count}");

        foreach (string dishName in foods)
        {
            Button btn = Instantiate(dishButtonPrefab, buttonsContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = dishName;

            string captured = dishName;
            btn.onClick.AddListener(() => StartCooking(captured));
        }
    }

    void ClearButtons()
    {
        if (buttonsContainer == null) return;

        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in buttonsContainer)
        {
            if (closeButton != null && child == closeButton.transform) continue;
            toDestroy.Add(child.gameObject);
        }

        foreach (GameObject go in toDestroy)
        {
            go.transform.SetParent(null);
            Destroy(go);
        }
    }

    void StartCooking(string dishName)
    {
        Close();
        miniGame.StartMiniGame(dishName);
    }

    // === Публичные методы для внешнего доступа ===
    public bool IsOpen() => panel != null && panel.activeSelf;
}