using System.Collections.Generic;  
using UnityEngine;

public class Pathfinding : MonoBehaviour , IPathfinder
{  
   private GridData gridData;  
   private PathNode[,] pathGrid; // Grille de calcul isolée
   
   private List<Node> dernierCheminValide = new List<Node>();

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

   // On pré-alloue tous les PathNodes UNE SEULE FOIS pour éviter le Garbage Collector
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
       // 1. RÉINITIALISATION COMPLÈTE de notre grille de calcul
       for (int x = 0; x < gridData.largeur; x++)  
       {  
           for (int y = 0; y < gridData.hauteur; y++)  
           {  
               PathNode node = pathGrid[x, y];  
               node.parent = null;  
               node.gCost = int.MaxValue; // On commence à l'infini pour l'A*
               node.hCost = 0;  
               node.HeapIndex = -1; // Indique qu'il n'est pas dans le Heap
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

           foreach (PathNode neighbor in GetNeighbors(currentNode))  
           {  
               // On lit le "isWalkable" depuis la vraie grille GridData sous-jacente
               if (!neighbor.GridNode.isWalkable || closedSet.Contains(neighbor))  
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

   private List<PathNode> GetNeighbors(PathNode node)  
   {  
       List<PathNode> neighbors = new List<PathNode>();  
       Vector2Int[] directions = new Vector2Int[]  
       {  
           new Vector2Int(0, 1), new Vector2Int(0, -1),  
           new Vector2Int(1, 0), new Vector2Int(-1, 0)  
       };  

       foreach (Vector2Int dir in directions)  
       {  
           int checkX = node.GridNode.gridX + dir.x;  
           int checkY = node.GridNode.gridY + dir.y;  

           if (checkX >= 0 && checkX < gridData.largeur && checkY >= 0 && checkY < gridData.hauteur)  
           {  
               neighbors.Add(pathGrid[checkX, checkY]);  
           }  
       }  
       return neighbors;  
   }  

   void RetracePath(PathNode startNode, PathNode endNode)  
   {  
       // On reconstruit une liste de "Node" purs pour ne pas casser ton GridVisualizer !
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
           Vector3 origineGrille = transform.position; // L'origine du monde de ta grille

           foreach (Node n in dernierCheminValide)
           {
               // On convertit les Node(X,Y) en Vector3.
               // On ajoute cellSize / 2f pour que l'ennemi marche au MILIEU de la case.
               float worldX = origineGrille.x + (n.gridX * gridData.cellSize) + (gridData.cellSize / 2f);
               float worldY = origineGrille.y + (n.gridY * gridData.cellSize) + (gridData.cellSize / 2f);
               
               waypointsFinaux.Add(new Vector3(worldX, worldY, 0));
           }

           // On envoie la liste verrouillée au LevelManager !
           if (LevelManager.Instance != null)
           {
               LevelManager.Instance.EnregistrerWaypoints(waypointsFinaux);
           }
       }
   }
   
   
}






public class PathNode : IHeapItem<PathNode>  
{  
   public Node GridNode { get; private set; } // Référence vers le nœud de données pur
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