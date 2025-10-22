using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthComp : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMax = 20f;
    [SerializeField] private Image imagenBarraVida;
    private float vidaActual;
    
    [Header("Golpes Para Empezar A Bloquear")]
    [SerializeField] private int golpesAntesDeBloquear = 3;
    [SerializeField] private int golpesRecibidos = 0;
    
    [Header("Stamina")]
    [SerializeField] private float staminaMax = 10f;
    [SerializeField] private float regeneracionStamina = 1f; // Por segundo
    [SerializeField] private float delayRegeneracionStamina = 5f;
    [SerializeField] private float duracionGuardBreak = 2f;
    [SerializeField] private Image imagenBarraStamina;

    private float staminaActual;
    private bool enGuardBreak = false;
    private Temporizador timerGuardBreak;
    private Coroutine regeneracionCoroutine;

    [Header("Stun")]
    [SerializeField] private float duracionStun = 2f;
    private bool estaStuneado = false;

    [Header("UI Finisher")] 
    [SerializeField] private GameObject uiBarrasNormales;
    [SerializeField] private GameObject uiFinisher;

    [Header("Booleanos Adicionales")]
    private bool estaBloqueado;
    private bool estaMuerto = false;
    private bool estaSiendoDanado = false;
    private bool estaEsquivando = false;
    private bool danoPendiente = false;
    private bool estaEnFinisher = false;


    public bool EstaMuerto => estaMuerto;
    public bool EstaSiendoDanado => estaSiendoDanado;
    public bool EstaEsquivando => estaEsquivando;
    public bool EstaStuneado => estaStuneado; 
    public bool EnGuardBreak => enGuardBreak;
    public bool EstaEnFinisher => estaEnFinisher;
    public float DuracionStun => duracionStun;

    private void Start()
    {
        vidaActual = vidaMax;
        staminaActual = staminaMax;

        timerGuardBreak = new Temporizador(duracionGuardBreak);
        timerGuardBreak.OnTimerStop += RecuperarGuardBreak;

        ActualizarBarra();
        ActualizarBarraStamina();
        MostrarUIBarras();
    }

    private void ActualizarBarra()
    {
        if (imagenBarraVida != null)
            imagenBarraVida.fillAmount = Mathf.Clamp01(vidaActual / vidaMax);
    }

    public void recibeDano(int cantidad)
    {
        if (estaMuerto || estaEsquivando || estaEnFinisher) return;

        if (estaBloqueado)
        {
            ConsumirStaminaPorBloqueo(cantidad); 
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
        estaEnFinisher = true; 
        OcultarUIFinisher();
    }
    public void FinalizarFinisher()
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
    
    public bool EnemigoFueDanado()
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
    public void ConsumirStaminaPorBloqueo(int danoBloqueado)
    {
        float staminaConsumida = danoBloqueado;
        staminaActual -= staminaConsumida;
        staminaActual = Mathf.Clamp(staminaActual, 0, staminaMax);

        ActualizarBarraStamina();

        Debug.Log($"Bloqueó {danoBloqueado} de daño. Stamina restante: {staminaActual}");

        EmpezarRegeneracionEstamina();

        if (staminaActual <= 0 && !enGuardBreak)
        {
            Debug.Log("¡Guardia Rota!");
            enGuardBreak = true;
            timerGuardBreak.Empezar();
            EntrarEnStun();
        }
    }
    private void ActualizarBarraStamina()
    {
        if (imagenBarraStamina != null)
            imagenBarraStamina.fillAmount = Mathf.Clamp01(staminaActual / staminaMax);
    }

    public void TickTimers(float deltaTime)
    {
        timerGuardBreak.Tick(deltaTime);
    }

    private void RecuperarGuardBreak()
    {
        enGuardBreak = false;
        staminaActual = staminaMax;

        ActualizarBarraStamina();
    }

    public void RegenerarStamina(float cantidad)
    {
        if (!enGuardBreak && staminaActual < staminaMax)
        {
            staminaActual += cantidad;
            staminaActual = Mathf.Clamp(staminaActual, 0, staminaMax);
            ActualizarBarraStamina();
        }
    }
    public void EmpezarRegeneracionEstamina()
    {
        if (regeneracionCoroutine != null)
            StopCoroutine(regeneracionCoroutine);

        regeneracionCoroutine = StartCoroutine(RegenerarEstaminaConDelay());
    }
    private IEnumerator RegenerarEstaminaConDelay()
    {
        yield return new WaitForSeconds(delayRegeneracionStamina);

        while (staminaActual < staminaMax && !enGuardBreak)
        {
            RegenerarStamina(regeneracionStamina * Time.deltaTime);
            yield return null;
        }

        regeneracionCoroutine = null;
    }

    public void RestablecerEstamina() {
        staminaActual = staminaMax; 
        ActualizarBarraStamina();
    }

    // --- Stun ---
    public void EntrarEnStun()
    {
        estaStuneado = true;
        MostrarUIFinisher();
        Debug.Log("Entrando en stun - UI Finisher mostrado");
    }
    public void SalirDeStun()
    {
        Debug.Log($"SalirDeStun llamado - estaEnFinisher: {estaEnFinisher}");

        if (!estaEnFinisher)
        {
            estaStuneado = false;
            OcultarUIFinisher();
            MostrarUIBarras();
            Debug.Log("Salió del stun normalmente - UI restaurado");
        }
        else
        {
            Debug.Log("Stun pausado por finisher - UI ya oculto");
        }
    }

    //Gestión de UI
    private void MostrarUIBarras()
    {
        if (uiBarrasNormales != null)
        {
            uiBarrasNormales.SetActive(true);
            OcultarUIFinisher();
        }

    }

    private void OcultarUIBarras()
    {
        if (uiBarrasNormales != null)
            uiBarrasNormales.SetActive(false);
    }

    private void MostrarUIFinisher()
    {
        if (uiFinisher != null)
        {
            uiFinisher.SetActive(true);
            OcultarUIBarras();
        }
    }

    private void OcultarUIFinisher()
    {
        if (uiFinisher != null)
            uiFinisher.SetActive(false);
    }
    public bool SePuedeHacerFinisher()
    {
        return estaStuneado && !estaEnFinisher && !estaMuerto;
    }

    public void SetGolpesRecibidos(int golpes)
    {
        golpesRecibidos = golpes;
    }
}
