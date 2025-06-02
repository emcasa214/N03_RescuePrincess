using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPosition;
    private static bool shouldPlayRespawnAnim = false; // Biến tĩnh để theo dõi trạng thái respawn

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 6f; // Độ mạnh của cú nảy

    [Header("Animation")]
    [SerializeField] private GameObject deadAnim;
    [SerializeField] private GameObject respawnAnim;
    [SerializeField] private float delayEff = 51;
    [SerializeField] private GameObject transitionEffect;
    private Rigidbody2D rb;
    private bool isRespawning = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject respawnObject = GameObject.FindGameObjectWithTag("Respawn");
        if (respawnObject != null)
        {
            respawnPosition = respawnObject.transform.position;
        }
        else
        {
            Debug.LogWarning("No object with tag 'Respawn' found. Using player's current position.");
            respawnPosition = transform.position;
        }

        // Nếu cần chạy respawnAnim (sau khi tải scene)
        if (shouldPlayRespawnAnim)
        {
            StartCoroutine(PlayRespawnOnStart());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isRespawning && collision.CompareTag("killzone"))
        {
            StartCoroutine(RespawnWithEffects());
        }
    }

    private IEnumerator RespawnWithEffects()
    {
        isRespawning = true;

        // 1. Tính knockback ngược hướng
        Vector2 currentVelocity = rb.velocity;
        Vector2 knockback = Vector2.up * knockbackMultiplier;
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            knockback = -currentVelocity.normalized * knockbackMultiplier;
        }

        // 2. Nảy lên
        rb.velocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        // 3. Chờ ngắn để thấy nảy
        yield return new WaitForSeconds(0.1f);

        // 4. Instantiate deadAnim tại vị trí hiện tại
        GameObject dead = Instantiate(deadAnim, transform.position, transform.rotation);
        RippleEffect.Instance.Emit(Camera.main.WorldToViewportPoint(transform.position));
        GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse(rb.velocity.normalized * 0.25f);
        dead.transform.localScale = transform.localScale;
        Destroy(dead, 1.17f);

        // 5. Tắt hiển thị nhân vật
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Color c = sr.color;
        if (sr != null)
        {
            c.a = 0f; // Alpha = 0 → trong suốt
            sr.color = c;
        }
        rb.simulated = false; // Tắt physics

        Vector3 camPos = Camera.main.transform.position;
        camPos.z += 1f;

        yield return new WaitForSecondsRealtime(70 / 60f); // Chờ hết deadAnim
        GameObject trans = Instantiate(transitionEffect, camPos, Quaternion.identity);
        DontDestroyOnLoad(trans);
        Destroy(trans, 90 / 60f);
        yield return new WaitForSecondsRealtime(39 / 60f);
        shouldPlayRespawnAnim = true; // Đánh dấu để chạy respawnAnim trong scene mới
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return new WaitForSecondsRealtime(51/60f); 
    }

    private IEnumerator PlayRespawnOnStart()
    {

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
        rb.simulated = false;
        yield return new WaitForSecondsRealtime(delayEff/60f); 
        // Phát respawnAnim tại vị trí spawn
        GameObject respawn = Instantiate(respawnAnim, respawnPosition, Quaternion.identity);
        respawn.transform.localScale = transform.localScale;
        Destroy(respawn, 0.83f);

        // Đặt vị trí player
        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(0.83f); // Chờ hết respawnAnim

        // Khôi phục trạng thái
        rb.simulated = true;
        c.a = 1f;
        sr.color = c;
        shouldPlayRespawnAnim = false;
        isRespawning = false;
    }
}