using UnityEngine;
using UnityEngine.UI;

public class DressCardView : MonoBehaviour
{
    [Header("Слои платья")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image patternImage;
    [SerializeField] private Image outlineImage;

    [SerializeField] private Button button;

    public Button Button => button;

    public void Setup(Color color, Sprite pattern)
    {
        if (fillImage != null) fillImage.color = color;

        if (patternImage != null)
        {
            patternImage.sprite = pattern;
            patternImage.gameObject.SetActive(pattern != null);
        }
    }
}