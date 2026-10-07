using System.Collections;
using UnityEngine;

public class Fridge : Interactable
{
    [Header("Дверь")]
    [SerializeField] private FridgeDoor door;

    [Header("Спавнер")]
    [SerializeField] private FridgeSpawner spawner;

    [Header("Мини-игра")]
    [SerializeField] private FridgeMiniGame miniGame;

    [Header("Тайминги")]
    [SerializeField] private float doorOpenDelay = 0.5f;
    [SerializeField] private float doorCloseDelay = 0.5f;

    private bool isBusy = false;

    public override void Interact(GameObject player)
    {
        if (isBusy) return;

        if (ItemSystem.Instance != null && ItemSystem.Instance.HasItem())
        {
            if (NotificationUI.Instance != null)
                NotificationUI.Instance.Show("Руки заняты!", true);
            return;
        }

        StartCoroutine(FridgeRoutine());
    }

    IEnumerator FridgeRoutine()
    {
        isBusy = true;

        // 1. Открываем дверь
        if (door != null) door.Open();

        // 2. Ждём открытия
        yield return new WaitForSeconds(doorOpenDelay);

        // 3. Спавним напитки ЗАНОВО (рандомно)
        if (spawner != null) spawner.RespawnItems();

        // 4. Запускаем мини-игру
        if (miniGame != null) miniGame.Open();

        // 5. Ждём окончания мини-игры
        while (miniGame != null && miniGame.IsPlaying())
            yield return null;

        // 6. Чистим
        if (spawner != null) spawner.ClearItems();

        // 7. Закрываем дверь
        yield return new WaitForSeconds(doorCloseDelay);
        if (door != null) door.Close();

        yield return new WaitForSeconds(0.6f);
        isBusy = false;
    }
}