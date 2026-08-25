using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    public Node[,] Grid;

    [Header("Configuration")]
    public float cellSize = 1f;
    
    [HideInInspector] public int largeur;
    [HideInInspector] public int hauteur;

    void OnEnable() { GameEvents.OnChunkUnlocked += UnlockChunk; }
    void OnDisable() { GameEvents.OnChunkUnlocked -= UnlockChunk; }

    public void Init()
    {
        // On récupère la taille géante du tapis de jeu (ex: 50x50)
        largeur = ChunkManager.Instance.globalGridSize;
        hauteur = ChunkManager.Instance.globalGridSize;
        
        GenerateGridData(); 
    }

    void GenerateGridData()
    {
        Grid = new Node[largeur, hauteur];

        for (int y = 0; y < hauteur; y++)
        {
            for (int x = 0; x < largeur; x++)
            {
                // On lit l'élévation "cuite" par le ChunkManager
                int bakedElevation = ChunkManager.Instance.bakedElevation[x, y];
                int bakedChunkId = ChunkManager.Instance.bakedChunkIds[x, y];

                // Si c'est du vide (-1), on le met à l'élévation 0 mais Non-Marchable
                float nodeElevation = (bakedElevation == -1) ? 0f : (float)bakedElevation;
                bool isWalkable = (bakedElevation != -1);

                Node newNode = new Node(x, y, isWalkable, nodeElevation, bakedChunkId);
                
                // Au début, on n'active que le Chunk n°1 (La Base posée par défaut)
                if (bakedChunkId == 1) newNode.isActive = true;

                Grid[x, y] = newNode;
            }
        }
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            int checkX = node.gridX + dir.x;
            int checkY = node.gridY + dir.y;

            if (checkX >= 0 && checkX < largeur && checkY >= 0 && checkY < hauteur)
            {
                Node targetNode = Grid[checkX, checkY];
                if (targetNode.isWalkable && targetNode.isActive)
                {
                    neighbors.Add(targetNode);
                }
            }
        }
        return neighbors;
    }
    
    // On débloque un Chunk ID (Utilisé quand le WaveManager déclenche un "Push")
    public void UnlockChunk(int chunkIdToUnlock)
    {
        bool chunkFound = false;
        for (int y = 0; y < hauteur; y++)
        {
            for (int x = 0; x < largeur; x++)
            {
                if (Grid[x, y].chunkId == chunkIdToUnlock && !Grid[x, y].isActive)
                {
                    Grid[x, y].isActive = true;
                    chunkFound = true;
                }
            }
        }
        
        if (chunkFound)
        {
            Debug.Log($"[GridData] Le Bloc {chunkIdToUnlock} est sorti de terre !");
            GameEvents.TriggerMapExpanded(); 
            GameEvents.TriggerGridChanged(); 
        }
    }
}