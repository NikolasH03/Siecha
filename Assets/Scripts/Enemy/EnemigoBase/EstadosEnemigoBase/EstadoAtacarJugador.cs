using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Combo de ataques del enemigo melee. Cada ataque espera el evento de animación
/// OnAnimacionAtaqueCompletada antes de lanzar el siguiente; al terminar el combo
/// devuelve el slot de ataque al EnemyManager y reevalúa el comportamiento.
/// </summary>
public class EstadoAtacarJugador : EstadoBase
{
    private const float FactorDistanciaDeAtaque = 0.8f;

    private readonly NavMeshAgent agent;
    private readonly float rangoDeAtaque;

    private bool esperandoFinAnimacion;
    private bool animacionCompletada;

    public EstadoAtacarJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, float rangoDeAtaque)
        : base(enemigo, animator)
    {
        this.agent         = agent;
        this.rangoDeAtaque = rangoDeAtaque;
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

        MantenerDistanciaYMirar(jugador);

        if (esperandoFinAnimacion && animacionCompletada)
            AvanzarCombo();
    }

    public override void OnExit()
    {
        enemigo.DesregistrarEstadoAtacar();
        enemigo.FinalizarCombo();

        esperandoFinAnimacion = false;
        animacionCompletada   = false;
        agent.isStopped       = false;
    }

    /// <summary>Evento de animación: el ataque actual terminó.</summary>
    public void OnAnimacionAtaqueCompletada() => animacionCompletada = true;

    // ─── Combo ────────────────────────────────────────────────────────────────

    private void AvanzarCombo()
    {
        animacionCompletada   = false;
        esperandoFinAnimacion = false;

        if (!enemigo.ComboCompletado())
        {
            EjecutarAtaque();
            return;
        }

        enemigo.FinalizarCombo();
        EnemyManager.instance.LiberarEnemigo(enemigo);
        enemigo.EvaluarComportamiento();
    }

    private void EjecutarAtaque()
    {
        ReproducirAnimacionCombo();
        enemigo.SiguienteAtaqueEnCombo();

        esperandoFinAnimacion = true;
        animacionCompletada   = false;
    }

    private void ReproducirAnimacionCombo()
    {
        int indice = enemigo.AtaqueActualEnCombo;
        int hash   = enemigo.TipoAtaqueActual == TipoAtaque.Ligero
            ? HashAtaqueLigero(indice)
            : HashAtaqueFuerte(indice);

        animator.CrossFade(hash, duracionTransicion);
    }

    private int HashAtaqueLigero(int indice)
    {
        switch (indice)
        {
            case 1:  return LightAttack2Hash;
            case 2:  return LightAttack3Hash;
            default: return LightAttack1Hash;
        }
    }

    private int HashAtaqueFuerte(int indice) => indice == 1 ? HeavyAttack2Hash : HeavyAttack1Hash;

    // ─── Movimiento ───────────────────────────────────────────────────────────

    /// <summary>Se coloca a una fracción del rango de ataque del jugador, mirándolo (sin inclinarse en Y).</summary>
    private void MantenerDistanciaYMirar(Transform jugador)
    {
        Vector3 haciaJugador = (jugador.position - enemigo.transform.position).normalized;
        agent.SetDestination(jugador.position - haciaJugador * (rangoDeAtaque * FactorDistanciaDeAtaque));

        Vector3 mirar = jugador.position;
        mirar.y = enemigo.transform.position.y;
        enemigo.transform.LookAt(mirar);
    }
}