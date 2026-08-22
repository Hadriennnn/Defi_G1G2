using System.Collections.Generic;  
using UnityEngine;

public class Pathfinding : MonoBehaviour , IPathfinder
{  
   private GridData gridData;  
   private PathNode[,] pathGrid; 
   
   private List<Node> dernierCheminValide = new List<Node>();

   // OPTIMISATION : Mise en cache des directions et de la liste des voisins
   private readonly Vector2Int[] directions = new Vector2Int[]  
   {  
       new Vector2Int(0, 1), new Vector2Int(0, -1),  
       new Vector2Int(1, 0), new Vector2Int(-1, 0)  
   };
   private List<PathNode> cachedNeighbors = new List<PathNode>(4);

   public void Init()  
   {  
       gridData = GetComponent<GridData>();  
       
       InitializePathGrid();  
       RecalculerCheminGlobal();
   }  
   
   void OnEnable()  
   {  
       GameEvents.OnGridChanged += RecalculerCheminGlobal;
       GameEvents.OnGameStateChanged += GererChangementPhase;
   }  

   void OnDisable()  
   {  
       GameEvents.OnGridChanged -= RecalculerCheminGlobal;  
       GameEvents.OnGameStateChanged -= GererChangementPhase;
   }  

   private void RecalculerCheminGlobal()  
   {  
       if (LevelManager.Instance != null)
       {
           FindPath(LevelManager.Instance.startCoords, LevelManager.Instance.endCoords);  
       }
   }

   private void InitializePathGrid()  
   {  
       pathGrid = new PathNode[gridData.largeur, gridData.hauteur];  
       for (int x = 0; x < gridData.largeur; x++)  
       {  
           for (int y = 0; y < gridData.hauteur; y++)  
           {  
               pathGrid[x, y] = new PathNode(gridData.Grid[x, y]);  
           }  
       }  
   }  

   public bool FindPath(Vector2Int startPos, Vector2Int targetPos)  
   {  
       for (int x = 0; x < gridData.largeur; x++)  
       {  
           for (int y = 0; y < gridData.hauteur; y++)  
           {  
               PathNode node = pathGrid[x, y];  
               node.parent = null;  
               node.gCost = int.MaxValue; 
               node.hCost = 0;  
               node.HeapIndex = -1; 
           }  
       }  

       PathNode startNode = pathGrid[startPos.x, startPos.y];  
       PathNode targetNode = pathGrid[targetPos.x, targetPos.y];  

       startNode.gCost = 0;  

       int maxSize = gridData.largeur * gridData.hauteur;  
       Heap<PathNode> openSet = new Heap<PathNode>(maxSize);  
       HashSet<PathNode> closedSet = new HashSet<PathNode>();  

       openSet.Add(startNode);  

       while (openSet.Count > 0)  
       {  
           PathNode currentNode = openSet.RemoveFirst();  
           closedSet.Add(currentNode);  

           if (currentNode == targetNode)  
           {  
               RetracePath(startNode, targetNode);  
               return true;  
           }  

           // NON-ALLOC : On remplit la liste mise en cache au lieu d'en créer une nouvelle
           GetNeighborsNonAlloc(currentNode, cachedNeighbors);

           // On utilise une boucle for classique (légèrement plus opti qu'un foreach sur certaines versions d'Unity)
           for (int i = 0; i < cachedNeighbors.Count; i++)
           {  
               PathNode neighbor = cachedNeighbors[i];

               if (!neighbor.GridNode.isWalkable || neighbor.GridNode.elevation > 0f || closedSet.Contains(neighbor))
               {  
                   continue;  
               }  

               int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);  

               if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))  
               {  
                   neighbor.gCost = newMovementCostToNeighbor;  
                   neighbor.hCost = GetDistance(neighbor, targetNode);  
                   neighbor.parent = currentNode;  

                   if (!openSet.Contains(neighbor))  
                   {  
                       openSet.Add(neighbor);  
                   }  
                   else  
                   {  
                       openSet.UpdateItem(neighbor);  
                   }  
               }  
           }  
       }  
       return false;  
   }  

   // OPTIMISATION : Ne retourne plus rien, modifie la liste passée en paramètre
   private void GetNeighborsNonAlloc(PathNode node, List<PathNode> results)  
   {  
       results.Clear(); // On vide la liste de la boucle précédente
       
       for (int i = 0; i < directions.Length; i++)  
       {  
           int checkX = node.GridNode.gridX + directions[i].x;  
           int checkY = node.GridNode.gridY + directions[i].y;  

           if (checkX >= 0 && checkX < gridData.largeur && checkY >= 0 && checkY < gridData.hauteur)  
           {  
               results.Add(pathGrid[checkX, checkY]);  
           }  
       }  
   }  

   void RetracePath(PathNode startNode, PathNode endNode)  
   {  
       List<Node> path = new List<Node>();  
       PathNode currentNode = endNode;  

       while (currentNode != startNode)  
       {  
           path.Add(currentNode.GridNode);  
           currentNode = currentNode.parent;  
       }  

       path.Reverse();  
       dernierCheminValide = path;
       GameEvents.TriggerPathUpdated(path);  
   }  

   int GetDistance(PathNode nodeA, PathNode nodeB)  
   {  
       int dstX = Mathf.Abs(nodeA.GridNode.gridX - nodeB.GridNode.gridX);  
       int dstY = Mathf.Abs(nodeA.GridNode.gridY - nodeB.GridNode.gridY);  
       return 10 * (dstX + dstY);  
   }  
   
   private void GererChangementPhase(GameState state)
   {
       if (state == GameState.ActionPhase && dernierCheminValide != null)
       {
           List<Vector3> waypointsFinaux = new List<Vector3>();
           Vector3 origineGrille = transform.position; 

           foreach (Node n in dernierCheminValide)
           {
               float worldX = origineGrille.x + (n.gridX * gridData.cellSize) + (gridData.cellSize / 2f);
               float worldZ = origineGrille.z + (n.gridY * gridData.cellSize) + (gridData.cellSize / 2f);
               
               waypointsFinaux.Add(new Vector3(worldX, origineGrille.y + 0.5f, worldZ));
           }

           if (LevelManager.Instance != null)
           {
               LevelManager.Instance.EnregistrerWaypoints(waypointsFinaux);
           }
       }
   }
}

public class PathNode : IHeapItem<PathNode>  
{  
   public Node GridNode { get; private set; } 
   public int gCost;  
   public int hCost;  
   public int fCost => gCost + hCost;  

   public PathNode parent;  
   private int heapIndex;  

   public int HeapIndex  
   {  
       get { return heapIndex; }  
       set { heapIndex = value; }  
   }  

   public PathNode(Node gridNode)  
   {  
       GridNode = gridNode;  
   }  

   public int CompareTo(PathNode nodeToCompare)  
   {  
       int compare = fCost.CompareTo(nodeToCompare.fCost);  
       if (compare == 0)  
       {  
           compare = hCost.CompareTo(nodeToCompare.hCost);  
       }  
       return -compare;  
   }
}