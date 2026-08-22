using System;  
using System.Collections.Generic;  
using UnityEngine;

/* 
 * RÉSUMÉ : Bus d'événements global (statique) permettant aux différents systèmes de communiquer entre eux.
 */

public static class GameEvents  
{  
    // L'événement classique quand la grille change
    public static event Action OnGridChanged;

    // Pour dire au visuel de dessiner le nouveau chemin
    public static event Action<List<Node>> OnPathUpdated;

    // Pour dire au visuel de dessiner ou retirer une barricade
    public static event Action<Vector3Int, bool> OnObstacleToggled;
    public static event Action<GameState> OnGameStateChanged;
    public static void TriggerGameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
    
    // Événement quand le joueur clique sur une carte dans sa main (clique gauche)
    public static event Action<CardData> OnCardSelected;
    public static void TriggerCardSelected(CardData card) => OnCardSelected?.Invoke(card);
    
    // Événement quand le joueur annule sa sélection (clic droit)
    public static event Action OnCardDeselected;
    public static void TriggerCardDeselected() => OnCardDeselected?.Invoke();
    

    public static void TriggerGridChanged() => OnGridChanged?.Invoke();  

    public static void TriggerPathUpdated(List<Node> path) => OnPathUpdated?.Invoke(path);  
    
    public static void TriggerObstacleToggled(Vector3Int position, bool isPlaced) => OnObstacleToggled?.Invoke(position, isPlaced);  
    
    // Pour les Waves
    public static event Action OnEnemySpawned;
    public static event Action OnEnemyRemoved;
    public static void TriggerEnemySpawned() => OnEnemySpawned?.Invoke();
    public static void TriggerEnemyRemoved() => OnEnemyRemoved?.Invoke();
}

// L'énumération globale pour les phases du jeu
public enum GameState
{
    PreparationPhase,
    ActionPhase
}