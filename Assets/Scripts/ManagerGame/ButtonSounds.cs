using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound; // Âm thanh khi hover
    [SerializeField] private AudioClip clickSound; // Âm thanh khi click

    // Sự kiện khi con trỏ chuột hover vào nút
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            AudioManager.Instance.PlaySound(hoverSound);
        }
    }

    // Sự kiện khi click vào nút
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }
    }
}
