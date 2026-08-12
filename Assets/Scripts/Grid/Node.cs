public class Node  
{  
    public bool isWalkable;  
    public int gridX;  
    public int gridY;  
    public float elevation; 

    public Node(int x, int y, bool walkable, float startElevation = 0f)  
    {  
        gridX = x;  
        gridY = y;  
        isWalkable = walkable;  
        elevation = startElevation;
    }  
}