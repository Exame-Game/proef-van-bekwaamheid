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
    private const string _walkBoolName = "walking";
    private const string _grabBoolName = "carrying";
    private const string _specialTriggerName = "specialIdle";

    [SerializeField] private Animator animator;

    // The random wait range (in seconds) before the special idle triggers
    [SerializeField] private float _minWait;
    [SerializeField] private float _maxWait;

    // Cached state to determine whether the player is currently idle
    private bool _carrying;
    private bool _walking;

    // Reference to the running coroutine so we can stop it if the player moves
    private Coroutine _doSpecialIdleRoutine;

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
        if (_doSpecialIdleRoutine == null)
            _doSpecialIdleRoutine = StartCoroutine(DoSpecialIdle());
    }

    /// <summary>
    /// Updates the walking animation state.
    /// Call this from the movement system whenever the player starts or stops walking.
    /// </summary>
    public void SetWalk(bool walking)
    {
        animator.SetBool(_walkBoolName, walking);
        _walking = walking;
    }

    /// <summary>
    /// Updates the carrying animation state.
    /// Call this from the item system whenever the player picks up or drops an item.
    /// </summary>
    public void SetGrab(bool grabing)
    {
        animator.SetBool(_grabBoolName, grabing);
        _carrying = grabing;
    }

    /// <summary>
    /// Waits a random duration, then fires the special idle trigger.
    /// Resets itself to null so the cycle can restart on the next Update.
    /// </summary>
    private IEnumerator DoSpecialIdle()
    {
        Debug.Log("start Coroutine");
        yield return new WaitForSeconds(Random.Range(_minWait, _maxWait));

        animator.SetTrigger(_specialTriggerName);

        // Null out so Update knows the routine has finished and can start a new one
        _doSpecialIdleRoutine = null;
    }
}