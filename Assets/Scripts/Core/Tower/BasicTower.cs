using System;
using UnityEngine;

public class BasicTower : MonoBehaviour
{
    [Header("Paramètres de Tir de Base")]
    [SerializeField] private float baseRange = 5f;
    [SerializeField] private float baseFireRate = 1f; 
    [SerializeField] private float initialDamage = 25f;

    public Stat Range;
    public Stat FireRate;
    public Stat Damage;

    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);

    [Header("Références (Logique)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask enemyLayer; 
    
    [Header("Optimisation")]
    [SerializeField] private int maxEnemiesEvaluated = 20;

    //La tour est éteinte par défaut 
    public bool hasCore = false;

    private float fireCountdown = 0f;
    private Transform currentTarget;
    private Collider[] enemyHitBuffer;

    public delegate void BeforeShootHandler(ref HitPayload payload);
    public event BeforeShootHandler OnBeforeShoot; 
    
    public event Action OnShoot; 
    public event Action<GameObject> OnTargetKilled;

    void Awake()
    {
        Range = new Stat(baseRange);
        FireRate = new Stat(baseFireRate);
        Damage = new Stat(initialDamage);
        enemyHitBuffer = new Collider[maxEnemiesEvaluated];
    }

    void Update()
    {
        //On ne fait rien si la tour n'a pas de Noyau
        if (!hasCore) return;

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
        int hits = Physics.OverlapSphereNonAlloc(trueCenter, Range.Value, enemyHitBuffer, enemyLayer);
        
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

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
            effectsToApply = null,
            isCritical = false
        };
        
        OnBeforeShoot?.Invoke(ref payload);
        
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
        // Si elle est éteinte, on renvoie null pour que la tourelle (TurretRotatorY) s'arrête de tourner
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