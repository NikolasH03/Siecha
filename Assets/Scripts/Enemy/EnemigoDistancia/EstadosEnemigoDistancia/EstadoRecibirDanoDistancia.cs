using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Versión de EstadoRebirDano para EnemigoDistancia.
/// 
/// Sobreescribe TransicionarAlSiguienteEstado para que, al terminar 
/// la animación de daño, llame a EvaluarComportamiento en lugar de ir 
/// ciegamente a EstadoSeguirJugador.
/// </summary>
public class EstadoRecibirDanoDistancia : EstadoRebirDano
{
    public EstadoRecibirDanoDistancia(
        Enemigo      enemigo,
        Animator     animator,
        NavMeshAgent agente,
        HealthComp   vidaEnemigo,
        float        duracionDano)
        : base(enemigo, animator, agente, vidaEnemigo, duracionDano)
    { }

    protected override void TransicionarAlSiguienteEstado()
    {
        // Forzamos al arquero a evaluar su contexto táctico (reposicionarse, disparar, etc.)
        ((EnemigoDistancia)enemigo).EvaluarComportamiento();
    }

    // Ya no necesitamos sobreescribir OnExit. 
    // La clase base limpiará el NavMeshAgent correctamente.
}