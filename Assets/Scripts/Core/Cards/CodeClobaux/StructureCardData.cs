using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "NewStructureCard", menuName = "TowerDefense/Cards/Structure")]
public class StructureCardData : CardData
{
    [Header("Données de Structure")]
    public GameObject chassisPrefab; 
    
    [Tooltip("Si vrai, la carte reste sélectionnée après utilisation pour en poser plusieurs de suite (ex: Murs).")]
    public bool isMultiCast = false;

    public override async Task<bool> TryPlayCardAsync(Vector2Int targetCoords, PlacementManager context)
    {
        Node clickedNode = context.GridData.Grid[targetCoords.x, targetCoords.y];  

        if (!clickedNode.isWalkable) 
        {
            Debug.LogWarning("Case déjà occupée");
            return false;
        }

        Vector3 worldPos = context.GetWorldPosition(targetCoords);
        float hauteurPhysique = clickedNode.elevation * context.GridVisualizer.hauteurNiveau;
        Vector3 cellCenter = new Vector3(worldPos.x, worldPos.y + hauteurPhysique + 0.5f, worldPos.z);

        if (context.IsEnemyOnCell(cellCenter))
        {
            Debug.LogWarning("Placement impossible : Un ennemi se trouve sur cette case");
            return false;
        }

        if (!EnergyManager.Instance.HasEnoughEnergy(cost))
        {
            Debug.LogWarning("Placement impossible : Pas assez d'énergie !");
            return false; 
        }

        clickedNode.isWalkable = false;  
        
        if (!await context.Pathfinder.CheckAllPathsAsync())  
        {  
            clickedNode.isWalkable = true;  
            Debug.LogWarning("Placement impossible : Cela bloque le chemin des ennemis !");
            return false;  
        }

        EnergyManager.Instance.TrySpendEnergy(cost);

        Vector3 spawnPos = new Vector3(worldPos.x, worldPos.y + hauteurPhysique, worldPos.z);
        GameObject nouvelleTour = Instantiate(chassisPrefab, spawnPos, chassisPrefab.transform.rotation);
        
        context.AddTower(targetCoords, nouvelleTour);

        AutoTilingBarricade autoTiling = nouvelleTour.GetComponent<AutoTilingBarricade>();
        if (autoTiling != null)
        {
            autoTiling.Init(targetCoords);
        }

        GameEvents.TriggerObstacleToggled(new Vector3Int(targetCoords.x, targetCoords.y, 0), true);  
        GameEvents.TriggerGridChanged(); 
        
        return true;
    }
}