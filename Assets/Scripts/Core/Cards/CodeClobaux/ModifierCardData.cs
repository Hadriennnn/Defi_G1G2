using UnityEngine;
using System.Threading.Tasks; 

public abstract class ModifierCardData : CardData
{
    public bool requiresTowerTarget;

    public abstract IModifierEffect CreateModifierEffect();

    public override Task<bool> TryPlayCardAsync(Vector2Int targetCoords, PlacementManager context)
    {
        GameObject targetTower = null;

        if (requiresTowerTarget)
        {
            if (!context.TryGetTowerAt(targetCoords, out targetTower)) return Task.FromResult(false);
        }

        if (!EnergyManager.Instance.TrySpendEnergy(cost)) return Task.FromResult(false);

        IModifierEffect modLogic = CreateModifierEffect();
        modLogic.Execute(targetTower);

        if (targetTower == null || !targetTower.activeInHierarchy)
        {
            context.RemoveTower(targetCoords);
            Node clickedNode = context.GridData.Grid[targetCoords.x, targetCoords.y];
            clickedNode.isWalkable = true;
            
            GameEvents.TriggerObstacleToggled(new Vector3Int(targetCoords.x, targetCoords.y, 0), false);
            GameEvents.TriggerGridChanged();
        }

        return Task.FromResult(true);
    }
}