using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float launchForce = 500f; // You can tweak this

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * launchForce); // Fires in the "forward" direction of FirePoint
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Demon"))
        {
            Debug.Log("Hit Enemy!");
               
        }
    }
}
