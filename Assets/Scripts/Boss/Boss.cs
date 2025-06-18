using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{

    public int health;
    public int damage;
    [SerializeField] private Collider2D bossColi;
    public Slider healthBar;
    private Animator anim;
    public bool isDead;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {

        if (health <= 16)
        {
            anim.SetTrigger("stageTwo");
        }

        if (health <= 0)
        {
            anim.SetTrigger("death");
            bossColi.enabled = false;
            GameObject onWayJump = GameObject.Find("OnWayJump (1)");
            if (onWayJump != null)
            {
                // Lấy vị trí hiện tại
                Vector3 newPosition = onWayJump.transform.position;
                // Tăng tọa độ Y lên
                newPosition.y += 5f;
                // Áp dụng vị trí mới
                onWayJump.transform.position = newPosition;
            }
        }

        healthBar.value = health;
    }
}
