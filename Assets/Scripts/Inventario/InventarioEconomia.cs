
using UnityEngine;
using UnityEngine.UI;

public class InventarioEconomia : MonoBehaviour
{

    public static InventarioEconomia instance;
    [SerializeField] UIEconomia uiEconomia;
    
    private int dineroPaco = 0;
    private int dineroTisqa = 0;

    private void Awake()
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

    public int getDinero()
    {
        if (ControladorCambiarPersonaje.instance.getEsMuisca()) { return dineroTisqa; }
        else { return dineroPaco; }      
    }
    public void SetDinero(int cont)
    {
        if (ControladorCambiarPersonaje.instance.getEsMuisca()) { dineroTisqa = cont; }
        else { dineroPaco = cont; }

        uiEconomia.RefrescarUI();
    }
    public void AumentarDinero(int cont)
    {
        if (ControladorCambiarPersonaje.instance.getEsMuisca()) { dineroTisqa += cont; }
        else { dineroPaco += cont; }

        uiEconomia.RefrescarUI();
    }
    public void RestarDinero(int cont)
    {
        if (ControladorCambiarPersonaje.instance.getEsMuisca()) { dineroTisqa -= cont; }
        else { dineroPaco -= cont; }

        uiEconomia.RefrescarUI();
    }


    public void enemigoMuerto(int tipoEnemigo)
    {
        switch (tipoEnemigo)
        {
            case 1: AumentarDinero(50); break;
            case 2: AumentarDinero(200); break;
            default: Debug.Log("No existe ese tipo de enemigo"); break;
        }
    }
}
