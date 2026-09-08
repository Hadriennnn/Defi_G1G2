using System.Collections.Generic;  
using UnityEngine;  

[RequireComponent(typeof(GridData))]  
public class GridVisualizer : MonoBehaviour  
{  
    private GridData gridData;  

    [Header("Configuration Verticalité")]
    public float hauteurNiveau = 0.7f; 
    public GameObject falaisePrefab; 
    public int profondeurSousTerre = 2; 

    [Header("Configuration Décors 3D (Surfaces)")]  
    public GameObject tuileSolPrefab;  
    
    [Header("Configuration Entrée / Sortie")]
    public GameObject tuileEntreePrefab;  
    public float offsetRotationEntree = 0f; 
    
    public GameObject tuileSortiePrefab;  
    public float offsetRotationSortie = 0f; 
    
    [Header("Configuration des Portes (Hologramme/Indicateur)")]
    [Tooltip("Objet posé PAR-DESSUS la tuile pour indiquer une porte vers le vide")]
    public GameObject indicateurPortePrefab; 
    public float offsetRotationPorte = 0f;
    public float hauteurIndicateur = 0.05f; // Légèrement au-dessus du sol
    
    [Header("Kit de Tuiles Modulaires (Chemin)")]  
    public GameObject tilePathStraight;
    public float offsetStraight = 0f;
    public GameObject tilePathCorner;   
    public float offsetCorner = 0f;
    public GameObject tilePathT;        
    public float offsetT = 0f;
    public GameObject tilePathCross;    
    public float offsetCross = 0f;

    private List<GameObject> objetsChemin = new List<GameObject>();
    private GameObject[,] tuilesSolMemoire;
    private List<GameObject> objetsFalaises = new List<GameObject>(); 
    
    // NOUVEAU : On garde en mémoire les indicateurs de porte pour les nettoyer
    private List<GameObject> indicateursActifs = new List<GameObject>();

    public void Init()  
    {  
        gridData = GetComponent<GridData>();  
        DrawBaseGrid();
    }  

    private void OnEnable()  
    {  
        GameEvents.OnPathUpdated += DrawPath;  
        GameEvents.OnMapExpanded += DrawBaseGrid; 
    }  

    private void OnDisable()  
    {  
        GameEvents.OnPathUpdated -= DrawPath;  
        GameEvents.OnMapExpanded -= DrawBaseGrid;
    }  

