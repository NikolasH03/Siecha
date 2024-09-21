using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent))]

public class Enemigo_Z : Enemy
{
    private NavMeshAgent agent;
    public int rutina;
    public float cronometro;
    public Animator animator;
    public Quaternion angulo;
    public float grado;
    public float speedWalk = 2;

    private void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        
    }

    private void Start()
    {
        //animator = GetComponent<Animator>();
    }
    public override void EstadoPatrulla()
    {
        base.EstadoPatrulla();
        if (animator != null) animator.SetBool("Atacando", false);
        if (animator != null) animator.SetBool("Correr", false);
        Comportamiento_Enemigo();
    }

    public override void EstadoSeguir()
    {
        base.EstadoSeguir();
        if(animator != null)animator.SetBool("Caminando", false);
        if(animator != null)animator.SetBool("Atacando", false);
        if(animator != null) animator.SetBool("Correr", true);
        agent.SetDestination(Target.position);
        transform.LookAt(Target.position);
    }

    public override void EstadoAtacar()
    {
        base.EstadoAtacar();
        if (animator != null) animator.SetBool("Caminando", false);
        if (animator != null) animator.SetBool("Correr", false);
        if(animator != null)animator.SetBool("Atacando", true);
        if (animator != null) animator.SetBool("Bloquear", false);
        agent.SetDestination(transform.position);
        transform.LookAt(Target.position);
    }

    public override void EstadoBloquear()
    {
        base.EstadoBloquear();
        if (animator != null) animator.SetBool("Atacando", false);
        if (animator != null) animator.SetBool("Bloquear", true);
    }

    public override void EstadoMuerto()
    {
        base.EstadoMuerto();
        if(animator != null)animator.SetBool("Vivo", false) ;
        agent.enabled = false;
    }

    public void Comportamiento_Enemigo()
    {

        cronometro += 1 * Time.deltaTime;
        if(cronometro >= 3)
        {
            rutina = Random.Range(0, 2);
            cronometro = 0;
        }
        switch(rutina) 
        {
            case 0:
                if(animator != null)animator.SetBool("Caminando", false);
                break;

            case 1:
                grado = Random.Range(0, 360);
                angulo = Quaternion.Euler(0, grado, 0);
                rutina++;
                break;

            case 2:
                transform.rotation = Quaternion.RotateTowards(transform.rotation, angulo, 0.5f);
                transform.Translate(Vector3.forward * speedWalk * Time.deltaTime);
                if(animator != null)animator.SetBool("Caminando", true);
                break;
        }
    }

}
