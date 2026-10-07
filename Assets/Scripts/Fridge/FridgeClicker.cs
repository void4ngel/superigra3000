using UnityEngine;

public class FridgeClicker : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private float maxDistance = 5f;

    private FridgeMiniGame game;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        game = FindObjectOfType<FridgeMiniGame>();
    }

    void Update()
    {
        if (game == null || !game.IsOpen()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, itemLayer))
            {
                var item = hit.collider.GetComponent<FridgeItem>();
                if (item != null)
                    game.OnItemClicked(item.itemName);
            }
        }
    }
}