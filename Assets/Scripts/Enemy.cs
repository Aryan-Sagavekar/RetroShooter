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
    private readonly string IS_RUNNING_PARAM = "IsRunning";
    private readonly string ATTACK_TRIGGER = "Attack";
    private readonly string IS_DEAD_PARAM = "IsDead";

    // Current state tracking
    private EnemyState currentState = EnemyState.Idle;

    private enum EnemyState
    {
        Idle,
        Walking,
        Running,
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
                Patroling();
            }
            if (playerInSightRange && !playerInAttackRange)
            {
                SetState(EnemyState.Running);
                ChasePlayer();
            }
            if (playerInSightRange && playerInAttackRange)
            {
                SetState(EnemyState.Attacking);
                AttackPlayer();
            }
        }
    }

    private void SetState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateAnimationState(newState);
        }
    }

    private void UpdateAnimationState(EnemyState state)
    {
        if (animator == null) return;

        // Reset all animation bools and triggers
        animator.SetBool(IS_RUNNING_PARAM, false);

        switch (state)
        {
            case EnemyState.Idle:
                // All bools are false = idle animation plays
                break;

            case EnemyState.Running:
                animator.SetBool(IS_RUNNING_PARAM, true);
                break;

            case EnemyState.Attacking:
                // Attack trigger will be set in AttackPlayer method
                break;

            case EnemyState.Dead:
                animator.SetBool(IS_DEAD_PARAM, true);
                break;
        }
    }

    private void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
            SetState(EnemyState.Idle);
        }

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);

            // Check if agent is moving to determine walk vs idle
            if (agent.velocity.magnitude > 0.1f)
            {
                SetState(EnemyState.Walking);
            }
            else
            {
                SetState(EnemyState.Idle);
            }
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
            SetState(EnemyState.Idle);
        }
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
        Debug.Log("Attack animation started");
    }

    public void OnAttackHit()
    {
        Debug.Log("Attack hit frame");
    }

    public void OnAttackEnd()
    {
        Debug.Log("Attack animation ended");
    }

    private void OnDestroy()
    {
        CancelInvoke(); 
        Debug.Log($"{gameObject.name} destroyed");
    }

}