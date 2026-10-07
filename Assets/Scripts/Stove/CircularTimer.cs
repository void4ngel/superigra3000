using UnityEngine;
using UnityEngine.UI;

public class CircularTimer : MonoBehaviour
{
    [SerializeField] private Image circleFill;

    public void SetFill(float value01)
    {
        if (circleFill != null) circleFill.fillAmount = Mathf.Clamp01(value01);
    }
}