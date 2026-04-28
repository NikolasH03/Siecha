using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EstadoMuerte : EstadoBase
{
    private readonly HealthComp vidaEnemigo;
    private readonly NavMeshAgent agente;
    public EstadoMuerte(Enemigo enemigo, Animator animator, NavMeshAgent agente, HealthComp vidaEnemigo) : base(enemigo, animator)
    {
        this.vidaEnemigo = vidaEnemigo;
        this.agente = agente;
    }

    public override void OnEnter()
    {
        animator.CrossFade(DeathHash, duracionTransicion);
        
        agente.isStopped = true;
        agente.velocity = Vector3.zero;
        enemigo.ActivarInvulnerabilidad();

        vidaEnemigo.OcultarUIBarras();
    }

    public override void Update()
    {
        if(vidaEnemigo.TerminaAnimacionMuerte)
        vidaEnemigo.Eliminar();
    }
}