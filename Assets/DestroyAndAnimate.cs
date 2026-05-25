using UnityEngine;

public class DestroyAndAnimate : MonoBehaviour
{
    public GameObject targetToDestroy; // Assign in Inspector
    public Animator animator;
    public string animationTrigger = "end"; // Name of the animation trigger

    private bool hasTriggered = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !hasTriggered)
        {
            hasTriggered = true;

            // Destroy the assigned target
            if (targetToDestroy != null)
            {
                Destroy(targetToDestroy);
            }

            // Play the animation on this object
            if (animator != null)
            {
                animator.SetTrigger(animationTrigger);
            }
        }
    }
}
