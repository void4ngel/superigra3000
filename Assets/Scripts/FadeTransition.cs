using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image blackScreen;
    [SerializeField] private GameObject dayPanel;
    [SerializeField] private TextMeshProUGUI dayText;

    [Header("Тайминги")]
    [SerializeField] private float fadeInDuration = 1f;    // затемнение
    [SerializeField] private float holdDuration = 1.5f;    // держим чёрный экран с текстом
    [SerializeField] private float fadeOutDuration = 1f;   // осветление

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (dayPanel != null)
            dayPanel.SetActive(false);
    }

    /// <summary>
    /// Запускает переход. onMidpoint выполняется, когда экран полностью чёрный.
    /// </summary>
    public IEnumerator PlayTransition(Action onMidpoint)
    {
        canvasGroup.blocksRaycasts = true;

        // 1. Затемнение
        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInDuration));

        // 2. Экран полностью чёрный — выполняем колбэк (NextDay и т.д.)
        onMidpoint?.Invoke();

        // 3. Показываем текст дня
        ShowDay();

        // 4. Держим
        yield return new WaitForSeconds(holdDuration);

        // 5. Скрываем текст дня
        HideDay();

        // 6. Осветление
        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutDuration));

        canvasGroup.blocksRaycasts = false;
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = from;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    void ShowDay()
    {
        if (dayPanel != null) dayPanel.SetActive(true);

        if (dayText != null && GameManager.Instance != null)
            dayText.text = $"День {GameManager.Instance.currentDay}";
    }

    void HideDay()
    {
        if (dayPanel != null) dayPanel.SetActive(false);
    }
    public IEnumerator PlayFinalFade(Action onMidpoint)
    {
        canvasGroup.blocksRaycasts = true;

        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInDuration));

        onMidpoint?.Invoke();

    }
}