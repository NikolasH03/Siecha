using UnityEngine;

public class Arco : MonoBehaviour
{
    public GameObject flechaPrefab;
    public Transform puntoDisparo;
    public float fuerzaMinima = 10f;
    public float fuerzaMaxima = 50f;
    public float tiempoDeCarga = 2f;

    private float tiempoDeApuntar;
    private bool cargandoFlecha = false;
    private GameObject flechaActual;

    [SerializeField] Player player;

    void Update()
    {
        // Apuntar con el botón derecho
        if (Input.GetMouseButtonDown(1))
        {
            cargandoFlecha = true;
            tiempoDeApuntar = 0f;

            // Instancia la flecha, pero aún no la dispara
            flechaActual = Instantiate(flechaPrefab, puntoDisparo.position, puntoDisparo.rotation);
            flechaActual.GetComponent<Rigidbody>().isKinematic = true; // Desactiva la física temporalmente

            player.anim.Play("shoot");
        }

        if (Input.GetMouseButton(1))
        {
            // Aumentar el tiempo de carga mientras se mantiene el botón derecho
            tiempoDeApuntar += Time.deltaTime;
        }

        // Disparar con el botón izquierdo
        if (Input.GetMouseButtonUp(0) && cargandoFlecha)
        {
            // Calcula la fuerza en función del tiempo de carga
            float fuerza = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, tiempoDeApuntar / tiempoDeCarga);

            // Activa la física de la flecha y la dispara
            flechaActual.GetComponent<Rigidbody>().isKinematic = false;
            flechaActual.GetComponent<Rigidbody>().AddForce(puntoDisparo.forward * fuerza, ForceMode.Impulse);
            flechaActual = null; // Resetea la referencia de la flecha

            cargandoFlecha = false;

            player.anim.Play("recoil");
        }

        // Cancelar si se suelta el botón derecho sin disparar
        if (Input.GetMouseButtonUp(1) && cargandoFlecha)
        {
            Destroy(flechaActual); // Elimina la flecha sin dispararla
            cargandoFlecha = false;
            player.anim.Play("recoil");
        }
    }
}
