using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float moveSpeed = 3f;

    private float currentHealth;
    private GameObject player; // Reference to the player GameObject

    void Awake()
    {
        currentHealth = maxHealth;
        // Find the player. Ensure your Player GameObject is active and in the scene.
        // You might tag your player as "Player" for a more robust lookup.
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Enemy: Player GameObject not found! Please ensure your player is tagged 'Player'.");
        }
    }

    void Update()
    {
        if (player != null)
        {
            MoveTowardsPlayer();
            RotateTowardsPlayer(); // Optional: Make enemy face the player
        }
    }

    private void MoveTowardsPlayer()
    {
        // Calculate direction to player, ignoring Y-axis for 2.5D movement
        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0; // Keep movement on the XZ plane
        directionToPlayer.Normalize();

        // Move using transform.position or Rigidbody.MovePosition
        // Since the Rigidbody is Kinematic, we'll use transform.position for simplicity.
        // For more complex interactions/physics, Rigidbody.MovePosition is preferred in FixedUpdate.
        transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
    }

    private void RotateTowardsPlayer()
    {
        // Calculate direction to player, ignoring Y-axis
        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            // Create a rotation that looks in the directionToPlayer
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // Smoothly rotate towards that direction
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " died!");
        // --- TODO: Add death animation, sound, score, drop items, etc. ---
        Destroy(gameObject);
    }

    // Optional: Draw a line in the editor to show the enemy's path to player
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
}