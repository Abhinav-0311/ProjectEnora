using UnityEngine;

/// <summary>Level 1 to Level 2: dungeon complete to castle / boss (lvl2).</summary>
public class Level2TransitionTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneTransitionController.LoadScene(SceneNames.Level2);
        }
    }
}
