using UnityEngine;
using System.Threading.Tasks; 

public abstract class SpellCardData : CardData
{
    public bool requiresTargeting;

    public abstract ISpellEffect CreateSpellEffect();

    public override Task<bool> TryPlayCardAsync(Vector2Int targetCoords, PlacementManager context)
    {
        Node clickedNode = context.GridData.Grid[targetCoords.x, targetCoords.y];  

        if (requiresTargeting && !clickedNode.isWalkable && !context.TryGetTowerAt(targetCoords, out _)) 
        {
            // Logique ciblée future
        }


        Vector3 targetPos = context.GetWorldPosition(targetCoords);
        ISpellEffect spellLogic = CreateSpellEffect();
        spellLogic.Execute(targetPos);

        return Task.FromResult(true);
    }
}