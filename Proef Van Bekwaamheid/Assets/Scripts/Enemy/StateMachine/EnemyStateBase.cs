public abstract class EnemyStateBase
{
    protected EnemyStateMachine Machine;

    public EnemyStateBase(EnemyStateMachine machine)
    {
        Machine = machine;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnUpdate();
}
