using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/* 
 * RÉSUMÉ : Gère l'interaction de la souris avec la grille via Raycast 3D, et l'instanciation des tours.
 */
public class PlacementManager : MonoBehaviour  
{  
    private GridData gridData;  
    private IPathfinder pathfinder; 

    [Header("Configuration du Survol (Hover 3D)")]  
    [Tooltip("Un GameObject plat (ex: un Quad ou Plane) pour montrer la case sélectionnée.")]
    public GameObject hoverIndicator; 
    [Tooltip("Le layer qui représente le sol physique pour arrêter le rayon de la souris.")]
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
       
        if (pathfinder == null)
        {
            Debug.LogError("PlacementManager : Aucun composant implémentant IPathfinder n'a été trouvé");
        }

        if (hoverIndicator != null) 
            hoverIndicator.SetActive(false);
    }

    //On s'abo aux evénements de changement de phase
    void OnEnable()
    {
        GameEvents.OnGameStateChanged += GererChangementDePhase;
    }

    void OnDisable()
    {
        GameEvents.OnGameStateChanged -= GererChangementDePhase;
    }

    private void GererChangementDePhase(GameState newState)
    {
        if (newState == GameState.PreparationPhase)
        {
            peutPlacer = true;
        }
        else if (newState == GameState.ActionPhase)
        {
            peutPlacer = false;
            if (hoverIndicator != null) hoverIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (!peutPlacer) return;
       
        // Sécurité : on empêche de construire si on clique sur un bouton de l'UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            MasquerHover();
            return; 
        }
       
        GererHover();  

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)  
        {  
            if (derniereCaseSurvolee.x != -1) // Si on pointe bien une case
            {
                PlaceBarricade();  
            }
        }  
    }  

    void GererHover()  
    {  
        if (Mouse.current == null || Camera.main == null) return;  

        Vector2 mousePosition = Mouse.current.position.ReadValue();  
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
       
        // On lance un laser de la caméra vers le sol (ground layer)
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 origineGrille = transform.position;
            
            //On calcule la position locale par rapport à l'origine de la grille
            float localX = hit.point.x - origineGrille.x;
            float localZ = hit.point.z - origineGrille.z; 
           
            int gridX = Mathf.FloorToInt(localX / gridData.cellSize);  
            int gridY = Mathf.FloorToInt(localZ / gridData.cellSize); 
           
            Vector2Int caseActuelle = new Vector2Int(gridX, gridY);  

            //On vérifie si on est dans les limites de la grille
            if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
            {  
                if (caseActuelle != derniereCaseSurvolee)  
                {  
                    derniereCaseSurvolee = caseActuelle;  
                   
                    //On déplace le visualiseur 3D
                    if (hoverIndicator != null)
                    {
                        hoverIndicator.SetActive(true);
                        float hauteurTuile = gridData.Grid[gridX, gridY].elevation;
                        
                        // On recalcule le centre exact de la case en 3D
                        float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                        float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                        
                        
                        hoverIndicator.transform.position = new Vector3(worldX, origineGrille.y + hauteurTuile + 0.03f, worldZ);
                    }
                }  
            }
            else
            {
                MasquerHover();
            }
        }
        else
        {
            MasquerHover();
        }
    }  
   
    void MasquerHover()
    {
        if (hoverIndicator != null) hoverIndicator.SetActive(false);
        derniereCaseSurvolee = new Vector2Int(-1, -1);
    }

    // S'appelle comme ça mais ça pose une tourelle pour l'instant
    //Plus tard on pourra poser le type qu'on a dans notre main, le nom est pas très bien choisit mais bon 
    void PlaceBarricade()  
    {  
        int gridX = derniereCaseSurvolee.x;  
        int gridY = derniereCaseSurvolee.y;  

        if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
        {  
            Node clickedNode = gridData.Grid[gridX, gridY];  
            Vector2Int inputCoords = new Vector2Int(gridX, gridY);  

            if (LevelManager.Instance != null && LevelManager.Instance.IsStartOrEnd(inputCoords))  
            {  
                return; 
            }  

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
                    float hauteurTuile = clickedNode.elevation;
                    // PLACEMENT EN 3D
                    Vector3 origineGrille = transform.position;
                    float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                    float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                    
                    
                    Vector3 spawnPos = new Vector3(worldX, origineGrille.y + hauteurTuile, worldZ);

                    // On remplace Quaternion.identity par towerPrefab.transform.rotation
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

            // On garde Vector3Int pour l'événement car le GameEvents l'utilise encore. On changera ça plus tard.
            GameEvents.TriggerObstacleToggled(new Vector3Int(gridX, gridY, 0), !clickedNode.isWalkable);  
            GameEvents.TriggerGridChanged();  
        }  
    }  
}