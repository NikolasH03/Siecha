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

    private void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        // Asegúrate de que todas las animaciones están desactivadas al inicio
        if (animator != null)
        {
            animator.SetBool("Caminando", false);
            animator.SetBool("Correr", false);
            animator.SetBool("Atacando", false);
            animator.SetBool("Bloquear", false);
        }

        DebugAnimationNames();
    }

    public override void EstadoPatrulla()
    {
        base.EstadoPatrulla();
        if (animator != null)
        {
            animator.SetBool("Atacando", false);
            animator.SetBool("Correr", false);
            animator.SetBool("Bloquear", false);
        }

        // Asegurarse de que el NavMeshAgent esté activo
        if (!agent.enabled) agent.enabled = true;

        Comportamiento_Enemigo();
    }

    public override void EstadoSeguir()
    {
        base.EstadoSeguir();
        if (animator != null)
        {
            animator.SetBool("Caminando", false);
            animator.SetBool("Atacando", false);
            animator.SetBool("Correr", true);
            animator.SetBool("Bloquear", false);
        }

        // Asegurarse de que el NavMeshAgent esté activo
        if (!agent.enabled) agent.enabled = true;

        agent.SetDestination(Target.position);
        transform.LookAt(Target.position);
    }

    public override void EstadoAtacar()
    {
        if (estaAtacando) return;
        base.EstadoAtacar();
        StartCoroutine(EjecutarAtaque());
        if (animator != null)
        {
            animator.SetBool("Caminando", false);
            animator.SetBool("Correr", false);
            animator.SetBool("Atacando", true);
            animator.SetBool("Bloquear", false);
        }
        agent.SetDestination(transform.position);
        transform.LookAt(Target.position);
    }

    public override void EstadoBloquear()
    {
        base.EstadoBloquear();
        if (animator != null)
        {
            // Resetear todos los parámetros primero
            animator.SetBool("Caminando", false);
            animator.SetBool("Correr", false);
            animator.SetBool("Atacando", false);
            animator.SetBool("Bloquear", true);

            // Forzar la entrada inmediata al estado de bloqueo en el Animator
            // Intenta con el nombre exacto de la animación en tu Animator Controller
            try
            {
                animator.Play("Standing Block", 0, 0f);
            }
            catch
            {
                // Si "Bloqueo" no funciona, intenta otras variantes comunes
                try
                {
                    animator.Play("Standing Block", 0, 0f);
                }
                catch
                {
                    try
                    {
                        // Otra alternativa es buscar cualquier animación que contenga "bloqueo" o "block"
                        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                        {
                            if (clip.name.ToLower().Contains("bloqueo") || clip.name.ToLower().Contains("block"))
                            {
                                animator.Play(clip.name, 0, 0f);
                                break;
                            }
                        }
                    }
                    catch
                    {
                        Debug.LogError("No se pudo encontrar la animación de bloqueo");
                    }
                }
            }

            Debug.Log("Activando animación de bloqueo");
        }
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
            rutina = Random.Range(0, 2);
            cronometro = 0;
        }
        switch (rutina)
        {
            case 0:
                if (animator != null) animator.SetBool("Caminando", false);
                break;

            case 1:
                grado = Random.Range(0, 360);
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

        yield return new WaitForSeconds(DuracionDeAnimacion());

        estaAtacando = false;

        if (Distance > disAtacar + 0.5f)
        {
            CambiarEstado(Estados.Seguir);
        }
    }

    private float DuracionDeAnimacion()
    {
        AnimatorStateInfo infoAnim = animator.GetCurrentAnimatorStateInfo(0);
        return infoAnim.length;
    }

    void DebugAnimationNames()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                Debug.Log("Animación disponible: " + clip.name);
            }
        }
    }
}