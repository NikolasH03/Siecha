using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ArmaEnemigo : MonoBehaviour
{
    [SerializeField] private Enemigo enemigo;
    private Collider armaCollider;

    private void Awake()
    {
        armaCollider = GetComponent<Collider>();
        if (enemigo == null)
        {
            enemigo = GetComponentInParent<Enemigo>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si golpeó al jugador
        if (other.CompareTag("Player"))
        {
            ControladorCombate jugador = other.GetComponent<ControladorCombate>();

                Debug.Log($"Enemigo golpeó al jugador con {enemigo.TipoAtaqueActual} - Daño: {enemigo.ObtenerDanoActual()}");

            jugador.JugadorRecibeDano(enemigo.ObtenerDanoActual());
            
        }
    }
}