using UnityEngine;

// L'Enum ModifierLogicType a été supprimé !

public abstract class ModifierCardData : CardData
{
    [Header("Données de Modification")]
    [Tooltip("Vrai si le joueur doit cliquer sur une tour alliée pour appliquer l'effet.")]
    public bool requiresTowerTarget;

    // La méthode magique du polymorphisme
    public abstract IModifierEffect CreateModifierEffect();
}