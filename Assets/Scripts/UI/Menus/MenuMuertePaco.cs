using UnityEngine;

public class MenuMuertePaco : MenuMuerteBase
{
    [Header("Referencias Paco")]
    [SerializeField] private HUDJugador paco;

    private void Start()
    {
        MenuManager.Instance.CloseAllMenus();
        paco = GameObject.FindWithTag("Player").GetComponent<HUDJugador>();
        paco.ActualizarContadorMuertes();
    }

    public override void Continuar()
    {
        if (paco != null)
        {
            paco.Reaparecer();
        }

        MenuManager.Instance?.GoBack();

        Debug.Log("paco continúa después de morir");
    }
}