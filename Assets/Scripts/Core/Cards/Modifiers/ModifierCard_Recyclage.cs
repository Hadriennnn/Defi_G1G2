using UnityEngine;

// 1. LA LOGIQUE DU MODIFICATEUR
public class Mod_Recyclage : IModifierEffect
{
    public void Execute(GameObject targetTower)
    {
        Debug.Log($"[Modificateur] Tour {targetTower.name} recyclée ! +3 Énergie.");
        EnergyManager.Instance.GainInstantEnergy(3);
        Object.Destroy(targetTower);
    }
}

// 2. LA CARTE QUI GÉNÈRE LE MODIFICATEUR
[CreateAssetMenu(fileName = "New_Mod_Recyclage", menuName = "TowerDefense/Cards/Modifiers/Recyclage")]
public class ModifierCard_Recyclage : ModifierCardData
{
    public override IModifierEffect CreateModifierEffect()
    {
        return new Mod_Recyclage();
    }
}