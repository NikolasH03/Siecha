
using UnityEngine;
using UnityEngine.UI;

public class InventarioEconomia : MonoBehaviour
{

    public static InventarioEconomia instance;

    
    [SerializeField] private int dinero = 0;

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
        return dinero;
    }
    public void setDinero(int cont)
    {
        dinero = cont;
    }
    public void AumentarDinero(int cont)
    {
        dinero += cont;
    }
    public void RestarDinero(int cont)
    {
        dinero -= cont;
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
