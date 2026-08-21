using System;
using System.Collections.Generic;
using UnityEngine;


public class BasicTower : MonoBehaviour
{
    [Header("Paramètres de Tir de Base")]
    [SerializeField] private float baseRange = 5f;
    [SerializeField] private float baseFireRate = 1f; 
    [SerializeField] private float initialDamage = 25f;

    // Nos Stats dynamiques de la Phase 1
    public Stat Range;
    public Stat FireRate;
    public Stat Damage;

    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);

    [Header("Références (Logique)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask enemyLayer; 

    private float fireCountdown = 0f;
    private Transform currentTarget;

    
    // Se déclenche juste avant le tir. Permet à la gemme de rajouter les effets.
    public event Action<HitPayload> OnBeforeShoot; 
    
    // Se déclenche au moment précis où le coup part (jvais mettre effet visu etc .
    public event Action OnShoot; 
    
    // Se déclenche si le monstre meurt des suites de cette attaque pour tout ce qui est gemme onkill.
    public event Action<GameObject> OnTargetKilled;

    void Awake()
    {
        Range = new Stat(baseRange);
        FireRate = new Stat(baseFireRate);
        Damage = new Stat(initialDamage);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.PreparationPhase) return;

        fireCountdown -= Time.deltaTime;
        
        UpdateTarget();

        if (currentTarget != null && fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / FireRate.Value; 
        }
    }

    void UpdateTarget()
    {
        Vector3 trueCenter = transform.position + centerOffset;
        Collider[] enemiesInRange = Physics.OverlapSphere(trueCenter, Range.Value, enemyLayer);
        
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
        //On crée le colis avec les valeurs de base
        HitPayload payload = new HitPayload 
        { 
            rawDamage = Damage.Value,
            element = ElementType.Normal,
            instigator = this.gameObject,
            effectsToApply = new List<StatusEffect>()
        };
        
        // Les gemmes vont modifier le colis avec ce qu'elles ont dedans
        OnBeforeShoot?.Invoke(payload);
        
        //On lance le proj avec le colis
        GameObject projGO = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projGO.GetComponent<Projectile>();
        
        if (projectile != null) projectile.Fire(currentTarget, payload);

        //On notifie que le tir est parti
        OnShoot?.Invoke();
    }

    //L'ennemi appellera cette méthode quand il mourra
    public void NotifyKill(GameObject victim)
    {
        OnTargetKilled?.Invoke(victim);
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    // Pour voir la range dans le mode éditeur, je ferai un truc plus propre et visible in game après 
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 trueCenter = transform.position + centerOffset;
        Gizmos.DrawWireSphere(trueCenter, Range != null ? Range.baseValue : baseRange);
    }
}