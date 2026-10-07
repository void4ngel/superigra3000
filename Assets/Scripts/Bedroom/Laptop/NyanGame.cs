using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NyanGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform playArea;
    [SerializeField] private RectTransform player;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject failOverlay;
    [SerializeField] private float failInputDelay = 0.5f;

    [Header("Фон (бесконечный скролл)")]
    [SerializeField] private RectTransform backgroundA;
    [SerializeField] private RectTransform backgroundB;
    [SerializeField] private float backgroundSpeed = 300f;

    [Header("Префабы предметов (собирать)")]
    [SerializeField] private RectTransform[] itemPrefabs;

    [Header("Префабы препятствий (избегать)")]
    [SerializeField] private RectTransform[] obstaclePrefabs;

    [Header("Настройки игрока")]
    [SerializeField] private float playerMoveSpeed = 500f;
    [SerializeField] private float playerStartY = 0f;
    [SerializeField] private float minY = -350f;
    [SerializeField] private float maxY = 350f;
    [SerializeField] private float playerXPos = -400f;

    [Header("Настройки спавна")]
    [SerializeField] private float spawnInterval = 0.9f;
    [SerializeField] private float spawnX = 700f;
    [SerializeField] private float objectSpeed = 500f;

    [Header("Цель")]
    [SerializeField] private int itemsToWin = 10;

    [Header("Столкновение")]
    [SerializeField] private float collisionPadding = 10f;

    private int itemsCollected = 0;
    private bool isRunning = false;
    private float bgSegmentWidth;

    private List<RectTransform> activeItems = new List<RectTransform>();
    private List<RectTransform> activeObstacles = new List<RectTransform>();

    private Coroutine spawnRoutine;
    private System.Action<bool> onFinish;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (failOverlay != null) failOverlay.SetActive(false);

        foreach (var p in itemPrefabs)
            if (p != null) p.gameObject.SetActive(false);

        foreach (var p in obstaclePrefabs)
            if (p != null) p.gameObject.SetActive(false);

        if (backgroundA != null)
            bgSegmentWidth = backgroundA.rect.width;
    }

    public void StartGame(System.Action<bool> callback)
    {
        Debug.Log("[NyanGame] StartGame вызван");

        onFinish = callback;
        isRunning = true;
        itemsCollected = 0;

        if (player == null) { Debug.LogError("[NyanGame] player == null!"); return; }
        if (panel == null) { Debug.LogError("[NyanGame] panel == null!"); return; }

        player.anchoredPosition = new Vector2(playerXPos, playerStartY);

        // Сброс фона
        if (backgroundA != null) backgroundA.anchoredPosition = new Vector2(0, 0);
        if (backgroundB != null) backgroundB.anchoredPosition = new Vector2(bgSegmentWidth, 0);

        ClearAll();

        if (failOverlay != null) failOverlay.SetActive(false);
        panel.SetActive(true);

        UpdateScore();

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (!isRunning) return;

        // ── Скролл фона ──
        ScrollBackground();

        // ── Движение игрока вверх/вниз ──
        float v = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        Vector2 pos = player.anchoredPosition;
        pos.y += v * playerMoveSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.x = playerXPos;
        player.anchoredPosition = pos;

        // ── Прямоугольник игрока (для проверки) ──
        Rect playerRect = GetRect(player);

        // ── Движение предметов ──
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            RectTransform item = activeItems[i];
            if (item == null) { activeItems.RemoveAt(i); continue; }

            item.anchoredPosition += Vector2.left * objectSpeed * Time.deltaTime;

            // Проверка сбора
            if (playerRect.Overlaps(GetRect(item)))
            {
                itemsCollected++;
                UpdateScore();
                Destroy(item.gameObject);
                activeItems.RemoveAt(i);

                if (itemsCollected >= itemsToWin)
                {
                    Win();
                    return;
                }
                continue;
            }

            if (item.anchoredPosition.x < -900f)
            {
                Destroy(item.gameObject);
                activeItems.RemoveAt(i);
            }
        }

        // ── Движение препятствий ──
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            RectTransform obs = activeObstacles[i];
            if (obs == null) { activeObstacles.RemoveAt(i); continue; }

            obs.anchoredPosition += Vector2.left * objectSpeed * Time.deltaTime;

            if (playerRect.Overlaps(GetRect(obs)))
            {
                Debug.Log($"[NyanGame] Столкновение с {obs.gameObject.name}");
                Fail();
                return;
            }

            if (obs.anchoredPosition.x < -900f)
            {
                Destroy(obs.gameObject);
                activeObstacles.RemoveAt(i);
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

    /// <summary>
    /// Возвращает прямоугольник объекта с учётом collisionPadding.
    /// </summary>
    Rect GetRect(RectTransform rt)
    {
        return new Rect(
            rt.anchoredPosition.x - rt.rect.width * 0.5f + collisionPadding,
            rt.anchoredPosition.y - rt.rect.height * 0.5f + collisionPadding,
            rt.rect.width - collisionPadding * 2f,
            rt.rect.height - collisionPadding * 2f
        );
    }

    IEnumerator SpawnLoop()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (!isRunning) yield break;

            SpawnObject();
        }
    }

    void SpawnObject()
    {
        bool spawnItem = Random.value < 0.3f;

        float y = Random.Range(minY, maxY);

        if (spawnItem)
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0) return;

            int idx = Random.Range(0, itemPrefabs.Length);
            RectTransform obj = Instantiate(itemPrefabs[idx], playArea);
            obj.gameObject.SetActive(true);
            obj.anchoredPosition = new Vector2(spawnX, y);
            obj.gameObject.name = "Item_" + idx;

            activeItems.Add(obj);
        }
        else
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

            int idx = Random.Range(0, obstaclePrefabs.Length);
            RectTransform obj = Instantiate(obstaclePrefabs[idx], playArea);
            obj.gameObject.SetActive(true);
            obj.anchoredPosition = new Vector2(spawnX, y);
            obj.gameObject.name = "Obstacle_" + idx;

            activeObstacles.Add(obj);
        }
    }

    void UpdateScore()
    {
        if (progressText != null)
            progressText.text = $"{itemsCollected} / {itemsToWin}";
    }

    void Win()
    {
        Debug.Log("[NyanGame] Win!");
        isRunning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        ClearAll();
        if (panel != null) panel.SetActive(false);
        onFinish?.Invoke(true);
    }

    void Fail()
    {
        isRunning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        if (failOverlay != null)
            failOverlay.transform.SetAsLastSibling();

        if (failOverlay != null) failOverlay.SetActive(true);
        StartCoroutine(WaitAndClose());
    }

    IEnumerator WaitAndClose()
    {
        yield return new WaitForSeconds(failInputDelay);

        while (!Input.anyKeyDown && !Input.GetMouseButtonDown(0))
            yield return null;

        if (failOverlay != null) failOverlay.SetActive(false);
        if (panel != null) panel.SetActive(false);

        ClearAll();
        onFinish?.Invoke(false);
    }

    void ClearAll()
    {
        foreach (var i in activeItems) if (i != null) Destroy(i.gameObject);
        activeItems.Clear();

        foreach (var o in activeObstacles) if (o != null) Destroy(o.gameObject);
        activeObstacles.Clear();
    }

    public bool IsOpen() => isRunning;
}