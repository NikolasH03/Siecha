using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HabilidadesJugador : MonoBehaviour
{
    public static HabilidadesJugador instance;
    public void Awake()
    {
        transform.parent = null;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    public enum TipoHabilidad
    {
        None,
        Capoeira,
        Habilidad2,
    }

    private List<TipoHabilidad> ListaHabilidadesDesbloqueadas;

    public HabilidadesJugador()
    {
        ListaHabilidadesDesbloqueadas = new List<TipoHabilidad> ();
    }

    private void DesbloquearHabilidad(TipoHabilidad tipoHabilidad)
    {
        if (!estaDesbloqueada(tipoHabilidad))
        {
            ListaHabilidadesDesbloqueadas.Add(tipoHabilidad);
        }
        
    }
    public bool IntentarDesbloquearHabilidad(TipoHabilidad tipoHabilidad)
    {
        TipoHabilidad requerimientoHabilidad = ObtenerRequerimientoHabilidad(tipoHabilidad);
        int dineroRequerido = ObtenerCostoHabilidad(tipoHabilidad);

        if(requerimientoHabilidad != TipoHabilidad.None)
        {
            if (estaDesbloqueada(requerimientoHabilidad))
            {
                if (InventarioEconomia.instance.getDinero() >= dineroRequerido)
                {
                    DesbloquearHabilidad(tipoHabilidad);
                    Debug.Log("todo melo");
                    return true;
                }
                return false; 
            }
            else { return false; }
          
        }
        else
        {
            if (InventarioEconomia.instance.getDinero() >= dineroRequerido)
            {
                DesbloquearHabilidad(tipoHabilidad);
                return true;
            }   
            return false;

            
        }
    }

    public bool estaDesbloqueada(TipoHabilidad tipoHabilidad)
    {
        return ListaHabilidadesDesbloqueadas.Contains (tipoHabilidad);
    }

    public TipoHabilidad ObtenerRequerimientoHabilidad(TipoHabilidad tipoHabilidad)
    {
        switch (tipoHabilidad)
        {
            case TipoHabilidad.Habilidad2: return TipoHabilidad.Capoeira;
        
        }
        return TipoHabilidad.None;

    }

    public int ObtenerCostoHabilidad(TipoHabilidad tipoHabilidad)
    {
        switch (tipoHabilidad)
        {
            case TipoHabilidad.Capoeira: return 50;
            case TipoHabilidad.Habilidad2: return 100;

        }
        return 0;

    }
}
