using System.Collections.Generic;  
using UnityEngine;  

/* 
 * RÉSUMÉ : Traduit la grille virtuelle mathématique en éléments visuels 3D concrets à l'écran grâce à des Prefabs.
 */
[RequireComponent(typeof(GridData))]  
public class GridVisualizer : MonoBehaviour  
{  
    private GridData gridData;  

    [Header("Configuration Décors 3D")]  
    [Tooltip("Le modèle 3D (Prefab) utilisé pour le sol de base.")]
    public GameObject tuileSolPrefab;  

    [Header("Configuration Interface 3D")]  
    public GameObject tuileEntreePrefab;  
    public GameObject tuileSortiePrefab;  
    [Tooltip("Le modèle 3D utilisé pour tracer le chemin de l'IA.")]
    public GameObject tuileCheminPrefab;  

    // Liste pour garder une trace des éléments du chemin et pouvoir les effacer
    private List<GameObject> objetsChemin = new List<GameObject>();

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

        for (int x = 0; x < gridData.largeur; x++)  
        {  
            for (int y = 0; y < gridData.hauteur; y++)  
            {  
                Vector3 position = GetWorldPosition(x, y);
                
                // On surélève légèrement le sol visuel (0.01f) pour qu'il soit juste au-dessus du "Ground" invisible
                position.y += 0.01f; 

                GameObject prefabADessiner = tuileSolPrefab;

                // On vérifie si la case actuelle est l'entrée ou la sortie
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
                    Instantiate(prefabADessiner, position, prefabADessiner.transform.rotation, transform);
                }
            }  
        }  
    }  

    public void DrawPath(List<Node> path)  
    {  
        //On nettoie l'ancien chemin visuel
        foreach (GameObject objet in objetsChemin)
        {
            Destroy(objet);
        }
        objetsChemin.Clear();

        if (LevelManager.Instance == null || path == null) return;

        Vector2Int startCoords = LevelManager.Instance.startCoords;
        Vector2Int endCoords = LevelManager.Instance.endCoords;

        //On dessine le nouveau chemin
        foreach (Node n in path)  
        {  
            // On empêche le chemin de recouvrir visuellement l'entrée ou la sortie
            if ((n.gridX == startCoords.x && n.gridY == startCoords.y) ||  
                (n.gridX == endCoords.x && n.gridY == endCoords.y))  
                continue;  

            if (tuileCheminPrefab != null)
            {
                Vector3 position = GetWorldPosition(n.gridX, n.gridY);
                
                //On surélève l'indicateur pour éviter la superposition
                // ATTENTION j'ai mis des nombres au pif donc faudra un jour gérer l'odre de superpostition 
                position.y += 0.05f; 

                // On respecte la rotation d'origine du Prefab
                GameObject cheminObj = Instantiate(tuileCheminPrefab, position, tuileCheminPrefab.transform.rotation, transform);
                objetsChemin.Add(cheminObj);
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