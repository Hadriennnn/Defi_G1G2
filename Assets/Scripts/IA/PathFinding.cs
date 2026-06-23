using System.Collections.Generic;  
using UnityEngine;

public class Pathfinding : MonoBehaviour  
{  
    GridManager gridManager;

    void Awake()  
    {  
        gridManager = GetComponent<GridManager>();  
    }  
    
    void OnEnable()
    {
        GameEvents.OnGridChanged += RecalculerCheminGlobal;
        GameEvents.RequestPathCheck += FindPath; 
    }

    void OnDisable()
    {
        GameEvents.OnGridChanged -= RecalculerCheminGlobal;
        GameEvents.RequestPathCheck -= FindPath;
    }

    private void RecalculerCheminGlobal()
    {
        FindPath(gridManager.startCoords, gridManager.endCoords);
    }
    
    public bool FindPath(Vector2Int startPos, Vector2Int targetPos)  
    {  
        gridManager.path = null;  
        for (int x = 0; x < gridManager.largeur; x++)  
        {  
            for (int y = 0; y < gridManager.hauteur; y++)  
            {  
                gridManager.Grid[x, y].parent = null;  
            }  
        }

        Node startNode = gridManager.Grid[startPos.x, startPos.y];  
        Node targetNode = gridManager.Grid[targetPos.x, targetPos.y];

        List<Node> openSet = new List<Node>();  
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)  
        {  
            Node currentNode = openSet[0];  
            for (int i = 1; i < openSet.Count; i++)  
            {  
                if (openSet[i].fCost < currentNode.fCost ||  
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))  
                {  
                    currentNode = openSet[i];  
                }  
            }

            openSet.Remove(currentNode);  
            closedSet.Add(currentNode);  
            
            if (currentNode == targetNode)  
            {  
                RetracePath(startNode, targetNode);  
                return true;  
            }

            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))  
            {  
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))  
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
                }  
            }  
        }

        return false;  
    }

    void RetracePath(Node startNode, Node endNode)  
    {  
        List<Node> path = new List<Node>();  
        Node currentNode = endNode;

        while (currentNode != startNode)  
        {  
            path.Add(currentNode);  
            currentNode = currentNode.parent;  
        }  
        
        path.Reverse();
        
        gridManager.path = path;  
        gridManager.UpdatePathVisuals();  
    }  
    
    int GetDistance(Node nodeA, Node nodeB)  
    {  
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);  
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return 10 * (dstX + dstY);  
    }
}