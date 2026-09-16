using UnityEngine;
using TMPro; 

public class TopBarUIManager : MonoBehaviour
{
    [Header("Références Textes (TextMeshPro)")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI baseHealthText;
    
    [Header("Écran de Game Over")]
    public GameObject gameOverPanel; 

    void OnEnable()
    {
        GameEvents.OnWaveNumberChanged += UpdateWaveText; 
        GameEvents.OnBaseHealthChanged += UpdateBaseHealthText;
        GameEvents.OnGameOver += ShowGameOverScreen;
    }

    void OnDisable()
    {
        GameEvents.OnWaveNumberChanged -= UpdateWaveText;
        GameEvents.OnBaseHealthChanged -= UpdateBaseHealthText;
        GameEvents.OnGameOver -= ShowGameOverScreen;

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

        // NOUVEAU : On "Pull" la donnée manuellement pour être sûr de l'afficher au démarrage !
        if (BaseManager.Instance != null)
        {
            UpdateBaseHealthText(BaseManager.Instance.CurrentHealth, BaseManager.Instance.MaxHealth);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveText != null) waveText.text = "Vague : 1";
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