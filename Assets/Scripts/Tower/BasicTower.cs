using UnityEngine;

/* 
 * RÉSUMÉ : Le Cerveau de la tour. Il gère uniquement la détection (mathématique) et le déclenchement des tirs.
 */
public class BasicTower : MonoBehaviour
{
    [Header("Paramètres de Tir")]
    public float range = 5f;
    public float fireRate = 1f; 
    public float baseDamage = 25f;
    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);

    [Header("Références (Logique)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask enemyLayer; 

    private float fireCountdown = 0f;
    private Transform currentTarget;

    void Update()
    {
        // On ne fait rien si on est en phase de préparation
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.PreparationPhase) return;

        fireCountdown -= Time.deltaTime;
        
        // Le cerveau cherche sa cible
        UpdateTarget();

        // S'il a une cible et que le canon est prêt, il tire
        if (currentTarget != null && fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
    }

    void UpdateTarget()
    {
        Vector3 trueCenter = transform.position + centerOffset;
        Collider[] enemiesInRange = Physics.OverlapSphere(trueCenter, range, enemyLayer); // Utilisation native de la détection[cite: 1]
        
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            float distanceToEnemy = Vector3.Distance(trueCenter, enemyCollider.transform.position);
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
        HitPayload payload = new HitPayload { rawDamage = baseDamage }; // Le contrat de dégâts reste intact[cite: 1]
        
        // On utilise notre PoolManager
        GameObject projGO = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projGO.GetComponent<Projectile>();
        
        if (projectile != null) projectile.Fire(currentTarget, payload);
    }

    // NOUVEAU : Le moyen de communication pour les autres scripts (les "Muscles")
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 trueCenter = transform.position + centerOffset;
        Gizmos.DrawWireSphere(trueCenter, range);
    }
}