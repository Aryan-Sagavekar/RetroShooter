using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float timeBetweenAttacks = 1f;
    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;

    [Header("Agent Settings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float walkPointRange;

    private NavMeshAgent agent;
    private EnemyBehaviour enemyBehaviour;
    private Animator animator;
    private float currentHealth;
    private Transform player;
    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool attacked = false;
    private bool playerInSightRange, playerInAttackRange;

    // Animation parameter names (make sure these match your Animator Controller)
    private readonly string SPEED_PARAM = "Speed";
    private readonly string IS_RUNNING_PARAM = "IsRunning";
    private readonly string ATTACK_TRIGGER = "Attack";
    private readonly string IS_DEAD_PARAM = "IsDead";

    // Current state tracking
    private EnemyState currentState = EnemyState.Patrolling;

    private enum EnemyState
    {
        Patrolling,
        Chasing,
        Attacking,
        Dead
    }

    private void Awake()
    {
        player = GameManager.Instance.currentPlayer.transform;
        agent = gameObject.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        enemyBehaviour = gameObject.GetComponent<EnemyBehaviour>();

        if (animator == null)
        {
            Debug.LogWarning($"No Animator found on {gameObject.name} or its children!");
        }
    }

    private void Update()
    {
        if (currentHealth > 0)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

            if (!playerInSightRange && !playerInAttackRange)
            {
                SetState(EnemyState.Patrolling);
                Patroling();
            }
            if (playerInSightRange && !playerInAttackRange)
            {
                SetState(EnemyState.Chasing);
                ChasePlayer();
            }
            if (playerInSightRange && playerInAttackRange)
            {
                SetState(EnemyState.Attacking);
                AttackPlayer();
            }

            UpdateAnimations();
        }
    }

    private void SetState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnStateChanged(newState);
        }
    }

    private void OnStateChanged(EnemyState newState)
    {
        if (animator == null) return;

        // Reset all animation bools first
        animator.SetBool(IS_RUNNING_PARAM, false);

        switch (newState)
        {
            case EnemyState.Patrolling:
                // Walking animation will be handled by Speed parameter
                break;

            case EnemyState.Chasing:
                animator.SetBool(IS_RUNNING_PARAM, true);
                break;

            case EnemyState.Attacking:
                // Attack animation will be triggered in AttackPlayer method
                break;

            case EnemyState.Dead:
                animator.SetBool(IS_DEAD_PARAM, true);
                break;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Update speed parameter based on agent velocity
        float speed = agent.velocity.magnitude;
        float normalizedSpeed = speed / agent.speed; // Normalize to 0-1 range
        animator.SetFloat(SPEED_PARAM, normalizedSpeed);
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Stop moving and look at player
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!attacked)
        {
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger(ATTACK_TRIGGER);
            }

            // Perform the actual attack
            enemyBehaviour.PerformAttack(attackDamage, player.transform);
            attacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        attacked = false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            SetState(EnemyState.Dead);

            // Disable the NavMeshAgent when dead
            if (agent != null)
                agent.enabled = false;

            Invoke(nameof(DestroyEnemy), 2f); // Give time for death animation
        }
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    public float AttackDamage => attackDamage;

    // Optional: Animation Events (call these from animation events in your attack animation)
    public void OnAttackStart()
    {
        // Called at the start of attack animation
        Debug.Log("Attack animation started");
    }

    public void OnAttackHit()
    {
        // Called at the moment the attack should deal damage
        // You can move the actual damage dealing logic here if you want frame-perfect timing
        Debug.Log("Attack hit frame");
    }

    public void OnAttackEnd()
    {
        // Called at the end of attack animation
        Debug.Log("Attack animation ended");
    }
}