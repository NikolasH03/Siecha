using UnityEngine;

/// <summary>
/// Maneja el disparo en area del arcabuz (protagonista espanol).
/// A diferencia del arco (que usa proyectil), el arcabuz golpea en una
/// esfera frente al jugador simulando el disparo de escopeta.
/// </summary>
public class ArcabuzDisparo : MonoBehaviour
{
    [Header("Configuracion de disparo")]
    [Tooltip("Distancia hacia adelante desde puntoDisparo donde se centra la esfera de impacto.")]
    [SerializeField] private float rangoDisparo  = 5f;
    [Tooltip("Radio de la esfera de impacto.")]
    [SerializeField] private float radioImpacto  = 3f;
    [Tooltip("Angulo del cono de disparo. Enemigos fuera de este angulo no reciben dano.")]
    [SerializeField] private float anguloDisparo = 60f;

    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Transform puntoDisparo;

    private ControladorCombate player;

    public void Awake()
    {
        player = GetComponent<ControladorCombate>();
    }

    public void Disparar()
    {
        if (puntoDisparo == null)
        {
            Debug.LogWarning("[ArcabuzDisparo] puntoDisparo no asignado.");
            return;
        }
        
        Vector3 centroDisparo = puntoDisparo.position + transform.forward * rangoDisparo;
        Collider[] impactados = Physics.OverlapSphere(centroDisparo, radioImpacto, capaEnemigos);

        float cosAngulo = Mathf.Cos(anguloDisparo * 0.5f * Mathf.Deg2Rad);

        foreach (Collider enemigo in impactados)
        {
            if (enemigo == null) continue;
            
            Vector3 dirEnemigo = (enemigo.transform.position - puntoDisparo.position).normalized;
            float   dot        = Vector3.Dot(transform.forward, dirEnemigo);

            if (dot < cosAngulo) continue; // fuera del cono, ignorar

            HealthComp salud = enemigo.GetComponent<HealthComp>();
            if (salud == null) continue;

            salud.recibeDano(player.EntregarDanoArmaDistancia());
            salud.setRecibiendoDano(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (puntoDisparo == null) return;

        Vector3 centro = puntoDisparo.position + transform.forward * rangoDisparo;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centro, radioImpacto);

        // Visualizar el cono de disparo
        float   mitad = anguloDisparo * 0.5f;
        Vector3 der   = Quaternion.Euler(0,  mitad, 0) * transform.forward * (rangoDisparo + radioImpacto);
        Vector3 izq   = Quaternion.Euler(0, -mitad, 0) * transform.forward * (rangoDisparo + radioImpacto);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(puntoDisparo.position, puntoDisparo.position + der);
        Gizmos.DrawLine(puntoDisparo.position, puntoDisparo.position + izq);
    }
}

