using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.EventSystems;

public abstract class MenuBase : MonoBehaviour
{
    [SerializeField] protected GameObject menuContainer;
    public bool pauseGame = false;

    protected bool isOpen = false;

    public virtual void OpenMenu()
    {
        if (isOpen) return;

        isOpen = true;
        EventSystem.current.SetSelectedGameObject(null);
        menuContainer.SetActive(true);

        if (pauseGame)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        OnMenuOpened();
    }

    public virtual void CloseMenu()
    {
        if (!isOpen) return;

        isOpen = false;
        EventSystem.current.SetSelectedGameObject(null);
        menuContainer.SetActive(false);

        if (pauseGame)
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        OnMenuClosed();
    }
    public bool Pause => pauseGame;

    public bool IsOpen => isOpen;

    protected virtual void OnMenuOpened() { }
    protected virtual void OnMenuClosed() { }
}

