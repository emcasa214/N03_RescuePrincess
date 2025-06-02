using UnityEngine;

public class IceFalls : MonoBehaviour
{
    [Header("Falling Settings")]
    public float distance = 6.6f;
    private bool hasFallen = false;
    private bool before;
    private Rigidbody2D rb;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float checkRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Landing Effect")]
    [SerializeField] private GameObject landingEffectPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    void Update()
    {
        // Debug.Log(rb.velocity.y);
        // 1. Rơi xuống khi chạm player
        if (!hasFallen) return;

        // 2. Kiểm tra đã chạm đất (layer Ground)
        bool isGrounded = before && rb.velocity.y == 0;

        if (isGrounded)
        {
            // Phát hiệu ứng
            if (landingEffectPrefab != null)
            {
                GameObject iceActive = Instantiate(landingEffectPrefab, groundCheckPoint.position, Quaternion.identity);
                Destroy(iceActive, 0.667f);
            }

            // Xóa object
            Destroy(gameObject);
        }

        before = rb.velocity.y < 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasFallen && other.CompareTag("Player"))
        {
            rb.gravityScale = 1;
            hasFallen = true;
        }
    }


    
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        }
    }
}
