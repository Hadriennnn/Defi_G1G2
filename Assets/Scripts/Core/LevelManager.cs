using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Points d'Ancrage du Niveau")]  
    public Vector2Int startCoords = new Vector2Int(0, 4);  
    public Vector2Int endCoords = new Vector2Int(15, 4);  

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Fonction utilitaire pour savoir si une coordonnée correspond à un ancrage
    public bool IsStartOrEnd(Vector2Int coords)
    {
        return coords == startCoords || coords == endCoords;
    }
}