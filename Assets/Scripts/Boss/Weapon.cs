using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public GameObject projectile;
    public Transform shotPoint;

    public int rotationOffset;
    private PlayerSound playerSound;

    public void Start()
    {
        playerSound = GetComponentInParent<PlayerSound>();
    }
    private void Update()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");

        Vector3 difference = boss.transform.position - transform.position;
        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + rotationOffset);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ItemElec"))

        {
            Instantiate(projectile, shotPoint.position, transform.rotation);
            playerSound.PlayAttack();
        }
    }
}
