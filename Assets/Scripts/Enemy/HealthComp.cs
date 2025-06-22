using UnityEngine;
using UnityEngine.UI;

public class HealthComp : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMax = 20f;
    [SerializeField] private Image imagenBarraVida;

    private float vidaActual;
    
    private bool estaBloqueado;
    private bool estaMuerto = false;
    private bool estaSiendoDanado = false;
    private bool estaEsquivando = false;

    public bool EstaMuerto => estaMuerto;
    public bool EstaSiendoDanado => estaSiendoDanado;
    public bool EstaEsquivando => estaEsquivando;

    private void Start()
    {
        vidaActual = vidaMax;
        ActualizarBarra();
    }

    private void ActualizarBarra()
    {
        if (imagenBarraVida != null)
            imagenBarraVida.fillAmount = Mathf.Clamp01(vidaActual / vidaMax);
    }

    public void recibeDano(int cantidad)
    {
        if (estaMuerto || estaBloqueado) return;
        

        vidaActual -= cantidad;
        ActualizarBarra();

        if (vidaActual <= 0f)
        {
            estaMuerto = true;
        }
    }

    public void setRecibiendoDano(bool valor)
    {
        estaSiendoDanado = valor;
    }

    public void setBloqueado(bool valor)
    {
        estaBloqueado = valor;
        setRecibiendoDano(false);
    }

    public void setEsquivando(bool valor)
    {
        estaEsquivando = valor;
    }

    public void Eliminar()
    {
        Inventario.instance.enemigoMuerto(1);
        Debug.Log("Enemigo eliminado");
        gameObject.SetActive(false);
    }
    
    public bool EnimigoFueDanado()
    {
        return estaSiendoDanado && !estaMuerto && !estaBloqueado;
    }

    public bool EnemigoHaMuerto()
    {
        return estaMuerto && vidaActual >= 0f;
    }
}

