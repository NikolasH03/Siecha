using UnityEngine;

public class CargandoDisparo : CombatState
{
    private ControladorApuntado apuntado;

    private const float TIMEOUT_SEGUNDOS = 5f;
    private float tiempoEnEstado = 0f;

    public CargandoDisparo(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        tiempoEnEstado = 0f;
        
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());
        apuntado.SetEstaApuntando(true);

        combatController.anim.SetTrigger("CargarDisparo");
        combatController.Reproducir("inicio_disparo_cargado");
    }

    public override void HandleInput()
    {
        if (InputJugador.instance.holdSuccess)
        {
            stateMachine.ChangeState(new DisparoCargadoExitoso(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.holdFail)
        {
            stateMachine.ChangeState(new DisparoCargadoFallido(stateMachine, combatController));
            return;
        }
    }

    public override void Update()
    {
        tiempoEnEstado += Time.deltaTime;
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());

        if (tiempoEnEstado >= TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning("CargandoDisparo: timeout. Tratando como disparo fallido.");
            stateMachine.ChangeState(new DisparoCargadoFallido(stateMachine, combatController));
        }
    }

    public override void Exit()
    {
        apuntado.SetEstaApuntando(false);
    }
}