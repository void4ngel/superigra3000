using System.Collections;
using UnityEngine;

public class RequestSystem : MonoBehaviour
{
    public static RequestSystem Instance { get; private set; }

    [Header("Таймер запросов")]
    [SerializeField] private float minDelay = 10f;
    [SerializeField] private float maxDelay = 60f;
    [SerializeField] private float timeToComplete = 20f;

    [Header("Возможные запросы")]
    [SerializeField] private string[] possibleRequests = { "Массаж", "Покусать", "Попить", "Поесть" };

    [Header("Награды")]
    [SerializeField] private float rewardForRequest = 12f;
    [SerializeField] private float rewardForInitiative = 2f;
    [SerializeField] private float penaltyForWrong = 2f;
    [SerializeField] private float penaltyForFail = 4f;

    [Header("Штрафы за действия парня (рассудок)")]
    [SerializeField] private float sanityLossForRequest = 10f;

    [Header("Результат (панель)")]
    [SerializeField] private float resultDelay = 0.5f;

    [Header("Ссылки")]
    [SerializeField] private RequestUI requestUI;

    private string currentRequest = null;
    private bool isRequestActive = false;

    private float nextRequestTimer = 0f;
    private float completionTimer = 0f;
    private bool timerPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ScheduleNextRequest();
    }

    void Update()
    {
        if (timerPaused) return;

        if (isRequestActive)
        {
            // Если это "Sleep" — не тикаем таймер, спит игрок сам
            if (currentRequest == "Sleep") return;

            completionTimer -= Time.deltaTime;
            if (completionTimer <= 0f)
                FailRequest();
        }
        else
        {
            nextRequestTimer -= Time.deltaTime;
            if (nextRequestTimer <= 0f)
                GenerateRequest();
        }
    }

    void ScheduleNextRequest()
    {
        nextRequestTimer = Random.Range(minDelay, maxDelay);
    }

    void GenerateRequest()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Если действия кончились — просим спать
        if (!gm.CanDoAction())
        {
            currentRequest = "Sleep";
            isRequestActive = true;

            Debug.Log("Парень: иди спать");
            if (requestUI != null) requestUI.ShowRequest("Ты слишком устала.\nЛожись спать...");
            return;
        }

        // Обычный запрос
        currentRequest = possibleRequests[Random.Range(0, possibleRequests.Length)];
        isRequestActive = true;
        completionTimer = timeToComplete;

        Debug.Log($"Парень хочет: {currentRequest}");
        if (requestUI != null) requestUI.ShowRequest(GetRequestText(currentRequest));
    }

    string GetRequestText(string action)
    {
        if (string.IsNullOrEmpty(action)) return "...";

        if (action.Contains(":"))
        {
            string[] parts = action.Split(':');
            string type = parts[0];
            string itemId = parts[1];

            string phrase = itemId;
            if (ItemSystem.Instance != null)
                phrase = ItemSystem.Instance.GetRequestPhrase(itemId);

            if (type == "Попить") return $"А что у нас есть попить? Принеси {phrase}.";
            if (type == "Поесть") return $"Я голодный :( Положи мне {phrase}.";
        }

        switch (action)
        {
            case "Массаж": return "У меня затекла шея( Вот бы кто нибудь сделал массаж...";
            case "Покусать": return "Иди сюда, я соскучился >:3";
            case "Sleep": return "Ты устала, ложись спать...";
            default: return "...";
        }
    }

    public void PlayerDidAction(string actionName)
    {
        if (isRequestActive)
        {
            if (actionName == currentRequest)
            {
                // ✅ Правильно
                CompleteRequest();
            }
            else
            {
                // ❌ Неправильно — запрос сразу провален
                Debug.Log($"Гошка хотел \"{currentRequest}\", а ты сделала \"{actionName}\". Задание провалено.");

                if (GameManager.Instance != null)
                    GameManager.Instance.RemoveLove(penaltyForWrong);

                isRequestActive = false;
                currentRequest = null;
                ScheduleNextRequest();

                if (requestUI != null) requestUI.HideRequest();

                StartCoroutine(ShowResultDelayed(false));
            }
        }
        else
        {
            // 😊 Инициатива
            Debug.Log($"Ты сделала \"{actionName}\". +{rewardForInitiative}");
            if (GameManager.Instance != null)
                GameManager.Instance.AddLove(rewardForInitiative);
        }
    }

    void CompleteRequest()
    {
        Debug.Log($"Запрос выполнен: {currentRequest}! +{rewardForRequest}, −{sanityLossForRequest} рассудка");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLove(rewardForRequest);
            GameManager.Instance.RemoveSanity(sanityLossForRequest);
        }

        isRequestActive = false;
        currentRequest = null;
        ScheduleNextRequest();
        if (requestUI != null) requestUI.HideRequest();

        StartCoroutine(ShowResultDelayed(true));
    }

    void FailRequest()
    {
        Debug.Log($"Запрос провален: {currentRequest}. -{penaltyForFail}");

        if (GameManager.Instance != null)
            GameManager.Instance.RemoveLove(penaltyForFail);

        isRequestActive = false;
        currentRequest = null;
        ScheduleNextRequest();
        if (requestUI != null) requestUI.HideRequest();

        StartCoroutine(ShowResultDelayed(false));
    }

    IEnumerator ShowResultDelayed(bool success)
    {
        yield return new WaitForSeconds(resultDelay);

        if (TaskResultUI.Instance != null)
            TaskResultUI.Instance.ShowResult(success);
    }

    public void PauseTimer() => timerPaused = true;
    public void ResumeTimer() => timerPaused = false;

    public bool IsRequestActive() => isRequestActive;
    public string GetCurrentRequest() => currentRequest;

    /// <summary>
    /// Полностью сбрасывает текущий запрос без награды и штрафа.
    /// Используется в Bed.cs перед сном.
    /// </summary>
    public void ForceCompleteCurrentRequest()
    {
        isRequestActive = false;
        currentRequest = null;
        if (requestUI != null) requestUI.HideRequest();
    }

    /// <summary>
    /// Вызывается при переходе на новый день (в HeartTransition или Bed).
    /// Сбрасывает всё и планирует первый запрос нового дня.
    /// </summary>
    public void ResetForNewDay()
    {
        Debug.Log("[RequestSystem] Сброс на новый день");

        // Останавливаем всё
        isRequestActive = false;
        currentRequest = null;
        completionTimer = 0f;
        nextRequestTimer = 0f;

        // Скрываем UI запроса
        if (requestUI != null) requestUI.HideRequest();

        // Снимаем паузу, если была
        timerPaused = false;

        // Планируем следующий запрос
        ScheduleNextRequest();
    }
}