using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth { get; private set; }

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        OnDeath?.Invoke();

        // Restart current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
