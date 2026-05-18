using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    [SerializeField] private MusicTrack[] musicTracks;
    
    public AudioClip GetClipFromName(string trackName)
    {
        foreach (MusicTrack track in musicTracks)
        {
            if (track.trackName == trackName)
                return track.clip;
        }

        Debug.LogWarning("Music track not found: " + trackName);
        return null;
    }
}
