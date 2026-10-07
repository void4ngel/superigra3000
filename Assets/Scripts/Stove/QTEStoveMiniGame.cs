using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTEStoveMiniGame : MonoBehaviour
{
    public static QTEStoveMiniGame Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI letterText;
    [SerializeField] private Image letterCircle;

    [Header("Настройки")]
    [SerializeField] private int lettersCount = 5;
    [SerializeField] private float timePerLetter = 2f;

    [Header("Камера")]
    [SerializeField] private Transform cookingCameraPos;
    [SerializeField] private Transform cookingCameraLookAt;

    [Header("Ссылки")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Анимация готовки")]
    [SerializeField] private CookingAnimationController cookingAnimation;

    private KeyCode[] allowedKeys = new KeyCode[]
    {
        KeyCode.Q, KeyCode.W, KeyCode.R, KeyCode.T, KeyCode.Y,
        KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.G, KeyCode.H,
        KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
        KeyCode.B, KeyCode.N, KeyCode.M
    };

    private List<KeyCode> sequence = new List<KeyCode>();
    private int currentIndex;
    private float letterTimer;

    private bool isPlaying = false;          // QTE идёт (буквы)
    private bool isSequenceRunning = false;  // весь процесс: QTE + анимация

    private string currentDish;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void StartMiniGame(string dishName)
    {
        if (isPlaying || isSequenceRunning) return;

        currentDish = dishName;

        // Генерируем случайную последовательность
        sequence.Clear();
        List<KeyCode> pool = new List<KeyCode>(allowedKeys);
        for (int i = 0; i < lettersCount; i++)
        {
            int r = Random.Range(0, pool.Count);
            sequence.Add(pool[r]);
            pool.RemoveAt(r);
        }

        currentIndex = 0;
        letterTimer = timePerLetter;
        isPlaying = true;
        isSequenceRunning = true;

        // Блокируем игрока и паузим запросы
        if (playerMovement != null) playerMovement.CanMove = false;
        if (RequestSystem.Instance != null) RequestSystem.Instance.PauseTimer();

        // Камера к плите
        if (CameraController.Instance != null && cookingCameraPos != null && cookingCameraLookAt != null)
            CameraController.Instance.MoveTo(cookingCameraPos, cookingCameraLookAt);

        // Курсор
        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuOpened();

        // UI
        if (panel != null) panel.SetActive(true);
        if (instructionText != null) instructionText.text = $"Готовим: {dishName}";
        ShowCurrentLetter();
    }

    void ShowCurrentLetter()
    {
        if (currentIndex >= sequence.Count) return;
        if (letterText != null) letterText.text = sequence[currentIndex].ToString();
        letterTimer = timePerLetter;
        if (letterCircle != null) letterCircle.fillAmount = 1f;
    }

    void Update()
    {
        if (!isPlaying) return;

        // Таймер на букву
        letterTimer -= Time.deltaTime;
        float fill = letterTimer / timePerLetter;
        if (letterCircle != null) letterCircle.fillAmount = Mathf.Clamp01(fill);

        // Время вышло — провал
        if (letterTimer <= 0f)
        {
            FailCooking();
            return;
        }

        // Проверка нажатия
        if (Input.anyKeyDown)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return;

            KeyCode pressed = GetPressedKey();
            if (pressed == KeyCode.None) return;
            if (pressed == KeyCode.F || pressed == KeyCode.E) return;

            if (pressed == sequence[currentIndex])
            {
                currentIndex++;

                if (currentIndex >= sequence.Count)
                    SuccessCooking();
                else
                    ShowCurrentLetter();
            }
            else
            {
                FailCooking();
            }
        }
    }

    KeyCode GetPressedKey()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            if (Input.GetKeyDown(key)) return key;
        return KeyCode.None;
    }

    void SuccessCooking()
    {
        isPlaying = false;
        // isSequenceRunning остаётся true — буква E будет скрыта до конца анимации

        if (panel != null) panel.SetActive(false);
        Debug.Log($"QTE пройден! {currentDish} — запускается анимация.");

        if (cookingAnimation != null)
        {
            cookingAnimation.PlayCookingAnimation(currentDish, () =>
            {
                GiveDishAndFinish();
            });
        }
        else
        {
            GiveDishAndFinish();
        }
    }

    void GiveDishAndFinish()
    {
        Debug.Log($"Успех! {currentDish} готово!");

        if (ItemSystem.Instance != null)
            ItemSystem.Instance.GiveItem(currentDish);

        EndCooking();
    }

    void FailCooking()
    {
        isPlaying = false;
        if (panel != null) panel.SetActive(false);
        Debug.Log($"Провал! {currentDish} сгорело...");

        EndCooking();
    }

    void EndCooking()
    {
        isPlaying = false;
        isSequenceRunning = false;  // ← сброс в самом конце

        if (CameraController.Instance != null) CameraController.Instance.ReturnToInitial();
        if (playerMovement != null) playerMovement.CanMove = true;
        if (RequestSystem.Instance != null) RequestSystem.Instance.ResumeTimer();
        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuClosed();
    }

    // === Публичные методы ===
    public bool IsOpen() => isSequenceRunning;   // для скрытия буквы E
    public bool IsPlaying() => isPlaying;        // для внутренних проверок
}