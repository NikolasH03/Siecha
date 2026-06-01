using UnityEngine;
using UnityEngine.AI;

public class EstadoAtacarJugador : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly float rangoDeAtaque;
    private readonly EnemyStats stats;

    private bool esperandoFinAnimacion = false;
    private bool animacionCompletada = false;

    public EstadoAtacarJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, float rangoDeAtaque)
        : base(enemigo, animator)
    {
        this.agent = agent;
        this.rangoDeAtaque = rangoDeAtaque;
        this.stats = enemigo.Stats;
    }

    public override void OnEnter()
    {
        enemigo.OrdenarAtacar();
        enemigo.RegistrarEstadoAtacar(this);
        enemigo.IniciarCombo(enemigo.TipoAtaqueActual);
        EjecutarAtaque();
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;
        if (jugador == null) return;

        // Posicionamiento
        Vector3 direccionAlJugador = (jugador.position - enemigo.transform.position).normalized;
        float distanciaDeseada = rangoDeAtaque * 0.8f;
        Vector3 destino = jugador.position - direccionAlJugador * distanciaDeseada;

        agent.SetDestination(destino);

        Vector3 lookPosition = jugador.position;
        lookPosition.y = enemigo.transform.position.y;
        enemigo.transform.LookAt(lookPosition);

        // Sistema de combo
        if (esperandoFinAnimacion && animacionCompletada)
        {
            animacionCompletada = false;
            esperandoFinAnimacion = false;

            if (!enemigo.ComboCompletado())
            {
                EjecutarAtaque();
            }
            else
            {
                enemigo.FinalizarCombo();
                EnemyManager.instance.LiberarEnemigo(enemigo);

                // Reevaluar comportamiento
                enemigo.EvaluarComportamiento();
            }
        }
    }

    private void EjecutarAtaque()
    {
        ObtenerAnimacionCombo();
        enemigo.SiguienteAtaqueEnCombo();
        esperandoFinAnimacion = true;
        animacionCompletada = false;
    }

    public void OnAnimacionAtaqueCompletada()
    {
        animacionCompletada = true;
    }

    private void ObtenerAnimacionCombo()
    {
        if (enemigo.TipoAtaqueActual == TipoAtaque.Ligero)
        {
            switch (enemigo.AtaqueActualEnCombo)
            {
                case 0: animator.CrossFade(LightAttack1Hash, duracionTransicion); break;
                case 1: animator.CrossFade(LightAttack2Hash, duracionTransicion); break;
                case 2: animator.CrossFade(LightAttack3Hash, duracionTransicion); break;
                default: animator.CrossFade(LightAttack1Hash, duracionTransicion); break;
            }
        }
        else
        {
            switch (enemigo.AtaqueActualEnCombo)
            {
                case 0: animator.CrossFade(HeavyAttack1Hash, duracionTransicion); break;
                case 1: animator.CrossFade(HeavyAttack2Hash, duracionTransicion); break;
                default: animator.CrossFade(HeavyAttack1Hash, duracionTransicion); break;
            }
        }
    }

    public override void OnExit()
    {
        enemigo.DesregistrarEstadoAtacar();
        enemigo.desactivarCollider();

        if (enemigo.EstaEnCombo)
        {
            enemigo.FinalizarCombo();
        }
    }
}