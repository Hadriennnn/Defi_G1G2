using System;
using UnityEngine;

[RequireComponent(typeof(EnemyStatusManager))]

// Gère la vie et la partie combat de l'ennemi
public class EnemyCore : MonoBehaviour, IStatusReceiver
{
    [Header("Données")]
    public EnemyData data; 

    // Les stats dynamiques (accessibles par les autres composants)
    public Stat MaxHealth { get; private set; }
    public Stat Armor { get; private set; }
    public Stat Speed { get; private set; }

    private float currentHealth;
    private bool isDead = false;
    private EnemyStatusManager statusManager;
    
    public event Action OnDeath;

    void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyCore] Aucune EnemyData assignée sur {gameObject.name} !");
            return;
        }

        // Initialisation 
        MaxHealth = new Stat(data.baseHealth);
        Armor = new Stat(data.baseArmor);
        Speed = new Stat(data.baseSpeed);
        
        currentHealth = MaxHealth.Value;
        statusManager = GetComponent<EnemyStatusManager>();
    }

    void Start()
    {
        GameEvents.TriggerEnemySpawned();
    }

    void OnDestroy()
    {
        GameEvents.TriggerEnemyRemoved();
    }

    public void ReceivePayload(HitPayload payload)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, payload.rawDamage - Armor.Value);
        currentHealth -= finalDamage;

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

        if (fatalPayload != null && fatalPayload.instigator != null)
        {
            BasicTower tower = fatalPayload.instigator.GetComponent<BasicTower>();
            if (tower != null) 
            {
                tower.NotifyKill(this.gameObject);
            }
        }
        
        Destroy(gameObject); 
    }

    // Méthode appelée par le script de mouvement quand la fin du chemin est atteinte
    public void ReachGoal()
    {
        if (isDead) return;
        isDead = true;
        
        // Plus tard : GameManager.Instance.LoseLife(1);
        Destroy(gameObject);
    }
}