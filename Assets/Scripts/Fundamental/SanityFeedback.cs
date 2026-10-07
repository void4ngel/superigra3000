using System.Collections;
using UnityEngine;

public class SanityFeedback : MonoBehaviour
{
    public static SanityFeedback Instance { get; private set; }

    [Header("Ссылки")]
    [SerializeField] private CanvasGroup plusGroup;
    [SerializeField] private CanvasGroup minusGroup;

    [Header("Настройки")]
    [SerializeField] private float fadeInTime = 0.15f;
    [SerializeField] private float displayTime = 0.8f;
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private float floatUpDistance = 30f;

    private RectTransform plusRect;
    private RectTransform minusRect;
    private Vector2 plusStartPos;
    private Vector2 minusStartPos;

    private Coroutine plusRoutine;
    private Coroutine minusRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        plusRect = plusGroup.GetComponent<RectTransform>();
        minusRect = minusGroup.GetComponent<RectTransform>();
        plusStartPos = plusRect.anchoredPosition;
        minusStartPos = minusRect.anchoredPosition;

        plusGroup.alpha = 0f;
        minusGroup.alpha = 0f;
    }

    public void ShowPlus()
    {
        if (plusRoutine != null) StopCoroutine(plusRoutine);
        plusRoutine = StartCoroutine(ShowRoutine(plusGroup, plusRect, plusStartPos));
    }

    public void ShowMinus()
    {
        if (minusRoutine != null) StopCoroutine(minusRoutine);
        minusRoutine = StartCoroutine(ShowRoutine(minusGroup, minusRect, minusStartPos));
    }

    IEnumerator ShowRoutine(CanvasGroup group, RectTransform rect, Vector2 startPos)
    {
        rect.anchoredPosition = startPos;
        group.alpha = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeInTime;
            group.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        group.alpha = 1f;

        float totalTime = displayTime + fadeOutTime;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / totalTime;

            float moveT = Mathf.SmoothStep(0f, 1f, t);
            rect.anchoredPosition = startPos + Vector2.up * floatUpDistance * moveT;

            if (t > displayTime / totalTime)
            {
                float fadeT = (t - displayTime / totalTime) / (fadeOutTime / totalTime);
                group.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        group.alpha = 0f;
        rect.anchoredPosition = startPos;
    }
}