    void DrawBaseGrid()  
    {  
        foreach (GameObject falaise in objetsFalaises) { if (falaise != null) Destroy(falaise); }
        objetsFalaises.Clear();

        // Nettoyage des anciens indicateurs de porte via le PoolManager
        foreach (GameObject ind in indicateursActifs) 
        { 
            if (ind.TryGetComponent<PooledItem>(out PooledItem poolItem)) poolItem.ReleaseToPool();
            else Destroy(ind);
        }
        indicateursActifs.Clear();

        if (tuilesSolMemoire != null)
        {
            foreach (GameObject sol in tuilesSolMemoire) { if (sol != null) Destroy(sol); }
        }

        tuilesSolMemoire = new GameObject[gridData.largeur, gridData.hauteur];

        for (int x = 0; x < gridData.largeur; x++)  
        {  
            for (int y = 0; y < gridData.hauteur; y++)  
            {  
                Node currentNode = gridData.Grid[x, y];
                
                if (!currentNode.isActive) continue;

                int targetHeight = Mathf.Max(0, Mathf.RoundToInt(currentNode.elevation));

                for (int h = -profondeurSousTerre; h <= targetHeight; h++)
                {
                    Vector3 position = GetWorldPosition(x, y);
                    position.y = transform.position.y + (h * hauteurNiveau);

                    bool isTopBlock = (h == targetHeight);

                    if (isTopBlock)
                    {
                        GameObject prefabADessiner = tuileSolPrefab;
                        float yRotationOffset = 0f; 

                        // 1. GESTION DE L'ENTRÉE
                        if (LevelManager.Instance.spawnPoints.Contains(new Vector2Int(x, y)) && tuileEntreePrefab != null)
                        {
                            prefabADessiner = tuileEntreePrefab;
                            yRotationOffset = offsetRotationEntree; 
                        }
                        
                        // 2. GESTION DE LA BASE 2x2
                        bool isBaseRoot = (x == ChunkManager.Instance.baseNoyauCoords.x && y == ChunkManager.Instance.baseNoyauCoords.y);
                        bool isPartOfBase = LevelManager.Instance.GetBaseCoords().Contains(new Vector2Int(x, y));

                        if (isBaseRoot && tuileSortiePrefab != null)
                        {
                            prefabADessiner = tuileSortiePrefab;
                            yRotationOffset = offsetRotationSortie; 
                            position.x += gridData.cellSize / 2f;
                            position.z += gridData.cellSize / 2f;
                        }
                        else if (isPartOfBase)
                        {
                            prefabADessiner = null; 
                        }

                        // 3. CRÉATION DU SOL NORMAL
                        if (prefabADessiner != null)
                        {
                            Vector3 originalEuler = prefabADessiner.transform.eulerAngles;
                            Quaternion finalRotation = Quaternion.Euler(originalEuler.x, originalEuler.y + yRotationOffset, originalEuler.z);

                            GameObject solInstancie = Instantiate(prefabADessiner, position, finalRotation, transform);
                            tuilesSolMemoire[x, y] = solInstancie;

                            if (prefabADessiner == tuileEntreePrefab)
                            {
                                EnemySpawner spawner = solInstancie.GetComponent<EnemySpawner>();
                                if (spawner != null) 
                                {
                                    int idDuChunk = currentNode.chunkId; 
                                    SpawnerProfile profilDuChunk = null;
                                    if (ChunkManager.Instance.activeSpawns.ContainsKey(new Vector2Int(x, y)))
                                    {
                                        profilDuChunk = ChunkManager.Instance.activeSpawns[new Vector2Int(x, y)];
                                    }
                                    spawner.Init(new Vector2Int(x, y), idDuChunk, profilDuChunk);
                                }
                            }
                        }

                        // 4. GESTION DES INDICATEURS DE PORTE (Par-dessus le sol !)
                        if (indicateurPortePrefab != null && ChunkManager.Instance != null)
                        {
                            foreach (GlobalSocket door in ChunkManager.Instance.allDoors)
                            {
                                if (door.position.x == x && door.position.y == y)
                                {
                                    // On vérifie si la porte donne sur le vide
                                    Vector2Int dirVec = ChunkManager.Instance.GetVectorFromDirection(door.direction);
                                    Vector2Int facingPos = door.position + dirVec;
                                    bool isConnected = false;
                                    
                                    if (facingPos.x >= 0 && facingPos.x < gridData.largeur && facingPos.y >= 0 && facingPos.y < gridData.hauteur)
                                    {
                                        if (gridData.Grid[facingPos.x, facingPos.y].isActive) isConnected = true;
                                    }

                                    if (!isConnected)
                                    {
                                        float rotY = 0f;
                                        switch (door.direction)
                                        {
                                            case SocketDirection.Nord: rotY = 0f + offsetRotationPorte; break;
                                            case SocketDirection.Est: rotY = 90f + offsetRotationPorte; break;
                                            case SocketDirection.Sud: rotY = 180f + offsetRotationPorte; break;
                                            case SocketDirection.Ouest: rotY = 270f + offsetRotationPorte; break;
                                        }

                                        Vector3 indPos = new Vector3(position.x, position.y + hauteurIndicateur, position.z);
                                        Quaternion indRot = Quaternion.Euler(90f, rotY, 0f);

                                        // On utilise le PoolManager pour ne pas saturer la mémoire
                                        GameObject indicateur = PoolManager.Instance.Spawn(indicateurPortePrefab, indPos, indRot);
                                        indicateursActifs.Add(indicateur);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (falaisePrefab != null)
                        {
                            GameObject falaise = Instantiate(falaisePrefab, position, falaisePrefab.transform.rotation, transform);
                            objetsFalaises.Add(falaise);
                        }
                    }
                }
            }  
        }  
    }  

    public void DrawPath(List<Node> path)  
    {  
        foreach (GameObject objet in objetsChemin) 
        { 
            if (objet.TryGetComponent<PooledItem>(out PooledItem poolItem))
            {
                poolItem.ReleaseToPool();
            }
            else
            {
                Destroy(objet); 
            }
        }
        objetsChemin.Clear();

        if (tuilesSolMemoire != null)
        {
            foreach (GameObject tuile in tuilesSolMemoire) { if (tuile != null) tuile.SetActive(true); }
        }

        if (LevelManager.Instance == null || path == null) return;

        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>();
        foreach (Node n in path) 
        {
            pathSet.Add(new Vector2Int(n.gridX, n.gridY));
        }

        foreach(var spawn in LevelManager.Instance.spawnPoints) { pathSet.Add(spawn); }
        foreach(var baseCoord in LevelManager.Instance.GetBaseCoords()) { pathSet.Add(baseCoord); }

        foreach (Node n in path)  
        {  
            if (LevelManager.Instance.IsStartOrEnd(new Vector2Int(n.gridX, n.gridY))) continue;  

            Vector2Int pos = new Vector2Int(n.gridX, n.gridY);
            
            int bitmask = 0;
            if (pathSet.Contains(pos + new Vector2Int(0, 1))) bitmask += 1;  
            if (pathSet.Contains(pos + new Vector2Int(1, 0))) bitmask += 2;  
            if (pathSet.Contains(pos + new Vector2Int(0, -1))) bitmask += 4; 
            if (pathSet.Contains(pos + new Vector2Int(-1, 0))) bitmask += 8; 

            GameObject prefabToInstantiate = tilePathStraight; 
            float rotationY = 0f;
            float currentOffset = 0f; 

            switch (bitmask)
            {
                case 1: case 4: case 5: prefabToInstantiate = tilePathStraight; rotationY = 90f; currentOffset = offsetStraight; break;
                case 2: case 8: case 10: prefabToInstantiate = tilePathStraight; rotationY = 0f; currentOffset = offsetStraight; break;
                case 3: prefabToInstantiate = tilePathCorner; rotationY = 0f; currentOffset = offsetCorner; break;
                case 6: prefabToInstantiate = tilePathCorner; rotationY = 90f; currentOffset = offsetCorner; break;
                case 12: prefabToInstantiate = tilePathCorner; rotationY = 180f; currentOffset = offsetCorner; break;
                case 9: prefabToInstantiate = tilePathCorner; rotationY = 270f; currentOffset = offsetCorner; break;
                case 7: prefabToInstantiate = tilePathT; rotationY = 0f; currentOffset = offsetT; break;
                case 14: prefabToInstantiate = tilePathT; rotationY = 90f; currentOffset = offsetT; break;
                case 13: prefabToInstantiate = tilePathT; rotationY = 180f; currentOffset = offsetT; break;
                case 11: prefabToInstantiate = tilePathT; rotationY = 270f; currentOffset = offsetT; break;
                case 15: prefabToInstantiate = tilePathCross; rotationY = 0f; currentOffset = offsetCross; break;
            }

            if (prefabToInstantiate != null && tuilesSolMemoire[n.gridX, n.gridY] != null) 
            {
                Vector3 position = GetWorldPosition(n.gridX, n.gridY);
                position.y = transform.position.y + (n.elevation * hauteurNiveau) + 0.01f; 
                
                Vector3 rotationDOrigine = prefabToInstantiate.transform.eulerAngles;
                float rotationFinaleY = rotationY + rotationDOrigine.y + currentOffset;
                Quaternion rotation = Quaternion.Euler(rotationDOrigine.x, rotationFinaleY, rotationDOrigine.z);
                
                GameObject cheminObj = PoolManager.Instance.Spawn(prefabToInstantiate, position, rotation);
                cheminObj.transform.SetParent(transform); 
                
                objetsChemin.Add(cheminObj);

                tuilesSolMemoire[n.gridX, n.gridY].SetActive(false);
            }
        }  
    }  

    private Vector3 GetWorldPosition(int gridX, int gridY)
    {
        Vector3 origineGrille = transform.position;
        float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
        float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
        
        return new Vector3(worldX, origineGrille.y, worldZ);
    }
}