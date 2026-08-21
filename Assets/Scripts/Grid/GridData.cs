using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    public Node[,] Grid;

    [Header("Configuration")]
    public float cellSize = 1f;
    
    [HideInInspector] public int largeur;
    [HideInInspector] public int hauteur;

    public void Init()
    {
        LevelConfig currentLevel = LevelManager.Instance.currentLevel;

        if (currentLevel != null)
        {
            largeur = currentLevel.largeur;
            hauteur = currentLevel.hauteur;
            GenerateGridData(currentLevel); 
        }
        else
        {
            Debug.LogError("[GridData] Impossible de générer la grille : LevelManager n'a pas de niveau !");
        }
    }

    void GenerateGridData(LevelConfig currentLevel)
    {
        Grid = new Node[largeur, hauteur];

        for (int y = 0; y < hauteur; y++)
        {
            for (int x = 0; x < largeur; x++)
            {
                float nodeElevation = 0f;

                if (currentLevel.elevationMap != null &&
                    y < currentLevel.elevationMap.Length &&
                    x < currentLevel.elevationMap[y].cells.Length)
                {
                    nodeElevation = currentLevel.elevationMap[y].cells[x];
                }

                Grid[x, y] = new Node(x, y, true, nodeElevation);
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
                if (targetNode.isWalkable)
                {
                    neighbors.Add(targetNode);
                }
            }
        }
        return neighbors;
    }
}