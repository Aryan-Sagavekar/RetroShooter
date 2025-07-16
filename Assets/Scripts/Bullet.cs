using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float damage = 2f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector3 direction)
    {
        // Set the projectile's velocity
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has the "Enemy" tag
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Try to get the Enemy component from the collided object
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Call the TakeDamage method on the enemy
            }
        // Destroy the projectile on any collision
            Destroy(gameObject);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        // We will add collision logic here later for hitting enemies.
        // For now, let's just destroy on collision with anything (except self/player if needed).
        // You might want to filter this later (e.g., not collide with another projectile).

        // Example: If it hits an object with the tag "Enemy", damage it.
        // if (other.CompareTag("Enemy"))
        // {
        //     // Damage enemy logic
        //     Debug.Log("Bullet hit enemy!");
        // }

        // Destroy the projectile on impact
        // Important: Add a check to prevent destroying itself if it's spawned inside something.
        // Or if you only want it to destroy on specific types of collisions.

        // For now, simple destroy on any collision
        Destroy(gameObject);
    }
}