using UnityEngine;
using System.Collections.Generic; // N'oublie pas l'import pour la List

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Configuration du Niveau")]
    public LevelConfig currentLevel; // ON INJECTE NOTRE FICHIER ICI

    // On cache les variables dans l'inspecteur car elles viennent du LevelConfig
    [HideInInspector] public Vector2Int startCoords;  
    [HideInInspector] public Vector2Int endCoords;  

    public List<Vector3> Waypoints { get; private set; } = new List<Vector3>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // On charge les données du ScriptableObject
            if (currentLevel != null)
            {
                startCoords = currentLevel.startCoords;
                endCoords = currentLevel.endCoords;
            }
            else
            {
                Debug.LogError("[LevelManager] Aucun LevelConfig n'est assigné !");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsStartOrEnd(Vector2Int coords)
    {
        return coords == startCoords || coords == endCoords;
    }
    
    public void EnregistrerWaypoints(List<Vector3> nouveauxWaypoints)
    {
        Waypoints = nouveauxWaypoints;
        Debug.Log($"[LevelManager] Phase d'Action : {Waypoints.Count} Waypoints verrouillés !");
    }
}