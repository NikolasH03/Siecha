using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthComp : MonoBehaviour
{
    // Valores inyectados desde EnemyStats en Inicializar(); no se configuran en el inspector.
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
    private int   golpesRecibidos;
    private bool  enGuardBreak;
    private bool  estaBloqueado;
    private bool  estaMuerto;
    private bool  terminaAnimacionMuerte;
    private bool  estaSiendoDanado;
    private bool  estaEsquivando;
    private bool  danoPendiente;
    private bool  estaEnFinisher;
    private bool  estaStuneado;
    private bool  inicializado;

    private Temporizador timerGuardBreak;
    private Coroutine    regeneracionCoroutine;

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

    public void Inicializar(EnemyStats stats)
    {
        vidaMax                  = stats.VidaMax;
        staminaMax               = stats.StaminaMax;
        regeneracionStamina      = stats.RegeneracionStamina;
        delayRegeneracionStamina = stats.DelayRegeneracionStamina;
        duracionGuardBreak       = stats.DuracionGuardBreak;
        duracionStun             = stats.DuracionStun;
        golpesAntesDeBloquear    = stats.GolpesAntesDeBloquear;

        vidaActual    = vidaMax;
        staminaActual = staminaMax;

        timerGuardBreak = new Temporizador(duracionGuardBreak);
        timerGuardBreak.OnTimerStop += RecuperarGuardBreak;

        ActualizarBarra();
        ActualizarBarraStamina();
        MostrarUIBarras();

        inicializado = true;
    }

    /// <summary>Actualiza parámetros de combate sin tocar la vida actual (cambio de fase del jefe).</summary>
    public void ActualizarParametrosDeCombate(EnemyStats stats)
    {
        staminaMax               = stats.StaminaMax;
        regeneracionStamina      = stats.RegeneracionStamina;
        delayRegeneracionStamina = stats.DelayRegeneracionStamina;
        golpesAntesDeBloquear    = stats.GolpesAntesDeBloquear;
    }

    // ─── Daño y vida ──────────────────────────────────────────────────────────

    public void recibeDano(int cantidad)
    {
        if (!inicializado || estaMuerto || estaEsquivando || estaEnFinisher) return;

        if (estaBloqueado)
        {
            ConsumirStaminaPorBloqueo(cantidad);
            return;
        }

        if (estaStuneado) SalirDeStun();

        golpesRecibidos++;
        vidaActual = Mathf.Max(vidaActual - cantidad, 0f);
        ActualizarBarra();

        danoPendiente    = true;
        estaSiendoDanado = true;

        if (vidaActual <= 0f)
        {
            estaMuerto = true;
            DetenerRegeneracion();
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    /// <summary>Consume el aviso de daño: devuelve true una sola vez por golpe recibido.</summary>
    public bool EnemigoFueDanado()
    {
        if (!danoPendiente || estaMuerto || estaBloqueado) return false;

        danoPendiente = false;
        return true;
    }

    public void TerminarDanoRecibido() => estaSiendoDanado = false;

    /// <summary>
    /// Los scripts del jugador llaman setRecibiendoDano(true) después de recibeDano() incluso si el golpe
    /// fue esquivado o bloqueado. Sin daño pendiente se ignora, para no dejar el flag atascado en true
    /// (ningún estado lo limpiaría y el enemigo dejaría de evaluar su IA).
    /// </summary>
    public void setRecibiendoDano(bool valor)
    {
        if (valor && !danoPendiente) return;
        estaSiendoDanado = valor;
    }

    public void  SetGolpesRecibidos(int g) => golpesRecibidos = g;
    public float GetVidaActual()           => vidaActual;
    public float GetVidaMaxima()           => vidaMax;
    public float GetVidaNormalizada()      => Mathf.Clamp01(vidaActual / vidaMax);

    // Tótems
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

    // ─── Muerte ───────────────────────────────────────────────────────────────

    public void TerminarAnimacionMuerte() => terminaAnimacionMuerte = true;

    /// <summary>
    /// Desregistra al enemigo del EnemyManager antes de desactivar el objeto
    /// para que sus listas no queden con referencias muertas.
    /// </summary>
    public void Eliminar()
    {
        var enemigo = GetComponent<Enemigo>();
        if (enemigo != null && EnemyManager.instance != null)
            EnemyManager.instance.DesregistrarEnemigo(enemigo);

        OnEnemyMuerto?.Invoke();
        gameObject.SetActive(false);
    }

    // ─── Bloqueo y esquive ────────────────────────────────────────────────────

    public void setBloqueado(bool valor)
    {
        estaBloqueado = valor;
        setRecibiendoDano(false);
    }

    public bool getBloqueando() => estaBloqueado;
    public bool DebeBloquear()  => golpesRecibidos >= golpesAntesDeBloquear;

    public void setEsquivando(bool valor) => estaEsquivando = valor;

    // ─── Stamina y guard break ────────────────────────────────────────────────

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

    public void TickTimers(float deltaTime) => timerGuardBreak?.Tick(deltaTime);

    private void RecuperarGuardBreak()
    {
        if (!enGuardBreak) return; 
        
        enGuardBreak  = false;
        staminaActual = staminaMax;
        ActualizarBarraStamina();
    }

    public void RegenerarStamina(float cantidad)
    {
        if (enGuardBreak || staminaActual >= staminaMax) return;

        staminaActual = Mathf.Clamp(staminaActual + cantidad, 0f, staminaMax);
        ActualizarBarraStamina();
    }

    public void EmpezarRegeneracionEstamina()
    {
        if (regeneracionCoroutine != null) StopCoroutine(regeneracionCoroutine);
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
        if (regeneracionCoroutine == null) return;

        StopCoroutine(regeneracionCoroutine);
        regeneracionCoroutine = null;
    }

    public void RestablecerEstamina()
    {
        staminaActual = staminaMax;
        ActualizarBarraStamina();
    }

    // ─── Stun y finisher ──────────────────────────────────────────────────────

    public void EntrarEnStun()
    {
        estaStuneado = true;
        MostrarUIFinisher();
    }

    public void SalirDeStun()
    {
        if (estaEnFinisher) return;

        estaStuneado = false;
        TerminarGuardBreak();   
        OcultarUIFinisher();
        MostrarUIBarras();
    }

    public bool SePuedeHacerFinisher() => estaStuneado && !estaEnFinisher && !estaMuerto;

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
    private void TerminarGuardBreak()
    {
        if (!enGuardBreak) return;

        enGuardBreak  = false;
        staminaActual = staminaMax;
        ActualizarBarraStamina();
    }

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
        SetActivo(uiBarrasNormales, true);
        OcultarUIFinisher();
    }

    public void OcultarUIBarras() => SetActivo(uiBarrasNormales, false);

    private void MostrarUIFinisher()
    {
        SetActivo(uiFinisher, true);
        OcultarUIBarras();
    }

    private void OcultarUIFinisher() => SetActivo(uiFinisher, false);

    // Se usa "!= null" y no "?." porque en objetos de Unity "?." ignora el chequeo de "destruido".
    private static void SetActivo(GameObject objeto, bool activo)
    {
        if (objeto != null) objeto.SetActive(activo);
    }
}