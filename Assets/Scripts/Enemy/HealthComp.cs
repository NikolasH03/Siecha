using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class HealthComp : MonoBehaviour
{
    // Los parámetros de configuración ya NO son SerializeField aquí.
    // Viven en EnemyStats y se inyectan por Inicializar().
    private float vidaMax;
    private float staminaMax;
    private float regeneracionStamina;
    private float delayRegeneracionStamina;
    private float duracionGuardBreak;
    private float duracionStun;
    private int   golpesAntesDeBloquear;

    [Header("UI - Barras")]
    [SerializeField] private Image imagenBarraVida;
    [SerializeField] private Image imagenBarraStamina;

    [Header("UI - Finisher")]
    [SerializeField] private GameObject uiBarrasNormales;
    [SerializeField] private GameObject uiFinisher;

    // Estado interno
    private float vidaActual;
    private float staminaActual;
    private int   golpesRecibidos = 0;
    private bool  enGuardBreak    = false;
    private bool  estaBloqueado   = false;
    private bool  estaMuerto      = false;
    private bool  terminaAnimacionMuerte = false;
    private bool  estaSiendoDanado = false;
    private bool  estaEsquivando   = false;
    private bool  danoPendiente    = false;
    private bool  estaEnFinisher   = false;
    private bool  estaStuneado     = false;
    private bool  inicializado     = false;

    private Temporizador timerGuardBreak;
    private Coroutine    regeneracionCoroutine;

    // ─── Eventos ──────────────────────────────────────────────────────────────
    public static event Action OnEnemyMuerto;

    // ─── Propiedades públicas ─────────────────────────────────────────────────
    public bool  EstaMuerto             => estaMuerto;
    public bool  TerminaAnimacionMuerte => terminaAnimacionMuerte;
    public bool  EstaSiendoDanado       => estaSiendoDanado;
    public bool  EstaEsquivando         => estaEsquivando;
    public bool  EstaStuneado           => estaStuneado;
    public bool  EnGuardBreak           => enGuardBreak;
    public bool  EstaEnFinisher         => estaEnFinisher;
    public float DuracionStun           => duracionStun;
    public float StaminaActual          => staminaActual;
    public float StaminaMax             => staminaMax;

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por Enemigo.Start() con los valores del EnemyStats.
    /// Centraliza toda la configuración en el SO — no hay valores
    /// hardcodeados ni duplicados entre componentes.
    /// </summary>
    public void Inicializar(EnemyStats stats)
    {
        vidaMax                    = stats.VidaMax;
        staminaMax                 = stats.StaminaMax;
        regeneracionStamina        = stats.RegeneracionStamina;
        delayRegeneracionStamina   = stats.DelayRegeneracionStamina;
        duracionGuardBreak         = stats.DuracionGuardBreak;
        duracionStun               = stats.DuracionStun;
        golpesAntesDeBloquear      = stats.GolpesAntesDeBloquear;

        vidaActual    = vidaMax;
        staminaActual = staminaMax;

        timerGuardBreak = new Temporizador(duracionGuardBreak);
        timerGuardBreak.OnTimerStop += RecuperarGuardBreak;

        ActualizarBarra();
        ActualizarBarraStamina();
        MostrarUIBarras();

        inicializado = true;
    }

    // ─── Daño ─────────────────────────────────────────────────────────────────

    public void recibeDano(int cantidad)
    {
        if (!inicializado || estaMuerto || estaEsquivando || estaEnFinisher) return;

        if (estaBloqueado)
        {
            ConsumirStaminaPorBloqueo(cantidad);
            return;
        }

        if (estaStuneado)
            SalirDeStun();

        golpesRecibidos++;
        vidaActual = Mathf.Max(vidaActual - cantidad, 0f);
        ActualizarBarra();

        danoPendiente    = true;
        estaSiendoDanado = true;

        if (vidaActual <= 0f)
        {
            estaMuerto = true;
            DetenerRegeneracion();
        }
    }

    public void TerminarDanoRecibido()    => estaSiendoDanado = false;
    public void TerminarAnimacionMuerte() => terminaAnimacionMuerte = true;
    public void setRecibiendoDano(bool v) => estaSiendoDanado = v;

    public bool EnemigoFueDanado()
    {
        if (danoPendiente && !estaMuerto && !estaBloqueado)
        {
            danoPendiente = false;
            return true;
        }
        return false;
    }

    // ─── Muerte ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Desregistra al enemigo del EnemyManager antes de desactivar
    /// el objeto, evitando referencias null en las listas del Manager.
    /// </summary>
    public void Eliminar()
    {
        var enemigo = GetComponent<Enemigo>();
        if (enemigo != null && EnemyManager.instance != null)
            EnemyManager.instance.DesregistrarEnemigo(enemigo);

        OnEnemyMuerto?.Invoke();
        gameObject.SetActive(false);
    }

    // ─── Bloqueo ──────────────────────────────────────────────────────────────

    public void setBloqueado(bool valor)
    {
        estaBloqueado = valor;
        setRecibiendoDano(false);
    }

    public bool getBloqueando() => estaBloqueado;
    public bool DebeBloquear()  => golpesRecibidos >= golpesAntesDeBloquear;

    // ─── Esquive ──────────────────────────────────────────────────────────────

    public void setEsquivando(bool valor) => estaEsquivando = valor;

    // ─── Finisher ─────────────────────────────────────────────────────────────

    public void SetFinisher()
    {
        estaEnFinisher = true;
        OcultarUIFinisher();
    }

    public void FinalizarFinisher()
    {
        vidaActual = 0f;
        estaMuerto = true;
        DetenerRegeneracion();
    }

    public bool SePuedeHacerFinisher() => estaStuneado && !estaEnFinisher && !estaMuerto;

    // ─── Stamina / Guard Break ────────────────────────────────────────────────

    public void ConsumirStaminaPorBloqueo(int danoBloqueado)
    {
        staminaActual = Mathf.Clamp(staminaActual - danoBloqueado, 0f, staminaMax);
        ActualizarBarraStamina();
        EmpezarRegeneracionEstamina();

        if (staminaActual <= 0f && !enGuardBreak)
        {
            enGuardBreak = true;
            timerGuardBreak.Empezar();
            EntrarEnStun();
        }
    }

    public void TickTimers(float deltaTime)
    {
        timerGuardBreak?.Tick(deltaTime);
    }

    private void RecuperarGuardBreak()
    {
        enGuardBreak  = false;
        staminaActual = staminaMax;
        ActualizarBarraStamina();
    }

    public void RegenerarStamina(float cantidad)
    {
        if (!enGuardBreak && staminaActual < staminaMax)
        {
            staminaActual = Mathf.Clamp(staminaActual + cantidad, 0f, staminaMax);
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

        while (staminaActual < staminaMax && !enGuardBreak && !estaMuerto)
        {
            RegenerarStamina(regeneracionStamina * Time.deltaTime);
            yield return null;
        }

        regeneracionCoroutine = null;
    }

    private void DetenerRegeneracion()
    {
        if (regeneracionCoroutine != null)
        {
            StopCoroutine(regeneracionCoroutine);
            regeneracionCoroutine = null;
        }
    }

    public void RestablecerEstamina()
    {
        staminaActual = staminaMax;
        ActualizarBarraStamina();
    }
    //Actualiza parámetros de combate sin tocar la vida actual-Sirve para el jefe
    public void ActualizarParametrosDeCombate(EnemyStats stats)
    {
        staminaMax               = stats.StaminaMax;
        regeneracionStamina      = stats.RegeneracionStamina;
        delayRegeneracionStamina = stats.DelayRegeneracionStamina;
        golpesAntesDeBloquear    = stats.GolpesAntesDeBloquear;
    }

    // ─── Stun ─────────────────────────────────────────────────────────────────

    public void EntrarEnStun()
    {
        estaStuneado = true;
        MostrarUIFinisher();
    }

    public void SalirDeStun()
    {
        if (!estaEnFinisher)
        {
            estaStuneado = false;
            OcultarUIFinisher();
            MostrarUIBarras();
        }
    }

    // ─── Vida max y stamina max (para tótems) ─────────────────────────────────

    public void AumentarVidaMax(float cantidad)
    {
        vidaMax    += cantidad;
        vidaActual  = Mathf.Min(vidaActual + cantidad, vidaMax);
        ActualizarBarra();
    }

    public void AumentarEstaminaMax(float cantidad)
    {
        staminaMax    += cantidad;
        staminaActual  = Mathf.Min(staminaActual + cantidad, staminaMax);
        ActualizarBarraStamina();
    }

    // ─── Getters ──────────────────────────────────────────────────────────────

    public void  SetGolpesRecibidos(int g) => golpesRecibidos = g;
    public float GetVidaActual()           => vidaActual;
    public float GetVidaMaxima()           => vidaMax;
    public float GetVidaNormalizada()      => Mathf.Clamp01(vidaActual / vidaMax);

    // ─── UI ───────────────────────────────────────────────────────────────────

    private void ActualizarBarra()
    {
        if (imagenBarraVida != null)
            imagenBarraVida.fillAmount = Mathf.Clamp01(vidaActual / vidaMax);
    }

    private void ActualizarBarraStamina()
    {
        if (imagenBarraStamina != null)
            imagenBarraStamina.fillAmount = Mathf.Clamp01(staminaActual / staminaMax);
    }

    private void MostrarUIBarras()
    {
        uiBarrasNormales?.SetActive(true);
        OcultarUIFinisher();
    }

    public void  OcultarUIBarras()    => uiBarrasNormales?.SetActive(false);
    private void MostrarUIFinisher()  { uiFinisher?.SetActive(true); OcultarUIBarras(); }
    private void OcultarUIFinisher()  => uiFinisher?.SetActive(false);
}
