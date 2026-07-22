using UnityEngine;

/* 
 * RÉSUMÉ : Gère le déplacement physique du projectile vers sa cible et lui inflige des dégâts (via IStatusReceiver) au moment de l'impact.
 * 
 * MÉTHODES PRINCIPALES :
 * - Fire() : Injecte la cible à suivre et le colis de dégâts (HitPayload) dans le projectile au moment du tir.
 * - Update() : Déplace le projectile, utilise un Raycast2D pour détecter la collision, appelle ReceivePayload() sur la cible puis se détruit.
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

        Vector3 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        // CORRECTION : On utilise Physics2D.Raycast qui renvoie un RaycastHit2D
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distanceThisFrame, enemyLayer);

        // En 2D, si hit.collider n'est pas null, c'est qu'on a touché quelque chose
        if (hit.collider != null)
        {
            IStatusReceiver receiver = hit.collider.GetComponent<IStatusReceiver>();
            if (receiver != null)
            {
                receiver.ReceivePayload(myPayload);
            }

            Destroy(gameObject); // Destruction du projectile après livraison
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);
    }
}