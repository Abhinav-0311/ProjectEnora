using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Level 1 → Level 2: dungeon complete → castle / boss (lvl2).</summary>
public class level2 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            SceneManager.LoadScene(SceneNames.Level2);
    }
}
