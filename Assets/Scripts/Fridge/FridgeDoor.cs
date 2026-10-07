using System.Collections;
using UnityEngine;

public class FridgeDoor : MonoBehaviour
{
    [Header("Петля (точка вращения)")]
    [SerializeField] private Transform hingePoint;

    [Header("Настройки")]
    [SerializeField] private float openAngle = -100f;
    [SerializeField] private float duration = 0.6f;

    private bool isOpen = false;
    private bool isAnimating = false;

    public void Open()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(RotateRoutine(openAngle, true));
    }

    public void Close()
    {
        if (!isOpen || isAnimating) return;
        StartCoroutine(RotateRoutine(-openAngle, false));
    }

    IEnumerator RotateRoutine(float totalAngle, bool newOpenState)
    {
        isAnimating = true;

        float rotated = 0f;
        float step;

        while (Mathf.Abs(rotated) < Mathf.Abs(totalAngle))
        {
            step = (totalAngle / duration) * Time.deltaTime;

            // Дверь вращается вокруг мировой позиции HingePoint
            transform.RotateAround(hingePoint.position, Vector3.up, step);

            rotated += step;
            yield return null;
        }

        isAnimating = false;
        isOpen = newOpenState;
    }
}