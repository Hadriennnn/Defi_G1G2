using System;  
using System.Collections.Generic;  
using UnityEngine;

public static class GameEvents  
{  
    public static event Action OnGridChanged;
    public static event Action<List<Node>> OnPathUpdated;
    public static event Action<Vector3Int, bool> OnObstacleToggled;
    public static event Action<GameState> OnGameStateChanged;
    
    public static event Action<int> OnChunkUnlocked;
    public static void TriggerChunkUnlocked(int chunkId) => OnChunkUnlocked?.Invoke(chunkId);

    public static event Action OnMapExpanded;
    public static void TriggerMapExpanded() => OnMapExpanded?.Invoke();

    public static void TriggerGameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
    public static event Action<CardData> OnCardSelected;
    public static void TriggerCardSelected(CardData card) => OnCardSelected?.Invoke(card);
    public static event Action OnCardDeselected;
    public static void TriggerCardDeselected() => OnCardDeselected?.Invoke();
    public static void TriggerGridChanged() => OnGridChanged?.Invoke();  
    public static void TriggerPathUpdated(List<Node> path) => OnPathUpdated?.Invoke(path);  
    public static void TriggerObstacleToggled(Vector3Int position, bool isPlaced) => OnObstacleToggled?.Invoke(position, isPlaced);  
    public static event Action OnEnemySpawned;
    public static event Action OnEnemyRemoved;
    public static void TriggerEnemySpawned() => OnEnemySpawned?.Invoke();
    public static void TriggerEnemyRemoved() => OnEnemyRemoved?.Invoke();
    
    //GESTION DES VAGUES
    public static event Action<int> OnWaveStarted;
    public static void TriggerWaveStarted(int waveNumber) => OnWaveStarted?.Invoke(waveNumber);

    public static event Action OnSpawnerFinished;
    public static void TriggerSpawnerFinished() => OnSpawnerFinished?.Invoke();
}

public enum GameState
{
    PreparationPhase,
    ActionPhase
}