using System.Collections.Generic;
using UnityEngine;

/* 
 * RÉSUMÉ : Contrôle l'adversaire (mouvement fluide sans collision physique, avec système de voies).
 */
public class Enemy : MonoBehaviour, IStatusReceiver
{
    [Header("Déplacement")]
    public float speed = 3f;
    public float maxOffset = 0.3f; 
    
    [Header("Combat")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    
    private Vector3 personalOffset;

    void Awake()
    {
        currentHealth = maxHealth;

        // Génération du décalage
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
            // Il apparait sur le waypoint + son décalage
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
            // Quaternion.Slerp permet à l'ennemi de tourner sur lui-même en douceur dans les virages
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime); 
        }
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
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
        currentHealth -= payload.rawDamage;
        if (currentHealth <= 0) Die(); 
    }

    private void Die()
    {
        Destroy(gameObject); 
    }
}