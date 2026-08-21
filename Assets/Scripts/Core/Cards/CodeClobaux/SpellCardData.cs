using UnityEngine;

public enum SpellLogicType
{
    OndeDeStase,
    Surcharge,
    Fumigene,
    MurDeFortune 
}

[CreateAssetMenu(fileName = "NewSpellCard", menuName = "TowerDefense/Cards/Spell")]
public class SpellCardData : CardData
{
    [Header("Données de Sort (Pouvoir)")]
    [Tooltip("Définit quel script de sort sera exécuté au déclenchement.")]
    public SpellLogicType spellType;
    
    [Tooltip("Vrai si le sort doit être ciblé sur une zone (AoE), Faux s'il est global.")]
    public bool requiresTargeting;
}