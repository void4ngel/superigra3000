using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Плавность перелёта")]
    public float transitionDuration = 1.2f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool isBusy = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Запоминаем изначальную позицию камеры
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// Плавно летит к targetPos и смотрит на lookAt.
    /// </summary>
    public void MoveTo(Transform targetPos, Transform lookAt)
    {
        StopAllCoroutines();
        isBusy = false;
        StartCoroutine(MoveRoutine(targetPos.position, lookAt.position, transitionDuration));
    }

    /// <summary>
    /// Возвращает камеру в изначальную позицию.
    /// </summary>
    public void ReturnToInitial()
    {
        StopAllCoroutines();
        isBusy = false;

        Vector3 lookAt = initialPosition + (initialRotation * Vector3.forward);
        StartCoroutine(MoveRoutine(initialPosition, lookAt, transitionDuration));
    }

    /// <summary>
    /// Меняет «начальную» позицию камеры. Используется при переходе между комнатами.
    /// </summary>
    public void SetInitialPosition(Vector3 pos, Vector3 lookAt)
    {
        initialPosition = pos;
        initialRotation = Quaternion.LookRotation(lookAt - pos);

        // Мгновенно перемещаем камеру
        transform.position = pos;
        transform.rotation = initialRotation;
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Vector3 lookAtPos, float duration)
    {
        isBusy = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Куда должна смотреть камера в конце
        Quaternion endRot = transform.rotation;
        if (lookAtPos != targetPos)
            endRot = Quaternion.LookRotation(lookAtPos - targetPos);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smoothPos = Mathf.SmoothStep(0f, 1f, t);

            // Позиция — плавно всё время
            transform.position = Vector3.Lerp(startPos, targetPos, smoothPos);

            // Поворот — только во второй половине полёта (мягко)
            float rotT = Mathf.InverseLerp(0.5f, 1f, t);
            rotT = Mathf.SmoothStep(0f, 1f, rotT);
            transform.rotation = Quaternion.Slerp(startRot, endRot, rotT);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = endRot;
        isBusy = false;
    }
}