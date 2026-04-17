using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private EnemyStateBase _currentState;

    private void Start()
    {
        TransitionTo(new PatrollingState(this));
    }

    private void Update()
    {
        _currentState?.OnUpdate();
    }

    public void TransitionTo(EnemyStateBase newState)
    {
        _currentState?.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }
}
