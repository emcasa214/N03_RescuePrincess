using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip dashSound;
    public AudioClip deathSound;
    public AudioClip deathPre;
    public AudioClip respawn;
    public AudioClip strawberry;
    public AudioClip attack;
    public AudioClip checkPoint;

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
    public void PlayDash() => PlaySound(dashSound,0.5f);
    public void PlayDeath() => PlaySound(deathSound);
    public void PlayPreDeath() => PlaySound(deathPre);
    public void PlayRespawn() => PlaySound(respawn);
    public void PlayCollect() => PlaySound(strawberry,0.5f);
    public void PlayAttack() => PlaySound(attack,0.5f);
    public void PlayCheckPoint() => PlaySound(checkPoint,0.5f);

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
