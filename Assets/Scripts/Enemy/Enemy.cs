using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Estados Estado;
    public float disSeguir;
    public float disAtacar;
    public float disEscape;
    public bool autoselectTarget = true;
    public Transform Target;
    public float Distance;
    public bool Alive = true;
    public float ProbBloqueo = 0.30f;
    private float bloquearTime; 
    private float bloquearDuration = 2.5f;
    private float tiempoUltimoBloqueo = 0f;
    private float intervaloBloqueo = 5f;

    public void Awake()
    {
        if(autoselectTarget) 
        {
            Target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        StartCoroutine(CalcularDistancia());
    }

    private void LateUpdate()
    {
        CheckEstado();
    }

    private void CheckEstado()
    {
        switch(Estado)
        {
            case Estados.Patrulla:
                EstadoPatrulla();
                break;
            case Estados.Seguir:
                EstadoSeguir();
                break;
            case Estados.atacar:
                EstadoAtacar();
                break;
            case Estados.Bloquear:
                EstadoBloquear();
                break;
            case Estados.muerto:
                EstadoMuerto();
                break;
        }
    }

    public void CambiarEstado(Estados state)
    {
        Estado = state;

        switch(Estado) 
        {
            case Estados.Patrulla:
                break;
            case Estados.Seguir:
                break;
            case Estados.atacar:
                break;
            case Estados.Bloquear:
                bloquearTime = 0f;
                break;
            case Estados.muerto:
                Alive = false;
                break;
            default:
                break;
        }
    }

    public virtual void EstadoPatrulla()
    {
          if(Distance < disSeguir)
        {
            CambiarEstado (Estados.atacar);
        }

    }

    public virtual void EstadoSeguir()
    {
        if (Distance < disAtacar)
        {
            CambiarEstado(Estados.atacar);
        }
        else if(Distance > disEscape)
        {
            CambiarEstado(Estados.Patrulla);
        }


    }

    public virtual void EstadoAtacar()
    {
        if (Distance > disAtacar + 0.5f)
        {
            CambiarEstado (Estados.Seguir);
        }

        if (Player.instance.atacandoDebil && Time.time - tiempoUltimoBloqueo > intervaloBloqueo)
        {
            float Bloqueo = UnityEngine.Random.Range(0.0f, 1.0f);

            //Debug.Log("Valor de Bloqueo: " + Bloqueo);

            if (Bloqueo <= ProbBloqueo)
            {
                CambiarEstado(Estados.Bloquear);
            }

            tiempoUltimoBloqueo = Time.time; 
        }
    }

    public virtual void EstadoBloquear()
    {

        bloquearTime += Time.deltaTime;

        //Debug.Log("Bloqueado!!!");

        if (bloquearTime >= bloquearDuration)
        {
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

    }


    public virtual void EstadoMuerto()
    {

    }

    IEnumerator CalcularDistancia()
    {
        while (Alive) 
        { 
            if(Target !=  null)
            {
                Distance = Vector3.Distance(transform.position, Target.position);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, disAtacar);
        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, Vector3.up, disSeguir);
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, disEscape);
    }
#endif
}

public enum Estados
{
    Patrulla = 0,
    Seguir = 1,
    atacar = 2,
    muerto = 3,
    Bloquear = 4,
}
