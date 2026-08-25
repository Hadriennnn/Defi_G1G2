using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GridRow
{
    public int[] cells;
}

[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "TowerDefense/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Paramètres de la Grille (Max)")]
    public int largeur = 16;
    public int hauteur = 9;

    [Header("Points d'Ancrage")]
    public Vector2Int endCoords = new Vector2Int(15, 4);
    public List<Vector2Int> spawnPoints = new List<Vector2Int>(); 

    [Header("Carte des Hauteurs (0 = Chemin, 1+ = Montagnes)")]
    public GridRow[] elevationMap;

    [Header("Carte des Blocs (1 = Bloc A, 2 = Bloc B, 0 = Vide)")]
    public GridRow[] chunkMap;

    [ContextMenu("Générer la grille (Auto-fill)")]
    public void GenererGrilleVide()
    {
        elevationMap = new GridRow[hauteur];
        chunkMap = new GridRow[hauteur]; 

        for (int y = 0; y < hauteur; y++)
        {
            elevationMap[y] = new GridRow();
            elevationMap[y].cells = new int[largeur];
            
            chunkMap[y] = new GridRow();
            chunkMap[y].cells = new int[largeur];
            
            for (int x = 0; x < largeur; x++)
            {
                elevationMap[y].cells[x] = 0;
                chunkMap[y].cells[x] = 1; 
            }
        }
        
        if (spawnPoints.Count == 0) spawnPoints.Add(new Vector2Int(0, 4));

        Debug.Log($"[LevelConfig] Grille de {largeur}x{hauteur} générée avec succès !");
    }
}