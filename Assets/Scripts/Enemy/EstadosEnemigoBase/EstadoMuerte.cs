using System.Collections;
using UnityEngine;

public class EstadoMuerte : EstadoBase
{
    private readonly float tiempoDeDesaparicion;
    private readonly HealthComp vidaEnemigo;

    public EstadoMuerte(Enemigo enemigo, Animator animator, HealthComp vidaEnemigo, float tiempoDeDesaparicion) : base(enemigo, animator)
    {
        this.vidaEnemigo = vidaEnemigo;
        this.tiempoDeDesaparicion = tiempoDeDesaparicion;
    }

    public override void OnEnter()
    {
        Debug.Log("Muelto!");
        animator.CrossFade(DeathHash, duracionTransicion);
        
        var col = enemigo.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var rb = enemigo.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        enemigo.StartCoroutine(EliminarEnemigo(tiempoDeDesaparicion));
    }

    private IEnumerator EliminarEnemigo(float delay)
    {
        yield return new WaitForSeconds(delay);
        vidaEnemigo.Eliminar();
    }
}