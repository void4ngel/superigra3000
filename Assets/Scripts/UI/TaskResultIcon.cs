using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskResultUI : MonoBehaviour
{
    public static TaskResultUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Sprites")]
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite sadSprite;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float displayTime = 2f;
    [SerializeField] private float hiddenX = 400f;
    [SerializeField] private float visibleX = 0f;

    private Coroutine routine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Vector2 pos = panel.anchoredPosition;
        pos.x = hiddenX;
        panel.anchoredPosition = pos;
        canvasGroup.alpha = 0f;
        panel.gameObject.SetActive(true);
    }

    public void ShowResult(bool success)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(success));
    }

    IEnumerator ShowRoutine(bool success)
    {
        // Устанавливаем внешний вид
        icon.sprite = success ? happySprite : sadSprite;
        text.text = success ? "Задание выполнено!" : "Задание провалено...";
        canvasGroup.alpha = 1f;

        // Выезд
        yield return SlideRoutine(hiddenX, visibleX);

        // Пауза
        yield return new WaitForSeconds(displayTime);

        // Уезд
        yield return SlideRoutine(visibleX, hiddenX);
        canvasGroup.alpha = 0f;
    }

    IEnumerator SlideRoutine(float fromX, float toX)
    {
        Vector2 start = new Vector2(fromX, panel.anchoredPosition.y);
        Vector2 target = new Vector2(toX, panel.anchoredPosition.y);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            panel.anchoredPosition = Vector2.Lerp(start, target, smooth);
            yield return null;
        }

        panel.anchoredPosition = target;
    }
}