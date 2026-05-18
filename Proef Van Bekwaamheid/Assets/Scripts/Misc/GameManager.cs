using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int playersReady = 0;
    private int totalPlayers = 2;
    public bool AllPlayersReady { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayerPressedReady()
    {
        playersReady++;

        if (playersReady == totalPlayers)
        {
            AllPlayersReady = true;
        }
    }

    public void SetTotalPlayers(int count)
    {
        totalPlayers = count;
        playersReady = 0;
        AllPlayersReady = false;
    }
}