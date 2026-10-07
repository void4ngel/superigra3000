using System.Collections;
using TMPro;
using UnityEngine;

public class RequestUI : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI requestText;

    [Header("Позиции (пиксели)")]
    [SerializeField] private float visibleX = 10f;
    [SerializeField] private float hiddenX = -420f;

    [Header("Скорость")]
    [SerializeField] private float slideDuration = 0.4f;

    private Coroutine routine;
    public static RequestUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        panel.gameObject.SetActive(true);
        panel.anchoredPosition = new Vector2(hiddenX, panel.anchoredPosition.y);
    }

    public void ShowRequest(string text)
    {
        if (requestText != null) requestText.text = text;
        Slide(visibleX);
    }

    public void HideRequest()
    {
        Slide(hiddenX);
    }

    void Slide(float targetX)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SlideRoutine(targetX));
    }

    IEnumerator SlideRoutine(float targetX)
    {
        Vector2 start = panel.anchoredPosition;
        Vector2 target = new Vector2(targetX, start.y);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            panel.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        panel.anchoredPosition = target;
    }
}