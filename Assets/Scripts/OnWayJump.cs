using UnityEngine;

public class OnWayJump : MonoBehaviour
{
    private Transform playerGroundCheckPoint; // Ground check point của nhân vật
    private Collider2D platformCollider;

    private void Start()
    {
        platformCollider = gameObject.GetComponent<Collider2D>();

        // Lấy groundCheckPoint từ PlayerMovement Singleton
        if (PlayerMovement.Instance != null)
        {
            playerGroundCheckPoint = PlayerMovement.Instance._groundCheckPoint;
            if (playerGroundCheckPoint == null)
            {
                Debug.LogError("GroundCheckPoint không được gán trong PlayerMovement. Kiểm tra Inspector của Player.");
            }
        }
        else
        {
            Debug.LogError("PlayerMovement Instance không được khởi tạo. Đảm bảo PlayerMovement được gắn vào Player.");
        }
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        // Kiểm tra null để tránh lỗi
        if (playerGroundCheckPoint == null || platformCollider == null) return;

        float platformY = transform.position.y; // Vị trí Y của thanh ngang
        float groundCheckY = playerGroundCheckPoint.position.y; // Vị trí Y của groundCheck
        // Debug.Log(groundCheckY + "----" + platformY);

        // Nếu groundCheck thấp hơn thanh ngang (dưới platform), bật Is Trigger
        if (groundCheckY < platformY)
        {
            platformCollider.isTrigger = true;
        }
        // Nếu groundCheck cao hơn hoặc bằng thanh ngang (trên platform), tắt Is Trigger
        else if (groundCheckY >= platformY)
        {
            platformCollider.isTrigger = false;
        }
    }
}