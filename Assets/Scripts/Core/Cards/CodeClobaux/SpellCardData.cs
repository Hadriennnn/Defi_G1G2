using UnityEngine;

// L'Enum SpellLogicType a été supprimé !

public abstract class SpellCardData : CardData
{
    [Header("Données de Sort (Pouvoir)")]
    [Tooltip("Vrai si le sort doit être ciblé sur une zone (AoE), Faux s'il est global.")]
    public bool requiresTargeting;

    // La méthode magique du polymorphisme
    public abstract ISpellEffect CreateSpellEffect();
}