using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public static class CheckpointData
{
    private static Dictionary<string, List<Vector3>> checkpoints = new Dictionary<string, List<Vector3>>();
    private static Dictionary<string, List<string>> collectedItems = new Dictionary<string, List<string>>();
    private static Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    private static int totalStrawberries = 0;

    public static void SaveCheckpoint(string sceneName, Vector3 position)
    {
        if (!checkpoints.ContainsKey(sceneName))
        {
            checkpoints[sceneName] = new List<Vector3>();
        }
        checkpoints[sceneName].Add(position); // Luôn thêm vị trí checkpoint mới nhất
    }

    public static bool HasCheckpoint(string sceneName)
    {
        return checkpoints.ContainsKey(sceneName) && checkpoints[sceneName].Count > 0;
    }

    public static Vector3 GetLastCheckpoint(string sceneName)
    {
        if (HasCheckpoint(sceneName))
        {
            return checkpoints[sceneName][checkpoints[sceneName].Count - 1];
        }
        return Vector3.zero;
    }

    public static void ClearCheckpoint(string sceneName)
    {
        if (checkpoints.ContainsKey(sceneName))
            checkpoints.Remove(sceneName);
        if (collectedItems.ContainsKey(sceneName))
            collectedItems.Remove(sceneName);
        if (itemCounts.ContainsKey(sceneName))
            itemCounts.Remove(sceneName);
    }

    public static void SaveItem(string sceneName, string itemName)
    {
        if (!collectedItems.ContainsKey(sceneName))
            collectedItems[sceneName] = new List<string>();
        if (!collectedItems[sceneName].Contains(itemName))
        {
            collectedItems[sceneName].Add(itemName);
            itemCounts[sceneName] = collectedItems[sceneName].Count;
            totalStrawberries++;
        }
    }

    public static List<string> GetCollectedItems(string sceneName)
    {
        return collectedItems.ContainsKey(sceneName) ? collectedItems[sceneName] : new List<string>();
    }

    public static int GetItemCount(string sceneName)
    {
        return itemCounts.ContainsKey(sceneName) ? itemCounts[sceneName] : 0;
    }

    public static int GetTotalStrawberries()
    {
        return totalStrawberries;
    }

    public static void ResetTotalStrawberries()
    {
        totalStrawberries = 0;
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
    [SerializeField] private TextMeshProUGUI itemCountText;

    private Rigidbody2D rb;
    private bool isRespawning = false;
    private string currentScene;
    private PlayerSound playerSound;
    private List<string> tempCollectedItems = new List<string>();
    private int tempItemCount = 0;
    private int displayedStrawberryCount = 0;

    private void Awake()
    {
        playerSound = GetComponent<PlayerSound>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentScene = SceneManager.GetActiveScene().name;

        if (CheckpointData.HasCheckpoint(currentScene))
        {
            respawnPosition = CheckpointData.GetLastCheckpoint(currentScene);
            tempItemCount = CheckpointData.GetItemCount(currentScene);
            tempCollectedItems = new List<string>(CheckpointData.GetCollectedItems(currentScene));
        }
        else
        {
            GameObject respawnObject = GameObject.FindGameObjectWithTag("Respawn");
            respawnPosition = respawnObject != null ? respawnObject.transform.position : transform.position;
            tempItemCount = 0;
            tempCollectedItems.Clear();
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
            CheckpointData.SaveCheckpoint(currentScene, collision.transform.position);
            foreach (string item in tempCollectedItems)
            {
                CheckpointData.SaveItem(currentScene, item);
            }
            StartCoroutine(AnimateStrawberryCount(displayedStrawberryCount, CheckpointData.GetTotalStrawberries()));
        }

        if (collision.CompareTag("Strawberry"))
        {
            string itemName = collision.gameObject.name;
            if (!tempCollectedItems.Contains(itemName))
            {
                tempCollectedItems.Add(itemName);
                tempItemCount++;
                collision.gameObject.SetActive(false);
                StartCoroutine(AnimateStrawberryCount(displayedStrawberryCount, displayedStrawberryCount + 1));
                displayedStrawberryCount++;
            }
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

        Vector2 currentVelocity = rb.velocity;
        Vector2 knockback = Vector2.up * knockbackMultiplier;
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            knockback = -currentVelocity.normalized * knockbackMultiplier;
        }
        playerSound.PlayDeath();
        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.1f);

        GameObject dead = Instantiate(deadAnim, transform.position, transform.rotation);
        RippleEffect.Instance.Emit(Camera.main.WorldToViewportPoint(transform.position));
        GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse(rb.velocity.normalized * 0.25f);
        dead.transform.localScale = transform.localScale;
        Destroy(dead, 1.17f);

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Color c = sr.color;
        if (sr != null)
        {
            c.a = 0f;
            sr.color = c;
        }
        rb.simulated = false;

        Vector3 camPos = Camera.main.transform.position;
        camPos.z += 1f;

        yield return new WaitForSecondsRealtime(70 / 60f);
        GameObject trans = Instantiate(transitionEffect, camPos, Quaternion.identity);
        yield return new WaitForSecondsRealtime(90 / 60f);

        tempCollectedItems = new List<string>(CheckpointData.GetCollectedItems(currentScene));
        tempItemCount = CheckpointData.GetItemCount(currentScene);
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
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
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
        respawn.transform.localScale = transform.localScale;
        Destroy(respawn, 0.83f);

        yield return new WaitForSecondsRealtime(0.83f);

        rb.simulated = true;
        c.a = 1f;
        sr.color = c;
        isRespawning = false;
    }
}