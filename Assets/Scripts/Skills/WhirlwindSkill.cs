using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "WhirlwindSkill", menuName = "ScriptableObjects/Skills/Whirlwind")]
public class WhirlwindSkill : Skill
{
    public float damagePerTick = 5f;
    public float staminaCostPerTick = 15f;
    public float effectDuration = 3f; // Total duration of the skill
    public GameObject whirlwindEffectPrefab;

    public override void UseSkill(PlayerAttack playerAttack)
    {
        if (!playerAttack.CanUseWhirlwind())
        {
            Debug.Log("Cannot use Whirlwind. A two-handed weapon is required.");
            return;
        }

        // Ensure the player has enough stamina before starting
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCostPerTick))
        {
            Debug.Log("Not enough stamina to start Whirlwind.");
            return;
        }

        playerAttack.StartCoroutine(WhirlwindRoutine(playerAttack));
    }

    private IEnumerator WhirlwindRoutine(PlayerAttack playerAttack)
    {
        float startTime = Time.time;
        playerAttack.isSkillActive = true;
        playerAttack.playerMovement.canMove = true;

        GameObject effectInstance = null;
        if (whirlwindEffectPrefab)
        {
            Vector3 effectPosition = playerAttack.transform.position + Vector3.up * 1.2f; // Adjusted to spawn at body height
            effectInstance = Instantiate(whirlwindEffectPrefab, effectPosition, Quaternion.identity, playerAttack.transform);
        }

        while (Time.time - startTime < effectDuration)
        {
            // Check if the player has enough stamina *before* deducting it
            if (!playerAttack.staminaManager.HasEnoughStamina(staminaCostPerTick))
            {
                Debug.Log("Whirlwind stopped due to insufficient stamina.");
                break; // Exit the loop if not enough stamina
            }

            playerAttack.staminaManager.UseStamina(staminaCostPerTick);
            playerAttack.animator.Play(playerAttack.whirlwindAttack.name);
            ApplyDamage(playerAttack);
            yield return new WaitForSeconds(playerAttack.whirlwindAttack.length * 0.5f);
        }

        if (effectInstance) Destroy(effectInstance);
        playerAttack.isSkillActive = false;
    }

    private void ApplyDamage(PlayerAttack playerAttack)
    {
        Vector3 hitboxPosition = playerAttack.playerTransform.position + Vector3.up * 1.2f + playerAttack.playerTransform.forward * (playerAttack.attackRange * 0.5f);
        Vector3 hitboxSize = new Vector3(1.5f, 2.0f, playerAttack.attackRange);

        Collider[] hits = Physics.OverlapBox(hitboxPosition, hitboxSize / 2, playerAttack.playerTransform.rotation, playerAttack.weaponManager.enemyLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                float totalDamage = damagePerTick;
                Weapon currentWeapon = playerAttack.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    totalDamage += currentWeapon.weaponData.damageBonus;
                }

                enemy.TakeDamage(totalDamage, (enemy.transform.position - playerAttack.playerTransform.position).normalized);
            }
        }
    }
}
