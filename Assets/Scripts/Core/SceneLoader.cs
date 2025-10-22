using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Variables Barra de carga")]
    [SerializeField] private GameObject CanvasLoader;
    [SerializeField] private Image ProgressBar;

    private float _target;
    [Header("Nombres de escenas")]
    public List<string> NombresEscenas;

    public event Action<string> OnSceneLoadedSuccessfully; 

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void LoadScene(int id)
    {
        StartCoroutine(LoadSceneAsync(id));
    }

    private IEnumerator LoadSceneAsync(int id)
    {
        ProgressBar.fillAmount = 0;
        _target = 0;

        string sceneName = NombresEscenas[id - 1];
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        CanvasLoader.SetActive(true);

        while (scene.progress < 0.9f)
        {
            _target = scene.progress;
            yield return null;
        }

        // Mostrar barra completa un momento antes de activar
        _target = 1f;
        yield return new WaitForSeconds(0.5f);

        scene.allowSceneActivation = true;

        CanvasLoader.SetActive(false);

        OnSceneLoadedSuccessfully?.Invoke(sceneName);
    }

    private void Update()
    {
        if (ProgressBar != null)
            ProgressBar.fillAmount = Mathf.MoveTowards(ProgressBar.fillAmount, _target, 3 * Time.deltaTime);
    }
}
