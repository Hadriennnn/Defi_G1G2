using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.Tilemaps;

public class PlacementManager : MonoBehaviour
{
    private GridManager gridManager;

    [Header("Configuration du Survol (Hover)")]
    public Tilemap hoverTilemap;         
    public TileBase tuileHover;          
    
    private Vector3Int derniereCaseSurvolee = new Vector3Int(-1, -1, -1); 

    void Awake()
    {
        gridManager = GetComponent<GridManager>();
    }

    void Update()
    {
        GererHover();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceBarricade();
        }
    }

    void GererHover()
    {
        if (Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPoint.z = 0; 

        Vector3Int caseActuelle = gridManager.solTilemap.WorldToCell(worldPoint);

        if (caseActuelle != derniereCaseSurvolee)
        {
            hoverTilemap.SetTile(derniereCaseSurvolee, null);

            if (caseActuelle.x >= 0 && caseActuelle.x < gridManager.largeur && 
                caseActuelle.y >= 0 && caseActuelle.y < gridManager.hauteur)
            {
                hoverTilemap.SetTile(caseActuelle, tuileHover);
            }

            derniereCaseSurvolee = caseActuelle;
        }
    }

    void PlaceBarricade()
    {
        int gridX = derniereCaseSurvolee.x;
        int gridY = derniereCaseSurvolee.y;

        if (gridX >= 0 && gridX < gridManager.largeur && gridY >= 0 && gridY < gridManager.hauteur)
        {
            Node clickedNode = gridManager.Grid[gridX, gridY];
            
            if ((gridX == gridManager.entreeX && gridY == gridManager.entreeY) || 
                (gridX == gridManager.sortieX && gridY == gridManager.sortieY))
            {
                return; 
            }

            // 1. Simulation mémoire
            clickedNode.isWalkable = !clickedNode.isWalkable; 

            //  2. Test Anti-Maze
            if (!clickedNode.isWalkable)
            {
                bool cheminPossible = GameEvents.CheckPathPossible(gridManager.startCoords, gridManager.endCoords);
                
                if (!cheminPossible)
                {
                    clickedNode.isWalkable = true; // Annulation
                    Debug.LogWarning("Placement refusé : Bloque la sortie !");
                    
                    GameEvents.CheckPathPossible(gridManager.startCoords, gridManager.endCoords);
                    return; 
                }
            }

            // 3. Mise à jour visuelle des obstacles
            if (!clickedNode.isWalkable)
            {
                gridManager.obstaclesTilemap.SetTile(derniereCaseSurvolee, gridManager.tuileBarricade);
            }
            else
            {
                gridManager.obstaclesTilemap.SetTile(derniereCaseSurvolee, null);
            }

            // 4. On annonce au jeu que la grille a changé
            GameEvents.TriggerGridChanged();
        }
    }
}