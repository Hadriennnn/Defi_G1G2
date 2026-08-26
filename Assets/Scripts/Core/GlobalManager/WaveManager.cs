using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Progression des Vagues")]
    [Tooltip("La vague actuelle. Doit toujours commencer à 1.")]
    public int currentWaveIndex = 1; 

    [Header("Progression de la Carte")]
    [Tooltip("Toutes les X vagues, un nouveau bloc sort de terre (0 = jamais)")]
    public int chunkUnlockInterval = 2;
    
    // Le chunk 1 est la base de départ, donc le prochain à débloquer est le 2
    private int nextChunkIdToUnlock = 2; 

    private int activeEnemyCount = 0;
    private int activeSpawnersCount = 0; 

    void OnEnable()
    {
        GameEvents.OnGameStateChanged += GererChangementPhase;
        GameEvents.OnEnemySpawned += (enemy) => activeEnemyCount++; 
        GameEvents.OnEnemyRemoved += (enemy) => GererMortEnnemi(); 
        GameEvents.OnSpawnerFinished += GererFinSpawner;
    }

    void OnDisable()
    {
        GameEvents.OnGameStateChanged -= GererChangementPhase;
        GameEvents.OnEnemySpawned -= (enemy) => activeEnemyCount++;
        GameEvents.OnEnemyRemoved -= (enemy) => GererMortEnnemi(); 
        GameEvents.OnSpawnerFinished -= GererFinSpawner;
    }

    private void GererApparitionEnnemi() => activeEnemyCount++;
    
    private void GererMortEnnemi() 
    { 
        activeEnemyCount--; 
        VerifierFinDeVague(); 
    }
    
    private void GererFinSpawner() 
    { 
        activeSpawnersCount--; 
        VerifierFinDeVague(); 
    }

    private void GererChangementPhase(GameState state)
    {
        if (state == GameState.PreparationPhase)
        {
            // Ca c'est pour l'UI. Et pas pour les ennemis 
            GameEvents.TriggerWaveNumberChanged(currentWaveIndex);

            if (chunkUnlockInterval > 0 && currentWaveIndex > 1)
            {
                if ((currentWaveIndex - 1) % chunkUnlockInterval == 0)
                {
                    Debug.Log($"[WaveManager] Vague {currentWaveIndex} atteinte : Déblocage du Chunk {nextChunkIdToUnlock} !");
                    GameEvents.TriggerChunkUnlocked(nextChunkIdToUnlock);
                    nextChunkIdToUnlock++;
                }
            }
        }
        else if (state == GameState.ActionPhase)
        {
            Debug.Log($"[WaveManager] --- DÉBUT DE LA VAGUE {currentWaveIndex} ---");
            
            activeSpawnersCount = FindObjectsOfType<EnemySpawner>().Length;

            if (activeSpawnersCount == 0)
            {
                Debug.LogWarning("[WaveManager] Aucun portail actif trouvé ");
                VerifierFinDeVague();
                return;
            }

            // ICI, on réveille les spawners pour qu'ils crachent les monstres !
            GameEvents.TriggerWaveStarted(currentWaveIndex);
        }
    }

    private void VerifierFinDeVague()
    {
        if (activeSpawnersCount <= 0 && activeEnemyCount <= 0 && GameManager.Instance.CurrentState == GameState.ActionPhase)
        {
            Debug.Log($"[WaveManager] Vague {currentWaveIndex} terminée avec succès !");
            currentWaveIndex++;
            GameManager.Instance.RelancerPhasePreparation();
        }
    }
}