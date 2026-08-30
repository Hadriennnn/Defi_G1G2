using UnityEngine;

public enum GemCategory 
{ 
    Noyau,          // Requis pour qu'une tour puisse tirer 
    Amplificateur   // Optionnel, booste les stats 
}

public abstract class GemCardData : CardData
{
    [Header("Données de Gemme")]
    public GemCategory gemCategory = GemCategory.Noyau;

    public abstract IGemEffect CreateGemEffect(); 

    public override bool TryPlayCard(Vector2Int targetCoords, PlacementManager context)
    {
        if (!context.TryGetTowerAt(targetCoords, out GameObject targetTower))
        {
            Debug.LogWarning("Il n'y a pas de tour sur cette case pour insérer une gemme");
            return false;
        }

        SocketManager sockets = targetTower.GetComponent<SocketManager>();

        if (sockets == null || !sockets.CanInsertGem())
        {
            Debug.LogWarning("Cette tour n'a plus de place");
            return false;
        }

        if (!EnergyManager.Instance.TrySpendEnergy(cost)) return false;
        
        IGemEffect gemLogic = CreateGemEffect();
        sockets.InsertGem(gemLogic);

        return true;
    }
}