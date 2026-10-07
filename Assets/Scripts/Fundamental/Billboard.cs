using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("Если true — объект будет смотреть на камеру всегда. Если false — только по горизонтали.")]
    public bool fullBillboard = true;

    private Camera cam;

    void Start()
    {
        // Кешируем камеру один раз
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // LateUpdate — потому что камера двигается в Update, а нам нужно повернуть текст УЖЕ ПОСЛЕ её движения
        if (fullBillboard)
        {
            // Смотрим точно на камеру
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                             cam.transform.rotation * Vector3.up);
        }
        else
        {
            // Смотрим только по горизонтали (текст не наклоняется вверх-вниз)
            Vector3 lookDir = cam.transform.position - transform.position;
            lookDir.y = 0f;
            transform.LookAt(transform.position + lookDir);
        }
    }
}