using UnityEngine;

public class Mod_Recyclage : IModifierEffect
{
    public void Execute(GameObject targetTower)
    {
        Debug.Log($"[Modificateur] Tour {targetTower.name} recyclée ! +3 Énergie.");
        EnergyManager.Instance.GainInstantEnergy(3);
        
        targetTower.SetActive(false);
        
        Object.Destroy(targetTower);
    }
}

[CreateAssetMenu(fileName = "New_Mod_Recyclage", menuName = "TowerDefense/Cards/Modifiers/Recyclage")]
public class ModifierCard_Recyclage : ModifierCardData
{
    public override IModifierEffect CreateModifierEffect()
    {
        return new Mod_Recyclage();
    }
}