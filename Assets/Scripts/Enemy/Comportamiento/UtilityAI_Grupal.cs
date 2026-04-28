using UnityEngine;

public enum AccionGrupal { Atacar, Flanquear, DefenderArquero, Rodear, Retirarse }

public class UtilityAI_Grupal
{
    private Enemigo enemigo;
    private EnemyManager manager;
    private Transform jugador;
    private HealthComp salud;

    public UtilityAI_Grupal(Enemigo enemigo, EnemyManager manager)
    {
        this.enemigo = enemigo;
        this.manager = manager;
        this.salud = enemigo.GetComponent<HealthComp>();
        this.jugador = enemigo.JugadorActual;
    }

    public AccionGrupal DecidirAccion()
    {
        float uAtacar = CalcularUtilidadAtacar();
        // float uFlanquear = CalcularUtilidadFlanquear();
        float uRodear = CalcularUtilidadRodear();
        float uRetirada = CalcularUtilidadRetirarse();

        float max = Mathf.Max(uAtacar, uRodear, uRetirada);

        if (max == uAtacar) return AccionGrupal.Atacar;
        // if (max == uFlanquear) return AccionGrupal.Flanquear;
        if (max == uRodear) return AccionGrupal.Rodear;
        return AccionGrupal.Retirarse;
    }
    public float CalcularUtilidadAtacar()
    {
        // NO retornar 0 si no tiene permiso. El Manager necesita saber 
        // quién QUIERE atacar para darle el permiso.
        float utilidad = 0.5f;

        float distancia = Vector3.Distance(enemigo.transform.position, jugador.position);
    
        // Inversamente proporcional a la distancia: más cerca = más utilidad
        utilidad += (1.0f - Mathf.Clamp01(distancia / 15f)) * 0.5f;

        if (!enemigo.EstaDisponibleParaAtacar())
            utilidad -= 0.4f;
        
        if (distancia <= enemigo.rangoDeAtaque) utilidad += 0.2f;

        return Mathf.Clamp01(utilidad);
    }

    // private float CalcularUtilidadFlanquear()
    // {
    //     float utilidad = 0.3f;
    //
    //     if (!JugadorMirandoEnemigo())
    //         utilidad += 0.3f;
    //
    //     Vector3 dirJugador = jugador.forward;
    //     Vector3 dirEnemigo = (enemigo.transform.position - jugador.position).normalized;
    //     float dot = Vector3.Dot(dirJugador, dirEnemigo);
    //
    //     if (dot > 0.5f) utilidad += 0.2f;
    //
    //     return Mathf.Clamp01(utilidad);
    // }

    private float CalcularUtilidadRodear()
    {
        float utilidad = 1.5f;

        // Si no tiene permiso de ataque, rodear
        if (!enemigo.EstaDisponibleParaAtacar())
            utilidad += 0.3f;

        float distancia = Vector3.Distance(enemigo.transform.position, jugador.position);

        // Distancia ideal para rodear (5-8m)
        if (distancia >= 5f && distancia <= 8f)
            utilidad += 0.2f;

        return Mathf.Clamp01(utilidad);
    }

    private float CalcularUtilidadRetirarse()
    {
        float utilidad = 0.1f;
        
        if (!enemigo.EstaDisponibleParaAtacar())
            utilidad += 0.1f; 

        // Solo retirarse de verdad si la vida es CRÍTICA (menos del 15%)
        if (salud.GetVidaNormalizada() < 0.15f)
            utilidad += 0.5f;

        return Mathf.Clamp01(utilidad);
    }

    private bool JugadorMirandoEnemigo()
    {
        Transform player = enemigo.JugadorActual;
        if (player == null) return false;

        Vector3 dirJugador = player.forward;
        Vector3 dirHaciaEnemigo = (enemigo.transform.position - player.position).normalized;
        float dot = Vector3.Dot(dirJugador, dirHaciaEnemigo);

        return dot > 0.7f;
    }
}
