using UnityEngine;
using UnityEngine.SceneManagement;

public class MuerteDefinitivaState : CombatState
{
    public MuerteDefinitivaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.SetBool("Muere", true);
        combatController.anim.Play("morir");

        combatController.GetComponent<Collider>().enabled = false;
        combatController.GetComponent<Rigidbody>().isKinematic = true;

        combatController.StartCoroutine(VolverMenu());
    }

    private System.Collections.IEnumerator VolverMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Menu");
        GameFlowManager.Instance.ReiniciarFlujoDeJuego();
    }
}