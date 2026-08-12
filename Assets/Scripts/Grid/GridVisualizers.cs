using System.Collections.Generic;  
using UnityEngine;  

/* 
 * RÉSUMÉ : Traduit la grille virtuelle mathématique en éléments visuels 3D concrets à l'écran grâce à des Prefabs.
 * Gère l'Auto-Tiling directionnel pour le chemin et masque le sol en dessous pour éviter le Z-Fighting.
 */
[RequireComponent(typeof(GridData))]  
public class GridVisualizer : MonoBehaviour  
{  
    private GridData gridData;  

    [Header("Configuration Décors 3D")]  
    [Tooltip("Le modèle 3D (Prefab) utilisé pour le sol de base.")]
    public GameObject tuileSolPrefab;  

    [Header("Kit de Tuiles Modulaires (Auto-Tiling)")]  
    public GameObject tuileEntreePrefab;  
    public GameObject tuileSortiePrefab;  
    
    [Space(10)]
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

    // Liste pour garder une trace des éléments du chemin et pouvoir les effacer
    private List<GameObject> objetsChemin = new List<GameObject>();
    
    // NOUVEAU : Tableau pour mémoriser toutes les cases de sol (pour les masquer/afficher dynamiquement)
    private GameObject[,] tuilesSolMemoire;

    public void Init()  
    {  
        gridData = GetComponent<GridData>();  
        DrawBaseGrid();
    }  

    private void OnEnable()  
    {  
        // On s'abonne aux ordres du cerveau  
        GameEvents.OnPathUpdated += DrawPath;  
    }  

    private void OnDisable()  
    {  
        GameEvents.OnPathUpdated -= DrawPath;  
    }  

    void DrawBaseGrid()  
    {  
        // Récupération dynamique des positions depuis le LevelManager
        Vector2Int startCoords = LevelManager.Instance.startCoords;
        Vector2Int endCoords = LevelManager.Instance.endCoords;

        // On initialise notre tableau de mémoire
        tuilesSolMemoire = new GameObject[gridData.largeur, gridData.hauteur];

        for (int x = 0; x < gridData.largeur; x++)  
        {  
            for (int y = 0; y < gridData.hauteur; y++)  
            {  
                Vector3 position = GetWorldPosition(x, y);
                
                GameObject prefabADessiner = tuileSolPrefab;
                float elevationOffset = 0.01f; // Par défaut, le sol normal est à +0.01f

                // On vérifie si la case actuelle est l'entrée ou la sortie
                if (x == startCoords.x && y == startCoords.y && tuileEntreePrefab != null)
                {
                    prefabADessiner = tuileEntreePrefab;
                    elevationOffset = 0.01f; // On surélève le départ
                }
                else if (x == endCoords.x && y == endCoords.y && tuileSortiePrefab != null)
                {
                    prefabADessiner = tuileSortiePrefab;
                    elevationOffset = 0.01f; // On surélève l'arrivée
                }

                // On applique l'élévation spécifique à cette tuile
                position.y += elevationOffset; 

                if (prefabADessiner != null)
                {
                    GameObject solInstancie = Instantiate(prefabADessiner, position, prefabADessiner.transform.rotation, transform);
                    
                    // On stocke la case instanciée dans notre mémoire pour pouvoir la manipuler plus tard
                    tuilesSolMemoire[x, y] = solInstancie;
                }
            }  
        }  
    }  

