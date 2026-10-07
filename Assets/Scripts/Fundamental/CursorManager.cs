using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Иконка курсора")]
    [SerializeField] private RectTransform cursorImage;
    [SerializeField] private CanvasGroup cursorCanvasGroup;

    private int openMenusCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(false);
    }

    public void RegisterMenuOpened()
    {
        openMenusCount++;
        Debug.Log($"[Cursor] OPEN. count = {openMenusCount}");
    }

    public void RegisterMenuClosed()
    {
        openMenusCount = Mathf.Max(0, openMenusCount - 1);
        Debug.Log($"[Cursor] CLOSE. count = {openMenusCount}");
    }

    void LateUpdate()
    {
        bool anyOpen = openMenusCount > 0;

        Cursor.visible = !anyOpen;
        Cursor.lockState = CursorLockMode.None;

        if (cursorImage != null)
        {
            if (cursorImage.gameObject.activeSelf != anyOpen)
                cursorImage.gameObject.SetActive(anyOpen);

            if (anyOpen)
                cursorImage.position = Input.mousePosition;
            cursorImage.SetAsLastSibling();
        }
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }
}