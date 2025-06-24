using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReFillDash : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator crystalAnim;
    private SpriteRenderer srCrystal;
    private PlayerSound playerSound;
    void Start()
    {
        crystalAnim = gameObject.GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        playerSound = GameObject.Find("Player").GetComponent<PlayerSound>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement.Instance.ResetDash();
            crystalAnim.SetBool("claim", true);
            playerSound.PlayAttack();
            StartCoroutine(HideForSeconds(3f)); // Ẩn GameObject trong 2 giây
        }
    }

    private IEnumerator HideForSeconds(float seconds)
    {
        yield return new WaitForSeconds(15/60f); 
        crystalAnim.SetBool("claim", false);
        srCrystal = gameObject.GetComponent<SpriteRenderer>();
        Collider2D colCrystal = gameObject.GetComponent<Collider2D>();
        srCrystal.color = new Color(1f,1f,1f, 0.03f);
        colCrystal.enabled = false;
        yield return new WaitForSeconds(seconds); // Chờ 2 giây
        srCrystal.color = new Color(1f,1f,1f, 1f);
        colCrystal.enabled = true;
    }
}
