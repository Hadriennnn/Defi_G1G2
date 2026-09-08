using UnityEngine;

public class AutoTilingBarricade : MonoBehaviour
{
    [Header("Configuration des Prefabs (Modèles)")]
    public GameObject straightPrefab;
    public float offsetStraight = 0f;
    public GameObject cornerPrefab;
    public float offsetCorner = 0f;
    public GameObject tPrefab;
    public float offsetT = 0f;
    public GameObject crossPrefab;
    public float offsetCross = 0f;

    [Header("Conteneur Visuel")]
    [Tooltip("L'objet enfant vide où le modèle 3D sera instancié")]
    public Transform visualContainer;

    private Vector2Int myCoords;
    private GameObject currentVisual;
    private bool isInitialized = false;

    void OnEnable()
    {
        GameEvents.OnObstacleToggled += HandleObstacleChanged;
    }

    void OnDisable()
    {
        GameEvents.OnObstacleToggled -= HandleObstacleChanged;
    }

    // Appelée par la carte au moment où la barricade est posée
    public void Init(Vector2Int coords)
    {
        myCoords = coords;
        isInitialized = true;
        UpdateVisual();
    }

    private void HandleObstacleChanged(Vector3Int pos, bool isPlaced)
    {
        if (!isInitialized) return;

        // On calcule la distance de Manhattan 
        int manhattanDist = Mathf.Abs(pos.x - myCoords.x) + Mathf.Abs(pos.y - myCoords.y);
        
        // Si le bloc posé ou détruit est exactement sur une case adjacente, on se met à jour
        if (manhattanDist == 1)
        {
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        if (PlacementManager.Instance == null) return;

        int bitmask = 0;
        
        // On vérifie le Nord (0, 1)
        if (IsBarricadeAt(myCoords + new Vector2Int(0, 1))) bitmask += 1;
        // On vérifie l'Est (1, 0)
        if (IsBarricadeAt(myCoords + new Vector2Int(1, 0))) bitmask += 2;
        // On vérifie le Sud (0, -1)
        if (IsBarricadeAt(myCoords + new Vector2Int(0, -1))) bitmask += 4;
        // On vérifie l'Ouest (-1, 0)
        if (IsBarricadeAt(myCoords + new Vector2Int(-1, 0))) bitmask += 8;

        GameObject prefabToInstantiate = straightPrefab; 
        float rotationY = 0f;
        float currentOffset = 0f; 

        switch (bitmask)
        {
            // Droite (Ou isolé)
            case 0: case 1: case 4: case 5: 
                prefabToInstantiate = straightPrefab; rotationY = 90f; currentOffset = offsetStraight; break;
            case 2: case 8: case 10: 
                prefabToInstantiate = straightPrefab; rotationY = 0f; currentOffset = offsetStraight; break;
            
            // Angles
            case 3: prefabToInstantiate = cornerPrefab; rotationY = 0f; currentOffset = offsetCorner; break;
            case 6: prefabToInstantiate = cornerPrefab; rotationY = 90f; currentOffset = offsetCorner; break;
            case 12: prefabToInstantiate = cornerPrefab; rotationY = 180f; currentOffset = offsetCorner; break;
            case 9: prefabToInstantiate = cornerPrefab; rotationY = 270f; currentOffset = offsetCorner; break;
            
            // Forme en T
            case 7: prefabToInstantiate = tPrefab; rotationY = 0f; currentOffset = offsetT; break;
            case 14: prefabToInstantiate = tPrefab; rotationY = 90f; currentOffset = offsetT; break;
            case 13: prefabToInstantiate = tPrefab; rotationY = 180f; currentOffset = offsetT; break;
            case 11: prefabToInstantiate = tPrefab; rotationY = 270f; currentOffset = offsetT; break;
            
            // Croisement
            case 15: prefabToInstantiate = crossPrefab; rotationY = 0f; currentOffset = offsetCross; break;
        }

        // On détruit l'ancien modèle s'il y en avait un
        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        // On instancie le nouveau
        if (prefabToInstantiate != null && visualContainer != null)
        {
            Vector3 originalEuler = prefabToInstantiate.transform.eulerAngles;
            float finalRotationY = rotationY + originalEuler.y + currentOffset;
            Quaternion finalRotation = Quaternion.Euler(originalEuler.x, finalRotationY, originalEuler.z);

            currentVisual = Instantiate(prefabToInstantiate, visualContainer.position, finalRotation, visualContainer);
        }
    }

    private bool IsBarricadeAt(Vector2Int pos)
    {
        // On demande au PlacementManager s'il y a une tour ici
        if (PlacementManager.Instance.TryGetTowerAt(pos, out GameObject tower))
        {
            return tower.GetComponent<AutoTilingBarricade>() != null;
        }
        return false;
    }
}