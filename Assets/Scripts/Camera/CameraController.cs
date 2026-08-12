using UnityEngine;
using UnityEngine.InputSystem;

/*
 * RÉSUMÉ : Contrôle le pivot de la caméra (Rotation, Zoom diagonal, et Déplacement 1:1 avec la souris).
 */
public class CameraController : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("La caméra enfant qui va subir le zoom")]
    public Transform camTransform;
    private Camera cam; // On met la caméra en cache pour les calculs de Raycast

    [Header("Rotation en Arc de Cercle (Clic Droit)")]
    public float rotationSpeed = 0.2f;
    public float minRotationY = -45f;
    public float maxRotationY = 45f;
    
    private float currentRotationY = 0f;

    [Header("Zoom Diagonale (Molette)")]
    public float zoomSpeed = 0.01f;
    public float minDistance = 5f; 
    public float maxDistance = 25f;  
    
    private float currentDistance;

    [Header("Déplacement (Clic Molette)")]
    [Tooltip("Distance maximum depuis le centre quand on est zoomé à fond")]
    public float maxPanRadius = 15f; 
    
    private Vector3 centerPosition; 
    
    // Variables pour le Drag & Drop 1:1
    private Vector3 panDragStartPos;
    private bool isPanning = false;

    void Start()
    {
        currentRotationY = transform.eulerAngles.y;
        centerPosition = transform.position;

        if (camTransform != null)
        {
            cam = camTransform.GetComponent<Camera>();
            currentDistance = camTransform.localPosition.magnitude;
        }
    }

    void Update()
    {
        if (Mouse.current == null || camTransform == null || cam == null) return;

        GererRotation();
        GererZoom();
        GererDeplacement(); 
    }

    void GererRotation()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            float mouseDeltaX = Mouse.current.delta.x.ReadValue();
            currentRotationY += mouseDeltaX * rotationSpeed;
            currentRotationY = Mathf.Clamp(currentRotationY, minRotationY, maxRotationY);
            
            transform.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
        }
    }

    void GererZoom()
    {
        float scroll = Mouse.current.scroll.y.ReadValue();

        if (Mathf.Abs(scroll) > 0.1f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            
            camTransform.localPosition = camTransform.localPosition.normalized * currentDistance;
            
            AppliquerLimitesDeplacement(); 
        }
    }

    void GererDeplacement()
    {
        // 1. Quand le joueur appuie sur le clic molette
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Un sol virtuel plat (Y=0)
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            // Si le laser touche le sol, on mémorise le point de départ
            if (groundPlane.Raycast(ray, out float distance))
            {
                panDragStartPos = ray.GetPoint(distance);
                isPanning = true;
            }
        }

        // 2. Tant que le joueur maintient le clic
        if (Mouse.current.middleButton.isPressed && isPanning)
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 currentMouseWorldPos = ray.GetPoint(distance);
                
                // On calcule la différence exacte entre là où on a cliqué et là où est la souris
                Vector3 difference = panDragStartPos - currentMouseWorldPos;
                
                // On applique cette différence au Pivot
                transform.position += difference;
                
                // On s'assure de ne pas sortir de la carte
                AppliquerLimitesDeplacement();
            }
        }

        // 3. Quand le joueur lâche le clic
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isPanning = false;
        }
    }

    void AppliquerLimitesDeplacement()
    {
        float zoomRatio = (currentDistance - minDistance) / (maxDistance - minDistance);
        float currentAllowedRadius = Mathf.Lerp(maxPanRadius, 0f, zoomRatio);

        Vector3 offset = transform.position - centerPosition;
        offset.y = 0f; 

        if (offset.magnitude > currentAllowedRadius)
        {
            Vector3 positionCorrigee = centerPosition + (offset.normalized * currentAllowedRadius);
            transform.position = new Vector3(positionCorrigee.x, transform.position.y, positionCorrigee.z);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? centerPosition : transform.position, maxPanRadius);
    }
}