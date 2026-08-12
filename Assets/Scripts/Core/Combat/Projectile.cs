using UnityEngine;

/*
 * RÉSUMÉ : Gère le déplacement physique 3D du projectile vers sa cible.
 */
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
            RetournerAuPool(); // <--- REMPLACE Destroy(gameObject)
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;
        
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanceThisFrame, enemyLayer))
        {
            IStatusReceiver receiver = hit.collider.GetComponent<IStatusReceiver>();
            if (receiver != null)
            {
                receiver.ReceivePayload(myPayload);
            }

            RetournerAuPool(); // <--- REMPLACE Destroy(gameObject)
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    private void RetournerAuPool()
    {
        if (TryGetComponent<PooledItem>(out PooledItem pooledItem))
        {
            pooledItem.ReleaseToPool();
        }
        else
        {
            Destroy(gameObject); // Sécurité de repli
        }
    }
}