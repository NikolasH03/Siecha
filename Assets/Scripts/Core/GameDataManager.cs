using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public bool DatosJugadorGuardados = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===============================
    //         GUARDADO
    // ===============================
    public void GuardarDesdeJugador(ControladorCombate jugador)
    {
        var stats = jugador.stats;

        PlayerPrefs.SetFloat("VidaMax", stats.VidaMax);
        PlayerPrefs.SetFloat("VidaActual", stats.VidaActual);
        PlayerPrefs.SetFloat("EstaminaMax", stats.EstaminaMax);
        PlayerPrefs.SetInt("NumMuertes", jugador.muertesActuales);
        PlayerPrefs.SetInt("Dinero", InventarioEconomia.instance.getDinero());

        GuardarColeccionables();

        PlayerPrefs.Save();
        DatosJugadorGuardados = true;
        Debug.Log("[GameData] Datos guardados.");
    }

    // ===============================
    //         CARGA
    // ===============================
    public void CargarEnJugador(ControladorCombate jugador)
    {
        var stats = jugador.stats;

        jugador.CargarEstadisticasActuales(PlayerPrefs.GetFloat("VidaMax"), PlayerPrefs.GetFloat("EstaminaMax"), PlayerPrefs.GetFloat("VidaActual"));
        jugador.SetNumeroMuertes(PlayerPrefs.GetInt("NumMuertes"));  
        InventarioEconomia.instance.SetDinero(PlayerPrefs.GetInt("Dinero"));

        CargarColeccionables();

        DatosJugadorGuardados = false;
        Debug.Log("[GameData] Datos cargados al jugador.");
    }

    // ===============================
    //        REINICIAR DATOS
    // ===============================
    public void ReiniciarDatosJugador()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        DatosJugadorGuardados = false;
        Debug.Log("[GameData] Datos reiniciados.");
    }

    public void GuardarColeccionables()
    {
        var inventario = InventarioColeccionables.instance;
        if (inventario == null)
        {
            Debug.LogWarning("[GameData] No se encontró el InventarioColeccionables para guardar.");
            return;
        }

        for (int i = 0; i < inventario.TotalColeccionables(); i++)
        {
            int id = inventario.GetDatos(i).id;
            bool desbloqueado = inventario.EstaDesbloqueado(id);
            PlayerPrefs.SetInt("Coleccionable_" + id, desbloqueado ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log("[GameData] Coleccionables guardados.");
    }
    public void CargarColeccionables()
    {
        var inventario = InventarioColeccionables.instance;
        if (inventario == null)
        {
            Debug.LogWarning("[GameData] No se encontró el InventarioColeccionables para cargar.");
            return;
        }

        for (int i = 0; i < inventario.TotalColeccionables(); i++)
        {
            int id = inventario.GetDatos(i).id;
            if (PlayerPrefs.GetInt("Coleccionable_" + id, 0) == 1)
            {
                inventario.Desbloquear(id);
            }
        }

        Debug.Log("[GameData] Coleccionables cargados y aplicados.");
    }


}

