using UnityEngine;

public class UIColeccionables : MonoBehaviour
{
    [SerializeField] GameObject contenedor;
    [SerializeField] GameObject prefabItemUI;

    public void RefrescarUI()
    {
        foreach (Transform hijo in contenedor.transform)
        {
            Destroy(hijo.gameObject);
        }


        for (int i = 0; i < InventarioColeccionables.instance.TotalColeccionables(); i++)
        {
            GameObject item = Instantiate(prefabItemUI, contenedor.transform);
            ColeccionableData datos = InventarioColeccionables.instance.GetDatos(i);
            bool activo = InventarioColeccionables.instance.EstaDesbloqueado(datos.id);
            item.GetComponent<UIItemColeccionable>().Configurar(datos, activo);
        }
    }

    private void Start()
    {
        RefrescarUI(); 
    }
}
