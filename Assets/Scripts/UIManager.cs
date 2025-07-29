using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Image oxygenImage;
    [SerializeField] private TextMeshProUGUI ammoText;

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

    public void UpdateAmmoUI(int magAmmo, int resAmmo)
    {
        ammoText.SetText(magAmmo + " / " + resAmmo);
    }
    
    public void ShowReloading()
    {
        ammoText.SetText("Realoding...");
    }
}
