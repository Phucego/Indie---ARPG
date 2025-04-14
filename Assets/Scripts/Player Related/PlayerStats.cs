using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHP = 100f;
    public float currentHP;
    public float defense = 10f;
    public float attackPower = 20f;
    public float damageBonus = 0f; // Added bonus damage from skills, equipment, etc.
    public float experience = 0f;

    [Header("Movement Modifiers")]
    [Range(0.1f, 2f)] public float movementSpeedModifier = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;

    [Header("Death Handling")]
    public GameObject deathVFX;
    public float respawnDelay = 3f;

    private PlayerMovement playerMovement;
    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isDead)
        {
            UpdateMovementSpeed();
        }
    }

    private void UpdateMovementSpeed()
    {
        float speed = baseMoveSpeed * movementSpeedModifier;

        if (currentHP < maxHP * 0.3f)
        {
            speed *= 1.2f;
        }

        if (experience > 100)
        {
            speed *= 1.1f;
        }

        playerMovement.moveSpeed = speed;
    }

    public void TakeDamage(float damage)
    {
        float effectiveDamage = Mathf.Max(0, damage - defense);
        currentHP -= effectiveDamage;

        if (currentHP <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public void GainExperience(float amount)
    {
        experience += amount;

        if (experience >= 100)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        experience = 0;
        attackPower += 10f;
        maxHP += 20f;
        currentHP = maxHP;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died.");

        if (deathVFX != null)
        {
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        }

        // Disable movement and other components
        playerMovement.canMove = false;
        GetComponent<Animator>()?.SetTrigger("Die");

        
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(respawnDelay);
        
    }
}
