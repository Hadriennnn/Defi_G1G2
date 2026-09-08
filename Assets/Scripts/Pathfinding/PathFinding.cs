using System.Collections.Generic;  
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour, IPathfinder
{  
    private GridData gridData;  
    private PathNode[,] pathGrid; 
   
    private Dictionary<Vector2Int, List<Node>> derniersCheminsValides = new Dictionary<Vector2Int, List<Node>>();

    private readonly Vector2Int[] directions = new Vector2Int[]  
    {  
        new Vector2Int(0, 1), new Vector2Int(0, -1),  
        new Vector2Int(1, 0), new Vector2Int(-1, 0)  
    };
    
    private List<PathNode> cachedNeighbors = new List<PathNode>(4);
    private HashSet<PathNode> closedSet = new HashSet<PathNode>();
    private Heap<PathNode> openSet;
    private int currentSearchId = 0;
    private HashSet<PathNode> cachedTargetNodes = new HashSet<PathNode>();

    public void Init()  
    {  
        gridData = GetComponent<GridData>();  
        InitializePathGrid();  
        
        int maxSize = gridData.largeur * gridData.hauteur;
        openSet = new Heap<PathNode>(maxSize);
        
        CheckAllPaths(); 
    }  
   
    void OnEnable()  
    {  
        GameEvents.OnGridChanged += () => CheckAllPaths();
    }  

    void OnDisable()  
    {  
        GameEvents.OnGridChanged -= () => CheckAllPaths();  
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

    public bool CheckAllPaths()
    {
        Dictionary<Vector2Int, List<Node>> temporaryPaths = new Dictionary<Vector2Int, List<Node>>();
        HashSet<Node> routesExistantes = new HashSet<Node>(); 

        // 1. VÉRIFICATION DES PORTAILS (Spawns)
        foreach (Vector2Int spawnCoord in LevelManager.Instance.spawnPoints)
        {
            if (!gridData.Grid[spawnCoord.x, spawnCoord.y].isActive) continue;

            // NOUVEAU : On récupère l'ancien chemin pour donner une "Inertie" à la recherche
            HashSet<Node> oldPathForThisSpawn = new HashSet<Node>();
            if (derniersCheminsValides != null && derniersCheminsValides.ContainsKey(spawnCoord))
            {
                oldPathForThisSpawn = new HashSet<Node>(derniersCheminsValides[spawnCoord]);
            }

            // On ajoute le paramètre oldPathForThisSpawn à la fonction
            List<Node> pathForThisSpawn = FindSinglePath(spawnCoord, LevelManager.Instance.GetBaseCoords(), routesExistantes, oldPathForThisSpawn);
           
            if (pathForThisSpawn == null) return false; 

            temporaryPaths[spawnCoord] = pathForThisSpawn;
           
            foreach(Node n in pathForThisSpawn) routesExistantes.Add(n);
        }

        // 2. VÉRIFICATION DE TOUTES LES PORTES 
        if (ChunkManager.Instance != null)
        {
            foreach (GlobalSocket door in ChunkManager.Instance.allDoors)
            {
                if (!gridData.Grid[door.position.x, door.position.y].isActive) continue;

                Vector2Int dirVec = ChunkManager.Instance.GetVectorFromDirection(door.direction);
                Vector2Int facingPos = door.position + dirVec;
                
                if (facingPos.x >= 0 && facingPos.x < gridData.largeur && facingPos.y >= 0 && facingPos.y < gridData.hauteur)
                {
                    if (gridData.Grid[facingPos.x, facingPos.y].isActive) continue;
                }

                // Pour les portes qui n'ont pas d'ennemis, on passe une mémoire vide
                HashSet<Node> emptyMemory = new HashSet<Node>();
                List<Node> pathForDoor = FindSinglePath(door.position, LevelManager.Instance.GetBaseCoords(), routesExistantes, emptyMemory);
                if (pathForDoor == null) return false; 
            }
        }

        // 3. ENREGISTREMENT DES CHEMINS 
        derniersCheminsValides = temporaryPaths;
        GameEvents.TriggerPathUpdated(routesExistantes.ToList()); 
        
        Vector3 origineGrille = transform.position; 
        foreach (var kvp in derniersCheminsValides)
        {
            Vector2Int spawnPoint = kvp.Key;
            List<Node> nodesDuChemin = kvp.Value;
            List<Vector3> waypointsFinaux = new List<Vector3>();

            foreach (Node n in nodesDuChemin)
            {
                float worldX = origineGrille.x + (n.gridX * gridData.cellSize) + (gridData.cellSize / 2f);
                float worldZ = origineGrille.z + (n.gridY * gridData.cellSize) + (gridData.cellSize / 2f);
                waypointsFinaux.Add(new Vector3(worldX, origineGrille.y + 0.5f, worldZ));
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.EnregistrerWaypoints(spawnPoint, waypointsFinaux);
            }
        }

        return true;
    }

    private void PrepareNode(PathNode node)
    {
        if (node.searchId != currentSearchId)
        {
            node.searchId = currentSearchId;
            node.parent = null;
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.HeapIndex = -1;
        }
    }

    // NOUVEAU SIGNATURE : Ajout de "oldPathNodes"
    private List<Node> FindSinglePath(Vector2Int startPos, List<Vector2Int> targetPositions, HashSet<Node> preferredNodes, HashSet<Node> oldPathNodes)  
    {  
        if (!pathGrid[startPos.x, startPos.y].GridNode.isWalkable) return null;

        currentSearchId++;
        openSet.Clear();
        closedSet.Clear();

        PathNode startNode = pathGrid[startPos.x, startPos.y];  
        PrepareNode(startNode);

        cachedTargetNodes.Clear();
        foreach (Vector2Int pos in targetPositions)
        {
            PathNode tNode = pathGrid[pos.x, pos.y];
            PrepareNode(tNode);
            cachedTargetNodes.Add(tNode);
        }

        startNode.gCost = 0;  
        openSet.Add(startNode);  

        while (openSet.Count > 0)  
        {  
            PathNode currentNode = openSet.RemoveFirst();  
            closedSet.Add(currentNode);  

            if (cachedTargetNodes.Contains(currentNode))  
            {  
                return RetracePath(startNode, currentNode);  
            }

            GetNeighborsNonAlloc(currentNode, cachedNeighbors);

            for (int i = 0; i < cachedNeighbors.Count; i++)
            {  
                PathNode neighbor = cachedNeighbors[i];
                PrepareNode(neighbor);

                if (!neighbor.GridNode.isWalkable || neighbor.GridNode.elevation > 0f || !neighbor.GridNode.isActive || closedSet.Contains(neighbor))
                {  
                    continue;  
                }  

                // CORRECTION MATHÉMATIQUE (x100 pour plus de précision)
                int coutDeBase = 1000; 
                
                if (preferredNodes.Contains(neighbor.GridNode)) 
                {
                    coutDeBase = 500; // Aimant pour fusionner les chemins
                }
                else if (oldPathNodes.Contains(neighbor.GridNode))
                {
                    coutDeBase = 999; // INERTIE : On donne 1 point de "réduction" si la case faisait partie de l'ancien chemin !
                }

                int newMovementCostToNeighbor = currentNode.gCost + coutDeBase;  

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))  
                {  
                    neighbor.gCost = newMovementCostToNeighbor;  
                    
                    // L'heuristique (hCost) s'adapte au coût minimum absolu possible (500) pour rester juste
                    PathNode referenceTarget = pathGrid[targetPositions[0].x, targetPositions[0].y];
                    neighbor.hCost = GetRawDistance(neighbor, referenceTarget) * 500; 
                    
                    neighbor.parent = currentNode;  

                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);  
                    else openSet.UpdateItem(neighbor);  
                }  
            }  
        }  
        return null; 
    }  

    private void GetNeighborsNonAlloc(PathNode node, List<PathNode> results)  
    {  
        results.Clear(); 
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

    List<Node> RetracePath(PathNode startNode, PathNode endNode)  
    {  
        List<Node> path = new List<Node>();  
        PathNode currentNode = endNode;  

        while (currentNode != startNode)  
        {  
            path.Add(currentNode.GridNode);  
            currentNode = currentNode.parent;  
        }  
       
        path.Add(startNode.GridNode); 
        path.Reverse();  
        return path;
    }  

    int GetRawDistance(PathNode nodeA, PathNode nodeB)  
    {  
        int dstX = Mathf.Abs(nodeA.GridNode.gridX - nodeB.GridNode.gridX);  
        int dstY = Mathf.Abs(nodeA.GridNode.gridY - nodeB.GridNode.gridY);  
        return dstX + dstY;  
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

    public int searchId;

    public int HeapIndex  
    {  
        get { return heapIndex; }  
        set { heapIndex = value; }  
    }  

    public PathNode(Node gridNode)  
    {  
        GridNode = gridNode;  
        searchId = 0; 
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