using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnRule
{
    public string ruleName = "Troupes basiques";
    public GameObject enemyPrefab;
    
    [Header("Formule Algébrique")]
    public float baseQuantity = 3f;
    public float incrementPerWave = 1.5f;
    public float spawnDelay = 1f; 
}

[System.Serializable]
public class MilestoneRule
{
    public string ruleName = "Boss";
    public GameObject enemyPrefab;
    public int waveInterval = 5;
    public int quantity = 1;
    public float spawnDelay = 2f;
}

[CreateAssetMenu(fileName = "NewSpawnerProfile", menuName = "TowerDefense/Spawner Profile")]
public class SpawnerProfile : ScriptableObject
{
    [Header("Modificateurs Géographiques (Multiplicateur)")]
    [Tooltip("Bonus appliqué pour chaque case d'éloignement (ex: 0.05 = +5% d'ennemis par case)")]
    public float distanceMultiplierPerCell = 0.05f;

    [Header("Plafonnage de la difficulté (Le Plateau)")]
    public bool enableIncrementCap = true;
    [Tooltip("Nombre maximum d'incrémentations de base (ex: 2)")]
    public float baseMaxIncrements = 2f;
    [Tooltip("Bonus de plafond par ordre d'apparition du Chunk (ex: 1.0 = +1 incrémentation max par nouveau chunk)")]
    public float capBonusPerChunk = 0.25f;
    [Tooltip("Bonus de plafond par case de distance (ex: 0.1 = +1 incrémentation max toutes les 10 cases)")]
    public float capBonusPerCellDistance = 0.1f;

    [Header("Les Troupes Régulières")]
    public List<SpawnRule> regularSpawns;

    [Header("Les Jalons (Boss / Events)")]
    public List<MilestoneRule> milestoneSpawns;
}