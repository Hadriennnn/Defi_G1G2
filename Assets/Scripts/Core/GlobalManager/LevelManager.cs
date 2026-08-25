using UnityEngine;
using System.Collections.Generic; 

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public List<Vector2Int> spawnPoints => new List<Vector2Int>(ChunkManager.Instance.activeSpawns.Keys);
    public List<Vector2Int> GetBaseCoords()
    {
        Vector2Int root = ChunkManager.Instance.baseNoyauCoords;
        return new List<Vector2Int>
        {
            root,                                     // En bas à gauche
            root + new Vector2Int(1, 0),              // En bas à droite
            root + new Vector2Int(0, 1),              // En haut à gauche
            root + new Vector2Int(1, 1)               // En haut à droite
        };
    }

    public Dictionary<Vector2Int, List<Vector3>> WaypointsParSpawn { get; private set; } = new Dictionary<Vector2Int, List<Vector3>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsStartOrEnd(Vector2Int coords)
    {
        // On vérifie si la case cliquée fait partie des 4 cases de la base
        if (GetBaseCoords().Contains(coords)) return true;
        return spawnPoints.Contains(coords);
    }
    
    public void EnregistrerWaypoints(Vector2Int spawnPoint, List<Vector3> nouveauxWaypoints)
    {
        WaypointsParSpawn[spawnPoint] = nouveauxWaypoints;
    }
}