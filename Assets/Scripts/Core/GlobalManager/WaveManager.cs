using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveConfig
{
    public string waveName = "Vague 1";
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float spawnDelay = 1.5f; 
}

/*
 * RÉSUMÉ : Gère l'apparition des ennemis et le retour à la phase de préparation.
 */
public class WaveManager : MonoBehaviour
{
    [Header("Configuration des Vagues")]
    public List<WaveConfig> waves;
    private int currentWaveIndex = 0;

    // Variables pour traquer l'état de la vague
    private int activeEnemyCount = 0;
    private bool isSpawning = false;

    void OnEnable()
    {
        GameEvents.OnGameStateChanged += GererChangementPhase;
        GameEvents.OnEnemySpawned += GererApparitionEnnemi;
        GameEvents.OnEnemyRemoved += GererMortEnnemi;
    }

    void OnDisable()
    {
        GameEvents.OnGameStateChanged -= GererChangementPhase;
        GameEvents.OnEnemySpawned -= GererApparitionEnnemi;
        GameEvents.OnEnemyRemoved -= GererMortEnnemi;
    }

    private void GererApparitionEnnemi() => activeEnemyCount++;
    
    private void GererMortEnnemi()
    {
        activeEnemyCount--;
        VerifierFinDeVague();
    }

    private void GererChangementPhase(GameState state)
    {
        if (state == GameState.ActionPhase)
        {
            StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        yield return new WaitForSeconds(0.1f);

        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("[WaveManager] Toutes les vagues ont été terminées !");
            isSpawning = false;
            VerifierFinDeVague();
            yield break;
        }

        WaveConfig vagueActuelle = waves[currentWaveIndex];
        Debug.Log($"[WaveManager] Début de la : {vagueActuelle.waveName}");

        for (int i = 0; i < vagueActuelle.enemyCount; i++)
        {
            if (vagueActuelle.enemyPrefab != null)
            {
                Instantiate(vagueActuelle.enemyPrefab);
            }
            yield return new WaitForSeconds(vagueActuelle.spawnDelay);
        }

        isSpawning = false;
        currentWaveIndex++;
        
        // On vérifie au cas où les tours auraient déjà tué tous les monstres avant la fin de l'invocation
        VerifierFinDeVague();
    }

    private void VerifierFinDeVague()
    {
        // Si l'invocation est terminée ET qu'il n'y a plus d'ennemis ET qu'on est bien en phase d'action
        if (!isSpawning && activeEnemyCount <= 0 && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.ActionPhase)
        {
            Debug.Log("[WaveManager] Vague nettoyée ! Retour à la phase de préparation.");
            
            // On ordonne au GameManager de repasser en préparation
            GameManager.Instance.RelancerPhasePreparation();
        }
    }
}