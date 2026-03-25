using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string hostSceneName = "HostScene";
    [SerializeField] private string clientSceneName = "ClientScene";

    public void StartAsHost()
    {
        Debug.Log("<color=cyan>[MenuController] StartAsHost() called — loading host scene...</color>");
        SceneManager.LoadScene(hostSceneName);
    }

    public void StartAsClient()
    {
        Debug.Log("<color=cyan>[MenuController] StartAsClient() called — loading client scene...</color>");
        SceneManager.LoadScene(clientSceneName);
    }
}