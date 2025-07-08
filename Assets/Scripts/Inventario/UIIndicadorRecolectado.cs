using UnityEditor;
using UnityEngine;
public class UIIndicadorRecolectado : MonoBehaviour
{
    public static UIIndicadorRecolectado instance;

    [SerializeField] GameObject panelIndicador;
    private int idActual;
    private bool puedeAbrirVisualizador = false;

    private void Awake()
    {
        instance = this;
    }

    public void MostrarIndicador(int idColeccionable)
    {
        idActual = idColeccionable;
        puedeAbrirVisualizador = true;

        panelIndicador.SetActive(true);

        CancelInvoke(nameof(OcultarIndicador));
        Invoke(nameof(OcultarIndicador), 5f);
    }

    private void OcultarIndicador()
    {
        panelIndicador.SetActive(false);
        puedeAbrirVisualizador = false;
    }

    private void Update()
    {
        if (puedeAbrirVisualizador && Input.GetKeyDown(KeyCode.Tab))
        {

            ColeccionableData data = InventarioColeccionables.instance.GetDatosPorID(idActual);
            PausaUI.instance.Pausar();
            PausaUI.instance.MostrarColeccionables();
            UIVisualizador3D.instance.Mostrar(data);



            OcultarIndicador();
        }
    }
}

