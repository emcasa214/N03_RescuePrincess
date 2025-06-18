using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip dashSound;
    public AudioClip deathSound;
    [Header("Volumes")]
    public float landVolume = 0.7f;
    public float jumpVolume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayLand() => PlaySound(landSound, landVolume);

    public void PlayJump() => PlaySound(jumpSound, jumpVolume);
    public void PlayDash() => PlaySound(dashSound);

    public void PlayDeath() => PlaySound(deathSound);

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
