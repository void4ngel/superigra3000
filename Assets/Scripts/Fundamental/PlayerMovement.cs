using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement settings ^^")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera UwU")]
    [SerializeField] private Camera playerCamera;

    [Header("Dance")]
    [SerializeField] private KeyCode danceKey = KeyCode.F;

    [Header("Инверсия движения (для комнат с другой камерой)")]
    public bool invertMovement = false;

    public bool CanMove { get; set; } = true;

    private Rigidbody rb;
    private Animator animator;

    private Vector3 camForward;
    private Vector3 camRight;

    private Vector3 moveDirection;
    private float currentSpeed = 0f;

    private bool isDancing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        RefreshCameraDirections();
    }

    /// <summary>
    /// Пересчитывает направление камеры. Вызывай при смене позиции камеры.
    /// </summary>
    public void RefreshCameraDirections()
    {
        camForward = playerCamera.transform.forward;
        camRight = playerCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
    }

    void Update()
    {
        if (!CanMove)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (Input.GetKeyDown(danceKey) && !isDancing)
        {
            StartDance();
            return;
        }

        if (isDancing)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 fwd = camForward;
        Vector3 right = camRight;

        if (invertMovement)
        {
            fwd = -fwd;
            right = -right;
        }

        moveDirection = (fwd * v + right * h).normalized;

        float targetSpeed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);
        animator.SetFloat("Speed", currentSpeed);
    }

    void FixedUpdate()
    {
        if (!CanMove || isDancing)
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.linearVelocity = vel;
            return;
        }

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            float maxDegrees = rotationSpeed * Time.fixedDeltaTime * 100f;
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, maxDegrees);

            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.linearVelocity = vel;
        }
    }

    void StartDance()
    {
        isDancing = true;
        animator.SetTrigger("F");
        StartCoroutine(WaitForDanceEnd());
    }

    System.Collections.IEnumerator WaitForDanceEnd()
    {
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("DanceState") && stateInfo.normalizedTime < 1f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        isDancing = false;
    }
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        // 1. Обнуляем скорость — иначе игрок улетит по инерции
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. Обновляем позицию в Rigidbody (это главное!)
        if (rb != null)
        {
            rb.position = position;
            rb.rotation = rotation;
        }

        // 3. Обновляем transform для визуала и камеры
        transform.position = position;
        transform.rotation = rotation;
    }
}