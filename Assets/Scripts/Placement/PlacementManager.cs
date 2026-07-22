using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;  
using UnityEngine.Tilemaps;

/* 
 * RÉSUMÉ : Gère l'interaction de la souris avec la grille, l'instanciation des tours.
 * 
 * MÉTHODES PRINCIPALES :
 * - GererChangementDePhase() : Active ou désactive la possibilité de construire selon l'état de la partie.
 * - GererHover() : Calcule la position de la souris et affiche un indicateur visuel sur la case survolée.
 * - PlaceBarricade() : Lance un test de pathfinding virtuel ; si valide, retire le statut "marchable", instancie la tour et déclenche la mise à jour visuelle.
 */

public class PlacementManager : MonoBehaviour  
{  
   private GridData gridData;  
   private IPathfinder pathfinder; 

   [Header("Configuration du Survol (Hover)")]  
   public Tilemap hoverTilemap;  
   public TileBase tuileHover;  

   [Header("Configuration des Structures")]
   public GameObject towerPrefab; // <-- NOUVEAU : Le Prefab de ta BasicTower
   
   // NOUVEAU : Un dictionnaire pour mémoriser quelle tour est sur quelle case
   private Dictionary<Vector2Int, GameObject> placedTowers = new Dictionary<Vector2Int, GameObject>();

   private Vector3Int derniereCaseSurvolee = new Vector3Int(-1, -1, -1);
   private bool peutPlacer = true;

   void Awake()  
   {  
       gridData = GetComponent<GridData>();  
       pathfinder = GetComponent<IPathfinder>();  
       
       if (pathfinder == null)
       {
           Debug.LogError("PlacementManager : Aucun composant implémentant IPathfinder n'a été trouvé !");
       }
   }

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
           hoverTilemap.ClearAllTiles();
       }
   }

   void Update()
   {
       if (!peutPlacer) return;
       
       if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
       {
           if (derniereCaseSurvolee.x != -1)
           {
               hoverTilemap.ClearAllTiles();
               derniereCaseSurvolee = new Vector3Int(-1, -1, -1);
           }
           return; 
       }
       
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
        
       int gridX = Mathf.FloorToInt(worldPoint.x / gridData.cellSize);  
       int gridY = Mathf.FloorToInt(worldPoint.y / gridData.cellSize);  
       Vector3Int caseActuelle = new Vector3Int(gridX, gridY, 0);  

       if (caseActuelle != derniereCaseSurvolee)  
       {  
           hoverTilemap.SetTile(derniereCaseSurvolee, null);  
           if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
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

       if (gridX >= 0 && gridX < gridData.largeur && gridY >= 0 && gridY < gridData.hauteur)  
       {  
           Node clickedNode = gridData.Grid[gridX, gridY];  
           Vector2Int inputCoords = new Vector2Int(gridX, gridY);  

           if (LevelManager.Instance != null && LevelManager.Instance.IsStartOrEnd(inputCoords))  
           {  
               return; // Interdit de bloquer l'entrée ou la sortie
           }  

           clickedNode.isWalkable = !clickedNode.isWalkable;  

           if (!clickedNode.isWalkable)  
           {  
               // 1. TENTATIVE DE PLACEMENT
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
                   // 2. PLACEMENT VALIDE : Instanciation de la tour
                   Vector3 origineGrille = transform.position;
                   float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                   float worldY = origineGrille.y + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                   Vector3 spawnPos = new Vector3(worldX, worldY, 0);

                   GameObject nouvelleTour = Instantiate(towerPrefab, spawnPos, Quaternion.identity);
                   
                   // On l'ajoute au dictionnaire pour pouvoir la retrouver
                   placedTowers.Add(inputCoords, nouvelleTour);
               }
           }  
           else  
           {  
               // 3. SUPPRESSION DE LA TOUR (Le joueur a recliqué sur la case)
               if (placedTowers.ContainsKey(inputCoords))
               {
                   Destroy(placedTowers[inputCoords]);
                   placedTowers.Remove(inputCoords);
               }
           }  

           GameEvents.TriggerObstacleToggled(derniereCaseSurvolee, !clickedNode.isWalkable);  
           GameEvents.TriggerGridChanged();  
       }  
   }  
}