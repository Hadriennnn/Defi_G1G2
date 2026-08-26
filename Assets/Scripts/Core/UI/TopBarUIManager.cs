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

    void Start()
    {
        // Les abonnements aux Singletons (Instance) doivent se faire dans le Start !
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged += UpdateEnergyText;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveText != null) waveText.text = "Vague : 1";
    }

    private void UpdateWaveText(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = $"Vague : {waveNumber}";
        }
    }

    private void UpdateEnergyText(int currentEnergy, int maxEnergy)
    {
        if (energyText != null)
        {
            energyText.text = $"Énergie : {currentEnergy} / {maxEnergy}";
        }
    }

    private void UpdateBaseHealthText(int currentHealth, int maxHealth)
    {
        if (baseHealthText != null)
        {
            baseHealthText.text = $"Base : {currentHealth} / {maxHealth} PV";
        }
    }

    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}