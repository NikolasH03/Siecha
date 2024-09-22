using System.Collections;
using UnityEngine;

public class Flecha : MonoBehaviour
{
    public float force = 50f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire()
    {
        rb.isKinematic = false;
        rb.AddForce(transform.forward * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Player")
        {
            rb.isKinematic = true; // Hace que la flecha se quede pegada
            StartCoroutine(Countdown());
        }
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject); // Destruye la flecha después de 10 segundos
    }
}


