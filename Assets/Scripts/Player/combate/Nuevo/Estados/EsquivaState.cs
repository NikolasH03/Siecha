using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsquivaState : CombatState
{
    public EsquivaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.SetBool("dashing", true);
        combatController.anim.SetTrigger("Esquiva");
        combatController.GetComponent<Collider>().enabled = false;
        combatController.GetComponent<Rigidbody>().isKinematic = true;
        combatController.anim.SetBool("RecibeDaño", false);
        combatController.anim.SetBool("running", false);
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        combatController.GetComponent<Collider>().enabled = true;
        combatController.GetComponent<Rigidbody>().isKinematic = false;
    }
}
