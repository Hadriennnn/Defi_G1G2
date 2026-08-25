using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Profil du Portail")]
    public SpawnerProfile profile;

    private Vector2Int myGridCoords;
    private int myChunkId;
    
    private float myDistanceMultiplier = 1f;
    private int myMaxIncrements = 999; // Infini par défaut

    void OnEnable() { GameEvents.OnWaveStarted += LancerGeneration; }
    void OnDisable() { GameEvents.OnWaveStarted -= LancerGeneration; }

    // NOUVEAU : On reçoit aussi le ChunkId
    public void Init(Vector2Int coords, int chunkId)
    {
        myGridCoords = coords;
        myChunkId = chunkId;
        CalculerDifficulte();
    }

    private void CalculerDifficulte()
    {
        Vector2Int baseCoords = ChunkManager.Instance.baseNoyauCoords;
        float distanceCellules = Vector2Int.Distance(myGridCoords, baseCoords);
        
        if (profile != null)
        {
            // 1. Calcul du multiplicateur de quantité
            myDistanceMultiplier = 1f + (distanceCellules * profile.distanceMultiplierPerCell);
            
            // 2. Calcul du Plafond (Cap)
            if (profile.enableIncrementCap)
            {
                // Le Chunk 1 est la base. Donc le 1er chunk généré est le Chunk 2.
                // On fait (myChunkId - 1) pour que le 1er chunk généré ait un ordre de 1.
                int chunkOrder = Mathf.Max(0, myChunkId - 1); 
                
                float rawCap = profile.baseMaxIncrements + (chunkOrder * profile.capBonusPerChunk) + (distanceCellules * profile.capBonusPerCellDistance);
                myMaxIncrements = Mathf.RoundToInt(rawCap);
            }

            Debug.Log($"[Spawner] Portail en {myGridCoords} (Chunk {myChunkId}). Dist: {distanceCellules:F1}. Multiplicateur: x{myDistanceMultiplier:F2}. Plafond d'incréments: {myMaxIncrements}");
        }
    }

    private void LancerGeneration(int waveNumber)
    {
        if (profile == null)
        {
            GameEvents.TriggerSpawnerFinished();
            return;
        }
        StartCoroutine(SpawnRoutine(waveNumber));
    }

    private IEnumerator SpawnRoutine(int waveNumber)
    {
        yield return new WaitForSeconds(0.1f);

        if (!LevelManager.Instance.WaypointsParSpawn.ContainsKey(myGridCoords))
        {
            GameEvents.TriggerSpawnerFinished();
            yield break;
        }

        List<Vector3> monChemin = LevelManager.Instance.WaypointsParSpawn[myGridCoords];

        // NOUVEAU : Calcul du nombre d'incrémentations (avec le plafond !)
        int incrementsToApply = waveNumber - 1;
        if (profile.enableIncrementCap)
        {
            incrementsToApply = Mathf.Min(incrementsToApply, myMaxIncrements);
        }

        foreach (SpawnRule rule in profile.regularSpawns)
        {
            if (rule.enemyPrefab == null) continue;

            // On utilise 'incrementsToApply' au lieu de '(waveNumber - 1)'
            float rawQuantity = (rule.baseQuantity + (rule.incrementPerWave * incrementsToApply)) * myDistanceMultiplier;
            int finalQuantity = Mathf.FloorToInt(rawQuantity);

            for (int i = 0; i < finalQuantity; i++)
            {
                SpawnEnemy(rule.enemyPrefab, monChemin);
                yield return new WaitForSeconds(rule.spawnDelay);
            }
        }

        foreach (MilestoneRule milestone in profile.milestoneSpawns)
        {
            if (milestone.enemyPrefab == null) continue;

            if (waveNumber % milestone.waveInterval == 0)
            {
                for (int i = 0; i < milestone.quantity; i++)
                {
                    SpawnEnemy(milestone.enemyPrefab, monChemin);
                    yield return new WaitForSeconds(milestone.spawnDelay);
                }
            }
        }

        GameEvents.TriggerSpawnerFinished();
    }

    private void SpawnEnemy(GameObject prefab, List<Vector3> chemin)
    {
        GameObject ennemi = Instantiate(prefab, transform.position, Quaternion.identity);
        EnemyMovement mouvement = ennemi.GetComponent<EnemyMovement>();
        if (mouvement != null) mouvement.AssignerChemin(chemin);
    }
}