using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class MenuBaseConNavegacion : MenuBase
{
    [Header("Navegación con Mando")]
    protected Selectable primerSeleccionable;
    [SerializeField] protected bool navegacionConMando = true;

    protected EventSystem eventSystem;

    protected virtual void OnEnable()
    {
        eventSystem = EventSystem.current;
        ConfigurarNavegacion();
    }

    public override void OpenMenu()
    {
        base.OpenMenu();

        if (navegacionConMando && primerSeleccionable != null)
        {
            StartCoroutine(SeleccionarEnProximoFrame());
        }
    }

    private System.Collections.IEnumerator SeleccionarEnProximoFrame()
    {
        yield return null;
        eventSystem.SetSelectedGameObject(primerSeleccionable.gameObject);
    }

    protected virtual void ConfigurarNavegacion()
    {
        // Implementar en clases hijas
    }

    protected void ConfigurarNavegacionBoton(Selectable boton,
        Selectable arriba = null, Selectable abajo = null,
        Selectable izquierda = null, Selectable derecha = null)
    {
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = arriba;
        nav.selectOnDown = abajo;
        nav.selectOnLeft = izquierda;
        nav.selectOnRight = derecha;
        boton.navigation = nav;
    }
}