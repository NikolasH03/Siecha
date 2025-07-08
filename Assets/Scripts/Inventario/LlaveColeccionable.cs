using UnityEngine;

public class LlaveColeccionable : MonoBehaviour
{
    [SerializeField] private int idColeccionable;
    [SerializeField] private GameObject canvasInteractuar;
    private bool jugadorCerca = false;

    public void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            canvasInteractuar.SetActive(false);
            InventarioColeccionables.instance.Desbloquear(idColeccionable);
            UIIndicadorRecolectado.instance.MostrarIndicador(idColeccionable);
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvasInteractuar.SetActive(true);
            jugadorCerca = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvasInteractuar.SetActive(false);
            jugadorCerca = false;
        }
    }
}
