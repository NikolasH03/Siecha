using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HabilidadesJugadorUI : MonoBehaviour
{

    public void BotonDesbloquearCapoeira()
    {
        if (!HabilidadesJugador.instance.IntentarDesbloquearHabilidad(HabilidadesJugador.TipoHabilidad.Capoeira))
        {
            Debug.Log("no se puede desbloquear");
        }
    }

    public void BotonDesbloquearHabilidad2()
    {
        if (!HabilidadesJugador.instance.IntentarDesbloquearHabilidad(HabilidadesJugador.TipoHabilidad.Habilidad2))
        {
            Debug.Log("no se puede desbloquear");
        }
    }

}
