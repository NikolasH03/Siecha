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
    protected float bloquearDuration = 2.5f;  // Cambiado a protected para que la clase derivada pueda acceder
    private float tiempoUltimoBloqueo = 0f;
    private float intervaloBloqueo = 5f;
    public ControladorCombate player;

    // Variable para controlar el estado anterior
    private Estados estadoAnterior;

    public void Start()
    {
        if (autoselectTarget)
        {
            Target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        StartCoroutine(CalcularDistancia());
        estadoAnterior = Estado;
    }

    private void LateUpdate()
    {
        if (autoselectTarget && Target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Target = playerObj.transform;
                player = playerObj.GetComponent<ControladorCombate>();
            }
        }

        CheckEstado();
    }

    private void CheckEstado()
    {
        switch (Estado)
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
        estadoAnterior = Estado;
        Estado = state;

        switch (Estado)
        {
            case Estados.Patrulla:
                // Reiniciar variables de patrulla en la subclase
                Enemigo_Z enemigo = GetComponent<Enemigo_Z>();
                if (enemigo != null)
                {
                    enemigo.rutina = 0;
                    enemigo.cronometro = 0;
                }
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

        Debug.Log($"Cambiando estado de {estadoAnterior} a {Estado}");
    }

    public virtual void EstadoPatrulla()
    {
        if (Distance < disSeguir)
        {
            CambiarEstado(Estados.Seguir);
        }
    }

    public virtual void EstadoSeguir()
    {
        if (Distance < disAtacar)
        {
            CambiarEstado(Estados.atacar);
        }
        else if (Distance > disEscape)
        {
            CambiarEstado(Estados.Patrulla);
        }
    }

    public virtual void EstadoAtacar()
    {
        if (Distance > disAtacar + 0.5f)
        {
            CambiarEstado(Estados.Seguir);
        }

        if (player != null && player.getAtacando() && Time.time - tiempoUltimoBloqueo > intervaloBloqueo)
        {
            float Bloqueo = UnityEngine.Random.Range(0.0f, 1.0f);
            Debug.Log("Valor de Bloqueo: " + Bloqueo);

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

        if (bloquearTime >= bloquearDuration)
        {
            // Simplificamos la transición después del bloqueo
            if (Distance < disAtacar)
            {
                CambiarEstado(Estados.atacar);
            }
            else
            {
                CambiarEstado(Estados.Seguir);
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
            if (Target != null)
            {
                Distance = Vector3.Distance(transform.position, Target.position);
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
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