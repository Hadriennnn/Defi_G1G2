using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    public Node[,] Grid;

    [Header("Configuration de la grille")]
    public int largeur = 16;
    public int hauteur = 9;
    public float cellSize = 1f;
    

    public void Init()
    {
        GenerateGridData();
    }

    void GenerateGridData()
    {
        Grid = new Node[largeur, hauteur];
        
        for (int x = 0; x < largeur; x++)
        {
            for (int y = 0; y < hauteur; y++)
            {
                Grid[x, y] = new Node(x, y, true);
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