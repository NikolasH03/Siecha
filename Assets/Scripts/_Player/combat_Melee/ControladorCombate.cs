using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

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
    [SerializeField] private Transform puntoSujecion;
    private GameObject armaInstanciada;

    //Daño del arma a distancia
    [SerializeField] private ArmaDistanciaData armaDistancia;
    private bool tieneBufoDisparo = false;

    //variables para generar los combos en los estados
    [HideInInspector] public bool puedeHacerCombo = false;
    [HideInInspector] public TipoInputCombate inputBufferCombo = TipoInputCombate.Ninguno;
    public List<TipoInputCombate> secuenciaInputs = new List<TipoInputCombate>();
    public Dictionary<string, Combo> combos;

    //colliders necesarios para generar daño
    [SerializeField] Collider ColliderArma;
    [SerializeField] Collider ColliderPierna;

    //layers para invulnerabilidad en el dash
    private int normalLayerIndex;
    private int esquivarLayerIndex;

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

        ColliderArma.enabled = false;
        ColliderPierna.enabled = false;

        normalLayerIndex = LayerMask.NameToLayer("Default");
        esquivarLayerIndex = LayerMask.NameToLayer("Esquivar");

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


        armaInstanciada = Instantiate(nuevaArma.prefab, puntoSujecion);
        armaInstanciada.transform.localPosition = Vector3.zero;
        armaInstanciada.transform.localRotation = Quaternion.identity;
        armaInstanciada.transform.localScale = Vector3.one;

        armaActual = nuevaArma;
    }

    public int EntregarDanoArmaMelee()
    {
        if (tipoAtaque == "ligero")
        {
            CameraShakeManager.instance.ShakeGolpeLigero();
            return armaActual.danoGolpeLigero;
        }
        else if (tipoAtaque == "fuerte")
        {
            CameraShakeManager.instance.ShakeGolpeFuerte();
            return armaActual.danoGolpeFuerte;
        }
        else
        {
            CameraShakeManager.instance.ShakeGolpeLigero();
            return armaActual.danoGolpeLigero;
        }

    }

    public int EntregarDanoArmaDistancia()
    {
        if (tieneBufoDisparo)
        {
            tieneBufoDisparo = false;
            return Mathf.RoundToInt(armaDistancia.danoDisparo * 1.5f);
        }
        return armaDistancia.danoDisparo;
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

        CambiarMovimientoCanMove(true);

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
        puedeHacerCombo = false;
        fsm.ChangeState(new VerificarTipoArmaState(fsm, this));
        inputBufferCombo = TipoInputCombate.Ninguno;
        LimpiarSecuenciaInputs();
    }
    public void TerminarEstadoDano()
    {
        controladorMovimiento.GetComponent<Collider>().enabled = true;
        controladorMovimiento.GetComponent<Rigidbody>().isKinematic = false;
        controladorMovimiento.setCanMove(true);
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
        gameObject.layer = esquivarLayerIndex;
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
    public void activarColliderPierna()
    {
        ColliderPierna.enabled = true;
    }
    public void desactivarColliderPierna()
    {
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
    public GameObject getArmaActual()
    {
        return armaInstanciada;
    }
    public int getLayerDodge()
    {
        return esquivarLayerIndex;
    }

    public void CambiarMovimientoCanMove(bool puedeMov)
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
