using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour  
{  
    private GridData gridData;  
    private IPathfinder pathfinder; 
    private GridVisualizer gridVisualizer; // On récupère le visualiseur pour connaître la hauteur physique

    [Header("Configuration du Survol (Hover 3D)")]  
    public GameObject hoverIndicator; 
    public LayerMask groundLayer; 

    [Header("Configuration des Structures")]
    public GameObject towerPrefab; 
   
    private Dictionary<Vector2Int, GameObject> placedTowers = new Dictionary<Vector2Int, GameObject>();

    private Vector2Int derniereCaseSurvolee = new Vector2Int(-1, -1);
    private bool peutPlacer = true;

    void Awake()  
    {  
        gridData = GetComponent<GridData>();  
        pathfinder = GetComponent<IPathfinder>();  
        gridVisualizer = GetComponent<GridVisualizer>();
       
        if (hoverIndicator != null) hoverIndicator.SetActive(false);
    }

    void OnEnable() { GameEvents.OnGameStateChanged += GererChangementDePhase; }
    void OnDisable() { GameEvents.OnGameStateChanged -= GererChangementDePhase; }

    private void GererChangementDePhase(GameState newState)
    {
        if (newState == GameState.PreparationPhase) peutPlacer = true;
        else if (newState == GameState.ActionPhase)
        {
            peutPlacer = false;
            if (hoverIndicator != null) hoverIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (!peutPlacer) return;
       
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            MasquerHover();
            return; 
        }
       
        GererHover();  

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)  
        {  
            if (derniereCaseSurvolee.x != -1) PlaceBarricade();  
        }  
    }  

    void GererHover()  
    {  
        if (Mouse.current == null || Camera.main == null) return;  

        Vector2 mousePosition = Mouse.current.position.ReadValue();  
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
       
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 origineGrille = transform.position;
            
            float localX = hit.point.x - origineGrille.x;
            float localZ = hit.point.z - origineGrille.z; 
           
            int gridX = Mathf.FloorToInt(localX / gridData.cellSize);  
            int gridY = Mathf.FloorToInt(localZ / gridData.cellSize); 
           
            Vector2Int caseActuelle = new Vector2Int(gridX, gridY);  

            if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
            {  
                if (caseActuelle != derniereCaseSurvolee)  
                {  
                    derniereCaseSurvolee = caseActuelle;  
                   
                    if (hoverIndicator != null)
                    {
                        hoverIndicator.SetActive(true);
                        
                        // NOUVEAU : On calcule la hauteur physique exacte
                        float hauteurPhysique = gridData.Grid[gridX, gridY].elevation * gridVisualizer.hauteurNiveau;
                        
                        float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                        float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                        
                        hoverIndicator.transform.position = new Vector3(worldX, origineGrille.y + hauteurPhysique + 0.03f, worldZ);
                    }
                }  
            }
            else MasquerHover();
        }
        else MasquerHover();
    }  
   
    void MasquerHover()
    {
        if (hoverIndicator != null) hoverIndicator.SetActive(false);
        derniereCaseSurvolee = new Vector2Int(-1, -1);
    }

    void PlaceBarricade()  
    {  
        int gridX = derniereCaseSurvolee.x;  
        int gridY = derniereCaseSurvolee.y;  

        if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
        {  
            Node clickedNode = gridData.Grid[gridX, gridY];  
            Vector2Int inputCoords = new Vector2Int(gridX, gridY);  

            if (LevelManager.Instance != null && LevelManager.Instance.IsStartOrEnd(inputCoords)) return; 

            clickedNode.isWalkable = !clickedNode.isWalkable;  

            if (!clickedNode.isWalkable)  
            {  
                bool cheminPossible = pathfinder.FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords);  

                if (!cheminPossible)  
                {  
                    clickedNode.isWalkable = true;  
                    Debug.LogWarning("Placement refusé : Bloque totalement la sortie !");  
                    pathfinder.FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords);  
                    return;  
                }  
                else
                {
                    // NOUVEAU : On applique la hauteur physique au placement de la tour
                    float hauteurPhysique = clickedNode.elevation * gridVisualizer.hauteurNiveau;
                    
                    Vector3 origineGrille = transform.position;
                    float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                    float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                    
                    Vector3 spawnPos = new Vector3(worldX, origineGrille.y + hauteurPhysique, worldZ);

                    GameObject nouvelleTour = Instantiate(towerPrefab, spawnPos, towerPrefab.transform.rotation);
                    placedTowers.Add(inputCoords, nouvelleTour);
                }
            }  
            else  
            {  
                if (placedTowers.ContainsKey(inputCoords))
                {
                    Destroy(placedTowers[inputCoords]);
                    placedTowers.Remove(inputCoords);
                }
            }  

            GameEvents.TriggerObstacleToggled(new Vector3Int(gridX, gridY, 0), !clickedNode.isWalkable);  
            GameEvents.TriggerGridChanged();  
        }  
    }  
}