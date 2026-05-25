using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    public Animator doorAnimator;
    public string doorOpenTrigger = "Open";

    public void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(doorOpenTrigger);
        }
        else
        {
            Debug.LogWarning("Door Animator not assigned!");
        }
    }
}
