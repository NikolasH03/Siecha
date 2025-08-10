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
        // Esperar un frame para dar tiempo a que todos los enemigos terminen su Start
        StartCoroutine(IniciarDespuesDeUnFrame());
    }

    private IEnumerator IniciarDespuesDeUnFrame()
    {
        yield return null; // espera 1 frame

        // Buscar todos los enemigos en la escena
        enemigos.Clear();
        enemigos.AddRange(FindObjectsOfType<Enemigo>());

        Debug.Log($"[EnemyManager] Enemigos detectados: {enemigos.Count}");

        // Arrancar la IA
        if (aiLoopCoroutine != null) StopCoroutine(aiLoopCoroutine);
        aiLoopCoroutine = StartCoroutine(AILoop());
    }

    private IEnumerator AILoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreDecisiones);

            // Seleccionar un enemigo disponible
            Enemigo candidato = SeleccionarEnemigoDisponible();
            if (candidato == null) continue;

            // Ordenar ataque
            candidato.OrdenarAtacar();

            // Esperar a que termine el ataque
            yield return new WaitUntil(() => !candidato.EstaAtacando());

            // Pequeña pausa antes de la siguiente decisión
            yield return new WaitForSeconds(Random.Range(0.15f, 0.6f));
        }
    }

    private Enemigo SeleccionarEnemigoDisponible()
    {
        var disponibles = enemigos.FindAll(e => e != null && e.EstaDisponibleParaAtacar());
        if (disponibles.Count == 0) return null;

        return disponibles[Random.Range(0, disponibles.Count)];
    }
}
