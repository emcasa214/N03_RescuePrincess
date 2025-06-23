using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strawberry : MonoBehaviour
{
    private Animator strawberryAnim;
    // Start is called before the first frame update
    void Start()
    {
        strawberryAnim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            strawberryAnim.SetBool("claim", true);
            Destroy(gameObject, 8 / 60f);
        }
    }
}
