using UnityEngine;


[RequireComponent(typeof(BasicTower))]
public class TurretRotatorY : MonoBehaviour
{
    [Header("Paramètres Visuels")]
    public float turnSpeed = 10f;
    public Transform turretHead;

    [Header("Correction Blender/Modèle 3D")]
    [Tooltip("Si le canon est de travers, mets 90, -90 ou 180 ici.")]
    public float yRotationOffset = 0f; 

    private BasicTower towerBrain;

    void Awake()
    {
        towerBrain = GetComponent<BasicTower>();
    }

    void Update()
    {
        Transform target = towerBrain.GetCurrentTarget();

        if (target != null && turretHead != null)
        {
            Vector3 direction = target.position - turretHead.position;
            direction.y = 0f; 

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                float targetAngleY = lookRotation.eulerAngles.y;
                
                float finalTargetAngleY = targetAngleY + yRotationOffset;

                float currentAngleY = turretHead.eulerAngles.y;
                
                float smoothedY = Mathf.LerpAngle(currentAngleY, finalTargetAngleY, Time.deltaTime * turnSpeed);
                
                turretHead.eulerAngles = new Vector3(turretHead.eulerAngles.x, smoothedY, turretHead.eulerAngles.z);
            }
        }
    }
}