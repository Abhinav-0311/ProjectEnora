using UnityEngine;
using UnityEngine.Events;

public class DemonHealth : MonoBehaviour
{
    public event System.Action<int> HitTaken;
    public event System.Action Defeated;

    public int hitsToDie = 3;
    private int currentHits = 0;

    public GameObject[] objectsToEnable;   // Assigned in Inspector
    public GameObject[] objectsToDisable;  // Assigned in Inspector

    [Header("Narrative")]
    public UnityEvent onDefeated;

    private bool isDead = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Fireball")) // Fireball should have this tag
        {
            currentHits++;
            HitTaken?.Invoke(currentHits);

            if (currentHits >= hitsToDie)
            {
                Die();
            }
        }
    }

    void Die()
    {
        isDead = true;

        // Enable objects
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Disable objects
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(false);
        }

        onDefeated?.Invoke();
        Defeated?.Invoke();

        // Optionally remove or deactivate the demon
        gameObject.SetActive(false);
    }
}
