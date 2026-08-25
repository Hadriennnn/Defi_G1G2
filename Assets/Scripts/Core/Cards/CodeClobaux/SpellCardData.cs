using UnityEngine;

public abstract class SpellCardData : CardData
{
    [Header("Données de Sort (Pouvoir)")]
    public bool requiresTargeting;

    public abstract ISpellEffect CreateSpellEffect();

    public override bool TryPlayCard(Vector2Int targetCoords, PlacementManager context)
    {
        Node clickedNode = context.GridData.Grid[targetCoords.x, targetCoords.y];  

        if (requiresTargeting && !clickedNode.isWalkable && !context.TryGetTowerAt(targetCoords, out _)) 
        {
            // Gestion ciblée à affiner plus tard
        }

        if (!EnergyManager.Instance.TrySpendEnergy(cost)) return false;

        Vector3 targetPos = context.GetWorldPosition(targetCoords);
        ISpellEffect spellLogic = CreateSpellEffect();
        spellLogic.Execute(targetPos);

        return true;
    }
}