    public void DrawPath(List<Node> path)  
    {  
        // 1. On nettoie l'ancien chemin visuel
        foreach (GameObject objet in objetsChemin)
        {
            Destroy(objet);
        }
        objetsChemin.Clear();

        // 2. On réactive toutes les cases d'herbe qui avaient été cachées par l'ancien chemin
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

        // 3. Création d'un "Set" pour vérifier super rapidement (sans boucle) si une case fait partie du chemin
        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>();
        foreach (Node n in path) 
        {
            pathSet.Add(new Vector2Int(n.gridX, n.gridY));
        }
        
        // On ajoute artificiellement le point de départ et d'arrivée pour que les tuiles s'y connectent joliment
        pathSet.Add(startCoords);
        pathSet.Add(endCoords);

        // 4. Calcul du Bitmask pour chaque case
        foreach (Node n in path)  
        {  
            // On empêche le chemin de recouvrir visuellement l'entrée ou la sortie
            if ((n.gridX == startCoords.x && n.gridY == startCoords.y) ||  
                (n.gridX == endCoords.x && n.gridY == endCoords.y))  
                continue;  

            Vector2Int pos = new Vector2Int(n.gridX, n.gridY);
            
            // On vérifie les voisins : Nord = 1, Est = 2, Sud = 4, Ouest = 8
            int bitmask = 0;
            if (pathSet.Contains(pos + new Vector2Int(0, 1))) bitmask += 1;  // Nord
            if (pathSet.Contains(pos + new Vector2Int(1, 0))) bitmask += 2;  // Est
            if (pathSet.Contains(pos + new Vector2Int(0, -1))) bitmask += 4; // Sud
            if (pathSet.Contains(pos + new Vector2Int(-1, 0))) bitmask += 8; // Ouest

            GameObject prefabToInstantiate = tilePathStraight; // Modèle par défaut
            float rotationY = 0f;
            float currentOffset = 0f; // L'offset spécifique à la tuile en cours

            // Choix de la tuile, de sa rotation logique ET de son offset correctif
            switch (bitmask)
            {
                // Lignes droites et culs-de-sac
                case 1: case 4: case 5: 
                    prefabToInstantiate = tilePathStraight; 
                    rotationY = 90f; 
                    currentOffset = offsetStraight;
                    break;
                case 2: case 8: case 10: 
                    prefabToInstantiate = tilePathStraight; 
                    rotationY = 0f; 
                    currentOffset = offsetStraight;
                    break;

                // Virages à 90 degrés
                case 3: 
                    prefabToInstantiate = tilePathCorner; 
                    rotationY = 0f; 
                    currentOffset = offsetCorner;
                    break;
                case 6: 
                    prefabToInstantiate = tilePathCorner; 
                    rotationY = 90f; 
                    currentOffset = offsetCorner;
                    break;
                case 12: 
                    prefabToInstantiate = tilePathCorner; 
                    rotationY = 180f; 
                    currentOffset = offsetCorner;
                    break;
                case 9: 
                    prefabToInstantiate = tilePathCorner; 
                    rotationY = 270f; 
                    currentOffset = offsetCorner;
                    break;

                // Intersections en T
                case 7: 
                    prefabToInstantiate = tilePathT; 
                    rotationY = 0f; 
                    currentOffset = offsetT;
                    break;
                case 14: 
                    prefabToInstantiate = tilePathT; 
                    rotationY = 90f; 
                    currentOffset = offsetT;
                    break;
                case 13: 
                    prefabToInstantiate = tilePathT; 
                    rotationY = 180f; 
                    currentOffset = offsetT;
                    break;
                case 11: 
                    prefabToInstantiate = tilePathT; 
                    rotationY = 270f; 
                    currentOffset = offsetT;
                    break;

                // Croisements 4 voies
                case 15: 
                    prefabToInstantiate = tilePathCross; 
                    rotationY = 0f; 
                    currentOffset = offsetCross;
                    break;
            }

            // 5. Instanciation visuelle
            if (prefabToInstantiate != null)
            {
                Vector3 position = GetWorldPosition(n.gridX, n.gridY);
                
                // On met le chemin EXACTEMENT à la même hauteur que le sol de base
                position.y += 0.01f; 
                
                Vector3 rotationDOrigine = prefabToInstantiate.transform.eulerAngles;
                
                // On applique l'offset spécifique à CETTE tuile
                float rotationFinaleY = rotationY + rotationDOrigine.y + currentOffset;
                
                // On préserve les axes X et Z d'origine
                Quaternion rotation = Quaternion.Euler(rotationDOrigine.x, rotationFinaleY, rotationDOrigine.z);
                
                GameObject cheminObj = Instantiate(prefabToInstantiate, position, rotation, transform);
                objetsChemin.Add(cheminObj);

                // NOUVEAU : On masque la case d'herbe qui se trouve en dessous pour éviter le Z-Fighting
                if (tuilesSolMemoire[n.gridX, n.gridY] != null)
                {
                    tuilesSolMemoire[n.gridX, n.gridY].SetActive(false);
                }
            }
        }  
    }  

    // Fonction utilitaire pour convertir X/Y (mathématique) en X/Z (physique)
    private Vector3 GetWorldPosition(int gridX, int gridY)
    {
        Vector3 origineGrille = transform.position;
        float worldX = origineGrille.x + (gridX * gridData.cellSize) + (gridData.cellSize / 2f);
        float worldZ = origineGrille.z + (gridY * gridData.cellSize) + (gridData.cellSize / 2f);
        
        return new Vector3(worldX, origineGrille.y, worldZ);
    }

    private void OnDrawGizmos()  
    {  
        if (gridData == null) gridData = GetComponent<GridData>();  
        if (gridData == null) return;  
        
        Gizmos.color = Color.white;  
        Vector3 origin = transform.position;  
        
        for (int i = 0; i <= gridData.largeur; i++)  
            Gizmos.DrawLine(origin + new Vector3(gridData.cellSize * i, 0, 0), origin + new Vector3(gridData.cellSize * i, 0, gridData.hauteur * gridData.cellSize));  
            
        for (int j = 0; j <= gridData.hauteur; j++)  
            Gizmos.DrawLine(origin + new Vector3(0, 0, gridData.cellSize * j), origin + new Vector3(gridData.largeur * gridData.cellSize, 0, gridData.cellSize * j));  
    }  
}