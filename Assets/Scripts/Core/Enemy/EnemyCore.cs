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
        GameEvents.TriggerEnemySpawned();
    }

    void OnDestroy()
    {
        GameEvents.TriggerEnemyRemoved();
    }

    // Le payload est maintenant un struct, il est passé par valeur (copie)
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

        // CORRECTION ICI : fatalPayload étant un struct, il ne peut pas être null.
        // On vérifie directement si l'instigateur (le GameObject de la tour) existe bien.
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
        
        Destroy(gameObject);
    }
}