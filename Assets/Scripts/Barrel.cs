using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 1f;
    public float explosionForce = 300f;
    public float damage = 30f;
    public GameObject explosionEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyHitbox") || collision.gameObject.CompareTag("Bullet"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Optional: instantiate explosion VFX
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Detect all colliders within radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.gameObject.CompareTag("Player"))
            {
                Health playerHealth = nearbyObject.GetComponent<Health>();
                playerHealth.TakeDamage(damage);
            }
            if (nearbyObject.gameObject.CompareTag("Enemy"))
            {
                Enemy e = nearbyObject.GetComponent<Enemy>();
                e.TakeDamage(damage);
            }

            // Apply explosion force (if Rigidbody present)
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Destroy the barrel
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize explosion radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
