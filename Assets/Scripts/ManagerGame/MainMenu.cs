using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject continueButton;
    private void Start()
    {
        if (!CheckpointData.HasSavedGame())
        {
            continueButton.SetActive(false);
        }
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void Options()
    {
        EnableMenu(settingMenu);
        DisableMenu(mainMenu);
    }

    public void Play()
    {
        
        CheckpointData.ClearAllData();
        PlayerPrefs.DeleteAll(); // Xóa tất cả dữ liệu đã lưu
        // Load scene mặc định để bắt đầu game mới
        SceneManager.LoadScene("Level");
    }

    public void ContinueGame()
    {
        if (CheckpointData.HasSavedGame())
        {
            string sceneToLoad = PlayerPrefs.GetString("LastScene", "Level");
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                CheckpointData.SetCurrentScene(sceneToLoad);
                CheckpointData.LoadGameState();
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("Tên scene đã lưu không hợp lệ. Load scene mặc định.");
                CheckpointData.SetCurrentScene("Level");
                SceneManager.LoadScene("Level");
            }
        }
        else
        {
            Debug.LogWarning("Không có dữ liệu game đã lưu. Load scene mặc định.");
            CheckpointData.SetCurrentScene("Level");
            SceneManager.LoadScene("Level");
        }
    }
    public void Update()
    {

    }

    public void DisableMenu(GameObject menuName)
    {
        CanvasGroup canvasGroup = menuName.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Làm Canvas trong suốt
        canvasGroup.blocksRaycasts = false; // Ngăn chặn raycast (click, hover)
        canvasGroup.interactable = false;   // Ngăn chặn tương tác
    }
    public void EnableMenu(GameObject menuName)
    {
        CanvasGroup canvasGroup = menuName.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; // Làm Canvas hiển thị
        canvasGroup.blocksRaycasts = true; // Cho phép raycast (click, hover)
        canvasGroup.interactable = true;   // Cho phép tương tác
    }
}
