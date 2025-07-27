using UnityEngine;
using UnityEngine.UI;

public class ControladorRamasUI : MonoBehaviour
{

    [SerializeField] private NodoUI[] nodos;
    [SerializeField] private RamaHabilidadData[] ramasTisqa;
    [SerializeField] private RamaHabilidadData[] ramasAlejandro;
    [SerializeField] private RamaHabilidadData ramaMaldiciones;


    [Header("Contenedor para todos los nodos")]
    public GameObject contenedorNodos;

    //public void ConfigurarRamasSegunPersonaje()
    //{
    //    // Apagar todas las ramas inicialmente
    //    ramaMacana.SetActive(false);
    //    ramaArco.SetActive(false);
    //    ramaEstoque.SetActive(false);
    //    ramaArcabuz.SetActive(false);
    //    ramaMaldiciones.SetActive(false);
    //    contenedorNodos.SetActive(false);

    //    // Activar botones apropiados
    //    if (ControladorPersonajeUI.EsTisqa())
    //    {
    //        botonRamaArmaMelee.gameObject.SetActive(true);
    //        botonRamaArmaDistancia.gameObject.SetActive(true); // Segunda arma Tisqa
    //    }
    //    else if (ControladorPersonajeUI.EsAlejandro())
    //    {
    //        botonRamaArmaMelee.gameObject.SetActive(true);
    //        botonRamaArmaDistancia.gameObject.SetActive(true); // Segunda arma Alejandro
    //    }

    //    botonRamaMaldicion.gameObject.SetActive(true); // Siempre disponible
    //}

    //public void MostrarRamaArma1()
    //{
    //    //ramaArmaTisqa.SetActive(ControladorPersonajeUI.EsTisqa());
    //    //ramaArmaAlejandro.SetActive(ControladorPersonajeUI.EsAlejandro());
    //    ramaMaldiciones.SetActive(false);
    //    contenedorNodos.SetActive(true);
    //}

    //public void MostrarRamaArma2()
    //{
    //    // Podrías agregar otros árboles o visuales distintos
    //    Debug.Log("Mostrando segunda rama de arma para " + ControladorPersonajeUI.ObtenerPersonajeActual());
    //    // Aquí podrías alternar entre ramas secundarias si están divididas.
    //}

    //public void MostrarRamaMaldicion()
    //{
    //    //ramaArmaTisqa.SetActive(false);
    //    //ramaArmaAlejandro.SetActive(false);
    //    ramaMaldiciones.SetActive(true);
    //    contenedorNodos.SetActive(true);
    //}
}


