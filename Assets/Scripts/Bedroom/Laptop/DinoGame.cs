using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DinoGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform playArea;
    [SerializeField] private RectTransform dino;
    [SerializeField] private RectTransform cactusPrefab;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Фон (бесконечный скролл)")]
    [SerializeField] private RectTransform backgroundA;
    [SerializeField] private RectTransform backgroundB;
    [SerializeField] private float backgroundSpeed = 300f;

    [Header("Провал")]
    [SerializeField] private GameObject failOverlay;
    [SerializeField] private float failInputDelay = 0.5f; 

    [Header("Настройки игры")]
    [SerializeField] private int pointsToWin = 5;
    [SerializeField] private float gravity = -1800f;
    [SerializeField] private float jumpForce = 750f;
    [SerializeField] private float cactusSpeed = 500f;
    [SerializeField] private float spawnInterval = 1.4f;
    [SerializeField] private float hitDistance = 90f;

    private int score = 0;
    private float dinoVelocity = 0f;
    private bool isRunning = false;
    private bool isJumping = false;
    private float bgSegmentWidth;
    private List<RectTransform> cacti = new List<RectTransform>();
    private Coroutine spawnRoutine;
    private System.Action<bool> onFinish;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (cactusPrefab != null) cactusPrefab.gameObject.SetActive(false);
        if (failOverlay != null) failOverlay.SetActive(false);

        // Запоминаем ширину фона
        if (backgroundA != null)
            bgSegmentWidth = backgroundA.rect.width;
    }

    public void StartGame(System.Action<bool> callback)
    {
        onFinish = callback;

        score = 0;
        dinoVelocity = 0f;
        isRunning = true;
        isJumping = false;

        if (dino != null)
            dino.anchoredPosition = new Vector2(-400f, -100f);

        // Сброс фона
        if (backgroundA != null) backgroundA.anchoredPosition = Vector2.zero;
        if (backgroundB != null) backgroundB.anchoredPosition = new Vector2(bgSegmentWidth, 0);

        ClearCacti();
        UpdateScore();

        if (failOverlay != null) failOverlay.SetActive(false);
        if (panel != null) panel.SetActive(true);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (!isRunning) return;

        // ── Фон (скролл) ──
        ScrollBackground();

        // ── Прыжок ──
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isJumping)
        {
            dinoVelocity = jumpForce;
            isJumping = true;
        }

        // ── Гравитация ──
        dinoVelocity += gravity * Time.deltaTime;
        dino.anchoredPosition += Vector2.up * dinoVelocity * Time.deltaTime;

        // Приземление
        if (dino.anchoredPosition.y <= -100f)
        {
            dino.anchoredPosition = new Vector2(dino.anchoredPosition.x, -100f);
            dinoVelocity = 0f;
            isJumping = false;
        }

        // ── Кактусы ──
        for (int i = cacti.Count - 1; i >= 0; i--)
        {
            RectTransform cactus = cacti[i];
            if (cactus == null) { cacti.RemoveAt(i); continue; }

            cactus.anchoredPosition += Vector2.left * cactusSpeed * Time.deltaTime;

            Vector2 diff = cactus.anchoredPosition - dino.anchoredPosition;
            if (Mathf.Abs(diff.x) < hitDistance && Mathf.Abs(diff.y) < hitDistance)
            {
                Fail();
                return;
            }

            if (cactus.anchoredPosition.x < -700f)
            {
                score++;
                UpdateScore();
                Destroy(cactus.gameObject);
                cacti.RemoveAt(i);

                if (score >= pointsToWin)
                {
                    Win();
                    return;
                }
            }
        }
    }

    // ────────────────────────────────────
    void ScrollBackground()
    {
        if (backgroundA == null || backgroundB == null || bgSegmentWidth <= 0f) return;

        backgroundA.anchoredPosition += Vector2.left * backgroundSpeed * Time.deltaTime;
        backgroundB.anchoredPosition += Vector2.left * backgroundSpeed * Time.deltaTime;

        if (backgroundA.anchoredPosition.x <= -bgSegmentWidth)
            backgroundA.anchoredPosition = new Vector2(backgroundB.anchoredPosition.x + bgSegmentWidth, backgroundA.anchoredPosition.y);

        if (backgroundB.anchoredPosition.x <= -bgSegmentWidth)
            backgroundB.anchoredPosition = new Vector2(backgroundA.anchoredPosition.x + bgSegmentWidth, backgroundB.anchoredPosition.y);
    }

    IEnumerator SpawnLoop()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (!isRunning) yield break;

            RectTransform cactus = Instantiate(cactusPrefab, playArea);
            cactus.gameObject.SetActive(true);
            cactus.anchoredPosition = new Vector2(700f, -100f);
            cacti.Add(cactus);
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = $"{score} / {pointsToWin}";
    }

    void Win()
    {
        Debug.Log("[DinoGame] Win() вызван!");
        isRunning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        ClearCacti();
        if (panel != null) panel.SetActive(false);
        onFinish?.Invoke(true);
        Debug.Log("[DinoGame] onFinish вызван");
    }

    void Fail()
    {
        Debug.Log("[DinoGame] Fail() вызван!");
        isRunning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        ClearCacti();
        if (failOverlay != null) failOverlay.SetActive(true);
        StartCoroutine(WaitAndClose());
    }

    System.Collections.IEnumerator WaitAndClose()
    {
        yield return new WaitForSeconds(failInputDelay);

        // Ждём любого клика / кнопки
        while (!Input.anyKeyDown) yield return null;

        // Скрываем overlay и закрываем
        if (failOverlay != null) failOverlay.SetActive(false);
        if (panel != null) panel.SetActive(false);
        Debug.Log("[DinoGame] Закрываю после провала, вызываю onFinish(false)");

        onFinish?.Invoke(false);
    }

    void ClearCacti()
    {
        foreach (var c in cacti) if (c != null) Destroy(c.gameObject);
        cacti.Clear();
    }
}