using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("Configuracion de Oleadas")]
    [Tooltip("Cada oleada debe ser un GameObject que contenga los enemigos como hijos.")]
    [SerializeField] private List<GameObject> oleadas = new List<GameObject>();

    [Header("Configuracion de Tutoriales")]
    [Tooltip("Paneles de tutorial asociados a las primeras oleadas.")]
    [SerializeField] private List<int> indicesPanelesTutorial = new List<int>();

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreOleadas = 2f;
    [SerializeField] private float tiempoEntreTutoriales = 3f; // usado solo en la primera oleada
    [SerializeField] private float tiempoAntesDeCargarMenu = 3f;

    public int indiceOleadaActual = 0;
    public bool oleadaActiva = false;

    private void Start()
    {
        DesactivarTodasLasOleadas();
        StartCoroutine(FlujoOleadas());
    }

    private void DesactivarTodasLasOleadas()
    {
        foreach (var oleada in oleadas)
        {
            if (oleada != null)
                oleada.SetActive(false);
        }
    }

    private IEnumerator FlujoOleadas()
    {
        yield return new WaitForSeconds(1f); // Peque�a espera inicial

        while (indiceOleadaActual < oleadas.Count)
        {
            // --- Mostrar panel de tutorial antes de activar la oleada ---
            if (indiceOleadaActual < indicesPanelesTutorial.Count)
            {
                int indexPanel = indicesPanelesTutorial[indiceOleadaActual];
                MenuManager.Instance?.AbrirPanelTutorial(indexPanel);
                Debug.Log($"[WaveManager] Mostrando tutorial {indexPanel}");

                // Caso especial: primera oleada muestra dos tutoriales seguidos
                if (indiceOleadaActual == 0 && indicesPanelesTutorial.Count > 1)
                {
                    yield return new WaitForSeconds(tiempoEntreTutoriales);
                    int segundoPanel = indicesPanelesTutorial[1];
                    MenuManager.Instance?.AbrirPanelTutorial(segundoPanel);
                    Debug.Log($"[WaveManager] Mostrando tutorial adicional {segundoPanel}");
                }

                // Esperar un poco antes de activar la oleada (para que el jugador lea)
                yield return new WaitForSeconds(tiempoEntreOleadas);
            }

            // --- Activar oleada actual ---
            GameObject oleada = oleadas[indiceOleadaActual];
            if (oleada != null)
            {
                oleada.SetActive(true);
                EnemyManager.instance.OnOleadaActivada(oleadas[indiceOleadaActual].transform);
            }

            oleadaActiva = true;
            Debug.Log($"[WaveManager] Oleada {indiceOleadaActual + 1} activada");

            // Esperar a que mueran todos los enemigos
            yield return StartCoroutine(EsperarOleadaCompletada());

            oleadaActiva = false;

            // --- Esperar un poco antes de pasar a la siguiente oleada ---
            yield return new WaitForSeconds(tiempoEntreOleadas);

            indiceOleadaActual++;
        }

        // --- Fin de todas las oleadas ---
        Debug.Log("[WaveManager] Todas las oleadas completadas. Volviendo al men� principal...");
        yield return new WaitForSeconds(tiempoAntesDeCargarMenu);
        SceneManager.LoadScene("Menu");
    }


    private IEnumerator EsperarOleadaCompletada()
    {
        // Asegurarse de que EnemyManager ya haya registrado a los enemigos de la oleada
        yield return new WaitForSeconds(0.5f);

        while (oleadaActiva)
        {
            if (EnemyManager.instance != null && EnemyManager.instance.AreAllEnemiesDead())
            {
                Debug.Log($"[WaveManager] Oleada {indiceOleadaActual + 1} completada.");
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

}
