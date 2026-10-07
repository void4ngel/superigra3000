using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartTransition : MonoBehaviour
{
    public static HeartTransition Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform heartsContainer;
    [SerializeField] private Image heartPrefab;
    [SerializeField] private GameObject dayAnnouncePanel;     // панель с текстом дня
    [SerializeField] private TextMeshProUGUI dayAnnounceText;

    [Header("Настройки сердечек")]
    [SerializeField] private int heartCount = 40;              // сколько сердечек заспавнить
    [SerializeField] private float minHeartSize = 60f;
    [SerializeField] private float maxHeartSize = 180f;

    [Header("Тайминги")]
    [SerializeField] private float flyInDuration = 0.8f;
    [SerializeField] private float holdDuration = 1.5f;        // сколько держим закрытый экран
    [SerializeField] private float flyOutDuration = 0.7f;

    private List<RectTransform> hearts = new List<RectTransform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (dayAnnouncePanel != null)
            dayAnnouncePanel.SetActive(false);
    }

    public IEnumerator PlayTransition(Action onMidpoint)
    {
        // 1. Показываем канвас
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 2. Спавним сердечки и запускаем их вылет
        SpawnHearts();
        yield return StartCoroutine(FlyIn());

        // 3. Экран закрыт — выполняем колбэк (тут — NextDay + показ дня)
        onMidpoint?.Invoke();

        // 4. Показываем день
        ShowDayAnnouncement();

        // 5. Ждём
        yield return new WaitForSeconds(holdDuration);

        // 6. Скрываем день
        HideDayAnnouncement();

        // 7. Сердечки улетают вправо
        yield return StartCoroutine(FlyOut());

        // 8. Чистим
        ClearHearts();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    void SpawnHearts()
    {
        ClearHearts();

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        for (int i = 0; i < heartCount; i++)
        {
            Image heartImg = Instantiate(heartPrefab, heartsContainer);
            RectTransform rt = heartImg.GetComponent<RectTransform>();

            float size = UnityEngine.Random.Range(minHeartSize, maxHeartSize);
            rt.sizeDelta = new Vector2(size, size);

            // Разброс по всей области за верхним-левым краем
            float x = UnityEngine.Random.Range(-screenSize.x, screenSize.x * 0.2f);
            float y = UnityEngine.Random.Range(screenSize.y * 0.4f, screenSize.y * 1.2f);
            rt.anchoredPosition = new Vector2(x, y);

            rt.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30f, 30f));

            // Случайный розовый
            heartImg.color = new Color(
                UnityEngine.Random.Range(0.9f, 1f),
                UnityEngine.Random.Range(0.4f, 0.7f),
                UnityEngine.Random.Range(0.6f, 0.85f),
                1f
            );

            hearts.Add(rt);
        }
    }

    IEnumerator FlyIn()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        List<Vector2> startPositions = new List<Vector2>();
        List<Vector2> targetPositions = new List<Vector2>();

        for (int i = 0; i < hearts.Count; i++)
        {
            startPositions.Add(hearts[i].anchoredPosition);

            float x = UnityEngine.Random.Range(-screenSize.x * 0.1f, screenSize.x * 0.7f);
            float y = UnityEngine.Random.Range(-screenSize.y * 0.7f, screenSize.y * 0.5f);
            targetPositions.Add(new Vector2(x, y));
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyInDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < hearts.Count; i++)
                hearts[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], smooth);

            yield return null;
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            float x = (i % 8 - 4) * screenSize.x / 8f + UnityEngine.Random.Range(-40f, 40f);
            float y = (i / 8 - 2) * screenSize.y / 5f + UnityEngine.Random.Range(-40f, 40f);
            hearts[i].anchoredPosition = new Vector2(x, y);
        }
    }

    IEnumerator FlyOut()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 rightOut = new Vector2(screenSize.x * 1.5f, 0f);

        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < hearts.Count; i++)
            startPositions.Add(hearts[i].anchoredPosition);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyOutDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < hearts.Count; i++)
            {
                Vector2 target = new Vector2(rightOut.x + UnityEngine.Random.Range(-200f, 200f),
                                             startPositions[i].y + UnityEngine.Random.Range(-200f, 200f));
                hearts[i].anchoredPosition = Vector2.Lerp(startPositions[i], target, smooth);

                Image img = hearts[i].GetComponent<Image>();
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, smooth);
                img.color = c;
            }

            yield return null;
        }
    }

    void ShowDayAnnouncement()
    {
        if (dayAnnouncePanel == null) return;

        dayAnnouncePanel.SetActive(true);

        if (dayAnnounceText != null && GameManager.Instance != null)
            dayAnnounceText.text = $"День {GameManager.Instance.currentDay}";
    }

    void HideDayAnnouncement()
    {
        if (dayAnnouncePanel != null)
            dayAnnouncePanel.SetActive(false);
    }

    void ClearHearts()
    {
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i] != null)
                Destroy(hearts[i].gameObject);
        }
        hearts.Clear();
    }
}