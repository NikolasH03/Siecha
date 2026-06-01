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

        float     mejorPuntaje  = Mathf.Cos(anguloBusqueda * 0.5f * Mathf.Deg2Rad);
        Transform mejorObjetivo = null;

        foreach (Collider col in enemigos)
        {
            // FIX: null check — Unity puede devolver colisores destruidos en el array
            if (col == null) continue;

            Vector3 dirEnemigo = (col.transform.position - transform.position);
            dirEnemigo.y = 0;
            dirEnemigo.Normalize();

            float dot = Vector3.Dot(dirMirada, dirEnemigo);
            if (dot > mejorPuntaje)
            {
                mejorPuntaje  = dot;
                mejorObjetivo = col.transform;
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
        }
        else
        {
            // Sin input: dash de esquive hacia atrás
            direccion = -referenciaRotacion.forward;
            direccion.y = 0;
            direccion.Normalize();
        }

        DesplazarseEnDireccion(direccion, distanciaDash, duracionDash, easingDash);
    }

    // ─── Movimiento base ──────────────────────────────────────────────────────

    public void DesplazarseEnDireccion(Vector3 direccion, float distancia, float duracion, Ease easing)
    {
        direccion.y = 0;
        direccion.Normalize();

        movimientoTween?.Kill();
        movimientoTween = transform.DOMove(transform.position + direccion * distancia, duracion)
                                   .SetEase(easing)
                                   .SetUpdate(UpdateType.Fixed);
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

