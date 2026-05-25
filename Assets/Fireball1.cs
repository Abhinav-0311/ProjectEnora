using UnityEngine;

public class Fireball1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("Demon")){
            Debug.Log("hello");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
