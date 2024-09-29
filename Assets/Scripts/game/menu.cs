using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menu : MonoBehaviour
{

    public void Empezar(string Nivel)
    {
        SceneManager.LoadScene(Nivel);
    }
    public void Salir()
    {
        Application.Quit();
        Debug.Log("Chao mundo");
    }
}
