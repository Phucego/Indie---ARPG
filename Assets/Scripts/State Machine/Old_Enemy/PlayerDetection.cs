using System;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    public float detectionRadius = 10f;
    public float detectionAngle = 45f; // Half of the total cone angle

    private void Update()
    {
        Vector3 directionToPlayer = PlayerMovement.Instance.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer < detectionRadius)
        {
            directionToPlayer.Normalize();
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
            
            if (angleToPlayer < detectionAngle && dotProduct > 0) // Ensures player is in front
            {
                // Player is within the cone
                Debug.Log("Found the player");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward * detectionRadius;
        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawLine(transform.position, transform.position + forward);

        int segments = 10;
        for (int i = 0; i <= segments; i++)
        {
            float angle = -detectionAngle + (i * (2 * detectionAngle / segments));
            Vector3 point = Quaternion.Euler(0, angle, 0) * forward;
            Gizmos.DrawLine(transform.position, transform.position + point);
        }
    }
}