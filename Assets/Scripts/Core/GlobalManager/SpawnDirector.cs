using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnTier
{
    public string tierName = "Tier 1 (Facile)";
    
    [Tooltip("Le Score de Menace minimum que le portail doit atteindre pour piocher ici.")]
    public float minThreatScore = 0f;
    
    [Tooltip("La liste des profils possibles pour ce niveau de difficulté.")]
    public List<SpawnerProfile> profiles;
}

public class SpawnDirector : MonoBehaviour
{
    public static SpawnDirector Instance { get; private set; }

    [Header("Calcul de la Menace (Threat Score)")]
    [Tooltip("Points de menace pour chaque nouveau Chunk")]
    public float weightPerChunkId = 2f;
    [Tooltip("Points de menace ajouté pour chaque case de distance avec la base")]
    public float weightPerCellDistance = 1f;

    [Header("Banque de Profils (Tiers)")]
    public List<SpawnTier> tiers;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Fonction appelée par le ChunkManager quand un portail n'a pas de profil assigné
    public SpawnerProfile GetRandomProfile(int chunkId, float distance)
    {
        // Calcul du Score de Menace
        float threatScore = (chunkId * weightPerChunkId) + (distance * weightPerCellDistance);
        
        SpawnTier selectedTier = null;
        float highestValidScore = -1f;

        // On cherche le Tier le plus difficile auquel on a droit
        foreach (var tier in tiers)
        {
            if (threatScore >= tier.minThreatScore && tier.minThreatScore > highestValidScore)
            {
                if (tier.profiles != null && tier.profiles.Count > 0)
                {
                    highestValidScore = tier.minThreatScore;
                    selectedTier = tier;
                }
            }
        }

        // On pioche au hasard dans ce Tier
        if (selectedTier != null && selectedTier.profiles.Count > 0)
        {
            int randomIndex = Random.Range(0, selectedTier.profiles.Count);
            SpawnerProfile chosenProfile = selectedTier.profiles[randomIndex];
            
            Debug.Log($"[SpawnDirector] Score Menace: {threatScore:F1} ➔ Pioche dans '{selectedTier.tierName}' ➔ Profil assigné : {chosenProfile.name}");
            return chosenProfile;
        }

        Debug.LogWarning("[SpawnDirector] Aucun profil valide trouvé");
        return null;
    }
}