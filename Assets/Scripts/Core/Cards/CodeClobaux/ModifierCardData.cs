using UnityEngine;

public enum ModifierLogicType
{
    DeplacerTour,
    RecyclerTour,
    PlanificationTactique // Pioche 2, défausse 1
}

[CreateAssetMenu(fileName = "NewModifierCard", menuName = "TowerDefense/Cards/Modifier")]
public class ModifierCardData : CardData
{
    [Header("Données de Modification")]
    [Tooltip("Définit l'action système que cette carte va exécuter.")]
    public ModifierLogicType modifierType;
    
    [Tooltip("Vrai si le joueur doit cliquer sur une tour alliée pour appliquer l'effet.")]
    public bool requiresTowerTarget;
}