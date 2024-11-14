using UnityEngine;

public class EnemyBaseState : MonoBehaviour
{
    private IEnemyState currentState;

    public void SwitchState(IEnemyState newState)
    {
        // Switch to the new state
        currentState = newState;

        // Call Enter on the new state
        currentState.Enter();
    }

    private void Update()
    {
        // Execute the current state's behavior
        if (currentState != null)
        {
            currentState.Execute();
        }
    }
}