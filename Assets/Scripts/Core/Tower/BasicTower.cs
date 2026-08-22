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
    
    [Header("Optimisation")]
    [Tooltip("Nombre maximum d'ennemis que la tour peut évaluer en même temps. 20 est largement suffisant.")]
    [SerializeField] private int maxEnemiesEvaluated = 20;

    private float fireCountdown = 0f;
    private Transform currentTarget;
    
    // BUFFER PRÉ-ALLOUÉ : Évite la création de tableaux à chaque frame
    private Collider[] enemyHitBuffer;

    public event Action<HitPayload> OnBeforeShoot; 
    public event Action OnShoot; 
    public event Action<GameObject> OnTargetKilled;

    void Awake()
    {
        Range = new Stat(baseRange);
        FireRate = new Stat(baseFireRate);
        Damage = new Stat(initialDamage);
        
        // Initialisation du buffer une seule fois
        enemyHitBuffer = new Collider[maxEnemiesEvaluated];
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
        
        // NON-ALLOC : Remplit le tableau existant et retourne le nombre d'ennemis trouvés
        int hits = Physics.OverlapSphereNonAlloc(trueCenter, Range.Value, enemyHitBuffer, enemyLayer);
        
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        // On boucle uniquement sur le nombre de "hits" trouvés, pas sur tout le tableau
        for (int i = 0; i < hits; i++)
        {
            float distanceToEnemy = Vector3.Distance(trueCenter, enemyHitBuffer[i].transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemyHitBuffer[i].transform;
            }
        }
        
        currentTarget = nearestEnemy;
    }

    void Shoot()
    {
        HitPayload payload = new HitPayload 
        { 
            rawDamage = Damage.Value,
            element = ElementType.Normal,
            instigator = this.gameObject,
            effectsToApply = new List<StatusEffect>()
        };
        
        OnBeforeShoot?.Invoke(payload);
        
        GameObject projGO = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projGO.GetComponent<Projectile>();
        
        if (projectile != null) projectile.Fire(currentTarget, payload);

        OnShoot?.Invoke();
    }

    public void NotifyKill(GameObject victim)
    {
        OnTargetKilled?.Invoke(victim);
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 trueCenter = transform.position + centerOffset;
        Gizmos.DrawWireSphere(trueCenter, Range != null ? Range.baseValue : baseRange);
    }
}