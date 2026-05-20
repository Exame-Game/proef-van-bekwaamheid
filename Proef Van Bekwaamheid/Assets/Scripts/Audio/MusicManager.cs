using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private MusicLibrary _musicLibrary;
    [SerializeField] private AudioSource _musicSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void PlayMusic(string trackName)
    {
        StartCoroutine(AnimateMusicCrossfade(_musicLibrary.GetClipFromName(trackName), 0.5f));
    }

    public void StopMusic()
    {
        StartCoroutine(AnimateMusicCrossfade(null, 0.5f));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            _musicSource.volume = Mathf.Lerp(1f, 0f, percent);
            yield return null;
        }

        _musicSource.clip = nextTrack;
        _musicSource.Play();

        percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            _musicSource.volume = Mathf.Lerp(0f, 1f, percent);
            yield return null;
        }
    }
}