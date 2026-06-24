using UnityEngine;  
using UnityEngine.InputSystem;  
using UnityEngine.Tilemaps;  

public class PlacementManager : MonoBehaviour  
{  
   private GridData gridData;  
   private IPathfinder pathfinder; // Référence directe et découplée via l'interface

   [Header("Configuration du Survol (Hover)")]  
   public Tilemap hoverTilemap;  
   public TileBase tuileHover;  

   private Vector3Int derniereCaseSurvolee = new Vector3Int(-1, -1, -1);  

   void Awake()  
   {  
       gridData = GetComponent<GridData>();  
       
       // On cherche un composant sur ce GameObject (ou dans la scène) qui implémente IPathfinder
       pathfinder = GetComponent<IPathfinder>();  
       
       if (pathfinder == null)
       {
           Debug.LogError("PlacementManager : Aucun composant implémentant IPathfinder n'a été trouvé sur ce GameObject !");
       }
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
               return;  
           }  

           clickedNode.isWalkable = !clickedNode.isWalkable;  

           if (!clickedNode.isWalkable)  
           {  
               // REFACTORISÉ : On appelle directement le pathfinder via son interface !
               // Plus de Func statique qui crie dans le vide.
               bool cheminPossible = pathfinder.FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords);  

               if (!cheminPossible)  
               {  
                   clickedNode.isWalkable = true;  
                   Debug.LogWarning("Placement refusé : Bloque la sortie !");  
                   
                   // On recalcule le chemin valide pour remettre le visuel à jour
                   pathfinder.FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords);  
                   return;  
               }  
           }  

           GameEvents.TriggerObstacleToggled(derniereCaseSurvolee, !clickedNode.isWalkable);  
           GameEvents.TriggerGridChanged();  
       }  
   }  
}