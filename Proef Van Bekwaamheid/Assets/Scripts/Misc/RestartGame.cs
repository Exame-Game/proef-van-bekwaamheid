using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class RestartGame : MonoBehaviour
{
    public void RestartGameScene()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}