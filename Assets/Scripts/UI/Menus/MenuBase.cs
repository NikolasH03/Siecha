using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public abstract class MenuBase : MonoBehaviour
{
    [SerializeField] protected GameObject menuContainer;
    [SerializeField] protected bool pauseGame = false;

    protected bool isOpen = false;

    public virtual void OpenMenu()
    {
        if (isOpen) return;

        isOpen = true;
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
        menuContainer.SetActive(false);

        if (pauseGame)
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        OnMenuClosed();
    }

    public bool IsOpen => isOpen;

    protected virtual void OnMenuOpened() { }
    protected virtual void OnMenuClosed() { }
}

