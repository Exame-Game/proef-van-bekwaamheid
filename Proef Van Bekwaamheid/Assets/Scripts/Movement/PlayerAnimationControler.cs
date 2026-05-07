using System.Collections;
using UnityEngine;

public class PlayerAnimationControler : MonoBehaviour
{
    private const string _walkBoolName = "walking";
    private const string _grabBoolName = "carrying";
    private const string _specialTriggerName = "specialIdle";

    [SerializeField] private Animator animator;
    [SerializeField] private float _minWait;
    [SerializeField] private float _maxWait;
    
    private bool _carrying;
    private bool _walking;

    private void Start()
    {
        
    }

    public void SetWalk(bool walking)
    {
        animator.SetBool(_walkBoolName, walking);
        _walking = walking;
    }

    public void SetGrab(bool grabing)
    {
        animator.SetBool(_grabBoolName, grabing);
        _carrying = grabing;
    }

    private IEnumerator DoSpecialIdle()
    {
        yield return new WaitForSeconds(Random.Range(_minWait, _maxWait));
        if (!_carrying && !_walking)
            animator.SetTrigger(_specialTriggerName);
    }
}
