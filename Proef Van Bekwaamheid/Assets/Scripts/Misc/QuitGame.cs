using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("QuitGame: Stopped Play Mode (Editor).");
#else
        Debug.Log("QuitGame: Application quitting.");
        Application.Quit();
#endif
    }
}
