using UnityEngine;
using UnityEngine.AI;

public class EstadoAtacarJugador : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly float rangoDeAtaque;
    private readonly EnemyStats stats;

    private Temporizador timerEntreAtaques;
    private bool esperandoSiguienteAtaque = false;

    private bool esperandoFinAnimacion = false;
    private bool animacionCompletada = false;


    public EstadoAtacarJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent,
        float rangoDeAtaque)
        : base(enemigo, animator)
    {
        this.agent = agent;
        this.rangoDeAtaque = rangoDeAtaque;
        this.stats = enemigo.Stats;
    }

    public override void OnEnter()
    {
        Debug.Log("Entrando a estado Atacar");

        enemigo.RegistrarEstadoAtacar(this);

        TipoAtaque tipoAtaque = DecidirTipoAtaque();
        enemigo.IniciarCombo(tipoAtaque);

        // Iniciar el primer ataque
        EjecutarAtaque();
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;
        if (jugador == null) return;

        // Posicionamiento y orientación
        Vector3 direccionAlJugador = (jugador.position - enemigo.transform.position).normalized;
        float distanciaDeseada = rangoDeAtaque * 0.8f; // Un poco más cerca para los ataques
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
                // Ejecutar siguiente ataque del combo
                EjecutarAtaque();
            }
            else
            {
                // Combo completado
                enemigo.FinalizarCombo();
            }
        }
    }

    private void EjecutarAtaque()
    {
        // Elegir animación según tipo de ataque y número en el combo
        ObtenerAnimacionCombo();

        // Avanzar contador del combo
        enemigo.SiguienteAtaqueEnCombo();

        // Marcar que estamos esperando que termine la animación
        esperandoFinAnimacion = true;
        animacionCompletada = false;
    }
    public void OnAnimacionAtaqueCompletada()
    {
        Debug.Log("Animación de ataque completada - listo para siguiente");
        animacionCompletada = true;
    }

    private void ObtenerAnimacionCombo()
    {
        int numeroAtaque = enemigo.AtaqueActualEnCombo + 1; // Para el log

        if (enemigo.TipoAtaqueActual == TipoAtaque.Ligero)
        {
            // Combo ligero: 3 animaciones diferentes
            switch (enemigo.AtaqueActualEnCombo)
            {
                case 0:
                    animator.CrossFade(LightAttack1Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque ligero 1 (ataque {numeroAtaque} del combo)");
                    break;
                case 1:
                    animator.CrossFade(LightAttack2Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque ligero 2 (ataque {numeroAtaque} del combo)");
                    break;
                case 2:
                    animator.CrossFade(LightAttack3Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque ligero 3 (ataque {numeroAtaque} del combo)");
                    break;
                default:
                    animator.CrossFade(LightAttack1Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque ligero 1 (default)");
                    break;
            }
        }
        else
        {
            // Combo fuerte: 2 animaciones diferentes
            switch (enemigo.AtaqueActualEnCombo)
            {
                case 0:
                    animator.CrossFade(HeavyAttack1Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque fuerte 1 (ataque {numeroAtaque} del combo)");
                    break;
                case 1:
                    animator.CrossFade(HeavyAttack2Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque fuerte 2 (ataque {numeroAtaque} del combo)");
                    break;
                default:
                    animator.CrossFade(HeavyAttack1Hash, duracionTransicion);
                    Debug.Log($"Ejecutando ataque fuerte 1 (default)");
                    break;
            }
        }
    }

    // ← TEMPORAL: Más adelante esto lo hará el Utility AI
    private TipoAtaque DecidirTipoAtaque()
    {
        // Por ahora, simple: si el jugador está bloqueando, usar ataques fuertes
        bool jugadorBloqueando = JugadorEstaBloqueando();

        if (jugadorBloqueando)
        {
            Debug.Log("Jugador bloqueando - usando ataques fuertes");
            return TipoAtaque.Fuerte;
        }

        // 70% ataques ligeros, 30% fuertes (aleatorio por ahora)
        return Random.value < 0.7f ? TipoAtaque.Ligero : TipoAtaque.Fuerte;
    }

    private bool JugadorEstaBloqueando()
    {
        Transform jugador = enemigo.JugadorActual;
        if (jugador == null) return false;

        var controladorCombate = jugador.GetComponent<ControladorCombate>();
        if (controladorCombate == null) return false;

        return controladorCombate.getBloqueando(); 
    }

    public override void OnExit()
    {
        Debug.Log("Saliendo del estado Atacar");

        enemigo.DesregistrarEstadoAtacar();

        enemigo.desactivarCollider();
        enemigo.FinalizarCombo();
    }
}