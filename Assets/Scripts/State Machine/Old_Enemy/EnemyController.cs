using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 2f;

   // public Animator Animator { get; private set; }
    private BaseEnemyState currentState;

    private void Start()
    {
       // Animator = GetComponent<Animator>();
        ChangeState(new EnemyIdleState(this));
    }

    private void Update()
    {
        currentState?.Execute();
    }

    public void ChangeState(BaseEnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public bool IsPlayerInDetectionRange()
    {
        return Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    }

    public bool IsPlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw attack radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
