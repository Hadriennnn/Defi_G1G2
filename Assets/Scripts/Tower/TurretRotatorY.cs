using UnityEngine;

/*
 * RÉSUMÉ : Le Muscle de rotation. Il regarde la cible du Cerveau et tourne uniquement sur l'axe Y (gauche/droite).
 */
[RequireComponent(typeof(BasicTower))]
public class TurretRotatorY : MonoBehaviour
{
    [Header("Paramètres Visuels")]
    public float turnSpeed = 10f;
    public Transform turretHead;

    private BasicTower towerBrain;

    void Awake()
    {
        // On récupère automatiquement le cerveau placé sur le même GameObject
        towerBrain = GetComponent<BasicTower>();
    }

    void Update()
    {
        // 1. On demande au cerveau sa cible actuelle
        Transform target = towerBrain.GetCurrentTarget();

        // 2. Si on a une cible, on exécute la rotation
        if (target != null && turretHead != null)
        {
            Vector3 direction = target.position - turretHead.position;
            direction.y = 0f; // On force la visée parallèle au sol

            if (direction != Vector3.zero)
            {
                // Calcul de la rotation
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                float targetAngleY = lookRotation.eulerAngles.y;
                float currentAngleY = turretHead.eulerAngles.y;
                
                // Rotation fluide
                float smoothedY = Mathf.LerpAngle(currentAngleY, targetAngleY, Time.deltaTime * turnSpeed);
                
                // Application de la rotation Y sans casser les axes X et Z d'origine de ton modèle
                turretHead.eulerAngles = new Vector3(turretHead.eulerAngles.x, smoothedY, turretHead.eulerAngles.z);
            }
        }
    }
}