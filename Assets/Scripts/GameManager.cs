using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private PlayerController playerController;

    private bool isGameOver = false;
    private Vector3 playerPosition;

    //Level Complete
    [SerializeField] GameObject levelCompletePanel;
    [SerializeField] TMP_Text leveCompletePanelTitle;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        UIManager.instance.fadeFromBlack = true;
        playerPosition = playerController.transform.position;
    }

    public void Death()
    {
        if (!isGameOver)
        {
            // Disable Mobile Controls
            UIManager.instance.DisableMobileControls();
            // Initiate screen fade
            UIManager.instance.fadeToBlack = true;

            // Disable the player object
            playerController.gameObject.SetActive(false);

            // Start death coroutine to wait and then respawn the player
            StartCoroutine(DeathCoroutine());

            // Update game state
            isGameOver = true;

            // Log death message
            Debug.Log("Died");
        }
    }

    public void LevelComplete()
    {
        levelCompletePanel.SetActive(true);
        leveCompletePanelTitle.text = "LEVEL COMPLETE";
        // Gọi LevelManager để chuyển level
        LevelManager.instance.LoadNextLevel();
    }

    public void IncrementCoinCount()
    {
        // TODO: Implement coin collection logic
        Debug.Log("Coin collected!");
    }

    public void IncrementGemCount()
    {
        // TODO: Implement gem collection logic
        Debug.Log("Gem collected!");
    }
   
    public IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(1f);
        playerController.transform.position = playerPosition;

        // Wait for 2 seconds
        yield return new WaitForSeconds(1f);

        // Check if the game is still over (in case player respawns earlier)
        if (isGameOver)
        {
            LevelManager.instance.RestartLevel();
        }
    }
}
