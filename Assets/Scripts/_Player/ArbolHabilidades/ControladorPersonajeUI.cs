using UnityEngine;

public class ControladorPersonajeUI : MonoBehaviour
{
    public enum PersonajeID { Tisqa = 0, Alejandro = 1 }
    public static PersonajeID personajeSeleccionado = PersonajeID.Tisqa;

    public void SeleccionarPersonaje(int id)
    {
        personajeSeleccionado = (PersonajeID)id;
        Debug.Log("Seleccionado personaje: " + personajeSeleccionado);
    }

    public PersonajeID ObtenerPersonajeActual()
    {
        return personajeSeleccionado;
    }

    public bool EsTisqa()
    {
        return personajeSeleccionado == PersonajeID.Tisqa;
    }

    public bool EsAlejandro()
    {
        return personajeSeleccionado == PersonajeID.Alejandro;
    }
}

