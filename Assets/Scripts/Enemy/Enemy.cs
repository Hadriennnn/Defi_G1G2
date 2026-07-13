using System.Collections.Generic;
using UnityEngine;

// L'ennemi hérite de MonoBehaviour ET signe le contrat IStatusReceiver
public class Enemy : MonoBehaviour, IStatusReceiver
{
    [Header("Déplacement")]
    public float speed = 3f;
    
    [Header("Combat")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    // Variables existantes
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;

    void Awake()
    {
        // Initialisation des PV à l'apparition
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            waypoints = LevelManager.Instance.Waypoints;
        }

        if (waypoints != null && waypoints.Count > 0)
        {
            transform.position = waypoints[0];
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
            return;

        Vector3 targetPosition = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
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
        Debug.Log("[Enemy] J'ai atteint la base !");
        Destroy(gameObject); 
    }
    
    public void ReceivePayload(HitPayload payload)
    {
        currentHealth -= payload.rawDamage;
        Debug.Log($"[Enemy] Touché ! Dégâts: {payload.rawDamage} | PV restants: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Enemy] Mort de l'ennemi.");
        Destroy(gameObject);
    }
}