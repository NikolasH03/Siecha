using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menu : MonoBehaviour
{
    public void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Empezar(string Nivel)
    {
        SceneManager.LoadScene(Nivel);
    }
    public void Salir()
    {
        Application.Quit();
        
    }
}
