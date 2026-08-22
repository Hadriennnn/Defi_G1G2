using UnityEngine;

// Le contrat pour tous les Sorts (Spells)
public interface ISpellEffect
{
    // Un sort s'exécute généralement à un endroit précis (ou au centre de la carte si global)
    void Execute(Vector3 targetPosition);
}

// Le contrat pour tous les Modificateurs (Modifiers)
public interface IModifierEffect
{
    // Un modificateur s'applique souvent sur une tour spécifique (ou null si c'est global)
    void Execute(GameObject targetTower);
}