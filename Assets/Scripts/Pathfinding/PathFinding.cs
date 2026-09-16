using System;
using System.Collections.Generic;  
using System.Linq;
using System.Threading.Tasks;
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
    
    private bool isCalculating = false;
    private bool pendingCalculation = false;

    public void Init()  
    {  
        gridData = GetComponent<GridData>();  
        InitializePathGrid();  
        
        int maxSize = gridData.largeur * gridData.hauteur;
        openSet = new Heap<PathNode>(maxSize);
        
        _ = CheckAllPathsAsync(); 
    }  
   
    void OnEnable()  
    {  
        GameEvents.OnGridChanged += HandleGridChanged;
    }  

    void OnDisable()  
    {  
        GameEvents.OnGridChanged -= HandleGridChanged;  
    }  

    private async void HandleGridChanged()
    {
        await CheckAllPathsAsync();
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

    public async Task<bool> CheckAllPathsAsync()
    {
        if (isCalculating) 
        {
            pendingCalculation = true;
            return false; 
        }
        
        isCalculating = true;

        List<Vector2Int> spawns = LevelManager.Instance != null ? LevelManager.Instance.spawnPoints.ToList() : new List<Vector2Int>();
        List<Vector2Int> bases = LevelManager.Instance != null ? LevelManager.Instance.GetBaseCoords() : new List<Vector2Int>();
        List<GlobalSocket> doors = ChunkManager.Instance != null ? ChunkManager.Instance.allDoors.ToList() : new List<GlobalSocket>();

        Dictionary<Vector2Int, List<Node>> safeAnciensChemins = new Dictionary<Vector2Int, List<Node>>(derniersCheminsValides);
        Dictionary<Vector2Int, List<Node>> temporaryPaths = new Dictionary<Vector2Int, List<Node>>();
        HashSet<Node> routesExistantes = new HashSet<Node>();
        
        bool allValid = true;
        Exception backgroundException = null;

        await Task.Run(() =>
        {
            try
            {
                foreach (Vector2Int spawnCoord in spawns)
                {
                    if (!gridData.Grid[spawnCoord.x, spawnCoord.y].isActive) continue;

                    HashSet<Node> oldPathForThisSpawn = new HashSet<Node>();
                    if (safeAnciensChemins.ContainsKey(spawnCoord)) 
                    {
                        oldPathForThisSpawn = new HashSet<Node>(safeAnciensChemins[spawnCoord]);
                    }

                    List<Node> pathForThisSpawn = FindSinglePath(spawnCoord, bases, routesExistantes, oldPathForThisSpawn);
                   
                    if (pathForThisSpawn == null) 
                    {
                        allValid = false;
                        break; 
                    }

                    temporaryPaths[spawnCoord] = pathForThisSpawn;
                    foreach(Node n in pathForThisSpawn) routesExistantes.Add(n);
                }

                if (!allValid) return; 

                foreach (GlobalSocket door in doors)
                {
                    if (!gridData.Grid[door.position.x, door.position.y].isActive) continue;

                    Vector2Int dirVec = GetDirectionVector(door.direction);
                    Vector2Int facingPos = door.position + dirVec;
                    
                    if (facingPos.x >= 0 && facingPos.x < gridData.largeur && facingPos.y >= 0 && facingPos.y < gridData.hauteur)
                    {
                        if (gridData.Grid[facingPos.x, facingPos.y].isActive) continue;
                    }

                    HashSet<Node> emptyMemory = new HashSet<Node>();
                    List<Node> pathForDoor = FindSinglePath(door.position, bases, routesExistantes, emptyMemory);
                    if (pathForDoor == null) 
                    {
                        allValid = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                backgroundException = ex;
                allValid = false;
            }
        });

        if (backgroundException != null) 
        {
            Debug.LogError($"[Pathfinding] Erreur arrière-plan : {backgroundException}");
        }

        if (allValid)
        {
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
                if (LevelManager.Instance != null) LevelManager.Instance.EnregistrerWaypoints(spawnPoint, waypointsFinaux);
            }
        }

        isCalculating = false;

        if (pendingCalculation)
        {
            pendingCalculation = false;
            _ = CheckAllPathsAsync();
        }

        return allValid;
    }

    private Vector2Int GetDirectionVector(SocketDirection dir)
    {
        switch (dir)
        {
            case SocketDirection.Nord: return new Vector2Int(0, 1);
            case SocketDirection.Sud: return new Vector2Int(0, -1);
            case SocketDirection.Est: return new Vector2Int(1, 0);
            case SocketDirection.Ouest: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
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

                int coutDeBase = 1000; 
                
                if (preferredNodes.Contains(neighbor.GridNode)) coutDeBase = 500;
                else if (oldPathNodes.Contains(neighbor.GridNode)) coutDeBase = 999; 

                int newMovementCostToNeighbor = currentNode.gCost + coutDeBase;  

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))  
                {  
                    neighbor.gCost = newMovementCostToNeighbor;  
                    
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
        if (compare == 0) compare = hCost.CompareTo(nodeToCompare.hCost);  
        return -compare;  
    }
}