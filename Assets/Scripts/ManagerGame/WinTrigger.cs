using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public string menuSceneName = "Menu";
    public string videoFileName = "WinVid.mp4";

    private void Start()
    {
        // Lấy hoặc thêm component VideoPlayer vào GameObject
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component không được tìm thấy trên GameObject này!");
            return;
        }

        // Thiết lập đường dẫn tới video trong thư mục StreamingAssets
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.loopPointReached += OnVideoFinished;

        // Đảm bảo video không phát tự động khi khởi tạo
        videoPlayer.playOnAwake = false;
        Time.timeScale = 0.7f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh rò rỉ bộ nhớ
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}