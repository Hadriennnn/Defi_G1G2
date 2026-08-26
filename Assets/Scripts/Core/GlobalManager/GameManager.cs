using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.PreparationPhase;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        GameEvents.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnGameOver -= HandleGameOver;
    }

    public void Init()
    {
        ChangerEtat(GameState.PreparationPhase);
    }

    public void LancerPhaseAction()
    {
        if (CurrentState == GameState.GameOver) return;
        ChangerEtat(GameState.ActionPhase);
    }

    public void RelancerPhasePreparation()
    {
        if (CurrentState == GameState.GameOver) return;
        ChangerEtat(GameState.PreparationPhase);
    }

    private void HandleGameOver()
    {
        ChangerEtat(GameState.GameOver);
        // écran de défaite ici
    }

    private void ChangerEtat(GameState nouvelEtat)
    {
        CurrentState = nouvelEtat;
        Debug.Log($"[GameManager] Changement de phase : {CurrentState}");
        GameEvents.TriggerGameStateChanged(CurrentState);
    }
}