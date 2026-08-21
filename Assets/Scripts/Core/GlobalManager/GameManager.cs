using UnityEngine;

/*
 * RÉSUMÉ : Singleton central qui gère l'état global de la partie (PreparationPhase vs ActionPhase).
 */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.PreparationPhase;

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
        ChangerEtat(GameState.PreparationPhase);
    }

    public void LancerPhaseAction()
    {
        ChangerEtat(GameState.ActionPhase);
    }

    public void RelancerPhasePreparation()
    {
        ChangerEtat(GameState.PreparationPhase);
    }

    private void ChangerEtat(GameState nouvelEtat)
    {
        CurrentState = nouvelEtat;
        Debug.Log($"[GameManager] Changement de phase : {CurrentState}");
        GameEvents.TriggerGameStateChanged(CurrentState);
    }
}