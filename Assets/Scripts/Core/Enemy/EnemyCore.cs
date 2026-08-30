using System;
using UnityEngine;

[RequireComponent(typeof(EnemyStatusManager))]
public class EnemyCore : MonoBehaviour, IStatusReceiver
{
    [Header("Données")]
    public EnemyData data; 

    public Stat MaxHealth { get; private set; }
    public Stat Armor { get; private set; }
    public Stat Speed { get; private set; }

    private float currentHealth;
    private bool isDead = false;
    private EnemyStatusManager statusManager;
    
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; 

    void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyCore] Aucune EnemyData assignée sur {gameObject.name} !");
            return;
        }

        MaxHealth = new Stat(data.baseHealth);
        Armor = new Stat(data.baseArmor);
        Speed = new Stat(data.baseSpeed);
        
        currentHealth = MaxHealth.Value;
        statusManager = GetComponent<EnemyStatusManager>();
    }

    void Start()
    {
        GameEvents.TriggerEnemySpawned(this); // On s'envoie nous-même !
    }

    void OnDestroy()
    {
        GameEvents.TriggerEnemyRemoved(this);
    }

    public void ReceivePayload(HitPayload payload)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, payload.rawDamage - Armor.Value);
        currentHealth -= finalDamage;
        
        // Visuel dégat
        Vector3 popupPos = transform.position + new Vector3(0, 0.2f, 0); // Légèrement au-dessus de la tête
        GameEvents.TriggerDamagePopup(popupPos, finalDamage, payload.element, payload.isCritical);

        OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);

        if (payload.effectsToApply != null && statusManager != null)
        {
            foreach (StatusEffect effect in payload.effectsToApply)
            {
                statusManager.ApplyStatus(effect);
            }
        }

        if (currentHealth <= 0) 
        {
            isDead = true;
            Die(payload); 
        }
    }

    private void Die(HitPayload fatalPayload)
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        OnDeath?.Invoke();

        if (fatalPayload.instigator != null)
        {
            BasicTower tower = fatalPayload.instigator.GetComponent<BasicTower>();
            if (tower != null) 
            {
                tower.NotifyKill(this.gameObject);
            }
        }
        
        Destroy(gameObject); 
    }

    public void ReachGoal()
    {
        if (isDead) return;
        isDead = true;
        
        int damageToDeal = (data != null) ? data.coreDamage : 1;
        GameEvents.TriggerBaseDamaged(damageToDeal);
        
        Destroy(gameObject);
    }
}