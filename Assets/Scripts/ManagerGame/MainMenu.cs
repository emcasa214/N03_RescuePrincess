using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject continueButton;

    public void Quit()
    {
        Application.Quit();
    }

    public void Options()
    {
        settingMenu.SetActive(true);
        CanvasGroup canvasGroup = mainMenu.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Làm Canvas trong suốt
        canvasGroup.blocksRaycasts = false; // (Tùy chọn) Ngăn chặn raycast (click, hover)
        canvasGroup.interactable = false;   // (Tùy chọn) Ngăn chặn tương tác
    }
    private void Start()
    {
        if (!CheckpointData.HasSavedGame())
        {
            continueButton.SetActive(false);
        }
    }

    public void Play()
    {
        // Xóa tất cả dữ liệu trong PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Reset trạng thái trong CheckpointData
        CheckpointData.ClearAllData();

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
}
