using UnityEngine;
public class ProyectilEnemigo : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 25f;
    [SerializeField] private float tiempoVidaMaximo = 6f;
    [SerializeField] private float distanciaMaxima = 80f;

    [Header("Capas")]
    [SerializeField] private LayerMask capasDeteccion = -1;

    // Estado interno
    private int dano;
    private Vector3 posicionInicial;
    private bool yaImpacto = false;
    private Rigidbody rb;

    // ─── Inicialización ───────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        posicionInicial = transform.position;
        gameObject.layer = LayerMask.NameToLayer("Enemigo");
    }

    /// <summary>
    /// Debe llamarse inmediatamente después de Instantiate().
    /// Inyecta el daño y lanza el proyectil en la dirección forward del Transform.
    /// </summary>
    public void Inicializar(int danoCausado)
    {
        dano = danoCausado;
        if (rb != null)
            rb.velocity = transform.forward * velocidad;

        Destroy(gameObject, tiempoVidaMaximo);
    }

    // ─── Loop ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (yaImpacto) return;

        if (Vector3.Distance(transform.position, posicionInicial) > distanciaMaxima)
            Destruir();
        else if (transform.position.y < posicionInicial.y - 20f)
            Destruir();
        else if (rb != null && rb.velocity.magnitude < 1f)
            Destruir();
    }

    // ─── Colisión ─────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (yaImpacto) return;

        int otherLayer = 1 << other.gameObject.layer;
        if ((capasDeteccion.value & otherLayer) == 0) return;

        if (other.CompareTag("Player"))
        {
            ControladorCombate jugador = other.GetComponent<ControladorCombate>();

            jugador.JugadorRecibeDano(dano);
            
        }

        Destruir();
    }

    // ─── Destrucción ──────────────────────────────────────────────────────────

    private void Destruir()
    {
        yaImpacto = true;

        if (rb != null)
        {
            rb.velocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic     = true;
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }
}
