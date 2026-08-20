using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    public Node[,] Grid;

    [Header("Configuration")]
    public LevelConfig currentLevel; // ON INJECTE LE FICHIER ICI AUSSI
    public float cellSize = 1f;
    
    [HideInInspector] public int largeur;
    [HideInInspector] public int hauteur;

    public void Init()
    {
        if (currentLevel != null)
        {
            largeur = currentLevel.largeur;
            hauteur = currentLevel.hauteur;
            GenerateGridData();
        }
        else
        {
            Debug.LogError("[GridData] Aucun LevelConfig assigné !");
        }
    }

    void GenerateGridData()
    {
        Grid = new Node[largeur, hauteur];
        
        for (int y = 0; y < hauteur; y++)
        {
            for (int x = 0; x < largeur; x++)
            {
                float nodeElevation = 0f;

                // On vérifie notre tableau dans l'inspecteur pour éviter les crashs
                if (currentLevel.elevationMap != null && 
                    y < currentLevel.elevationMap.Length && 
                    x < currentLevel.elevationMap[y].cells.Length)
                {
                    // On lit la hauteur (0 ou 1) dans notre fichier de niveau
                    nodeElevation = currentLevel.elevationMap[y].cells[x];
                }

                // On crée le noeud avec la bonne élévation
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