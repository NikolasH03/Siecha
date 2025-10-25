using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

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
    public void GuardarDatosJugador(
        float vidaMax,
        float vidaActual,
        float estaminaMax,
        int numMuertes,
        int dinero,
        bool[] coleccionables)
    {
        PlayerPrefs.SetFloat("VidaMax", vidaMax);
        PlayerPrefs.SetFloat("VidaActual", vidaActual);
        PlayerPrefs.SetFloat("EstaminaMax", estaminaMax);

        PlayerPrefs.SetInt("NumMuertes", numMuertes);
        PlayerPrefs.SetInt("Dinero", dinero);

        for (int i = 0; i < coleccionables.Length; i++)
        {
            PlayerPrefs.SetInt("Coleccionable_" + i, coleccionables[i] ? 1 : 0);
        }

        PlayerPrefs.Save(); 
        Debug.Log("Datos del jugador guardados");
    }

    // ===============================
    //         CARGA
    // ===============================
    public void CargarDatosJugador(
        out float vidaMax,
        out float vidaActual,
        out float estaminaMax,
        out int numMuertes,
        out int dinero,
        out bool[] coleccionables)
    {
        vidaMax = PlayerPrefs.GetFloat("VidaMax", 100f);
        vidaActual = PlayerPrefs.GetFloat("VidaActual", vidaMax);
        estaminaMax = PlayerPrefs.GetFloat("EstaminaMax", 100f);

        numMuertes = PlayerPrefs.GetInt("NumMuertes", 0);
        dinero = PlayerPrefs.GetInt("Dinero", 0);

        coleccionables = new bool[5];
        for (int i = 0; i < coleccionables.Length; i++)
        {
            coleccionables[i] = PlayerPrefs.GetInt("Coleccionable_" + i, 0) == 1;
        }

        Debug.Log("Datos del jugador cargados");
    }

    // ===============================
    //        REINICIAR DATOS
    // ===============================
    public void ReiniciarDatosJugador()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Datos del jugador reiniciados");
    }
}
