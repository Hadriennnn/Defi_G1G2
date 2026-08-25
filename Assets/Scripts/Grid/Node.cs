public class Node  
{  
    public bool isWalkable;  
    public int gridX;  
    public int gridY;  
    public float elevation; 
    
    public int chunkId;      
    public bool isActive;    

    public Node(int x, int y, bool walkable, float startElevation = 0f, int chunk = 1)  
    {  
        gridX = x;  
        gridY = y;  
        isWalkable = walkable;  
        elevation = startElevation;
        
        chunkId = chunk;
        isActive = false; 
    }  
}