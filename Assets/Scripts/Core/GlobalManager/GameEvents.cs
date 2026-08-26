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
    
    // GESTION DES ENNEMIS
    public static event Action<EnemyCore> OnEnemySpawned;
    public static event Action<EnemyCore> OnEnemyRemoved;
    public static void TriggerEnemySpawned(EnemyCore enemy) => OnEnemySpawned?.Invoke(enemy);
    public static void TriggerEnemyRemoved(EnemyCore enemy) => OnEnemyRemoved?.Invoke(enemy);
    
    //GESTION DES VAGUES
    public static event Action<int> OnWaveStarted;
    public static void TriggerWaveStarted(int waveNumber) => OnWaveStarted?.Invoke(waveNumber);
    public static event Action<int> OnWaveNumberChanged;
    public static void TriggerWaveNumberChanged(int waveNumber) => OnWaveNumberChanged?.Invoke(waveNumber);

    public static event Action OnSpawnerFinished;
    public static void TriggerSpawnerFinished() => OnSpawnerFinished?.Invoke();
    
    // GESTION DE LA BASE ET GAME OVER
    public static event Action<int> OnBaseDamaged;
    public static void TriggerBaseDamaged(int damageAmount) => OnBaseDamaged?.Invoke(damageAmount);

    public static event Action<int, int> OnBaseHealthChanged;
    public static void TriggerBaseHealthChanged(int currentHealth, int maxHealth) => OnBaseHealthChanged?.Invoke(currentHealth, maxHealth);

    public static event Action OnGameOver;
    public static void TriggerGameOver() => OnGameOver?.Invoke();
}

public enum GameState
{
    PreparationPhase,
    ActionPhase,
    GameOver
}