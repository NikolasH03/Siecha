using DG.Tweening;
using UnityEngine;

public class AutoTargeting : MonoBehaviour
{
    [Header("Busqueda de objetivos")]
    [SerializeField] private float rangoBusqueda   = 6f;
    [SerializeField] private float anguloBusqueda  = 120f;
    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Transform referenciaRotacion;
    [SerializeField] private float velocidadGiro   = 10f;

    [Header("Aproximacion al atacar")]
    [SerializeField] private float distanciaMinimaAtaque  = 1.6f;
    [SerializeField] private float duracionAproximacion   = 0.20f;
    [SerializeField] private Ease  easingAproximacion     = Ease.OutQuad;

    [Header("Desplazamiento sin objetivo")]
    [SerializeField] private float distanciaDesplazamientoLibre = 2f;
    [SerializeField] private float duracionDesplazamientoLibre  = 0.25f;

    [Header("Dash")]
    [SerializeField] private float distanciaDash  = 4f;
    [SerializeField] private float duracionDash   = 0.15f;
    [SerializeField] private Ease  easingDash     = Ease.OutQuint;

    [Header("Colisiones (dash, desplazamiento y aproximación)")]
    [Tooltip("Capas sólidas que bloquean el movimiento (muros, estructuras, props). " +
             "NO incluir Player, JugadorInvulnerable, Enemigo ni Arma.")]
    [SerializeField] private LayerMask capasObstaculos = 1; 
    [SerializeField] private float radioJugador = 0.5f;
    [SerializeField] private float alturaJugador = 1.8f;
    [Tooltip("Obstáculos más bajos que esto (escalones, bordillos) no bloquean.")]
    [SerializeField] private float alturaMinimaObstaculo = 0.3f;
    [SerializeField] private float margenSeguridad = 0.1f;
    
    [Header("Componentes Físicos")]
    [SerializeField] private Rigidbody rb;
    
    private Transform enemigoObjetivo;
    private Tween     movimientoTween;

    public Transform EnemigoObjetivo => enemigoObjetivo;

    // ─── Targeting ────────────────────────────────────────────────────────────

    public void BuscarSegunDireccionDeMirada(Vector2 inputMirar)
    {
        if (inputMirar.sqrMagnitude < 0.1f) return;

        Camera cam    = Camera.main;
        Vector3 fwd   = cam.transform.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cam.transform.right;  right.y = 0; right.Normalize();

        Vector3 dirMirada = (fwd * inputMirar.y + right * inputMirar.x).normalized;

        Collider[] enemigos = Physics.OverlapSphere(transform.position, rangoBusqueda, capaEnemigos);

        // Puntaje mínimo para ser considerado (dentro del cono visual)
        float umbralAngulo = Mathf.Cos(anguloBusqueda * 0.5f * Mathf.Deg2Rad);
        
        float mejorPuntaje  = float.MinValue; 
        Transform mejorObjetivo = null;

        foreach (Collider col in enemigos)
        {
            if (col == null) continue;

            Vector3 dirEnemigo = (col.transform.position - transform.position);
            dirEnemigo.y = 0;
            
            float distancia = dirEnemigo.magnitude;
            dirEnemigo.Normalize();

            float dot = Vector3.Dot(dirMirada, dirEnemigo);
            
            if (dot > umbralAngulo)
            {
                // SISTEMA DE PUNTAJE MIXTO
                // Multiplicamos el dot (de -1 a 1) por un peso, y le restamos la distancia.
                // Ajusta el '5f' según qué tanto quieras priorizar el joystick vs la cercanía.
                float puntaje = (dot * 5f) - distancia;

                if (puntaje > mejorPuntaje)
                {
                    mejorPuntaje  = puntaje;
                    mejorObjetivo = col.transform;
                }
            }
        }

        if (mejorObjetivo != null)
        {
            enemigoObjetivo = mejorObjetivo;

            Vector3 dir = (enemigoObjetivo.position - transform.position);
            dir.y = 0; dir.Normalize();
            referenciaRotacion.DORotateQuaternion(Quaternion.LookRotation(dir), 0.15f);

            MoverHaciaObjetivoSiEsNecesario();
        }
        else
        {
            enemigoObjetivo = null;
            DesplazarseEnDireccion(dirMirada, distanciaDesplazamientoLibre,
                                   duracionDesplazamientoLibre, easingAproximacion);
            referenciaRotacion.DORotateQuaternion(Quaternion.LookRotation(dirMirada), 0.15f);
        }
    }

