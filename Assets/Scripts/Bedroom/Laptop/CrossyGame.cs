using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrossyGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform playArea;
    [SerializeField] private RectTransform player;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject failOverlay;
    [SerializeField] private float failInputDelay = 0.5f;

    [Header("Y-координаты дорог")]
    [SerializeField] private float[] laneYPositions = { -250f, -50f, 150f, 350f };
    [SerializeField] private bool[] laneDirectionsRight = { true, false, true, false };

    [Header("Префабы машин")]
    [SerializeField] private RectTransform[] carPrefabs;

    [Header("Настройки игрока")]
    [SerializeField] private float playerMoveSpeed = 400f;
    [SerializeField] private float startY = -400f;
    [SerializeField] private float finishY = 450f;
    [SerializeField] private float minX = -500f;
    [SerializeField] private float maxX = 500f;

    [Header("Настройки машин")]
    [SerializeField] private float carSpeed = 400f;
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private float carSpawnX = 800f;

    [Header("Столкновение")]
    [Tooltip("Внутренний отступ хитбокса. Чем больше — тем «прощающе».")]
    [SerializeField] private float collisionPadding = 15f;

    private bool isRunning = false;
    private List<RectTransform> activeCars = new List<RectTransform>();
    private Coroutine spawnRoutine;
    private System.Action<bool> onFinish;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (failOverlay != null) failOverlay.SetActive(false);

        foreach (var car in carPrefabs)
            if (car != null) car.gameObject.SetActive(false);
    }

    public void StartGame(System.Action<bool> callback)
    {
        Debug.Log("[CrossyGame] StartGame вызван");

        onFinish = callback;
        isRunning = true;

        if (player == null) { Debug.LogError("[CrossyGame] player == null!"); return; }
        if (panel == null) { Debug.LogError("[CrossyGame] panel == null!"); return; }
        if (playArea == null) { Debug.LogError("[CrossyGame] playArea == null!"); return; }
        if (carPrefabs == null || carPrefabs.Length == 0) { Debug.LogError("[CrossyGame] нет префабов машин!"); return; }

        player.anchoredPosition = new Vector2(0f, startY);
        ClearCars();

        if (failOverlay != null) failOverlay.SetActive(false);
        panel.SetActive(true);

        Debug.Log("[CrossyGame] Панель включена");

        if (progressText != null)
            progressText.text = "Reach the TOP!";

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (!isRunning) return;

        // ── Движение игрока ──
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;

        Vector2 move = new Vector2(h, v).normalized;

        Vector2 pos = player.anchoredPosition;
        pos += move * playerMoveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        player.anchoredPosition = pos;

        if (pos.y >= finishY)
        {
            Win();
            return;
        }

        // ── Движение машин + столкновение ──
        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            RectTransform car = activeCars[i];
            if (car == null) { activeCars.RemoveAt(i); continue; }

            bool movingRight = car.gameObject.name.EndsWith("_R");
            Vector2 dir = movingRight ? Vector2.right : Vector2.left;

            car.anchoredPosition += dir * carSpeed * Time.deltaTime;

            // ── Столкновение с внутренним отступом ──
            Rect playerRect = new Rect(
                player.anchoredPosition.x - player.rect.width * 0.5f + collisionPadding,
                player.anchoredPosition.y - player.rect.height * 0.5f + collisionPadding,
                player.rect.width - collisionPadding * 2f,
                player.rect.height - collisionPadding * 2f
            );

            Rect carRect = new Rect(
                car.anchoredPosition.x - car.rect.width * 0.5f + collisionPadding,
                car.anchoredPosition.y - car.rect.height * 0.5f + collisionPadding,
                car.rect.width - collisionPadding * 2f,
                car.rect.height - collisionPadding * 2f
            );

            if (playerRect.Overlaps(carRect))
            {
                Debug.Log($"[CrossyGame] Столкновение. Player={player.anchoredPosition}, Car={car.anchoredPosition}");
                Fail();
                return;
            }

            // Удаление за границей
            if (movingRight && car.anchoredPosition.x > 900f)
            {
                Destroy(car.gameObject);
                activeCars.RemoveAt(i);
            }
            else if (!movingRight && car.anchoredPosition.x < -900f)
            {
                Destroy(car.gameObject);
                activeCars.RemoveAt(i);
            }
        }
    }

    IEnumerator SpawnLoop()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (!isRunning) yield break;

            SpawnCar();
        }
    }

    void SpawnCar()
    {
        if (carPrefabs == null || carPrefabs.Length == 0) return;

        int lane = Random.Range(0, laneYPositions.Length);
        float y = laneYPositions[lane];

        bool movingRight = lane < laneDirectionsRight.Length ? laneDirectionsRight[lane] : true;

        int carIndex = Random.Range(0, carPrefabs.Length);
        RectTransform car = Instantiate(carPrefabs[carIndex], playArea);
        car.gameObject.SetActive(true);

        float spawnX = movingRight ? -carSpawnX : carSpawnX;
        car.anchoredPosition = new Vector2(spawnX, y);

        car.gameObject.name = movingRight ? "Car_R" : "Car_L";

        Vector3 scale = car.localScale;
        scale.x = movingRight ? 1f : -1f;
        car.localScale = scale;

        activeCars.Add(car);

        Debug.Log($"[CrossyGame] Спавн. Lane={lane}, Y={y}, SpawnX={spawnX}, Dir={(movingRight ? "R" : "L")}");
    }

    void Win()
    {
        Debug.Log("[CrossyGame] Win!");
        isRunning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        ClearCars();
        if (panel != null) panel.SetActive(false);
        onFinish?.Invoke(true);
    }

    void Fail()
    {
        isRunning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

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

        onFinish?.Invoke(false);
    }

    void ClearCars()
    {
        foreach (var c in activeCars) if (c != null) Destroy(c.gameObject);
        activeCars.Clear();
    }

    public bool IsOpen() => isRunning;
}