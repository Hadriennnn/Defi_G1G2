using UnityEngine;
using UnityEngine.InputSystem;

/*
 * RÉSUMÉ : Caméra RTS Libre. 
 * Déplacement : ZQSD / Flèches (Relatif à l'écran) ou Clic Molette.
 * Rotation : Clic Droit (360°).
 * Zoom : Molette.
 */
public class CameraController : MonoBehaviour
{
    [Header("Références")]
    public Transform camTransform;
    private Camera cam; 

    [Header("Déplacement (Clavier & Molette)")]
    public float keyboardPanSpeed = 20f;
    private Vector3 panDragStartPos;
    private bool isPanning = false;

    [Header("Rotation (Clic Droit)")]
    public float rotationSpeed = 0.2f;
    private float currentRotationY = 0f;

    [Header("Zoom (Molette)")]
    public float zoomSpeed = 5f; 
    [Tooltip("Distance minimum (Zoom max)")]
    public float minDistance = 10f; 
    [Tooltip("Distance maximum (Dézoom max)")]
    public float maxDistance = 60f; 
    private float currentDistance;

    private bool isInitialized = false;

    public void Init()
    {
        currentRotationY = transform.eulerAngles.y;

        if (camTransform != null)
        {
            cam = camTransform.GetComponent<Camera>();
            currentDistance = camTransform.localPosition.magnitude;
        }
    }

    void Update()
    {
        // Auto-centrage au démarrage
        if (!isInitialized && ChunkManager.Instance != null && ChunkManager.Instance.baseNoyauCoords != Vector2Int.zero)
        {
            CentrerSurLaBase();
        }

        if (Mouse.current == null || Keyboard.current == null || camTransform == null || cam == null || !isInitialized) return;

        GererDeplacementClavier();
        GererDeplacementSouris();
        GererRotation();
        GererZoom(); 
    }

    private void CentrerSurLaBase()
    {
        GridData grid = FindObjectOfType<GridData>();
        float cellSize = grid != null ? grid.cellSize : 1f;
        Vector3 origineGrille = grid != null ? grid.transform.position : Vector3.zero;

        Vector2Int baseCoords = ChunkManager.Instance.baseNoyauCoords;
        
        float worldX = origineGrille.x + (baseCoords.x * cellSize) + (cellSize / 2f);
        float worldZ = origineGrille.z + (baseCoords.y * cellSize) + (cellSize / 2f);

        transform.position = new Vector3(worldX, transform.position.y, worldZ);
        isInitialized = true;
    }

    void GererDeplacementClavier()
    {
        float moveX = 0f;
        float moveZ = 0f;

        // Support ZQSD et Flèches
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

        if (moveX != 0 || moveZ != 0)
        {
            // CORRECTION : Déplacement relatif à l'ÉCRAN (la caméra)
            Vector3 forward = camTransform.forward;
            Vector3 right = camTransform.right;
            
            // On annule l'axe Y pour que la caméra reste bien plate sur le sol (pas de vol)
            forward.y = 0f;
            right.y = 0f;
            
            forward.Normalize();
            right.Normalize();

            // On combine les directions
            Vector3 moveDir = (forward * moveZ + right * moveX).normalized;
            transform.position += moveDir * keyboardPanSpeed * Time.deltaTime;
        }
    }

    void GererDeplacementSouris()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); 
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                panDragStartPos = ray.GetPoint(distance);
                isPanning = true;
            }
        }

        if (Mouse.current.middleButton.isPressed && isPanning)
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 currentMouseWorldPos = ray.GetPoint(distance);
                Vector3 difference = panDragStartPos - currentMouseWorldPos;
                
                transform.position += difference;
            }
        }

        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isPanning = false;
        }
    }

    void GererRotation()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            float mouseDeltaX = Mouse.current.delta.x.ReadValue();
            currentRotationY += mouseDeltaX * rotationSpeed;
            
            transform.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
        }
    }

    void GererZoom()
    {
        float scroll = Mouse.current.scroll.y.ReadValue();

        if (Mathf.Abs(scroll) > 0.1f)
        {
            currentDistance -= scroll * zoomSpeed * 0.01f;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            
            camTransform.localPosition = camTransform.localPosition.normalized * currentDistance;
        }
    }
}