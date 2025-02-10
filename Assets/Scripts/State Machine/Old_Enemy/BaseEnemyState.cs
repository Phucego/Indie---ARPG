using UnityEngine;

public abstract class BaseEnemyState : IEnemyState
{
    protected EnemyController enemyController;

    public BaseEnemyState(EnemyController controller)
    {
        enemyController = controller;
    }

    public virtual void Enter() { }
    public abstract void Execute();
    public virtual void Exit() { }
}