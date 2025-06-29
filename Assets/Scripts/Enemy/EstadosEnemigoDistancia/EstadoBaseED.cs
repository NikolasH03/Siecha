using UnityEngine;
using UnityEngine.AI;

public abstract class EstadoBaseED : IEstado
{
    protected readonly EnemigoADistancia enemigo;
    protected readonly Animator animator;
    
    protected static readonly int IddleHash = Animator.StringToHash("Iddle");
    protected static readonly int WalkingHash = Animator.StringToHash("Caminando");
    protected static readonly int AttackHash = Animator.StringToHash("Atacando");
    // protected static readonly int DamageHash = Animator.StringToHash("damage");
    protected static readonly int DodgeHash = Animator.StringToHash("Esquivando");
    // protected static readonly int DeathHash = Animator.StringToHash("Falling Back Death");

    protected const float duracionTransicion = 0.1f;

    protected EstadoBaseED(EnemigoADistancia enemigo, Animator animator)
    {
        this.enemigo = enemigo;
        this.animator = animator;
    }
    

    public virtual void OnEnter()
    {
        // noop
    }

    public virtual void Update()
    {
        // noop
    }

    public virtual void FixedUpdate()
    {
        // noop
    }

    public virtual void OnExit()
    {
        // noop
    }
    
}