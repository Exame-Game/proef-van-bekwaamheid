using System.Collections;
using UnityEngine;

/// <summary>
/// Controls player animations based on movement and carrying state.
/// Automatically triggers a special idle animation after the player
/// has been idle for a random amount of time.
/// </summary>
public class PlayerAnimationControler : MonoBehaviour
{
    // Animator parameter name constants — avoids magic strings scattered through the code
    private const string k_walkBoolName = "walking";
    private const string k_grabBoolName = "carrying";
    private const string k_specialTriggerName = "specialIdle";

    [SerializeField] private Animator _animator;

    // The random wait range (in seconds) before the special idle triggers
    [SerializeField] private float _minWait;
    [SerializeField] private float _maxWait;

    // Reference to the running coroutine so we can stop it if the player moves
    private Coroutine _doSpecialIdleRoutine;

    // Cached state to determine whether the player is currently idle
    private bool _carrying;
    private bool _walking;


    private void Update()
    {
        // Player is idle only when neither walking nor carrying
        bool isIdle = !_walking && !_carrying;

        if (!isIdle)
        {
            // If a special idle coroutine is running, cancel it
            if (_doSpecialIdleRoutine != null)
                StopCoroutine(_doSpecialIdleRoutine);
            _doSpecialIdleRoutine = null;
            return;
        }

        // Start the special idle countdown if it isn't already running
        if (_doSpecialIdleRoutine != null)
            return;    
        _doSpecialIdleRoutine = StartCoroutine(DoSpecialIdle());
    }

    /// <summary>
    /// Updates the walking animation state.
    /// Call this from the movement system whenever the player starts or stops walking.
    /// </summary>
    public void SetWalk(bool walking)
    {
        _animator.SetBool(k_walkBoolName, walking);
        _walking = walking;
    }

    /// <summary>
    /// Updates the carrying animation state.
    /// Call this from the item system whenever the player picks up or drops an item.
    /// </summary>
    public void SetGrab(bool grabing)
    {
        _animator.SetBool(k_grabBoolName, grabing);
        _carrying = grabing;
    }

    /// <summary>
    /// Waits a random duration, then fires the special idle trigger.
    /// Resets itself to null so the cycle can restart on the next Update.
    /// </summary>
    private IEnumerator DoSpecialIdle()
    {
        yield return new WaitForSeconds(Random.Range(_minWait, _maxWait));

        _animator.SetTrigger(k_specialTriggerName);

        // Null out so Update knows the routine has finished and can start a new one
        _doSpecialIdleRoutine = null;
    }
}