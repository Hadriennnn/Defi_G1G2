public class Node  
{  
    public bool isWalkable;  
    public int gridX;  
    public int gridY;  

    // Constructeur pur, aucune variable A* ici !
    public Node(int x, int y, bool walkable)  
    {  
        gridX = x;  
        gridY = y;  
        isWalkable = walkable;  
    }  
}