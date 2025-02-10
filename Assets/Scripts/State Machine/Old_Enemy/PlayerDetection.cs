using System;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    public float detectionRadius = 10f;

    private void Update()
    {
        if (Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) < detectionRadius)
        {
            // Trigger some logic like switching states to Attack
            Debug.Log("Found the player");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }
}