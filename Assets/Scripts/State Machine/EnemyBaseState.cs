using UnityEngine;

public class EnemyBaseState : MonoBehaviour
{
    public IEnemyState currentState;
    //private Animator animator;

    private void Start()
    {
        //animator = GetComponent<Animator>();
        // Initialize with Idle state
        currentState = new EnemyIdleState(this/*, animator*/);
    }

    private void Update()
    {
        currentState.Execute();
        
        Debug.Log(currentState);
    }

    public void SwitchState(IEnemyState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}