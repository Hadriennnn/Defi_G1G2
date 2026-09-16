using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public LayerMask enemyLayer; 

    private Transform target;
    private HitPayload myPayload;

    public void Fire(Transform newTarget, HitPayload payload)
    {
        target = newTarget;
        myPayload = payload;
    }

    void Update()
    {
        if (target == null)
        {
            RetournerAuPool(); 
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;
        
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanceThisFrame, enemyLayer))
        {
            // 1. On prévient la tour (Les gemmes peuvent encore modifier l'AoE ici !)
            if (myPayload.instigator != null)
            {
                BasicTower tower = myPayload.instigator.GetComponent<BasicTower>();
                if (tower != null) 
                {
                    tower.NotifyHit(ref myPayload, hit.collider.gameObject);
                }
            }

            // 2. GESTION DE L'IMPACT (Monocible vs AoE)
            if (myPayload.aoeRadius > 0f)
            {
                // EXPLOSION ! On récupère tous les ennemis dans le rayon
                Collider[] hits = Physics.OverlapSphere(hit.point, myPayload.aoeRadius, enemyLayer);
                foreach (Collider c in hits)
                {
                    IStatusReceiver receiver = c.GetComponent<IStatusReceiver>();
                    if (receiver != null) receiver.ReceivePayload(myPayload);
                }
                
                // Petit retour visuel basique (optionnel, pour tes tests)
                Debug.Log($"[Projectile] Explosion AoE ! Rayon: {myPayload.aoeRadius}, Touchés: {hits.Length}");
            }
            else
            {
                // TIR NORMAL (Monocible)
                IStatusReceiver receiver = hit.collider.GetComponent<IStatusReceiver>();
                if (receiver != null) receiver.ReceivePayload(myPayload);
            }

            RetournerAuPool();
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private void RetournerAuPool()
    {
        if (TryGetComponent<PooledItem>(out PooledItem pooledItem)) pooledItem.ReleaseToPool();
        else Destroy(gameObject); 
    }
}