using Cinemachine;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Windows;

public class ControladorCombate : MonoBehaviour
{
    public Animator anim;

    //ataque
    [SerializeField] bool atacando = false;
    public string tipoAtaque;
    //bloqueo y dash
    [SerializeField] bool bloqueando = false;

    //intanciar arma melee
    [SerializeField] private ArmaData armaActual;
    [SerializeField] private Transform puntoSujecionArmaPrincipal;
    [SerializeField] private Transform puntoSujecionArmaSecundaria;
    private GameObject armaInstanciada;
    private GameObject armaSecundariaInstanciada;
    public ArmaVFX vfxPrincipal;
    private ArmaVFX vfxSecundaria;
    public CinemachineVirtualCamera camaraFinisher;

    //Daño del arma a distancia
    [SerializeField] private ArmaDistanciaData armaDistancia;
    private bool tieneBufoDisparo = false;

    //variables para generar los combos en los estados
    [HideInInspector] public bool puedeHacerCombo = false;
    [HideInInspector] public TipoInputCombate inputBufferCombo = TipoInputCombate.Ninguno;
    public List<TipoInputCombate> secuenciaInputs = new List<TipoInputCombate>();
    public Dictionary<string, Combo> combos;

    //colliders necesarios para generar daño
    private Collider ColliderArma;
    private Collider ColliderArmaSecundaria;
    [SerializeField] Collider ColliderPierna;

    //layers para invulnerabilidad en el dash
    private int normalLayerIndex;
    private int InvulnerabilidadLayerIndex;

    //variables y referencias relacionadas con las barras de vida, estamina y numero de muertes
    public EstadisticasCombateSO statsBase;
    [HideInInspector] public EstadisticasCombate stats;

    private Coroutine regeneracionCoroutine;
    private float delayRegeneracion = 5f;
    private float tasaRegeneracion = 3f;

    public int muertesActuales = 0;
    public int muertesMaximas = 5;

    //referencias a otros codigos
    [SerializeField] private EventosAnimacion eventosAnimacion;
    [SerializeField] ControladorCambioArmas cambioArma;
    private ControladorMovimiento controladorMovimiento;
    private CombatStateMachine fsm;
    private AutoTargeting targeting;
    //[SerializeField] HabilidadesJugador habilidadesJugador;

