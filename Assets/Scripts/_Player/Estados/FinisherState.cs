using UnityEngine;

public class FinisherState : CombatState
{
    private HealthComp target;

    public FinisherState(CombatStateMachine fsm, ControladorCombate combatController, HealthComp target)
        : base(fsm, combatController)
    {
        this.target = target;
    }

    public override void Enter()
    {  
        combatController.setAtacando(true);
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("Finisher");

        combatController.camaraFinisher.gameObject.SetActive(true);
        combatController.camaraFinisher.LookAt = target.transform;
        combatController.camaraFinisher.Follow = target.transform;

        combatController.transform.position = target.transform.position + (-target.transform.forward * 2f);
        combatController.transform.LookAt(target.transform.position);
    }
    public override void Exit()
    {
        combatController.setAtacando(false);
        combatController.camaraFinisher.LookAt = null;
        combatController.camaraFinisher.gameObject.SetActive(false);
        target.SetFinisher();

    }
}

