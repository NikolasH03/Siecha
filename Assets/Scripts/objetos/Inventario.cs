
using UnityEngine;
using UnityEngine.UI;

public class Inventario : MonoBehaviour
{

    public static Inventario instance;

    
    private int dinero = 0;

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
    public void aumentarDinero(int cont)
    {
        dinero += cont;
    }


    public void enemigoMuerto(int tipoEnemigo)
    {
        switch (tipoEnemigo)
        {
            case 1: dinero += 50; break;
            case 2: dinero += 200; break;
            default: Debug.Log("No existe ese tipo de enemigo"); break;
        }
    }
}
