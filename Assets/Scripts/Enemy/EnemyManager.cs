using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Main AI Loop - Settings")]
    public float tiempoEntreDecisiones = 0.5f;

    private List<Enemigo> enemigos = new List<Enemigo>();
    private Coroutine aiLoopCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(IniciarDespuesDeUnFrame());
    }

    public void ActualizarJugador()
    {
        foreach (var enemigo in enemigos)
        {
            if (enemigo == null) continue;
            enemigo.BuscarJugador(); 
            DetectarJugador detector = enemigo.GetComponent<DetectarJugador>();
            if (detector != null) detector.BuscarJugador();
        }
    }

    private IEnumerator IniciarDespuesDeUnFrame()
    {
        yield return null; // esperar un frame

        enemigos.Clear();
        enemigos.AddRange(FindObjectsOfType<Enemigo>());

        Debug.Log($"[EnemyManager] Enemigos detectados: {enemigos.Count}");

        if (aiLoopCoroutine != null) StopCoroutine(aiLoopCoroutine);
        aiLoopCoroutine = StartCoroutine(AILoop());
    }

    private IEnumerator AILoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreDecisiones);

            Enemigo candidato = SeleccionarEnemigoDisponible();
            if (candidato == null) continue;

            candidato.OrdenarAtacar();

            // Esperar hasta que deje de atacar O muera O sea destruido
            yield return new WaitUntil(() =>
                candidato == null || !candidato.EstaAtacando() || candidato.EstaMuerto()
            );

            yield return new WaitForSeconds(Random.Range(0.15f, 0.6f));
        }
    }

    private Enemigo SeleccionarEnemigoDisponible()
    {
        enemigos.RemoveAll(e => e == null || e.EstaMuerto()); // limpiar muertos

        var disponibles = enemigos.FindAll(e => e.EstaDisponibleParaAtacar());
        if (disponibles.Count == 0) return null;

        return disponibles[Random.Range(0, disponibles.Count)];
    }
}
