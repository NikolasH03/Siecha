using UnityEngine;

public class InventarioColeccionables : MonoBehaviour
{
    public static InventarioColeccionables instance;

    [SerializeField] ColeccionableData[] coleccionables;
    private bool[] desbloqueados;

    [SerializeField] UIColeccionables uiColeccionables;

    private void Awake()
    {
        instance = this;
        desbloqueados = new bool[coleccionables.Length];
    }

    public void Desbloquear(int id)
    {
        for (int i = 0; i < coleccionables.Length; i++)
        {
            if (coleccionables[i].id == id)
            {
                desbloqueados[i] = true;
                Debug.Log("Coleccionable desbloqueado: " + coleccionables[i].nombre);

                if (uiColeccionables != null)
                    uiColeccionables.RefrescarUI();

                return;
            }
        }

        Debug.LogWarning("No se encontró coleccionable con id: " + id);
    }

    public bool EstaDesbloqueado(int id)
    {
        for (int i = 0; i < coleccionables.Length; i++)
        {
            if (coleccionables[i].id == id)
            {
                return desbloqueados[i];
            }
        }

        return false;
    }
    public ColeccionableData GetDatos(int id)
    {
        return coleccionables[id];
    }
    public ColeccionableData GetDatosPorID(int id)
    {
        for (int i = 0; i < coleccionables.Length; i++)
        {
            if (coleccionables[i].id == id)
            {
                return coleccionables[i];
            }
        }
        return null;


    }
    public int TotalColeccionables()
    {
        return coleccionables.Length;
    }
}

