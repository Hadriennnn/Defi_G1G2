using UnityEngine;

// 1. LA LOGIQUE DU SORT
public class Spell_OndeDeStase : ISpellEffect
{
    public void Execute(Vector3 targetPosition)
    {
        Debug.Log($"[Sort] Onde de Stase explosée à la position {targetPosition} ! Les ennemis sont gelés.");
        // Plus tard : Physics.OverlapSphere ici pour geler les ennemis
    }
}

// 2. LA CARTE QUI GÉNÈRE LE SORT
[CreateAssetMenu(fileName = "New_Spell_OndeStase", menuName = "TowerDefense/Cards/Spells/Onde de Stase")]
public class SpellCard_OndeDeStase : SpellCardData
{
    public override ISpellEffect CreateSpellEffect()
    {
        return new Spell_OndeDeStase();
    }
}