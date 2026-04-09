using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    public static PlayerCameraManager Instance { get; private set; }

    [SerializeField] private CinemachineTargetGroup targetGroup;

    void Awake() => Instance = this;

    public void RegisterPlayer(Transform player)
    {
        targetGroup.AddMember(player, weight: 1f, radius: 2f);
    }

    public void UnregisterPlayer(Transform player)
    {
        targetGroup.RemoveMember(player);
    }
}