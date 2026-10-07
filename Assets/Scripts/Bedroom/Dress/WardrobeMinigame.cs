using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeMinigame : MonoBehaviour
{
    public static WardrobeMinigame Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image girlImage;
    [SerializeField] private TextMeshProUGUI requestText;
    [SerializeField] private Transform dressContainer;
    [SerializeField] private GameObject dressCardPrefab;

    [System.Serializable]
    public class DressColorOption
    {
        public string displayName;
        public Color color;
    }

    [System.Serializable]
    public class DressPatternOption
    {
        public string displayName;
        public Sprite pattern;
    }

    [Header("Цвета")]
    [SerializeField] private DressColorOption[] colors;

    [Header("Орнаменты")]
    [SerializeField] private DressPatternOption[] patterns;

    [Header("Настройки")]
    [SerializeField] private int dressCount = 4;

    [Header("Таймер")]    [SerializeField] private float minTime = 4f;
    [SerializeField] private float maxTime = 9f;

    [Header("Награды")]
    [SerializeField] private float sanityGain = 15f;
    [SerializeField] private float lovePenalty = 10f;
    [SerializeField] private float wrongPenalty = 5f;
    [SerializeField] private float timeoutPenalty = 10f;

    [Header("Ссылки")]
    [SerializeField] private PlayerMovement playerMovement;

    private DressColorOption targetColor;
    private DressPatternOption targetPattern;
    private int correctIndex = -1;
    private bool isPlaying = false;
    private float timeLeft = 0f;
    private List<DressCardView> cards = new List<DressCardView>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (!isPlaying) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            Timeout();
            return;
        }

        UpdateRequestText();
    }

    void UpdateRequestText()
    {
        if (requestText == null) return;

        int seconds = Mathf.CeilToInt(timeLeft);
        requestText.text = $"Хочу {targetColor.displayName} платье в {targetPattern.displayName}~\nВремя: {seconds}";
    }

    public void Open()
    {
        if (isPlaying) return;
        isPlaying = true;

        if (playerMovement != null) playerMovement.CanMove = false;
        if (RequestSystem.Instance != null) RequestSystem.Instance.PauseTimer();
        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuOpened();

        if (panel != null) panel.SetActive(true);

        GenerateRequest();
    }

    void GenerateRequest()
    {
        targetColor = colors[Random.Range(0, colors.Length)];
        targetPattern = patterns[Random.Range(0, patterns.Length)];

        timeLeft = Random.Range(minTime, maxTime);
        UpdateRequestText();

        List<(DressColorOption, DressPatternOption)> options = new List<(DressColorOption, DressPatternOption)>();

        options.Add((targetColor, targetPattern));

        int safety = 0;
        while (options.Count < dressCount && safety < 200)
        {
            safety++;

            DressColorOption c = colors[Random.Range(0, colors.Length)];
            DressPatternOption p = patterns[Random.Range(0, patterns.Length)];

            bool duplicate = false;
            foreach (var opt in options)
            {
                if (opt.Item1 == c && opt.Item2 == p) { duplicate = true; break; }
            }

            if (!duplicate)
                options.Add((c, p));
        }

        ShuffleList(options);

        ClearCards();
        correctIndex = -1;

        for (int i = 0; i < options.Count; i++)
        {
            GameObject go = Instantiate(dressCardPrefab, dressContainer);
            DressCardView card = go.GetComponent<DressCardView>();

            if (card == null)
            {
                continue;
            }

            card.Setup(options[i].Item1.color, options[i].Item2.pattern);

            int captured = i;
            card.Button.onClick.RemoveAllListeners();
            card.Button.onClick.AddListener(() => OnDressClicked(captured));

            if (options[i].Item1 == targetColor && options[i].Item2 == targetPattern)
                correctIndex = i;

            cards.Add(card);
        }

    }

    void OnDressClicked(int index)
    {
        if (!isPlaying) return;

        if (index == correctIndex)
            Success();
        else
            WrongAnswer();
    }

    void WrongAnswer()
    {
        isPlaying = false;
        if (panel != null) panel.SetActive(false);
        ClearCards();

        if (GameManager.Instance != null)
            GameManager.Instance.RemoveSanity(wrongPenalty);

        if (NotificationUI.Instance != null)
            NotificationUI.Instance.Show($"Не то! Ты хотела другое платье...");

        EndMinigame();
    }

    void Timeout()
    {
        isPlaying = false;
        if (panel != null) panel.SetActive(false);
        ClearCards();

        if (GameManager.Instance != null)
            GameManager.Instance.RemoveSanity(timeoutPenalty);

        if (NotificationUI.Instance != null)
            NotificationUI.Instance.Show("Ты не успела выбрать...");

        EndMinigame();
    }

    void Success()
    {
        isPlaying = false;
        if (panel != null) panel.SetActive(false);
        ClearCards();

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.AddSanity(sanityGain);
            gm.RemoveLove(lovePenalty);
            gm.RegisterAction();
        }

        if (NotificationUI.Instance != null)
            NotificationUI.Instance.Show("Образ собран. Ты чувствуешь себя лучше.");

        EndMinigame();
    }

    void EndMinigame()
    {
        if (playerMovement != null) playerMovement.CanMove = true;
        if (RequestSystem.Instance != null) RequestSystem.Instance.ResumeTimer();
        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuClosed();
    }

    void ClearCards()
    {
        foreach (var c in cards)
            if (c != null) Destroy(c.gameObject);
        cards.Clear();
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    public bool IsOpen() => isPlaying;
}