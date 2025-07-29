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
    private float currentHealth;
    private Transform player; // Reference to the player GameObject
    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool attacked = false;
    private bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameManager.Instance.currentPlayer.transform;
        agent = gameObject.GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f) walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask)) walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!attacked)
        {
            //TODO: Add the attack logic here. different enemies would have different attacks

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

        if (currentHealth <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}