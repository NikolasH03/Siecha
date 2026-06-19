using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSequence : MonoBehaviour
{
    [Header("Tutoriales")]
    [SerializeField] private List<int> indicesPanelesTutorial = new();

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreTutoriales = 5f;

    private void Start()
    {
        StartCoroutine(MostrarTutoriales());
    }

    private IEnumerator MostrarTutoriales()
    {
        if (indicesPanelesTutorial.Count == 0)
            yield break;

        // Primer tutorial
        MenuManager.Instance?.AbrirPanelTutorial(indicesPanelesTutorial[0]);
        Debug.Log($"[TutorialSequence] Mostrando tutorial {indicesPanelesTutorial[0]}");

        // Caso especial: mostrar el segundo tutorial después
        if (indicesPanelesTutorial.Count > 1)
        {
            yield return new WaitForSeconds(tiempoEntreTutoriales);

            MenuManager.Instance?.AbrirPanelTutorial(indicesPanelesTutorial[1]);
            Debug.Log($"[TutorialSequence] Mostrando tutorial {indicesPanelesTutorial[1]}");
        }

        // Continuar con el resto
        for (int i = 2; i < indicesPanelesTutorial.Count; i++)
        {
            yield return new WaitForSeconds(tiempoEntreTutoriales);

            MenuManager.Instance?.AbrirPanelTutorial(indicesPanelesTutorial[i]);
            Debug.Log($"[TutorialSequence] Mostrando tutorial {indicesPanelesTutorial[i]}");
        }
    }
}
