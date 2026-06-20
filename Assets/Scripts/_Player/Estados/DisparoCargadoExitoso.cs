using UnityEngine;

public class DisparoCargadoExitoso : CombatState
{
    private ControladorApuntado apuntado;

    private const float TIMEOUT_SEGUNDOS = 2f;
    private float tiempoEnEstado = 0f;

    public DisparoCargadoExitoso(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        tiempoEnEstado = 0f;
        combatController.tipoAtaque = "cargado";
        combatController.anim.SetTrigger("DisparoExitoso");
        apuntado.InstanciarBala(apuntado.ObtenerPosicionObjetivo());
        combatController.InvulneravilidadJugador();
    }

    public override void Update()
    {
        tiempoEnEstado += Time.deltaTime;

        if (tiempoEnEstado >= TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning("DisparoCargadoExitoso: timeout. Revisa el Animation Event.");
            combatController.EntrarCooldownDisparo();
        }
    }

    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
    }
}