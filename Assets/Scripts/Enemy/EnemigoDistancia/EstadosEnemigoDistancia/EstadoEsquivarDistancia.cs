using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Versión del esquive para EnemigoDistancia.
/// Hereda de EstadoDeEsquivar para ser compatible con el campo
/// estadoEsquivar del base (que es de tipo EstadoDeEsquivar).
/// 
/// ÚNICA DIFERENCIA:
/// Sobreescribe IrAEstadoPostEsquive() para ir a EstadoRodearDistancia
/// en lugar de EstadoRodearJugador.
/// </summary>
public class EstadoEsquivarDistancia : EstadoDeEsquivar
{
    private readonly EstadoRodearDistancia estadoRodear;
    private readonly EnemigoDistancia      enemigoDistancia;

    public EstadoEsquivarDistancia(
        Enemigo               enemigo,
        Animator              animator,
        NavMeshAgent          agent,
        HealthComp            vidaEnemigo,
        float                 distanciaEsquivar,
        float                 velocidadEsquivo,
        EstadoRodearDistancia estadoRodear)
        : base(enemigo, animator, agent, vidaEnemigo, distanciaEsquivar, velocidadEsquivo)
    {
        this.estadoRodear      = estadoRodear;
        this.enemigoDistancia  = (EnemigoDistancia)enemigo;
    }

    protected override void IrAEstadoPostEsquive()
    {
        enemigoDistancia.CambiarAEstadoRodear();
    }
}
