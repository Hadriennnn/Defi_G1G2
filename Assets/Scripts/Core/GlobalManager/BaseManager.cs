using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public static BaseManager Instance { get; private set; }

    [Header("Paramètres de la Base")]
    public int maxHealth = 20;
    
    private int currentHealth;

    // NOUVEAU : On permet aux autres scripts de lire la vie
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Le fameux Init
    public void Init()
    {
        currentHealth = maxHealth;
        GameEvents.TriggerBaseHealthChanged(currentHealth, maxHealth);
    }

    void OnEnable() { GameEvents.OnBaseDamaged += TakeDamage; }
    void OnDisable() { GameEvents.OnBaseDamaged -= TakeDamage; }

    private void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth); 

        Debug.Log($"[BaseManager] La base prend {damageAmount} dégâts ; PV restants : {currentHealth}/{maxHealth}");

        GameEvents.TriggerBaseHealthChanged(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("[BaseManager]  GAME OVER ! ");
            GameEvents.TriggerGameOver();
        }
    }
}