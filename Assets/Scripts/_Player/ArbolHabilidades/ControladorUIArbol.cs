using UnityEngine;


public class ControladorUIArbol : MonoBehaviour
{
    public enum EstadoUI
    {
        SeleccionProtagonista,
        SeleccionRama,
        VistaRama,
        ConfirmacionCompra
    }

    [Header("Secciones de la UI")]
    [SerializeField] private GameObject panelSeleccionProtagonista;
    [SerializeField] private GameObject panelSeleccionRamasTisqa;
    [SerializeField] private GameObject panelSeleccionRamasAlejandro;
    [SerializeField] private GameObject panelVistaRama;
    [SerializeField] private GameObject panelConfirmacion;



    [Header("Controladores externos")]
    public ControladorPersonajeUI controladorPersonaje;
    public ControladorRamasUI controladorRama;
    public VisualizadorNodoUI visualizadorHabilidad;

    private EstadoUI estadoActual;

    private void Start()
    {
        CambiarEstado(EstadoUI.SeleccionProtagonista);
    }

    public void CambiarEstado(EstadoUI nuevoEstado)
    {

        panelSeleccionProtagonista.SetActive(false);
        panelSeleccionRamasTisqa.SetActive(false);
        panelSeleccionRamasAlejandro.SetActive(false);
        panelVistaRama.SetActive(false);
        panelConfirmacion.SetActive(false);

        estadoActual = nuevoEstado;

        switch (estadoActual)
        {
            case EstadoUI.SeleccionProtagonista:
                panelSeleccionProtagonista.SetActive(true);
                break;
            case EstadoUI.SeleccionRama:
                if (controladorPersonaje.EsTisqa())
                    panelSeleccionRamasTisqa.SetActive(true);
                else if (controladorPersonaje.EsAlejandro())
                    panelSeleccionRamasAlejandro.SetActive(true);
                break;
            case EstadoUI.VistaRama:
                panelVistaRama.SetActive(true);
                //controladorRama.MostrarRamaActual(); 
                break;
            case EstadoUI.ConfirmacionCompra:
                panelConfirmacion.SetActive(true);
                break;
        }
    }

    // Llamado desde botón UI
    public void SeleccionarProtagonista(int id)
    {
        controladorPersonaje.SeleccionarPersonaje(id);
        CambiarEstado(EstadoUI.SeleccionRama);
    }

    //public void MostrarRama(int idRama)
    //{
    //    RamaHabilidadData ramaActual;

    //    if (controladorPersonaje.EsTisqa())
    //        ramaActual = ramasTisqa[idRama];
    //    else if (controladorPersonaje.EsAlejandro())
    //        ramaActual = ramasAlejandro[idRama];
    //    else
    //        ramaActual = ramaMaldiciones;

    //    for (int i = 0; i < nodos.Length; i++)
    //    {
    //        if (i < ramaActual.habilidades.Length)
    //        {
    //            nodos[i].Configurar(ramaActual.habilidades[i]);
    //        }
    //        else
    //        {
    //            nodos[i].gameObject.SetActive(false);
    //        }
    //    }
    //}
    public void VerConfirmacionCompra(HabilidadData data)
    {
        visualizadorHabilidad.Mostrar(data);
        CambiarEstado(EstadoUI.ConfirmacionCompra);
    }

    //public void ConfirmarCompra()
    //{
    //    if (controladorRama.IntentarComprarSeleccionada())
    //    {
    //        CambiarEstado(EstadoUI.VistaRama);
    //    }
    //    else
    //    {
    //        Debug.Log("No se pudo comprar.");
    //    }
    //}


    public void BotonVolver()
    {
        switch (estadoActual)
        {
            case EstadoUI.SeleccionRama:
                CambiarEstado(EstadoUI.SeleccionProtagonista);
                break;
            case EstadoUI.VistaRama:
                CambiarEstado(EstadoUI.SeleccionRama);
                break;
            case EstadoUI.ConfirmacionCompra:
                CambiarEstado(EstadoUI.VistaRama);
                break;
        }
    }
}

