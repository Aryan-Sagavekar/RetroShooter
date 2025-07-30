using UnityEngine;

public interface EnemyBehaviour
{
    public abstract void PerformAttack(float damage, Transform target);
}
