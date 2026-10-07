using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Любовь")]
    public float loveLevel = 70f;
    public float maxLove = 100f;

    [Header("Рассудок")]
    public float sanityLevel = 70f;
    public float maxSanity = 100f;

    [Header("Прогресс")]
    public int currentDay = 1;
    public int maxDays = 4;

    [Header("Лимит действий за день")]
    public int maxActionsPerDay = 10;
    public int actionsDoneToday = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // === ЛЮБОВЬ ===
    public void AddLove(float amount)
    {
        loveLevel = Mathf.Clamp(loveLevel + amount, 0f, maxLove);
        if (LoveUI.Instance != null) LoveUI.Instance.UpdateUI();
        Debug.Log($"Любовь: {loveLevel}");
        if (LoveFeedback.Instance != null)
    {
        if (amount > 0f)
            LoveFeedback.Instance.ShowPlus();
        else if (amount < 0f)
            LoveFeedback.Instance.ShowMinus();
    }

    }

    public void RemoveLove(float amount) => AddLove(-amount);

    // === РАССУДОК ===
    public void AddSanity(float amount)
    {
        sanityLevel = Mathf.Clamp(sanityLevel + amount, 0f, maxSanity);

        if (SanityUI.Instance != null)
            SanityUI.Instance.UpdateUI();

        if (SanityFeedback.Instance != null)
        {
            if (amount > 0f)
                SanityFeedback.Instance.ShowPlus();
            else if (amount < 0f)
                SanityFeedback.Instance.ShowMinus();
        }

        Debug.Log($"Рассудок: {sanityLevel}");
    }

    public void RemoveSanity(float amount) => AddSanity(-amount);

    // === ДЕЙСТВИЯ ===
    public bool CanDoAction()
    {
        return actionsDoneToday < maxActionsPerDay;
    }

    public void RegisterAction()
    {
        actionsDoneToday++;
        Debug.Log($"Действий за день: {actionsDoneToday}/{maxActionsPerDay}");
    }

    // === ДЕНЬ ===
    public void NextDay()
    {
        currentDay++;
        actionsDoneToday = 0;

        Debug.Log($"=== День {currentDay} ===");

        CheckEndings();
    }

    void CheckEndings()
    {
        
        if (sanityLevel <= 0f)
        {
            if (EndingsManager.Instance != null)
                EndingsManager.Instance.TriggerInsanityEnding();
            return;
        }

        if (loveLevel <= 0f)
        {
            if (EndingsManager.Instance != null)
                EndingsManager.Instance.TriggerLoveEnding();
            return;
        }

        if (currentDay > maxDays)
        {
            if (EndingsManager.Instance != null)
                EndingsManager.Instance.TriggerTrueEnding();
            return;
        }
    }
}