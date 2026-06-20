using UnityEngine;

public class DisparoCargadoFallido : CombatState
{
    private const float TIMEOUT_SEGUNDOS = 2f;
    private float tiempoEnEstado = 0f;

    public DisparoCargadoFallido(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        tiempoEnEstado = 0f;
        combatController.tipoAtaque = "cargado";
        combatController.anim.SetTrigger("DisparoFallido");
    }

    public override void Update()
    {
        tiempoEnEstado += Time.deltaTime;

        if (tiempoEnEstado >= TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning("DisparoCargadoFallido: timeout. Revisa el Animation Event.");
            combatController.EntrarCooldownDisparo();
        }
    }

    public override void Exit() { }
}