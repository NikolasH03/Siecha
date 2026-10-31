using UnityEngine;

public abstract class CombatState
{
    protected CombatStateMachine stateMachine;
    protected ControladorCombate combatController;
    protected Animator anim;
    protected ControladorMovimiento movimiento;


    public CombatState(CombatStateMachine stateMachine, ControladorCombate combatController)
    {
        this.stateMachine = stateMachine;
        this.combatController = combatController;
        movimiento = combatController.GetComponent<ControladorMovimiento>();
        anim = combatController.anim;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void HandleInput() { }


    // Metodo comun para verificar finisher
    protected bool IntentaEjecutarFinisher()
    {
        var enemigo = combatController.DetectarEnemigoStunned(5f);
        if (enemigo != null && InputJugador.instance.FinisherInput)
        {
            stateMachine.ChangeState(new FinisherState(stateMachine, combatController, enemigo));
            return true;
        }
        return false;
    }

    protected bool IntentaHacerAccionDefensiva()
    {
        if (InputJugador.instance.esquivar)
        {
            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return true;
        }
        if (InputJugador.instance.bloquear)
        {
            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new BloqueoState(stateMachine, combatController));
            return true;
        }

        return false;
    }

    protected void IntentaCambiarProtagonista()
    {
        if (InputJugador.instance.cambiarProtagonista)
        {
            ControladorCambiarPersonaje.instance.CambiarProtagonista();
        }
    }
    protected void IntentaCorrer()
    {
        if (InputJugador.instance.correr && InputJugador.instance.moverse.sqrMagnitude > 0.01f)
        {
            anim.SetBool("running", true);
            movimiento.CambiarVelocidad(true);
        }
        if (!InputJugador.instance.correr || InputJugador.instance.moverse.sqrMagnitude < 0.01f)
        {
            anim.SetBool("running", false);
            movimiento.CambiarVelocidad(false);
        }
    }
    
}
