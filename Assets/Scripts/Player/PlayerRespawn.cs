using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public static class CheckpointData
{
    private static Dictionary<string, List<Vector3>> checkpoints = new Dictionary<string, List<Vector3>>();
    private static Dictionary<string, List<string>> collectedItems = new Dictionary<string, List<string>>();
    private static Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    private static int totalStrawberries = 0;
    private static string currentScene = "";
    private static HashSet<string> savedScenes = new HashSet<string>();

    public static void SetCurrentScene(string sceneName)
    {
        currentScene = sceneName;
    }

    public static string GetCurrentScene()
    {
        return currentScene;
    }

    public static void SaveCheckpoint(Vector3 position)
    {
        if (string.IsNullOrEmpty(currentScene))
            return;

        if (!checkpoints.ContainsKey(currentScene))
        {
            checkpoints[currentScene] = new List<Vector3>();
        }
        checkpoints[currentScene].Add(position);
        savedScenes.Add(currentScene); // Theo dõi scene đã được lưu
    }

    public static bool HasCheckpoint()
    {
        return checkpoints.ContainsKey(currentScene) && checkpoints[currentScene].Count > 0;
    }

    public static Vector3 GetLastCheckpoint()
    {
        if (HasCheckpoint())
        {
            return checkpoints[currentScene][checkpoints[currentScene].Count - 1];
        }
        return Vector3.zero;
    }

    public static void ClearCheckpoint()
    {
        if (checkpoints.ContainsKey(currentScene))
            checkpoints.Remove(currentScene);
        if (collectedItems.ContainsKey(currentScene))
            collectedItems.Remove(currentScene);
        if (itemCounts.ContainsKey(currentScene))
            itemCounts.Remove(currentScene);
        savedScenes.Remove(currentScene);
    }

    public static void SaveItem(string itemName)
    {
        if (string.IsNullOrEmpty(currentScene))
            return;

        if (!collectedItems.ContainsKey(currentScene))
            collectedItems[currentScene] = new List<string>();
        if (!collectedItems[currentScene].Contains(itemName))
        {
            collectedItems[currentScene].Add(itemName);
            itemCounts[currentScene] = collectedItems[currentScene].Count;
            totalStrawberries++;
        }
    }

    public static List<string> GetCollectedItems()
    {
        return collectedItems.ContainsKey(currentScene) ? collectedItems[currentScene] : new List<string>();
    }

    public static int GetItemCount()
    {
        return itemCounts.ContainsKey(currentScene) ? itemCounts[currentScene] : 0;
    }

    public static int GetTotalStrawberries()
    {
        return totalStrawberries;
    }

    public static void SaveGameState()
    {
        if (string.IsNullOrEmpty(currentScene))
            return;

        PlayerPrefs.SetString("LastScene", currentScene); // Lưu scene cuối cùng
        PlayerPrefs.SetString("SavedScenes", string.Join(",", savedScenes)); // Lưu danh sách các scene đã lưu

        // Lưu trạng thái của scene hiện tại
        if (HasCheckpoint())
        {
            Vector3 lastCheckpoint = GetLastCheckpoint();
            PlayerPrefs.SetFloat(currentScene + "_CheckpointX", lastCheckpoint.x);
            PlayerPrefs.SetFloat(currentScene + "_CheckpointY", lastCheckpoint.y);
            PlayerPrefs.SetFloat(currentScene + "_CheckpointZ", lastCheckpoint.z);
        }
        string items = string.Join(",", GetCollectedItems());
        PlayerPrefs.SetString(currentScene + "_CollectedItems", items);
        PlayerPrefs.SetInt(currentScene + "_ItemCount", GetItemCount());

        // Lưu tổng số dâu tây
        PlayerPrefs.SetInt("TotalStrawberries", totalStrawberries);
        PlayerPrefs.Save();
    }

    public static void LoadGameState()
    {
        if (string.IsNullOrEmpty(currentScene))
            return;

        // Tải danh sách các scene đã lưu
        string savedScenesStr = PlayerPrefs.GetString("SavedScenes", "");
        if (!string.IsNullOrEmpty(savedScenesStr))
        {
            savedScenes = new HashSet<string>(savedScenesStr.Split(','));
        }

        // Tải trạng thái cho scene hiện tại
        if (PlayerPrefs.HasKey(currentScene + "_CheckpointX"))
        {
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat(currentScene + "_CheckpointX"),
                PlayerPrefs.GetFloat(currentScene + "_CheckpointY"),
                PlayerPrefs.GetFloat(currentScene + "_CheckpointZ")
            );
            SaveCheckpoint(position);
        }
        string items = PlayerPrefs.GetString(currentScene + "_CollectedItems", "");
        if (!string.IsNullOrEmpty(items))
        {
            collectedItems[currentScene] = new List<string>(items.Split(','));
            itemCounts[currentScene] = collectedItems[currentScene].Count;
        }
        totalStrawberries = PlayerPrefs.GetInt("TotalStrawberries", 0);
    }

    public static bool HasSavedGame()
    {
        return PlayerPrefs.HasKey("LastScene");
    }

    public static void ClearAllData()
    {
        
        checkpoints.Clear();
        collectedItems.Clear();
        itemCounts.Clear();
        totalStrawberries = 0;
        currentScene = "";
        savedScenes.Clear();
        
        PlayerPrefs.Save();
    }
}

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPosition;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 6f;

    [Header("Animation")]
    [SerializeField] private GameObject deadAnim;
    [SerializeField] private GameObject respawnAnim;
    [SerializeField] private GameObject transitionEffect;
    [SerializeField] private GameObject retrans;

    [Header("UI")]
    private TextMeshProUGUI itemCountText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button continueButton;
    private Rigidbody2D rb;
    private bool isRespawning = false;
    private string currentScene;
    private PlayerSound playerSound;
    private List<string> tempCollectedItems = new List<string>();
    private int tempItemCount = 0;
    private int displayedStrawberryCount = 0;
    private GameObject respawnObject;
    private bool restartOk = false;
    

    private void Awake()
    {
        playerSound = GetComponent<PlayerSound>();
    }

    private void Start()
    {
        respawnObject = GameObject.FindGameObjectWithTag("Respawn");
        rb = GetComponent<Rigidbody2D>();
        itemCountText = GameObject.Find("Score").GetComponent<TextMeshProUGUI>();
        currentScene = SceneManager.GetActiveScene().name;
        CheckpointData.SetCurrentScene(currentScene);

        // Tải trạng thái cho scene hiện tại, bất kể có phải scene cuối cùng hay không
        CheckpointData.LoadGameState();

        if (CheckpointData.HasCheckpoint() && !restartOk)
        {
            respawnPosition = CheckpointData.GetLastCheckpoint();
            tempItemCount = CheckpointData.GetItemCount();
            tempCollectedItems = new List<string>(CheckpointData.GetCollectedItems());
        }
        else
        {
            respawnPosition = respawnObject != null ? respawnObject.transform.position : transform.position;
            tempItemCount = 0;
            tempCollectedItems.Clear();
            restartOk = false;
        }

        displayedStrawberryCount = CheckpointData.GetTotalStrawberries();
        UpdateItemCountUI();
        StartCoroutine(PlayRespawnOnStart());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isRespawning && collision.CompareTag("killzone"))
        {
            StartCoroutine(RespawnWithEffects());
        }

        if (collision.CompareTag("Checkpoint"))
        {
            CheckpointData.SaveCheckpoint(collision.transform.position);
            foreach (string item in tempCollectedItems)
            {
                CheckpointData.SaveItem(item);
            }
            StartCoroutine(AnimateStrawberryCount(displayedStrawberryCount, CheckpointData.GetTotalStrawberries()));
            SaveGame(); // Lưu trạng thái ngay khi đạt checkpoint
            // playerSound.PlayCheckPoint();
        }

        if (collision.CompareTag("Strawberry"))
        {
            string itemName = collision.gameObject.name;
            if (!tempCollectedItems.Contains(itemName))
            {
                tempCollectedItems.Add(itemName);
                tempItemCount++;

                // ẩn dâu tây đã nhặt
                // collision.gameObject.SetActive(false);


                StartCoroutine(AnimateStrawberryCount(displayedStrawberryCount, displayedStrawberryCount + 1));
                displayedStrawberryCount++;
            }
            playerSound.PlayCollect(); 
        }
    }

    private void UpdateItemCountUI()
    {
        if (itemCountText != null)
        {
            itemCountText.text = $"{displayedStrawberryCount}";
        }
    }

    private IEnumerator AnimateStrawberryCount(int startCount, int targetCount)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            displayedStrawberryCount = Mathf.RoundToInt(Mathf.Lerp(startCount, targetCount, t));
            UpdateItemCountUI();
            yield return null;
        }
        displayedStrawberryCount = targetCount;
        UpdateItemCountUI();
    }

    private IEnumerator RespawnWithEffects()
    {
        isRespawning = true;
        Time.timeScale = 0.4f;
        Vector2 currentVelocity = rb.velocity;
        Vector2 knockback = Vector2.up * knockbackMultiplier;
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            knockback = -currentVelocity.normalized * knockbackMultiplier;
        }
        playerSound.PlayPreDeath();
        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 1f;
        GameObject dead = Instantiate(deadAnim, transform.position, transform.rotation);
        RippleEffect.Instance.Emit(Camera.main.WorldToViewportPoint(transform.position));
        GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse(rb.velocity.normalized * 0.25f);
        dead.transform.localScale = transform.localScale;
        Destroy(dead, 1.17f);

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        
        if (sr != null)
        {
            sr.color = new Color(0f,0f,0f, 0f);
        }
        rb.simulated = false;

        Vector3 camPos = Camera.main.transform.position;
        camPos.z += 1f;

        yield return new WaitForSecondsRealtime(70 / 60f);
        GameObject trans = Instantiate(transitionEffect, camPos, Quaternion.identity);
        yield return new WaitForSecondsRealtime(90 / 60f);

        tempCollectedItems = new List<string>(CheckpointData.GetCollectedItems());
        tempItemCount = CheckpointData.GetItemCount();
        displayedStrawberryCount = CheckpointData.GetTotalStrawberries();
        UpdateItemCountUI();

        foreach (string itemName in tempCollectedItems)
        {
            GameObject item = GameObject.Find(itemName);
            if (item != null)
            {
                item.SetActive(false);
            }
        }

        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    private IEnumerator PlayRespawnOnStart()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        sr.color = new Color(0f,0f,0f, 0f);
        rb.simulated = false;
        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;

        foreach (string itemName in tempCollectedItems)
        {
            GameObject item = GameObject.Find(itemName);
            if (item != null)
            {
                item.SetActive(false);
            }
        }

        Vector3 camPos = new Vector3(respawnPosition.x, Camera.main.transform.position.y, respawnPosition.z + 1f);
        GameObject trans1 = Instantiate(retrans, camPos, Quaternion.identity);
        yield return new WaitForSecondsRealtime(70 / 60f);
        Destroy(trans1);

        GameObject respawn = Instantiate(respawnAnim, respawnPosition, Quaternion.identity);
        playerSound.PlayRespawn();
        respawn.transform.localScale = transform.localScale;
        Destroy(respawn, 0.83f);

        yield return new WaitForSecondsRealtime(0.83f);

        rb.simulated = true;
        sr.color = new Color(1f,1f,1f,1f);
        isRespawning = false;
    }
    
    public void Restart()
    {
        CheckpointData.ClearCheckpoint();
        tempCollectedItems.Clear();
        tempItemCount = 0;
        displayedStrawberryCount = CheckpointData.GetTotalStrawberries();
        UpdateItemCountUI();
        restartOk = true;
        SceneManager.LoadScene(currentScene);
    }

    public void SaveGame()
    {
        CheckpointData.SaveGameState();
    }

    
}