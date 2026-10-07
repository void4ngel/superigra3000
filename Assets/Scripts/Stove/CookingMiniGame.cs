using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingMiniGame : MonoBehaviour
{
    [Header("UI мини-игры")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Настройки")]
    [SerializeField] private float timeLimit = 5f;
    [SerializeField] private int sequenceLength = 3;   // длина QTE для ВСЕХ блюд

    [Header("Камера")]
    [SerializeField] private Transform cookingCameraPos;
    [SerializeField] private Transform cookingCameraLookAt;

    [Header("Ссылки")]
    [SerializeField] private PlayerMovement playerMovement;

    // Разрешённые клавиши (исключены F и E)
    private KeyCode[] allowedKeys = new KeyCode[]
    {
        KeyCode.Q, KeyCode.W, KeyCode.R, KeyCode.T, KeyCode.Y,
        KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.G, KeyCode.H,
        KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
        KeyCode.B, KeyCode.N, KeyCode.M
    };

    private KeyCode[] currentSequence;
    private int currentIndex;
    private float timeLeft;
    private bool isPlaying = false;
    private string currentDish;

    void Start()
    {
        panel.SetActive(false);
    }

    public void StartMiniGame(string dishName)
    {
        currentDish = dishName;
        currentSequence = GenerateRandomSequence();
        currentIndex = 0;
        timeLeft = timeLimit;
        isPlaying = true;

        // Блокируем игрока
        if (playerMovement != null) playerMovement.CanMove = false;

        // Пауза таймера запросов
        if (RequestSystem.Instance != null) RequestSystem.Instance.PauseTimer();

        // Плавный перелёт камеры
        if (CameraController.Instance != null && cookingCameraPos != null && cookingCameraLookAt != null)
            CameraController.Instance.MoveTo(cookingCameraPos, cookingCameraLookAt);

        // UI
        panel.SetActive(true);
        instructionText.text = $"Готовим: {dishName}";
        UpdateProgressText();
    }

    /// <summary>
    /// Создаёт случайную последовательность клавиш заданной длины.
    /// Одна и та же длина для всех блюд — настраивается в инспекторе.
    /// </summary>
    KeyCode[] GenerateRandomSequence()
    {
        List<KeyCode> pool = new List<KeyCode>(allowedKeys);

        KeyCode[] result = new KeyCode[sequenceLength];

        for (int i = 0; i < sequenceLength; i++)
        {
            // Перемешиваем пул алгоритмом Фишера-Йетса
            for (int j = pool.Count - 1; j > 0; j--)
            {
                int r = Random.Range(0, j + 1);
                KeyCode tmp = pool[j];
                pool[j] = pool[r];
                pool[r] = tmp;
            }

            // Берём последний элемент (он случайный)
            result[i] = pool[pool.Count - 1];

            // Удаляем из пула, чтобы не было повторов в одной последовательности
            pool.RemoveAt(pool.Count - 1);
        }

        return result;
    }

    void Update()
    {
        if (!isPlaying) return;

        timeLeft -= Time.deltaTime;
        timerText.text = $"Время: {timeLeft:F1}";

        if (timeLeft <= 0f)
        {
            FailCooking();
            return;
        }

        if (Input.anyKeyDown)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return;

            KeyCode pressed = GetPressedKey();
            if (pressed == KeyCode.None) return;

            // Игнорируем F и E, даже если игрок нажал случайно
            if (pressed == KeyCode.F || pressed == KeyCode.E) return;

            if (pressed == currentSequence[currentIndex])
            {
                currentIndex++;
                UpdateProgressText();

                if (currentIndex >= currentSequence.Length)
                    SuccessCooking();
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

    void UpdateProgressText()
    {
        string s = "";
        for (int i = 0; i < currentSequence.Length; i++)
        {
            if (i < currentIndex)
                s += $"[{currentSequence[i]}] ";
            else if (i == currentIndex)
                s += $"<color=yellow>{currentSequence[i]}</color> ";
            else
                s += $"{currentSequence[i]} ";
        }
        progressText.text = s;
    }

    void SuccessCooking()
    {
        isPlaying = false;
        panel.SetActive(false);
        Debug.Log($"Успех! {currentDish} готово!");

        // Кладём блюдо в руку
        if (ItemSystem.Instance != null)
            ItemSystem.Instance.GiveItem(currentDish);

        EndCooking();
    }

    void FailCooking()
    {
        isPlaying = false;
        panel.SetActive(false);
        Debug.Log($"Провал! {currentDish} сгорело...");

        EndCooking();
    }

    void EndCooking()
    {
        // Возвращаем камеру
        if (CameraController.Instance != null) CameraController.Instance.ReturnToInitial();

        // Разблокируем игрока
        if (playerMovement != null) playerMovement.CanMove = true;

        // Возобновляем таймер запросов
        if (RequestSystem.Instance != null) RequestSystem.Instance.ResumeTimer();
    }
}