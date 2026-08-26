using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public static BaseManager Instance { get; private set; }

    [Header("Paramètres de la Base")]
    public int maxHealth = 20;
    
    private int currentHealth;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        // On prévient l'UI au début
        GameEvents.TriggerBaseHealthChanged(currentHealth, maxHealth);
    }

    void OnEnable()
    {
        GameEvents.OnBaseDamaged += TakeDamage;
    }

    void OnDisable()
    {
        GameEvents.OnBaseDamaged -= TakeDamage;
    }

    private void TakeDamage(int damageAmount)
    {
        // Si on est déjà mort on ignore
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth); // On empêche de descendre sous 0

        Debug.Log($"[BaseManager] La base prend {damageAmount} dégâts ; PV restants : {currentHealth}/{maxHealth}");

        GameEvents.TriggerBaseHealthChanged(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("[BaseManager]  GAME OVER ! ");
            GameEvents.TriggerGameOver();
        }
    }
}