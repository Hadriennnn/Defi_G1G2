using UnityEngine;

[System.Serializable]
public class GridRow
{
    public int[] cells;
}

[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "TowerDefense/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Paramètres de la Grille")]
    public int largeur = 16;
    public int hauteur = 9;

    [Header("Points d'Ancrage")]
    public Vector2Int startCoords = new Vector2Int(0, 4);
    public Vector2Int endCoords = new Vector2Int(15, 4);

    [Header("Carte des Hauteurs (0 = Normal, 1 = Surélevé)")]
    public GridRow[] elevationMap;

    
    [ContextMenu("Générer la grille (Auto-fill)")]
    public void GenererGrilleVide()
    {
        elevationMap = new GridRow[hauteur];
        for (int y = 0; y < hauteur; y++)
        {
            elevationMap[y] = new GridRow();
            elevationMap[y].cells = new int[largeur];
            
            for (int x = 0; x < largeur; x++)
            {
                elevationMap[y].cells[x] = 0;
            }
        }
        Debug.Log($"[LevelConfig] Grille de {largeur}x{hauteur} générée avec succès !");
    }
}