    void Awake()
    {
        stats = new EstadisticasCombate(statsBase);
        EquiparArma(armaActual);
    }
    private void Start()
    {
        
        ColliderArma = armaInstanciada.GetComponent<Collider>();
        ColliderArmaSecundaria = armaSecundariaInstanciada.GetComponent<Collider>();

        ColliderArma.enabled = false;
        ColliderArmaSecundaria.enabled = false;
        ColliderPierna.enabled = false;

        normalLayerIndex = LayerMask.NameToLayer("Default");
        InvulnerabilidadLayerIndex = LayerMask.NameToLayer("JugadorInvulnerable");

        anim = GetComponent<Animator>();
        controladorMovimiento = GetComponent<ControladorMovimiento>();
        targeting = GetComponent<AutoTargeting>();

        fsm = new CombatStateMachine();
        fsm.ChangeState(new VerificarTipoArmaState(fsm, this));
        combos = ComboDatabase.Combos;

        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        AudioManager.Instance.PlayMusic(AudioManager.Instance.mus_exploracion);
        AudioManager.Instance.PlayAmbience(AudioManager.Instance.amb_naturaleza);
    }
    public void Update()
    {
        fsm.Update();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "hostile")
        {
            JugadorRecibeDano();
        }
    }

    public void EmpezarRegeneracionEstamina()
    {
        if (regeneracionCoroutine != null) StopCoroutine(regeneracionCoroutine);
        regeneracionCoroutine = StartCoroutine(RegenerarEstamina());
    }

    private IEnumerator RegenerarEstamina()
    {
        yield return new WaitForSeconds(delayRegeneracion);

        while (stats.EstaminaActual < stats.EstaminaMax)
        {
            stats.RegenerarEstamina(tasaRegeneracion * Time.deltaTime);
            yield return null;
        }

        regeneracionCoroutine = null;
    }

    public void JugadorRecibeDano()
    {
        if (bloqueando)
        {
            fsm.ChangeState(new DanoBloqueandoState(fsm, this));
        }
        else
        {
            fsm.ChangeState(new DanoState(fsm, this));
        }
    }

    public int VerificarArmaEquipada()
    {
        return cambioArma.getterArma();
    }

    public void EquiparArma(ArmaData nuevaArma)
    {
        if (nuevaArma == null) return;


        armaInstanciada = Instantiate(nuevaArma.prefabArmaPrincipal, puntoSujecionArmaPrincipal);
        armaInstanciada.transform.localPosition = Vector3.zero;
        armaInstanciada.transform.localRotation = Quaternion.identity;
        armaInstanciada.transform.localScale = Vector3.one;

        armaSecundariaInstanciada = Instantiate(nuevaArma.prefabArmaSecundaria, puntoSujecionArmaSecundaria);
        armaSecundariaInstanciada.transform.localPosition = Vector3.zero;
        armaSecundariaInstanciada.transform.localRotation = Quaternion.identity;
        armaSecundariaInstanciada.transform.localScale = Vector3.one;

        vfxPrincipal = armaInstanciada.GetComponent<ArmaVFX>();
        vfxSecundaria = armaSecundariaInstanciada.GetComponent<ArmaVFX>();

        vfxPrincipal?.DesactivarTrail();
        vfxSecundaria?.DesactivarTrail();

        armaActual = nuevaArma;
    }

    public int EntregarDañoArmaMelee(bool enemigoBloqueando)
    {
        if (!enemigoBloqueando)
        {
            if (tipoAtaque == "ligero")
            {
                CameraShakeManager.instance.ShakeGolpeLigero();
                return armaActual.dañoGolpeLigero;
            }
            else if (tipoAtaque == "fuerte")
            {
                CameraShakeManager.instance.ShakeGolpeFuerte();
                return armaActual.dañoGolpeFuerte;
            }
            else if (tipoAtaque == "cargado")
            {
                CameraShakeManager.instance.ShakeGolpeFuerte();
                return armaActual.dañoGolpeCargado;
            }
            else
            {
                CameraShakeManager.instance.ShakeGolpeLigero();
                return armaActual.dañoGolpeLigero;
            }
        }
        else
        {
            if (tipoAtaque == "ligero")
            {
                CameraShakeManager.instance.ShakeGolpeLigero();
                return armaActual.dañoGolpeLigeroGuardia;
            }
            else if (tipoAtaque == "fuerte")
            {
                CameraShakeManager.instance.ShakeGolpeFuerte();
                return armaActual.dañoGolpeFuerteGuardia;
            }
            else if (tipoAtaque == "cargado")
            {
                CameraShakeManager.instance.ShakeGolpeFuerte();
                return armaActual.dañoGolpeCargado;
            }
            else
            {
                CameraShakeManager.instance.ShakeGolpeLigero();
                return armaActual.dañoGolpeLigeroGuardia;
            }
        }
    }
    public HealthbarEnemigo DetectarEnemigoStunned(float rango)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, rango, LayerMask.GetMask("Enemigo"));

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<HealthbarEnemigo>();
            if (enemy != null && enemy.enemigoStunned)
            {
                return enemy;
            }
        }

        return null;
    }

    public int EntregarDañoArmaDistancia()
    {
        if (tieneBufoDisparo)
        {
            tieneBufoDisparo = false;
            return Mathf.RoundToInt(armaDistancia.dañoDisparo * 1.5f);
        }
        return armaDistancia.dañoDisparo;
    }
    public void ActivarBufoDisparo()
    {
        tieneBufoDisparo = true;
    }

    public void LimpiarSecuenciaInputs()
    {
        secuenciaInputs.Clear();
    }
    public void Revivir()
    {
        stats.CurarVida(stats.VidaMax);
        stats.RegenerarEstamina(stats.EstaminaMax);
        stats.RecibirDano(0);
        stats.UsarEstamina(0);

        CambiarCanMove(true);

        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;

        fsm.ChangeState(new VerificarTipoArmaState(fsm, this));
    }
    public void OrientarJugador()
    {
        targeting.BuscarYOrientar();
    }
    public void ReproducirVFX(int indexVFX, int indexPivot = 0)
    {
        eventosAnimacion.ReproducirVFX(indexVFX, indexPivot);
    }

    public void ReproducirSonido(int indexSonido, int indexPivot = 0)
    {
        eventosAnimacion.ReproducirSonidoImpacto(indexSonido, indexPivot);
    }

    // funciones para los Animation Events
    public void VolverAIdle()
    {
        DesactivarVentanaCombo();
        fsm.ChangeState(new VerificarTipoArmaState(fsm, this));
    }
    public void TerminarAtaqueCargado()
    {
        DesactivarVentanaCombo();
        fsm.ChangeState(new CooldownCargadoState(fsm, this, 0.5f));
    }
    public void TerminarEstadoDano()
    {
        gameObject.layer = normalLayerIndex;
        fsm.ChangeState(new VerificarTipoArmaState(fsm, this));
    }
    public void TerminarEstadoDanoBloqueando()
    {
        gameObject.layer = normalLayerIndex;
        fsm.ChangeState(new BloqueoState(fsm, this));
    }
    public void TerminarEstadoDisparo()
    {
        fsm.ChangeState(new RecargarState(fsm, this));
    }
    public void TerminarEstadoRecarga()
    {
        fsm.ChangeState(new ApuntarState(fsm, this));
    }
    public void EntrarCooldownDisparo()
    {
        fsm.ChangeState(new CooldownDisparoCargado(fsm, this, 0.5f));
    }
    public void ActivarVentanaCombo()
    {
        puedeHacerCombo = true;
    }
    public void DesactivarVentanaCombo()
    {
        puedeHacerCombo = false;
        inputBufferCombo = TipoInputCombate.Ninguno;
        LimpiarSecuenciaInputs();
    }
    public void InvulneravilidadJugador()
    {
        gameObject.layer = InvulnerabilidadLayerIndex;
    }
    public void TerminarDash()
    {
        gameObject.layer = normalLayerIndex;
        fsm.ChangeState(new VerificarTipoArmaState(fsm, this));
    }
    public void TerminarInvulnerabilidad()
    {
        gameObject.layer = normalLayerIndex;
    }


    public void activarColliderArma()
    {
        ColliderArma.enabled = true;
    }
    public void desactivarColliderArma()
    {
        ColliderArma.enabled = false;
    }
    public void activarColliderArmaSecundaria()
    {
        ColliderArmaSecundaria.enabled = true;
    }
    public void desactivarColliderArmaSecundaria()
    {
        ColliderArmaSecundaria.enabled = false;
    }
    public void activarColliderPierna()
    {
        ColliderPierna.enabled = true;
    }
    public void desactivarColliderPierna()
    {
        ColliderPierna.enabled = false;
    }

    public void DesactivarTodosLosCollider()
    {
        ColliderArma.enabled = false;
        ColliderArmaSecundaria.enabled = false;
        ColliderPierna.enabled = false;
    }
    public void AnimationEvent_ReproducirPieIzq(int indexVFX)
    {
        eventosAnimacion.ReproducirVFX(indexVFX, 3);
        eventosAnimacion.ReproducirSonidoImpacto(indexVFX, 3);
    }
    public void AnimationEvent_ReproducirPieDer(int indexVFX)
    {
        eventosAnimacion.ReproducirVFX(indexVFX, 4);
        eventosAnimacion.ReproducirSonidoImpacto(indexVFX, 4);
    }

    public void AnimationEvent_ReproducirBloqueo(int indexVFX)
    {
        eventosAnimacion.ReproducirVFX(indexVFX, 0);
        eventosAnimacion.ReproducirSonidoImpacto(indexVFX, 0);
    }
    public void ActivarTrailArmaPrincipal()
    {
        vfxPrincipal?.ActivarTrail();
    }
    public void DesactivarTrailArmaPrincipal()
    {
        vfxPrincipal?.DesactivarTrail();
    }

    public void ActivarTrailArmaSecundaria()
    {
        vfxSecundaria?.ActivarTrail();
    }
    public void DesactivarTrailArmaSecundaria()
    {
        vfxSecundaria?.DesactivarTrail();
    }

    public void DesactivarTodosLosTrails()
    {
        vfxPrincipal?.DesactivarTrail();
        vfxSecundaria?.DesactivarTrail();
    }


    //prueba para el arbol de habilidades
    //public bool PuedeUsarCapoeira()
    //{
    //    return HabilidadesJugador.instance.estaDesbloqueada(HabilidadesJugador.TipoHabilidad.Capoeira);
    //}


    //setters y getters

    public bool getAtacando()
    {
        return atacando;
    }
    public void setAtacando(bool ataque)
    {
        atacando = ataque;
    }
    public bool getDashing()
    {
        return anim.GetBool("dashing");
    }
    public bool getBloqueando()
    {
        return bloqueando;
    }
    public void setBloqueando(bool block)
    {
        bloqueando = block;
    }
    public List<GameObject> getArmaActual()
    {
        List<GameObject> armas = new List<GameObject>();
        armas.Add(armaInstanciada);
        armas.Add(armaSecundariaInstanciada);

        return armas;
    }
    public int getLayerDodge()
    {
        return InvulnerabilidadLayerIndex;
    }

    public void CambiarCanMove(bool puedeMov)
    {
        controladorMovimiento.setCanMove(puedeMov);
    }
    public void CambiarArmaMelee()
    {
        cambioArma.CambiarArmaMelee();
    }
    public void CambiarArmaDistancia()
    {
        cambioArma.CambiarArmaDistancia();
    }

    public void AumentarVidaTotem(float cantidad)
    {
        stats.AumentarVidaMax(cantidad);
    }

    public void AumentarEstaminaTotem(float cantidad)
    {
        stats.AumentarEstaminaMax(cantidad);
    }

}
