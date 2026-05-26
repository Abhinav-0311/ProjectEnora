using UnityEngine;

/// <summary>Hazard projectile. Default: reload current scene when hitting the player or cannon target.</summary>
public class Fireball : MonoBehaviour
{
    private Vector3 movementDirection;

    [SerializeField] private bool reloadActiveSceneOnPlayerHit = true;
    [SerializeField] private string loadSceneNameOnPlayerHit = SceneNames.Level2;
    [SerializeField] private string primaryHitTag = "Player";
    [SerializeField] private string secondaryHitTag = "Cannon";

    public void Initialize(Vector3 direction)
    {
        movementDirection = direction;
    }

    private void Update()
    {
        if (movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HitsTrackedTarget(other))
        {
            return;
        }

        if (reloadActiveSceneOnPlayerHit)
        {
            SceneTransitionController.ReloadCurrentScene();
        }
        else
        {
            SceneTransitionController.LoadScene(loadSceneNameOnPlayerHit);
        }
    }

    private bool HitsTrackedTarget(Collider other)
    {
        return other.CompareTag(primaryHitTag)
            || (!string.IsNullOrWhiteSpace(secondaryHitTag) && other.CompareTag(secondaryHitTag));
    }
}
