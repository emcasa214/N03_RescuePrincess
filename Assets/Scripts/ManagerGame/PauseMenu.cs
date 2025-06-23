using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject BindingMenu;
    public GameObject mainMenu;
    private bool isPaused;


    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
    }

    public void OnTogglePause(InputAction.CallbackContext ctxt)
    {
        if (ctxt.started)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Time.timeScale = 0;
                MainMenu.Instance.EnableMenu(BindingMenu);
            }
            else
            {
                Time.timeScale = 1;
                MainMenu.Instance.DisableMenu(BindingMenu);
            }
        }
    }
    public void Options()
    {
        MainMenu.Instance.DisableMenu(BindingMenu);
        MainMenu.Instance.EnableMenu(mainMenu);
    }
    public void onOk()
    {
        Time.timeScale = 1;
        MainMenu.Instance.DisableMenu(BindingMenu);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        MainMenu.Instance.DisableMenu(BindingMenu);
    }

    public void BackMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
    // Update is called once per frame
    void Update()
    {
    }

}
