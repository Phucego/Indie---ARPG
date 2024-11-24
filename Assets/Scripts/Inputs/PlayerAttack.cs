using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown;
    public float attackRange = 2.0f;
    public int attackDamage = 10;

    [SerializeField] private float nextAttackTime = 0f;

    public StaminaManager staminaManager;
    private PlayerMovement _playerMovement;
    
    public float staminaCost = 20f;
   
    public const int _enemyLayerValue = 6;
    public const int _breakablePropsValue = 7;

    private void Start()
    {
        staminaManager = GetComponentInChildren<StaminaManager>();
        _playerMovement = GetComponentInChildren<PlayerMovement>();

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button for attack
        {
            PerformAttack();
         
        }
    }

    void PerformAttack()
    {
        if (Time.time >= nextAttackTime && staminaManager.HasEnoughStamina(staminaCost) && !PlayerMovement.Instance.isBlocking)
        {
            _playerMovement.ChangeAnimation("Melee_Slice");
            staminaManager.UseStamina(staminaCost); // Consume stamina

            nextAttackTime = Time.time + attackCooldown;

            // Define an attack area in front of the player
            Vector3 attackCenter = transform.position + transform.forward * attackRange / 2;
            Vector3 attackSize = new Vector3(attackRange, 1.0f, attackRange);
            
            // Detect objects in the attack area
            Collider[] hits = Physics.OverlapBox(attackCenter, attackSize / 2, transform.rotation);
            foreach (Collider hit in hits)
            {
                int layerHit = hit.gameObject.layer;
                
                switch (layerHit)
                {
                    case _enemyLayerValue: // 6 is the enemy layer
                        EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage(attackDamage); // Apply damage
                        }
                        break;

                    case _breakablePropsValue: // 7 is the breakable props layer
                        BreakableProps breakableProps = hit.GetComponent<BreakableProps>();
                        if (breakableProps != null)
                        {
                            breakableProps.Destroy();
                        }
                        break;
                }
            }
        }
        else
        {
            Debug.Log("Not enough stamina or still in cooldown.");
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the attack area
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + transform.forward * attackRange / 2;
        Vector3 attackSize = new Vector3(attackRange, 1.0f, attackRange);
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, attackSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}