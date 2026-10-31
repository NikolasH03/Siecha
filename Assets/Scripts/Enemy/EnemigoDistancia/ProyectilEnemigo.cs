using UnityEngine;

/// <summary>Proyectil del arquero: vuela en línea recta, daña al jugador al impactar y se destruye.</summary>
public class ProyectilEnemigo : MonoBehaviour
{
    private const float CaidaMaxima         = 20f; // metros bajo la altura inicial antes de destruirse
    private const float VelocidadMinima     = 1f;  // por debajo se considera atascado

    [Header("Movimiento")]
    [SerializeField] private float velocidad        = 25f;
    [SerializeField] private float tiempoVidaMaximo = 6f;
    [SerializeField] private float distanciaMaxima  = 80f;

    [Header("Capas")]
    [SerializeField] private LayerMask capasDeteccion = -1;

    private int       dano;
    private Vector3   posicionInicial;
    private bool      yaImpacto;
    private Rigidbody rb;

    private void Awake()
    {
        rb               = GetComponent<Rigidbody>();
        posicionInicial  = transform.position;
        gameObject.layer = LayerMask.NameToLayer("Enemigo");
    }

    /// <summary>Debe llamarse justo después de Instantiate(): inyecta el daño y lanza el proyectil hacia su forward.</summary>
    public void Inicializar(int danoCausado)
    {
        dano = danoCausado;
        if (rb != null) rb.velocity = transform.forward * velocidad;

        Destroy(gameObject, tiempoVidaMaximo);
    }

    private void Update()
    {
        if (yaImpacto) return;

        bool demasiadoLejos = Vector3.Distance(transform.position, posicionInicial) > distanciaMaxima;
        bool cayoAlVacio    = transform.position.y < posicionInicial.y - CaidaMaxima;
        bool atascado       = rb != null && rb.velocity.magnitude < VelocidadMinima;

        if (demasiadoLejos || cayoAlVacio || atascado)
            Destruir();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaImpacto) return;
        if ((capasDeteccion.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.CompareTag("Player"))
        {
            ControladorCombate jugador = other.GetComponent<ControladorCombate>();
            if (jugador != null) jugador.JugadorRecibeDano(dano);
        }

        Destruir();
    }

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