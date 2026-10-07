using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoveUI : MonoBehaviour
{
    public static LoveUI Instance { get; private set; }

    [Header("Love UI")]
    [SerializeField] private Image loveFill;                // полоска заполнения
    [SerializeField] private TextMeshProUGUI loveText;      // текст "Любовь: 50/100"

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        float current = GameManager.Instance.loveLevel;
        float max = GameManager.Instance.maxLove;
        float fill = Mathf.Clamp01(current / max);

        if (loveFill != null) loveFill.fillAmount = fill;
        if (loveText != null) loveText.text = $"Любовь: {Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
    }
}