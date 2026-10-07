using System.Collections;
using TMPro;
using UnityEngine;

public class DayInfoUI : MonoBehaviour
{
    public static DayInfoUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [SerializeField] private int fallbackDay = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        StartCoroutine(ShowDayRoutine());
    }

    public void ShowCurrentDay()
    {
        StopAllCoroutines();
        StartCoroutine(ShowDayRoutine());
    }

    IEnumerator ShowDayRoutine()
    {
        int day = fallbackDay;
        if (GameManager.Instance != null) day = GameManager.Instance.currentDay;

        if (dayText != null) dayText.text = $"День {day}";
        if (panel != null) panel.SetActive(true);

        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeInDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(holdDuration);

        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeOutDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        if (panel != null) panel.SetActive(false);
    }
}