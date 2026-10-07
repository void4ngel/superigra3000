using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SanityUI : MonoBehaviour
{
    public static SanityUI Instance { get; private set; }

    [Header("Заполнение")]
    [SerializeField] private Image sanityFill;
    [SerializeField] private TextMeshProUGUI sanityText;

    [Header("Фон (вся панель)")]
    [SerializeField] private Image backgroundImage;

    [Header("Fallback")]
    [SerializeField] private float fallbackCurrent = 70f;
    [SerializeField] private float fallbackMax = 100f;

    private Color fillOriginalColor;
    private Color bgOriginalColor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (sanityFill != null) fillOriginalColor = sanityFill.color;
        if (backgroundImage != null) bgOriginalColor = backgroundImage.color;

        UpdateUI();
    }

    public void UpdateUI()
    {
        float current = GetCurrent();
        float max = GetMax();
        float fill = Mathf.Clamp01(current / max);

        if (sanityFill != null) sanityFill.fillAmount = fill;
        if (sanityText != null)
            sanityText.text = $"Рассудок: {Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";

        ApplyDesaturation(fill);
    }

    void ApplyDesaturation(float ratio)
    {
        // Ниже 0.3 — полностью серый. Выше — плавно от 0 к 1.
        float saturation = Mathf.InverseLerp(0.3f, 1f, ratio);

        if (sanityFill != null)
            sanityFill.color = Desaturate(fillOriginalColor, saturation);

        if (backgroundImage != null)
            backgroundImage.color = Desaturate(bgOriginalColor, saturation);
    }

    Color Desaturate(Color color, float t)
    {
        // Яркость (luminance) — как серый эквивалент
        float gray = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
        Color grayColor = new Color(gray, gray, gray, color.a);

        return Color.Lerp(grayColor, color, t);
    }

    float GetCurrent()
    {
        if (GameManager.Instance != null) return GameManager.Instance.sanityLevel;
        return fallbackCurrent;
    }

    float GetMax()
    {
        if (GameManager.Instance != null) return GameManager.Instance.maxSanity;
        return fallbackMax;
    }
}