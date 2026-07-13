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
    
    [Header("Données de Combat (Générées)")]
    public System.Collections.Generic.List<Vector3> Waypoints { get; private set; } = new System.Collections.Generic.List<Vector3>();

    // Fonction appelée par le Pathfinding au début de la phase d'action
    public void EnregistrerWaypoints(System.Collections.Generic.List<Vector3> nouveauxWaypoints)
    {
        Waypoints = nouveauxWaypoints;
        Debug.Log($"[LevelManager] Phase d'Action : {Waypoints.Count} Waypoints verrouillés pour la vague !");
    }
}