using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class ExperienceOrb : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float baseMoveSpeed = 8f;
    [SerializeField] private float accelerationDuration = 0.5f;
    [SerializeField] private float experienceAmount = 20f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Transform player;
    private PlayerLevel playerLevel;
    private AudioSource audioSource;

    private bool isMoving = false;
    private float elapsedTime = 0f;
    private Vector3 startPosition;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerLevel = player.GetComponent<PlayerLevel>();
            audioSource = player.GetComponent<AudioSource>();
        }

        SetupTrail(); // Apply blue holo trail
    }

    void Update()
    {
        if (!isMoving && player != null && Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            StartCoroutine(MoveToPlayer());
        }
    }

    private IEnumerator MoveToPlayer()
    {
        isMoving = true;
        startPosition = transform.position;
        Vector3 target = player.position;
        elapsedTime = 0f;

        while (Vector3.Distance(transform.position, player.position) > 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / accelerationDuration); // Goes from 0 → 1 over time
            float speedFactor = Mathf.SmoothStep(0.2f, 1f, t); // Ease-in acceleration
            float currentSpeed = baseMoveSpeed * speedFactor;

            transform.position = Vector3.MoveTowards(transform.position, player.position, currentSpeed * Time.deltaTime);

            yield return null;
        }

        // Grant EXP
        if (playerLevel != null)
        {
            playerLevel.GainExperience(experienceAmount);
        }

        // Play sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        Destroy(gameObject);
    }

    private void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();

        if (trail != null)
        {
            trail.time = 0.5f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default")); // Simple transparent shader
            trail.startColor = new Color(0.4f, 0.8f, 1f, 0.8f); // Bright blue
            trail.endColor = new Color(0.2f, 0.6f, 1f, 0f);     // Fades out
        }
    }
}
