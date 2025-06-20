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
        CanvasGroup canvasGroup = mainMenu.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; 
        canvasGroup.blocksRaycasts = true; 
        canvasGroup.interactable = true; 
    }
    public void onOk()
    {
        Time.timeScale = 1;
        BindingMenu.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        BindingMenu.SetActive(false);
    }

    public void BackMenu()
    {
        Time.timeScale = 1;
        // Debug.Log($"Saving before returning to MainMenu from {currentScene}");
        // CheckpointData.SaveProgress(currentScene);
        SceneManager.LoadScene("Menu");
    }
    // Update is called once per frame
    void Update()
    {
    }

}
