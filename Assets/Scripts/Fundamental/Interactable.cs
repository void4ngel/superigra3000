using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public string promptText = "Нажми E, чтобы взаимодействовать";
    public bool isInteractable = true;

    [Header("Удержание E")]
    [Tooltip("Если true — надо зажать E на holdDuration секунд")]
    public bool requiresHold = false;
    public float holdDuration = 1.5f;

    [Header("World Prompt")]
    public WorldPrompt prompt;

    protected virtual void Start()
    {
        if (prompt == null) prompt = GetComponent<WorldPrompt>();
    }

    public abstract void Interact(GameObject player);

    public virtual void OnPlayerEnter()
    {
        if (prompt != null) prompt.Show();
    }

    public virtual void OnPlayerExit()
    {
        if (prompt != null) prompt.Hide();
    }

    // === Прогресс удержания ===
    public void SetHoldProgress(float t)
    {
        if (prompt != null) prompt.SetHoldProgress(t);
    }

    public void ResetHoldProgress()
    {
        if (prompt != null) prompt.ResetHoldProgress();
    }
}