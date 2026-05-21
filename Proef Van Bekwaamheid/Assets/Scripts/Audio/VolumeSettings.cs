using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Awake()
    {
        SaveVolume();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", DecibelToVolume(volume));
    }

    public void UpdateSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", DecibelToVolume(volume));
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", VolumeToDecibel(musicVolume));

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", VolumeToDecibel(sfxVolume));
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
    
    public float DecibelToVolume(float volume)
    {
        float decibelVolume = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        return decibelVolume;
    }

    public float VolumeToDecibel(float volume)
    {
        float decibelVolume = Mathf.Pow(10f, volume / 20f);
        return decibelVolume;
    }
}
