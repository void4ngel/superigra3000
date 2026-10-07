using UnityEngine;

public class Boyfriend : Interactable
{
    [Header("Камера при взаимодействии")]
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Transform cameraLookAt;

    public override void Interact(GameObject player)
    {
        // Приближаем камеру
        if (CameraController.Instance != null && cameraPos != null && cameraLookAt != null)
            CameraController.Instance.MoveTo(cameraPos, cameraLookAt);

        // Открываем меню — передаём себя, чтобы меню знало, кому возвращать камеру
        if (InteractionMenuUI.Instance != null)
            InteractionMenuUI.Instance.OpenForBoyfriend(this);
    }

    public void ReturnCamera()
    {
        if (CameraController.Instance != null)
            CameraController.Instance.ReturnToInitial();
    }
}