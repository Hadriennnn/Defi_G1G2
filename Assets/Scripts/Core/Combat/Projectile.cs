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
            Destroy(gameObject);
            return;
        }

        // Le projectile vise le centre de la cible en 3D
        Vector3 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;
        
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanceThisFrame, enemyLayer))
        {
            // Si on touche un collider, on vérifie s'il a le contrat IStatusReceiver
            IStatusReceiver receiver = hit.collider.GetComponent<IStatusReceiver>();
            if (receiver != null)
            {
                receiver.ReceivePayload(myPayload);
            }

            Destroy(gameObject); // Destruction du projectile après livraison
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);
        
        // Faire pointer le projectile visuellement vers la cible
        transform.rotation = Quaternion.LookRotation(direction);
    }
}