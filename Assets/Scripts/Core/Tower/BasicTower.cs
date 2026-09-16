using System;
using UnityEngine;

public class BasicTower : MonoBehaviour
{
    [Header("Paramètres de Tir de Base")]
    [SerializeField] private float baseRange = 5f;
    [SerializeField] private float baseFireRate = 1f; 
    [SerializeField] private float initialDamage = 25f;
    [SerializeField] private float baseAoERadius = 0f; 

    public Stat Range;
    public Stat FireRate;
    public Stat Damage;
    public Stat AoERadius; 

    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);

    [Header("Balistique & VFX")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    
    [Tooltip("Si décoché, le tir vise un point fixe au sol (esquivable).")]
    public bool isHoming = true;
    [Tooltip("Hauteur de la courbe en cloche. 0 = Tir droit.")]
    public float arcHeight = 0f;
    [Tooltip("Le visuel qui apparait à l'impact (si AoE > 0)")]
    public GameObject explosionPrefab;

    [Header("Optimisation")]
    public LayerMask enemyLayer; 
    [SerializeField] private int maxEnemiesEvaluated = 20;

    public bool hasCore = false;

    private float fireCountdown = 0f;
    private Transform currentTarget;
    public Collider[] enemyHitBuffer;

    public delegate void HitPayloadModifier(ref HitPayload payload);
    public event HitPayloadModifier OnBeforeShoot; 
    
    public delegate void OnHitHandler(ref HitPayload payload, GameObject victim);
    public event OnHitHandler OnHit;
    
    public event Action OnShoot; 
    public event Action<GameObject> OnTargetKilled;

    public delegate Transform TargetingStrategy(Vector3 trueCenter, float range, Collider[] buffer, LayerMask layer);
    public TargetingStrategy targetingLogic;

    void Awake()
    {
        Range = new Stat(baseRange);
        FireRate = new Stat(baseFireRate);
        Damage = new Stat(initialDamage);
        AoERadius = new Stat(baseAoERadius); 
        
        enemyHitBuffer = new Collider[maxEnemiesEvaluated];
        targetingLogic = DefaultNearestTargeting;
    }

    void Update()
    {
        if (!hasCore) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.PreparationPhase) return;

        fireCountdown -= Time.deltaTime;
        
        Vector3 trueCenter = transform.position + centerOffset;
        currentTarget = targetingLogic?.Invoke(trueCenter, Range.Value, enemyHitBuffer, enemyLayer);

        if (currentTarget != null && fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / FireRate.Value; 
        }
    }

    private Transform DefaultNearestTargeting(Vector3 trueCenter, float range, Collider[] buffer, LayerMask layer)
    {
        int hits = Physics.OverlapSphereNonAlloc(trueCenter, range, buffer, layer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        for (int i = 0; i < hits; i++)
        {
            float distanceToEnemy = Vector3.Distance(trueCenter, buffer[i].transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = buffer[i].transform;
            }
        }
        return nearestEnemy;
    }

    void Shoot()
    {
        HitPayload payload = new HitPayload 
        { 
            rawDamage = Damage.Value,
            element = ElementType.Normal,
            instigator = this.gameObject,
            effectsToApply = null,
            isCritical = false,
            aoeRadius = AoERadius.Value 
        };
        
        OnBeforeShoot?.Invoke(ref payload);
        
        GameObject projGO = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projGO.GetComponent<Projectile>();
        
        // La tour passe toute la configuration visuelle et physique au projectile
        if (projectile != null) 
        {
            projectile.Fire(currentTarget, payload, isHoming, arcHeight, explosionPrefab);
        }

        OnShoot?.Invoke();
    }

    public void NotifyHit(ref HitPayload finalPayload, GameObject victim)
    {
        OnHit?.Invoke(ref finalPayload, victim);
    }

    public void NotifyKill(GameObject victim)
    {
        OnTargetKilled?.Invoke(victim);
    }

    public Transform GetCurrentTarget()
    {
        if (!hasCore) return null;
        return currentTarget;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 trueCenter = transform.position + centerOffset;
        Gizmos.DrawWireSphere(trueCenter, Range != null ? Range.baseValue : baseRange);
    }
}