using UnityEngine;
using UnityEngine.UI;

public class HealthComp : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMax = 20f;
    [SerializeField] private Image imagenBarraVida;
    private float vidaActual;
    
    [Header("Golpes Para Empezar A Bloquear")]
    [SerializeField] private int golpesAntesDeBloquear = 2;
    [SerializeField] private int golpesRecibidos = 0;
    
    [Header("Stamina")]
    [SerializeField] private float staminaMax = 100f;
    [SerializeField] private float costoBloqueo = 20f;
    [SerializeField] private float regeneracionStamina = 10f; // Por segundo
    [SerializeField] private float duracionGuardBreak = 2f;

    private float staminaActual;
    private bool enGuardBreak = false;
    private Temporizador timerGuardBreak;

    [Header("Stun")]
    [SerializeField] private float duracionStun = 2f;
    private bool estaStuneado = false;
    private Temporizador timerStun;

    [Header("Booleanos Adicionales")]
    private bool estaBloqueado;
    private bool estaMuerto = false;
    private bool estaSiendoDanado = false;
    private bool estaEsquivando = false;
    private bool danoPendiente = false;


    public bool EstaMuerto => estaMuerto;
    public bool EstaSiendoDanado => estaSiendoDanado;
    public bool EstaEsquivando => estaEsquivando;
    public bool EstaStuneado => estaStuneado; // 👈 Nuevo getter público
    public bool EnGuardBreak => enGuardBreak;

    private void Start()
    {
        vidaActual = vidaMax;
        staminaActual = staminaMax;

        timerGuardBreak = new Temporizador(duracionGuardBreak);
        timerGuardBreak.OnTimerStop += RecuperarGuardBreak;

        timerStun = new Temporizador(duracionStun);
        timerStun.OnTimerStop += () => estaStuneado = false;

        ActualizarBarra();
    }

    private void ActualizarBarra()
    {
        if (imagenBarraVida != null)
            imagenBarraVida.fillAmount = Mathf.Clamp01(vidaActual / vidaMax);
    }

    public void recibeDano(int cantidad)
    {
        if (estaMuerto || estaEsquivando) return;

        // Si está bloqueando, no recibe daño, pero gasta stamina
        if (estaBloqueado)
        {
            ConsumirStaminaPorBloqueo();
            return;
        }

        golpesRecibidos++;
        vidaActual -= cantidad;
        ActualizarBarra();

        danoPendiente = true;
        estaSiendoDanado = true;

        if (vidaActual <= 0f)
        {
            estaMuerto = true;
        }
    }

    public void setRecibiendoDano(bool valor) => estaSiendoDanado = valor;

    public void setBloqueado(bool valor)
    {
        estaBloqueado = valor;
        setRecibiendoDano(false);
    }
    public bool getBloqueando()
    {
        return estaBloqueado;
    }
    
    public bool DebeBloquear() => golpesRecibidos >= golpesAntesDeBloquear;

    public void setEsquivando(bool valor) => estaEsquivando = valor;

    public void SetFinisher()
    {
        vidaActual = 0f;
        estaMuerto = true;
    }
    public void Eliminar()
    {
        InventarioEconomia.instance.enemigoMuerto(1);
        Debug.Log("Enemigo eliminado");
        gameObject.SetActive(false);
    }
    
    public bool EnimigoFueDanado()
    {
        if (danoPendiente && !estaMuerto && !estaBloqueado)
        {
            danoPendiente = false;
            return true;
        }
        return false;
    }

    public bool EnemigoHaMuerto() => estaMuerto;
    
    // --- Stamina / Guard Break ---
    public void ConsumirStaminaPorBloqueo()
    {
        staminaActual -= costoBloqueo;
        staminaActual = Mathf.Clamp(staminaActual, 0, staminaMax);
        Debug.Log(staminaActual);

        if (staminaActual <= 0 && !enGuardBreak)
        {
            Debug.Log("Guardia Rota!!!");
            enGuardBreak = true;
            timerGuardBreak.Empezar();

            // Cuando se rompe la guardia, activar stun
            EntrarEnStun();
        }
    }
    
    public void TickTimers(float deltaTime)
    {
        timerGuardBreak.Tick(deltaTime);
        timerStun.Tick(deltaTime);
    }

    private void RecuperarGuardBreak()
    {
        enGuardBreak = false;
        staminaActual = staminaMax * 0.5f;
    }
    
    public void RegenerarStamina(float deltaTime)
    {
        if (!enGuardBreak && staminaActual < staminaMax)
        {
            staminaActual += regeneracionStamina * deltaTime;
            staminaActual = Mathf.Clamp(staminaActual, 0, staminaMax);
        }
    }
    
    public void RestablecerEstamina() => staminaActual = staminaMax;

    // --- Stun ---
    public void EntrarEnStun()
    {
        estaStuneado = true;
        timerStun.Reiniciar();
        timerStun.Empezar();
    }
}
