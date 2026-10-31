using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Herramienta de diagnóstico, solo editor: dibuja sobre cada enemigo su estado de la FSM, sus flags
/// de combate y una línea hasta el destino del NavMeshAgent. Añádelo al prefab (o quítalo): no afecta al juego.
/// Se ve en la vista Scene, y en Game si activas Gizmos.
/// </summary>
[RequireComponent(typeof(Enemigo))]
public class EnemigoDebugVisual : MonoBehaviour
{
#if UNITY_EDITOR
    private Enemigo enemigo;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (enemigo == null) enemigo = GetComponent<Enemigo>();
        HealthComp salud = enemigo.GetHealthComp();
        if (salud == null || salud.EstaMuerto) return;

        // Línea al destino: si cruza al jugador, el enemigo está "arrollando" para llegar a él.
        var agent = enemigo.Agent;
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = enemigo.EstaAtacando() ? Color.red : Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, agent.destination + Vector3.up * 0.5f);
            Gizmos.DrawWireSphere(agent.destination, 0.3f);
        }

        string texto =
            $"{enemigo.NombreEstadoActual}\n" +
            $"slot:{enemigo.EstaAtacando()} ({enemigo.TiempoConSlot:F1}s)  combo:{enemigo.EstaEnCombo}\n" +
            $"danado:{salud.EstaSiendoDanado}  bloq:{salud.getBloqueando()}  stun:{salud.EstaStuneado}  guardBreak:{salud.EnGuardBreak}";

        Handles.Label(transform.position + Vector3.up * 2.6f, texto);
    }
#endif
}