using UnityEngine;
using UnityEngine.AI;

public abstract class EstadoBase : IEstado
{
    protected readonly Enemigo enemigo;
    protected readonly Animator animator;
    
    protected static readonly int IddleHash = Animator.StringToHash("Neutral Idle");
    protected static readonly int WalkingHash = Animator.StringToHash("Walking");
    protected static readonly int RunningHash = Animator.StringToHash("Runing");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");
    protected static readonly int Attack1Hash = Animator.StringToHash("punchAttack");
    protected static readonly int HeavyAttackHash = Animator.StringToHash("heavyAttack");
    protected static readonly int BlockHash = Animator.StringToHash("Bloqueo");
    protected static readonly int DamageHash = Animator.StringToHash("damage");
    protected static readonly int SecuenceHash = Animator.StringToHash("Secuencia");
    protected static readonly int DodgeHash = Animator.StringToHash("Esquivar Izquierda");
    protected static readonly int DeathHash = Animator.StringToHash("Falling Back Death");
    protected static readonly int GuardBreakHash = Animator.StringToHash("Rompe Guardia");
    protected static readonly int StunHash = Animator.StringToHash("StunAnimation");

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