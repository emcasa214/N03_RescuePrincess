using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject mainMenu;
    public void Play()
    {
        SceneManager.LoadScene("Level");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Options()
    {
        settingMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
}
