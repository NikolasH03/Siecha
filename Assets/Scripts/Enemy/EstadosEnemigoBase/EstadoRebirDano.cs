using System;
using UnityEngine;

public class EstadoRebirDano : EstadoBase
{
    private readonly Temporizador temporizadorDano;
    private readonly HealthComp vidaEnemigo;

    public EstadoRebirDano(Enemigo enemigo, Animator animator, HealthComp vidaEnemigo, float duracionDano) : base(enemigo, animator)
    {
        this.vidaEnemigo = vidaEnemigo;
       this.temporizadorDano = new Temporizador(duracionDano);
    }

    public override void OnEnter()
    {
        Debug.Log("Recibiendo Daño!");
        enemigo.desactivarCollider();
        animator.CrossFade(DamageHash, duracionTransicion);
        vidaEnemigo.setRecibiendoDano(false);
        temporizadorDano.Empezar();
    }

    public override void Update()
    {
        temporizadorDano.Tick(Time.deltaTime);
    }


    public bool TerminoTiempoDano => temporizadorDano.HaFinalizado;
}