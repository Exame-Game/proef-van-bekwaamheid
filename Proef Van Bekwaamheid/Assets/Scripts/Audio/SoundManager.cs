using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundLibrary _sfxLibrary;
    [SerializeField] private AudioSource _sfx2DSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound3D(AudioClip clip, Vector3 position)
    {
        if (clip == null)
        {
            Debug.LogWarning("Attempted to play a null audio clip.");
            return;
        }
        
        AudioSource.PlayClipAtPoint(clip, position);
    }

    public void PlaySound3D(Vector3 position, string soundName)
    {
        PlaySound3D(_sfxLibrary.GetClipFromName(soundName), position);
    }

    public void PlaySound2D(string soundName)
    {
       _sfx2DSource.PlayOneShot(_sfxLibrary.GetClipFromName(soundName));
    }
}