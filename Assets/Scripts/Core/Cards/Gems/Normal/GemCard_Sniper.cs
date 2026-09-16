using UnityEngine;

[CreateAssetMenu(fileName = "New_Gem_Sniper", menuName = "TowerDefense/Cards/Gems/Normal/Sniper")]
public class GemCard_Sniper : GemCardData
{
    public override IGemEffect CreateGemEffect()
    {
        return new Gem_Sniper();
    }
}