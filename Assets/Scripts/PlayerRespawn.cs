using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPosition;

    private void Start()
    {
        // Tìm GameObject có tag Respawn
        GameObject respawnObject = GameObject.FindGameObjectWithTag("Respawn");
        if (respawnObject != null)
        {
            respawnPosition = respawnObject.transform.position;
        }
        else
        {
            Debug.LogWarning("No object with tag 'Respawn' found. Using player's current position.");
            respawnPosition = transform.position; // fallback nếu không có Respawn object
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("killzone"))
        {
            Respawn();
        }
    }

    void Respawn()
    {
        transform.position = respawnPosition;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    // Optional: dùng hàm này để cập nhật vị trí spawn (ví dụ checkpoint)
    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
}
