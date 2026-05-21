using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    private void Start()
    {
        MusicManager.Instance.PlayMusic("DayMusic");
    }
}
