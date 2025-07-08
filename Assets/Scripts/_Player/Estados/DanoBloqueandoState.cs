using UnityEngine;

public class DanoBloqueandoState : CombatState
{
    public DanoBloqueandoState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {

        combatController.GetComponent<Collider>().enabled = false;
        combatController.GetComponent<Rigidbody>().isKinematic = true;
        combatController.stats.UsarEstamina(combatController.stats.DanoBase);
        combatController.EmpezarRegeneracionEstamina();

        if (combatController.stats.EstaminaActual <= 0)
        {
            stateMachine.ChangeState(new PerderGuardiaState(stateMachine, combatController));
        }
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("DanoBloqueando");


    }
}
