using UnityEngine;
using TMPro;

public class FridgeMiniGame : MonoBehaviour
{
    public static FridgeMiniGame Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Настройки")]
    [SerializeField] private float timeLimit = 15f;

    [Header("Камера")]
    [SerializeField] private Transform fridgeCameraPos;
    [SerializeField] private Transform fridgeCameraLookAt;

    [Header("Ссылки")]
    [SerializeField] private PlayerMovement playerMovement;

    private string targetItem;
    private float timeLeft;
    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Open()
    {
        if (isPlaying) return;  // уже открыто

        bool hasDrinkRequest = false;

        if (RequestSystem.Instance != null && RequestSystem.Instance.IsRequestActive())
        {
            string requested = RequestSystem.Instance.GetCurrentRequest();
            if (!string.IsNullOrEmpty(requested) && requested.StartsWith("Попить:"))
            {
                targetItem = requested.Split(':')[1];
                hasDrinkRequest = true;
                Debug.Log($"[Fridge] Есть запрос парня на: {targetItem}");
            }
        }

        if (!hasDrinkRequest)
        {
            if (ItemSystem.Instance != null)
            {
                var drinkNames = ItemSystem.Instance.GetAllDrinkNames();
                if (drinkNames.Count > 0)
                    targetItem = drinkNames[Random.Range(0, drinkNames.Count)];
                else
                    targetItem = "Кола";
            }
            Debug.Log($"[Fridge] Случайный напиток: {targetItem}");
        }

        timeLeft = timeLimit;
        isPlaying = true;

        if (playerMovement != null) playerMovement.CanMove = false;
        if (RequestSystem.Instance != null) RequestSystem.Instance.PauseTimer();

        if (CameraController.Instance != null && fridgeCameraPos != null && fridgeCameraLookAt != null)
            CameraController.Instance.MoveTo(fridgeCameraPos, fridgeCameraLookAt);

        if (panel != null) panel.SetActive(true);
        if (instructionText != null) instructionText.text = $"Найди: {targetItem}";

        isPlaying = true;

        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuOpened();

    }

    void Update()
    {
        if (!isPlaying) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            Debug.Log("Время вышло!");
            FailSearch();
        }
    }

    public void OnItemClicked(string itemName)
    {
        if (!isPlaying) return;

        Debug.Log($"Кликнули по: {itemName}, нужно: {targetItem}");

        if (itemName == targetItem)
            SuccessSearch();
        else
            FailSearch();
    }

    void SuccessSearch()
    {
        isPlaying = false;
        if (panel != null) panel.SetActive(false);
        Debug.Log($"Найдено: {targetItem}");

        if (ItemSystem.Instance != null)
            ItemSystem.Instance.GiveItem(targetItem);

        EndMiniGame();
    }

    void FailSearch()
    {
        isPlaying = false;
        if (panel != null) panel.SetActive(false);
        Debug.Log("Не нашёл...");

        EndMiniGame();
    }

    void EndMiniGame()
    {
        if (CameraController.Instance != null) CameraController.Instance.ReturnToInitial();
        if (playerMovement != null) playerMovement.CanMove = true;
        if (RequestSystem.Instance != null) RequestSystem.Instance.ResumeTimer();

        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuClosed();
    }

    public bool IsOpen() => isPlaying;
    public bool IsPlaying() => isPlaying;
}