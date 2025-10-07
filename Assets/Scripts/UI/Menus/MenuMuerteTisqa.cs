using UnityEngine;

public class MenuMuerteTisqa : MenuMuerteBase
{
    [Header("Referencias Tisqa")]
    [SerializeField] private HUDJugador tisqa; 

    private void Start()
    {
        MenuManager.Instance.CloseAllMenus();
        tisqa = GameObject.FindWithTag("Player").GetComponent<HUDJugador>();
        tisqa.ActualizarContadorMuertes();  
    }

    public override void Continuar()
    {
        if (tisqa != null)
        {
            tisqa.Reaparecer();
        }

        MenuManager.Instance?.GoBack();

        Debug.Log("tisqa continúa después de morir");
    }
}