using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public LayerMask enemyLayer; 

    private Transform targetTransform;
    private HitPayload myPayload;
    
    private bool isHoming;
    private float arcHeight;
    private GameObject explosionPrefab;
    
    private Vector3 startPos;
    private Vector3 fixedTargetPos;
    private float journeyTime; 
    private float elapsedTime; 

    // Reçoit les ordres de la tour
    public void Fire(Transform newTarget, HitPayload payload, bool homing, float arc, GameObject vfxPrefab)
    {
        targetTransform = newTarget;
        myPayload = payload;
        isHoming = homing;
        arcHeight = arc;
        explosionPrefab = vfxPrefab;
        
        startPos = transform.position;
        elapsedTime = 0f;

        // Si la cible est un point fixe 
        if (!isHoming && targetTransform != null)
        {
            fixedTargetPos = targetTransform.position;
            fixedTargetPos.y += 0.3f; // Vise le corps, pas les pieds
        }

        float distance = Vector3.Distance(startPos, isHoming ? targetTransform.position : fixedTargetPos);
        journeyTime = distance / speed;
    }

    void Update()
    {
        if (isHoming && targetTransform == null)
        {
            RetournerAuPool(); 
            return;
        }

        //TIR DROIT (Gatling / Sniper / Baliste)
        if (isHoming && arcHeight <= 0f)
        {
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            float distanceThisFrame = speed * Time.deltaTime;
            
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanceThisFrame, enemyLayer))
            {
                TriggerImpact(hit.point, hit.collider.gameObject);
                return;
            }

            transform.Translate(direction * distanceThisFrame, Space.World);
            transform.rotation = Quaternion.LookRotation(direction);
        }
        //TIR EN CLOCHE (Mortier) 
        else
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / journeyTime; 

            if (percent >= 1f)
            {
                Vector3 impactPoint = isHoming ? targetTransform.position : fixedTargetPos;
                TriggerImpact(impactPoint, null); 
                return;
            }

            Vector3 targetPos = isHoming ? targetTransform.position : fixedTargetPos;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);

            // Interpolation sinusoïdale pour la hauteur de la cloche
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * arcHeight;

            Vector3 dir = (currentPos - transform.position).normalized;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

            transform.position = currentPos;
        }
    }

    // Gère l'impact final pour tous les modes
    private void TriggerImpact(Vector3 impactPoint, GameObject directHitObj)
    {
        //Prévenient la tour (pour les effets des gemmes)
        if (myPayload.instigator != null && directHitObj != null)
        {
            BasicTower tower = myPayload.instigator.GetComponent<BasicTower>();
            if (tower != null) tower.NotifyHit(ref myPayload, directHitObj);
        }

        //AoE
        if (myPayload.aoeRadius > 0f)
        {
            if (explosionPrefab != null)
            {
                GameObject vfx = PoolManager.Instance.Spawn(explosionPrefab, impactPoint, Quaternion.identity);
                ExplosionVFX vfxScript = vfx.GetComponent<ExplosionVFX>();
                if (vfxScript != null) vfxScript.Setup(myPayload.aoeRadius);
            }

            Collider[] hits = Physics.OverlapSphere(impactPoint, myPayload.aoeRadius, enemyLayer);
            foreach (Collider c in hits)
            {
                IStatusReceiver receiver = c.GetComponent<IStatusReceiver>();
                if (receiver != null) receiver.ReceivePayload(myPayload);
            }
        }
        // ou DÉGÂTS MONOCIBLES
        else if (directHitObj != null)
        {
            IStatusReceiver receiver = directHitObj.GetComponent<IStatusReceiver>();
            if (receiver != null) receiver.ReceivePayload(myPayload);
        }

        RetournerAuPool();
    }
    
    private void RetournerAuPool()
    {
        if (TryGetComponent<PooledItem>(out PooledItem pooledItem)) pooledItem.ReleaseToPool();
        else Destroy(gameObject); 
    }
}