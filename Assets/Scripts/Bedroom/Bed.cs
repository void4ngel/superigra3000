using System.Collections;
using UnityEngine;

public class Bed : Interactable
{
    [Header("Ссылки")]
    [SerializeField] private FadeTransition fadeTransition;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Настройки")]
    [SerializeField] private float sleepDelay = 1f;
    [SerializeField] private float afterDayDelay = 0.5f;

    public override void Interact(GameObject player)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.CanDoAction())
        {
            if (NotificationUI.Instance != null)
                NotificationUI.Instance.Show("Ещё рано спать. Парень хочет внимания.");
            return;
        }

        StartCoroutine(SleepRoutine(player));
    }

    IEnumerator SleepRoutine(GameObject player)
    {
        if (playerMovement == null && player != null)
            playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null) playerMovement.CanMove = false;

        yield return new WaitForSeconds(sleepDelay);

        if (RequestSystem.Instance != null)
            RequestSystem.Instance.ForceCompleteCurrentRequest();

        if (fadeTransition != null)
        {
            // Проверяем — это последний день или нет?
            var gm = GameManager.Instance;
            bool isLastDay = gm.currentDay >= gm.maxDays;

            if (isLastDay)
            {
                // ФИНАЛЬНЫЙ переход — экран не открывается
                yield return fadeTransition.PlayFinalFade(() =>
                {
                    gm.NextDay();
                    // Концовка вызовется автоматически из CheckEndings()
                });
            }
            else
            {
                // Обычный переход на новый день
                yield return fadeTransition.PlayTransition(() =>
                {
                    gm.NextDay();

                    if (RequestSystem.Instance != null)
                        RequestSystem.Instance.ResetForNewDay();
                });

                yield return new WaitForSeconds(afterDayDelay);
            }
        }

        if (playerMovement != null) playerMovement.CanMove = true;
    }
}