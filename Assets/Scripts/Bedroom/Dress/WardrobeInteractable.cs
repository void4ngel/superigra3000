using UnityEngine;

public class WardrobeInteractable : Interactable
{
    [SerializeField] private WardrobeMinigame minigame;

    protected override void Start()
    {
        base.Start();
        requiresHold = true;
        holdDuration = 1.5f;
    }

    public override void Interact(GameObject player)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (!gm.CanDoAction())
        {
            NotificationUI.Instance?.Show("Слишком устала. Пора спать.");
            return;
        }

        if (ItemSystem.Instance != null && ItemSystem.Instance.HasItem())
        {
            NotificationUI.Instance?.Show("Сначала отнеси то, что держишь.", true);
            return;
        }

        if (minigame != null) minigame.Open();
    }
}