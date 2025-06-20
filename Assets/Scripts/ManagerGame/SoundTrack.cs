using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundTrack : MonoBehaviour
{
    public static SoundTrack Instance { get; private set; }
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            audioSource.Play(); // Bắt đầu phát nhạc ngay
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LevelBoss" || scene.name == "Ending" || scene.name == "Menu")
        {
            audioSource.Stop(); 
        }
        else
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play(); // Phát tiếp → nếu nhạc chưa phát → phát lại từ đầu
            }
        }
    }
}
