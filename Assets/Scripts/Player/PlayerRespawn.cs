using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPosition;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 6f; // Độ mạnh của cú nảy

    [Header("Animation")]
    [SerializeField] private GameObject deadAnim;
    [SerializeField] private GameObject respawnAnim;
    [SerializeField] private GameObject transitionEffect;
    [SerializeField] private GameObject retrans;
    private Rigidbody2D rb;
    private bool isRespawning = false;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject respawnObject = GameObject.FindGameObjectWithTag("Respawn");
        respawnPosition = respawnObject.transform.position;
        
        // chạy respawnAnim (sau khi tải scene)
        StartCoroutine(PlayRespawnOnStart());
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
        yield return new WaitForSecondsRealtime(90 / 60f); // Chờ trans
        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name); //chờ load xong scence
    }

    private IEnumerator PlayRespawnOnStart()
    {
        // yield return null;
        
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Vector3 camPos = Camera.main.transform.position;
        camPos.z += 1f;
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
        rb.simulated = false;
        GameObject trans1 = Instantiate(retrans, camPos, Quaternion.identity);
        yield return new WaitForSecondsRealtime(70 / 60f);
        Destroy(trans1);
        // Phát respawnAnim tại vị trí spawn
        GameObject respawn = Instantiate(respawnAnim, respawnPosition, Quaternion.identity);
        respawn.transform.localScale = transform.localScale;
        Destroy(respawn, 0.83f);



        yield return new WaitForSecondsRealtime(0.83f); // Chờ hết respawnAnim
        // Đặt vị trí player
        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;

        // Khôi phục trạng thái
        rb.simulated = true;
        c.a = 1f;
        sr.color = c;
        isRespawning = false;
    }
}