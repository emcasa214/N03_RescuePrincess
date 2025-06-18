using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public float speed;
    public float closeDistance = 0.1f; // Khoảng cách tối thiểu để coi là "trúng"
    public GameObject explosion;
    public GameObject explosionTwo;
    // private Animator camAnim;
    private Transform target; // Tham chiếu đến boss

    private void Start()
    {
        // camAnim = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Animator>();
        // Tìm boss khi đạn được tạo
        target = GameObject.FindGameObjectWithTag("Boss")?.transform;
        // Tự hủy sau 5 giây nếu không trúng
        Destroy(gameObject, 5.0f);
    }

    private void Update()
    {
        if (target != null)
        {
            // Tính khoảng cách từ đạn đến boss
            float distance = Vector3.Distance(transform.position, target.position);
            
            // Nếu chưa đủ gần, di chuyển đến boss
            if (distance > closeDistance)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            }
            // Khi đủ gần, coi như trúng và hủy đạn
            else
            {
                OnHitBoss();
            }
        }
    }

    private void OnHitBoss()
    {
        if (target != null)
        {
            // camAnim.SetTrigger("shake");
            target.GetComponent<Boss>().health -= damage;
            Instantiate(explosion, transform.position, Quaternion.identity);
            Instantiate(explosionTwo, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss"))
        {
            OnHitBoss();
        }
    }
}