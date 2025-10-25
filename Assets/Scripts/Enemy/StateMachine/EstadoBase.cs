using UnityEngine;
using UnityEngine.AI;

public abstract class EstadoBase : IEstado
{
    protected readonly Enemigo enemigo;
    protected readonly Animator animator;
    
    protected static readonly int IddleHash = Animator.StringToHash("Idle");
    protected static readonly int WalkingHash = Animator.StringToHash("Walking");
    protected static readonly int RunningHash = Animator.StringToHash("Running");
    protected static readonly int BlockHash = Animator.StringToHash("Blocking");
    protected static readonly int DamageHash = Animator.StringToHash("Hit");
    protected static readonly int DodgeHash = Animator.StringToHash("Dodge");
    protected static readonly int DeathHash = Animator.StringToHash("Dying");
    protected static readonly int GuardBreakHash = Animator.StringToHash("GuardBreak");
    protected static readonly int StunHash = Animator.StringToHash("Stun");

    // Ataques ligeros (combo de 3)
    protected static readonly int LightAttack1Hash = Animator.StringToHash("Ligero1");
    protected static readonly int LightAttack2Hash = Animator.StringToHash("Ligero2");
    protected static readonly int LightAttack3Hash = Animator.StringToHash("Ligero3");

    // Ataques fuertes (combo de 2)
    protected static readonly int HeavyAttack1Hash = Animator.StringToHash("Fuerte1");
    protected static readonly int HeavyAttack2Hash = Animator.StringToHash("Fuerte2");

    protected const float duracionTransicion = 0.1f;

    protected EstadoBase(Enemigo enemigo, Animator animator)
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