    private void MoverHaciaObjetivoSiEsNecesario()
    {
        if (enemigoObjetivo == null) return;

        float dist = Vector3.Distance(transform.position, enemigoObjetivo.position);
        if (dist <= distanciaMinimaAtaque + 0.1f) return;

        Vector3 dir      = (enemigoObjetivo.position - transform.position).normalized;
        Vector3 destino  = enemigoObjetivo.position - dir * distanciaMinimaAtaque;
        destino = LimitarDestino(destino); 
        
        movimientoTween?.Kill();
        movimientoTween = transform.DOMove(destino, duracionAproximacion)
                                   .SetEase(easingAproximacion)
                                   .SetUpdate(UpdateType.Fixed);
    }

    // ─── Dash ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Ejecuta el dash usando el input de movimiento si se proporciona.
    /// Si no hay input (Vector2.zero o null), el dash va hacia atrás como dodge.
    /// Esto permite:
    ///   - Dash direccional: el jugador elige hacia donde escapar
    ///   - Dash de dodge: sin input = siempre atrás (comportamiento anterior)
    /// FIX: antes siempre iba hacia atrás ignorando el input que se pasaba
    /// desde ControladorCombate.DesplazamientoDash().
    /// </summary>
    public void EjecutarDash(Vector2? inputMovimiento = null)
    {
        Vector3 direccion;

        if (inputMovimiento.HasValue && inputMovimiento.Value.sqrMagnitude > 0.1f)
        {
            // Convertir input 2D a dirección del mundo usando la cámara
            Camera  cam   = Camera.main;
            Vector3 fwd   = cam.transform.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = cam.transform.right;   right.y = 0; right.Normalize();
            direccion = (fwd * inputMovimiento.Value.y + right * inputMovimiento.Value.x).normalized;
            Debug.Log("si entra a la seccion con input");
        }
        else
        {
            // Sin input: dash de esquive hacia atrás
            direccion = -referenciaRotacion.forward;
            direccion.y = 0;
            direccion.Normalize();
        }

        DesplazarseEnDireccion(direccion, distanciaDash, duracionDash, easingDash);
        
        if (inputMovimiento.HasValue && inputMovimiento.Value.sqrMagnitude > 0.1f)
        {
            referenciaRotacion.DORotateQuaternion(Quaternion.LookRotation(direccion), 0.1f);
        }
    }

    // ─── Movimiento base ──────────────────────────────────────────────────────

    public void DesplazarseEnDireccion(Vector3 direccion, float distancia, float duracion, Ease easing)
    {
        direccion.y = 0;
        direccion.Normalize();

        movimientoTween?.Kill();

        Vector3 destino = LimitarDestino(transform.position + direccion * distancia);

        movimientoTween = transform.DOMove(destino, duracion)
            .SetEase(easing)
            .SetUpdate(UpdateType.Fixed);
    }
    /// <summary>
    /// Devuelve el destino recortado para que el jugador se detenga antes de un obstáculo.
    /// </summary>
    private Vector3 LimitarDestino(Vector3 destino)
    {
        Vector3 origen = transform.position;

        Vector3 delta = destino - origen;
        delta.y = 0f;
        float distancia = delta.magnitude;
        if (distancia < 0.001f) return origen;

        Vector3 dir = delta / distancia;

        // Radio ligeramente menor para no "empezar dentro" de una pared pegada.
        float radioCast = radioJugador * 0.95f;
        Vector3 p1 = origen + Vector3.up * (alturaMinimaObstaculo + radioCast);
        Vector3 p2 = origen + Vector3.up * Mathf.Max(alturaMinimaObstaculo + radioCast, alturaJugador - radioCast);

        Vector3 resultado = destino;

        if (Physics.CapsuleCast(p1, p2, radioCast, dir, out RaycastHit hit,
                distancia + margenSeguridad, capasObstaculos,
                QueryTriggerInteraction.Ignore))
        {
            float distanciaSegura = Mathf.Max(0f, hit.distance - margenSeguridad);
            resultado = origen + dir * distanciaSegura;
        }

        resultado.y = destino.y;
        return resultado;
    }
    public void CancelarMovimiento() => movimientoTween?.Kill();

    private void OnDestroy() => movimientoTween?.Kill();

    // ─── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoBusqueda);

        if (enemigoObjetivo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, enemigoObjetivo.position);
        }
    }
}

