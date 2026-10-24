public abstract class EnemyState
{
    protected readonly EnemyController enemy;
    protected readonly EnemyStateMachine stateMachine;

    protected EnemyState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }

    public virtual void Tick(float deltaTime) { }

    public virtual void FixedTick(float fixedDeltaTime) { }

    public virtual void Exit() { }
}