using System.Collections.Generic;  
using UnityEngine;

public class Pathfinding : MonoBehaviour , IPathfinder
{  
   private GridData gridData;  
   private PathNode[,] pathGrid; // Grille de calcul isolée

   void Awake()  
   {  
       gridData = GetComponent<GridData>();  
       InitializePathGrid();  
   }  

   void Start()  
   {  
       // Calcul du chemin initial au lancement  
       RecalculerCheminGlobal();
   }  

   void OnEnable()  
   {  
       GameEvents.OnGridChanged += RecalculerCheminGlobal;  
   }  

   void OnDisable()  
   {  
       GameEvents.OnGridChanged -= RecalculerCheminGlobal;  
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
       GameEvents.TriggerPathUpdated(path);  
   }  

   int GetDistance(PathNode nodeA, PathNode nodeB)  
   {  
       int dstX = Mathf.Abs(nodeA.GridNode.gridX - nodeB.GridNode.gridX);  
       int dstY = Mathf.Abs(nodeA.GridNode.gridY - nodeB.GridNode.gridY);  
       return 10 * (dstX + dstY);  
   }  
}

// LA CLASSE REFACTORISÉE POUR LE COUPLAGE FORT (SOLID)
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