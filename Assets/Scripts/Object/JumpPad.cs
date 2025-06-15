using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpPadForce = 10f; // Lực nhảy của jump pad
    private Collider2D jumpPadCollider;
    private Animator anim;
    public PlayerDataWithDash Data;
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        jumpPadCollider = GetComponent<Collider2D>();
        if (jumpPadCollider == null || !jumpPadCollider.isTrigger)
        {
            Debug.LogWarning("Not Is Trigger!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerRb != null)
            {
                anim.SetTrigger("triggerPad");
                // Reset vận tốc Y trước khi áp dụng lực mới
                playerRb.velocity = new Vector2(0f, 0f);
                //playerMovement.SetGravityScale(Data.gravityScale);
                
                // Áp dụng lực nhảy mạnh khi chạm jump pad
                playerRb.AddForce(Vector2.up * jumpPadForce, ForceMode2D.Impulse);
                playerMovement.JumpPadState();
            }
        }
    }
}