using UnityEngine;

public class StartMusic : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.PlayMusic("DayMusic");
    }
}
