using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    [Header("Lock-On Settings")]
    public float maxLockOnDistance = 20f;
    public float minLockOnDistance = 2f;
    public float rotationSpeed = 15f;
    public LayerMask enemyLayer;
    public GameObject lockOnIcon;

    [Header("Visual Indicators")]
    public float iconOffset = 1.5f;

    private Transform currentTarget;
    public List<Transform> availableTargets = new List<Transform>();
    private Camera mainCamera;
    private PlayerMovement playerMovement;
    private bool isLocked;

    private void Start()
    {
        mainCamera = Camera.main;
        playerMovement = GetComponent<PlayerMovement>();

        if (lockOnIcon != null)
            lockOnIcon.SetActive(false);
    }

    private void Update()
    {
        // Prevent lock-on when dodging
        if (playerMovement.IsDodging)
        {
            if (isLocked) DisableLockOn();
            return;
        }

        // Lock-On Toggle
        if (Input.GetMouseButtonDown(2)) // Middle Mouse Button (Lock-On Toggle)
        {
            if (!isLocked)
                TryLockOn();
            else
                DisableLockOn();
        }

        if (isLocked)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow Keys
            if (horizontalInput > 0.1f)
                CycleTarget(true);
            else if (horizontalInput < -0.1f)
                CycleTarget(false);

            UpdateLockOnIconPosition();

            if (currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                if (distanceToTarget > maxLockOnDistance)
                {
                    DisableLockOn();
                }
                else
                {
                    HandleLockedRotation();
                }
            }
            else
            {
                DisableLockOn();
            }
        }
    }

    private void HandleLockedRotation()
    {
        if (playerMovement.IsDodging) return;

        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        directionToTarget.y = 0;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void TryLockOn()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxLockOnDistance, enemyLayer);
        availableTargets.Clear();

        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance >= minLockOnDistance && distance <= maxLockOnDistance)
            {
                availableTargets.Add(col.transform);
            }
        }

        if (availableTargets.Count > 0)
        {
            // Prioritize enemies in front of the player
            availableTargets = availableTargets.OrderBy(t => GetScreenPositionScore(t)).ToList();
            currentTarget = availableTargets[0];
            isLocked = true;

            if (lockOnIcon != null)
                lockOnIcon.SetActive(true);
        }
    }

    private float GetScreenPositionScore(Transform target)
    {
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.position);
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        return Vector2.Distance(screenCenter, new Vector2(screenPoint.x, screenPoint.y));
    }

    private void CycleTarget(bool next)
    {
        if (availableTargets.Count <= 1) return;

        int currentIndex = availableTargets.IndexOf(currentTarget);
        int newIndex = next ? (currentIndex + 1) % availableTargets.Count : (currentIndex - 1 + availableTargets.Count) % availableTargets.Count;

        currentTarget = availableTargets[newIndex];
    }

    private void UpdateLockOnIconPosition()
    {
        if (currentTarget != null && lockOnIcon != null)
        {
            Vector3 targetPosition = currentTarget.position + Vector3.up * iconOffset;
            lockOnIcon.transform.position = targetPosition;
            lockOnIcon.transform.LookAt(mainCamera.transform);
        }
    }

    private void DisableLockOn()
    {
        isLocked = false;
        currentTarget = null;
        if (lockOnIcon != null)
            lockOnIcon.SetActive(false);
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}
