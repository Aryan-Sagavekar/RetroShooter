using System.IO.Pipes;
using UnityEngine;

public class RangedAttack : MonoBehaviour, EnemyBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    public void PerformAttack(float damage, Transform target)
    {
        // TODO: add the enemy attack animation
        //if (anim != null)
        //    anim.SetTrigger("Attack");

        if (projectilePrefab == null || target == null)
            return;

        // Decide where to spawn from
        Transform spawnOrigin = firePoint != null ? firePoint : transform;

        //Vector3 spawnPos = spawnOrigin.position + spawnOrigin.forward * 1.2f + Vector3.up * 0.5f;
        //Vector3 direction = (target.position + Vector3.up * 0.5f - firePoint.position).normalized;
        Vector3 direction = firePoint.forward;

        GameObject bulletGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        bulletGO.tag = "EnemyHitbox";
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.Shooter = gameObject;

        if (bullet != null)
        {
            bullet.Initialize(direction);
        }
        else
            Debug.LogError("Projectile prefab does not have a Bullet script attached!");
    }
}
