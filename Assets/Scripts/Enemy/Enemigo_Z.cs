using System;
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
    public bool estaAtacando = false;
    public bool establoqueando = false;

    //private void Awake()
    //{
    //    base.Start();
    //    agent = GetComponent<NavMeshAgent>();

    //}

    private void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
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
        if (animator != null) animator.SetBool("Caminando", false);
        if (animator != null) animator.SetBool("Atacando", false);
        if (animator != null) animator.SetBool("Correr", true);
        agent.SetDestination(Target.position);
        transform.LookAt(Target.position);
    }

    public override void EstadoAtacar()
    {
        if (estaAtacando) return;
        base.EstadoAtacar();
        StartCoroutine(EjecutarAtaque());
        if (animator != null) animator.SetBool("Caminando", false);
        if (animator != null) animator.SetBool("Correr", false);
        if (animator != null) animator.SetBool("Atacando", true);
        if (animator != null) animator.SetBool("Bloquear", false);
        agent.SetDestination(transform.position);
        transform.LookAt(Target.position);


    }

    public override void EstadoBloquear()
    {
        if (establoqueando) return;
        base.EstadoBloquear();
        StartCoroutine(EjecutarBloqueo());
        if (animator != null) animator.SetBool("Atacando", false);
        if (animator != null) animator.SetBool("Bloquear", true);
    }

    public override void EstadoMuerto()
    {
        base.EstadoMuerto();
        if (animator != null) animator.SetBool("Vivo", false);
        agent.enabled = false;
    }

    public void Comportamiento_Enemigo()
    {

        cronometro += 1 * Time.deltaTime;
        if (cronometro >= 3)
        {
            rutina = UnityEngine.Random.Range(0, 2);
            cronometro = 0;
        }
        switch (rutina)
        {
            case 0:
                if (animator != null) animator.SetBool("Caminando", false);
                break;

            case 1:
                grado = UnityEngine.Random.Range(0, 360);
                angulo = Quaternion.Euler(0, grado, 0);
                rutina++;
                break;

            case 2:
                transform.rotation = Quaternion.RotateTowards(transform.rotation, angulo, 0.5f);
                transform.Translate(Vector3.forward * speedWalk * Time.deltaTime);
                if (animator != null) animator.SetBool("Caminando", true);
                break;
        }
    }

    private IEnumerator EjecutarAtaque()
    {
        estaAtacando = true;

        //Debug.Log("Bloqueado!!!");

        yield return new WaitForSeconds(DuracionDeAnimacion());

        estaAtacando = false;

        if (Distance > disAtacar + 0.5f)
        {
            CambiarEstado(Estados.Seguir);
        }
    }
    private IEnumerator EjecutarBloqueo()
    {
        establoqueando = true;
        yield return new WaitForSeconds(DuracionDeAnimacion());
        establoqueando = false;

        if (Distance < disSeguir)
        {
            CambiarEstado(Estados.atacar);
        }

        if (Distance > disAtacar + 0.5f)
        {
            CambiarEstado(Estados.Seguir);
        }

        if (Distance < disAtacar)
        {
            CambiarEstado(Estados.atacar);
        }

        else if (Distance > disEscape)
        {
            CambiarEstado(Estados.Patrulla);
        }

    }


    private float DuracionDeAnimacion()
    {
        AnimatorStateInfo infoAnim = animator.GetCurrentAnimatorStateInfo(0);
        return infoAnim.length;
    }

}