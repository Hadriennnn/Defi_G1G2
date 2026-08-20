using System.Collections.Generic;  
using UnityEngine;  

[RequireComponent(typeof(GridData))]  
public class GridVisualizer : MonoBehaviour  
{  
    private GridData gridData;  

    [Header("Configuration Verticalité")]
    [Tooltip("La hauteur exacte d'un bloc en unités Unity")]
    public float hauteurNiveau = 0.7f; 
    [Tooltip("Le préfab de falaise/corps utilisé sous la surface")]
    public GameObject falaisePrefab; 
    
    [Tooltip("Nombre de blocs générés en dessous du niveau 0 pour donner de l'épaisseur à la carte")]
    public int profondeurSousTerre = 2; // Par défaut on descend jusqu'à -2 (soit 3 blocs d'épaisseur avec le niveau 0)

    [Header("Configuration Décors 3D (Surfaces)")]  
    public GameObject tuileSolPrefab;  
    public GameObject tuileEntreePrefab;  
    public GameObject tuileSortiePrefab;  
    
    [Header("Kit de Tuiles Modulaires (Chemin)")]  
    public GameObject tilePathStraight;
    public float offsetStraight = 0f;
    
    [Space(5)]
    public GameObject tilePathCorner;   
    public float offsetCorner = 0f;
    
    [Space(5)]
    public GameObject tilePathT;        
    public float offsetT = 0f;
    
    [Space(5)]
    public GameObject tilePathCross;    
    public float offsetCross = 0f;

    private List<GameObject> objetsChemin = new List<GameObject>();
    private GameObject[,] tuilesSolMemoire;
    private List<GameObject> objetsFalaises = new List<GameObject>(); 

    public void Init()  
    {  
        gridData = GetComponent<GridData>();  
        DrawBaseGrid();
    }  

    private void OnEnable()  
    {  
        GameEvents.OnPathUpdated += DrawPath;  
    }  

    private void OnDisable()  
    {  
        GameEvents.OnPathUpdated -= DrawPath;  
    }  

    void DrawBaseGrid()  
    {  
        // Nettoyage complet des objets existants
        foreach (GameObject falaise in objetsFalaises) 
        { 
            if (falaise != null) Destroy(falaise); 
        }
        objetsFalaises.Clear();

        if (tuilesSolMemoire != null)
        {
            foreach (GameObject sol in tuilesSolMemoire)
            {
                if (sol != null) Destroy(sol);
            }
        }

        Vector2Int startCoords = LevelManager.Instance.startCoords;
        Vector2Int endCoords = LevelManager.Instance.endCoords;

        tuilesSolMemoire = new GameObject[gridData.largeur, gridData.hauteur];

        for (int x = 0; x < gridData.largeur; x++)  
        {  
            for (int y = 0; y < gridData.hauteur; y++)  
            {  
                Node currentNode = gridData.Grid[x, y];
                int targetHeight = Mathf.Max(0, Mathf.RoundToInt(currentNode.elevation));

                // NOUVEAU : On commence la boucle en négatif sous la terre !
                for (int h = -profondeurSousTerre; h <= targetHeight; h++)
                {
                    Vector3 position = GetWorldPosition(x, y);
                    
                    // Le Y va automatiquement être négatif pour les blocs du dessous
                    position.y = transform.position.y + (h * hauteurNiveau);

                    bool isTopBlock = (h == targetHeight);

                    if (isTopBlock)
                    {
                        GameObject prefabADessiner = tuileSolPrefab;

                        if (x == startCoords.x && y == startCoords.y && tuileEntreePrefab != null)
                        {
                            prefabADessiner = tuileEntreePrefab;
                        }
                        else if (x == endCoords.x && y == endCoords.y && tuileSortiePrefab != null)
                        {
                            prefabADessiner = tuileSortiePrefab;
                        }

                        if (prefabADessiner != null)
                        {
                            GameObject solInstancie = Instantiate(prefabADessiner, position, prefabADessiner.transform.rotation, transform);
                            tuilesSolMemoire[x, y] = solInstancie;
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
            Destroy(objet);
        }
        objetsChemin.Clear();

        if (tuilesSolMemoire != null)
        {
            foreach (GameObject tuile in tuilesSolMemoire)
            {
                if (tuile != null) tuile.SetActive(true);
            }
        }

        if (LevelManager.Instance == null || path == null) return;

        Vector2Int startCoords = LevelManager.Instance.startCoords;
        Vector2Int endCoords = LevelManager.Instance.endCoords;

        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>();
        foreach (Node n in path) 
        {
            pathSet.Add(new Vector2Int(n.gridX, n.gridY));
        }
        
        pathSet.Add(startCoords);
        pathSet.Add(endCoords);

        foreach (Node n in path)  
        {  
            if ((n.gridX == startCoords.x && n.gridY == startCoords.y) ||  
                (n.gridX == endCoords.x && n.gridY == endCoords.y))  
                continue;  

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

            if (prefabToInstantiate != null)
            {
                Vector3 position = GetWorldPosition(n.gridX, n.gridY);
                position.y = transform.position.y + (n.elevation * hauteurNiveau) + 0.01f; 
                
                Vector3 rotationDOrigine = prefabToInstantiate.transform.eulerAngles;
                float rotationFinaleY = rotationY + rotationDOrigine.y + currentOffset;
                Quaternion rotation = Quaternion.Euler(rotationDOrigine.x, rotationFinaleY, rotationDOrigine.z);
                
                GameObject cheminObj = Instantiate(prefabToInstantiate, position, rotation, transform);
                objetsChemin.Add(cheminObj);

                if (tuilesSolMemoire[n.gridX, n.gridY] != null)
                {
                    tuilesSolMemoire[n.gridX, n.gridY].SetActive(false);
                }
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