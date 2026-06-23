using UnityEngine;

public class Node 
{
    public bool isWalkable; 
    public int gridX;       
    public int gridY;       

    //  A*
    public int gCost; // Coût depuis le point de départ
    public int hCost; // Coût estimé jusqu'à l'arrivée
    public int fCost => gCost + hCost; // Coût total 

    public Node parent; // Le Node précédent dans le chemin final

    // 🏗Constructeur
    public Node(int x, int y, bool walkable) 
    {
        gridX = x;
        gridY = y;
        isWalkable = walkable;
    }
}