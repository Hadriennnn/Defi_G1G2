using UnityEngine;
using System.Collections.Generic;

public enum SocketDirection { Nord, Sud, Est, Ouest, Aucune }

public enum SocketType 
{ 
    Porte_Entree,  
    Porte_Sortie,  
    Portail_Spawn, 
    Noyau_Base     
}

// NOUVEAU : Catégorie du Chunk pour le tri automatique
public enum ChunkCategory 
{ 
    Standard,        // Chunk d'expansion normale (Labyrinthe, Chemin)
    BaseDeDepart,    // Tiré au sort au début du jeu (doit contenir un Noyau_Base)
    Exclu            // Tests, Boss, etc. Ne sera jamais utilisé automatiquement.
}

[System.Serializable]
public class ChunkSocket
{
    public Vector2Int localPosition;
    
    [Tooltip("La direction vers laquelle ce socket 'regarde'. Utile uniquement pour les Portes.")]
    public SocketDirection direction;
    
    public SocketType type;

    [Tooltip("À remplir uniquement si c'est un Portail_Spawn. Laisse vide pour l'IA Director.")]
    public SpawnerProfile spawnerProfile; 
}

[System.Serializable]
public class ChunkRow
{
    public int[] cells;
}

[CreateAssetMenu(fileName = "NewChunkData", menuName = "TowerDefense/Terrain/Chunk Data")]
public class ChunkData : ScriptableObject
{
    [Header("Catégorisation (Outils auto)")]
    [Tooltip("Détermine dans quelle liste l'outil automatique va ranger ce Chunk.")]
    public ChunkCategory category = ChunkCategory.Standard; 

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