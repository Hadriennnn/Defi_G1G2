using UnityEngine;

/* 
 * RÉSUMÉ : Singleton central qui gère l'état global de la partie (PreparationPhase vs ActionPhase) et en informe les autres scripts.
 * 
 * MÉTHODES PRINCIPALES :
 * - Awake() / Init() : Initialise l'instance unique et démarre le jeu en phase de préparation.
 * - LancerPhaseAction() / RelancerPhasePreparation() : Méthodes appelées par l'UI pour changer l'état du jeu.
 * - ChangerEtat() : Modifie la variable CurrentState et déclenche l'événement global via GameEvents.
 */

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // L'état actuel du jeu (par défaut en préparation)
    public GameState CurrentState { get; private set; } = GameState.PreparationPhase;

    [Header("Test")]
    public GameObject enemyPrefabTest;
    

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

    public void Init()
    {
        // On s'assure que tout le monde sait qu'on démarre en préparation
        ChangerEtat(GameState.PreparationPhase);
        Debug.Log($"[GameManager] Starting game : {CurrentState}");
    }

    // Fonction publique à lier au bouton UI "Lancer la Vague"
    public void LancerPhaseAction()
    {
        ChangerEtat(GameState.ActionPhase);
        
        // TEST 
        Invoke(nameof(SpawnTestEnemy),0.5f);
        Debug.Log($"[GameManager] Starting game : {CurrentState}");
    }

    // Fonction publique à lier à un bouton "Réinitialiser" (optionnel pour tes tests)
    public void RelancerPhasePreparation()
    {
        ChangerEtat(GameState.PreparationPhase);
    }

    private void ChangerEtat(GameState nouvelEtat)
    {
        CurrentState = nouvelEtat;
        Debug.Log($"[GameManager] Changement de phase : {CurrentState}");
        
        // On crie à tous les scripts que la phase a changé !
        GameEvents.TriggerGameStateChanged(CurrentState);
    }
    
    // TEST
    private void SpawnTestEnemy()
    {
        if (enemyPrefabTest != null)
        {
            Instantiate(enemyPrefabTest);
        }
    }
}