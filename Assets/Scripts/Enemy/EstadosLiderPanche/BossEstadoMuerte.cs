using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado de muerte del jefe. A diferencia de EstadoMuerte normal,
/// NO llama Eliminar() directamente. En su lugar dispara un evento
/// que BossBattleState recibe para reproducir la cinemática de muerte.
/// Solo después de la cinemática, BossBattleState llama Eliminar().
/// </summary>
public class BossEstadoMuerte : EstadoBase
{
    private readonly HealthComp   vidaEnemigo;
    private readonly NavMeshAgent agente;
    private readonly BossEnemigo  boss;

    // BossBattleState se suscribe a este evento.
    public static event Action<BossEnemigo> OnBossMuertoListo;

    private bool eventoDisparado = false;

    public BossEstadoMuerte(BossEnemigo boss, Animator animator, NavMeshAgent agente, HealthComp vidaEnemigo)
        : base(boss, animator)
    {
        this.boss        = boss;
        this.agente      = agente;
        this.vidaEnemigo = vidaEnemigo;
    }

    public override void OnEnter()
    {
        eventoDisparado = false;

        // Detener todo movimiento
        agente.velocity  = Vector3.zero;
        agente.isStopped = true;
        agente.enabled   = false;

        boss.ActivarInvulnerabilidad();
        boss.desactivarCollider();
        vidaEnemigo.OcultarUIBarras();

        // Animación de muerte (arte definirá el clip concreto)
        animator.CrossFade(DeathHash, 0.2f);
    }

    public override void Update()
    {
        // Esperamos un frame para que la animación arranque antes de disparar el evento.
        // Esto evita que la cinemática corte la transición de animación bruscamente.
        if (!eventoDisparado)
        {
            eventoDisparado = true;
            OnBossMuertoListo?.Invoke(boss);
        }
    }
    // No tiene OnExit: el jefe no sale de este estado por su cuenta.
    // BossBattleState llama HealthComp.Eliminar() después de la cinemática.
}
