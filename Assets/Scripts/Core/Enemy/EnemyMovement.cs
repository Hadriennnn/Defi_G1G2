using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyCore))]
public class EnemyMovement : MonoBehaviour
{
    public float maxOffset = 0.3f; 
    
    private EnemyCore core;
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private Vector3 personalOffset;

    void Awake()
    {
        core = GetComponent<EnemyCore>();

        float randomX = Random.Range(-maxOffset, maxOffset);
        float randomZ = Random.Range(-maxOffset, maxOffset);
        personalOffset = new Vector3(randomX, 0f, randomZ); 
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            waypoints = LevelManager.Instance.Waypoints; 
        }

        if (waypoints != null && waypoints.Count > 0)
        {
            transform.position = waypoints[0] + personalOffset; 
        }
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
        
        // On lit la vitesse dynamiquement depuis le Core (qui peut être modifiée par des trucs)
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, core.Speed.Value * Time.deltaTime);
        
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.z);

        if (Vector2.Distance(currentPos2D, targetPos2D) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                core.ReachGoal();
            }
        }
    }
}