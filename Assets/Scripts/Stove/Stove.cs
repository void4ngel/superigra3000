using UnityEngine;

public class Stove : Interactable
{
    public override void Interact(GameObject player)
    {
        // Руки заняты — нельзя готовить
        if (ItemSystem.Instance != null && ItemSystem.Instance.HasItem())
        {
            if (NotificationUI.Instance != null)
                NotificationUI.Instance.Show("Руки заняты!", true);
            return;
        }

        if (CookingUI.Instance != null)
            CookingUI.Instance.Open();
    }
}