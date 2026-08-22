using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IStatusReceiver
{
    [Header("Déplacement")]
    public Stat speed = new Stat(3f); 
    public float maxOffset = 0.3f; 
    
    [Header("Combat")]
    public Stat maxHealth = new Stat(100f);
    public Stat armor = new Stat(5f);
    
    private float currentHealth;
    
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    
    private Vector3 personalOffset;
    private EnemyStatusManager statusManager;

    // Corrige le bug où quand 2 tours tuent la cible en même temps on a 2 kills comptabilisés
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth.Value;
        statusManager = GetComponent<EnemyStatusManager>();

        float randomX = Random.Range(-maxOffset, maxOffset);
        float randomZ = Random.Range(-maxOffset, maxOffset);
        personalOffset = new Vector3(randomX, 0f, randomZ); 
    }

    void Start()
    {
        GameEvents.TriggerEnemySpawned(); 

        if (LevelManager.Instance != null)
        {
            waypoints = LevelManager.Instance.Waypoints; 
        }

        if (waypoints != null && waypoints.Count > 0)
        {
            transform.position = waypoints[0] + personalOffset; 
        }
    }

    void OnDestroy()
    {
        GameEvents.TriggerEnemyRemoved();
    }
    
    void Update()
    {
        // Si l'ennemi est mort, on arrête de le faire avancer
        if (isDead || waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
            return;

        Vector3 targetPosition = waypoints[currentWaypointIndex] + personalOffset;
        targetPosition.y = transform.position.y;
        
        Vector3 direction = targetPosition - transform.position;
        
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime); 
        }
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed.Value * Time.deltaTime);
        
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.z);

        if (Vector2.Distance(currentPos2D, targetPos2D) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                AtteindreObjectif();
            }
        }
    }

    private void AtteindreObjectif()
    {
        if (isDead) return;
        isDead = true;
        
        // Ici tu pourras ajouter : GameManager.Instance.PerdreDesVies(1);
        Destroy(gameObject); 
    }
    
    public void ReceivePayload(HitPayload payload)
    {
        // Si l'ennemi est déjà mort on ignore le projectile
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, payload.rawDamage - armor.Value);
        currentHealth -= finalDamage;

        if (payload.effectsToApply != null && statusManager != null)
        {
            foreach (StatusEffect effect in payload.effectsToApply)
            {
                statusManager.ApplyStatus(effect);
            }
        }
        
        Debug.Log($"Dégâts reçus : {payload.rawDamage} | Élément : {payload.element}");

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
}