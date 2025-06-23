using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class VolumeData
{
    public float musicVolume;
    public float sfxVolume;
}

public class Volume : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private string savePath;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "volumeSettings.json");
    }

    void Start()
    {
        if (audioMixer == null || musicSlider == null || sfxSlider == null)
        {
            return;
        }
        LoadVolume();
    }

    public void UpdateMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", volume);
        }
    }

    public void UpdateSoundVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", volume);
        }
    }

    public void SaveVolume()
    {
        if (audioMixer != null)
        {
            audioMixer.GetFloat("MusicVolume", out float musicVolume);
            audioMixer.GetFloat("SFXVolume", out float sfxVolume);

            VolumeData data = new VolumeData
            {
                musicVolume = musicVolume,
                sfxVolume = sfxVolume
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
    }

    public void LoadVolume()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            VolumeData data = JsonUtility.FromJson<VolumeData>(json);

            musicSlider.value = data.musicVolume;
            sfxSlider.value = data.sfxVolume;
            audioMixer.SetFloat("MusicVolume", data.musicVolume);
            audioMixer.SetFloat("SFXVolume", data.sfxVolume);
        }
        else
        {
            musicSlider.value = 1f;
            sfxSlider.value = 1f;
            audioMixer.SetFloat("MusicVolume", 1f);
            audioMixer.SetFloat("SFXVolume", 1f);
        }
    }
}
