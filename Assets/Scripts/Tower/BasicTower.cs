using UnityEngine;

/* 
 * RÉSUMÉ : Comportement autonome de la tour de défense qui scanne son environnement et tire des projectiles selon une certaine cadence.
 * 
 * MÉTHODES PRINCIPALES :
 * - Update() : Désactivée en phase de préparation. En action, elle gère le délai (fireCountdown) et ordonne les tirs.
 * - UpdateTarget() : Cherche tous les ennemis via "Physics2D.OverlapCircleAll" et verrouille le plus proche du centre de la tour.
 * - Shoot() : Crée le "colis" de dégâts et instancie un Projectile qu'elle associe à l'ennemi verrouillé.
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
        // 1. CORRECTION : Ajout des accolades pour englober le return !
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
        // 2. CORRECTION : Utilisation de Physics2D et Collider2D
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemyCollider.transform.position);
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
        Gizmos.DrawWireSphere(transform.position, range);
    }
}