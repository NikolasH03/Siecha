using UnityEngine;

/// <summary>
/// Componente en el jugador. Calcula cada frame quién es el objetivo real:
/// - ObjetivoMeleeActual: el enemigo dentro del cono de ataque más cercano
/// - ObjetivoRangeActual: el enemigo que intersecta el raycast de apuntado
///
/// Los enemigos consultan EsSoyElObjetivo() en lugar de reaccionar a
/// banderas globales como "jugadorEstaAtacando", resolviendo el problema
/// de que todos bloqueen al mismo tiempo aunque no les estén atacando.
///
/// SETUP: agregar este componente al mismo GameObject que AutoTargeting
/// y ControladorApuntado.
/// </summary>
public class DetectorObjetivoJugador : MonoBehaviour
{
    [Header("Melee")]
    [Tooltip("Debe coincidir con el rango de ataque real del arma cuerpo a cuerpo.")]
    [SerializeField] private float rangoMelee = 3f;
    [Tooltip("Ángulo del cono de ataque. 90 = 45° a cada lado del forward.")]
    [SerializeField] private float anguloMelee = 90f;
    [SerializeField] private LayerMask capaEnemigos;

    [Header("Ranged")]
    [SerializeField] private float rangoRaycast = 60f;

    // Referencias — se buscan automáticamente en Awake
    private AutoTargeting       autoTargeting;
    private ControladorApuntado controladorApuntado;
    private ControladorCombate  controladorCombate;

    // ─── Resultados públicos ──────────────────────────────────────────────────
    public Transform ObjetivoMeleeActual { get; private set; }
    public Transform ObjetivoRangeActual { get; private set; }

    // ─── Inicialización ───────────────────────────────────────────────────────

    private void Awake()
    {
        autoTargeting       = GetComponent<AutoTargeting>();
        controladorApuntado = GetComponent<ControladorApuntado>();
        controladorCombate  = GetComponent<ControladorCombate>();
    }

    // ─── Loop ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        ActualizarObjetivoMelee();
        ActualizarObjetivoRange();
    }

    // ─── Detección melee ──────────────────────────────────────────────────────

    private void ActualizarObjetivoMelee()
    {
        // Primero intentamos usar el objetivo que ya calculó AutoTargeting.
        // Es el enemigo más alineado con la dirección de movimiento — exactamente
        // el que recibirá el golpe si el jugador ataca en este frame.
        if (autoTargeting != null && autoTargeting.EnemigoObjetivo != null)
        {
            Transform candidato = autoTargeting.EnemigoObjetivo;
            float distancia = Vector3.Distance(transform.position, candidato.position);
            Vector3 dir     = (candidato.position - transform.position).normalized;
            float angulo    = Vector3.Angle(transform.forward, dir);

            // Validamos que el objetivo de AutoTargeting siga dentro del cono
            // y rango de golpe real (puede haberse movido desde la búsqueda).
            if (distancia <= rangoMelee && angulo <= anguloMelee * 0.5f)
            {
                ObjetivoMeleeActual = candidato;
                return;
            }
        }

        // Fallback: búsqueda directa si AutoTargeting no tiene objetivo válido.
        ObjetivoMeleeActual = BuscarMasCercanoEnCono();
    }

    private Transform BuscarMasCercanoEnCono()
    {
        Collider[] candidatos = Physics.OverlapSphere(transform.position, rangoMelee, capaEnemigos);

        Transform mejor        = null;
        float     menorDist    = float.MaxValue;
        float     cosMinAngulo = Mathf.Cos(anguloMelee * 0.5f * Mathf.Deg2Rad);

        foreach (var col in candidatos)
        {
            if (col == null) continue;

            Vector3 dir = (col.transform.position - transform.position).normalized;
            float   dot = Vector3.Dot(transform.forward, dir);

            if (dot >= cosMinAngulo)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < menorDist)
                {
                    menorDist = dist;
                    mejor     = col.transform;
                }
            }
        }

        return mejor;
    }

    // ─── Detección ranged ─────────────────────────────────────────────────────

    private void ActualizarObjetivoRange()
    {
        // Solo calculamos si el jugador está en modo apuntado.
        if (controladorApuntado == null || !controladorApuntado.GetEstaApuntando())
        {
            ObjetivoRangeActual = null;
            return;
        }

        // Raycast desde el centro de pantalla — igual que ObtenerPosicionObjetivo()
        // en ControladorApuntado. Funciona para ambos protagonistas porque
        // ambos apuntan usando la misma cámara central.
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

        ObjetivoRangeActual = Physics.Raycast(ray, out RaycastHit hit, rangoRaycast, capaEnemigos)
            ? hit.transform
            : null;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// ¿Es este enemigo el objetivo actual del jugador?
    /// Ranged tiene prioridad si el jugador está apuntando.
    /// Melee aplica si está atacando cuerpo a cuerpo.
    /// </summary>
    public bool EsSoyElObjetivo(Transform enemigo)
    {
        if (enemigo == null) return false;

        bool estaApuntando = controladorApuntado != null && controladorApuntado.GetEstaApuntando();

        if (estaApuntando)
            return ObjetivoRangeActual == enemigo;

        bool estaAtacando = controladorCombate != null && controladorCombate.getAtacando();

        if (estaAtacando)
            return ObjetivoMeleeActual == enemigo;

        return false;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Cono de melee
        float mitad = anguloMelee * 0.5f;
        Vector3 der  = Quaternion.Euler(0,  mitad, 0) * transform.forward * rangoMelee;
        Vector3 izq  = Quaternion.Euler(0, -mitad, 0) * transform.forward * rangoMelee;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoMelee);
        Gizmos.DrawLine(transform.position, transform.position + der);
        Gizmos.DrawLine(transform.position, transform.position + izq);

        // Objetivo melee actual
        if (ObjetivoMeleeActual != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, ObjetivoMeleeActual.position);
        }

        // Objetivo ranged actual
        if (ObjetivoRangeActual != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, ObjetivoRangeActual.position);
        }
    }
}
