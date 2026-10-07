using UnityEngine;
using UnityEngine.UI;

public class WorldPrompt : MonoBehaviour
{
    [Header("Префаб буквы E")]
    [Tooltip("Если Prompt UI пуст — создаём из этого префаба")]
    [SerializeField] private GameObject promptPrefab;

    [Header("UI")]
    public RectTransform promptUI;
    public CanvasGroup promptCanvasGroup;
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Hold progress circle")]
    [SerializeField] private Image holdCircle;

    [Header("Camera")]
    [SerializeField] private Camera cam;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 6f;

    private bool isVisible = false;
    private float targetAlpha = 0f;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Если PromptUI пуст — создаём из префаба
        if (promptUI == null && promptPrefab != null)
        {
            Canvas canvas = null;
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c.name == "Canvas") { canvas = c; break; }
            }
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();  // fallback
            if (canvas != null)
            {
                GameObject instance = Instantiate(promptPrefab, canvas.transform);
                instance.transform.localPosition = Vector3.zero;
                promptUI = instance.GetComponent<RectTransform>();
                promptCanvasGroup = instance.GetComponent<CanvasGroup>();

                if (promptCanvasGroup == null)
                    promptCanvasGroup = instance.AddComponent<CanvasGroup>();
            }
            else
            {
                Debug.LogError("[WorldPrompt] Canvas не найден в сцене!");
                return;
            }
        }

        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptUI.gameObject.SetActive(true);
        }

        // Ищем кольцо внутри префаба
        if (promptUI != null && holdCircle == null)
        {
            Transform found = promptUI.Find("HoldCircle");
            if (found != null) holdCircle = found.GetComponent<Image>();
        }

        if (holdCircle != null) holdCircle.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (promptUI == null || promptCanvasGroup == null || cam == null) return;

        promptCanvasGroup.alpha = Mathf.Lerp(
            promptCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * fadeSpeed
        );

        if (promptCanvasGroup.alpha < 0.01f) return;

        Vector3 worldPos = transform.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f) return;

        screenPos.z = 0f;
        promptUI.position = screenPos;
    }

    public void Show()
    {
        isVisible = true;
        targetAlpha = 1f;
    }

    public void Hide()
    {
        isVisible = false;
        targetAlpha = 0f;
        ResetHoldProgress();
    }

    public void SetHoldProgress(float t)
    {
        if (holdCircle == null) return;

        if (!holdCircle.gameObject.activeSelf)
            holdCircle.gameObject.SetActive(true);

        holdCircle.fillAmount = Mathf.Clamp01(t);
    }

    public void ResetHoldProgress()
    {
        if (holdCircle == null) return;
        holdCircle.fillAmount = 0f;
        holdCircle.gameObject.SetActive(false);
    }
}