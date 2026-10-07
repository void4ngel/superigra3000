using System.Collections;
using UnityEngine;

public class DoorTeleport : Interactable
{
    [Header("Куда телепортируем")]
    [SerializeField] private Transform targetSpawnPoint;
    [SerializeField] private Transform targetCameraPos;
    [SerializeField] private Transform targetCameraLookAt;

    [Header("Инверсия движения")]
    [SerializeField] private bool invertMovementInTargetRoom = false;

    [Header("Звук и задержка")]
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private float delayBeforeTeleport = 0.05f;

    private bool isBusy = false;

    public override void Interact(GameObject player)
    {
        if (isBusy) return;
        StartCoroutine(TeleportRoutine(player));
    }

    IEnumerator TeleportRoutine(GameObject player)
    {
        isBusy = true;

        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.CanMove = false;

        if (doorSound != null)
            AudioSource.PlayClipAtPoint(doorSound, transform.position);

        if (delayBeforeTeleport > 0f)
            yield return new WaitForSeconds(delayBeforeTeleport);

        // === Телепорт через Rigidbody ===
        if (targetSpawnPoint != null && movement != null)
        {
            movement.Teleport(targetSpawnPoint.position, targetSpawnPoint.rotation);
        }

        // Камера
        if (CameraController.Instance != null && targetCameraPos != null && targetCameraLookAt != null)
        {
            CameraController.Instance.SetInitialPosition(
                targetCameraPos.position,
                targetCameraLookAt.position
            );
        }

        // Инверсия
        if (movement != null)
            movement.invertMovement = invertMovementInTargetRoom;

        // Пересчёт направлений камеры
        if (movement != null)
            movement.RefreshCameraDirections();

        yield return new WaitForSeconds(0.05f);

        // === Возвращаем управление ===
        if (movement != null) movement.CanMove = true;
        isBusy = false;
    }
}