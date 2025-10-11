using System.Collections.Generic;
using UnityEngine;

public class AutoTargeting : MonoBehaviour
{
    [SerializeField] private float rangoBusqueda = 4f;
    [SerializeField] private float anguloBusqueda = 90f;
    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Transform referenciaRotacion;

    private Transform enemigoObjetivo;

    public void BuscarYOrientar()
    {
        Collider[] enemigos = Physics.OverlapSphere(transform.position, rangoBusqueda, capaEnemigos);
        float mejorAngulo = anguloBusqueda;
        Transform mejorObjetivo = null;

        foreach (Collider col in enemigos)
        {
            Vector3 direccionAlEnemigo = (col.transform.position - transform.position).normalized;
            float angulo = Vector3.Angle(referenciaRotacion.forward, direccionAlEnemigo);

            if (angulo < mejorAngulo)
            {
                mejorAngulo = angulo;
                mejorObjetivo = col.transform;
            }
        }

        if (mejorObjetivo != null)
        {
            Vector3 direccion = (mejorObjetivo.position - transform.position).normalized;
            direccion.y = 0;
            Quaternion rotacion = Quaternion.LookRotation(direccion);
            referenciaRotacion.rotation = rotacion;
            enemigoObjetivo = mejorObjetivo;
        }
    }
}
