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
    private Coroutine _doSpecialIdleRoutine;

    private void Update()
    {
        bool isIdle = !_walking && !_carrying;
        if (!isIdle)
        {
            if (_doSpecialIdleRoutine != null)
            StopCoroutine(_doSpecialIdleRoutine);
            _doSpecialIdleRoutine = null;
            return;
        }

        if (_doSpecialIdleRoutine == null)
            _doSpecialIdleRoutine = StartCoroutine(DoSpecialIdle());
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
        Debug.Log("start Coroutine");
        yield return new WaitForSeconds(Random.Range(_minWait, _maxWait));
        
        animator.SetTrigger(_specialTriggerName);
        _doSpecialIdleRoutine = null;
    }
}
