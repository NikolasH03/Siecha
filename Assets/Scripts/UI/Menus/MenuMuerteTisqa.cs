using UnityEngine;

public class MenuMuerteTisqa : MenuMuerteBase
{
    [Header("Referencias Tisqa")]
    [SerializeField] private HUDJugador tisqa; 
    
    public override void OpenMenu()
    {
        // Buscamos al Player fresco de la escena actual
        GameObject playerObj = GameObject.FindWithTag("Player");
        
        if (playerObj != null)
        {
            tisqa = playerObj.GetComponent<HUDJugador>();
            tisqa.ActualizarContadorMuertes();
        }
        else
        {
            Debug.LogWarning("[MenuMuerteTisqa] No se encontró al Player en la escena al intentar abrir el menú.");
        }
        
        base.OpenMenu(); 
    }

    public override void Continuar()
    {
        if (tisqa != null)
        {
            tisqa.Reaparecer();
        }
        else
        {
            // Fail-safe: Si por algún motivo la referencia se perdió, la busca en caliente
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                tisqa = playerObj.GetComponent<HUDJugador>();
                tisqa.Reaparecer();
            }
        }

        MenuManager.Instance?.GoBack();

        Debug.Log("Tisqa continúa después de morir");
    }
}