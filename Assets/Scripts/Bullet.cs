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
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
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
        //Destroy(gameObject);
    }
}