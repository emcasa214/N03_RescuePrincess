using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 1f;
    private float destroyDelay = 0.5f;

    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float shakeDuration = 0.2f; // Thời gian rung
    [SerializeField] private float shakeMagnitude = 0.1f; // Độ lớn của rung

    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position; // Lưu vị trí ban đầu
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Shake()); // Bắt đầu hiệu ứng rung
            StartCoroutine(Fall());  // Bắt đầu hiệu ứng rơi
        }
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            float yOffset = Random.Range(-shakeMagnitude, shakeMagnitude);

            transform.position = new Vector3(
                originalPosition.x + xOffset,
                originalPosition.y + yOffset,
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Trả về vị trí ban đầu sau khi rung
        transform.position = originalPosition;
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, destroyDelay);
    }
}
