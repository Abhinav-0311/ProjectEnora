using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Image crosshairImage; // Assign the Crosshair UI Image
    public Color defaultColor = Color.white;
    public Color interactableColor = Color.green;

    private bool isVisible = true;

    private void Start()
    {
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = defaultColor;
            crosshairImage.enabled = isVisible;
        }
    }

    public void SetCrosshairState(bool isInteractable)
    {
        if (crosshairImage == null || !isVisible)
        {
            return;
        }

        crosshairImage.color = isInteractable ? interactableColor : defaultColor;
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;

        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        if (crosshairImage == null)
        {
            return;
        }

        crosshairImage.enabled = isVisible;
        if (isVisible)
        {
            crosshairImage.color = defaultColor;
        }
    }
}
