using UnityEngine;

public class LaptopInteractable : Interactable
{
    [SerializeField] private LaptopArcade arcade;

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
            NotificationUI.Instance?.Show("Руки заняты!", true);
            return;
        }

        if (arcade != null) arcade.Open();
    }
}