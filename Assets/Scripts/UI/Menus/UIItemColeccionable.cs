using UnityEngine;
using UnityEngine.UI;
public class UIItemColeccionable : MonoBehaviour
{
    [SerializeField] Image icono;
    [SerializeField] Button boton;
    private ColeccionableData data;
    private bool activado = false;

    public void Configurar(ColeccionableData datos, bool activo)
    {
        data = datos;
        icono.sprite = datos.icono;

        if (activo)
        {
            boton.interactable = true;
            icono.color = Color.white;
            activado = true;
        }
        else
        {
            boton.interactable = false;
            icono.color = Color.gray;
        }
    }
    public void Mostrar()
    {
        if (activado)
        {
            MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuVisualizador3D);
            UIVisualizador3D.instance.Mostrar(data);
        }
    }
}
