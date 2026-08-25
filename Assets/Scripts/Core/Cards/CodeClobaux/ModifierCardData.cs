using UnityEngine;

public abstract class ModifierCardData : CardData
{
    [Header("Données de Modification")]
    public bool requiresTowerTarget;

    public abstract IModifierEffect CreateModifierEffect();

    public override bool TryPlayCard(Vector2Int targetCoords, PlacementManager context)
    {
        GameObject targetTower = null;

        if (requiresTowerTarget)
        {
            if (!context.TryGetTowerAt(targetCoords, out targetTower))
            {
                Debug.LogWarning("Ce modificateur nécessite de cibler une tour !");
                return false;
            }
        }

        if (!EnergyManager.Instance.TrySpendEnergy(cost)) return false;

        IModifierEffect modLogic = CreateModifierEffect();
        modLogic.Execute(targetTower);

        // Si la carte a détruit la tour (ex: Carte Recyclage)
        if (targetTower == null || !targetTower.activeInHierarchy)
        {
            context.RemoveTower(targetCoords);
            Node clickedNode = context.GridData.Grid[targetCoords.x, targetCoords.y];
            clickedNode.isWalkable = true;
            
            GameEvents.TriggerObstacleToggled(new Vector3Int(targetCoords.x, targetCoords.y, 0), false);
            GameEvents.TriggerGridChanged();
        }

        return true;
    }
}