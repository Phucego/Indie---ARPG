using System.Collections;
using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;  // Range at which the orb will move to the player
    [SerializeField] private float moveSpeed = 2f;      // Speed at which the orb moves to the player
    [SerializeField] private AudioClip pickupSound;     // Sound when picked up
    private Transform player;  // Reference to the player
    private bool isMoving = false;  // Flag to check if the orb is moving towards the player
    private PlayerMovement playerMovement; 

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;  // Find the player by tag
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Check if the orb is within the detection range and start moving towards the player
        if (!isMoving && Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            StartCoroutine(MoveToPlayer());
        }
    }

    private IEnumerator MoveToPlayer()
    {
        isMoving = true;

        // Move towards the player until it reaches them
        while (Vector3.Distance(transform.position, player.position) > 0.1f)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // Play sound when moving towards the player
            if (!playerMovement.GetComponent<AudioSource>().isPlaying)
            {
                playerMovement.GetComponent<AudioSource>().PlayOneShot(pickupSound);
            }

            yield return null;
        }

        // Optionally, play sound and destroy the orb when it reaches the player
        if (pickupSound != null)
        {
            playerMovement.GetComponent<AudioSource>().PlayOneShot(pickupSound);
        }
        
        Destroy(gameObject); // Destroy the orb after pickup
    }
}
