using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TipoRumble
{
    GolpeLigero,
    GolpeFuerte,
    GolpeCargado,
    DisparoArco,
    DisparoArcabuz,
    ImpactoFlecha,
    RecibirDano,
    BloqueoImpacto,
    GuardiaRota
}

/// <summary>
/// Vibración de gamepad por acción de combate.
/// Solo vibra cuando el esquema de control activo es el gamepad (InputJugador.UsaGamepad).
/// Uso desde cualquier script:  RumbleManager.Play(TipoRumble.GolpeLigero);
/// </summary>
public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;

    [System.Serializable]
    public struct Preset
    {
        public TipoRumble tipo;
        [Tooltip("Motor de baja frecuencia (grave, pesado).")]
        [Range(0f, 1f)] public float graves;
        [Tooltip("Motor de alta frecuencia (agudo, seco).")]
        [Range(0f, 1f)] public float agudos;
        [Tooltip("Duración en segundos.")]
        public float duracion;
    }

    [Header("Ajustes globales (conectar al menú de opciones)")]
    public bool vibracionActivada = true;
    [Range(0f, 1f)] public float intensidadGlobal = 1f;

    [Header("Envolvente")]
    [Tooltip("Cómo decae la intensidad durante la duración del pulso (X: 0-1 tiempo, Y: 0-1 intensidad).")]
    [SerializeField] private AnimationCurve decaimiento = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Presets (editables en el Inspector)")]
    [SerializeField] private List<Preset> presets = new List<Preset>();

    private struct Pulso
    {
        public float inicio;
        public float duracion;
        public float graves;
        public float agudos;
    }

    private readonly List<Pulso> pulsos = new List<Pulso>();
    private readonly Dictionary<TipoRumble, Preset> tabla = new Dictionary<TipoRumble, Preset>();

    private Gamepad gamepadActivo;
    private bool motoresActivos;

    // ---------------------------------------------------------------- Ciclo de vida

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ConstruirTabla();
    }

    // Se llama al agregar el componente o con "Reset" en el Inspector.
    private void Reset()
    {
        presets.Clear();
        foreach (TipoRumble t in System.Enum.GetValues(typeof(TipoRumble)))
            presets.Add(PresetPorDefecto(t));
    }

    private void OnDisable()
    {
        pulsos.Clear();
        DetenerMotores();
    }

    private void OnApplicationFocus(bool tieneFoco)
    {
        // Evita que el control se quede vibrando si el juego pierde el foco.
        if (!tieneFoco)
        {
            pulsos.Clear();
            DetenerMotores();
        }
    }

    private void Update()
    {
        if (pulsos.Count == 0)
        {
            if (motoresActivos) DetenerMotores();
            return;
        }

        // Si el jugador cambió a teclado/mouse en medio de una vibración, se corta.
        if (!UsandoGamepad())
        {
            pulsos.Clear();
            DetenerMotores();
            return;
        }

        float graves = 0f;
        float agudos = 0f;
        float ahora = Time.unscaledTime; // sigue funcionando aunque el juego esté en pausa / hit-stop

        for (int i = pulsos.Count - 1; i >= 0; i--)
        {
            Pulso p = pulsos[i];
            float t = (ahora - p.inicio) / p.duracion;

            if (t >= 1f)
            {
                pulsos.RemoveAt(i);
                continue;
            }

            float factor = decaimiento.Evaluate(t);

            // Se toma el máximo (no la suma) para que varios eventos juntos no saturen el motor.
            graves = Mathf.Max(graves, p.graves * factor);
            agudos = Mathf.Max(agudos, p.agudos * factor);
        }

        Gamepad pad = ObtenerGamepad();
        if (pad == null)
        {
            pulsos.Clear();
            return;
        }

        gamepadActivo = pad;
        pad.SetMotorSpeeds(graves * intensidadGlobal, agudos * intensidadGlobal);
        motoresActivos = true;

        if (pulsos.Count == 0) DetenerMotores();
    }

    // ---------------------------------------------------------------- API pública

    /// <summary>Forma segura de llamarlo desde cualquier script (no falla si no hay RumbleManager en escena).</summary>
    public static void Play(TipoRumble tipo)
    {
        if (instance != null) instance.Reproducir(tipo);
    }

    public void Reproducir(TipoRumble tipo)
    {
        if (!vibracionActivada || !UsandoGamepad()) return;
        if (!tabla.TryGetValue(tipo, out Preset p)) return;

        pulsos.Add(new Pulso
        {
            inicio = Time.unscaledTime,
            duracion = Mathf.Max(p.duracion, 0.01f),
            graves = p.graves,
            agudos = p.agudos
        });
    }

    /// <summary>Corta toda vibración (útil al pausar, morir o cambiar de escena).</summary>
    public void DetenerTodo()
    {
        pulsos.Clear();
        DetenerMotores();
    }

    // ---------------------------------------------------------------- Internos

    private bool UsandoGamepad()
    {
        return InputJugador.instance != null && InputJugador.instance.UsaGamepad;
    }

    private Gamepad ObtenerGamepad()
    {
        // Prioriza el gamepad asignado al PlayerInput; si no hay, el último usado.
        PlayerInput pi = InputJugador.instance != null ? InputJugador.instance.GetInputJugador() : null;
        if (pi != null)
        {
            foreach (InputDevice d in pi.devices)
            {
                if (d is Gamepad g) return g;
            }
        }
        return Gamepad.current;
    }

    private void DetenerMotores()
    {
        if (gamepadActivo != null)
            gamepadActivo.ResetHaptics();

        motoresActivos = false;
    }

    private void ConstruirTabla()
    {
        tabla.Clear();

        foreach (Preset p in presets)
            tabla[p.tipo] = p;

        // Si se agregó un tipo nuevo al enum y no está en el Inspector, se completa con el valor por defecto.
        foreach (TipoRumble t in System.Enum.GetValues(typeof(TipoRumble)))
        {
            if (!tabla.ContainsKey(t))
            {
                Preset d = PresetPorDefecto(t);
                tabla[t] = d;
                presets.Add(d);
            }
        }
    }

    // ---------------------------------------------------------------- Valores por defecto
    // graves = motor pesado (izquierdo), agudos = motor ligero (derecho)

    private static Preset PresetPorDefecto(TipoRumble tipo)
    {
        switch (tipo)
        {
            // Melee: de un "tick" seco a un golpe contundente
            case TipoRumble.GolpeLigero:     return Crear(tipo, 0.20f, 0.40f, 0.10f);
            case TipoRumble.GolpeFuerte:     return Crear(tipo, 0.55f, 0.45f, 0.18f);
            case TipoRumble.GolpeCargado:    return Crear(tipo, 0.85f, 0.65f, 0.30f);

            // Distancia
            case TipoRumble.DisparoArco:     return Crear(tipo, 0.00f, 0.25f, 0.08f); // suelta de cuerda, sutil
            case TipoRumble.DisparoArcabuz:  return Crear(tipo, 0.90f, 0.70f, 0.30f); // explosión
            case TipoRumble.ImpactoFlecha:   return Crear(tipo, 0.10f, 0.30f, 0.08f); // confirmación ligera

            // Defensa
            case TipoRumble.RecibirDano:     return Crear(tipo, 0.80f, 0.55f, 0.35f); // el más fuerte del juego, castiga
            case TipoRumble.BloqueoImpacto:  return Crear(tipo, 0.45f, 0.20f, 0.12f); // pesado y corto, "choque"
            case TipoRumble.GuardiaRota:     return Crear(tipo, 0.70f, 0.50f, 0.30f);

            default:                         return Crear(tipo, 0.30f, 0.30f, 0.10f);
        }
    }

    private static Preset Crear(TipoRumble tipo, float graves, float agudos, float duracion)
    {
        return new Preset { tipo = tipo, graves = graves, agudos = agudos, duracion = duracion };
    }
}
