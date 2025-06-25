using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject continueButton;
    private float targetAspect = 16f / 9f; // Tỷ lệ 16:9 (thay đổi theo độ phân giải của bạn, ví dụ 1920/1080)

    private int lastWidth;
    private int lastHeight;

    void Update()
    {
        // Chỉ cập nhật khi kích thước thay đổi
        if (lastWidth != Screen.width || lastHeight != Screen.height)
        {
            UpdateResolution();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }

    void UpdateResolution()
    {
        // Lấy kích thước hiện tại của màn hình
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // Tính toán kích thước mới để duy trì tỷ lệ
        if (scaleHeight < 1.0f)
        {
            Rect rect = new Rect(0, 0, Screen.width, Screen.height * scaleHeight);
            Screen.SetResolution((int)rect.width, (int)rect.height, Screen.fullScreen);
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = new Rect(0, 0, Screen.width * scaleWidth, Screen.height);
            Screen.SetResolution((int)rect.width, (int)rect.height, Screen.fullScreen);
        }
    }
    private void Start()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        UpdateResolution();
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
