using UnityEngine;
using UnityEngine.UI;

public class MenuCreditos : MenuBaseConNavegacion
{
    [SerializeField] private Button botonVolver;

    protected override void ConfigurarNavegacion()
    {
        AudioManager.Instance.StopMusic();
        if (botonVolver)
            primerSeleccionable = botonVolver;
    }

    public void VolverAtras()
    {
        MenuManager.Instance.GoBack();
        // AudioManager.Instance.PlayMusic(AudioManager.Instance.mus_menu);
    }
}