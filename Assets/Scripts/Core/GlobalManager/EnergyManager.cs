using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    [Header("Paramètres d'Énergie")]
    [Tooltip("L'énergie donnée au joueur au début de chaque phase de préparation.")]
    public int baseEnergyPerWave = 8; 
    
    private int bonusEnergyPerWave = 0; 
    
    private int currentEnergy;

    public event Action<int, int> OnEnergyChanged; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.PreparationPhase)
        {
            RefillEnergy();
        }
    }

    private void RefillEnergy()
    {
        currentEnergy = baseEnergyPerWave + bonusEnergyPerWave;
        NotifyUI();
        Debug.Log($"[EnergyManager] Nouvelle vague : Énergie restaurée à {currentEnergy}");
    }

    public bool HasEnoughEnergy(int amount)
    {
        return currentEnergy >= amount;
    }

    public bool TrySpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            NotifyUI();
            return true;
        }
        
        Debug.LogWarning("[EnergyManager] Pas assez d'énergie pour jouer cette carte !");
        return false;
    }

    public void GainInstantEnergy(int amount)
    {
        currentEnergy += amount;
        NotifyUI();
    }

    public void AddPermanentBonusEnergy(int amount)
    {
        bonusEnergyPerWave += amount;
    }
    
    public void RemovePermanentBonusEnergy(int amount)
    {
        bonusEnergyPerWave = Mathf.Max(0, bonusEnergyPerWave - amount);
    }

    private void NotifyUI()
    {
        int maxEnergy = baseEnergyPerWave + bonusEnergyPerWave;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
}