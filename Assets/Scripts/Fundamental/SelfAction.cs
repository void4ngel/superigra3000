using UnityEngine;

public class SelfAction : Interactable
{
    [Header("Что даёт действие")]
    [SerializeField] private float sanityGain = 15f;
    [SerializeField] private float lovePenalty = 10f;

    [Header("Идентификатор (для мини-игр)")]
    [SerializeField] private string actionId = "walk";

    [Header("Тексты уведомлений")]
    [SerializeField] private string actionName = "Прогулка";
    [SerializeField] private string successMessage = "Ты немного отдохнула";

    [Header("Требует расходник в руках (еда ИЛИ питьё)")]
    [Tooltip("Действие возможно только когда игрок держит еду или напиток")]
    [SerializeField] private bool requiresConsumable = false;

    [Header("Сообщения")]
    [SerializeField] private string noConsumableMessage = "Сначала возьми еду или напиток!";
    [SerializeField] private string handsBusyMessage = "Сначала отнеси то, что держишь.";

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
            if (NotificationUI.Instance != null)
                NotificationUI.Instance.Show("Слишком устала. Пора спать");
            return;
        }

        if (requiresConsumable)
        {
            if (!HasConsumable())
            {
                if (NotificationUI.Instance != null)
                    NotificationUI.Instance.Show(noConsumableMessage, true);
                return;
            }
        }
        else
        {
            if (ItemSystem.Instance != null && ItemSystem.Instance.HasItem())
            {
                if (NotificationUI.Instance != null)
                    NotificationUI.Instance.Show(handsBusyMessage, true);
                return;
            }
        }

        StartSelfAction();
    }

    bool HasConsumable()
    {
        if (ItemSystem.Instance == null) return false;
        if (!ItemSystem.Instance.HasItem()) return false;

        var cat = ItemSystem.Instance.GetItemCategory();
        return cat == ItemSystem.ItemCategory.Food || cat == ItemSystem.ItemCategory.Drink;
    }

    void StartSelfAction()
    {
        var gm = GameManager.Instance;

        if (requiresConsumable && ItemSystem.Instance != null)
            ItemSystem.Instance.ClearItem();

        gm.AddSanity(sanityGain);
        gm.RemoveLove(lovePenalty);
        gm.RegisterAction();

        Debug.Log($"[SelfAction] {actionName}: +{sanityGain} рассудка, −{lovePenalty} любви");

        if (NotificationUI.Instance != null)
            NotificationUI.Instance.Show(successMessage);
    }
}