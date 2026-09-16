using UnityEngine;

[CreateAssetMenu(fileName = "New_Gem_NoyauBasique", menuName = "TowerDefense/Cards/Gems/Normal/Noyau Basique")]
public class GemCard_NoyauBasique : GemCardData
{
    private void OnEnable()
    {
        gemCategory = GemCategory.Noyau; 
    }

    public override IGemEffect CreateGemEffect()
    {
        return new Gem_NoyauBasique();
    }
}