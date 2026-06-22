using UnityEngine;

public class MenuMuertePaco : MenuMuerteBase
{
    [Header("Referencias Paco")]
    [SerializeField] private HUDJugador paco;

    public override void OpenMenu()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        
        if (playerObj != null)
        {
            paco = playerObj.GetComponent<HUDJugador>();
            paco.ActualizarContadorMuertes();
        }
        else
        {
            Debug.LogWarning("[MenuMuertePaco] No se encontró al Player en la escena al intentar abrir el menú.");
        }
        
        base.OpenMenu(); 
    }

    public override void Continuar()
    {
        if (paco != null)
        {
            paco.Reaparecer();
        }
        else
        {
            // Fail-safe: Si por alguna razón la referencia se volvió null, la busca en caliente
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                paco = playerObj.GetComponent<HUDJugador>();
                paco.Reaparecer();
            }
        }

        MenuManager.Instance?.GoBack();
        Debug.Log("Paco continúa después de morir");
    }
}