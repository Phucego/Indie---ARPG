public abstract class BaseEnemyState
{
    protected EnemyController enemy;
    public BaseEnemyState(EnemyController enemy) { this.enemy = enemy; }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}