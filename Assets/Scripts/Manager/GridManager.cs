using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Node[,] Grid;
    public List<Node> path;
    
    [Header("Configuration de la grille")]
    public int largeur = 16;
    public int hauteur = 9;
    public float cellSize = 1f;

    [Header("entrée et sortie")]
    [Range(0, 15)] public int entreeX = 0;
    [Range(0, 8)]  public int entreeY = 4;
    [Space]
    [Range(0, 15)] public int sortieX = 15;
    [Range(0, 8)]  public int sortieY = 4;

    [Header("Configuration Tilemap (Décors)")]
    public Tilemap solTilemap;         
    public Tilemap obstaclesTilemap;   
    public TileBase tuileSol;          
    public TileBase tuileBarricade;    

    [Header("Configuration Tilemap (Interface)")]
    public Tilemap cheminTilemap;      
    public TileBase tuileEntree;      
    public TileBase tuileSortie;       
    public TileBase tuileChemin;     

    public Vector2Int startCoords => new Vector2Int(entreeX, entreeY);
    public Vector2Int endCoords => new Vector2Int(sortieX, sortieY);
    
    void Awake()
    {
        GenerateGrid(); 
    }

    private void Start()
    {
        GetComponent<Pathfinding>().FindPath(startCoords, endCoords);
    }

    void GenerateGrid()
    {
        Grid = new Node[largeur, hauteur];
        solTilemap.ClearAllTiles(); 
        obstaclesTilemap.ClearAllTiles();
        cheminTilemap.ClearAllTiles(); // On nettoie aussi le calque du chemin

        for (int x = 0; x < largeur; x++)
        {
            for (int y = 0; y < hauteur; y++)
            {
                Grid[x, y] = new Node(x, y, true);
                solTilemap.SetTile(new Vector3Int(x, y, 0), tuileSol);
            }
        }
    }
    
    
    public void UpdatePathVisuals()
    {
        // 1. On efface l'ancien chemin
        cheminTilemap.ClearAllTiles();

        // 2. On dessine l'Entrée et la Sortie
        cheminTilemap.SetTile(new Vector3Int(entreeX, entreeY, 0), tuileEntree);
        cheminTilemap.SetTile(new Vector3Int(sortieX, sortieY, 0), tuileSortie);

        // 3. On dessine les cases du chemin (sauf si c'est la case de départ/arrivée )
        if (path != null)
        {
            foreach (Node n in path)
            {
                if ((n.gridX == entreeX && n.gridY == entreeY) || (n.gridX == sortieX && n.gridY == sortieY))
                    continue;

                cheminTilemap.SetTile(new Vector3Int(n.gridX, n.gridY, 0), tuileChemin);
            }
        }
    }

    
    public List<Node> GetNeighbors(Node node)  
    {  
        List<Node> neighbors = new List<Node>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Haut
            new Vector2Int(0, -1), // Bas
            new Vector2Int(1, 0),  // Droite
            new Vector2Int(-1, 0)  // Gauche
        };

        foreach (Vector2Int dir in directions)
        {
            int checkX = node.gridX + dir.x;
            int checkY = node.gridY + dir.y;

            // 1. On vérifie qu'on ne sort pas des limites de la grille
            if (checkX >= 0 && checkX < largeur && checkY >= 0 && checkY < hauteur)
            {
                // 2. On récupère la case cible et on s'assure qu'elle est walkable
                Node targetNode = Grid[checkX, checkY];  
                if (targetNode.isWalkable) 
                {
                    neighbors.Add(targetNode);
                }
            }
        }
        
        return neighbors;  
    }

    private void OnDrawGizmos()
    {
        // On ne garde que les lignes blanches pour debuggage
        Gizmos.color = Color.white;
        Vector3 origin = transform.position;

        for (int i = 0; i <= largeur; i++) 
            Gizmos.DrawLine(origin + new Vector3(cellSize * i, 0, 0), origin + new Vector3(cellSize * i, hauteur * cellSize, 0));
            
        for (int j = 0; j <= hauteur; j++) 
            Gizmos.DrawLine(origin + new Vector3(0, cellSize * j, 0), origin + new Vector3(largeur * cellSize, cellSize * j, 0));
    }
}