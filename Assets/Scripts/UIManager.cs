using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Image oxygenImage;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        healthImage.fillAmount = 1;
        oxygenImage.fillAmount = 1;
    }

    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        healthImage.fillAmount = currentHealth / maxHealth;
    }

    public void UpdateOxygenUI(float currentO2)
    {
        oxygenImage.fillAmount = currentO2 / 180f;
    }
}
