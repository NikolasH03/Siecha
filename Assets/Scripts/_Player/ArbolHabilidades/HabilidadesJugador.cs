using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HabilidadesJugador : MonoBehaviour
{
    public static HabilidadesJugador instance;

    [SerializeField] private List<HabilidadData> todasLasHabilidades;

    private HashSet<HabilidadData> desbloqueadas = new();

    private void Awake()
    {
        transform.parent = null;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool EstaDesbloqueada(HabilidadData habilidad)
    {
        return desbloqueadas.Contains(habilidad);
    }

    public bool PuedeDesbloquearse(HabilidadData habilidad)
    {
        if (EstaDesbloqueada(habilidad)) return false;

        if (habilidad.requisito != null && !EstaDesbloqueada(habilidad.requisito))
            return false;

        return InventarioEconomia.instance.getDinero() >= habilidad.costo;
    }

    public bool IntentarDesbloquear(HabilidadData habilidad)
    {
        if (!PuedeDesbloquearse(habilidad)) return false;

        InventarioEconomia.instance.RestarDinero(habilidad.costo);
        desbloqueadas.Add(habilidad);

        Debug.Log($"Habilidad desbloqueada: {habilidad.nombre}");
        return true;
    }

    public List<HabilidadData> ObtenerTodas()
    {
        return todasLasHabilidades;
    }

}

