using UnityEngine;

/* 
 * RÉSUMÉ : Comportement autonome de la tour de défense qui scanne son environnement 3D et tire des projectiles.
 */
public class BasicTower : MonoBehaviour
{
    [Header("Paramètres de Tir")]
    public float range = 5f;
    public float fireRate = 1f; // Tirs par seconde
    public float baseDamage = 25f;

    [Header("Références")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask enemyLayer; 

    private float fireCountdown = 0f;
    private Transform currentTarget;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.PreparationPhase)
        {
            return;
        }

        fireCountdown -= Time.deltaTime;

        UpdateTarget();

        if (currentTarget != null && fireCountdown <= 0f)
        {
            Shoot();
            Debug.Log("[Tower] Pew ! Je tire !");
            fireCountdown = 1f / fireRate;
        }
    }

    void UpdateTarget()
    {
        // CORRECTION 3D : On utilise Collider et Physics.OverlapSphere
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, enemyLayer);
        
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            // CORRECTION 3D : On utilise Vector3 pour calculer la distance
            float distanceToEnemy = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemyCollider.transform;
            }
        }

        currentTarget = nearestEnemy;
    }

    void Shoot()
    {
        HitPayload payload = new HitPayload { rawDamage = baseDamage };

        // On instancie le projectile au niveau du canon de la tour
        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projGO.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            projectile.Fire(currentTarget, payload);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // DrawWireSphere fonctionne parfaitement en 3D !
        Gizmos.DrawWireSphere(transform.position, range);
    }
}