using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Image crosshairImage; // Assign the Crosshair UI Image
    public Color defaultColor = Color.white;
    public Color interactableColor = Color.green;

    void Start()
    {
        if (crosshairImage == null)
            crosshairImage = GetComponent<Image>();

        crosshairImage.color = defaultColor; // Set default color
    }

    public void SetCrosshairState(bool isInteractable)
    {
        crosshairImage.color = isInteractable ? interactableColor : defaultColor;
    }
}
