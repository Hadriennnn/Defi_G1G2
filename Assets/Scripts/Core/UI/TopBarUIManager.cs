using UnityEngine;
using TMPro; 

public class TopBarUIManager : MonoBehaviour
{
    [Header("Références Textes (TextMeshPro)")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI baseHealthText;
    
    // NOUVEAU : Référence pour le texte de la phase actuelle
    public TextMeshProUGUI phaseText; 
    
    [Header("Écran de Game Over")]
    public GameObject gameOverPanel; 

    void OnEnable()
    {
        GameEvents.OnWaveNumberChanged += UpdateWaveText; 
        GameEvents.OnBaseHealthChanged += UpdateBaseHealthText;
        GameEvents.OnGameOver += ShowGameOverScreen;
        
        // NOUVEAU : On écoute les changements de phase
        GameEvents.OnGameStateChanged += UpdatePhaseText; 
    }

    void OnDisable()
    {
        GameEvents.OnWaveNumberChanged -= UpdateWaveText;
        GameEvents.OnBaseHealthChanged -= UpdateBaseHealthText;
        GameEvents.OnGameOver -= ShowGameOverScreen;
        
        GameEvents.OnGameStateChanged -= UpdatePhaseText; 

        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged -= UpdateEnergyText;
        }
    }

    public void Init()
    {
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged += UpdateEnergyText;
        }

        if (BaseManager.Instance != null)
        {
            UpdateBaseHealthText(BaseManager.Instance.CurrentHealth, BaseManager.Instance.MaxHealth);
        }

        if (GameManager.Instance != null)
        {
            UpdatePhaseText(GameManager.Instance.CurrentState);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveText != null) waveText.text = "Vague : 1";
    }

    private void UpdatePhaseText(GameState state)
    {
        if (phaseText != null)
        {
            switch (state)
            {
                case GameState.PreparationPhase:
                    phaseText.text = "Phase de Préparation";
                    break;
                case GameState.ActionPhase:
                    phaseText.text = "Phase d'Action";
                    break;
                case GameState.GameOver:
                    phaseText.text = "Fin de Partie";
                    break;
            }
        }
    }

    private void UpdateWaveText(int waveNumber)
    {
        if (waveText != null) waveText.text = $"Vague : {waveNumber}";
    }

    private void UpdateEnergyText(int currentEnergy, int maxEnergy)
    {
        if (energyText != null) energyText.text = $"Énergie : {currentEnergy} / {maxEnergy}";
    }

    private void UpdateBaseHealthText(int currentHealth, int maxHealth)
    {
        if (baseHealthText != null) baseHealthText.text = $"Base : {currentHealth} / {maxHealth} PV";
    }

    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}