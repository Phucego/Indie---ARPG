using UnityEngine;
using System.Collections;

public class SmokeBombSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Smoke Bomb";
        description = "Throw a smoke bomb at the mouse position, blinding enemies and turning the player invisible while in the smoke.";
        staminaCost = 25f;
        cooldown = 15f;
        isHoldable = false;
        tickRate = 0f;
    }

    protected override void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, playerAttack.targetLayerMask))
        {
            Vector3 smokePos = hit.point;
            Collider[] hits = Physics.OverlapSphere(smokePos, 5f, playerAttack.targetLayerMask);
            foreach (var hitCollider in hits)
            {
                if (hitCollider.TryGetComponent(out EnemyHealth enemy))
                {
                    enemy.Blind(5f);
                }
            }

            playerAttack.StartCoroutine(ManageInvisibility(playerAttack, smokePos));
            Debug.Log("Smoke Bomb VFX at " + smokePos);
            playerAttack.animator.Play("Attack_Combo_1");
        }
        else
        {
            Debug.LogWarning($"{skillName} failed: No valid target location.", this);
        }
    }

    private IEnumerator ManageInvisibility(PlayerAttack playerAttack, Vector3 smokePos)
    {
        playerAttack.SetInvisible(true);
        float duration = 5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float distanceToSmoke = Vector3.Distance(playerAttack.transform.position, smokePos);
            if (distanceToSmoke > 5f)
            {
                playerAttack.SetInvisible(false);
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerAttack.SetInvisible(false);
    }
}