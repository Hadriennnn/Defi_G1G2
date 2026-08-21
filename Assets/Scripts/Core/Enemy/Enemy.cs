using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour, IStatusReceiver
{
    [Header("Déplacement")]
    // La vitesse devient une Stat dynamique les ralentissements
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

    void Awake()
    {
        currentHealth = maxHealth.Value;
        
        // On lie le gestionnaire de statuts
        statusManager = GetComponent<EnemyStatusManager>();

        // Génération du décalage pour éviter la file indienne
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
        if (waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
            return;

        Vector3 targetPosition = waypoints[currentWaypointIndex] + personalOffset;
        targetPosition.y = transform.position.y;
        
        Vector3 direction = targetPosition - transform.position;
        
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime); 
        }
        
        // On utilise speed.Value pour appliquer les potentiels ralentissements
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
        Destroy(gameObject); 
    }
    
    public void ReceivePayload(HitPayload payload)
    {
        // Calcul des dégâts avec armure fixe
        float finalDamage = Mathf.Max(1f, payload.rawDamage - armor.Value);
        
        currentHealth -= finalDamage;

        // Application des altérations d'état
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
            Die(payload); 
        }
    }

    private void Die(HitPayload fatalPayload)
    {
        if (fatalPayload != null && fatalPayload.instigator != null)
        {
            // On prévient la tour qu'elle a tué
            BasicTower tower = fatalPayload.instigator.GetComponent<BasicTower>();
            if (tower != null) 
            {
                tower.NotifyKill(this.gameObject);
            }
        }
        
        Destroy(gameObject); 
    }
}