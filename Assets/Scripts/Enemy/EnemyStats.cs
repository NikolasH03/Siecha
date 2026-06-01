using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Combat/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Identidad")]
    public string nombreEnemigo = "Enemigo Base";

    [Header("Vida")]
    [SerializeField] private float vidaMax = 20f;
    public float VidaMax => vidaMax;

    [Header("Stamina y Bloqueo")]
    [SerializeField] private float staminaMax                = 10f;
    [SerializeField] private float regeneracionStamina       = 1f;
    [SerializeField] private float delayRegeneracionStamina  = 5f;
    [SerializeField] private float duracionGuardBreak        = 2f;
    [SerializeField] private int   golpesAntesDeBloquear     = 2;

    public float StaminaMax                => staminaMax;
    public float RegeneracionStamina       => regeneracionStamina;
    public float DelayRegeneracionStamina  => delayRegeneracionStamina;
    public float DuracionGuardBreak        => duracionGuardBreak;
    public int   GolpesAntesDeBloquear     => golpesAntesDeBloquear;

    [Header("Stun")]
    [SerializeField] private float duracionStun = 2f;
    public float DuracionStun => duracionStun;

    [Header("Dano de Ataques")]
    [SerializeField] private int danoAtaqueLigero = 5;
    [SerializeField] private int danoAtaqueFuerte = 15;
    public int DanoAtaqueLigero => danoAtaqueLigero;
    public int DanoAtaqueFuerte => danoAtaqueFuerte;

    [Header("Combos")]
    [SerializeField] private int   maxAtaquesLigerosEnCombo  = 3;
    [SerializeField] private int   maxAtaquesFuertesEnCombo  = 2;
    public int MaxAtaquesLigerosEnCombo => maxAtaquesLigerosEnCombo;
    public int MaxAtaquesFuertesEnCombo => maxAtaquesFuertesEnCombo;

    [Header("Timing de Combate")]
    [SerializeField] private float tiempoEntreAtaquesCombo  = 0.5f;
    [SerializeField] private float cooldownDespuesDeCombo   = 1.5f;
    [SerializeField] private float duracionDanoRecibido     = 1.1f;
    public float TiempoEntreAtaquesCombo => tiempoEntreAtaquesCombo;
    public float CooldownDespuesDeCombo  => cooldownDespuesDeCombo;
    public float DuracionDanoRecibido    => duracionDanoRecibido;

    [Header("Movimiento")]
    [SerializeField] private float velocidadEnEstadoSeguir = 4f;
    [SerializeField] private float distanciaEsquivar       = 3f;
    [SerializeField] private float velocidadEsquivar       = 10f;
    public float VelocidadEnEstadoSeguir => velocidadEnEstadoSeguir;
    public float DistanciaEsquivar       => distanciaEsquivar;
    public float VelocidadEsquivar       => velocidadEsquivar;

    [Header("Patrulla")]
    [SerializeField] private float tiempoDeEspera  = 1.5f;
    [SerializeField] private float radioDePatrulla = 15f;
    public float TiempoDeEspera  => tiempoDeEspera;
    public float RadioDePatrulla => radioDePatrulla;

    [Header("Deteccion")]
    [SerializeField] private float anguloDeDeteccion           = 120f;
    [SerializeField] private float radioDeDeteccion            = 15f;
    [SerializeField] private float radioDeDeteccionAutomatica  = 5f;
    [SerializeField] private float tiempoPorDeteccion          = 1f;
    public float AnguloDeDeteccion           => anguloDeDeteccion;
    public float RadioDeDeteccion            => radioDeDeteccion;
    public float RadioDeDeteccionAutomatica  => radioDeDeteccionAutomatica;
    public float TiempoPorDeteccion          => tiempoPorDeteccion;

    [Header("Rango de Ataque")]
    [SerializeField] private float rangoDeAtaque = 3f;
    public float RangoDeAtaque => rangoDeAtaque;

    // ─── Capacidades por fase ─────────────────────────────────────────────────
    // Estas flags permiten configurar qué puede hacer cada variante/fase
    // sin escribir código nuevo. La UtilityAI las consulta antes de decidir.

    [Header("Capacidades de Combate")]
    [Tooltip("Fase 2 del jefe tiene escudo → true. Fase 1 (cuchillos) → false.")]
    [SerializeField] private bool puedeBloquear = true;
    [Tooltip("Fase 1 del jefe (ágil) → true. Fase 2 (lento con escudo) → false.")]
    [SerializeField] private bool puedeEsquivar = true;
    [Tooltip("Multiplica el cooldown entre ataques. Fase berserker = 0.5 (ataca el doble de rápido).")]
    [SerializeField] [Range(0.1f, 2f)] private float multiplicadorAgresividad = 1f;

    public bool  PuedeBloquear              => puedeBloquear;
    public bool  PuedeEsquivar             => puedeEsquivar;
    public float MultiplicadorAgresividad  => multiplicadorAgresividad;
}