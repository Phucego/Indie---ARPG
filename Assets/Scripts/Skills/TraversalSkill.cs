using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "TraversalSkill", menuName = "ScriptableObjects/Skills/Traversal")]
public class TraversalSkill : Skill
{
    public float dashDistance = 5f; // How far the player will dash
    public float dashDuration = 0.2f; // Duration of the dash
    public float dashCooldown = 1f; // Time before the skill can be used again
    public GameObject trailEffectPrefab; // Trail effect prefab to instantiate
    public Transform trailEffectParent; // The parent object to which the trail effect will be attached

    public override void UseSkill(PlayerAttack playerAttack)
    {
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            Debug.Log("Not enough stamina to use Traversal skill.");
            return;
        }

        playerAttack.StartCoroutine(TraversalRoutine(playerAttack));
    }

    private IEnumerator TraversalRoutine(PlayerAttack playerAttack)
    {
        if (playerAttack.isSkillActive)
        {
            Debug.Log("Traversal skill is already active.");
            yield return null;
        }

        playerAttack.staminaManager.UseStamina(staminaCost);
        playerAttack.isSkillActive = true;

        // Instantiate the trail effect at the player's position and attach it to the player
        GameObject trailEffect = Instantiate(trailEffectPrefab, playerAttack.transform.position, Quaternion.identity);
        trailEffect.transform.SetParent(trailEffectParent); // Optionally set a parent to manage the trail's lifetime

        // Dash in the direction the player is facing
        Vector3 dashDirection = playerAttack.playerTransform.forward; // Dash in the direction the player is facing
        Vector3 targetPosition = playerAttack.transform.position + dashDirection * dashDistance;

        // Adjust the trail's position slightly behind and above the player's position
        Vector3 trailOffset = -dashDirection * 1f; // Move the trail a bit behind the player (1 unit behind)
        trailOffset.y = 0.5f; 

        float startTime = Time.time;
        Vector3 startPos = playerAttack.transform.position;

        while (Time.time - startTime < dashDuration)
        {
            playerAttack.transform.position = Vector3.Lerp(startPos, targetPosition, (Time.time - startTime) / dashDuration);

            // Update trail position to follow the player but offset slightly behind
            trailEffect.transform.position = playerAttack.transform.position + trailOffset;

            yield return null;
        }

        // Destroy the trail effect after the dash is completed
        Destroy(trailEffect);

        playerAttack.isSkillActive = false;
        Debug.Log("Traversal skill completed.");
    }
}
