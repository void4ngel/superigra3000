using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconImage;             // ← иконка 🚫

    [Header("Настройки")]
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine routine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    /// <summary>
    /// Показать уведомление. showIcon = true — показывает иконку запрета.
    /// </summary>
    public void Show(string message, bool showIcon = false)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(message, showIcon));
    }

    IEnumerator ShowRoutine(string message, bool showIcon)
    {
        Debug.Log($"[Notification] message='{message}', showIcon={showIcon}, iconActive={iconImage?.gameObject.activeSelf}");

        if (text != null) text.text = message;

        // Показываем/скрываем иконку
        if (iconImage != null)
            iconImage.gameObject.SetActive(showIcon);

        if (panel != null) panel.SetActive(true);

        // Появление
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(showDuration);

        // Исчезновение
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        if (iconImage != null)
            iconImage.gameObject.SetActive(false);
        if (panel != null) panel.SetActive(false);
    }
}