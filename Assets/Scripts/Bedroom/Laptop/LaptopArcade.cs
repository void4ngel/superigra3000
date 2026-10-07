using UnityEngine;

public class LaptopArcade : MonoBehaviour
{
    public static LaptopArcade Instance { get; private set; }

    [Header("Мини-игры")]
    [SerializeField] private DinoGame dinoGame;
    [SerializeField] private NyanGame nyanGame;      // ← заменили FlappyGame на NyanGame
    [SerializeField] private CrossyGame crossyGame;   // ← это оставь, но CrossyGame должен иметь StartGame

    [Header("Награды")]
    [SerializeField] private float sanityGain = 15f;
    [SerializeField] private float lovePenalty = 10f;
    [SerializeField] private float partialGain = 5f;

    [Header("Ссылки")]
    [SerializeField] private PlayerMovement playerMovement;

    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Open()
    {
        Debug.Log("[LaptopArcade] Open() вызван");
        if (isPlaying) { Debug.Log("[LaptopArcade] isPlaying == true, выход"); return; }
        isPlaying = true;

        if (playerMovement != null) playerMovement.CanMove = false;
        if (RequestSystem.Instance != null) RequestSystem.Instance.PauseTimer();
        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuOpened();

        int total = 0;
        if (dinoGame != null) total++;
        if (nyanGame != null) total++;
        if (crossyGame != null) total++;
        Debug.Log($"[LaptopArcade] Доступно игр: {total}");

        if (total == 0) { Debug.LogError("[LaptopArcade] Нет игр!"); Close(); return; }

        int pick = Random.Range(0, total);
        Debug.Log($"[LaptopArcade] pick = {pick}");

        if (dinoGame != null)
        {
            if (pick == 0) { Debug.Log("[LaptopArcade] Запуск Dino"); dinoGame.StartGame(OnGameFinished); return; }
            pick--;
        }

        if (nyanGame != null)
        {
            if (pick == 0) { Debug.Log("[LaptopArcade] Запуск Nyan"); nyanGame.StartGame(OnGameFinished); return; }
            pick--;
        }

        if (crossyGame != null)
        {
            Debug.Log("[LaptopArcade] Запуск Crossy");
            crossyGame.StartGame(OnGameFinished);
            return;
        }

        Debug.LogError("[LaptopArcade] Не удалось выбрать игру");
        Close();
    }

    void OnGameFinished(bool success)
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            if (success)
            {
                gm.AddSanity(sanityGain);
                gm.RemoveLove(lovePenalty);
                gm.RegisterAction();
                NotificationUI.Instance?.Show("Отвлеклась от мыслей. Стало легче.");
            }
            else
            {
                gm.AddSanity(partialGain);
                gm.RegisterAction();
                NotificationUI.Instance?.Show("Не выиграла, но отвлеклась.");
            }
        }

        Debug.Log($"[LaptopArcade] OnGameFinished получен. success = {success}");
        Close();
    }

    void Close()
    {
        isPlaying = false;
        if (playerMovement != null) playerMovement.CanMove = true;
        if (RequestSystem.Instance != null) RequestSystem.Instance.ResumeTimer();
        if (CursorManager.Instance != null) CursorManager.Instance.RegisterMenuClosed();
    }

    public bool IsOpen() => isPlaying;
}