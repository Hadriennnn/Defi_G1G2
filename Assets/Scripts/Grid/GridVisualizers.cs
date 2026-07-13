using System.Collections.Generic;  
using UnityEngine;  
using UnityEngine.Tilemaps;  

[RequireComponent(typeof(GridData))]  
public class GridVisualizer : MonoBehaviour  
{  
   private GridData gridData;  

   [Header("Configuration Tilemap (Décors)")]  
   public Tilemap solTilemap;  
   public Tilemap obstaclesTilemap;  
   public TileBase tuileSol;  
   public TileBase tuileBarricade;  

   [Header("Configuration Tilemap (Interface)")]  
   public Tilemap cheminTilemap;  
   public TileBase tuileEntree;  
   public TileBase tuileSortie;  
   public TileBase tuileChemin;  

   public void Init()  
   {  
       gridData = GetComponent<GridData>();  
       DrawBaseGrid();
   }  

   private void OnEnable()  
   {  
       // On s'abonne aux ordres du cerveau  
       GameEvents.OnPathUpdated += DrawPath;  
       GameEvents.OnObstacleToggled += DrawObstacle;  
   }  

   private void OnDisable()  
   {  
       GameEvents.OnPathUpdated -= DrawPath;  
       GameEvents.OnObstacleToggled -= DrawObstacle;  
   }  

   void DrawBaseGrid()  
   {  
       solTilemap.ClearAllTiles();  
       obstaclesTilemap.ClearAllTiles();  
       cheminTilemap.ClearAllTiles();  
       
       // Récupération dynamique des positions depuis le LevelManager
       Vector3Int startTilePos = new Vector3Int(LevelManager.Instance.startCoords.x, LevelManager.Instance.startCoords.y, 0);
       Vector3Int endTilePos = new Vector3Int(LevelManager.Instance.endCoords.x, LevelManager.Instance.endCoords.y, 0);

       cheminTilemap.SetTile(startTilePos, tuileEntree);  
       cheminTilemap.SetTile(endTilePos, tuileSortie);  

       for (int x = 0; x < gridData.largeur; x++)  
       {  
           for (int y = 0; y < gridData.hauteur; y++)  
           {  
               solTilemap.SetTile(new Vector3Int(x, y, 0), tuileSol);  
           }  
       }  
   }  

   public void DrawPath(List<Node> path)  
   {  
       cheminTilemap.ClearAllTiles();  

       // Sécurité si le LevelManager n'est pas encore instancié
       if (LevelManager.Instance == null) return;

       // Récupération dynamique des positions depuis le LevelManager
       Vector3Int startTilePos = new Vector3Int(LevelManager.Instance.startCoords.x, LevelManager.Instance.startCoords.y, 0);
       Vector3Int endTilePos = new Vector3Int(LevelManager.Instance.endCoords.x, LevelManager.Instance.endCoords.y, 0);

       cheminTilemap.SetTile(startTilePos, tuileEntree);  
       cheminTilemap.SetTile(endTilePos, tuileSortie);  

       if (path != null)  
       {  
           foreach (Node n in path)  
           {  
               // On empêche la tuile "chemin" d'écraser visuellement l'entrée ou la sortie
               if ((n.gridX == startTilePos.x && n.gridY == startTilePos.y) ||  
                   (n.gridX == endTilePos.x && n.gridY == endTilePos.y))  
                   continue;  

               cheminTilemap.SetTile(new Vector3Int(n.gridX, n.gridY, 0), tuileChemin);  
           }  
       }  
   }  

   public void DrawObstacle(Vector3Int position, bool isPlaced)  
   {  
       if (isPlaced)  
           obstaclesTilemap.SetTile(position, tuileBarricade);  
       else  
           obstaclesTilemap.SetTile(position, null);  
   }  

   private void OnDrawGizmos()  
   {  
       if (gridData == null) gridData = GetComponent<GridData>();  
       if (gridData == null) return;  
        
       Gizmos.color = Color.white;  
       Vector3 origin = transform.position;  

       for (int i = 0; i <= gridData.largeur; i++)  
           Gizmos.DrawLine(origin + new Vector3(gridData.cellSize * i, 0, 0), origin + new Vector3(gridData.cellSize * i, gridData.hauteur * gridData.cellSize, 0));  
            
       for (int j = 0; j <= gridData.hauteur; j++)  
           Gizmos.DrawLine(origin + new Vector3(0, gridData.cellSize * j, 0), origin + new Vector3(gridData.largeur * gridData.cellSize, gridData.cellSize * j, 0));  
   }  
}