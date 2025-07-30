using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MeleeAttack : MonoBehaviour, EnemyBehaviour
{
    [SerializeField] private Animator anim;

    public void PerformAttack(float damage, Transform target)
    {
        // TODO: add the enemy attack animation

        Vector3 spawnPos = transform.position + transform.forward * 1.2f + Vector3.up * 1f;
        GameObject hitbox = new GameObject("EnemyHitbox");
        hitbox.transform.SetParent(transform);
        hitbox.tag = "EnemyHitbox";

        hitbox.transform.position = spawnPos;
        hitbox.transform.rotation = transform.rotation;

        var collider = hitbox.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(1.2f, 1.5f, 1.2f);

        var rb = hitbox.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Destroy(hitbox, 1f);
    }
}
