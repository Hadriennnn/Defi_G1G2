using UnityEngine;
using System.Collections.Generic;

public enum SocketDirection { Nord, Sud, Est, Ouest, Aucune }

// On définit clairement le RÔLE du point d'intérêt
public enum SocketType 
{ 
    Porte_Entree,  // Bordure : Le chemin vient d'un autre Chunk
    Porte_Sortie,  // Bordure : Le chemin part vers un autre Chunk
    Portail_Spawn, // N'importe où : Les ennemis apparaissent 
    Noyau_Base     // N'importe où : La base du joueur 
}

[System.Serializable]
public class ChunkSocket
{
    public Vector2Int localPosition;
    
    [Tooltip("La direction vers laquelle ce socket 'regarde'. Utile uniquement pour les Portes.")]
    public SocketDirection direction;
    
    public SocketType type;
}

[System.Serializable]
public class ChunkRow
{
    public int[] cells;
}

[CreateAssetMenu(fileName = "NewChunkData", menuName = "TowerDefense/Terrain/Chunk Data")]
public class ChunkData : ScriptableObject
{
    [Header("Dimensions du Chunk")]
    public int largeur = 5;
    public int hauteur = 5;

    [Header("Points d'Intérêt (Sockets / Spawns / Base)")]
    public List<ChunkSocket> sockets = new List<ChunkSocket>();

    [HideInInspector]
    public ChunkRow[] elevationMap;

    public void GenererGrilleVide()
    {
        elevationMap = new ChunkRow[hauteur];
        for (int y = 0; y < hauteur; y++)
        {
            elevationMap[y] = new ChunkRow();
            elevationMap[y].cells = new int[largeur];
            
            for (int x = 0; x < largeur; x++)
            {
                elevationMap[y].cells[x] = 0; 
            }
        }
        Debug.Log($"[ChunkData] Grille locale de {largeur}x{hauteur} générée.");
    }
}