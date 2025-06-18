using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
                BindingMenu.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                BindingMenu.SetActive(false);
            }
        }
    }
    public void Options()
    {
        BindingMenu.SetActive(false);
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }
    }
    public void onOk()
    {
        Time.timeScale = 1;
        BindingMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

}
