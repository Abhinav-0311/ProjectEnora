using UnityEngine;

public class FinalPuzzle : MonoBehaviour
{
    public Animator playerAnimator;
    public Transform playerTransform; // Reference to the player's Transform
    public GameObject triggerlocation; // Trigger location object
    public float triggerDistance = 1.0f; // Distance threshold to activate animation

    private void Update()
    {
        if (Vector3.Distance(playerTransform.position, triggerlocation.transform.position) <= triggerDistance)
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Open");
                enabled = false; // Disable the script after triggering to avoid repeated triggers
            }
        }
    }
}
