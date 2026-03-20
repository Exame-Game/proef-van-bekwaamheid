using UnityEngine;

/// <summary>
/// Forwards UI button input to the local PlayerMovement component.
/// Polls until PlayerMovement is found in the scene, then caches it.
/// </summary>
public class NetworkController : MonoBehaviour
{
    private TestNetworkingMovement _movement;
    private bool _pcAssigned;

    private void Start()
    {
        InvokeRepeating(nameof(AssignPlayerController), 0.1f, 0.1f);
    }

    public void Right()   { if (_pcAssigned) _movement.Movement("Right"); }
    public void Left()    { if (_pcAssigned) _movement.Movement("Left"); }
    public void Forward() { if (_pcAssigned) _movement.Movement("Forward"); }
    public void Back()    { if (_pcAssigned) _movement.Movement("Back"); }

    /// <summary>Call this when the network disconnects to re-search for a fresh PlayerMovement.</summary>
    public void ResetController()
    {
        _pcAssigned = false;
        _movement = null;
        InvokeRepeating(nameof(AssignPlayerController), 0.1f, 0.1f);
    }

    private void AssignPlayerController()
    {
        if (_movement == null)
        {
            _movement = FindFirstObjectByType<TestNetworkingMovement>();
        }
        else if (_movement == FindFirstObjectByType<TestNetworkingMovement>())
        {
            _pcAssigned = true;
            CancelInvoke();
        }
    }
}
