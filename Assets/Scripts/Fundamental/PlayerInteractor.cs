using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private Interactable currentInteractable;
    private float currentHoldTime = 0f;

    private bool actionExecutedThisPress = false;

    void Update()
    {
        if (IsAnyMenuOpen())
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnPlayerExit();
                currentInteractable = null;
            }
            currentHoldTime = 0f;
            actionExecutedThisPress = false;
            return;
        }

        FindNearestInteractable();

        if (currentInteractable == null)
        {
            currentHoldTime = 0f;
            actionExecutedThisPress = false;
            return;
        }

        if (currentInteractable.requiresHold)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                actionExecutedThisPress = false;
            }

            if (actionExecutedThisPress)
                return;

            if (Input.GetKey(KeyCode.E))
            {
                currentHoldTime += Time.deltaTime;
                float progress = currentHoldTime / currentInteractable.holdDuration;

                currentInteractable.SetHoldProgress(1f - progress);

                if (currentHoldTime >= currentInteractable.holdDuration)
                {
                    currentInteractable.ResetHoldProgress();
                    currentInteractable.Interact(gameObject);
                    currentHoldTime = 0f;
                    actionExecutedThisPress = true; 
                }
            }
            else
            {
                if (currentHoldTime > 0f)
                {
                    currentHoldTime = 0f;
                    currentInteractable.ResetHoldProgress();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    void FindNearestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);

        Interactable nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null && interactable.isInteractable)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = interactable;
                }
            }
        }

        if (nearest != currentInteractable)
        {
            if (currentInteractable != null)
                currentInteractable.OnPlayerExit();

            currentInteractable = nearest;
            currentHoldTime = 0f;

            if (currentInteractable != null)
                currentInteractable.OnPlayerEnter();
        }
    }

    bool IsAnyMenuOpen()
    {
        if (InteractionMenuUI.Instance != null && InteractionMenuUI.Instance.IsOpen()) return true;
        if (CookingUI.Instance != null && CookingUI.Instance.IsOpen()) return true;
        if (FridgeMiniGame.Instance != null && FridgeMiniGame.Instance.IsOpen()) return true;
        if (QTEStoveMiniGame.Instance != null && QTEStoveMiniGame.Instance.IsOpen()) return true;
        if (LaptopArcade.Instance != null && LaptopArcade.Instance.IsOpen()) return true;
        if (WardrobeMinigame.Instance != null && WardrobeMinigame.Instance.IsOpen()) return true;

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}