using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    [Header("Participants")]
    [SerializeField] private Transform player;
    [SerializeField] private List<EnemyController> enemies = new();

    [Header("Selection")]
    [SerializeField, Min(0f)] private float selectionRange = 15f;
    [SerializeField, Min(0.05f)] private float selectionInterval = 0.25f;

    [Header("Pacing")]
    [SerializeField, Min(0f)] private float minAttackGap = 0.5f;
    [SerializeField, Min(0f)] private float maxAttackGap = 1.5f;

    [Header("Reservation")]
    [Tooltip("Maximum time to approach and START an attack.")]
    [SerializeField, Min(0.1f)] private float approachTimeout = 4f;

    [Header("Debug")]
    [SerializeField] Color debugColour = Color.crimson;

    private readonly List<EnemyController> candidates = new();

    public EnemyController CurrentAttacker { get; private set; }

    private EnemyController previousAttacker;
    private float nextSelectionTime;
    private float reservationExpires;
    private bool attackStarted;

    private void OnEnable()
    {
        nextSelectionTime = Time.time + minAttackGap;
    }

    private void Update()
    {
        // Cancel reservations and exit early if there is no player
        if (player == null)
        {
            CancelCurrentReservation();
            return;
        }

        if (CurrentAttacker != null)
        {
            // Cancel the attack if the attacker is no longer available
            if (!IsAvailable(CurrentAttacker))
            {
                CancelCurrentReservation();
                return;
            }

            // Cancel the attack if the enemy fails to attack within the set time
            if (!attackStarted && Time.time >= reservationExpires)
                CancelCurrentReservation();

            return;
        }

        // Wait until the next selection time
        if (Time.time < nextSelectionTime)
            return;

        // Once the next selection time has been reached, reset and find an attacker
        nextSelectionTime = Time.time + selectionInterval;
        SelectAttacker();
    }

    private void SelectAttacker()
    {
        candidates.Clear();

        foreach (EnemyController enemy in enemies)
        {
            // Ignore unavailable enemies
            if (!IsAvailable(enemy))
                continue;

            // Ignore enemies that are already attacking
            if (enemy.Combat.IsAttacking)
                continue;

            float distanceSq = (enemy.transform.position - player.position).sqrMagnitude;

            // Ignore if outside the selection range
            if (distanceSq > selectionRange * selectionRange)
                continue;

            candidates.Add(enemy);
        }

        // Remove the previous attacker if there are enough candidates
        if (candidates.Count > 1)
            candidates.Remove(previousAttacker);

        // Exit early if there are no eligible candidates
        if (candidates.Count == 0)
            return;

        // Assign a random enemy
        CurrentAttacker = candidates[Random.Range(0, candidates.Count)];
        attackStarted = false;
        reservationExpires = Time.time + approachTimeout;
    }

    private bool IsAvailable(EnemyController enemy)
    {
        return enemy != null
            && enemy.isActiveAndEnabled
            && enemy.Combat != null
            && enemy.Combat.isActiveAndEnabled
            && enemy.Target == player;

        // todo: add is alive to this check
    }

    public bool HasAttackPermission(EnemyController enemy)
    {
        return enemy != null && CurrentAttacker == enemy;
    }

    public void NotifyAttackStarted(EnemyController enemy)
    {
        if (HasAttackPermission(enemy))
            attackStarted = true;
    }

    public void ReleaseAttackPermission(EnemyController enemy)
    {
        if (!HasAttackPermission(enemy))
            return;

        // Attack has completed or failed, reassign the current attacker
        previousAttacker = CurrentAttacker;
        CurrentAttacker = null;
        attackStarted = false;

        nextSelectionTime = Time.time + Random.Range(minAttackGap, maxAttackGap);
    }

    // Register new enemies
    public void Register(EnemyController enemy)
    {
        if (enemy != null && !enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    // Unregister new enemies
    public void Unregister(EnemyController enemy)
    {
        // Cancel the current attack if possible
        if (HasAttackPermission(enemy))
            CancelCurrentReservation();

        enemies.Remove(enemy);
    }

    private void CancelCurrentReservation()
    {
        EnemyController enemy = CurrentAttacker;

        CurrentAttacker = null;
        attackStarted = false;
        nextSelectionTime = Time.time + minAttackGap;

        if (enemy != null && enemy.Combat != null)
            enemy.Combat.InterruptAttack();
    }

    private void OnDisable()
    {
        CancelCurrentReservation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = debugColour;

        foreach (EnemyController enemy in enemies)
        {
            Gizmos.DrawWireSphere(enemy.transform.position, selectionRange);
        }
    }
}